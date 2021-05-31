//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.IO;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the Control Command packet type which is used for
	///   administrative tasks like resetting or clearing the Console.
	/// </summary>
	/// <!--
	/// <remarks>
	///   A Control Command can be used for several administrative Console
	///   tasks. Among other things, this packet type allows you to
	///   <link Session.ClearAll, reset the Console>.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe. However, instances
	///   of this class will normally only be used in the context of a single
	///   thread.
	/// </threadsafety>
	/// -->

	public sealed class ControlCommand: Packet
	{
		private Stream fData;
		private ControlCommandType fControlCommandType;
		private const int HEADER_SIZE = 8;

		/// <summary>
		///   Overloaded. Creates and initializes a ControlCommand instance.
		/// </summary>

		public ControlCommand()
		{
 
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ControlCommand instance
		///   with a custom control command type.
		/// </summary>
		/// <param name="controlCommandType">
		///   The type of the new Control Command describes the way the
		///   Console interprets this packet. Please see the ControlCommandType
		///   enum for more information.
		/// </param>

		public ControlCommand(ControlCommandType controlCommandType)
		{
			Level = Level.Control;
			this.fControlCommandType = controlCommandType;
		}

		/// <summary>
		///   Overridden. Returns the total occupied memory size of this
		///   Control Command packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The total occupied memory size of this Control Command is
		///   the size of memory occupied the optional Data stream and any
		///   internal data structures of this Control Command.
		/// </remarks>
		/// -->

		public override int Size
		{
			get 
			{
				int result = HEADER_SIZE;

				if (this.fData != null)
				{
					result += (int) this.fData.Length;
				}

				return result;
			}
		}

		/// <summary>
		///   Overridden. Returns PacketType.ControlCommand.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For a complete list of available packet types, please have a
		///   look at the documentation of the PacketType enum.
		/// </remarks>
		/// -->

		public override PacketType PacketType
		{
			get { return PacketType.ControlCommand; }
		}

		/// <summary>
		///   Represents the type of this Control Command.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The type of the Control Command describes the way the Console
		///   interprets this packet. Please see the ControlCommandType enum
		///   for more information.
		/// </remarks>
		/// -->

		public ControlCommandType ControlCommandType
		{
			get { return this.fControlCommandType; }
			set { this.fControlCommandType = value; }
		}

		/// <summary>
		///   Represents the optional data stream of the Control Command.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The property can be null if this Control Command does not
		///   contain additional data.
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
		///   Indicates if this Control Command contains optional data or
		///   not.
		/// </summary>
		/// <remarks>
		///   Returns true if this Control Command packet contains optional
		///   data and false otherwise.
		/// </remarks>

		internal bool HasData
		{
			get { return this.fData != null && this.fData.Length > 0; }
		}
	}
}
