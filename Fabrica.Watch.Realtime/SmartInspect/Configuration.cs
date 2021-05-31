//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;
using System.IO;
using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for handling the SmartInspect configuration and loading
	///   it from a file.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is responsible for loading and reading values from a
	///   SmartInspect configuration file. For more information, please refer
	///   to the SmartInspect.LoadConfiguration method.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class Configuration
	{
		private LookupTable fItems;
		private IList fKeys;

		/// <summary>
		///   Creates and initializes a new Configuration instance.
		/// </summary>

		public Configuration()
		{
			this.fItems = new LookupTable();
			this.fKeys = new ArrayList();
		}

		/// <summary>
		///   Loads the configuration from a file.
		/// </summary>
		/// <param name="fileName">
		///   The name of the file to load the configuration from.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method loads key/value pairs separated with a '='
		///   character from a file. Empty, unrecognized lines or lines
		///   beginning with a ';' character are ignored.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   IOException             An I/O error occurred while trying
		///                            to load the configuration or if the
		///                            specified file does not exist.
		///   ArgumentNullException   The fileName argument is null.
		/// </table>
		/// </exception>	
		/// -->
	
		public void LoadFromFile(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			StreamReader r = new StreamReader(fileName);
			try
			{
				Clear();
				string line;

				while ( (line = r.ReadLine()) != null)
				{
					line = line.Trim();
					if (line.Length > 0 && !line.StartsWith(";"))
					{
						Parse(line);
					}
				}
			}
			finally
			{
				r.Close();
			}
		}

		private void Parse(string pair)
		{
			int index = pair.IndexOf('=');

			if (index == -1)
			{
				return;
			}

			string key = pair.Substring(0, index).Trim();
			string value = pair.Substring(index + 1).Trim();

			if (!this.fItems.Contains(key))
			{
				this.fKeys.Add(key);
			}

			this.fItems.Put(key, value);
		}

		/// <summary>
		///   Tests if the configuration contains a value for a given key. 
		/// </summary>
		/// <param name="key">The key to test for.</param>
		/// <returns>
		///   True if a value exists for the given key and false otherwise.
		/// </returns>
	
		public bool Contains(string key)
		{
			return this.fItems.Contains(key);
		}

		/// <summary>
		///   Removes all key/value pairs of the configuration.
		/// </summary>
	
		public void Clear()
		{
			this.fKeys.Clear();
			this.fItems.Clear();
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

		public Color ReadColor(string key, Color defaultValue)
		{
			return this.fItems.GetColorValue(key, defaultValue);
		}

		/// <summary>
		///   Returns a string value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value for a given key if an element with the 
		///   given key exists or defaultValue otherwise.
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
	
		public string ReadString(string key, string defaultValue)
		{
			return this.fItems.GetStringValue(key, defaultValue);
		}

		/// <summary>
		///   Returns a Level value of an element for a given key.
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
	
		public Level ReadLevel(string key, Level defaultValue)
		{
			return this.fItems.GetLevelValue(key, defaultValue);
		}

		/// <summary>
		///   Returns a boolean value of an element for a given key.
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
	
		public bool ReadBoolean(string key, bool defaultValue)
		{
			return this.fItems.GetBooleanValue(key, defaultValue);
		}

		/// <summary>
		///   Returns an integer value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <param name="defaultValue">
		///   The value to return if the given key is unknown.
		/// </param>
		/// <returns>
		///   Either the value converted to an int for the given key if an
		///   element with the given key exists and the found value is a
		///   valid int or defaultValue otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns the defaultValue argument if either the
		///   supplied key is unknown or the found value is not a valid int.
		///   Only non-negative int values are recognized as valid. 
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The key argument is null.
		/// </table>
		/// </exception>
		/// -->
	
		public int ReadInteger(string key, int defaultValue)
		{
			return this.fItems.GetIntegerValue(key, defaultValue);
		}

		/// <summary>
		///   Returns a key of this SmartInspect configuration for a
		///   given index.
		/// </summary>
		/// <param name="index">
		///   The index in this SmartInspect configuration.
		/// </param>
		/// <returns>
		///   A key of this SmartInspect configuration for the given index.
		/// </returns>
		/// <!--
		/// <remarks>
		///   To find out the total number of key/value pairs in this
		///   SmartInspect configuration, use Count. To get the value for
		///   a given key, use ReadString.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type               Condition
		///   +                            +
		///   ArgumentOutOfRangeException  The index argument is not a valid
		///                                 index of this SmartInspect
		///                                 configuration.
		/// </table>
		/// </exception>
		/// -->

		public string ReadKey(int index)
		{
			return (string) this.fKeys[index];
		}

		/// <summary>
		///   Returns the number of key/value pairs of this SmartInspect
		///   configuration.
		/// </summary>

		public int Count
		{
			get { return this.fItems.Count; }
		}
	}
}
