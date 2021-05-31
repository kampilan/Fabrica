//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used in the LogSource methods of the Session class to specify
	///   the type of source code.
	/// </summary>

	public enum SourceId
	{
		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for HTML.
		/// </summary>

		Html = (int) ViewerId.HtmlSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for Java Script.
		/// </summary>

		JavaScript = (int) ViewerId.JavaScriptSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for VBScript.
		/// </summary>

		VbScript = (int) ViewerId.VbScriptSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for Perl.
		/// </summary>

		Perl = (int) ViewerId.PerlSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for SQL.
		/// </summary>

		Sql = (int) ViewerId.SqlSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for INI files.
		/// </summary>

		Ini = (int) ViewerId.IniSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for Python.
		/// </summary>

		Python = (int) ViewerId.PythonSource,

		/// <summary>
		///   Instructs the Session.LogSource methods to use
		///   syntax highlighting for XML.
		/// </summary>

		Xml = (int) ViewerId.XmlSource
	}
}
