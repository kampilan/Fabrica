//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the inspector viewer in the Console which displays
	///   key/value pairs in an object inspector control.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The inspector viewer in the Console interprets the
	///   <link LogEntry.Data, data of a Log Entry> as a key/value list with
	///   group support like object inspectors from popular IDEs. This class
	///   takes care of the necessary formatting and escaping required by the
	///   corresponding inspector viewer in the Console.
	///   
	///   You can use the InspectorViewerContext class for creating custom
	///   log methods around <link Session.LogCustomContext, LogCustomContext>
	///   for sending custom data organized as grouped key/value pairs.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class InspectorViewerContext: ValueListViewerContext
	{
		/// <summary>
		///   Creates and initializes an InspectorViewerContext instance.
		/// </summary>

		public InspectorViewerContext(): base(ViewerId.Inspector)
		{
		}

		/// <summary>Starts a new group.</summary>
		/// <param name="group">The name of the group to use.</param>

		public void StartGroup(string group)
		{
			if (group != null)
			{
				AppendText("[");
				AppendText(EscapeItem(group));
				AppendText("]\r\n");
			}
		}

		/// <summary>Overridden. Escapes a key or a value.</summary>
		/// <param name="item">The key or value to escape.</param>
		/// <returns>The escaped key or value.</returns>
		/// <!--
		/// <remarks>
		///   This method ensures that the escaped key or value does
		///   not contain any newline characters, such as the carriage
		///   return or linefeed characters. Furthermore, it escapes
		///   the '\', '=', '[' and ']' characters.
		/// </remarks>
		/// -->

		public override string EscapeItem(string item)
		{
			return EscapeLine(item, "\\=[]");
		}
	}
}
