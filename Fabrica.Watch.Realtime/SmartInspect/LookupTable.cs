//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;
using System.Text;
using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents a simple collection of key/value pairs.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The LookupTable class is responsible for storing and returning
	///   values which are organized by keys. Values can be added with
	///   the Put method. To query a String value for a given key, the
	///   GetStringValue method can be used. To query and automatically
	///   convert values to types other than String, please have a look
	///   at the Get method family.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class LookupTable
	{
		private IDictionary fItems;
		private const int SECONDS_FACTOR = 1000;
		private const int MINUTES_FACTOR = SECONDS_FACTOR * 60;
		private const int HOURS_FACTOR = MINUTES_FACTOR * 60;
		private const int DAYS_FACTOR = HOURS_FACTOR * 24;
		private const int KB_FACTOR = 1024;
		private const int MB_FACTOR = KB_FACTOR * 1024;
		private const int GB_FACTOR = MB_FACTOR * 1024;

		/// <summary>
		///   Creates and initializes a LookupTable instance.
		/// </summary>
	
		public LookupTable()
		{
#if SI_DOTNET_1x
			this.fItems = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
#else
			this.fItems = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);
#endif
		}

		/// <summary>
		///   Adds or updates an element with a specified key and value
		///   to the LookupTable.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		/// <param name="value">The value of the element.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new element with a given key and value to
		///   the collection of key/value pairs. If an element for the
		///   given key already exists, the original element's value is
		///   updated.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key or value argument is null.
		/// </table>
		/// </exception>
		/// -->
	
		public void Put(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			else if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			else 
			{
				this.fItems[key] = value;
			}
		}

		/// <summary>
		///   Adds a new element with a specified key and value to the
		///   LookupTable.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		/// <param name="value">The value of the element.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new element with a given key and value to
		///   the collection of key/value pairs. If an element for the
		///   given key already exists, the original element's value is
		///   not updated.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key or value argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void Add(string key, string value)
		{
			if (!Contains(key))
			{
				Put(key, value);
			}
		}

		/// <summary>
		///   Removes an existing element with a given key from this lookup
		///   table.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <!--
		/// <remarks>
		///  This method removes the element with the given key from the
		///  internal list. Nothing happens if no element with the given
		///  key can be found.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void Remove(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			else 
			{
				this.fItems.Remove(key);
			}
		}

		/// <summary>
		///   Tests if the collection contains a value for a given key. 
		/// </summary>
		/// <param name="key">The key to test for.</param>
		/// <returns>
		///   True if a value exists for the given key and false otherwise.
		/// </returns>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
	
		public bool Contains(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			else
			{
				return this.fItems.Contains(key);
			}
		}

		/// <summary>
		///   Removes all key/value pairs of the collection.
		/// </summary>

		public void Clear()
		{
			this.fItems.Clear();
		}

		/// <summary>
		///   Returns the number of key/value pairs of this collection.
		/// </summary>

		public int Count
		{
			get { return this.fItems.Count; }
		}

		/// <summary>
		///   Returns a value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value for a given key if an element with the given
		///   key exists or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->
	
		public string GetStringValue(string key, string defaultValue)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			else 
			{
				string value = this.fItems[key] as string;
				return value == null ? defaultValue : value;
			}
		}

		/// <summary>
		///   Returns a value of an element converted to an integer for a
		///   given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to an integer for the given key if
		///   an element with the given key exists and the found value is a
		///   valid integer or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns the defaultValue argument if either the
		///   supplied key is unknown or the found value is not a valid
		///   integer. Only non-negative integer values are recognized as
		///   valid. 
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->
	
		public int GetIntegerValue(string key, int defaultValue)
		{
			int result = defaultValue;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				value = value.Trim();
				if (IsValidInteger(value))
				{
					try
					{
						result = Int32.Parse(value);
					}
					catch (OverflowException)
					{
						/* return default */
					}
				}
			}

			return result;
		}

		private static bool IsValidInteger(string value)
		{
			if (value != null && value.Length > 0)
			{
				foreach (char c in value)
				{
					if (!Char.IsDigit(c))
					{
						return false;
					}
				}

				return true;
			}
			else 
			{
				return false;
			}
		}

		/// <summary>
		///   Returns a value of an element converted to a bool for a
		///   given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to a bool for the given key if an
		///   element with the given key exists or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns a bool value of true if the found value
		///   of the given key matches either "true", "1" or "yes" and false
		///   otherwise. If the supplied key is unknown, the defaultValue
		///   argument is returned.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->
	
		public bool GetBooleanValue(string key, bool defaultValue)
		{
			bool result = defaultValue;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				switch (value.ToLower().Trim())
				{
					case "1":
					case "yes":
					case "true":
						result = true;
						break;
					default:
						result = false;
						break;
				}
			}

			return result;
		}

		/// <summary>
		///   Returns a value of an element converted to a Level value for
		///   a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to the corresponding Level value for
		///   the given key if an element with the given key exists and the
		///   found value is a valid Level value or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns the defaultValue argument if either the
		///   supplied key is unknown or the found value is not a valid Level
		///   value. Please see the Level enum for more information on the
		///   available values.  
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public Level GetLevelValue(string key, Level defaultValue)
		{
			Level result = defaultValue;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				try
				{
					value = value.ToLower().Trim();
					result = (Level)Enum.Parse(typeof(Level), value, true);
				}
				catch (ArgumentException)
				{
					// Ignore this exception and simply return the
					// defaultValue parameter to the caller.
				}
			}

			return result;
		}

		private static bool IsValidSizeUnit(string u)
		{
			return u.Equals("kb") || u.Equals("mb") || u.Equals("gb");
		}

		/// <summary>
		///   Returns a value of an element converted to an integer for a
		///   given key. The integer value is interpreted as a byte size and
		///   it is supported to specify byte units.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to an integer for the given key if
		///   an element with the given key exists and the found value is a
		///   valid integer or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns the defaultValue argument if either the
		///   supplied key is unknown or the found value is not a valid
		///   integer or ends with an unknown byte unit. Only non-negative
		///   integer values are recognized as valid.
		///
		///   It is possible to specify a size unit at the end of the value.
		///   If a known unit is found, this function multiplies the
		///   resulting value with the corresponding factor. For example, if
		///   the value of the element is "1KB", the return value of this
		///   function would be 1024.
		///
		///   The following table lists the available units together with a
		///   short description and the corresponding factor.
		///
		///   <table>
		///   Unit Name  Description  Factor
		///   +          +            +
		///   KB         Kilo Byte    1024
		///   MB         Mega Byte    1024^2
		///   GB         Giga Byte    1024^3
		///   </table>
		///
		///   If no unit is specified, this function defaults to the KB
		///   unit.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public long GetSizeValue(string key, long defaultValue)
		{
			long result = defaultValue * KB_FACTOR;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				int factor = KB_FACTOR;
				value = value.Trim();

				if (value.Length >= 2)
				{
					string unit =
						value.Substring(value.Length - 2).ToLower();

					if (IsValidSizeUnit(unit))
					{
						value = value.Substring(0, value.Length - 2).Trim();
						switch (unit)
						{
							case "kb": factor = KB_FACTOR; break;
							case "mb": factor = MB_FACTOR; break;
							case "gb": factor = GB_FACTOR; break;
						}
					}
				}

				if (IsValidInteger(value))
				{
					try
					{
						result = factor * Int64.Parse(value);
					}
					catch (OverflowException)					
					{ 
						/* return default */
					}
				}
			}

			return result;
		}

		private static bool IsValidTimespanUnit(string u)
		{
			return u.Equals("s") || u.Equals("m") || u.Equals("h") ||
				u.Equals("d");
		}

		/// <summary>
		///   Returns a value of an element converted to an integer for a
		///   given key. The integer value is interpreted as a time span
		///   and it is supported to specify time span units.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to an integer for the given key if
		///   an element with the given key exists and the found value is a
		///   valid integer or defaultValue otherwise. The value is returned
		///   in milliseconds.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns the defaultValue argument if either the
		///   supplied key is unknown or the found value is not a valid
		///   integer or ends with an unknown time span unit.
		///
		///   It is possible to specify a time span unit at the end of the
		///   value. If a known unit is found, this function multiplies the
		///   resulting value with the corresponding factor. For example, if
		///   the value of the element is "1s", the return value of this
		///   function would be 1000.
		///
		///   The following table lists the available units together with a
		///   short description and the corresponding factor.
		///
		///   <table>
		///   Unit Name  Description  Factor
		///   +          +            +
		///   s          Seconds      1000
		///   m          Minutes      60*s
		///   h          Hours        60*m
		///   d          Days         24*h
		///   </table>
		///
		///   If no unit is specified, this function defaults to the Seconds
		///   unit. Please note that the value is always returned in
		///   milliseconds.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public long GetTimespanValue(string key, long defaultValue)
		{
			long result = defaultValue * SECONDS_FACTOR;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				int factor = SECONDS_FACTOR;
				value = value.Trim();

				if (value.Length >= 1)
				{
					string unit =
						value.Substring(value.Length - 1).ToLower();

					if (IsValidTimespanUnit(unit))
					{
						value = value.Substring(0, value.Length - 1).Trim();
						switch (unit)
						{
							case "s": factor = SECONDS_FACTOR; break;
							case "m": factor = MINUTES_FACTOR; break;
							case "h": factor = HOURS_FACTOR; break;
							case "d": factor = DAYS_FACTOR; break;
						}
					}
				}

				if (IsValidInteger(value))
				{
					try
					{
						result = factor * Int64.Parse(value);
					}
					catch (OverflowException)
					{
						/* return default */
					}
				}
			}

			return result;
		}

		/// <summary>
		///   Returns a Color value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown or if the
		///   found value has an invalid format.
		/// </param>
		/// <returns>
		///   Either the value converted to a Color value for the given key
		///   if an element with the given key exists and the found value
		///   has a valid format or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   The element value must be specified as hexadecimal string.
		///   To indicate that the element value represents a hexadecimal
		///   string, the element value must begin with "0x", "&H" or "$".
		///   A '0' nibble is appended if the hexadecimal string has an odd
		///   length.
		/// 
		///   The hexadecimal value must represent a three or four byte
		///   integer value. The hexadecimal value is handled as follows.
		///   
		///   <table>
		///   Bytes          Format
		///   +              +
		///   3              RRGGBB
		///   4              AARRGGBB
		///   Other          Ignored
		///   </table>
		/// 
		///   A stands for the alpha channel and R, G and B represent the
		///   red, green and blue channels, respectively. If the value is not
		///   given as hexadecimal value with a length of 6 or 8 characters
		///   excluding the hexadecimal prefix identifier or if the value
		///   does not have a valid hexadecimal format, this method returns
		///   defaultValue.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public Color GetColorValue(string key, Color defaultValue)
		{
			string value = GetStringValue(key, null);

			if (value != null)
			{
				byte[] b = ConvertHexValue(value.Trim());

				if (b == null)
				{
					return defaultValue; /* Invalid hex format */
				}

				switch (b.Length)
				{
					case 3:
						return Color.FromArgb(b[0], b[1], b[2]);

					case 4:
						return Color.FromArgb(b[0], b[1], b[2], b[3]);
				}
			}

			return defaultValue;
		}

		/// <summary>
		///   Returns a value of an element converted to a FileRotate
		///   value for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to a FileRotate value for the
		///   given key if an element with the given key exists and the found
		///   value is a valid FileRotate or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public FileRotate GetRotateValue(string key, FileRotate defaultValue)
		{
			FileRotate result = defaultValue;
			string value = GetStringValue(key, null);

			if (value != null)
			{
				try
				{
					value = value.ToLower().Trim();
					result = (FileRotate) 
						Enum.Parse(typeof(FileRotate), value, true);
				}
				catch (ArgumentException)
				{
					// Ignore this exception and simply return the
					// defaultValue parameter to the caller.
				}
			}

			return result;
		}

		/// <summary>
		///   Returns a byte array value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="size">
		///   The desired size in bytes of the returned byte array. If
		///   the element value does not have the expected size, it is
		///   shortened or padded automatically.
		/// </param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown or if the
		///   found value has an invalid format.
		/// </param>
		/// <returns>
		///   Either the value converted to a byte array for the given key
		///   if an element with the given key exists and the found value
		///   has a valid format or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   The returned byte array always has the desired length as
		///   specified by the size argument. If the element value does
		///   not have the required size after conversion, it is shortened
		///   or padded (with zeros) automatically. This method returns
		///   the defaultValue argument if either the supplied key is
		///   unknown or the found value does not have a valid format
		///   (e.g. invalid characters when using hexadecimal strings).
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public byte[] GetBytesValue(string key, int size,
			byte[] defaultValue)
		{
			string value = GetStringValue(key, null);

			if (value != null)
			{
				byte[] b = ConvertUnicodeValue(value.Trim());

				if (b == null)
				{
					return defaultValue; /* Invalid hex format */
				}
				else if (b.Length == size)
				{
					return b;
				}

				byte[] r = new byte[size]; /* Automatically zero'd */

				if (b.Length > size)
				{
					Buffer.BlockCopy(b, 0, r, 0, r.Length);
				}
				else
				{
					Buffer.BlockCopy(b, 0, r, 0, b.Length);
				}

				return r;
			}

			return defaultValue;
		}

		private static string[] HEX_ID = 
		{ 
			"0x", /* C# and Java */
			"&H", /* Visual Basic .NET */
			"$"   /* Object Pascal */
		};

		private static byte[] ConvertHexValue(string value)
		{
			/* Hexadecimal format? */
			foreach (string id in HEX_ID)
			{
				if (value.StartsWith(id))
				{
					value = value.Substring(id.Length);
					return ConvertHexString(value);
				}
			}

			return null;
		}

		private static byte[] ConvertUnicodeValue(string value)
		{
			/* Normal Unicode string encoded in UTF-8 */
			return Encoding.UTF8.GetBytes(value);
		}

		private static byte[] HEX_TBL = 
		{
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
			0x08, 0x09, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff
		};

		private static bool IsValidHex(string value)
		{
			foreach (char c in value)
			{
				if (c >= HEX_TBL.Length || HEX_TBL[c] > 0x0f)
				{
					return false;
				}
			}

			return true;
		}

		private static byte[] ConvertHexString(string value)
		{
			value = value.ToUpper();

			if ((value.Length & 1) != 0) /* Odd? */
			{
				value = value + "0";
			}

			byte[] b = null;

			if (IsValidHex(value))
			{
				b = new byte[value.Length / 2];

				for (int i = 0; i < b.Length; i++)
				{
					byte hi = HEX_TBL[value[i << 1]];
					byte lo = HEX_TBL[value[(i << 1) + 1]];
					b[i] = (byte) (hi << 4 | lo);
				}
			}

			return b;
		}
	}
}
