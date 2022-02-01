using System;
using System.Collections.Concurrent;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Appliance;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Mongo.Sink;
using Fabrica.Watch.Sink;

namespace Fabrica.Watch.Api.Components
{


    public class WatchSinkCache: CorrelatedObject, IDisposable
    {

        public WatchSinkCache(ICorrelation correlation): base(correlation)
        {

            Sinks = new ConcurrentDictionary<string, IEventSink>();

        }

        public void Dispose()
        {

            foreach( var sink in Sinks.Values )
                sink.Stop();

        }


        public string WatchEventStoreUri { get; set; } = "";


        private ConcurrentDictionary<string,IEventSink> Sinks { get; }


        public IEventSink GetSink( string domain )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(domain), domain);


            // *****************************************************************
            logger.Debug("Attempting to get cached sink");

            if( Sinks.TryGetValue(domain, out var sink) )
                return sink;



            // *****************************************************************
            logger.Debug("Attempting to create new factory");

            var mgSink = new MongoEventSink();
            mgSink.WithServerUri(WatchEventStoreUri).WithDomainName(domain);
            mgSink.Start();    


            Sinks.TryAdd(domain, mgSink);


            // *****************************************************************
            return mgSink;


        }


    }

}
