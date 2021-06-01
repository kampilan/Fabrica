﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.Api.Support.Identity.Key;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Utilities.Drawing;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Fabrica.Proxy.Appliance
{


    public class TheBootstrap: KestrelBootstrap<TheModule,ProxyOptions>
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
                .AddYamlFile("local.yml", true);

        }
#endif


        private bool RunProxy { get; set; }

        protected override void ConfigureServices(IServiceCollection services)
        {


            using var logger = this.EnterMethod();


            if( Options.RunningOnEC2 && !string.IsNullOrWhiteSpace(Options.DataProtectionParameterName) && !string.IsNullOrWhiteSpace(Options.ApplicationDiscriminator) )
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
            logger.Debug("Attempting to Add default CORS policy");
            services.AddCors();


            // *****************************************************************
            logger.Inspect(nameof(Options.ConfigureForAuthentication), Options.ConfigureForAuthentication);
            if( Options.ConfigureForAuthentication )
            {

                // *****************************************************************
                logger.Debug("Attempting to configure Authentication");
                var authBuilder = services.AddAuthentication(op =>
                    {
                        op.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
                        op.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
                        op.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    });



                // *****************************************************************
                logger.Inspect( nameof(Options.IncludeUserAuthentication), Options.IncludeUserAuthentication );
                if (Options.IncludeUserAuthentication)
                {
                    authBuilder
                        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
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
                logger.Inspect(nameof(Options.IncludeKeyAuthentication), Options.IncludeKeyAuthentication);
                if (Options.IncludeKeyAuthentication)
                {
                    schemas.Add("ApiKey");
                    authBuilder.AddApiKey();
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


            }


        }


        protected override void ConfigureWebApp(IApplicationBuilder builder)
        {

            builder.UseRequestLogging();

            builder.UseForwardedHeaders();

            builder.UseRouting();

            builder.UseCors(b =>
            {
                b.AllowAnyMethod();
                b.AllowAnyHeader();
                b.SetIsOriginAllowed(_ => true);
                b.AllowCredentials();
            });

            if ( Options.ConfigureForAuthentication )
            {
                builder.UseAuthentication();
                builder.UseAuthorization();
            }

            builder.UseMiddleware<ProxyTokenBuilderMiddleware>();

            builder.UseEndpoints(ep =>
            {


                if( Options.ConfigureForAuthentication && Options.IncludeUserAuthentication )
                {

                    ep.Map( Options.LoginRoute, async c =>
                    {

                        var authProps = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc   = DateTime.UtcNow.AddHours(24),
                            RedirectUri  = Options.PostLoginRedirectUri
                        };

                        await c.ChallengeAsync( OpenIdConnectDefaults.AuthenticationScheme, authProps );

                    });


                    ep.Map( Options.LogoutRoute, async c =>
                    {

                        await c.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                        await c.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    });

                }


                if( RunProxy )
                    ep.MapReverseProxy();


            });

        }


        private void ConfigureOidcAuth(OpenIdConnectOptions options)
        {

            options.MetadataAddress = Options.MetadataAddress;
            options.ClientId        = Options.ClientId;
            options.ClientSecret    = Options.ClientSecret;

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce      = true;



            options.Scope.Clear();

            var scopes = new List<string>( Options.Scopes.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim()) );
            if( scopes.Count == 0 )
                scopes.Add("openid");

            foreach( var scope in scopes )
                options.Scope.Add(scope);



            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens                    = true;

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

                    context.ProtocolMessage.PostLogoutRedirectUri = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{Options.PostLogoutRedirectUri}";
                    if( !string.IsNullOrWhiteSpace(Options.ProviderSignOutUri) )
                    {
                        context.Response.Redirect( Options.ProviderSignOutUri );
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



    }


}
