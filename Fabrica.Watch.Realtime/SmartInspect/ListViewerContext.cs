//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the list viewer in the Console which can display simple
	///   lists of text data.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The list viewer in the Console interprets the
	///   <link LogEntry.Data, data of a Log Entry> as a list. Every line in
	///   the text data is interpreted as one item of the list. This class
	///   takes care of the necessary formatting and escaping required by the
	///   corresponding list viewer in the Console.
	///   
	///   You can use the ListViewerContext class for creating custom
	///   log methods around <link Session.LogCustomContext, LogCustomContext>
	///   for sending custom data organized as simple lists.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class ListViewerContext: TextContext
	{
		/// <summary>
		///   Overloaded. Creates and initializes a ListViewerContext
		///   instance.
		/// </summary>

		public ListViewerContext(): base(ViewerId.List)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ListViewerContext
		///   instance using a different viewer ID.
		/// </summary>
		/// <param name="vi">The viewer ID to use.</param>
		/// <!--
		/// <remarks>
		///   This constructor is intended for derived classes, such
		///   as the ValueListViewerContext class, which extend the
		///   capabilities of this class and use a different viewer ID.
		/// </remarks>
		/// -->

		protected ListViewerContext(ViewerId vi): base(vi)
		{
		}

		/// <summary>Overridden. Escapes a line.</summary>
		/// <param name="line">The line to escape.</param>
		/// <returns>The escaped line.</returns>
		/// <!--
		/// <remarks>
		///   This method ensures that the escaped line does not
		///   contain any newline characters, such as the carriage
		///   return or linefeed characters.
		/// </remarks>
		/// -->

        protected override string EscapeLine(string line)
        {
            return EscapeLine(line, null);
        }

        /// <summary>Escapes a line.</summary>
        /// <param name="line">The line to escape.</param>
        /// <param name="toEscape">
        ///   A set of characters which should be escaped in addition
        ///   to the newline characters. Can be null or empty.
        /// </param>		
        /// <returns>The escaped line.</returns>
        /// <!--
        /// <remarks>
        ///   This method ensures that the escaped line does not
        ///   contain characters listed in the toEscape parameter plus
        ///   any newline characters, such as the carriage return or
        ///   linefeed characters.
        /// </remarks>
        /// -->

		protected static string EscapeLine(string line, string toEscape)
		{
			if (line == null || line.Length == 0)
			{
				return line;
			}
			else 
			{
				char b = '\u0000';
				StringBuilder sb = new StringBuilder(line.Length);
				
				for (int i = 0, len = line.Length; i < len; i++)
				{
					char c = line[i];
					if (c == '\r' || c == '\n')
					{
						if (b != '\r' && b != '\n')
						{
							// Newline characters need to be removed,
							// they would break the list format.
							sb.Append(' ');
						}
					}
                    else if (toEscape != null && toEscape.IndexOf(c) != -1)
                    {
                        // The current character needs to be escaped as
                        // well (with the \ character).
                        sb.Append("\\");
                        sb.Append(c);
                    }
                    else
					{
						// This character is valid, so just append it.
						sb.Append(c);
					}
					b = c;
				}

				return sb.ToString();
			}
		}
	}
}
