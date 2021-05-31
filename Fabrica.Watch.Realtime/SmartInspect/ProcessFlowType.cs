//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the type of a ProcessFlow packet. The type of a
	///   Process Flow entry specifies the way the Console interprets this
	///   packet.
	/// </summary>
	/// <!--
	/// <remarks>
	///  For example, if a Process Flow entry has a type of
	///  ProcessFlowType.EnterThread, the Console interprets this packet as
	///  information about a new thread of your application.
	/// </remarks>
	/// -->

	public enum ProcessFlowType
	{
		/// <summary>
		///   Instructs the Console to enter a new method.
		/// </summary>

		EnterMethod,

		/// <summary>
		///   Instructs the Console to leave a method.
		/// </summary>

		LeaveMethod,

		/// <summary>
		///   Instructs the Console to enter a new thread.
		/// </summary>

		EnterThread,

		/// <summary>
		///   Instructs the Console to leave a thread.
		/// </summary>

		LeaveThread,

		/// <summary>
		///   Instructs the Console to enter a new process.
		/// </summary>

		EnterProcess,

		/// <summary>
		///   Instructs the Console to leave a process.
		/// </summary>

		LeaveProcess
	}
}
