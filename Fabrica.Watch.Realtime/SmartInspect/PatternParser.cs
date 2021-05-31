//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Capable of parsing and expanding a pattern string as used in the
	///   TextProtocol and TextFormatter classes.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The PatternParser class is capable of creating a text
	///   representation of a LogEntry object (see Expand). The string
	///   representation can be influenced by setting a pattern string.
	///   Please see the Pattern property for a description.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class PatternParser
	{
		private static readonly string SPACES = "   ";
		private int fPosition;
		private string fPattern;
		private ArrayList fTokens;
		private bool fIndent;
		private int fIndentLevel;
		private StringBuilder fBuilder;

		/// <summary>
		///   Creates and initializes a PatternParser instance.
		/// </summary>

		public PatternParser()
		{
			this.fTokens = new ArrayList();
			this.fBuilder = new StringBuilder();
			this.fPattern = String.Empty;
		}

		/// <summary>
		///   Creates a text representation of a LogEntry by applying a
		///   user-specified Pattern string.
		/// </summary>
		/// <param name="logEntry">
		///   The LogEntry whose text representation should be computed by
		///   applying the current Pattern string. All recognized variables
		///   in the pattern string are replaced with the actual values of
		///   this LogEntry.
		/// </param>
		/// <returns>
		///   The text representation for the supplied LogEntry object.
		/// </returns>

		public string Expand(LogEntry logEntry)
		{
			if (this.fTokens.Count == 0)
			{
				return String.Empty;
			}

			this.fBuilder.Length = 0;
			if (logEntry.LogEntryType == LogEntryType.LeaveMethod)
			{
				if (this.fIndentLevel > 0)
				{
					this.fIndentLevel--;
				}
			}

			foreach (Token token in this.fTokens)
			{
				if (this.fIndent && token.Indent)
				{
					for (int i = 0; i < this.fIndentLevel; i++)
					{
						this.fBuilder.Append(SPACES);
					}
				}

				string expanded = token.Expand(logEntry);

				if (token.Width < 0)
				{
					/* Left-aligned */
					this.fBuilder.Append(expanded);

					int pad = -token.Width - expanded.Length;

					for (int i = 0; i < pad; i++)
					{
						this.fBuilder.Append(' ');
					}
				}
				else if (token.Width > 0)
				{
					int pad = token.Width - expanded.Length;

					for (int i = 0; i < pad; i++)
					{
						this.fBuilder.Append(' ');
					}

					/* Right-aligned */
					this.fBuilder.Append(expanded);
				}
				else 
				{
					this.fBuilder.Append(expanded);
				}
			}

			if (logEntry.LogEntryType == LogEntryType.EnterMethod)
			{
				this.fIndentLevel++;
			}

			return this.fBuilder.ToString();
		}

		private Token Next()
		{
			int length = this.fPattern.Length;

			if (this.fPosition < length)
			{
				bool isVariable = false;
				int pos = this.fPosition;

				if (this.fPattern[pos] == '%')
				{
					isVariable = true;
					pos++;
				}

				while (pos < length)
				{
					if (this.fPattern[pos] == '%')
					{
						if (isVariable)
						{
							pos++;
						}
						break;
					}
					pos++;
				}

				string value = this.fPattern.Substring(this.fPosition, 
					pos - this.fPosition);
				this.fPosition = pos;

				return TokenFactory.GetToken(value);
			}
			else 
			{
				return null;
			}
		}

		private void Parse()
		{
			this.fTokens.Clear();
			Token token = Next();
			while (token != null)
			{
				this.fTokens.Add(token);
				token = Next();
			}
		}

		/// <summary>
		///   Represents the pattern string for this PatternParser object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The pattern string influences the way a text representation of
		///   a LogEntry object is created. A pattern string consists of a
		///   list of so called variable and literal tokens. When a string
		///   representation of a LogEntry object is created, the variables
		///   are replaced with the actual values of the LogEntry object.
		///
		///   Variables have a unique name, are surrounded with '%' characters
		///   and can have an optional options string enclosed in curly
		///   braces like this: %name{options}%.
		///   
		///   You can also specify the minimum width of a value like this:
		///   %name,width%. Width must be a valid positive or negative
		///   integer. If the width is greater than 0, formatted values will
		///   be right-aligned. If the width is less than 0, they will be
		///   left-aligned.
		///
		///   The following table lists the available variables together with
		///   the corresponding LogEntry property.
		///   
		///   <table>
		///   Variable        Corresponding Property
		///   +               +            
		///   %appname%       LogEntry.AppName
		///   %color%         LogEntry.Color
		///   %hostname%      LogEntry.HostName
		///   %level%         Packet.Level
		///   %logentrytype%  LogEntry.LogEntryType
		///   %process%       LogEntry.ProcessId
		///   %session%       LogEntry.SessionName
		///   %thread%        LogEntry.ThreadId
		///   %timestamp%     LogEntry.Timestamp
		///   %title%         LogEntry.Title
		///   %viewerid%      LogEntry.ViewerId
		///   </table>
		///
		///   For the timestamp token, you can use the options string to
		///   pass a custom date/time format string. This can look as
		///   follows:
		///   
		///   %timestamp{HH:mm:ss.fff}%
		///   
		///   The format string must be a valid .NET DateTime format
		///   string. The default format string used by the timestamp token
		///   is "yyyy-MM-dd HH:mm:ss.fff".
		///   
		///   Literals are preserved as specified in the pattern string. When
		///   a specified variable is unknown, it is handled as literal.
		/// </remarks>  
		/// <example>
		/// <code>
		/// "[%timestamp%] %level,8%: %title%"
		/// "[%timestamp%] %session%: %title% (Level: %level%)"
		/// </code>
		/// </example>
		/// -->

		public string Pattern
		{
			get { return this.fPattern; }

			set 
			{
				this.fPosition = 0;
				this.fIndentLevel = 0;

				if (value != null)
				{
					this.fPattern = value.Trim();
				}
				else 
				{
					this.fPattern = String.Empty;
				}

				Parse();
			}
		}

		/// <summary>
		///   Indicates if the Expand method should automatically intend
		///   log packets like in the Views of the SmartInspect Console.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Log Entry packets of type EnterMethod increase the indentation
		///   and packets of type LeaveMethod decrease it.
		/// </remarks>
		/// -->

		public bool Indent
		{
			get { return this.fIndent; }
			set { this.fIndent = value; }
		}
	}
}
