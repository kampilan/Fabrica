//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the OptionsParser.Parse method.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by the OptionsParser class to inform
	///   interested parties about found options. It offers the necessary
	///   properties to retrieve the found options in the event handlers.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class OptionsParserEventArgs: System.EventArgs
	{
		private string fProtocol;
		private string fKey;
		private string fValue;

		/// <summary>
		///   Creates and initializes a new OptionsParserEventArgs
		///   instance.
		/// </summary>
		/// <param name="protocol">The protocol of the new option.</param>
		/// <param name="key">The key of the new option.</param>
		/// <param name="value">The value of the new option.</param>

		public OptionsParserEventArgs(string protocol, string key, 
			string value)
		{
			this.fProtocol = protocol;
			this.fKey = key;
			this.fValue = value;
		}

		/// <summary>
		///   This read-only property returns the protocol of the option
		///   which has just been found by a OptionsParser object.
		/// </summary>

		public string Protocol
		{
			get { return this.fProtocol; }
		}

		/// <summary>
		///   This read-only property returns the key of the option which
		///   has just been found by a OptionsParser object.
		/// </summary>

		public string Key
		{
			get { return this.fKey; }
		}

		/// <summary>
		///   This read-only property returns the value of the option which
		///   has just been found by a OptionsParser object.
		/// </summary>

		public string Value
		{
			get { return this.fValue; }
		}
	}
}
