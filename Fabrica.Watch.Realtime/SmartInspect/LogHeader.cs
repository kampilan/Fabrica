//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the Log Header packet type which is used for storing
	///   and transferring log metadata.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The LogHeader class is used to store and transfer log metadata.
	///   After the PipeProtocol or TcpProtocol has established a connection,
	///   a Log Header packet with the metadata of the current logging
	///   context is created and written. Log Header packets are used by
	///   the SmartInspect Router application for its filter and trigger
	///   functionality.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe. However, instances
	///   of this class will normally only be used in the context of a single
	///   thread.
	/// </threadsafety>
	/// -->

	public class LogHeader: Packet
	{
		private const int HEADER_SIZE = 4;

		private string fAppName;
		private string fHostName;

		/// <summary>
		///   Overridden. Returns the total occupied memory size of this Log
		///   Header packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The total occupied memory size of this Log Header is the size
		///   of memory occupied by all strings and any internal data
		///   structures of this Log Header.
		/// </remarks>
		/// -->

		public override int Size
		{
			get 
			{
				return HEADER_SIZE +
					GetStringSize(Content);
			}
		}

		/// <summary>
		///   Represents the application name of this Log Header.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The application name of a Log Header is usually set to the
		///   name of the application this Log Header is created in.
		/// </remarks>
		/// -->

		public string AppName
		{
			get { return this.fAppName; }
			set { this.fAppName = value; }
		}

		/// <summary>
		///   Represents the hostname of this Log Header.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The hostname of a Log Header is usually set to the name of
		///   the machine this Log Header is sent from.
		/// </remarks>
		/// -->

		public string HostName
		{
			get { return this.fHostName; }
			set { this.fHostName = value; }
		}

		/// <summary>
		///   Represents the entire content of this Log Header packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The content of a Log Header packet is a key-value (syntax:
		///   key=value) list of the properties of this Log Header packet
		///   (currently only the AppName and the HostName strings).
		///   Key-value pairs are separated by carriage return and newline
		///   characters.
		/// </remarks>
		/// -->

		public string Content
		{
			get 
			{
				return String.Concat(
					"hostname=",
					this.fHostName,
					"\r\n",
					"appname=",
					this.fAppName,
					"\r\n");
			}
		}

		/// <summary>
		///   Overridden. Returns PacketType.LogHeader.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For a complete list of available packet types, please have a
		///   look at the documentation of the PacketType enum.
		/// </remarks>
		/// -->

		public override PacketType PacketType
		{
			get { return PacketType.LogHeader; }
		}
	}
}
