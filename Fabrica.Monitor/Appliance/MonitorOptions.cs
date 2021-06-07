using Fabrica.Api.Support.One;

namespace Fabrica.Monitor.Appliance
{

    public class MonitorOptions: ApplianceOptions
    {

        public bool ConfigureHealthCheck => !string.IsNullOrWhiteSpace(HealthcheckRoute);
        public string HealthcheckRoute { get; set; } = "/healthcheck";

        public bool ConfigureCatchAll => !string.IsNullOrWhiteSpace(CatchAllRoute);
        public string CatchAllRoute { get; set; } = "/{**catch-all}";

    }


}
