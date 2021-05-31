//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the SmartInspect.ControlCommand event.
	/// </summary>
	/// <!--
	/// <remarks>
	///   It has only one public class member named ControlCommand. This
	///   member is a property, which just returns the sent packet.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class ControlCommandEventArgs: System.EventArgs
	{
		private ControlCommand fControlCommand;

		/// <summary>
		///   Creates and initializes a ControlCommandEvent args instance.
		/// </summary>
		/// <param name="controlCommand">
		///   The Control Command packet which caused the event.
		/// </param>

		public ControlCommandEventArgs(ControlCommand controlCommand)
		{
			this.fControlCommand = controlCommand;
		}

		/// <summary>
		///   This read-only property returns the ControlCommand packet,
		///   which has just been sent.
		/// </summary>

		public ControlCommand ControlCommand
		{
			get { return this.fControlCommand; }
		}
	}
}
