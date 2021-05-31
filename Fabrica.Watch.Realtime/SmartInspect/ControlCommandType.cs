//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the type of a ControlCommand packet. The type of
	///   a Control Commmand influences the way the Console interprets the
	///   packet.
	/// </summary>
	/// <!--
	/// <remarks>
	///   For example, if a Control Command packet has a type of
	///   ControlCommandType.ClearAll, the entire Console is reset when
	///   this packet arrives. Also have a look at the corresponding
	///   Session.ClearAll method.
	/// </remarks>
	/// -->

	public enum ControlCommandType
	{
		/// <summary>
		///   Instructs the Console to clear all Log Entries.
		/// </summary>

		ClearLog,

		/// <summary>
		///   Instructs the Console to clear all Watches.
		/// </summary>

		ClearWatches,

		/// <summary>
		///   Instructs the Console to clear all AutoViews.
		/// </summary>

		ClearAutoViews,

		/// <summary>
		///   Instructs the Console to reset the whole Console.
		/// </summary>

		ClearAll,

		/// <summary>
		///   Instructs the Console to clear all Process Flow entries.
		/// </summary>

		ClearProcessFlow
	}
}
