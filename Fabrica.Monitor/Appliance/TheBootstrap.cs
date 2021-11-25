using System.Drawing;
using System.Threading.Tasks;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Configuration.Yaml;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Monitor.Appliance
{



    public class TheBootstrap: KestrelBootstrap<TheModule, MonitorOptions>
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
                .AddYamlFile("configuration.yml", true);

        }
#endif


        protected override void ConfigureServices(IServiceCollection services)
        {
        }

        protected override void ConfigureWebApp(IApplicationBuilder builder)
        {

            if( Options.ConfigureCatchAll )
                builder.UseRequestLogging();

            builder.UseRouting();

            builder.UseEndpoints(ep =>
            {

                if( Options.ConfigureHealthCheck )
                {

                    ep.Map(Options.HealthcheckRoute, _ =>
                    {
                        var result = new StatusCodeResult(200);
                        return Task.FromResult(result);
                    });

                }

                if( Options.ConfigureCatchAll )
                {

                    ep.Map( Options.CatchAllRoute, _ =>
                    {
                        var result = new StatusCodeResult(200);
                        return Task.FromResult(result);
                    });

                }


            });

        }

    }
}
