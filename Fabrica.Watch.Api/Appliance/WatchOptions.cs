using Fabrica.Api.Support.One;

namespace Fabrica.Watch.Api.Appliance
{


    public class WatchOptions : ApplianceOptions
    {

        public string WatchEventStoreUri { get; set; } = "";

        public string DefaultWatchDomain { get; set; } = "";

    }


}
