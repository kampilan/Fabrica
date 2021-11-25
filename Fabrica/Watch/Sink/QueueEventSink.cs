using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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


        public Task Accept( ILogEvent logEvent )
        {

            Interlocked.Add(ref _accepted, 1);

            if ( Queue.Count >= MaximumCount )
                Queue.TryDequeue( out _ );

            Queue.Enqueue( logEvent );

            return Task.CompletedTask;

        }

        public Task Accept( IEnumerable<ILogEvent> batch )
        {

            foreach( var le in batch )
                Accept(le);

            return Task.CompletedTask;

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
