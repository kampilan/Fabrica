//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the type of a packet. In the SmartInspect concept,
	///   there are multiple packet types each serving a special purpose.
	///   For a good starting point on packets, please have a look at the
	///   documentation of the Packet class.
	/// </summary>

	public enum PacketType
	{
		/// <summary>
		///   Identifies a packet as Log Entry. Please have a look at the
		///   documentation of the LogEntry class for information about
		///   this packet type.
		/// </summary>

		LogEntry = 4,

		/// <summary>
		///   Identifies a packet as Control Command. Please have a look
		///   at the documentation of the ControlCommand class for more
		///   information about this packet type.
		/// </summary>

		ControlCommand = 1,

		/// <summary>
		///   Identifies a packet as Watch. Please have a look at the
		///   documentation of the Watch class for information about
		///   this packet type.
		/// </summary>

		Watch = 5,

		/// <summary>
		///   Identifies a packet as Process Flow entry. Please have a
		///   look at the documentation of the ProcessFlow class for
		///   information about this packet type.
		/// </summary>

		ProcessFlow = 6,

		/// <summary>
		///   Identifies a packet as Log Header. Please have a look
		///   at the documentation of the LogHeader class for information
		///   about this packet type.
		/// </summary>

		LogHeader = 7
	}
}
