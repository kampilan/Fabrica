using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Fabrica.Watch.Sink
{

    
    public class QueueEventSink: IEventSink
    {


        public int MaximumCount { get; set; } = 1000;

        private ConcurrentQueue<ILogEvent> Queue { get; } = new ConcurrentQueue<ILogEvent>();


        public void Start()
        {
        }

        public void Stop()
        {
        }


        public void Accept( ILogEvent logEvent )
        {

            Interlocked.Add(ref _accepted, 1);

            if ( Queue.Count >= MaximumCount )
                Queue.TryDequeue( out _ );

            Queue.Enqueue( logEvent );

        }

        public void Accept( IEnumerable<ILogEvent> batch )
        {
            foreach( var le in batch )
                Accept(le);
        }

        private int _accepted = 0;
        public int Accepted => _accepted;

        public int Available => Queue.Count;


        public IList<ILogEvent> All()
        {

            var all = new List<ILogEvent>();
            while( Queue.TryDequeue( out var le) )
                all.Add(le);

            return all;

        }


    }


}
