//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the SmartInspect.Filter event.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class consists of only two class members. At first we
	///   have the Packet property, which returns the packet which
	///   caused the event.
	///   
	///   Then there is the Cancel property which can be used to cancel
	///   the processing of certain packets. For more information, please
	///   refer to the SmartInspect.Filter documentation.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class FilterEventArgs
	{
		private bool fCancel;
		private Packet fPacket;

		/// <summary>
		///   Creates and initializes a FilterEventArgs instance.
		/// </summary>
		/// <param name="packet">
		///   The packet which caused the event.
		/// </param>

		public FilterEventArgs(Packet packet)
		{
			this.fCancel = false;
			this.fPacket = packet;
		}

		/// <summary>
		///   This read-only property returns the packet, which caused
		///   the event.
		/// </summary>

		public Packet Packet
		{
			get { return this.fPacket; }
		}

		/// <summary>
		///   This property can be used to cancel the processing of certain
		///   packets during the SmartInspect.Filter event.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For more information on how to use this property, please refer
		///   to the SmartInspect.Filter documentation.
		/// </remarks>
		/// -->

		public bool Cancel
		{
			get { return this.fCancel; }
			set { this.fCancel = value; }
		}
	}
}
