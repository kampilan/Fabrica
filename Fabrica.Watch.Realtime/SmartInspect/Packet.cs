//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.Diagnostics;
using System.Security;
using System.Threading;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Is the abstract base class for all packets in the SmartInspect
	///   .NET library.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is the base class for all packets in the SmartInspect
	///   .NET library. The following table lists the available packets
	///   together with a short description.
	///
	///   <table>
	///   Packet              Description
	///   +                   +
	///   ControlCommand      Responsible for administrative tasks like
	///                         clearing the Console.
	///
	///   LogEntry            Represents the most important packet in the
	///                         entire SmartInspect concept. Is used for
	///                         the majority of logging methods in the
	///                         Session class.
	///
	///   LogHeader           Responsible for storing and transferring
	///                         log metadata. Used by the PipeProtocol and
	///                         TcpProtocol classes to support the filter
	///                         and trigger functionality of the
	///                         SmartInspect Router service application.
	///
	///   ProcessFlow         Responsible for managing thread and process
	///                         information about your application.
	///
	///   Watch               Responsible for handling variable watches. 
	///   </table>
	/// </remarks>
	/// <threadsafety>
	///   This class and sub-classes are not guaranteed to be threadsafe.
	///   To ensure thread-safety, use ThreadSafe as well as the Lock and
	///   Unlock methods.
	/// </threadsafety>
	/// -->

	public abstract class Packet
	{
		private Level fLevel;
		internal const int PACKET_HEADER = 6;
		private int fBytes;
		private object fLock;
		private bool fThreadSafe;

		/// <summary>
		///   Creates and initializes a Packet instance with a default log
		///   level of Level.Message.
		/// </summary>

		public Packet()
		{
			this.fLevel = Level.Message;
		}

		/// <summary>
		///   Represents the amount of bytes needed for storing this packet
		///   in the standard SmartInspect binary log file format as
		///   represented by BinaryFormatter.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Please note that this property is only set and used by the
		///   SmartInspect SDK. The SmartInspect SDK is a small library
		///   for reading SmartInspect binary log files and is available
		///   for download on the Gurock Software website.
		/// </remarks>
		/// -->

		public int Bytes
		{
			get { return this.fBytes; }
			set { this.fBytes = value; }
		}

		/// <summary>
		///   Represents the log level of this packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Every packet can have a certain log level value. Log levels
		///   describe the severity of a packet. Please see the Level
		///   enum for more information about log levels and their usage.
		/// </remarks>
		/// -->

		public Level Level
		{
			get { return this.fLevel; }
			set { this.fLevel = value; }
		}

		/// <summary>
		///   Returns the ID of the current thread.
		/// </summary>
		/// <returns>
		///   The ID the current thread or 0 if the caller does not have
		///   the required permissions to retrieve the ID of the current
		///   thread.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method is intended to be used by derived packet classes
		///   which make use of a thread ID. Please note that this method
		///   catches any SecurityException and returns 0 in this case.
		/// </remarks>
		/// -->

		protected static int GetThreadId()
		{
			int threadId;

			try 
			{
#if (SI_DOTNET_1x)
				threadId = AppDomain.GetCurrentThreadId();
#else
				threadId = Thread.CurrentThread.ManagedThreadId;
#endif
			}
			catch (SecurityException)
			{
				threadId = 0;
			}

			return threadId;
		}

		/// <summary>
		///   Returns the ID of the current process.
		/// </summary>
		/// <returns>
		///   The ID the current process or 0 if the caller does not have
		///   the required permissions to retrieve the ID of the current
		///   process.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method is intended to be used by derived packet classes
		///   which make use of a process ID. Please note that this method
		///   catches any SecurityException and returns 0 in this case.
		/// </remarks>
		/// -->

		protected static int GetProcessId()
		{
			int processId;

			try 
			{
				processId = Process.GetCurrentProcess().Id;
			}
			catch (SecurityException)
			{
				processId = 0;
			}

			return processId;
		}

		/// <summary>
		///   Returns the memory size occupied by a string.
		/// </summary>
		/// <param name="s">
		///   The string whose memory size to return. Can be null.
		/// </param>
		/// <returns>
		///   The memory size occupied by the supplied string or 0 if the
		///   supplied argument is null.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method calculates and returns the total memory size
		///   occupied by the supplied string. If the supplied argument
		///   is null, 0 is returned.
		/// </remarks>
		/// -->

		protected static int GetStringSize(string s)
		{
			if (s != null)
			{
				return s.Length * 2;
			}
			else 
			{
				return 0;
			}
		}

		/// <summary>
		///   Calculates and returns the total memory size occupied by
		///   this packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This read-only property returns the total occupied memory
		///   size of this packet. This functionality is used by the
		///   <link Protocol.IsValidOption, backlog> protocol feature
		///   to calculate the total backlog queue size.
		/// </remarks>
		/// -->

		public abstract int Size { get; }

		/// <summary>
		///   Represents the type of this packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This read-only property returns the type of this packet.
		///   Please see the PacketType enum for a list of available
		///   packet types.
		/// </remarks>
		/// -->

		public abstract PacketType PacketType { get; }

		/// <summary>
		///   Locks this packet for safe multi-threaded packet processing
		///   if this packet is operating in thread-safe mode.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Call this method before reading or changing properties of a
		///   packet when using this packet from multiple threads at the
		///   same time. This is needed, for example, when one or more
		///   <link SmartInspect.Connections, connections> of a SmartInspect
		///   object are told to operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>. Each Lock call must be
		///   matched by a call to Unlock.
		/// 
		///   Before using Lock and Unlock in a multi-threaded environment
		///   you must indicate that this packet should operate in
		///   thread-safe mode by setting the ThreadSafe property to true.
		///   Otherwise, the Lock and Unlock methods do nothing. Note
		///   that setting the ThreadSafe property is done automatically
		///   if this packet has been created by the Session class and is
		///   processed by a related SmartInspect object which has one or
		///   more connections which operate in asynchronous protocol
		///   mode.
		/// </remarks>
		/// -->

		public void Lock()
		{
			if (this.fThreadSafe)
			{
				Monitor.Enter(this.fLock);
			}
		}

		/// <summary>
		///   Unlocks a previously locked packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Call this method after reading or changing properties of a
		///   packet when using this packet from multiple threads at the
		///   same time. This is needed, for example, when one or more
		///   <link SmartInspect.Connections, connections> of a SmartInspect
		///   object are told to operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>. Each Unlock call must be
		///   matched by a previous call to Lock.
		/// 
		///   Before using Lock and Unlock in a multi-threaded environment
		///   you must indicate that this packet should operate in
		///   thread-safe mode by setting the ThreadSafe property to true.
		///   Otherwise, the Lock and Unlock methods do nothing. Note
		///   that setting the ThreadSafe property is done automatically
		///   if this packet has been created by the Session class and is
		///   processed by a related SmartInspect object which has one or
		///   more connections which operate in asynchronous protocol
		///   mode.
		/// </remarks>
		/// -->

		public void Unlock()
		{
			if (this.fThreadSafe)
			{
				Monitor.Exit(this.fLock);
			}
		}

		/// <summary>
		///   Indicates if this packet is used in a multi-threaded
		///   SmartInspect environment.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Set this property to true before calling Lock and Unlock
		///   in a multi-threaded environment. Otherwise, the Lock and
		///   Unlock methods do nothing. Note that setting this
		///   property is done automatically if this packet has been
		///   created by the Session class and is processed by a related
		///   SmartInspect object which has one or more connections which
		///   operate in asynchronous protocol mode.
		/// 
		///   Setting this property must be done before using this packet
		///   from multiple threads simultaneously.
		/// </remarks>
		/// -->

		public bool ThreadSafe
		{
			get { return this.fThreadSafe; }
			
			set 
			{
				if (value == this.fThreadSafe)
				{
					return;
				}

				this.fThreadSafe = value;

				if (value)
				{
					this.fLock = new object();
				}
				else 
				{
					this.fLock = null;
				}
			}
		}
	}
}
