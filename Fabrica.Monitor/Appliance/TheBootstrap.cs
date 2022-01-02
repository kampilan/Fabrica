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



    public class TheBootstrap: KestrelBootstrap<TheModule, MonitorOptions,InitService>
    {


        protected override void ConfigureApp(ConfigurationBuilder builder)
        {

            // *****************************************************************
            builder
                .AddYamlFile("configuration.yml", true)
                .AddJsonFile("environment.json", true)
                .AddJsonFile("mission.json", true);

        }
        

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
