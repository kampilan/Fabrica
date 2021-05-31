//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.IO;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used for writing customizable plain text log files.
	/// </summary>
	/// <!--
	/// <remarks>
	///   TextProtocol is used for writing plain text log files. This
	///   class is used when the 'text' protocol is specified in the
	///   <link SmartInspect.Connections, connections string>. See the
	///   IsValidOption method for a list of available protocol options.
	/// </remarks>
	/// <threadsafety>
	///   The public members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	public class TextProtocol: FileProtocol
	{
		private static byte[] HEADER = new byte[] {0xEF, 0xBB, 0xBF};
		private const bool DEFAULT_INDENT = false;
		private const string DEFAULT_PATTERN = 
			"[%timestamp%] %level%: %title%";

		private bool fIndent;
		private string fPattern;
		private Formatter fFormatter;

		/// <summary>
		///   Overridden. Returns the formatter for this log file protocol.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The standard implementation of this method returns an instance
		///   of the TextFormatter class. Derived classes can change this
		///   behavior by overriding this method.
		/// </remarks>
		/// -->

		protected override Formatter Formatter
		{
			get
			{
				if (this.fFormatter == null)
				{
					this.fFormatter = new TextFormatter();
				}

				return this.fFormatter;
			}
		}

		/// <summary>
		///   Overridden. Returns the default filename for this log file
		///   protocol.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The standard implementation of this method returns the string
		///   "log.txt" here. Derived classes can change this behavior by
		///   overriding this method.
		/// </remarks>
		/// -->

		protected override string DefaultFileName
		{
			get { return "log.txt"; }
		}

		/// <summary>
		///   Overridden. Returns "text".
		/// </summary>
		/// <!--
		/// <remarks>
		///   Just "text". Derived classes can change this behavior by
		///   overriding this property.
		/// </remarks>
		/// -->

		protected override string Name
		{
			get { return "text"; }
		}

		/// <summary>
		///   Overridden. Intended to write the header of a log file.
		/// </summary>
		/// <param name="stream">
		///   The stream to which the header should be written to.
		/// </param>
		/// <param name="size">
		///   Specifies the current size of the supplied stream.
		/// </param>
		/// <returns>
		///   The new size of the stream after writing the header. If no
		///   header is written, the supplied size argument is returned.
		/// </returns>
		/// <!--
		/// <remarks>
		///   The implementation of this method writes the standard UTF8
		///   BOM (byte order mark) to the supplied stream in order to
		///   identify the log file as text file in UTF8 encoding. Derived
		///   classes may change this behavior by overriding this method.
		/// </remarks>
		/// -->

		protected override long WriteHeader(Stream stream, long size)
		{
			if (size == 0)
			{
				stream.Write(HEADER, 0, HEADER.Length);
				stream.Flush();
				return HEADER.Length;
			}
			else
			{
				return size;
			}
		}

		/// <summary>
		///   Overridden. Intended to write the footer of a log file.
		/// </summary>
		/// <param name="stream">
		///   The stream to which the footer should be written to.
		/// </param>
		/// <!--
		/// <remarks>
		///   The implementation of this method does nothing. Derived
		///   class may change this behavior by overriding this method.
		/// </remarks>
		/// -->

		protected override void WriteFooter(Stream stream)
		{

		}

		/// <summary>
		///   Overridden. Fills a ConnectionsBuilder instance with the
		///   options currently used by this text protocol.
		/// </summary>
		/// <param name="builder">
		///   The ConnectionsBuilder object to fill with the current options
		///   of this protocol.
		/// </param>

		protected override void BuildOptions(ConnectionsBuilder builder)
		{
			base.BuildOptions(builder);
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
		///   values and descriptions for this text file protocol. For a
		///   list of options common to all file protocols, please have a
		///   look at the <link FileProtocol.IsValidOption, IsValidOption>
		///   method of the parent class. Please note that this text
		///   protocol <b>does not support log file encryption</b>.
		///
		///   <table>
		///   Valid Options  Default Value                     Description
		///   +              +                                 +
		///   indent         false                             Indicates if
		///                                                     the logging
		///                                                     output should
		///                                                     automatically
		///                                                     be indented
		///                                                     like in the
		///                                                     Console.
		///   
		///   pattern        "[%timestamp%] %level%: %title%"  Specifies the
		///                                                     pattern used
		///                                                     to create a
		///                                                     text
		///                                                     representation
		///                                                     of a packet.
		///   </table>
		///
		///   For detailed information of how a pattern string can look like,
		///   please have a look at the documentation of the PatternParser
		///   class, especially the PatternParser.Pattern property.
		/// </remarks>
		/// <example>
		/// <code>
		/// SiAuto.Si.Connections = "text()";
		/// SiAuto.Si.Connections = "text(filename=\\"log.txt\\", append=true)";
		/// SiAuto.Si.Connections = "text(filename=\\"log.txt\\")";
		/// SiAuto.Si.Connections = "text(maxsize=\\"16MB\\")";
		/// </code>
		/// </example>
		/// -->

		protected override bool IsValidOption(string name)
		{
			if (name.Equals("encrypt") || name.Equals("key"))
			{
				return false;
			}
			else
			{
				return
					name.Equals("pattern") ||
					name.Equals("indent") ||
					base.IsValidOption(name);
			}
		}

		/// <summary>
		///   Overridden. Loads and inspects file specific options.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method loads all relevant options and ensures their
		///   correctness. See IsValidOption for a list of options which
		///   are recognized by the text protocol.
		/// </remarks>
		/// -->

		protected override void LoadOptions()
		{
			base.LoadOptions();
			this.fPattern = GetStringOption("pattern", DEFAULT_PATTERN);
			this.fIndent = GetBooleanOption("indent", DEFAULT_INDENT);
			((TextFormatter) Formatter).Pattern = this.fPattern;
			((TextFormatter) Formatter).Indent = this.fIndent;
		}
	}
}
