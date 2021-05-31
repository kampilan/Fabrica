using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Threading
{


    public static class AsyncPump
    {
        /// <summary>Runs the specified asynchronous method.</summary>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        public static void Run([NotNull] Action asyncMethod)
        {
            if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(true);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function
                syncCtx.OperationStarted();
                asyncMethod();
                syncCtx.OperationCompleted();

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
            }
            finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
        }

        /// <summary>Runs the specified asynchronous method.</summary>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        public static void Run([NotNull] Func<Task> asyncMethod)
        {
            if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(false);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = asyncMethod();
                if (t == null) throw new InvalidOperationException("No task provided.");
                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
                t.GetAwaiter().GetResult();
            }
            finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
        }

        /// <summary>Runs the specified asynchronous method.</summary>
        /// <param name="asyncMethod">The asynchronous method to execute.</param>
        public static T Run<T>(Func<Task<T>> asyncMethod)
        {
            if (asyncMethod == null) throw new ArgumentNullException("asyncMethod");

            var prevCtx = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                var syncCtx = new SingleThreadSynchronizationContext(false);
                SynchronizationContext.SetSynchronizationContext(syncCtx);

                // Invoke the function and alert the context to when it completes
                var t = asyncMethod();
                if (t == null) throw new InvalidOperationException("No task provided.");
                t.ContinueWith(delegate { syncCtx.Complete(); }, TaskScheduler.Default);

                // Pump continuations and propagate any exceptions
                syncCtx.RunOnCurrentThread();
                return t.GetAwaiter().GetResult();
            }
            finally { SynchronizationContext.SetSynchronizationContext(prevCtx); }
        }

        /// <inheritdoc />
        /// <summary>Provides a SynchronizationContext that's single-threaded.</summary>
        private sealed class SingleThreadSynchronizationContext : SynchronizationContext
        {
            /// <summary>The queue of work items.</summary>
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();
            /// <summary>The processing thread.</summary>
            /// <summary>The number of outstanding operations.</summary>
            private int _operationCount;
            /// <summary>Whether to track operations _operationCount.</summary>
            private readonly bool _trackOperations;

            /// <inheritdoc />
            /// <summary>Initializes the context.</summary>
            /// <param name="trackOperations">Whether to track operation count.</param>
            internal SingleThreadSynchronizationContext(bool trackOperations)
            {
                _trackOperations = trackOperations;
            }

            /// <inheritdoc />
            /// <summary>Dispatches an asynchronous message to the synchronization context.</summary>
            /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
            /// <param name="state">The object passed to the delegate.</param>
            public override void Post([NotNull] SendOrPostCallback d, object state)
            {
                if (d == null) throw new ArgumentNullException("d");
                _queue.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
            }

            /// <inheritdoc />
            /// <summary>Not supported.</summary>
            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronously sending is not supported.");
            }

            /// <summary>Runs an loop to process all queued work items.</summary>
            public void RunOnCurrentThread()
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                    workItem.Key(workItem.Value);
            }

            /// <summary>Notifies the context that no more work will arrive.</summary>
            public void Complete() { _queue.CompleteAdding(); }

            /// <inheritdoc />
            /// <summary>Invoked when an async operation is started.</summary>
            public override void OperationStarted()
            {
                if (_trackOperations)
                    Interlocked.Increment(ref _operationCount);
            }

            /// <inheritdoc />
            /// <summary>Invoked when an async operation is completed.</summary>
            public override void OperationCompleted()
            {
                if (_trackOperations &&
                    Interlocked.Decrement(ref _operationCount) == 0)
                    Complete();
            }

        }

    }


}
