//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the Watch packet type which is used in the Watch
	///   methods in the Session classes.
	/// </summary>
	/// <!--
	/// <remarks>
	///   A Watch is responsible for sending variables and their values
	///   to the Console. These key/value pairs will be displayed in the
	///   Watches toolbox. If a Watch with the same name is sent twice,
	///   the old value is overwritten and the Watches toolbox displays
	///   the most current value.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe. However, instances
	///   of this class will normally only be used in the context of a
	///   single thread.
	/// </threadsafety>
	/// -->

	public sealed class Watch: Packet
	{
		private WatchType fWatchType;
		private string fName;
		private string fValue;
		private DateTime fTimestamp;
		private const int HEADER_SIZE = 20;

		/// <summary>
		///   Overloaded. Creates and initializes a Watch instance.
		/// </summary>

		public Watch()
		{
 
		}

		/// <summary>
		///   Overloaded. Creates and initializes a Watch instance with a
		///   custom watch type.
		/// </summary>
		/// <param name="watchType">
		///   The type of the new Watch describes the variable type (String,
		///   Integer and so on). Please see the WatchType enum for more
		///   information.
		/// </param>

		public Watch(WatchType watchType)
		{
			this.fWatchType = watchType;
		}

		/// <summary>
		///   Overridden. Returns the total occupied memory size of this Watch
		///   packet.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The total occupied memory size of this Watch is the size of
		///   memory occupied by all strings and any internal data structures
		///   of this Watch.
		/// </remarks>
		/// -->

		public override int Size
		{
			get 
			{ 
				return HEADER_SIZE +
					GetStringSize(this.fName) +
					GetStringSize(this.fValue);
			}
		}

		/// <summary>
		///   Overridden. Returns PacketType.Watch.
		/// </summary>
		/// <!--
		/// <remarks>
		///   For a complete list of available packet types, please have a
		///   look at the documentation of the PacketType enum.
		/// </remarks>
		/// -->

		public override PacketType PacketType
		{
			get { return PacketType.Watch; }
		}

		/// <summary>
		///   Represents the name of this Watch.
		/// </summary>
		/// <!--
		/// <remarks>
		///   If a Watch with the same name is sent twice, the old value is
		///   overwritten and the Watches toolbox displays the most current
		///   value. The name of this Watch will be empty in the SmartInspect
		///   Console when this property is set to null.
		/// </remarks>
		/// -->

		public string Name
		{
			get { return this.fName; }
			set { this.fName = value; }
		}

		/// <summary>
		///   Represents the value of this Watch.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The value of a Watch is always sent as String. To view the
		///   type of this variable Watch, please have a look at the
		///   WatchType property. The value of this Watch will be empty in
		///   the SmartInspect Console when this property is set to null.
		/// </remarks>
		/// -->

		public string Value
		{
			get { return this.fValue; }
			set { this.fValue = value; }
		}

		/// <summary>
		///   Represents the type of this Watch.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The type of this Watch describes the variable type (String,
		///   Integer and so on). Please see the WatchType enum for more
		///   information.
		/// </remarks>
		/// -->

		public WatchType WatchType
		{
			get { return this.fWatchType; }
			set { this.fWatchType = value; }
		}

		/// <summary>
		///   Represents the timestamp of this Watch object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property returns the creation time of this Watch
		///   object. If specified, the SmartInspect .NET library tries
		///   to use high-resolution timestamps. Please see the
		///   SmartInspect.Resolution property for more information
		///   on timestamps.
		/// </remarks>
		/// -->

		public DateTime Timestamp
		{
			get { return this.fTimestamp; }
			set { this.fTimestamp = value; }
		}
	}
}
