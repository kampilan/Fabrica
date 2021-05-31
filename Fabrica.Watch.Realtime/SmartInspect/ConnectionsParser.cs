//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for parsing a SmartInspect connections string.
	/// </summary>
	/// <!--
	/// <seealso cref="Gurock.SmartInspect.ConnectionsParserEventHandler"/>
	/// <remarks>
	///   This class offers a single method only, called Parse, which is
	///   responsible for parsing a connections string. This method informs
	///   the caller about found protocols and options with a supplied
	///   callback delegate.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class ConnectionsParser
	{
		private void DoProtocol(ConnectionsParserEventHandler callback,
			string protocol, string options)
		{
			options = options.Trim();
			protocol = protocol.ToLower().Trim();
			callback(this, new ConnectionsParserEventArgs(protocol, options));
		}

		private void InternalParse(string connections,
			ConnectionsParserEventHandler callback)
		{
			char c;
			StringBuilder protocol = new StringBuilder();
			StringBuilder options = new StringBuilder();

			for (int i = 0, length = connections.Length; i < length; )
			{
				// Store protocol name.
				c = connections[i];
				while (i++ < length - 1)
				{
					protocol.Append(c);
					c = connections[i];
					if (c == '(')
					{
						break;
					}
				}

				if (c != '(')
				{
					// The connections string is invalid because the '('
					// character is missing.
					throw new SmartInspectException(
						"Missing \"(\" at position " + (i + 1)
					);
				}
				else if (i < length)
				{
					i++;
				}

				// Store protocol options
				bool quoted = false;
				while (i < length)
				{
					c = connections[i++];
					if (c == '"')
					{
						if (i < length)
						{
							if (connections[i] != '"')
							{
								quoted = !quoted;
							}
							else 
							{
								i++;
								options.Append('"');
							}
						}
					}
					else if (c == ')' && !quoted)
					{
						break;
					}
					options.Append(c);
				}

				if (quoted)
				{
					throw new SmartInspectException(
						"Quoted values not closed at protocol \"" +
						protocol + "\""
					);
				}

				if (c != ')')
				{
					// The connections string is invalid because the ')'
					// character is missing.
					throw new SmartInspectException(
						"Missing \")\" at position " + (i + 1)
					);
				}
				else if (i < length && connections[i] == ',')
				{
					// Skip the ',' character.
					i++;
				}

				DoProtocol(callback, protocol.ToString(), options.ToString());
				protocol.Length = 0;
				options.Length = 0;
			}
		}

		/// <summary>
		///   Parses a connections string.
		/// </summary>
		/// <param name="connections">
		///   The connections string to parse. Not allowed to be null.
		/// </param>
		/// <param name="callback">
		///   The callback delegate which should be informed about found
		///   protocols and their options. Not allowed to be null.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method parses the supplied connections string and
		///   informs the caller about found protocols and options with
		///   the supplied callback delegate.
		/// 
		///   For information about the correct syntax, please refer to
		///   the documentation of the SmartInspect.Connections property.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The connections or callback argument
		///                              is null.
		///   SmartInspectException    Invalid connections string syntax.
		/// </table>
		/// </exception>
		/// -->

		public void Parse(string connections, 
			ConnectionsParserEventHandler callback)
		{
			if (connections == null)
			{
				throw new ArgumentNullException("connections");
			}
			else if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			else
			{
				connections = connections.Trim();
				if (connections.Length > 0)
				{
					InternalParse(connections, callback);
				}
			}
		}
	}
}
