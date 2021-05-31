namespace Fabrica.Watch.Mongo
{


    public class WatchMongoOptions: IWatchMongoModule
    {

        public bool RealtimeLogging { get; set; } = false;

        public string WatchEventStoreUri { get; set; } = "";
        public string WatchDomainName { get; set; } = "";
        public int WatchPollingDurationSecs { get; set; } = 15;


    }


}
