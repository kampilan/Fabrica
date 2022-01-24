using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Aws.Secrets;
using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
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

namespace Fabrica.Proxy.Appliance;

public class TheBootstrap: KestrelBootstrap<TheModule,ProxyOptions,InitService>
{


#if DEBUG

    protected override void ConfigureWatch()
    {

        var maker = WatchFactoryBuilder.Create();
        maker.UseRealtime();
        maker.UseLocalSwitchSource()
            .WhenMatched( "Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle )
            .WhenNotMatched(Level.Debug, Color.Azure);

        maker.Build();

    }

    protected override void ConfigureApp(ConfigurationBuilder builder)
    {

        // *****************************************************************
        builder
            .AddYamlFile("configuration.yml", true)
            .AddYamlFile("e:/locals/proxy/local.yml", true);

    }

#endif


    protected override void ConfigurOptions()
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.Inspect(nameof(Options.AwsSecretsId), Options.AwsSecretsId);    
        if ( !string.IsNullOrWhiteSpace(Options.AwsSecretsId) )
        {
            logger.Debug("Attempting to populate Options with AWS Secrets");
            AsyncPump.Run(async () => await AwsSecretsHelper.PopulateWithSecrets(Options, Options.AwsSecretsId, Options.RunningOnEc2, Options.ProfileName, Options.RegionName));
        }


    }


    private bool RunProxy { get; set; }

    protected override void ConfigureServices(IServiceCollection services)
    {


        using var logger = this.EnterMethod();


        if( Options.RunningOnEc2 && !string.IsNullOrWhiteSpace(Options.DataProtectionParameterName) && !string.IsNullOrWhiteSpace(Options.ApplicationDiscriminator) )
        {
            services.AddDataProtection(o => o.ApplicationDiscriminator = Options.ApplicationDiscriminator)
                .PersistKeysToAWSSystemsManager( Options.DataProtectionParameterName );
        }



        // *****************************************************************
        logger.Debug("Attempting to configure Reverse Proxy");
        var proxyCfg = Configuration.GetSection("Proxy");

        logger.Inspect("ProxyConfigExists", proxyCfg.Exists());

        if( proxyCfg.Exists() )
        {
            services.AddReverseProxy()
                .LoadFromConfig(proxyCfg);
            RunProxy = true;
        }



        // *****************************************************************
        logger.Debug("Attempting to configure Forwarding");
        services.Configure<ForwardedHeadersOptions>(options =>
        {

            options.RequireHeaderSymmetry = false;
            options.ForwardedHeaders      = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

        });



        // *****************************************************************
        logger.Inspect(nameof(Options.ConfigureForAuthentication), Options.ConfigureForAuthentication);
        if( Options.ConfigureForAuthentication )
        {

            // *****************************************************************
            logger.Debug("Attempting to configure Authentication");
            var authBuilder = services.AddAuthentication(op =>
            {
                op.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
                op.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                op.DefaultSignOutScheme   = OpenIdConnectDefaults.AuthenticationScheme;
            });



            // *****************************************************************
            logger.Inspect( nameof(Options.IncludeUserAuthentication), Options.IncludeUserAuthentication );
            if (Options.IncludeUserAuthentication)
            {
                authBuilder
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,ConfigureCookieAuth)
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, ConfigureOidcAuth);
            }

            var schemas = new List<string>();

