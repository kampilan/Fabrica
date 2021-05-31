//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.IO;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used for writing log data to memory and saving it to a stream
	///   or another protocol object on request.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used for writing log data to memory. On request
	///   this data can be saved to a stream or to another protocol object.
	///   To initiate such a request, use the InternalDispatch method.
	/// 
	///   This class is used when the 'mem' protocol is specified in the
	///   <link SmartInspect.Connections, connections string>. Please see
	///   the IsValidOption method for a list of available options for
	///   this protocol.
	/// </remarks>
	/// <threadsafety>
	///   The public members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	public class MemoryProtocol: Protocol
	{
		private static byte[] BOM = new byte[] { 0xEF, 0xBB, 0xBF };
		private static byte[] HEADER = Encoding.ASCII.GetBytes("SILF");
		private const bool DEFAULT_INDENT = false;
		private const string DEFAULT_PATTERN =
			"[%timestamp%] %level%: %title%";

		private bool fIndent;
		private Formatter fFormatter;
		private PacketQueue fQueue;
		private long fMaxSize;
		private bool fAsText;
		private string fPattern;

		/// <summary>
		///   Creates and initializes a MemoryProtocol instance. For a list
		///   of available memory protocol options, please refer to the
		///   IsValidOption method.
		/// </summary>
 
		public MemoryProtocol()
		{
			LoadOptions(); // Set default options
		}

		/// <summary>
		///   Overridden. Fills a ConnectionsBuilder instance with the
		///   options currently used by this memory protocol.
		/// </summary>
		/// <param name="builder">
		///   The ConnectionsBuilder object to fill with the current options
		///   of this protocol.
		/// </param>

		protected override void BuildOptions(ConnectionsBuilder builder)
		{
			base.BuildOptions(builder);
			builder.AddOption("maxsize", (int) this.fMaxSize / 1024);
			builder.AddOption("astext", this.fAsText);
			builder.AddOption("indent", this.fIndent);
			builder.AddOption("pattern", this.fPattern);
		}

		/// <summary>
		///   Overridden. Validates if a protocol option is supported.
		/// </summary>
		/// <param name="name">The option name to validate.</param>
		/// <returns>
		///   True if the option is supported and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   The following table lists all valid options, their default
		///   values and descriptions for this memory protocol. For a
		///   list of options common to all protocols, please have a look
		///   at the <link Protocol.IsValidOption, IsValidOption> method
		///   of the parent class.
		///
		///   <table>
		///   Valid Options  Default Value                     Description
		///   +              +                                 +
		///   astext         false                             Specifies if
		///                                                     logging data
		///                                                     should be
		///                                                     written as
		///                                                     text instead
		///                                                     of binary.
		///                                                     
		///   indent         false                             Indicates if
		///                                                     the logging
		///                                                     output should
		///                                                     automatically
		///                                                     be indented
		///                                                     like in the
		///                                                     Console if
		///                                                     'astext' is
		///                                                     set to true.
		/// 
		///   maxsize        2048                              Specifies the
		///                                                     maximum size
		///                                                     of the packet
		///                                                     queue of this
		///                                                     protocol in
		///                                                     kilobytes.
		///                                                     Specify size
		///                                                     units like
		///                                                     this: "1 MB".
		///                                                     Supported 
		///                                                     units are
		///                                                     "KB", "MB" and
		///                                                     "GB".
		/// 
		///   pattern        "[%timestamp%] %level%: %title%"  Specifies the
		///                                                     pattern used
		///                                                     to create a
		///                                                     text
		///                                                     representation
		///                                                     of a packet.
		///   </table>
		/// 
		///   If the "astext" option is used for creating a textual output
		///   instead of the default binary, the "pattern" string specifies
		///   the textual representation of a log packet. For detailed
		///   information of how a pattern string can look like, please
		///   have a look at the documentation of the PatternParser class,
		///   especially the PatternParser.Pattern property.
		/// </remarks>
		/// <example>
		/// <code>
		/// SiAuto.Si.Connections = "mem()";
		/// SiAuto.Si.Connections = "mem(maxsize=\\"8MB\\")";
		/// SiAuto.Si.Connections = "mem(astext=true)";
		/// </code>
		/// </example>
		/// -->

		protected override bool IsValidOption(string name)
		{
			return 
				name.Equals("maxsize") ||
				name.Equals("astext") ||
				name.Equals("pattern") ||
				name.Equals("indent") ||
				base.IsValidOption(name);
		}

		/// <summary>
		///   Overridden. Returns "mem".
		/// </summary>
		/// <!--
		/// <remarks>
		///   Just "mem". Derived classes can change this behavior by
		///   overriding this property.
		/// </remarks>
		/// -->
		
		protected override string Name
		{
			get { return "mem"; }
		}

		private void InitializeFormatter()
		{
			if (this.fAsText)
			{
				this.fFormatter = new TextFormatter();
				((TextFormatter) this.fFormatter).Pattern = this.fPattern;
				((TextFormatter) this.fFormatter).Indent = this.fIndent;
			}
			else
			{
				this.fFormatter = new BinaryFormatter();
			}
		}

		/// <summary>
		///   Overridden. Loads and inspects memory specific options.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method loads all relevant options and ensures their
		///   correctness. See IsValidOption for a list of options which
		///   are recognized by the memory protocol.
		/// </remarks>
		/// -->

		protected override void LoadOptions()
		{
			base.LoadOptions();
			this.fMaxSize = GetSizeOption("maxsize", 2048);
			this.fAsText = GetBooleanOption("astext", false);
			this.fIndent = GetBooleanOption("indent", DEFAULT_INDENT);
			this.fPattern = GetStringOption("pattern", DEFAULT_PATTERN);
			InitializeFormatter();
		}

		/// <summary>
		///   Overridden. Creates and initializes the packet queue.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method creates and initializes a new packet queue with
		///   a maximum size as specified by the Initialize method. For
		///   other valid options which might affect the behavior of this
		///   method and protocol, please see the IsValidOption method.
		/// </remarks>
		/// -->

		protected override void InternalConnect()
		{
			this.fQueue = new PacketQueue();
			this.fQueue.Backlog = this.fMaxSize;
		}

		/// <summary>
		///   Overridden. Writes a packet to the packet queue.
		/// </summary>
		/// <param name="packet">The packet to write.</param>
		/// <!--
		/// <remarks>
		///   This method writes the supplied packet to the internal
		///   queue of packets. If the size of the queue exceeds the
		///   maximum size as specified by the Options property, the
		///   queue is automatically resized and older packets are
		///   discarded.
		/// </remarks>
		/// -->

		protected override void InternalWritePacket(Packet packet)
		{
			this.fQueue.Push(packet);
		}

		/// <summary>
		///   Overridden. Implements a custom action for saving the current
		///   queue of packets of this memory protocol to a stream or
		///   protocol object.
		/// </summary>
		/// <param name="command">
		///   The protocol command which is expected to provide the stream
		///   or protocol object.
		/// </param>
		/// <seealso cref="Protocol.Dispatch"/>
		/// <seealso cref="SmartInspect.Dispatch"/>
		/// <!--
		/// <remarks>
		///   Depending on the supplied command argument, this method does
		///   the following.
		/// 
		///   If the supplied State object of the protocol command is of
		///   type Stream, then this method uses this stream to write the
		///   entire content of the internal queue of packets. The necessary
		///   header is written first and then the actual packets are
		///   appended.
		/// 
		///   The header and packet output format can be influenced with
		///   the "astext" protocol option (see IsValidOption). If the
		///   "astext" option is true, the header is a UTF8 Byte Order
		///   Mark and the packets are written in plain text format. If
		///   the "astext" option is false, the header is the standard
		///   header for SmartInspect log files and the packets are
		///   written in the default binary mode. In the latter case, the
		///   resulting log files can be loaded by the SmartInspect
		///   Console.
		///   
		///   If the supplied State object of the protocol command is of
		///   type Protocol instead, then this method uses this protocol
		///   object to call its WritePacket method for each packet in the
		///   internal packet queue.
		/// 
		///   The Action property of the command argument should currently
		///   always be set to 0. If the State object is not a stream or
		///   protocol command or if the command argument is null, then
		///   this method does nothing.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type      Condition
		///   +                   +
		///   Exception           Writing the internal queue of packets
		///                         to the supplied stream or protocol
		///                         failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalDispatch(ProtocolCommand command)
		{
			if (command == null)
			{
				return;
			}

			// Check if the supplied object is a stream
			Stream stream = command.State as Stream;

			if (stream != null)
			{
				FlushToStream(stream);
			}
			else
			{
				// Check if the supplied object is a protocol
				Protocol protocol = command.State as Protocol;

				if (protocol != null)
				{
					FlushToProtocol(protocol);
				}
			}
		}

		private void FlushToStream(Stream stream)
		{
			// Write the necessary file header
			if (this.fAsText)
			{
				stream.Write(BOM, 0, BOM.Length);
			}
			else
			{
				stream.Write(HEADER, 0, HEADER.Length);
			}

			// Write the current content of our queue
			Packet packet = this.fQueue.Pop();
			while (packet != null)
			{
				this.fFormatter.Format(packet, stream);
				packet = this.fQueue.Pop();
			}
		}

		private void FlushToProtocol(Protocol protocol)
		{
			// Write the current content of our queue
			Packet packet = this.fQueue.Pop();
			while (packet != null)
			{
				protocol.WritePacket(packet);
				packet = this.fQueue.Pop();
			}
		}

		/// <summary>
		///   Overridden. Clears the internal queue of packets.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method does nothing more than to clear the internal
		///   queue of packets. After this method has been called, the
		///   InternalDispatch method writes an empty log unless new
		///   packets are queued in the meantime.
		/// </remarks>
		/// -->

		protected override void InternalDisconnect()
		{
			if (this.fQueue != null)
			{
				this.fQueue.Clear();
				this.fQueue = null;
			}
		}
	}
}
