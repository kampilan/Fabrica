namespace Fabrica.Watch.Mongo
{

    public interface IWatchMongoModule
    {

        string WatchEventStoreUri { get; set; }
        string WatchDomainName { get; set; }

        int WatchPollingDurationSecs { get; set; }


    }


}
