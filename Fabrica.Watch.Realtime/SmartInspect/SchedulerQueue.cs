//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Manages a queue of scheduler commands.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is responsible for managing a queue of scheduler
	///   commands. This functionality is needed by the
	///   <link Protocol.IsValidOption, asynchronous protocol mode>
	///   and the Scheduler class. New commands can be added with the
	///   Enqueue method. Commands can be dequeued with Dequeue. This
	///   queue does not have a maximum size or count.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class SchedulerQueue
	{
		private const int OVERHEAD = 24;
		private long fSize;
		private int fCount;
		private SchedulerQueueItem fHead;
		private SchedulerQueueItem fTail;

		class SchedulerQueueItem
		{
			public SchedulerCommand Command;
			public SchedulerQueueItem Next;
			public SchedulerQueueItem Previous;
		}

		/// <summary>
		///   Adds a new scheduler command to the queue.
		/// </summary>
		/// <param name="command">The command to add.</param>
		/// <!--
		/// <remarks>
		///   This method adds the supplied scheduler command to the
		///   queue. The Size of the queue is incremented by the size of
		///   the supplied command (plus some internal management overhead).
		///   This queue does not have a maximum size or count.
		/// </remarks>
		/// -->

		public void Enqueue(SchedulerCommand command)
		{
			SchedulerQueueItem item = new SchedulerQueueItem();
			item.Command = command;
			Add(item);
		}

		private void Add(SchedulerQueueItem item)
		{
			if (this.fTail == null)
			{
				this.fTail = item;
				this.fHead = item;
			}
			else
			{
				this.fTail.Next = item;
				item.Previous = this.fTail;
				this.fTail = item;
			}

			this.fCount++;
			this.fSize += item.Command.Size + OVERHEAD;
		}

		/// <summary>
		///   Returns a scheduler command and removes it from the queue.
		/// </summary>
		/// <returns>
		///   The removed scheduler command or null if the queue does not
		///   contain any packets.
		/// </returns>
		/// <!--
		/// <remarks>
		///   If the queue is not empty, this method removes the oldest
		///   scheduler command from the queue (also known as FIFO) and
		///   returns it. The total Size of the queue is decremented by
		///   the size of the returned command (plus some internal
		///   management overhead).
		/// </remarks>
		/// -->

		public SchedulerCommand Dequeue()
		{
			SchedulerQueueItem item = this.fHead;

			if (item != null)
			{
				Remove(item);
				return item.Command;
			}
			else
			{
				return null;
			}
		}

		private void Remove(SchedulerQueueItem item)
		{
			if (item == this.fHead) /* Head */
			{
				this.fHead = item.Next;
				if (this.fHead != null)
				{
					this.fHead.Previous = null;
				}
				else /* Was also tail */
				{
					this.fTail = null;
				}
			}
			else 
			{
				item.Previous.Next = item.Next;
				if (item.Next == null) /* Tail */
				{
					this.fTail = item.Previous;
				}
				else 
				{
					item.Next.Previous = item.Previous;
				}
			}

			this.fCount--;
			this.fSize -= item.Command.Size + OVERHEAD;
		}

		/// <summary>
		///   Tries to skip and remove scheduler commands from this queue.
		/// </summary>
		/// <param name="size">
		///   The minimum amount of bytes to remove from this queue.
		/// </param>
		/// <returns>
		///   True if enough scheduler commands could be removed and false
		///   otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method removes the next WritePacket scheduler commands
		///   from this queue until the specified minimum amount of bytes
		///   has been removed. Administrative scheduler commands (connect,
		///   disconnect or dispatch) are not removed. If the queue is
		///   currently empty or does not contain enough WritePacket
		///   commands to achieve the specified minimum amount of bytes,
		///   this method returns false.
		/// </remarks>
		/// -->

		public bool Trim(int size)
		{
			if (size <= 0)
			{
				return true;
			}

			int removedBytes = 0;
			SchedulerQueueItem item = this.fHead;

			while (item != null)
			{
				if (item.Command.Action == SchedulerAction.WritePacket)
				{
					removedBytes += item.Command.Size + OVERHEAD;
					Remove(item);

					if (removedBytes >= size)
					{
						return true;
					}
				}

				item = item.Next;
			}

			return false;
		}

		/// <summary>
		///   Removes all scheduler commands from this queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Removing all scheduler commands of the queue is done by
		///   calling the Dequeue method for each command in the current
		///   queue.
		/// </remarks>
		/// -->

		public void Clear()
		{
			while (Dequeue() != null) ;
		}

		/// <summary>
		///   Returns the current amount of scheduler commands in this
		///   queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For each added scheduler command this counter is incremented
		///   by one and for each removed command (with Dequeue) this
		///   counter is decremented by one. If the queue is empty, this
		///   property returns 0.
		/// </remarks>
		/// -->

		public int Count
		{
			get { return this.fCount; }
		}

		/// <summary>
		///   Returns the current size of this queue in bytes.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For each added scheduler command this counter is incremented
		///   by the size of the command (plus some internal management
		///   overhead) and for each removed command (with Dequeue) this
		///   counter is then decremented again. If the queue is empty,
		///   this property returns 0.
		/// </remarks>
		/// -->

		public long Size
		{
			get { return this.fSize; }
		}
	}
}