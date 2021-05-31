//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents a custom protocol action command as used by the
	///   Protocol.Dispatch method.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by custom protocol actions. For detailed 
	///   information about custom protocol actions, please refer to
	///   the Protocol.Dispatch and SmartInspect.Dispatch methods.
	/// </remarks>
	/// <threadsafety>
	///   The public members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	public class ProtocolCommand
	{
		private int fAction;
		private object fState;

		/// <summary>
		///   Creates and initializes a new ProtocolCommand instance.
		/// </summary>
		/// <param name="action">
		///   The custom protocol action to execute.
		/// </param>
		/// <param name="state">
		///   Optional object which provides additional information about
		///   the custom protocol action.
		/// </param>

		public ProtocolCommand(int action, object state)
		{
			this.fAction = action;
			this.fState = state;
		}

		/// <summary>
		///   Returns the custom protocol action to execute. The value
		///   of this property is protocol specific.
		/// </summary>

		public int Action
		{
			get { return this.fAction; }
		}

		/// <summary>
		///   Returns the optional protocol command object which provides
		///   additional information about the custom protocol action.
		///   This property can be null.
		/// </summary>

		public object State
		{
			get { return this.fState; }
		}
	}
}
