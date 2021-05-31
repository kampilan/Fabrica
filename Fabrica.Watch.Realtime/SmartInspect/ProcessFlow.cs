//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the Process Flow packet type which is used in the
	///   Enter-/Leave methods in the Session class.
	/// </summary>
	/// <!--
	/// <remarks>
	///   A Process Flow entry is responsible for illustrated process and
	///   thread information. 
	///   
	///   It has several properties which describe its creation context
	///   (like a thread ID, timestamp or hostname) and other properties
	///   which specify the way the Console interprets this packet (like the
	///   process flow ID). Furthermore a Process Flow entry contains the
	///   actual data, namely the title, which will be displayed in the
	///   Console.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe. However, instances
	///   of this class will normally only be used in the context of a
	///   single thread.
	/// </threadsafety>
	/// -->

	public sealed class ProcessFlow: Packet
	{
		private string fHostName;
		private ProcessFlowType fProcessFlowType;
		private string fTitle;
		private DateTime fTimestamp;
		private int fThreadId;
		private int fProcessId;

		private static int PROCESS_ID = GetProcessId();
		private const int HEADER_SIZE = 28;

		/// <summary>
		///   Overloaded. Creates and initializes a ProcessFlow instance.
		/// </summary>

		public ProcessFlow()
		{
 
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ProcessFlow instance with
		///   a custom process flow type.
		/// </summary>
		/// <param name="processFlowType">
		///   The type of the new Process Flow entry describes the way the
		///   Console interprets this packet. Please see the ProcessFlowType
		///   enum for more information.
		/// </param>

		public ProcessFlow(ProcessFlowType processFlowType)
		{
			this.fProcessFlowType = processFlowType;
			this.fThreadId = GetThreadId();
			this.fProcessId = PROCESS_ID;
		}

		/// <summary>
		///   Overridden. Returns the total occupied memory size of this
		///   Process Flow packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The total occupied memory size of this Process Flow entry is
		///   the size of memory occupied by all strings and any internal
		///   data structures of this Process Flow entry.
		/// </remarks>
		/// -->

		public override int Size
		{
			get 
			{ 
				return HEADER_SIZE +
					GetStringSize(this.fTitle) +
					GetStringSize(this.fHostName);
			}
		}

		/// <summary>
		///   Overridden. Returns PacketType.ProcessFlow.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For a complete list of available packet types, please have a
		///   look at the documentation of the PacketType enum.
		/// </remarks>
		/// -->

		public override PacketType PacketType
		{
			get { return PacketType.ProcessFlow; }
		}

		/// <summary>
		///   Represents the title of this Process Flow entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The title of this Process Flow entry will be empty in the
		///   SmartInspect Console when this property is set to null.
		/// </remarks>
		/// -->

		public string Title
		{
			get { return this.fTitle; }
			set { this.fTitle = value; }
		}

		/// <summary>
		///   Represents the hostname of this Process Flow entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The hostname of this Process Flow entry is usually set to the
		///   name of the machine this Process Flow entry is sent from. It
		///   will be empty in the SmartInspect Console when this property
		///   is set to null.
		/// </remarks>
		/// -->

		public string HostName
		{
			get { return this.fHostName; }
			set { this.fHostName = value; }
		}

		/// <summary>
		///   Represents the type of this Process Flow entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The type of the Process Flow entry describes the way the
		///   Console interprets this packet. Please see the ProcessFlowType
		///   enum for more information.
		/// </remarks>
		/// -->

		public ProcessFlowType ProcessFlowType
		{
			get { return this.fProcessFlowType; }
			set { this.fProcessFlowType = value; }
		}

		/// <summary>
		///   Represents the timestamp of this ProcessFlow object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property returns the creation time of this ProcessFlow
		///   object. If specified, the SmartInspect .NET library tries to
		///   use high-resolution timestamps. Please see the
		///   SmartInspect.Resolution property for more information
		///   on timestamps.
		/// </remarks>
		/// -->

		public DateTime Timestamp
		{
			get { return this.fTimestamp; }
			set { this.fTimestamp = value; }
		}

		/// <summary>
		///   Represents the thread ID of this ProcessFlow object.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   This property represents the ID of the thread this
		///   ProcessFlow object was created in.
		/// </remarks>
		/// -->

		public int ThreadId
		{
			get { return this.fThreadId; }
			set { this.fThreadId = value; }
		}

		/// <summary>
		///   Represents the process ID of this ProcessFlow object.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   This property represents the ID of the process this
		///   ProcessFlow object was created in.
		/// </remarks>
		/// -->

		public int ProcessId
		{
			get { return this.fProcessId; }
			set { this.fProcessId = value; }
		}
	}
}
