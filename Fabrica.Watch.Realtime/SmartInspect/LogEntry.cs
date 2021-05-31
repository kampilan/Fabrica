//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;
using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the Log Entry packet type which is used for nearly
	///   all logging methods in the Session class.
	/// </summary>
	/// <!--
	/// <remarks>
	///   A Log Entry is the most important packet available in the
	///   SmartInspect concept. It is used for almost all logging methods
	///   in the Session class, like, for example, Session.LogMessage,
	///   Session.LogObject or Session.LogSql.
	///   
	///   A Log Entry has several properties which describe its creation
	///   context (like a thread ID, timestamp or hostname) and other
	///   properties which specify the way the Console interprets this packet
	///   (like the viewer ID or the background color). Furthermore a Log
	///   Entry contains the actual data which will be displayed in the
	///   Console.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe. However, instances
	///   of this class will normally only be used in the context of a single
	///   thread.
	/// </threadsafety>
	/// -->

	public sealed class LogEntry: Packet
	{
		private string fSessionName;
		private string fTitle;
		private string fAppName;
		private string fHostName;
	    private string fCorrelationId;
        private LogEntryType fLogEntryType;
		private ViewerId fViewerId;
		private Color fColor;
		private Stream fData;
		private DateTime fTimestamp;
		private int fThreadId;
		private int fProcessId;

		private static int PROCESS_ID = GetProcessId();
		private const int HEADER_SIZE = 48;

		/// <summary>
		///   Overloaded. Creates and initializes a LogEntry instance.
		/// </summary>

		public LogEntry()
		{
 
		}

		/// <summary>
		///   Overloaded. Creates and initializes a LogEntry instance with
		///   a custom log entry type and custom viewer ID.
		/// </summary>
		/// <param name="logEntryType">
		///   The type of the new Log Entry describes the way the Console
		///   interprets this packet. Please see the LogEntryType enum for
		///   more information.
		/// </param>
		/// <param name="viewerId">
		///   The viewer ID of the new Log Entry describes which viewer
		///   should be used in the Console when displaying the data of
		///   this Log Entry. Please see ViewerId for more information.
		/// </param>

		public LogEntry(LogEntryType logEntryType, ViewerId viewerId)
		{
			this.fLogEntryType = logEntryType;
			this.fViewerId = viewerId;
			this.fColor = Session.DEFAULT_COLOR;
			this.fThreadId = GetThreadId();
			this.fProcessId = PROCESS_ID;
		}

		/// <summary>
		///   Overridden. Returns the total occupied memory size of this Log
		///   Entry packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The total occupied memory size of this Log Entry is the size
		///   of memory occupied by all strings, the optional Data stream
		///   and any internal data structures of this Log Entry.
		/// </remarks>
		/// -->

		public override int Size
		{
			get
			{
				int result = HEADER_SIZE +
					GetStringSize(this.fTitle) +
					GetStringSize(this.fSessionName) +
					GetStringSize(this.fHostName) +
                    GetStringSize(this.fCorrelationId) +
                    GetStringSize(this.fAppName);

				if (this.fData != null)
				{
					result += (int) this.fData.Length;
				}

				return result;
			}
		}

		/// <summary>
		///   Overridden. Returns PacketType.LogEntry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For a complete list of available packet types, please have a
		///   look at the documentation of the PacketType enum.
		/// </remarks>
		/// -->

		public override PacketType PacketType
		{
			get { return PacketType.LogEntry; }
		}

		/// <summary>
		///   Represents the title of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The title of this Log Entry will be empty in the SmartInspect
		///   Console when this property is set to null.
		/// </remarks>
		/// -->

		public string Title
		{
			get { return this.fTitle; }
			set { this.fTitle = value; }
		}

		/// <summary>
		///   Represents the session name of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The session name of a Log Entry is normally set to the name
		///   of the session which sent this Log Entry. It will be empty in
		///   the SmartInspect Console when this property is set to null.
		/// </remarks>
		/// -->

		public string SessionName
		{
			get { return this.fSessionName; }
			set { this.fSessionName = value; }
		}

		/// <summary>
		///   Represents the background color of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The background color of a Log Entry is normally set to the
		///   color of the session which sent this Log Entry.
		/// </remarks>
		/// -->

		public Color Color
		{
			get { return this.fColor; }
			set { this.fColor = value; }
		}

		/// <summary>
		///   Represents the type of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The type of this Log Entry describes the way the Console
		///   interprets this packet. Please see the LogEntryType enum for more
		///   information.
		/// </remarks>
		/// -->

		public LogEntryType LogEntryType
		{
			get { return this.fLogEntryType; }
			set { this.fLogEntryType = value; }
		}

		/// <summary>
		///   Represents the viewer ID of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The viewer ID of the Log Entry describes which viewer should
		///   be used in the Console when displaying the data of this Log
		///   Entry. Please see the ViewerId enum for more information.
		/// </remarks>
		/// -->

		public ViewerId ViewerId
		{
			get { return this.fViewerId; }
			set { this.fViewerId = value; }
		}

		/// <summary>
		///   Represents the optional data stream of the Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The property can be null if this Log Entry does not contain
		///   additional data. 
		///   
		///   <b>Important:</b> Treat this stream as read-only. This means,
		///   modifying this stream in any way is not supported. Additionally,
		///   only pass streams which support seeking. Streams which do not
		///   support seeking cannot be used by this class.
		/// </remarks>
		/// -->

		public Stream Data
		{
			get { return this.fData; }
			set { this.fData = value; }
		}

		/// <summary>
		///   Represents the application name of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The application name of a Log Entry is usually set to the
		///   name of the application this Log Entry is created in. It will
		///   be empty in the SmartInspect Console when this property is set
		///   to null.
		/// </remarks>
		/// -->

		public string AppName
		{
			get { return this.fAppName; }
			set { this.fAppName = value; }
		}

		/// <summary>
		///   Represents the hostname of this Log Entry.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The hostname of a Log Entry is usually set to the name of
		///   the machine this Log Entry is sent from. It will be empty in
		///   the SmartInspect Console when this property is set to null.
		/// </remarks>
		/// -->

		public string HostName
		{
			get { return this.fHostName; }
			set { this.fHostName = value; }
		}


        /// <summary>
        ///   Represents the correlationId of this Log Entry.
        /// </summary>
        /// <!--
        /// <remarks>
        ///   The correlationId of a Log Entry.
        /// </remarks>
        /// -->

        public string CorrelationId
        {
            get { return this.fCorrelationId; }
            set { this.fCorrelationId = value; }
        }



		/// <summary>
		///   Represents the timestamp of this LogEntry object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property returns the creation time of this LogEntry
		///   object. If specified, the SmartInspect .NET library tries
		///   to use high-resolution timestamps. Please see the
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
		///   Represents the thread ID of this LogEntry object.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   This property represents the ID of the thread this LogEntry
		///   object was created in.
		/// </remarks>
		/// -->

		public int ThreadId
		{
			get { return this.fThreadId; }
			set { this.fThreadId = value; }
		}

		/// <summary>
		///   Represents the process ID of this LogEntry object.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   This property represents the ID of the process this LogEntry
		///   object was created in.
		/// </remarks>
		/// -->

		public int ProcessId
		{
			get { return this.fProcessId; }
			set { this.fProcessId = value; }
		}

		/// <summary>
		///   Indicates if this LogEntry contains optional data or not.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Returns true if this LogEntry packet contains optional data
		///   and false otherwise.
		/// </remarks>
		/// -->

		internal bool HasData
		{
			get { return this.fData != null && this.Data.Length > 0; }
		}
	}
}
