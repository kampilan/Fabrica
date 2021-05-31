//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This is the callback type for the OptionsParser.Parse method.
	/// </summary>
	/// <param name="sender">The object which fired the event.</param>
	/// <param name="e">
	///   An OptionsParserEventArgs argument which offers the possibility
	///   of retrieving information about the found options.
	/// </param>
	/// <!--
	/// <remarks>
	///   In addition to the sender parameter, an OptionsParserEventArgs
	///   argument will be passed to the event handlers which offers the
	///   possibility of retrieving information about the found option.
	/// </remarks>
	/// -->

	public delegate void OptionsParserEventHandler(object sender,
		OptionsParserEventArgs e);
}
