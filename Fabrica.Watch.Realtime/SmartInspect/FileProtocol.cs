//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   The standard SmartInspect protocol for writing log packets to a log
	///   file.
	/// </summary>
	/// <!--
	/// <remarks>
	///   FileProtocol is the base class for all protocol classes which
	///   deal with log files. By default, it uses the binary log file format
	///   which is compatible to the Console. Derived classes can change this
	///   behavior. For example, for a simple protocol which is capable of
	///   creating plain text files, see the TextProtocol class.
	///
	///   The file protocol supports a variety of options, such as log
	///   rotation (by size and date), encryption and I/O buffers. For a
	///   complete list of available protocol options, please have a look at
	///   the IsValidOption method.
	/// </remarks>
	/// <threadsafety>
	///   The public members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	public class FileProtocol: Protocol
	{
		private const int DEFAULT_BUFFER = 0x2000;
		private const int KEY_SIZE = 16; /* Do not change */
		private const int BLOCK_SIZE = 16; /* Do not change */

		private static byte[] SILE = Encoding.ASCII.GetBytes("SILE"); 
		private static byte[] SILF = Encoding.ASCII.GetBytes("SILF");

		private int fIOBufferCounter;
		private long fFileSize;
		private Stream fStream;
		private Formatter fFormatter;
		private FileRotater fRotater;

		private int fIOBuffer;
		private bool fEncrypt;
		private byte[] fKey;
		private FileRotate fRotate = FileRotate.None;
		private bool fAppend; // = false; /* Init not needed. See FxCop. */
		private string fFileName = "log.sil";
		private long fMaxSize; // = 0; /* Init not needed. See FxCop. */
		private int fMaxParts = 5;

		/// <summary>
		///   Creates and initializes a FileProtocol instance. For a list
		///   of available file protocol options, please refer to the
		///   IsValidOption method.
		/// </summary>

		public FileProtocol()
		{
			this.fRotater = new FileRotater();
			LoadOptions(); /* Set default options */
		}

		/// <summary>
		///   Overridden. Returns "file".
		/// </summary>
		/// <!--
		/// <remarks>
		///   Just "file". Derived classes can change this behavior by
		///   overriding this property.
		/// </remarks>
		/// -->

		protected override string Name
		{
			get { return "file"; }
		}

		/// <summary>
		///  Returns the formatter for this log file protocol.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The standard implementation of this method returns an instance
		///   of the BinaryFormatter class. Derived classes can change this
		///   behavior by overriding this method.
		/// </remarks>
		/// -->

		protected virtual Formatter Formatter
		{
			get 
			{
				if (this.fFormatter == null)
				{
					this.fFormatter = new BinaryFormatter();
				}

				return this.fFormatter;
			}
		}

		/// <summary>
		///   Returns the default filename for this log file protocol.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The standard implementation of this method returns the string
		///   "log.sil" here. Derived classes can change this behavior by
		///   overriding this method.
		/// </remarks>
		/// -->

		protected virtual string DefaultFileName
		{
			get { return "log.sil"; }
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
		///   values and descriptions for the file protocol.
		///   
		///   <table>
		///   Valid Options  Default Value  Description
		///   +              +              +
		///   append         false          Specifies if new packets should be
		///                                   appended to the destination file
		///                                   instead of overwriting the file
		///                                   first.
		///                                   
		///   buffer         0              Specifies the I/O buffer size in
		///                                   kilobytes. It is possible to
		///                                   specify size units like this:
		///                                   "1 MB". Supported units are "KB",
		///                                   "MB" and "GB". A value of 0
		///                                   disables this feature. Enabling
		///                                   the I/O buffering greatly improves
		///                                   the logging performance but has
		///                                   the disadvantage that log packets
		///                                   are temporarily stored in memory
		///                                   and are not immediately written
		///                                   to disk.
		///                                   
		///   encrypt        false          Specifies if the resulting log
		///                                   file should be encrypted. Note
		///                                   that the 'append' option cannot
		///                                   be used with encryption enabled.
		///                                   If encryption is enabled the
		///                                   'append' option has no effect.
		/// 
		///   filename       [varies]       Specifies the filename of the log.
		/// 
		///   key            [empty]        Specifies the secret encryption
		///                                   key as string if the 'encrypt'
		///                                   option is enabled.
		/// 
		///   maxparts       [varies]       Specifies the maximum amount of
		///                                   log files at any given time when
		///                                   log rotating is enabled or the
		///                                   maxsize option is set. Specify
		///                                   0 for no limit. See below for
		///                                   information on the default
		///                                   value for this option.
		/// 
		///   maxsize        0              Specifies the maximum size of a
		///                                   log file in kilobytes. When this
		///                                   size is reached, the current log
		///                                   file is closed and a new file
		///                                   is opened. The maximum amount
		///                                   of log files can be set with
		///                                   the maxparts option. It is
		///                                   possible to specify size units
		///                                   like this: "1 MB". Supported
		///                                   units are "KB", "MB" and "GB".
		///                                   A value of 0 disables this
		///                                   feature.
		///
		///   rotate         none           Specifies the rotate mode for log
		///                                   files. Please see below for a
		///                                   list of available values. A
		///                                   value of "none" disables this
		///                                   feature. The maximum amount
		///                                   of log files can be set with
		///                                   the maxparts option.
		///   </table>
		///   
		///   When using the standard binary log file protocol ("file" in the
		///   <link SmartInspect.Connections, connections string>), the default
		///   filename is set to "log.sil". When using text log files ("text"
		///   in the <link SmartInspect.Connections, connections string>), the
		///   default filename is "log.txt".
		///
		///   The append option specifies if new packets should be appended
		///   to the destination file instead of overwriting the file. The
		///   default value of this option is "false". 
		///
		///   The rotate option specifies the date log rotate mode for this
		///   file protocol. When this option is used, the filename of the
		///   resulting log consists of the value of the filename option and
		///   an appended time stamp (the used time stamp format thereby is
		///   "yyyy-MM-dd-HH-mm-ss"). To avoid problems with daylight saving
		///   time or time zone changes, the time stamp is always in UTC
		///   (Coordinated Universal Time). The following table lists the
		///   available rotate modes together with a short description.
		///   
		///   <table>
		///   Rotate Mode     Description
		///   +               +
		///   None            Rotating is disabled
		///   Hourly          Rotate hourly
		///   Daily           Rotate daily
		///   Weekly          Rotate weekly
		///   Monthly         Rotate monthly
		///   </table>
		///   
		///   As example, if you specify "log.sil" as value for the filename
		///   option and use the Daily rotate mode, the log file is rotated
		///   daily and always has a name of log-yyyy-MM-dd-HH-mm-ss.sil. In
		///   addition to, or instead of, rotating log files by date, you
		///   can also let the file protocol rotate log files by size. To
		///   enable this feature, set the maxsize option to the desired
		///   maximum size. Similar to rotating by date, the resulting log
		///   files include a time stamp. Note that starting with
		///   SmartInspect 3.0, it is supported to combine the maxsize and
		///   rotate options (i.e. use both options at the same time).
		///   
		///   To control the maximum amount of created log files for the
		///   rotate and/or maxsize options, you can use the maxparts option.
		///   The default value for maxparts is 2 when used with the maxsize
		///   option, 0 when used with rotate and 0 when both options,
		///   maxsize and rotate, are used.
		///
		///   SmartInspect log files can be automatically encrypted by
		///   enabling the 'encrypt' option. The used cipher is Rijndael
		///   (AES) with a key size of 128 bit. The secret encryption key
		///   can be specified with the 'key' option. The specified
		///   key is automatically shortened or padded (with zeros) to a
		///   key size of 128 bit. Note that the 'append' option cannot be
		///   used in combination with encryption enabled. If encryption
		///   is enabled the 'append' option has no effect.
		/// 
		///   For further options which affect the behavior of this protocol,
		///   please have a look at the documentation of the
		///   <link Protocol.IsValidOption, IsValidOption> method of the
		///   parent class.
		/// </remarks>
		/// <example>
		/// <code>
		/// SiAuto.Si.Connections = "file()";
		/// SiAuto.Si.Connections = "file(filename=\\"log.sil\\", append=true)";
		/// SiAuto.Si.Connections = "file(filename=\\"log.sil\\")";
		/// SiAuto.Si.Connections = "file(maxsize=\\"16MB\\", maxparts=5)";
		/// SiAuto.Si.Connections = "file(rotate=weekly)";
		/// SiAuto.Si.Connections = "file(encrypt=true, key=\\"secret\\")";
		/// </code>
		/// </example>
		/// -->

		protected override bool IsValidOption(string name)
		{
			return
				name.Equals("append") ||
				name.Equals("buffer") ||
				name.Equals("encrypt") ||
				name.Equals("filename") ||
				name.Equals("key") ||
				name.Equals("maxsize") ||
				name.Equals("maxparts") ||
				name.Equals("rotate") ||
				base.IsValidOption(name);
		}

		/// <summary>
		///   Overridden. Fills a ConnectionsBuilder instance with the
		///   options currently used by this file protocol.
		/// </summary>
		/// <param name="builder">
		///   The ConnectionsBuilder object to fill with the current options
		///   of this protocol.
		/// </param>

		protected override void BuildOptions(ConnectionsBuilder builder)
		{
			base.BuildOptions(builder);
			builder.AddOption("append", this.fAppend);
			builder.AddOption("buffer", (int)this.fIOBuffer / 1024);
			builder.AddOption("filename", this.fFileName);
			builder.AddOption("maxsize", (int)this.fMaxSize / 1024);
			builder.AddOption("maxparts", this.fMaxParts);
			builder.AddOption("rotate", this.fRotate);

			/* Do not add encryption options for security */
		}

		/// <summary>
		///   Overridden. Loads and inspects file specific options.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method loads all relevant options and ensures their
		///   correctness. See IsValidOption for a list of options which
		///   are recognized by the file protocol.
		/// </remarks>
		/// -->

		protected override void LoadOptions()
		{
			base.LoadOptions();

			this.fFileName = GetStringOption("filename", DefaultFileName);
			this.fAppend = GetBooleanOption("append", false);

			this.fIOBuffer = (int) GetSizeOption("buffer", 0);
			this.fRotate = GetRotateOption("rotate", FileRotate.None);
			this.fMaxSize = GetSizeOption("maxsize", 0);

			if (this.fMaxSize > 0 && this.fRotate == FileRotate.None)
			{
				/* Backwards compatibility */
				this.fMaxParts = GetIntegerOption("maxparts", 2);
			}
			else
			{
				this.fMaxParts = GetIntegerOption("maxparts", 0);
			}

			this.fKey = GetBytesOption("key", KEY_SIZE, null);
			this.fEncrypt = GetBooleanOption("encrypt", false);

			if (this.fEncrypt)
			{
				this.fAppend = false; /* Not applicable */
			}

			this.fRotater.Mode = this.fRotate;
		}

		private void ThrowException(string message)
		{
			throw new ProtocolException(message);
		}

		/// <summary>
		///   Overridden. Opens the destination file.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method tries to open the destination file, which can
		///   be specified by passing the "filename" option to the Initialize
		///   method. For other valid options which might affect the
		///   behavior of this method, please see the IsValidOption method.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   Exception              Opening the destination file failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalConnect()
		{
			InternalDoConnect(this.fAppend);
		}

		private void InternalBeforeConnect()
		{
			/* Validate encryption key before connecting */
			if (this.fEncrypt)
			{
				if (this.fKey == null)
				{
					ThrowException("No encryption key");
				}
				else
				{
					if (this.fKey.Length != KEY_SIZE)
					{
						ThrowException("Invalid key size");
					}
				}
			}
		}

		private void InternalDoConnect(bool append)
		{
			InternalBeforeConnect();

			string fileName;

			if (IsRotating())
			{
				fileName = 
					FileHelper.GetFileName(this.fFileName, append);
			}
			else
			{
				fileName = this.fFileName;
			}

			/* Open the destination file */
			this.fStream = new FileStream(
				fileName,
				append ? FileMode.Append : FileMode.Create,
				FileAccess.Write,
				FileShare.Read
			);

			this.fFileSize = new FileInfo(fileName).Length;

			if (this.fEncrypt)
			{
				this.fStream = GetCipher(this.fStream);
			}

			this.fStream = GetStream(this.fStream);
			this.fFileSize = WriteHeader(this.fStream, this.fFileSize);

			if (this.fIOBuffer > 0)
			{
				this.fStream = new BufferedStream(this.fStream, this.fIOBuffer);
				this.fIOBufferCounter = 0; /* Reset buffer counter */
			}
			else
			{
				this.fStream = new BufferedStream(this.fStream, DEFAULT_BUFFER);
			}


			InternalAfterConnect(fileName);
		}

		private void InternalAfterConnect(string fileName)
		{
			if (!IsRotating())
			{
				return; /* Nothing to do */
			}

			if (this.fRotate != FileRotate.None)
			{
				/* We need to initialize our FileRotater object with
				 * the creation time of the opened log file in order
				 * to be able to correctly rotate the log by date in
				 * InternalWritePacket. */

				DateTime fileDate = FileHelper.GetFileDate(
					this.fFileName, fileName);

				this.fRotater.Initialize(fileDate);
			}

			if (this.fMaxParts == 0) /* Unlimited log files */
			{
				return;
			}

			/* Ensure that we have at most 'maxParts' files */
			FileHelper.DeleteFiles(this.fFileName, this.fMaxParts);
		}

		private bool IsRotating()
		{
			return this.fRotate != FileRotate.None || this.fMaxSize > 0;
		}

		private static byte[] GetIVector()
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] data = BitConverter.GetBytes(DateTime.Now.Ticks);
			return md5.ComputeHash(data); /* Always 128 bit */
		}

		private Stream GetCipher(Stream stream)
		{
			byte[] iv = GetIVector();

			/* Prepend the encryption header */
			stream.Write(SILE, 0, SILE.Length);
			stream.Write(iv, 0, iv.Length);
			stream.Flush();

			Rijndael rijndael = Rijndael.Create();
			rijndael.Mode = CipherMode.CBC;
			rijndael.Padding = PaddingMode.PKCS7;
			rijndael.BlockSize = BLOCK_SIZE * 8; /* Given in bits */
			rijndael.KeySize = KEY_SIZE * 8; /* Given in bits */

			/* And then wrap the passed stream */
			return new CryptoStream(
				stream,
				rijndael.CreateEncryptor(this.fKey, iv),
				CryptoStreamMode.Write
			);
		}

		/// <summary>
		///   Intended to provide a wrapper stream for the underlying
		///   file stream.
		/// </summary>
		/// <param name="stream">The underlying file stream.</param>
		/// <returns>The wrapper stream.</returns>
		/// <!--
		/// <remarks>
		///   This method can be used by custom protocol implementers
		///   to wrap the underlying file stream into a filter stream.
		///   Such filter streams include
		///   System.Security.Cryptography.CryptoStream for encrypting
		///   or System.IO.Compression.DeflateStream for compressing log
		///   files, for example.
		///
		///   By default, this method simply returns the passed stream
		///   argument.
		/// </remarks>
		/// -->

		protected virtual Stream GetStream(Stream stream)
		{
			return stream;
		}

		/// <summary>
		///   Intended to write the header of a log file.
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
		///   This default implementation of this method writes the standard
		///   binary protocol header to the supplied Stream instance.
		///   Derived classes may change this behavior by overriding this
		///   method.
		/// </remarks>
		/// -->

		protected virtual long WriteHeader(Stream stream, long size)
		{
			if (size == 0)
			{
				stream.Write(SILF, 0, SILF.Length);
				stream.Flush();
				return SILF.Length;
			}
			else 
			{
				return size;
			}
		}

		/// <summary>
		///   Intended to write the footer of a log file.
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

		protected virtual void WriteFooter(Stream stream)
		{
			
		}

		private void Rotate()
		{
			InternalDisconnect();
			InternalDoConnect(false); /* Always create a new file */
		}

		/// <summary>
		///   Overridden. Writes a packet to the destination file.
		/// </summary>
		/// <param name="packet">The packet to write.</param>
		/// <!--
		/// <remarks>
		///   If the "maxsize" option is set and the supplied packet would
		///   exceed the maximum size of the destination file, then the 
		///   current log file is closed and a new file is opened.
		///   Additionally, if the "rotate" option is active, the log file
		///   is rotated if necessary. Please see the documentation of the
		///   IsValidOption method for more information.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   Exception              Writing the packet to the destination
		///                            file failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalWritePacket(Packet packet)
		{
			Formatter formatter = Formatter;
			int packetSize = formatter.Compile(packet);

			if (this.fRotate != FileRotate.None)
			{
				if (this.fRotater.Update(DateTime.UtcNow))
				{
					Rotate();
				}
			}

			if (this.fMaxSize > 0)
			{
				this.fFileSize += packetSize;
				if (this.fFileSize > this.fMaxSize)
				{
					Rotate();

					if (packetSize > this.fMaxSize)
					{
						return;
					}

					this.fFileSize += packetSize;
				}
			}
				
			formatter.Write(this.fStream);

			if (this.fIOBuffer > 0)
			{
				this.fIOBufferCounter += packetSize;
				if (this.fIOBufferCounter > this.fIOBuffer)
				{
					this.fIOBufferCounter = 0;
					this.fStream.Flush();
				}
			}
			else
			{
				this.fStream.Flush();
			}
		}

		/// <summary>
		///   Overridden. Closes the destination file.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method closes the underlying file handle if previously
		///   created and disposes any supplemental objects.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   Exception              Closing the destination file failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalDisconnect()
		{
			if (this.fStream!= null) 
			{
				WriteFooter(this.fStream);
				this.fStream.Close();
				this.fStream = null;
			}
		}
	}
}
