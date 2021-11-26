
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fabrica.Watch.Sink
{

    public class TimerBatchEventSink: IEventSink
    {


        public TimerBatchEventSink([NotNull] IEventSink targetSink)
        {
            TargetSink = targetSink;
        }

        private IEventSink TargetSink { get; }


        private ConcurrentQueue<ILogEvent> Queue { get; } = new();
        private Timer DrainTimer { get; set; }

        public void Start()
        {

            DrainTimer = new Timer(Process, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1) );

            void Process( object state )
            {
                _ = _drain();
            }

        }

        private async Task _drain( bool all=false )
        {

            if (Queue.IsEmpty)
                return;

            var batch = new List<ILogEvent>();

            while (!Queue.IsEmpty)
            {

                if (!all && batch.Count >= 50)
                    break;

                if (!Queue.TryDequeue(out var le))
                    break;

                batch.Add(le);

            }

            if (batch.Count > 0)
                await TargetSink.Accept(batch);

        }


        public void Stop()
        {
            DrainTimer.Dispose();
        }


        public virtual Task Accept(ILogEvent logEvent)
        {
            Queue.Enqueue(logEvent);
            return Task.CompletedTask;
        }

        public virtual Task Accept([NotNull] IEnumerable<ILogEvent> batch)
        {

            foreach (var le in batch)
                Queue.Enqueue(le);

            return Task.CompletedTask;

        }


    }


}
