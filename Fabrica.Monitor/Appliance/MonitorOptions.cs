using Fabrica.Api.Support.One;

namespace Fabrica.Monitor.Appliance
{

    public class MonitorOptions: ApplianceOptions
    {

        public bool ConfigureHealthCheck => !string.IsNullOrWhiteSpace(HealthcheckRoute);
        public string HealthcheckRoute { get; set; } = "/healthcheck";

    }


}
