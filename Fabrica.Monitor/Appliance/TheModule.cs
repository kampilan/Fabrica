using System.Threading.Tasks;
using Autofac;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Monitor.Appliance;

public class TheModule: BootstrapModule
{


    public bool ConfigureHealthCheck => !string.IsNullOrWhiteSpace(HealthcheckRoute);
    public string HealthcheckRoute { get; set; } = "/healthcheck";

    public bool ConfigureCatchAll => !string.IsNullOrWhiteSpace(CatchAllRoute);
    public string CatchAllRoute { get; set; } = "/{**catch-all}";

    public override void ConfigureContainer(ContainerBuilder builder)
    {

        builder.AddCorrelation();

    }

    public override void ConfigureWebApp(IApplicationBuilder builder)
    {


        if (ConfigureCatchAll)
            builder.UseRequestLogging();

        builder.UseRouting();

        builder.UseEndpoints(ep =>
        {

            if (ConfigureHealthCheck)
            {

                ep.Map(HealthcheckRoute, _ =>
                {
                    var result = new StatusCodeResult(200);
                    return Task.FromResult(result);
                });

            }

            if (ConfigureCatchAll)
            {

                ep.Map(CatchAllRoute, _ =>
                {
                    var result = new StatusCodeResult(200);
                    return Task.FromResult(result);
                });

            }


        });

    }



}