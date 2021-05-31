//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.IO;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for creating a text representation of a packet and
	///   writing it to a stream.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class creates a text representation of a packet and writes
	///   it to a stream. The representation can be influenced with the
	///   Pattern property. The Compile method preprocesses a packet and
	///   computes the required size of the packet. The Write method writes
	///   the preprocessed packet to the supplied stream.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class TextFormatter: Formatter
	{
		private byte[] fLine;
		private PatternParser fParser;

		/// <summary>
		///   Creates and initializes a TextFormatter instance.
		/// </summary>

		public TextFormatter()
		{
			this.fParser = new PatternParser();	
		}

		/// <summary>
		///   Overridden. Preprocesses (or compiles) a packet and returns the
		///   required size for the compiled result.
		/// </summary>
		/// <param name="packet">The packet to compile.</param>
		/// <returns>The size for the compiled result.</returns>
		/// <!--
		/// <remarks>
		///   This method creates a text representation of the supplied
		///   packet and computes the required size. The resulting
		///   representation can be influenced with the Pattern property.
		///   To write a compiled packet, call the Write method. Please
		///   note that this method only supports LogEntry objects and
		///   ignores any other packet. This means, for packets other
		///   than LogEntry, this method always returns 0.
		/// </remarks>
		/// -->

		public override int Compile(Packet packet)
		{
			if (packet.PacketType == PacketType.LogEntry)
			{
				string line = this.fParser.Expand((LogEntry) packet) + "\r\n";
				this.fLine = Encoding.UTF8.GetBytes(line);
				return this.fLine.Length;
			}
			else 
			{
				this.fLine = null;
				return 0;
			}
		}

		/// <summary>
		///   Overridden. Writes a previously compiled packet to the supplied
		///   stream.
		/// </summary>
		/// <param name="stream">The stream to write the packet to.</param>
		/// <!--
		/// <remarks>
		///   This method writes the previously computed text representation
		///   of a packet (see Compile) to the supplied stream object.
		///   If the return value of the Compile method was 0, nothing is
		///   written.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   IOException             An I/O error occurred while trying
		///                             to write the compiled packet.
		/// </table>
		/// </exception>
		/// -->

		public override void Write(Stream stream)
		{
			if (this.fLine != null)
			{
				stream.Write(this.fLine, 0, this.fLine.Length);
			}
		}

		/// <summary>
		///   Represents the pattern used to create a text representation
		///   of a packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For detailed information of how a pattern string can look like,
		///   please have a look at the documentation of the PatternParser
		///   class, especially the PatternParser.Pattern property.
		/// </remarks>
		/// -->

		public string Pattern
		{
			get { return this.fParser.Pattern; }
			set { this.fParser.Pattern = value; }
		}

		/// <summary>
		///   Indicates if this formatter should automatically intend log
		///   packets like in the Views of the SmartInspect Console.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Log Entry packets of type EnterMethod increase the indentation
		///   and packets of type LeaveMethod decrease it.
		/// </remarks>  
		/// -->

		public bool Indent
		{
			get { return this.fParser.Indent; }
			set { this.fParser.Indent = value; }
		} 
	}
}
