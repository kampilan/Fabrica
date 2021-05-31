//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents a token in the pattern string of the TextProtocol
	///   protocol.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This is the abstract base class for all available tokens. Derived
	///   classes are not documented for clarity reasons. To create a
	///   suitable token object for a given token string, you can use the
	///   TokenFactory class.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public abstract class Token
	{
		private string fValue;
		private string fOptions;
		private int fWidth;

		/// <summary>
		///   Creates a string representation of a variable or literal token.
		/// </summary>
		/// <param name="logEntry">
		///   The LogEntry to use to create the string representation.
		/// </param>
		/// <returns>
		///   The text representation of this token for the supplied LogEntry
		///   object.
		/// </returns>
		/// <!--
		/// <remarks>
		///   With the help of the supplied LogEntry, this token is expanded
		///   into a string. For example, if this token represents the
		///   %session% variable of a pattern string, this Expand method
		///   simply returns the session name of the supplied LogEntry.
		///
		///   For a literal token, the supplied LogEntry argument is ignored
		///   and the Value property is returned.
		/// </remarks>
		/// -->

		public abstract string Expand(LogEntry logEntry);

		/// <summary>
		///   Represents the raw string value of the parsed pattern string
		///   for this token.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property represents the raw string of this token as found
		///   in the parsed pattern string. For a variable, this property is
		///   set to the variable name surrounded with '%' characters and an
		///   optional options string like this: %name{options}%. For a
		///   literal, this property can have any value.
		/// </remarks>
		/// -->

		public string Value
		{
			get { return this.fValue; }
			set { this.fValue = value; }
		}

		/// <summary>
		///   Represents the optional options string for this token.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   A variable token can have an optional options string. In the
		///   raw string representation of a token, an options string can be
		///   specified in curly braces after the variable name like this:
		///   %name{options}%. For a literal, this property is always set to
		///   an empty string. 
		/// </remarks>
		/// -->

		public string Options
		{
			get { return this.fOptions; }
			set { this.fOptions = value; }
		}

		/// <summary>
		///   Indicates if this token supports indenting.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property always returns false unless this token represents
		///   the title token of a pattern string. This property is used
		///   by the PatternParser.Expand method to determine if a token
		///   allows indenting.
		/// </remarks>
		/// -->

		public virtual bool Indent
		{
			get { return false; }
		}

		/// <summary>
		///   Represents the minimum width of this token.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   A variable token can have an optional width modifier. In the
		///   raw string representation of a token, a width modifier can be
		///   specified after the variable name like this: %name,width%.
		///   Width must be a valid positive or negative integer.
		///   
		///   If the width is greater than 0, formatted values will be
		///   right-aligned. If the width is less than 0, they will be
		///   left-aligned.
		///   
		///   For a literal, this property is always set to 0. 
		/// </remarks>
		/// -->

		public int Width
		{
			get { return this.fWidth; }
			set { this.fWidth = value; }
		}
	}
}
