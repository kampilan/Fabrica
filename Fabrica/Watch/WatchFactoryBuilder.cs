/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using JetBrains.Annotations;

namespace Fabrica.Watch
{

    public class WatchFactoryBuilder
    {


        [NotNull]
        public static WatchFactoryBuilder Create()
        {
            return new WatchFactoryBuilder();
        }

        public int InitialPoolSize { get; set; } = 1000;


        public CompositeSink Sinks { get; } = new CompositeSink();

        public ISwitchSource Source { get; set; } = new SwitchSource();

        public IList<object> Infrastructure { get; } = new List<object>();


        private bool Quiet { get; set; } = false;

        public WatchFactoryBuilder WithQuiet( bool quiet )
        {
            Quiet = quiet;
            return this;
        }



        private bool WillUseBatching { get; set; }
        private int BatchSize { get; set; } = 10;
        private TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
        [NotNull]
        public WatchFactoryBuilder UseBatching( int batchSize=10, TimeSpan pollingInterval=default(TimeSpan) ) 
        {

            WillUseBatching = true;

            BatchSize = batchSize;

            if ( pollingInterval != default(TimeSpan))
                PollingInterval = pollingInterval;

            return this;

        }


        private bool WillUseTimerBatching { get; set; }

        [NotNull]
        public WatchFactoryBuilder UseTimerBatching(int batchSize = 10, TimeSpan pollingInterval = default )
        {

            WillUseTimerBatching = true;

            BatchSize = batchSize;

            if (pollingInterval != default(TimeSpan))
                PollingInterval = pollingInterval;

            return this;

        }



        private IEventSink _buildSink()
        {

            if( WillUseBatching )
            {
                var sinks = new BatchEventSink(Sinks)
                {
                    BatchSize = BatchSize,
                    PollingInterval = PollingInterval
                };
                return sinks;
            }
            
            if( WillUseTimerBatching )
            {
                var sinks = new TimerBatchEventSink(Sinks);
                return sinks;
            }

            return Sinks;

        }




        public void Build<TFactory>() where TFactory : class, IWatchFactory, new()
        {

            var sink = _buildSink();

            var factory = new TFactory();

            foreach( var i in Infrastructure)
                factory.AddInfrastructure(i);

            factory.Configure( Source, sink, Quiet );

            WatchFactoryLocator.SetFactory( factory );

        }

        public void Build<TFactory>( [NotNull] Func<TFactory> builder ) where TFactory : class, IWatchFactory
        {

            var sink = _buildSink();

            var factory = builder();

            foreach (var i in Infrastructure)
                factory.AddInfrastructure(i);

            factory.Configure(Source, sink, Quiet );

            WatchFactoryLocator.SetFactory( factory );

        }

        public void Build()
        {

            var sink = _buildSink();

            var factory = new WatchFactory(InitialPoolSize);

            foreach (var i in Infrastructure)
                factory.AddInfrastructure(i);

            factory.Configure( Source, sink, Quiet );

            WatchFactoryLocator.SetFactory(factory);

        }



        public IWatchFactory BuildNoSet()
        {

            var sink = _buildSink();

            var factory = new WatchFactory(InitialPoolSize);

            foreach (var i in Infrastructure)
                factory.AddInfrastructure(i);

            factory.Configure(Source, sink, Quiet);

            return factory;

        }





    }


}
