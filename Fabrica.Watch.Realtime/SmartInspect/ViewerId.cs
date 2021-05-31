//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Specifies the viewer for displaying the title or data of a Log
	///   Entry in the Console.
	/// </summary>
	/// <!--
	/// <remarks>
	///   There are many viewers available for displaying the data of a
	///   Log Entry in different ways. For example, there are viewers that
	///   can display lists, tables, binary dumps of data or even websites.
	///   
	///   Every viewer in the Console has a corresponding so called viewer
	///   context in this library which can be used to send custom logging
	///   information. To get started, please see the documentation of the
	///   Session.LogCustomContext method and ViewerContext class.
	/// </remarks>
	/// -->

	public enum ViewerId
	{
		/// <summary>
		///   Instructs the Console to use no viewer at all.
		/// </summary>

		None = -1,

		/// <summary>
		///   Instructs the Console to display the title of a Log Entry
		///   in a read-only text field.
		/// </summary>

		Title,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   in a read-only text field.
		/// </summary>

		Data,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a list.
		/// </summary>

		List,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a key/value list.
		/// </summary>

		ValueList,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   using an object inspector.
		/// </summary>

		Inspector,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a table.
		/// </summary>

		Table,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a website.
		/// </summary>

		Web = 100,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a binary dump using a read-only hex editor.
		/// </summary>

		Binary = 200,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as HTML source with syntax highlighting.
		/// </summary>

		HtmlSource = 300,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as Java Script source with syntax highlighting.
		/// </summary>

		JavaScriptSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as VBScript source with syntax highlighting.
		/// </summary>

		VbScriptSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as Perl source with syntax highlighting.
		/// </summary>

		PerlSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as SQL source with syntax highlighting.
		/// </summary>

		SqlSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as INI source with syntax highlighting.
		/// </summary>

		IniSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as Python source with syntax highlighting.
		/// </summary>

		PythonSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as XML source with syntax highlighting.
		/// </summary>

		XmlSource,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as bitmap image.
		/// </summary>

		Bitmap = 400,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as JPEG image.
		/// </summary>

		Jpeg,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as a Windows icon.
		/// </summary>

		Icon,

		/// <summary>
		///   Instructs the Console to display the data of a Log Entry
		///   as Windows Metafile image.
		/// </summary>

		Metafile
	}
}
