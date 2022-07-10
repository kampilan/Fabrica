using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws.Secrets;
using Fabrica.Watch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using Yarp.ReverseProxy.Transforms;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace Fabrica.Proxy.Appliance
{


    public class TheBootstrap : BaseBootstrap
    {

        public bool RunningOnEC2 { get; set; } = true;
        public string RegionName { get; set; } = "";
        public string Profile { get; set; } = "";

        public string AwsSecretsId { get; set; } = "";


        public string ApplicationDiscriminator { get; set; } = "";
        public string DataProtectionParameterName { get; set; } = "";


        public string OidcClientId { get; set; } = "";
        public string OidcClientSecret { get; set; } = "";


        public bool UseSession { get; set; } = false;
        public string RedisConnectionStr { get; set; } = "";

        public bool UseCors { get; set; } = false;


        public bool ConfigureForAuthentication => !string.IsNullOrWhiteSpace(MetadataAddress);

        public bool IncludeUserAuthentication { get; set; } = true;
        public bool IncludeApiAuthentication { get; set; } = true;


        public string MetadataAddress { get; set; } = "";

        public string Audience { get; set; }

        public string Scopes { get; set; } = "";


        public string ProviderSignOutUri { get; set; } = "";


        public string LoginRoute { get; set; } = "/login";
        public string LogoutRoute { get; set; } = "/logout";
        public string PostLoginRedirectUri { get; set; } = "/";
        public string PostLogoutRedirectUri { get; set; } = "/";


        public int TokenTimeToLiveSecs { get; set; } = 30;


        public override void ConfigureWatch()
        {

#if DEBUG
            ConfigureDebugWatch(s =>
            {
                s
                    .WhenMatched("Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle)
                    .WhenMatched("Microsoft", "", Level.Warning, Color.BurlyWood)
                    .WhenNotMatched(Level.Warning, Color.Azure);
            });
#else
            base.ConfigureWatch();
#endif

        }


        public override async Task OnConfigured()
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Inspect(nameof(AwsSecretsId), AwsSecretsId);
            if (!string.IsNullOrWhiteSpace(AwsSecretsId))
            {
                logger.Debug("Attempting to populate Module with AWS Secrets");
                var secrets = new SecretsModel();
                await AwsSecretsHelper.PopulateWithSecrets(secrets, AwsSecretsId, RunningOnEC2, Profile, RegionName);
                OidcClientId = secrets.OidcClientId;
                OidcClientSecret = secrets.OidcClientSecret;
            }

        }


        private bool RunProxy { get; set; }

        public override void ConfigureServices(IServiceCollection services)
        {


            using var logger = EnterMethod();


            if (RunningOnEC2 && !string.IsNullOrWhiteSpace(DataProtectionParameterName) && !string.IsNullOrWhiteSpace(ApplicationDiscriminator))
            {
                services.AddDataProtection(o => o.ApplicationDiscriminator = ApplicationDiscriminator)
                    .PersistKeysToAWSSystemsManager(DataProtectionParameterName);
            }



            // *****************************************************************
            logger.Debug("Attempting to configure Reverse Proxy");
            var proxyCfg = Configuration.GetSection("Proxy");

            logger.Inspect("ProxyConfigExists", proxyCfg.Exists());

            if( proxyCfg.Exists() )
            {

                services.AddReverseProxy()
                    .LoadFromConfig(proxyCfg)
                    .AddTransforms(bc =>
                    {
                        bc.AddRequestHeaderRemove("Cookies");
                        bc.AddRequestHeaderRemove("Authorization");
                    });

                RunProxy = true;

            }



            // *****************************************************************
            logger.Debug("Attempting to configure Forwarding");
            services.Configure<ForwardedHeadersOptions>(options =>
            {

                options.RequireHeaderSymmetry = false;
                options.ForwardedHeaders = ForwardedHeaders.All;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

            });


            // *****************************************************************
            if (UseCors)
            {
                logger.Debug("Attempting to Add default CORS policy");
                services.AddCors();

            }


            // *****************************************************************
            logger.Inspect(nameof(ConfigureForAuthentication), ConfigureForAuthentication);
            if (ConfigureForAuthentication)
            {

                // *****************************************************************
                logger.Debug("Attempting to configure Authentication");
                var authBuilder = services.AddAuthentication(op =>
                {
                    op.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    op.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
                });



                // *****************************************************************
                logger.Inspect(nameof(IncludeUserAuthentication), IncludeUserAuthentication);
                if (IncludeUserAuthentication)
                {
                    authBuilder
                        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, ConfigureCookieAuth)
                        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, ConfigureOidcAuth);
                }

                var schemas = new List<string>();

                // *****************************************************************
                logger.Inspect(nameof(IncludeApiAuthentication), IncludeApiAuthentication);
                if (IncludeApiAuthentication)
                {
                    schemas.Add(JwtBearerDefaults.AuthenticationScheme);
                    authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, ConfigureJwtBearer);
                }


                // *****************************************************************
                logger.Debug("Attempting to configure Authorization");
                services.AddAuthorization(op =>
                {

                    var builder = new AuthorizationPolicyBuilder();
                    var defPolicy = builder.RequireAuthenticatedUser().Build();

                    op.DefaultPolicy = defPolicy;
                    op.FallbackPolicy = defPolicy;

                    op.AddPolicy("Authentication:None", p =>
                    {
                        p.RequireAssertion(_ => true);
                    });

                    op.AddPolicy("Authentication:Api", p =>
                    {
                        p.RequireAssertion(_ => true);
                        //p.RequireAuthenticatedUser();
                        p.AddAuthenticationSchemes(schemas.ToArray());
                    });

                    op.AddPolicy("Authentication:User", defPolicy);

                });

                if (UseSession)
                {

                    services.AddStackExchangeRedisCache(o =>
                    {
                        o.Configuration = RedisConnectionStr;
                        o.InstanceName = "session_";
                    });

                    services.AddSession(o =>
                    {
                        o.IdleTimeout = TimeSpan.FromMinutes(30);
                        o.Cookie.Name = "Fabrica.Proxy";
                        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        o.Cookie.HttpOnly = true;
                        o.Cookie.SameSite = SameSiteMode.Strict;
                    });

                }


            }


        }


        public override void ConfigureContainer(ContainerBuilder builder)
        {


            using var logger = EnterMethod();

            builder.Register(_ =>
                {

                    byte[] key = null;
                    if (!string.IsNullOrWhiteSpace(TokenSigningKey))
                        key = Convert.FromBase64String(TokenSigningKey);

                    var comp = new ProxyTokenJwtEncoder
                    {
                        TokenSigningKey = key,
                        TokenTimeToLive = TimeSpan.FromSeconds(TokenTimeToLiveSecs)
                    };

                    return comp;

                })
                .As<IProxyTokenEncoder>()
                .SingleInstance();


            builder.Register(_ =>
                {
                    var comp = new ClaimTokenPayloadBuilder();
                    return comp;
                })
                .As<IProxyTokenPayloadBuilder>()
                .SingleInstance()
                .AutoActivate();


        }


        public override void ConfigureWebApp( WebApplication builder )
        {

            builder.UseRequestLogging();


            if( IncludeUserAuthentication )
                builder.UseSecurityHeaders();


            builder.UseForwardedHeaders();

            builder.UseRouting();


            if (UseCors)
            {

                builder.UseCors(b =>
                {
                    b.AllowAnyMethod();
                    b.AllowAnyHeader();
                    b.SetIsOriginAllowed(_ => true);
                    b.AllowCredentials();
                });

            }


            if( ConfigureForAuthentication )
            {


                builder.Use(async (ctx, next) =>
                {

                    await next();

                    if( ctx.Request.Path == "/signin-oidc" && ctx.Response.StatusCode == 302 )
                    {
                        var loc = ctx.Response.Headers["location"];
                        ctx.Response.StatusCode = 200;

                        var html = $@"
                        <html><head>
                            <meta http-equiv='refresh' content='0;url={loc}' />
                        </head></html>";

                        await ctx.Response.WriteAsync(html);

                    }

                });
                
                
                builder.UseAuthentication();
                builder.UseAuthorization();


            }

            builder.UseMiddleware<ProxyTokenBuilderMiddleware>();


            if (UseSession)
                builder.UseSession();


            builder.UseEndpoints(ep =>
            {


                if (ConfigureForAuthentication && IncludeUserAuthentication)
                {

                    ep.Map(LoginRoute, async c =>
                    {

                        var returnTo = PostLoginRedirectUri;
                        if (c.Request.Query.TryGetValue("ReturnTo", out var value))
                            returnTo = value.FirstOrDefault("");

                        var authProps = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(24),
                            RedirectUri = returnTo
                        };

                        await c.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);

                    }).AllowAnonymous();


                    ep.MapGet("/me", async c =>
                    {

                        var me = c.User.Identity?.IsAuthenticated ?? false ? await CreateUserInfo(c.User) : UserInfo.Anonymous;
                        var options = new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                            PropertyNamingPolicy = null
                        };

                        await c.Response.WriteAsJsonAsync(me, options);

                    }).AllowAnonymous();


                    ep.Map(LogoutRoute, async c =>
                   {

                       if (c.User.Identity?.IsAuthenticated ?? false)
                       {

                           c.Session.Clear();
                           await c.Session.CommitAsync();

                           foreach (var ck in c.Request.Cookies.Keys)
                               c.Response.Cookies.Delete(ck);

                           var returnTo = PostLoginRedirectUri;
                           if (c.Request.Query.TryGetValue("ReturnTo", out var value))
                               returnTo = value.FirstOrDefault("");

                           var authProps = new AuthenticationProperties
                           {
                               RedirectUri = returnTo
                           };

                           await c.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
                           await c.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                       }

                   }).AllowAnonymous();

                }


                if (RunProxy)
                    ep.MapReverseProxy();


            });

        }


        private void ConfigureCookieAuth(CookieAuthenticationOptions options)
        {
            options.Cookie.Name     = "__Fabrica.Proxy";
            options.Cookie.SameSite = SameSiteMode.Strict;
        }

        private void ConfigureOidcAuth(OpenIdConnectOptions options)
        {

            options.SignInScheme = "Cookies";

            options.MetadataAddress = MetadataAddress;
            options.ClientId = OidcClientId;
            options.ClientSecret = OidcClientSecret;

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;

            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = true;


            options.Scope.Clear();

            var scopes = new List<string>(Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));
            if (scopes.Count == 0)
                scopes.Add("openid");

            foreach (var scope in scopes)
                options.Scope.Add(scope);


            options.Events = new OpenIdConnectEvents
            {


                // called if user clicks Cancel during login
                OnAccessDenied = context =>
                {

                    context.Response.Redirect(PostLogoutRedirectUri);
                    context.HandleResponse();

                    return Task.CompletedTask;

                },

                // handle the logout redirection 
                OnRedirectToIdentityProviderForSignOut = context =>
                {

                    string returnTo;
                    if (context.HttpContext.Request.Query.TryGetValue("ReturnTo", out var value))
                        returnTo = value;
                    else
                        returnTo = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{PostLogoutRedirectUri}";

                    context.ProtocolMessage.PostLogoutRedirectUri = returnTo;

                    if (!string.IsNullOrWhiteSpace(ProviderSignOutUri))
                    {
                        var act = string.Format(ProviderSignOutUri, returnTo);
                        context.Response.Redirect(act);
                        context.HandleResponse();
                    }

                    return Task.CompletedTask;

                }


            };


        }

        private void ConfigureJwtBearer(JwtBearerOptions options)
        {

            options.MetadataAddress = MetadataAddress;
            options.Audience = Audience;

        }


        private Task<UserInfo> CreateUserInfo(ClaimsPrincipal claimsPrincipal)
        {

            var userInfo = new UserInfo
            {
                IsAuthenticated = true
            };

            if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
            {
                userInfo.NameClaimType = claimsIdentity.NameClaimType;
                userInfo.RoleClaimType = claimsIdentity.RoleClaimType;
            }
            else
            {
                userInfo.NameClaimType = ClaimTypes.Name;
                userInfo.RoleClaimType = ClaimTypes.Role;
            }


            if (!claimsPrincipal.Claims.Any())
                return Task.FromResult(userInfo);


            var nameClaims = claimsPrincipal.FindAll(userInfo.NameClaimType).ToList();

            var claims = nameClaims.Select(claim => new ClaimValue(userInfo.NameClaimType, claim.Value)).ToList();

            claims.AddRange(claimsPrincipal.Claims.Except(nameClaims).Select(claim => new ClaimValue(claim.Type, claim.Value)));

            userInfo.Claims = claims;

            return Task.FromResult(userInfo);

        }



    }


}


[JsonObject(MemberSerialization.OptIn)]
public class SecretsModel
{

    [JsonProperty("oidc-client-id")]
    public string OidcClientId { get; set; } = "";

    [JsonProperty("oidc-client-secret")]
    public string OidcClientSecret { get; set; } = "";

}


public class ClaimValue
{

    public ClaimValue(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; set; }

    public string Value { get; set; }

}

public class UserInfo
{

    public static readonly UserInfo Anonymous = new();

    public bool IsAuthenticated { get; set; }

    public string NameClaimType { get; set; }

    public string RoleClaimType { get; set; }

    public ICollection<ClaimValue> Claims { get; set; }

}

