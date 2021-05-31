//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.Threading;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for scheduling protocol operations and executing
	///   them asynchronously in a different thread of control.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by the <link Protocol.IsValidOption,
	///   asynchronous protocol mode> to asynchronously execute protocol
	///   operations. New commands can be scheduled for execution with
	///   the Schedule method. The scheduler can be started and stopped
	///   with the Start and Stop methods. The scheduler uses a size
	///   limited queue to buffer scheduler commands. The maximum size of
	///   this queue can be set with the Threshold property. To influence
	///   the behavior of the scheduler if new commands are enqueued and
	///   the queue is currently considered full, you can specify the
	///   Throttle mode.
	/// </remarks>
	/// <threadsafety>
	///   This class is guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class Scheduler
	{
		private const int BUFFER_SIZE = 0x10;

		private Thread fThread;
		private SchedulerQueue fQueue;
		private object fMonitor;
		private bool fThrottle;
		private long fThreshold;
		private Protocol fProtocol;
		private bool fStopped;
		private bool fStarted;
		private SchedulerCommand[] fBuffer;

		/// <summary>
		///   Creates and initializes a new Scheduler instance.
		/// </summary>
		/// <param name="protocol">
		///   The protocol on which to execute the actual operations like
		///   connect, disconnect, write or dispatch.
		/// </param>

		public Scheduler(Protocol protocol)
		{
			this.fProtocol = protocol;
			this.fMonitor = new object();
			this.fQueue = new SchedulerQueue();
			this.fBuffer = new SchedulerCommand[BUFFER_SIZE];
		}

		/// <summary>
		///   Starts this scheduler and the internal scheduler thread.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method must be called before scheduling new commands
		///   with the Schedule method. Call Stop to stop the internal
		///   thread when the scheduler is no longer needed. Note that
		///   this method starts the internal scheduler thread only once.
		///   This means that subsequent calls to this method have no
		///   effect.
		/// </remarks>
		/// -->

		public void Start()
		{
			lock (this.fMonitor)
			{
				if (this.fStarted)
				{
					return; /* Can be started only once */
				}

				this.fThread = new Thread(new ThreadStart(Run));
				this.fThread.Start();
				this.fStarted = true;
			}
		}

		/// <summary>
		///   Stops this scheduler and the internal scheduler thread.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This is the matching method for Start. After calling this
		///   method, new commands will no longer be accepted by Schedule
		///   and are ignored. This method blocks until the internal
		///   thread has processed the current content of the queue.
		///   Call Clear before calling Stop to exit the internal thread
		///   as soon as possible.
		/// </remarks>
		/// -->

		public void Stop()
		{
			lock (this.fMonitor)
			{
				if (!this.fStarted)
				{
					return; /* Not started yet */
				}

				this.fStopped = true;
				Monitor.Pulse(this.fMonitor);
			}

			this.fThread.Join();
		}

		private void Run()
		{
			while (true)
			{
				int count = Dequeue();

				if (count == 0)
				{
					break; /* Stopped and no more commands */
				}

				if (!RunCommands(count))
				{
					break; /* Stopped */
				}
			}

			this.fBuffer = null;
		}

		private bool RunCommands(int count)
		{
			for (int i = 0; i < count; i++)
			{
				bool stopped = this.fStopped; /* See below. */

				SchedulerCommand command = this.fBuffer[i];
				RunCommand(command);
				this.fBuffer[i] = null;

				if (!stopped)
				{
					continue;
				}

				/* The scheduler has been stopped before the last
				 * command has been processed. To shutdown this
				 * thread as fast as possible we check if the last
				 * command of the protocol has failed (or if the
				 * last command has failed to change the previous
				 * failure status, respectively). If this is the
				 * case, we clear the queue and exit this thread
				 * immediately. */

				if (this.fProtocol.Failed)
				{
					Clear();
					return false;
				}
			}

			return true;
		}

		private void RunCommand(SchedulerCommand command)
		{
			/* Process the dequeued command. The Impl methods cannot
			 * throw an exception. Exceptions are reported with the
			 * error event of the protocol in asynchronous mode. */

			switch (command.Action)
			{
				case SchedulerAction.Connect:
					this.fProtocol.ImplConnect();
					break;

				case SchedulerAction.WritePacket:
					Packet packet = (Packet)command.State;
					this.fProtocol.ImplWritePacket(packet);
					break;

				case SchedulerAction.Disconnect:
					this.fProtocol.ImplDisconnect();
					break;

				case SchedulerAction.Dispatch:
					ProtocolCommand cmd =
						(ProtocolCommand) command.State;
					this.fProtocol.ImplDispatch(cmd);
					break;
			}
		}

		/// <summary>
		///   Schedules a new command for asynchronous execution.
		/// </summary>
		/// <param name="command">The command to schedule.</param>
		/// <returns>
		///   True if the command could be scheduled for asynchronous
		///   execution and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method adds the passed command to the internal queue
		///   of scheduler commands. The command is eventually executed
		///   by the internal scheduler thread. This method can block the
		///   caller if the scheduler operates in Throttle mode and the
		///   internal queue is currently considered full (see Threshold).
		/// </remarks>
		/// -->

		public bool Schedule(SchedulerCommand command)
		{
			return Enqueue(command);
		}

		/// <summary>
		///   Specifies the maximum size of the scheduler command queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   To influence the behavior of the scheduler if new commands
		///   are enqueued and the queue is currently considered full,
		///   you can specify the Throttle mode.
		/// </remarks>
		/// -->

		public long Threshold
		{
			get { return this.fThreshold; }
			set { this.fThreshold = value; }
		}

		/// <summary>
		///   Specifies if the scheduler should automatically throttle
		///   threads that enqueue new scheduler commands.
		/// </summary>
		/// <!--
		/// <remarks>
		///   If this property is true and the queue is considered full
		///   when enqueuing new commands, the enqueuing thread is
		///   automatically throttled until there is room in the queue
		///   for the new command. In non-throttle mode, the thread is
		///   not blocked but older commands are removed from the queue.
		/// </remarks>
		/// -->

		public bool Throttle
		{
			get { return this.fThrottle; }
			set { this.fThrottle = value; }
		}

		private bool Enqueue(SchedulerCommand command)
		{
			if (!this.fStarted)
			{
				return false; /* Not yet started */
			}

			if (this.fStopped)
			{
				return false; /* No new commands anymore */
			}

			int commandSize = command.Size;

			if (commandSize > this.fThreshold)
			{
				return false;
			}

			lock (this.fMonitor)
			{
				if (!this.fThrottle || this.fProtocol.Failed)
				{
					if (this.fQueue.Size + commandSize > this.fThreshold)
					{
						this.fQueue.Trim(commandSize);
					}
				}
				else 
				{
					while (this.fQueue.Size + commandSize > this.fThreshold)
					{
						Monitor.Wait(this.fMonitor);
					}
				}

				this.fQueue.Enqueue(command);
				Monitor.Pulse(this.fMonitor);
			}

			return true;
		}

		private int Dequeue()
		{
			int count = 0;
			int length = this.fBuffer.Length;

			lock (this.fMonitor)
			{
				while (this.fQueue.Count == 0)
				{
					if (this.fStopped)
					{
						break;
					}

					Monitor.Wait(this.fMonitor);
				}

				while (this.fQueue.Count > 0)
				{
					this.fBuffer[count] = this.fQueue.Dequeue();

					if (++count >= length)
					{
						break;
					}
				}

				Monitor.Pulse(this.fMonitor);
			}

			return count;
		}

		/// <summary>
		///   Removes all scheduler commands from this scheduler.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method clears the current queue of scheduler commands.
		///   If the Stop method is called after calling Clear and no new
		///   commands are stored between these two calls, the internal
		///   scheduler thread will exit as soon as possible (after the
		///   current command, if any, has been processed).
		/// </remarks>
		/// -->

		public void Clear()
		{
			lock (this.fMonitor)
			{
				this.fQueue.Clear();
				Monitor.Pulse(this.fMonitor);
			}
		}
	}
}
