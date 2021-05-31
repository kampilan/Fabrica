//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the SmartInspect.Error event.
	/// </summary>
	/// <!--
	/// <remarks>
	///   It has only one public class member named Exception. This member
	///   is a property, which just returns the occurred exception.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class ErrorEventArgs: System.EventArgs
	{
		private Exception fException;

		/// <summary>
		///   Creates and initializes an ErrorEventArgs instance.
		/// </summary>
		/// <param name="e">The occurred exception.</param>

		public ErrorEventArgs(Exception e)
		{
			this.fException = e;
		}

		/// <summary>
		///   This read-only property returns the occurred exception.
		/// </summary>

		public Exception Exception
		{
			get { return this.fException; }
		}
	}
}
