//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents a scheduler action to execute when a protocol is
	///   operating in asynchronous mode. For general information about
	///   the asynchronous mode, please refer to Protocol.IsValidOption.
	/// </summary>

	public enum SchedulerAction
	{
		/// <summary>
		///   Represents a connect protocol operation. This action is
		///   enqueued when the Protocol.Connect method is called and
		///   the protocol is operating in asynchronous mode.
		/// </summary>

		Connect,

		/// <summary>
		///   Represents a write protocol operation. This action is
		///   enqueued when the Protocol.WritePacket method is called
		///   and the protocol is operating in asynchronous mode.
		/// </summary>

		WritePacket,

		/// <summary>
		///   Represents a disconnect protocol operation. This action
		///   is enqueued when the Protocol.Disconnect method is called
		///   and the protocol is operating in asynchronous mode.
		/// </summary>

		Disconnect,

		/// <summary>
		///   Represents a dispatch protocol operation. This action is
		///   enqueued when the Protocol.Dispatch method is called and
		///   the protocol is operating in asynchronous mode.
		/// </summary>

		Dispatch
	}
}
