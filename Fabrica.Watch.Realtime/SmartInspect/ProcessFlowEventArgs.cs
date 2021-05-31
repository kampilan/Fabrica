//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the SmartInspect.ProcessFlow event.
	/// </summary>
	/// <!--
	/// <remarks>
	///   It has only one public class member named ProcessFlow. This
	///   member is a property, which just returns the sent packet.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class ProcessFlowEventArgs: System.EventArgs
	{
		private ProcessFlow fProcessFlow;

		/// <summary>
		///   Creates and initializes a ProcessFlowEventArgs instance.
		/// </summary>
		/// <param name="processFlow">
		///   The Process Flow packet which caused the event.
		/// </param>

		public ProcessFlowEventArgs(ProcessFlow processFlow)
		{
			this.fProcessFlow = processFlow;
		}

		/// <summary>
		///   This read-only property returns the ProcessFlow
		///   packet, which has just been sent.
		/// </summary>

		public ProcessFlow ProcessFlow
		{
			get { return this.fProcessFlow; }
		}
	}
}
