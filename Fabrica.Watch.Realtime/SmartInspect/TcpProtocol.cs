//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System.IO;
using System.Text;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used for sending packets to the SmartInspect Console over a TCP
	///   socket connection.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used for sending packets over a TCP connection to
	///   the Console. It is used when the 'tcp' protocol is specified in
	///   the <link SmartInspect.Connections, connections string>. Please
	///   see the IsValidOption method for a list of available protocol
	///   options.
	/// </remarks>
	/// <threadsafety>
	///   The public members of this class are threadsafe.
	/// </threadsafety>
	/// -->

	public class TcpProtocol: Protocol 
	{
		private static readonly byte[] CLIENT_BANNER = 
			Encoding.ASCII.GetBytes("SmartInspect .NET Library v" +
			SmartInspect.Version + "\n");

		private const int ANSWER_SIZE = 2;

		private TcpSocket fSocket;
		private Stream fStream;
		private Formatter fFormatter;
		private byte[] fAnswer;

		private int fTimeout = 30000;
		private string fHostName = "127.0.0.1";
		private int fPort = 4228;

		/// <summary>
		///   Creates and initializes a TcpProtocol instance. For a list
		///   of available TCP protocol options, please refer to the
		///   IsValidOption method.
		/// </summary>

		public TcpProtocol()
		{
			this.fAnswer = new byte[ANSWER_SIZE];
			this.fFormatter = new BinaryFormatter();
			LoadOptions(); // Set default options
		}

		/// <summary>
		///   Overridden. Returns "tcp".
		/// </summary>

		protected override string Name
		{
			get { return "tcp"; }
		}

		/// <summary>
		///   Overridden. Validates if a protocol option is supported.
		/// </summary>
		/// <param name="name">The option name to validate.</param>
		/// <returns>
		///   True if the option is supported and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   The following table lists all valid options, their default
		///   values and descriptions for the TCP protocol.
		///   
		///   <table>
		///   Valid Options  Default Value  Description
		///   +              +              +
		///   host           "127.0.0.1"    Specifies the hostname where 
		///                                   the Console is running.
		///                                   
		///   port           4228           Specifies the Console port.
		///   
		///   timeout        30000          Specifies the connect, receive and
		///                                   send timeout in milliseconds.
		///   </table>
		///   
		///   For further options which affect the behavior of this protocol,
		///   please have a look at the documentation of the
		///   <link Protocol.IsValidOption, IsValidOption> method of the
		///   parent class.
		/// </remarks>
		/// <example>
		/// <code>
		/// SiAuto.Si.Connections = "tcp()";
		/// SiAuto.Si.Connections = "tcp(host=\\"localhost\\", port=4229)";
		/// SiAuto.Si.Connections = "tcp(timeout=2500)";
		/// </code>
		/// </example>
		/// -->

		protected override bool IsValidOption(string name)
		{
			return
				name.Equals("host") ||
				name.Equals("port") ||
				name.Equals("timeout") ||
				base.IsValidOption(name);
		}

		/// <summary>
		///   Overridden. Fills a ConnectionsBuilder instance with the
		///   options currently used by this TCP protocol.
		/// </summary>
		/// <param name="builder">
		///   The ConnectionsBuilder object to fill with the current options
		///   of this protocol.
		/// </param>

		protected override void BuildOptions(ConnectionsBuilder builder)
		{
			base.BuildOptions(builder);
			builder.AddOption("host", this.fHostName);
			builder.AddOption("port", this.fPort);
			builder.AddOption("timeout", this.fTimeout);
		}

		/// <summary>
		///   Overridden. Loads and inspects TCP specific options.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method loads all relevant options and ensures their
		///   correctness. See IsValidOption for a list of options which
		///   are recognized by the TCP protocol.
		/// </remarks>
		/// -->

		protected override void LoadOptions()
		{
			base.LoadOptions();
			this.fHostName = GetStringOption("host", "127.0.0.1");
			this.fTimeout = GetIntegerOption("timeout", 30000);
			this.fPort = GetIntegerOption("port", 4228);
		}

		private static void DoHandShake(Stream stream)
		{
			int n;
					
			// Read the server banner from the Console. 
			while ( (n = stream.ReadByte()) != '\n')
			{
				if (n == -1)
				{
					// This indicates a failure on the server
					// side. Doesn't make sense to proceed here.

					throw new SmartInspectException(
							"Could not read server banner correctly: " +
							"Connection has been closed unexpectedly"
						);
				}
			}
							
			// And write ours in return!
			stream.Write(CLIENT_BANNER, 0, CLIENT_BANNER.Length);
			stream.Flush();
		}

		/// <summary>
		///   Overridden. Creates and connects a TCP socket.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method tries to connect a TCP socket to a SmartInspect
		///   Console. The hostname and port can be specified by passing
		///   the "hostname" and "port" options to the Initialize method.
		///   Furthermore, it is possible to specify the connect timeout
		///   by using the "timeout" option.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type      Condition
		///   +                   +
		///   Exception           Creating or connecting the socket failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalConnect()
		{
			this.fSocket = new TcpSocket(this.fHostName, this.fPort);

			// Set send, receive timeout and connect.
			this.fSocket.Timeout = this.fTimeout;
			this.fSocket.Connect(this.fTimeout);

			this.fStream = new BufferedStream(
					this.fSocket.GetStream(), 0x2000
				);

			DoHandShake(this.fStream);
			InternalWriteLogHeader(); /* Write a log header */
		}

		/// <summary>
		///   Overridden. Sends a packet to the Console.
		/// </summary>
		/// <param name="packet">The packet to write.</param>
		/// <!--
		/// <remarks>
		///   This method sends the supplied packet to the SmartInspect
		///   Console and waits for a valid response.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type      Condition
		///   +                   +
		///   Exception           Sending the packet to the Console failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalWritePacket(Packet packet)
		{
			this.fFormatter.Format(packet, this.fStream);
			this.fStream.Flush();

			if (this.fStream.Read(this.fAnswer, 0, ANSWER_SIZE) != ANSWER_SIZE)
			{
				throw new SmartInspectException(
					"Could not read server answer correctly: " +
					"Connection has been closed unexpectedly"
				);
			}
		}

		/// <summary>
		///   Overridden. Closes the TCP socket connection.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method closes the underlying socket handle if previously
		///   created and disposes any supplemental objects.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   Exception              Closing the TCP socket failed.
		/// </table>
		/// </exception>
		/// -->

		protected override void InternalDisconnect()
		{
			if (this.fStream != null) 
			{
				this.fStream.Close();
				this.fStream = null;
			}
			
			if (this.fSocket != null) 
			{
				this.fSocket.Close();
				this.fSocket = null;
			}
		}
	}
}
