//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This is the event handler type for the SmartInspect.Watch event.
	/// </summary>
	/// <param name="sender">The object which fired the event.</param>
	/// <param name="e">
	///   A WatchEventArgs argument which offers the possibility of
	///   retrieving information about the sent packet.
	/// </param>
	/// <!--
	/// <remarks>
	///   In addition to the sender parameter, a WatchEventArgs argument
	///   will be passed to the event handlers which offers the possibility
	///   of retrieving information about the sent packet.
	/// </remarks>
	/// -->

	public delegate void WatchEventHandler(object sender, WatchEventArgs e);
}
