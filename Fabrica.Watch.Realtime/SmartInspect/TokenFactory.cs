//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Creates instances of Token subclasses.
	/// </summary>
	/// <!-- 
	/// <remarks>
	///   This class has only one public method called GetToken, which
	///   is capable of creating Token objects depending on the given
	///   argument.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class TokenFactory
	{
		private static IDictionary fTokens;

		private class AppNameToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.AppName;
			}
		}

		private class SessionToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.SessionName;
			}
		}

		private class HostNameToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.HostName;
			}
		}

		private class TitleToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.Title;
			}

			public override bool Indent
			{
				get { return true; }
			}
		}

		private class TimestampToken: Token
		{
			private const string FORMAT = "yyyy-MM-dd HH:mm:ss.fff";

			public override string Expand(LogEntry logEntry)
			{
				if (Options != null && Options.Length > 0)
				{
					try
					{
						// Try to use a custom format string
						return logEntry.Timestamp.ToString(Options);
					}
					catch (FormatException)
					{ 
					}
				}

				return logEntry.Timestamp.ToString(FORMAT);
			}
		}

		private class LevelToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.Level.ToString();
			}
		}

		private class ColorToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				if (logEntry.Color != Session.DEFAULT_COLOR)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("0x");
					sb.Append(logEntry.Color.R.ToString("x2"));
					sb.Append(logEntry.Color.G.ToString("x2"));
					sb.Append(logEntry.Color.B.ToString("x2"));
					return sb.ToString();
				}
				else 
				{
					return "<default>";
				}
			}
		}

		private class LogEntryTypeToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.LogEntryType.ToString();
			}
		}

		private class ViewerIdToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.ViewerId.ToString();
			}
		}

		private class ThreadIdToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.ThreadId.ToString();
			}
		}

		private class ProcessIdToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return logEntry.ProcessId.ToString();
			}
		}

		private class LiteralToken: Token
		{
			public override string Expand(LogEntry logEntry)
			{
				return Value;
			}
		}

		static TokenFactory()
		{
			fTokens = new Hashtable();
			fTokens.Add("%appname%", typeof(AppNameToken));
			fTokens.Add("%session%", typeof(SessionToken));
			fTokens.Add("%hostname%", typeof(HostNameToken));
			fTokens.Add("%title%", typeof(TitleToken));
			fTokens.Add("%timestamp%", typeof(TimestampToken));
			fTokens.Add("%level%", typeof(LevelToken));
			fTokens.Add("%color%", typeof(ColorToken));
			fTokens.Add("%logentrytype%", typeof(LogEntryTypeToken));
			fTokens.Add("%viewerid%", typeof(ViewerIdToken));
			fTokens.Add("%thread%", typeof(ThreadIdToken));
			fTokens.Add("%process%", typeof(ProcessIdToken));
		}

		/// <summary>
		///   Creates instance of Token subclasses.
		/// </summary>
		/// <param name="value">
		///   The original string representation of the token.
		/// </param>
		/// <returns>
		///   An appropriate Token object for the given string representation
		///   of a token.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method analyses and parses the supplied representation of
		///   a token and creates an appropriate Token object. For example,
		///   if the value argument is set to "%session%", a Token object
		///   is created and returned which is responsible for expanding the
		///   %session% variable. For a list of available tokens and a
		///   detailed description, please have a look at the PatternParser
		///   class, especially the PatternParser.Pattern property.
		/// </remarks>
		/// -->

		public static Token GetToken(string value)
		{
			if (value == null)
			{
				return CreateLiteral(String.Empty);
			}

			if (value.Length <= 2)
			{
				return CreateLiteral(value);
			}

			int length = value.Length;

			if (value[0] != '%' || value[length - 1] != '%')
			{
				return CreateLiteral(value);
			}

			string original = value;
			string options = String.Empty;

			// Extract the token options: %token{options}%
			int index;
			if (value[length - 2] == '}')
			{
				index = value.IndexOf('{');

				if (index > -1)
				{
					index++;
					options = value.Substring(index, length - index - 2);
					value = value.Remove(index - 1, length - index);
					length = value.Length;
				}
			}

			string width = String.Empty;
			index = value.IndexOf(",");

			// Extract the token width: %token,width%
			if (index != -1)
			{
				index++;
				width = value.Substring(index, length - index - 1);
				value = value.Remove(index - 1, length - index);
			}

			value = value.ToLower();
			Type type = (Type) fTokens[value];

			if (type == null)
			{
				return CreateLiteral(original);
			}

			Token token;
			try
			{
				// Create the token and assign the properties
				token = (Token) Activator.CreateInstance(type);
				token.Options = options;
				token.Value = original;
				token.Width = ParseWidth(width);
			}
			catch (Exception)
			{
				return CreateLiteral(original);
			}

			return token;
		}

		private static Token CreateLiteral(string value)
		{
			Token token = new LiteralToken();
			token.Options = String.Empty;
			token.Value = value;
			return token;
		}

		private static int ParseWidth(string value)
		{
			if (value == null)
			{
				return 0;
			}

			value = value.Trim();
			if (value.Length == 0)
			{
				return 0;
			}

			int width;

			try
			{
				width = Convert.ToInt32(value);
			}
			catch (Exception)
			{
				width = 0;
			}

			return width;
		}
	}
}
