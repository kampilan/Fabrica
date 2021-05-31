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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fabrica.Watch.Sink
{

    public class BatchEventSink: IEventSink
    {

        public BatchEventSink( [NotNull] IEventSink targetSink )
        {
            TargetSink = targetSink;
        }

        private IEventSink TargetSink { get; }


        private ConcurrentQueue<ILogEvent> Queue { get; } = new ConcurrentQueue<ILogEvent>();
        private EventWaitHandle MustStop { get; } = new EventWaitHandle(false, EventResetMode.ManualReset);
        private EventWaitHandle Stopped { get; } = new EventWaitHandle(false, EventResetMode.ManualReset);


        public int BatchSize { get; set; } = 10;
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(50);
        public TimeSpan WaitForStopInterval { get; set; } = TimeSpan.FromSeconds(5);


        private bool Started { get; set; }
        public virtual void Start()
        {

            if (Started)
                return;

            TargetSink.Start();

            var task = new Task(_process);
            task.Start();

            Started = true;

        }


        public virtual void Stop()
        {

            MustStop.Set();

            Stopped.WaitOne(WaitForStopInterval);

        }


        public virtual void Accept(ILogEvent logEvent)
        {
            Queue.Enqueue(logEvent);
        }

        public virtual void Accept( [NotNull] IEnumerable<ILogEvent> batch )
        {

            foreach ( var le in batch )
                Queue.Enqueue( le );

        }


        private void _process()
        {

            while( !MustStop.WaitOne(PollingInterval) )
                Drain(false);

            Drain(true);

            Stopped.Set();

        }


        protected virtual void Drain( bool all )
        {

            if( Queue.IsEmpty )
                return;

            var batch = new List<ILogEvent>();

            while( !Queue.IsEmpty )
            {

                if( !all && batch.Count >= BatchSize )
                    break;

                if ( !Queue.TryDequeue(out var le) )
                    break;

                batch.Add(le);

            }

            if( batch.Count > 0 )
                TargetSink.Accept(batch);

        }


    }


}
