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
using System.Collections.Concurrent;
using System.Linq;
using Fabrica.Utilities.Pooling;
using Fabrica.Utilities.Types;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Switching;
using JetBrains.Annotations;

namespace Fabrica.Watch
{


    public class WatchFactory : IWatchFactory
    {


        public WatchFactory( int initialPoolSize=1000 )
        {

            Pool = new Pool<Logger>( ()=> new Logger((l) => Pool.Return(l)), initialPoolSize * 10);

            for( var i=0; i<initialPoolSize; i++ )
                Pool.Return( new Logger((l) => Pool.Return(l)) );

        }


        private Pool<Logger> Pool { get; }


        private static readonly ILogger Silencer = new QuietLogger();

        public bool Quiet { get; set; }

        public ISwitchSource Switches { get; set; }
        public IEventSink Sink { get; set; }

        public IEventSink GetSink<T>() where T : class, IEventSink
        {

            IEventSink snk = null;
            switch (Sink)
            {
                case CompositeSink cs:
                    snk = cs.InnerSinks.FirstOrDefault(s => s is T);
                    break;
                case T _:
                    snk = Sink;
                    break;
            }

            return snk;

        }

        private readonly ConcurrentBag<object> _infrastructure = new ConcurrentBag<object>();
        public TType GetInfrastructure<TType>() where TType: class
        {
            var item = _infrastructure.FirstOrDefault(i => i is TType);
            return (TType)item;
        }

        public void AddInfrastructure( object item )
        {
            _infrastructure.Add(item);
        }


        public virtual void Configure( ISwitchSource switches, IEventSink sink, bool quiet=false )
        {

            Quiet = quiet;

            Switches = switches ?? throw new ArgumentNullException(nameof(switches));
            Sink     = sink ?? throw new ArgumentNullException(nameof(sink));

        }


        public virtual void Start()
        {

            Switches.Start();

            Sink.Start();

        }


        public virtual void Stop()
        {

            try
            {
                Switches.Stop();
            }
            catch
            {
                //ignored
            }

            try
            {
                Sink.Stop();
            }
            catch
            {
                //ignored
            }

            try
            {

                foreach( var item in _infrastructure )
                {
                    if( item is IDisposable disp )
                        disp.Dispose();
                }

            }
            catch
            {
                //ignored
            }


        }



        [NotNull]
        public virtual ILogger GetLogger( string category, bool retroOn=true )
        {

            if (Quiet)
                return Silencer;


            var corrId = ShortGuid.NewSequentialGuid().ToString();
            var sw     = Switches.Lookup( category );


            var logger = Pool.Aquire(0);
            
            logger.Config(Sink, retroOn, "", "", sw.Tag, category, corrId, sw.Level, sw.Color);

            return logger;

        }


        [NotNull]
        public virtual ILogger GetLogger<T>( bool retroOn = true )
        {

            if (Quiet)
                return Silencer;

            var category = typeof(T).FullName??"";
            var logger   = GetLogger( category, retroOn );

            return logger;

        }


        [NotNull]
        public virtual ILogger GetLogger( Type type, bool retroOn = true)
        {

            var category = type.FullName??"";
            var logger   = GetLogger( category, retroOn );

            return logger;

        }


        [NotNull]
        public ILogger GetLogger( LoggerRequest request, bool retroOn = true )
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            if (Quiet)
                return Silencer;


            // ************************************************************
            var sw = Switches.GetDefaultSwitch();

            if (request.Debug)
                sw = Switches.GetDebugSwitch();
            else
            {

                var found = false;
                foreach( var (key, target) in request.FilterKeys )
                {
                    found = Switches.Lookup(key, target, request.Category, out sw);
                    if( found )
                        break;
                }

                if( !found )
                    sw = Switches.Lookup( request.Category );

            }


            // ************************************************************
            var logger = Pool.Aquire(0);

            logger.Config( Sink, retroOn, request.Tenant, request.Subject, sw.Tag, request.Category, request.CorrelationId, sw.Level, sw.Color );


            // ************************************************************
            return logger;


        }



    }


}
