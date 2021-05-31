//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents a scheduler command as used by the Scheduler class
	///   and the asynchronous protocol mode.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by the Scheduler class to enqueue protocol
	///   operations for later execution when operating in asynchronous
	///   mode. For detailed information about the asynchronous protocol 
	///   mode, please refer to Protocol.IsValidOption.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class SchedulerCommand
	{
		private SchedulerAction fAction;
		private object fState;

		/// <summary>
		///   Represents the scheduler action to execute. Please refer
		///   to the documentation of the SchedulerAction enum for more
		///   information about possible values.
		/// </summary>

		public SchedulerAction Action
		{
			get { return this.fAction; }
			set { this.fAction = value; }
		}

		/// <summary>
		///   Represents the optional scheduler command state object which
		///   provides additional information about the scheduler command.
		///   This property can be null.
		/// </summary>

		public object State
		{
			get { return this.fState; }
			set { this.fState = value; }
		}

		/// <summary>
		///   Calculates and returns the total memory size occupied by
		///   this scheduler command.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This read-only property returns the total occupied memory
		///   size of this scheduler command. This functionality is used by
		///   the <link Protocol.IsValidOption, asynchronous protocol mode>
		///   to track the total size of scheduler commands.
		/// </remarks>
		/// -->

		public int Size
		{
			get 
			{
				if (this.fAction != SchedulerAction.WritePacket)
				{
					return 0;
				}

				if (this.fState != null)
				{
					return ((Packet) this.fState).Size;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
