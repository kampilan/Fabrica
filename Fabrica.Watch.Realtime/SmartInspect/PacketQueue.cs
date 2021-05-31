//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Manages a memory size limited queue of packets.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is responsible for managing a size limited queue
	///   of packets. This functionality is needed by the protocol
	///   <link Protocol.IsValidOption, backlog> feature. The maximum
	///   total memory size of the queue can be set with the Backlog
	///   property. New packets can be added with the Push method. Packets
	///   which are no longer needed can be retrieved and removed from the
	///   queue with the Pop method.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class PacketQueue
	{
		private const int OVERHEAD = 24;
		private long fBacklog;
		private long fSize;
		private int fCount;
		private PacketQueueItem fHead;
		private PacketQueueItem fTail;

		class PacketQueueItem
		{
			public Packet Packet;
			public PacketQueueItem Next;
			public PacketQueueItem Previous;
		}

		/// <summary>
		///   Adds a new packet to the queue.
		/// </summary>
		/// <param name="packet">The packet to add.</param>
		/// <!--
		/// <remarks>
		///   This method adds the supplied packet to the queue. The size
		///   of the queue is incremented by the size of the supplied
		///   packet (plus some internal management overhead). If the total
		///   occupied memory size of this queue exceeds the Backlog limit
		///   after adding the new packet, then already added packets will
		///   be removed from this queue until the Backlog size limit is
		///   reached again.
		/// </remarks>
		/// -->

		public void Push(Packet packet)
		{
			PacketQueueItem item = new PacketQueueItem();
			item.Packet = packet;

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
			this.fSize += packet.Size + OVERHEAD;
			Resize();
		}

		/// <summary>
		///   Returns a packet and removes it from the queue.
		/// </summary>
		/// <returns>
		///   The removed packet or null if the queue does not contain any
		///   packets.
		/// </returns>
		/// <!--
		/// <remarks>
		///   If the queue is not empty, this method removes the oldest
		///   packet from the queue (also known as FIFO) and returns it.
		///   The total size of the queue is decremented by the size of
		///   the returned packet (plus some internal management overhead).
		/// </remarks>
		/// -->

		public Packet Pop()
		{
			Packet result = null;
			PacketQueueItem item = this.fHead;

			if (item != null)
			{
				result = item.Packet;
				this.fHead = item.Next;

				if (this.fHead != null)
				{
					this.fHead.Previous = null;
				}
				else 
				{
					this.fTail = null;
				}

				this.fCount--;
				this.fSize -= result.Size + OVERHEAD;
			}

			return result;
		}

		/// <summary>
		///   Removes all packets from this queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Removing all packets of the queue is done by calling the Pop
		///   method for each packet in the current queue.
		/// </remarks>
		/// -->

		public void Clear()
		{
			while (Pop() != null);
		}

		private void Resize()
		{
			while (this.fBacklog < this.fSize)
			{
				if (Pop() == null)
				{
					this.fSize = 0;
					break;
				}
			}
		}

		/// <summary>
		///   Specifies the total maximum memory size of this queue in
		///   bytes.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Each time a new packet is added with the Push method, it will
		///   be verified that the total occupied memory size of the queue
		///   still falls below the supplied Backlog limit. To satisfy this
		///   constraint, old packets are removed from the queue when
		///   necessary.
		/// </remarks>
		/// -->

		public long Backlog
		{
			get { return this.fBacklog; }

			set 
			{
				this.fBacklog = value; 
				Resize();
			}
		}

		/// <summary>
		///   Returns the current amount of packets in this queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For each added packet this counter is incremented by one
		///   and for each removed packet (either with the Pop method
		///   or automatically while resizing the queue) this counter
		///   is decremented by one. If the queue is empty, this property
		///   returns 0.
		/// </remarks>
		/// -->

		public int Count
		{
			get { return this.fCount; }
		}
	}
}
