//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the table viewer in the Console which can display text
	///   data as a table.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The table viewer in the Console interprets the
	///   <link LogEntry.Data, data of a Log Entry> as a table. This class
	///   takes care of the necessary formatting and escaping required by
	///   the corresponding table viewer in the Console.
	///   
	///   You can use the TableViewerContext class for creating custom
	///   log methods around <link Session.LogCustomContext, LogCustomContext>
	///   for sending custom data organized as tables.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class TableViewerContext: ListViewerContext
	{
		private bool fLineStart;

		/// <summary>
		///   Creates and initializes a TableViewerContext instance.
		/// </summary>

		public TableViewerContext(): base(ViewerId.Table)
		{
			this.fLineStart = true;
		}

		/// <summary>
		///   Appends a header to the text data.
		/// </summary>
		/// <param name="header">The header to append.</param>

		public void AppendHeader(string header)
		{
			AppendLine(header);
			AppendLine(String.Empty);
		}

		/// <summary>
		///   Overloaded. Adds a string entry to the current row.
		/// </summary>
		/// <param name="entry">The string entry to add.</param>

		public void AddRowEntry(string entry)
		{
			if (entry != null)
			{
				if (this.fLineStart)
				{
					this.fLineStart = false;
				}
				else
				{
					AppendText(", ");
				}
				AppendText(EscapeCSVEntry(entry));
			}
		}

		/// <summary>
		///   Overloaded. Adds a char entry to the current row.
		/// </summary>
		/// <param name="entry">The char entry to add.</param>

		public void AddRowEntry(char entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a byte entry to the current row.
		/// </summary>
		/// <param name="entry">The byte entry to add.</param>

		public void AddRowEntry(byte entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a short entry to the current row.
		/// </summary>
		/// <param name="entry">The short entry to add.</param>

		public void AddRowEntry(short entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds an int entry to the current row.
		/// </summary>
		/// <param name="entry">The int entry to add.</param>

		public void AddRowEntry(int entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a long entry to the current row.
		/// </summary>
		/// <param name="entry">The long entry to add.</param>

		public void AddRowEntry(long entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a float entry to the current row.
		/// </summary>
		/// <param name="entry">The float entry to add.</param>

		public void AddRowEntry(float entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a double entry to the current row.
		/// </summary>
		/// <param name="entry">The double entry to add.</param>

		public void AddRowEntry(double entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a decimal entry to the current row.
		/// </summary>
		/// <param name="entry">The decimal entry to add.</param>

		public void AddRowEntry(decimal entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a DateTime entry to the current row.
		/// </summary>
		/// <param name="entry">The DateTime entry to add.</param>

		public void AddRowEntry(DateTime entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds a bool entry to the current row.
		/// </summary>
		/// <param name="entry">The bool entry to add.</param>

		public void AddRowEntry(bool entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		/// <summary>
		///   Overloaded. Adds an object entry to the current row.
		/// </summary>
		/// <param name="entry">The object entry to add.</param>

		public void AddRowEntry(object entry)
		{
			AddRowEntry(Convert.ToString(entry));
		}

		private static string EscapeCSVEntry(string entry)
		{
			if (entry == null || entry.Length == 0)
			{
				return entry;
			}
			else
			{
				StringBuilder sb = new StringBuilder(2*entry.Length);
				sb.Append("\"");

				for (int i = 0, len = entry.Length; i < len; i++)
				{
					char c = entry[i];

					if (Char.IsWhiteSpace(c))
					{
						// Newline characters need to be escaped,
						// they would break the csv format.
						sb.Append(" ");
					}
					else if (c == '"')
					{
						// '"' characters are used to surround entries
						// in the csv format, so they need to be escaped.
						sb.Append("\"\"");
					}
					else 
					{
						// This character is valid, so just append it.
						sb.Append(c);
					}
				}

				sb.Append("\"");
				return sb.ToString();
			}
		}

		/// <summary>
		///   Begins a new row.
		/// </summary>

		public void BeginRow()
		{
			this.fLineStart = true;
		}

		/// <summary>
		///   Ends the current row.
		/// </summary>

		public void EndRow()
		{
			AppendLine(String.Empty);
		}
	}
}
