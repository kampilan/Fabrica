//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for parsing the options part of a SmartInspect
	///   connections string.
	/// </summary>
	/// <!--
	/// <seealso cref="Gurock.SmartInspect.OptionsParserEventHandler"/>
	/// <remarks>
	///   This class offers a single method only, called Parse, which
	///   is responsible for parsing the options part of a connections
	///   string. This method informs the caller about found options
	///   with a supplied callback delegate.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class OptionsParser
	{
		private void DoOption(OptionsParserEventHandler callback, 
			string protocol, string key, string value)
		{
			value = value.Trim();
			key = key.ToLower().Trim();
			callback(this, new OptionsParserEventArgs(protocol, key, value));
		}

		private void InternalParse(string protocol, string options,
			OptionsParserEventHandler callback)
		{
			char c;
			StringBuilder key = new StringBuilder();
			StringBuilder value = new StringBuilder();

			for (int i = 0, length = options.Length; i < length; )
			{
				// Store key
				c = options[i];
				while (i++ < length - 1)
				{
					key.Append(c);
					c = options[i];
					if (c == '=')
					{
						break;
					}
				}

				if (c != '=')
				{
					// The options string is invalid because the '='
					// character is missing.
					throw new SmartInspectException(
						"Missing \"=\" at " + protocol + " protocol"
					);
				}
				else if (i < length)
				{
					i++;
				}

				// Store value
				bool quoted = false;
				while (i < length)
				{
					c = options[i++];
					if (c == '"')
					{
						if (i < length)
						{
							if (options[i] != '"')
							{
								quoted = !quoted;
								continue;
							}
							else
							{
								i++; // Skip one '"'
							}
						}
						else 
						{
							quoted = !quoted;
							continue;
						}
					}
					else if (c == ',' && !quoted)
					{
						break;
					}

					value.Append(c);
				}

				if (quoted)
				{
					throw new SmartInspectException(
						"Quoted value not closed at protocol \"" +
						protocol + "\""
					);
				}

				DoOption(callback, protocol, key.ToString(), 
					value.ToString());

				key.Length = 0;
				value.Length = 0;
			}
		}

		/// <summary>
		///   Parses the options part of a connections string.
		/// </summary>
		/// <param name="protocol">
		///   The related protocol. Not allowed to be null.
		/// </param>
		/// <param name="options">
		///   The options to parse. Not allowed to be null.
		/// </param>
		/// <param name="callback">
		///   The callback delegate which should be informed about found
		///   options. Not allowed to be null.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.OptionsParserEventHandler"/>
		/// <remarks>
		///   This method parses the supplied options part of a connections
		///   string and informs the caller about found options with the
		///   supplied callback delegate.
		/// 
		///   For information about the correct syntax of the options,
		///   please refer to the documentation of the Protocol.Options
		///   property.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The protocol, options or callback
		///                              argument is null.
		///   SmartInspectException    Invalid options string syntax.
		/// </table>
		/// </exception>
		/// -->

		public void Parse(string protocol, string options,
			OptionsParserEventHandler callback)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException("protocol");
			}
			else if (options == null)
			{
				throw new ArgumentNullException("options");
			}
			else if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			else
			{
				options = options.Trim();
				if (options.Length > 0)
				{
					InternalParse(protocol, options, callback);
				}
			}
		}
	}
}
