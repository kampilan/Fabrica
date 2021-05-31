//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Assists in building a SmartInspect connections string.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The ConnectionsBuilder class assists in creating connections
	///   strings as used by the SmartInspect.Connections property. To
	///   get started, please have a look at the following example. For
	///   general information about connections strings, please refer to
	///   the SmartInspect.Connections property.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// <example>
	/// <code>
	/// // [C# Example]
	/// 
	/// ConnectionsBuilder builder = new ConnectionsBuilder();
	/// builder.BeginProtocol("file");
	/// builder.AddOption("filename", "log.sil");
	/// builder.AddOption("append", true);
	/// builder.EndProtocol();
	/// SiAuto.Si.Connections = builder.Connections;
	/// </code>
	/// 
	/// <code>
	/// ' [VB.NET Example]
	/// 
	/// Dim builder As ConnectionsBuilder = New ConnectionsBuilder()
	/// builder.BeginProtocol("file")
	/// builder.AddOption("filename", "log.sil")
	/// builder.AddOption("append", True)
	/// builder.EndProtocol()
	/// SiAuto.Si.Connections = builder.Connections
	/// </code>
	/// </example>
	/// -->

	public class ConnectionsBuilder
	{
		private bool fHasOptions;
		private StringBuilder fBuilder;

		/// <summary>
		///   Creates and initializes a ConnectionsBuilder instance.
		/// </summary>

		public ConnectionsBuilder()
		{
			this.fBuilder = new StringBuilder();
		}

		/// <summary>
		///   Clears this ConnectionsBuilder instance by removing all
		///   protocols and their options.
		/// </summary>
		/// <!--
		/// <remarks>
		///   After this method has been called, the Connections property
		///   returns an empty string.
		/// </remarks>
		/// -->

		public void Clear()
		{
			this.fBuilder.Length = 0;
		}

		/// <summary>
		///   Begins a new protocol section.
		/// </summary>
		/// <param name="protocol">The name of the new protocol.</param>
		/// <!--
		/// <remarks>
		///   This method begins a new protocol with the supplied name.
		///   All subsequent protocol options are added to this protocol
		///   until the new protocol section is closed by calling the
		///   EndProtocol method.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The protocol argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void BeginProtocol(string protocol)
		{
			if (protocol == null)
			{
				throw new ArgumentNullException("protocol");
			}
			else 
			{
				if (this.fBuilder.Length != 0)
				{
					this.fBuilder.Append(", ");
				}

				this.fBuilder.Append(protocol);
				this.fBuilder.Append("(");
				this.fHasOptions = false;
			}
		}

		/// <summary>
		///   Ends the current protocol section.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method ends the current protocol. To begin a new protocol
		///   section, use the BeginProtocol method.
		/// </remarks>
		/// -->

		public void EndProtocol()
		{
			this.fBuilder.Append(")");
		}

		private string Escape(string value)
		{
			return value.Replace("\"", "\"\"");
		}

		/// <summary>
		///   Overloaded. Adds a new string option to the current protocol
		///   section.
		/// </summary>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new string option to the current protocol
		///   section. The supplied value argument is properly escaped if
		///   necessary.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The key or value argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AddOption(string key, string value)
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
				if (this.fHasOptions)
				{
					this.fBuilder.Append(", ");
				}

				this.fBuilder.Append(key);
				this.fBuilder.Append("=\"");
				this.fBuilder.Append(Escape(value));
				this.fBuilder.Append("\"");

				this.fHasOptions = true;
			}
		}

		/// <summary>
		///   Overloaded. Adds a new boolean option to the current protocol
		///   section.
		/// </summary>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new bool option to the current protocol
		///   section.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AddOption(string key, bool value)
		{
			AddOption(key, value ? "true" : "false");
		}

		/// <summary>
		///   Overloaded. Adds a new integer option to the current protocol
		///   section.
		/// </summary>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new int option to the current protocol
		///   section.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AddOption(string key, int value)
		{
			AddOption(key, Convert.ToString(value));
		}

		/// <summary>
		///   Overloaded. Adds a new Level option to the current protocol
		///   section.
		/// </summary>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new Level option to the current protocol
		///   section.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AddOption(string key, Level value)
		{
			AddOption(key, value.ToString().ToLower());
		}

		/// <summary>
		///   Overloaded. Adds a new FileRotate option to the current
		///   protocol section.
		/// </summary>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>
		/// <!--
		/// <remarks>
		///   This method adds a new FileRotate option to the current
		///   protocol section.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentNullException    The key argument is null.
		/// </table>
		/// </exception>
		/// -->

		public void AddOption(string key, FileRotate value)
		{
			AddOption(key, value.ToString().ToLower());
		}

		/// <summary>
		///   Returns the built connections string.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This read-only property returns the connections string which
		///   has previously been built with the BeginProtocol, AddOption
		///   and EndProtocol methods.
		/// </remarks>
		/// -->

		public string Connections
		{
			get { return this.fBuilder.ToString(); }
		}
	}
}
