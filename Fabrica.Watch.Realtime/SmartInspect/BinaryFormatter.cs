//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;
using System.Text;
using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for formatting and writing a packet in the standard
	///   SmartInspect binary format.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class formats and writes a packet in the standard binary
	///   format which can be read by the SmartInspect Console. The
	///   Compile method preprocesses a packet and computes the required
	///   size of the packet. The Write method writes the preprocessed
	///   packet to the supplied stream.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class BinaryFormatter: Formatter
	{
		private const long TICKS = 621355968000000000L;
		private const long MICROSECONDS_PER_DAY = 86400000000;
		private const int DAY_OFFSET = 25569;
		private const int MAX_STREAM_CAPACITY = 1 * 1024 * 1024;

		private int fSize;
		private byte[] fBuffer;
		private Stream fStream;
		private Packet fPacket;

		/// <summary>
		///   Creates and initializes a BinaryFormatter instance.
		/// </summary>

		public BinaryFormatter()
		{
			this.fBuffer = new byte[0x2000];
			this.fStream = new MemoryStream();
		}

		private void ResetStream()
		{
			if (this.fSize > MAX_STREAM_CAPACITY)
			{
				// Reset the stream capacity if the previous packet
				// was very big. This ensures that the amount of memory
				// can shrink again after a big packet has been sent.
				this.fStream = new MemoryStream();
			}
			else
			{
				// Only reset the position. This ensures a very good
				// performance since no reallocations are necessary.
				this.fStream.Position = 0;
			}
		}

		/// <summary>
		///   Overridden. Preprocesses (or compiles) a packet and returns the
		///   required size for the compiled result.
		/// </summary>
		/// <param name="packet">The packet to compile.</param>
		/// <returns>The size for the compiled result.</returns>
		/// <!--
		/// <remarks>
		///   This method preprocesses the supplied packet and computes the
		///   required binary format size. To write this compiled packet,
		///   call the Write method.
		/// </remarks>
		/// -->

		public override int Compile(Packet packet)
		{
			ResetStream();
			this.fPacket = packet;

			switch (this.fPacket.PacketType)
			{
				case PacketType.LogEntry: 
					CompileLogEntry();
					break;
				case PacketType.LogHeader:
					CompileLogHeader();
					break;
				case PacketType.Watch:
					CompileWatch();
					break;
				case PacketType.ControlCommand:
					CompileControlCommand();
					break;
				case PacketType.ProcessFlow:
					CompileProcessFlow();
					break;
			}

			this.fSize = (int) this.fStream.Position;
			return this.fSize + Packet.PACKET_HEADER;
		}

		private void WriteLength(byte[] value)
		{
			if (value != null)
			{
				WriteInt((int) value.Length);
			}
			else 
			{
				WriteInt((int) 0);
			}
		}

		private void WriteLength(Stream value)
		{
			if (value != null)
			{
				WriteInt((int) value.Length);
			}
			else 
			{
				WriteInt((int) 0);
			}
		}

		private void WriteTimestamp(DateTime value)
		{
			long us;
			double timestamp;

			// Calculate current Timestamp:
			// A Timestamp is represented by a double. The integral
			// part of the from is the number of days that have
			// passed since 12/30/1899. The fractional part of the
			// from is the fraction of a 24 hour day that has elapsed.

			us = (value.Ticks - TICKS) / 10L;
			timestamp = us / MICROSECONDS_PER_DAY + DAY_OFFSET;
			timestamp += (double) (us % MICROSECONDS_PER_DAY) / 
				MICROSECONDS_PER_DAY;

			WriteDouble(timestamp);
		}

		private void CopyStream(Stream to, Stream from, int count)
		{
			from.Position = 0;

			while (count > 0)
			{
				int toRead;

				if (count > this.fBuffer.Length)
				{
					toRead = this.fBuffer.Length;
				}
				else 
				{
					toRead = count;
				}

				int n = from.Read(this.fBuffer, 0, toRead);

				if (n > 0)
				{
					to.Write(this.fBuffer, 0, n);
					count -= n;
				}
				else 
				{
					break;
				}
			}
		}

		private void WriteColor(Color value)
		{
			int color = value.R | 
				value.G << 8 | 
				value.B << 16 | 
				value.A << 24;

			WriteInt(color);
		}

		private void WriteShort(short value)
		{
			WriteShort(this.fStream, value);
		}

		private void WriteShort(Stream stream, short value)
		{
			this.fBuffer[0] = (byte) value;
			this.fBuffer[1] = (byte) (value >> 8);
			stream.Write(this.fBuffer, 0, 2);
		}

		private void WriteInt(int value)
		{
			WriteInt(this.fStream, value);
		}

		private void WriteInt(Stream stream, int value)
		{
			this.fBuffer[0] = (byte) value;
			this.fBuffer[1] = (byte) (value >> 8);
			this.fBuffer[2] = (byte) (value >> 16);
			this.fBuffer[3] = (byte) (value >> 24);
			stream.Write(this.fBuffer, 0, 4);
		}

		private void WriteLong(long value)
		{
			this.fBuffer[0] = (byte) value;
			this.fBuffer[1] = (byte) (value >> 8);
			this.fBuffer[2] = (byte) (value >> 16);
			this.fBuffer[3] = (byte) (value >> 24);
			this.fBuffer[4] = (byte) (value >> 32);
			this.fBuffer[5] = (byte) (value >> 40);
			this.fBuffer[6] = (byte) (value >> 48);
			this.fBuffer[7] = (byte) (value >> 56);
			this.fStream.Write(this.fBuffer, 0, 8);
		}

		private void WriteDouble(double value)
		{
			WriteLong(BitConverter.DoubleToInt64Bits(value));
		}

		private byte[] EncodeString(string value)
		{
			if (value != null)
			{
				return Encoding.UTF8.GetBytes(value);		
			}
			else 
			{
				return null;
			}
		}

		private void WriteData(byte[] value)
		{
			if (value != null)
			{
				this.fStream.Write(value, 0, value.Length);
			}
		}

		private void CompileControlCommand()
		{
			ControlCommand controlCommand = 
				(ControlCommand) this.fPacket;

			WriteInt((int) controlCommand.ControlCommandType);
			WriteLength(controlCommand.Data);

			if (controlCommand.HasData)
			{
				CopyStream(this.fStream, controlCommand.Data, 
					(int) controlCommand.Data.Length);
			}
		}

		private void CompileLogHeader()
		{
			LogHeader logHeader = (LogHeader) this.fPacket;
			byte[] content = EncodeString(logHeader.Content);
			WriteLength(content);
			WriteData(content);
		}

		private void CompileLogEntry()
		{
			LogEntry logEntry = (LogEntry) this.fPacket;

			byte[] appName = EncodeString(logEntry.AppName);
			byte[] sessionName = EncodeString(logEntry.SessionName);
			byte[] title = EncodeString(logEntry.Title);
			byte[] hostName = EncodeString(logEntry.HostName);

			WriteInt((int) logEntry.LogEntryType);
			WriteInt((int) logEntry.ViewerId);
			WriteLength(appName);
			WriteLength(sessionName);
			WriteLength(title);
			WriteLength(hostName);
			WriteLength(logEntry.Data);
			WriteInt(logEntry.ProcessId);
			WriteInt(logEntry.ThreadId);
			WriteTimestamp(logEntry.Timestamp);
			WriteColor(logEntry.Color);

			WriteData(appName);
			WriteData(sessionName);
			WriteData(title);
			WriteData(hostName);

			if (logEntry.HasData)
			{
				CopyStream(this.fStream, logEntry.Data, 
					(int) logEntry.Data.Length);
			}
		}

		private void CompileProcessFlow()
		{
			ProcessFlow processFlow = (ProcessFlow) this.fPacket;

			byte[] title = EncodeString(processFlow.Title);
			byte[] hostName = EncodeString(processFlow.HostName);

			WriteInt((int) processFlow.ProcessFlowType);
			WriteLength(title);
			WriteLength(hostName);
			WriteInt(processFlow.ProcessId);
			WriteInt(processFlow.ThreadId);
			WriteTimestamp(processFlow.Timestamp);

			WriteData(title);
			WriteData(hostName);
		}

		private void CompileWatch()
		{
			Watch watch = (Watch) this.fPacket;

			byte[] name = EncodeString(watch.Name);
			byte[] value = EncodeString(watch.Value);

			WriteLength(name);
			WriteLength(value);
			WriteInt((int) watch.WatchType);
			WriteTimestamp(watch.Timestamp);

			WriteData(name);
			WriteData(value);
		}

		/// <summary>
		///   Overridden. Writes a previously compiled packet to the supplied
		///   stream.
		/// </summary>
		/// <param name="stream">The stream to write the packet to.</param>
		/// <!--
		/// <remarks>
		///   This method writes the previously compiled packet (see Compile)
		///   to the supplied stream object. If the return value of the
		///   Compile method was 0, nothing is written.
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
			if (this.fSize > 0)
			{
				WriteShort(stream, (short) this.fPacket.PacketType);
				WriteInt(stream, this.fSize);
				CopyStream(stream, this.fStream, this.fSize);
			}
		}
	}
}
