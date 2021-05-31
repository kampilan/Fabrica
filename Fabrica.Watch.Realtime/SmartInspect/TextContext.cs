//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Is the base class for all viewer contexts, which deal with text
	///   data. A viewer context is the library-side representation of a
	///   viewer in the Console.
	/// </summary>
	/// <!--
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class TextContext: ViewerContext
	{
		private static byte[] BOM = new byte[] { 0xEF, 0xBB, 0xBF };
		private StringBuilder fData;

		/// <summary>
		///   Creates and initializes a TextContent instance.
		/// </summary>
		/// <param name="vi">The viewer ID to use.</param>

		public TextContext(ViewerId vi): base(vi)
		{
			this.fData = new StringBuilder();
		}

		/// <summary>
		///   Overridden. Returns the actual data which will be displayed
		///   in the viewer specified by the ViewerId.
		/// </summary>

		public override Stream ViewerData
		{
			get
			{
				// Create stream and write UTF8 bom
				MemoryStream stream = new MemoryStream();
				stream.Write(BOM, 0, BOM.Length);

				// Write text content
				byte[] data = Encoding.UTF8.GetBytes(this.fData.ToString());
				stream.Write(data, 0, data.Length);

				return stream;
			}
		}

		/// <summary>Resets the internal data.</summary>
		/// <!--
		/// <remarks>
		///   This method is intended to reset the internal text data if
		///   custom handling of data is needed by derived classes.
		/// </remarks>
		/// -->

		protected void ResetData()
		{
			this.fData.Length = 0;
		}

		/// <summary>Loads the text from a file.</summary>
		/// <param name="fileName">
		///   The name of the file to load the text from.
		/// </param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The fileName argument is null.
		///   IOException            An I/O error occurred.
		/// </table>
		/// </exception>
		/// -->

		public void LoadFromFile(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			else 
			{
				LoadFromReader(new StreamReader(fileName));
			}
		}

		/// <summary>Loads the text from a stream.</summary>
		/// <param name="stream">
		///   The stream to load the text from.
		/// </param>
		/// <!--
		/// <remarks>
		///   If the supplied stream supports seeking then the entire
		///   stream content will be read and the stream position will be
		///   restored correctly. Otherwise the data will be read from the
		///   current position to the end and the original position can
		///   not be restored.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The stream argument is null.
		///   IOException            An I/O error occurred.
		/// </table>
		/// </exception>
		/// -->

		public void LoadFromStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			long oldPosition = 0;

			if (stream.CanSeek)
			{
				// Save original stream position.
				oldPosition = stream.Position;
				stream.Position = 0;
			}

			try
			{
				LoadFromReader(new StreamReader(stream));
			}
			finally
			{
				if (stream.CanSeek)
				{
					// Restore stream position.
					stream.Position = oldPosition;
				}
			}
		}

		/// <summary>Loads the text from a reader.</summary>
		/// <param name="reader">
		///   The reader to read the text from.
		/// </param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The reader argument is null.
		///   IOException            An I/O error occurred.
		/// </table>
		/// </exception>
		/// -->

		public void LoadFromReader(TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			else 
			{
				ResetData();
				AppendText(reader.ReadToEnd());
			}
		}

		/// <summary>Loads the text.</summary>
		/// <param name="text">The text to load.</param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The text argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void LoadFromText(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			else
			{
				ResetData();
				AppendText(text);
			}
		}

		/// <summary>Appends a line to the text data.</summary>
		/// <param name="line">The line to append.</param>
		/// <!--
		/// <remarks>
		///   This method appends the supplied line and a carriage return
		///   + linefeed character to the internal text data after it has
		///   been escaped by the EscapeLine method.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The line argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AppendLine(string line)
		{
			if (line == null)
			{
				throw new ArgumentNullException("line");
			}
			else 
			{
				this.fData.Append(EscapeLine(line));
				this.fData.Append("\r\n");
			}
		}

		/// <summary>Appends text.</summary>
		/// <param name="text">The text to append.</param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The text argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AppendText(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			else
			{
				this.fData.Append(text);
			}
		}

		/// <summary>Escapes a line.</summary>
		/// <param name="line">The line to escape.</param>
		/// <returns>The escaped line.</returns>
		/// <!--
		/// <remarks>
		///   If overridden in derived classes, this method escapes a
		///   line depending on the viewer format used. The default
		///   implementation does no escaping.
		/// </remarks>
		/// -->

		protected virtual string EscapeLine(string line)
		{
			// The default implementation does no escaping.
			return line;
		}

		/// <summary>
		///   Overridden. Releases any resources.
		/// </summary>
		/// <param name="disposing">
		///   True if managed resources should be released and false
		///   otherwise.
		/// </param>

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.fData = null;
			}
		}
	}
}
