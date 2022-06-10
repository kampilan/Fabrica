using System.Threading.Tasks;
using Fabrica.Api.Support.Middleware;
using Fabrica.Api.Support.One;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Monitor.Appliance;

public class TheBootstrap: BaseBootstrap
{

    public bool ConfigureHealthCheck => !string.IsNullOrWhiteSpace(HealthcheckRoute);
    public string HealthcheckRoute { get; set; } = "/healthcheck";

    public bool ConfigureCatchAll => !string.IsNullOrWhiteSpace(CatchAllRoute);
    public string CatchAllRoute { get; set; } = "/{**catch-all}";

    public override void ConfigureWebApp( WebApplication app )
    {


        if (ConfigureCatchAll)
            app.UseRequestLogging();

        app.UseRouting();

        app.UseEndpoints(ep =>
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