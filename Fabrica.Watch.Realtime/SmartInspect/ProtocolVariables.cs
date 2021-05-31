//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Manages connection variables.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class manages a list of connection variables. Connection
	///   variables are placeholders for strings in the
	///   <link SmartInspect.Connections, connections string> of the
	///   SmartInspect class. Please see SmartInspect.SetVariable for
	///   more information.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class ProtocolVariables
	{
		private IDictionary fItems;
		private object fLock;

		/// <summary>
		///   Creates and initializes a new ProtocolVariables instance.
		/// </summary>

		public ProtocolVariables()
		{
#if SI_DOTNET_1x
			this.fItems = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
#else
			this.fItems = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);
#endif
			this.fLock = new object();
		}

		/// <summary>
		///   Adds or updates an element with a specified key and value
		///   to the set of connection variables.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		/// <param name="value">The value of the element.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new element with a given key and value to
		///   the set of connection variables. If an element for the given
		///   key already exists, the original element's value is updated.
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
				lock (this.fLock)
				{
					this.fItems[key] = value;
				}
			}
		}

		/// <summary>
		///   Adds a new element with a specified key and value to the
		///   set of connection variables.
		/// </summary>
		/// <param name="key">The key of the element.</param>
		/// <param name="value">The value of the element.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new element with a given key and value to
		///   the set of connection variables. If an element for the given
		///   key already exists, the original element's value is not
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

		public void Add(string key, string value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			lock (this.fLock)
			{
				if (!this.fItems.Contains(key))
				{
					Put(key, value);
				}
			}
		}

		/// <summary>
		///   Removes an existing element with a given key from this set
		///   of connection variables.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <!--
		/// <remarks>
		///  This method removes the element with the given key from the
		///  internal set of connection variables. Nothing happens if no
		///  element with the given key can be found.
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
				lock (this.fLock)
				{
					this.fItems.Remove(key);
				}
			}
		}

		/// <summary>
		///   Tests if the collection contains a value for a given key. 
		/// </summary>
		/// <param name="key">The key to test for.</param>
		/// <returns>
		///   True if a value exists for the given key and false
		///   otherwise.
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
				lock (this.fLock)
				{
					return this.fItems.Contains(key);
				}
			}
		}

		/// <summary>
		///   Removes all key/value pairs of the collection.
		/// </summary>

		public void Clear()
		{
			lock (this.fLock)
			{
				this.fItems.Clear();
			}
		}

		/// <summary>
		///   Expands and returns a connections string.
		/// </summary>
		/// <param name="connections">
		///	  The connections string to expand and return.
		/// </param>
		/// <returns>The expanded connections string.</returns>
		/// <!--
		/// <remarks>
		///  This method replaces all variables which have previously
		///  been added to this collection (with Add or Put) in the
		///  given connections string with their respective values and
		///  then returns it. Variables in the connections string must
		///  have the following form: $variable$.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type          Condition
		///   +                       +
		///   ArgumentNullException   The connections argument is null.
		/// </table>
		/// </exception>
		/// -->

		public string Expand(string connections)
		{
			if (connections == null)
			{
				throw new ArgumentNullException("connections");
			}

			lock (this.fLock)
			{
				if (this.fItems.Count == 0)
				{
					return connections;
				}

				foreach (DictionaryEntry e in this.fItems)
				{
					string key = "$" + e.Key + "$";
					string value = (string)e.Value;
					connections = connections.Replace(key, value);
				}
			}

			return connections;
		}

		/// <summary>
		///   Returns the number of key/value pairs of this collection.
		/// </summary>

		public int Count
		{
			get 
			{
				lock (this.fLock)
				{
					return this.fItems.Count;
				}
			}
		}

		/// <summary>
		///   Returns a value of an element for a given key.
		/// </summary>
		/// <param name="key">The key whose value to return.</param>
		/// <returns>
		///   Either the value for a given key if an element with the
		///   given key exists or null otherwise.
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

		public string Get(string key)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			else
			{
				string value;

				lock (this.fLock)
				{
					value = this.fItems[key] as string;
				}

				return value;
			}
		}
	}
}
