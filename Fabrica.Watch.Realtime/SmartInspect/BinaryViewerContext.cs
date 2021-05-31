//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the binary viewer in the Console which can display binary
	///   data in a read-only hex editor.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The binary viewer in the Console interprets the <link LogEntry.Data,
	///   data of a Log Entry> as binary data and displays it in a read-only
	///   hex editor.
	///   
	///   You can use the BinaryViewerContext class for creating custom log
	///   methods around <link Session.LogCustomContext, LogCustomContext> for
	///   sending custom binary data.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class BinaryViewerContext: BinaryContext
	{
		/// <summary>
		///   Creates and initializes a BinaryViewerContext instance.
		/// </summary>

		public BinaryViewerContext(): base(ViewerId.Binary)
		{
		}
	}
}
