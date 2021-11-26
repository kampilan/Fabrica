using System;
using System.Collections.Concurrent;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Appliance;
using Fabrica.Watch.Mongo;

namespace Fabrica.Watch.Api.Components
{


    public class WatchFactoryCache: CorrelatedObject, IDisposable
    {

        public WatchFactoryCache(ICorrelation correlation, WatchOptions options): base(correlation)
        {

            Factories = new ConcurrentDictionary<string, IWatchFactory>();

            Options     = options;

        }

        public void Dispose()
        {
            foreach (var factory in Factories.Values)
                factory.Stop();

        }


        private WatchOptions Options { get; }

        private ConcurrentDictionary<string,IWatchFactory> Factories { get; }


        public IWatchFactory GetFactory( string domain )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(domain), domain);



            // *****************************************************************
            logger.Debug("Attempting to get cached factory");

            if( Factories.TryGetValue(domain, out var factory))
                return factory;



            // *****************************************************************
            logger.Debug("Attempting to create new factory");

            var maker = new WatchFactoryBuilder();
            maker.UseMongo( Options.WatchEventStoreUri, domain );

            factory = maker.BuildNoSet();
            factory.Start();

            Factories.TryAdd(domain, factory);


            // *****************************************************************
            return factory;


        }


    }

}
