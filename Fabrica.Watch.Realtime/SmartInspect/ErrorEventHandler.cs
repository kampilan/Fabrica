//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This is the event handler type for the SmartInspect.Error
	///   and Protocol.Error events.
	/// </summary>
	/// <param name="sender">The object which fired the event.</param>
	/// <param name="e">
	///   An ErrorEventArgs argument which provides information why the
	///   event has been fired.
	/// </param>
	/// <!--
	/// <remarks>
	///   In addition to the sender parameter, an ErrorEventArgs argument
	///   will be passed to the event handlers which offers the possibility
	///   of retrieving information about the occurred error.
	/// </remarks>
	/// -->

	public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
}
