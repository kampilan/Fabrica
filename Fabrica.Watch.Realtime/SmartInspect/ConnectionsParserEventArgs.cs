//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the ConnectionsParser.Parse method.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by the ConnectionsParser class to inform
	///   interested parties about found protocols and options. It offers
	///   the necessary properties to retrieve the found protocols and
	///   options in the event handlers.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class ConnectionsParserEventArgs: System.EventArgs
	{
		private string fProtocol;
		private string fOptions;

		/// <summary>
		///   Creates and initializes a new ConnectionsParserEventArgs
		///   instance.
		/// </summary>
		/// <param name="protocol">The protocol which has been found.</param>
		/// <param name="options">The options of the new protocol.</param>

		public ConnectionsParserEventArgs(string protocol, string options)
		{
			this.fProtocol = protocol;
			this.fOptions = options;
		}

		/// <summary>
		///   This read-only property returns the protocol which has just
		///   been found by a ConnectionsParser object.
		/// </summary>

		public string Protocol
		{
			get { return this.fProtocol; }
		}

		/// <summary>
		///   This read-only property returns the related options for the
		///   protocol which has just been found by a ConnectionsParser
		///   object.
		/// </summary>

		public string Options		
		{
			get { return this.fOptions; }
		}
	}
}