            // *****************************************************************
            logger.Inspect( nameof(Options.IncludeApiAuthentication), Options.IncludeApiAuthentication );
            if (Options.IncludeApiAuthentication)
            {
                schemas.Add(JwtBearerDefaults.AuthenticationScheme);
                authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, ConfigureJwtBearer);
            }


            // *****************************************************************
            logger.Debug("Attempting to configure Authorization");
            services.AddAuthorization(op =>
            {

                var builder   = new AuthorizationPolicyBuilder();
                var defPolicy = builder.RequireAuthenticatedUser().Build();

                op.DefaultPolicy  = defPolicy;
                op.FallbackPolicy = defPolicy;

                op.AddPolicy("Authentication:None", p =>
                {
                    p.RequireAssertion(_ => true);
                });

                op.AddPolicy("Authentication:Api", p =>
                {
                    p.RequireAssertion(_ => true);
                    //p.RequireAuthenticatedUser();
                    p.AddAuthenticationSchemes( schemas.ToArray() );
                });

                op.AddPolicy("Authentication:User", defPolicy);

            });

            if( Options.UseSession )
            {

                services.AddStackExchangeRedisCache(o=>
                {
                    o.Configuration = Options.RedisConnectionStr;
                    o.InstanceName = "session_";
                });

                services.AddSession(o =>
                {
                    o.IdleTimeout         = TimeSpan.FromMinutes(30);
                    o.Cookie.Name         = "Fabrica.Proxy";
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    o.Cookie.HttpOnly     = true;
                    o.Cookie.SameSite     = SameSiteMode.Strict;
                });

            }


        }


    }


    protected override void ConfigureWebApp(IApplicationBuilder builder)
    {

        builder.UseRequestLogging();

        builder.UseForwardedHeaders();

        builder.UseRouting();


        if ( Options.ConfigureForAuthentication )
        {
            builder.UseAuthentication();
            builder.UseAuthorization();
        }

        builder.UseMiddleware<ProxyTokenBuilderMiddleware>();


        if( Options.UseSession )
            builder.UseSession();


        builder.UseEndpoints(ep =>
        {


            if( Options.ConfigureForAuthentication && Options.IncludeUserAuthentication )
            {

                ep.Map( Options.LoginRoute, async c =>
                {

                    var returnTo = Options.PostLoginRedirectUri;
                    if (c.Request.Query.TryGetValue("ReturnTo", out var value))
                        returnTo = value.FirstOrDefault("");

                    var authProps = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc   = DateTime.UtcNow.AddHours(24),
                        RedirectUri  = returnTo
                    };

                    await c.ChallengeAsync( OpenIdConnectDefaults.AuthenticationScheme, authProps );

                }).AllowAnonymous();


                ep.MapGet("/me", async c =>
                {

                    var me = c.User.Identity?.IsAuthenticated??false ? await CreateUserInfo(c.User) : UserInfo.Anonymous;
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        PropertyNamingPolicy = null
                    };

                    await c.Response.WriteAsJsonAsync(me, options);

                }).AllowAnonymous();


                ep.Map( Options.LogoutRoute, async c =>
                {

                    if( c.User.Identity?.IsAuthenticated??false )
                    {

                        c.Session.Clear();
                        await c.Session.CommitAsync();

                        foreach (var ck in c.Request.Cookies.Keys)
                            c.Response.Cookies.Delete(ck);

                        var returnTo = Options.PostLoginRedirectUri;
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


            if( RunProxy )
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


        options.MetadataAddress = Options.MetadataAddress;
        options.ClientId        = Options.OidcClientId;
        options.ClientSecret    = Options.OidcClientSecret;

        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce      = true;

        options.MapInboundClaims = false;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;


        options.Scope.Clear();

        var scopes = new List<string>( Options.Scopes.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim()) );
        if( scopes.Count == 0 )
            scopes.Add("openid");

        foreach( var scope in scopes )
            options.Scope.Add(scope);


        options.Events = new OpenIdConnectEvents
        {
                

            // called if user clicks Cancel during login
            OnAccessDenied = context =>
            {

                context.Response.Redirect(Options.PostLogoutRedirectUri);
                context.HandleResponse();

                return Task.CompletedTask;

            },

            // handle the logout redirection 
            OnRedirectToIdentityProviderForSignOut = context =>
            {

                string returnTo;
                if( context.HttpContext.Request.Query.TryGetValue("ReturnTo", out var value) )
                    returnTo = value;
                else
                    returnTo = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{Options.PostLogoutRedirectUri}";

                context.ProtocolMessage.PostLogoutRedirectUri = returnTo;

                if( !string.IsNullOrWhiteSpace(Options.ProviderSignOutUri) )
                {
                    var act = string.Format(Options.ProviderSignOutUri, returnTo);
                    context.Response.Redirect( act );
                    context.HandleResponse();
                }

                return Task.CompletedTask;

            }


        };


    }

    private void ConfigureJwtBearer(JwtBearerOptions options)
    {

        options.MetadataAddress = Options.MetadataAddress;
        options.Audience        = Options.Audience;

    }


    private Task<UserInfo> CreateUserInfo( ClaimsPrincipal claimsPrincipal )
    {

        var userInfo = new UserInfo
        {
            IsAuthenticated = true
        };

        if( claimsPrincipal.Identity is ClaimsIdentity claimsIdentity )
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
    public ClaimValue()
    {
    }

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
    public static readonly UserInfo Anonymous = new UserInfo();

    public bool IsAuthenticated { get; set; }

    public string NameClaimType { get; set; }

    public string RoleClaimType { get; set; }

    public ICollection<ClaimValue> Claims { get; set; }
}

