//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This is the base class for all viewer contexts, which deal with
	///   binary data. A viewer context is the library-side representation
	///   of a viewer in the Console.
	/// </summary>
	/// <!--
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class BinaryContext: ViewerContext
	{
		private MemoryStream fData;

		/// <summary>
		///   Creates and initializes a BinaryContext instance.
		/// </summary>
		/// <param name="vi">The viewer ID to use.</param>

		public BinaryContext(ViewerId vi): base(vi)
		{
			this.fData = new MemoryStream();
		}

		/// <summary>
		///   Overridden. Returns the actual binary data which will be
		///   displayed in the viewer specified by the ViewerId.
		/// </summary>

		public override Stream ViewerData
		{
			get { return this.fData; }
		}

		/// <summary>Resets the internal data stream.</summary>
		/// <!--
		/// <remarks>
		///   This method is intended to reset the internal data stream
		///   if custom handling of data is needed by derived classes.
		/// </remarks>
		/// -->

		protected void ResetData()
		{
			this.fData.SetLength(0);
		}

		/// <summary>Loads the binary data from a file.</summary>
		/// <param name="fileName">
		///   The name of the file to load the binary data from.
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
				using (FileStream fs = File.OpenRead(fileName))
				{
					InternalLoadFromStream(fs);
				}
			}
		}

		private void InternalLoadFromStream(Stream stream)
		{
			int n;
			byte[] b = new byte[0x2000];						

			ResetData();
			while ( (n = stream.Read(b, 0, b.Length)) > 0)
			{
				this.fData.Write(b, 0, n);
			}
		}

		/// <summary>
		///   Loads the binary data from a stream.
		/// </summary>
		/// <param name="stream">
		///   The stream to load the binary data from.
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
				InternalLoadFromStream(stream);
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

		/// <summary>
		///   Overloaded. Appends a buffer.
		/// </summary>
		/// <param name="buffer">The buffer to append.</param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type               Condition
		///   +                            +
		///   ArgumentNullException        The buffer argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AppendBytes(byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			else 
			{
				this.fData.Write(buffer, 0, buffer.Length);
			}
		}

		/// <summary>
		///   Overloaded. Appends a buffer. Lets you specify the offset in
		///   the buffer and the amount of bytes to append.
		/// </summary>
		/// <param name="buffer">The buffer to append.</param>
		/// <param name="offset">
		///   The offset at which to begin appending.
		/// </param>
		/// <param name="count">The number of bytes to append.</param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type               Condition
		///   +                            +
		///   ArgumentException            The sum of the offset and count
		///                                  parameters is greater than the
		///                                  actual buffer length.
		///                                  
		///   ArgumentNullException        The buffer argument is null.
		///   ArgumentOutOfRangeException  The offset or count parameter is
		///                                  negative.
		/// </table>
		/// </exception>
		/// -->

		public void AppendBytes(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			else
			{
				this.fData.Write(buffer, offset, count);
			}
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
				if (this.fData != null)
				{
					this.fData.Close();
					this.fData = null;
				}
			}
		}
	}
}
