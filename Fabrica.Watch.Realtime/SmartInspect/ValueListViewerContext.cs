//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the value list viewer in the Console which can display
	///   data as a key/value list.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The value list viewer in the Console interprets the
	///   <link LogEntry.Data, data of a Log Entry> as a simple key/value list.
	///   Every line in the text data is interpreted as one key/value item of
	///   the list. This class takes care of the necessary formatting and
	///   escaping required by the corresponding value list viewer of the
	///   Console.
	///   
	///   You can use the ValueListViewerContext class for creating custom
	///   log methods around <link Session.LogCustomContext, LogCustomContext>
	///   for sending custom data organized as key/value lists.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class ValueListViewerContext: ListViewerContext
	{
		/// <summary>
		///   Overloaded. Creates and initializes a ValueListViewerContext
		///   instance.
		/// </summary>

		public ValueListViewerContext(): base(ViewerId.ValueList)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ValueListViewerContext
		///   instance using a different viewer ID.
		/// </summary>
		/// <param name="vi">The viewer ID to use.</param>
		/// <!--
		/// <remarks>
		///   This constructor is intended for derived classes, such
		///   as the InspectorViewerContext class, which extend the
		///   capabilities of this class and use a different viewer ID.
		/// </remarks>
		/// -->

		protected ValueListViewerContext(ViewerId vi): base(vi)
		{
		}

		/// <summary>
		///   Overloaded. Appends a string value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The string value to use.</param>

		public void AppendKeyValue(string key, string value)
		{
			if (key != null)
			{
				AppendText(EscapeItem(key));
				AppendText("=");
				if (value != null)
				{
					AppendText(EscapeItem(value));
				}
				AppendText("\r\n");
			}
		}

		/// <summary>
		///   Overloaded. Appends a char value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The char value to use.</param>

		public void AppendKeyValue(string key, char value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a byte value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The byte value to use.</param>

		public void AppendKeyValue(string key, byte value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a short value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The short value to use.</param>

		public void AppendKeyValue(string key, short value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends an int value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The int value to use.</param>

		public void AppendKeyValue(string key, int value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a long value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The long value to use.</param>

		public void AppendKeyValue(string key, long value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a float value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The float value to use.</param>

		public void AppendKeyValue(string key, float value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a double value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The double value to use.</param>
		
		public void AppendKeyValue(string key, double value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a decimal value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The decimal value to use.</param>

		public void AppendKeyValue(string key, decimal value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a DateTime value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The DateTime value to use.</param>

		public void AppendKeyValue(string key, DateTime value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends a bool value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The bool value to use.</param>

		public void AppendKeyValue(string key, bool value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Appends an object value and its key.
		/// </summary>
		/// <param name="key">The key to use.</param>
		/// <param name="value">The object value to use.</param>

		public void AppendKeyValue(string key, object value)
		{
			AppendKeyValue(key, Convert.ToString(value));
		}

		/// <summary>Escapes a key or a value.</summary>
		/// <param name="item">The key or value to escape.</param>
		/// <returns>The escaped key or value.</returns>
		/// <!--
		/// <remarks>
		///   This method ensures that the escaped key or value does not
		///   contain any newline characters, such as the carriage return
		///   or linefeed characters. Furthermore, it escapes the '\' and
		///   '=' characters.
		/// </remarks>
		/// -->
		
		public virtual string EscapeItem(string item)
		{
			return EscapeLine(item, "\\=");
		}
	}
}
