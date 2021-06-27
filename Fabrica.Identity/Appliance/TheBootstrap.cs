using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Identity.Contexts;
using Fabrica.Identity.Models;
using Fabrica.Identity.Services;
using Fabrica.Utilities.Drawing;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;

namespace Fabrica.Identity.Appliance
{


    public class TheBootstrap: KestrelBootstrap<TheModule,ApplianceOptions>
    {


#if DEBUG

        protected override void ConfigureWatch()
        {

            var maker = WatchFactoryBuilder.Create();
            maker.UseRealtime();
            maker.UseLocalSwitchSource()
                .WhenMatched("Fabrica.Diagnostics.Http", "", Level.Debug, Color.Thistle)
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




        protected override void ConfigureServices( IServiceCollection services )
        {

            services.AddControllersWithViews();

            services.AddDbContext<ApplicationDbContext>(o =>
            {

                var cnstr = Configuration.GetValue<string>("OriginDbConnectionStr");
                o.UseMySql(cnstr, ServerVersion.AutoDetect(cnstr));
                o.UseOpenIddict();

            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType   = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType     = OpenIddictConstants.Claims.Role;
            });


            services.AddOpenIddict()
                .AddCore(o =>
                {
                    o.UseEntityFrameworkCore()
                        .UseDbContext<ApplicationDbContext>();
                })
                .AddServer(o =>
                {

                    o.SetTokenEndpointUris("/connect/token");

                    o.AllowClientCredentialsFlow();
                    o.AllowPasswordFlow();
                    o.AllowRefreshTokenFlow();

                    o.AddDevelopmentEncryptionCertificate();
                    o.AddDevelopmentSigningCertificate();

                    o.UseAspNetCore()
                        .EnableTokenEndpointPassthrough();

                })
                .AddValidation(o =>
                {
                    o.UseLocalServer();
                    o.UseAspNetCore();
                });


            services.Configure<ForwardedHeadersOptions>(options =>
            {

                options.RequireHeaderSymmetry = false;
                options.ForwardedHeaders      = ForwardedHeaders.All;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

            });

            services.AddHostedService<Worker>();



        }

        protected override void ConfigureWebApp( IApplicationBuilder builder )
        {


            builder.UsePipelineMonitor();
            builder.UseDebugMode();
            builder.UseRequestLogging();


            builder.UseForwardedHeaders();


            builder.UseDeveloperExceptionPage();

            builder.UseRouting();

            builder.UseAuthentication();
            builder.UseAuthorization();


            builder.UseEndpoints(o =>
            {
                o.MapControllers();
            });

        }

    }


}
