//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Net;
using System.Net.Sockets;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used in the TcpProtocol class for the low-level socket
	///   connection. Provides the needed connection timeout.
	/// </summary>
	/// <threadsafety>
	///   This class is not threadsafe.
	/// </threadsafety>

	internal class TcpSocket: System.IDisposable
	{
		private IPEndPoint fEndPoint;
		private Socket fSocket;
		private int fTimeout;

		private const string
			INVALID_PORT = "Invalid port argument",
			INVALID_TIMEOUT = "Invalid timeout argument";

		/// <summary>
		///   Creates and initializes a TcpSocket instance.
		/// </summary>
		/// <param name="host">The name of the host to use.</param>
		/// <param name="port">The port of the TCP server to use.</param>
		/// <!--
		/// <remarks>
		///   The constructor tries to resolve the supplied hostname
		///   and creates a TCP socket with a default send and receive
		///   <link TcpSocket.Timeout, timeout> of 30 seconds.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   ArgumentNullException  The host argument is null.
		///   ArgumentException      The port argument is less than 0.
		///   SocketException        Resolving the host failed.
		///   
		///   SecurityException      The caller does not have the required
		///                            permission to access DNS information
		///                            to resolve the host.
		/// </table>
		/// </exception>
		/// -->

		public TcpSocket(string host, int port)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			else if (port < 0)
			{
				throw new ArgumentException(INVALID_PORT);
			}
			else
			{
				IPAddress ip = LookupHost(host);
				this.fEndPoint = new IPEndPoint(ip, port);

				// Create the tcp socket.
				this.fSocket = new Socket(
						AddressFamily.InterNetwork, 
						SocketType.Stream, ProtocolType.Tcp
					);

				// Disable nagle algorithm
				this.fSocket.SetSocketOption(
						SocketOptionLevel.Tcp, 
						SocketOptionName.NoDelay, 1
					);

				// Set the default timeout.
				Timeout = 30000;
			}
		}

		private IPAddress LookupHost(string host)
		{
			IPAddress ip = null;
			IPAddress[] addresses = null;

#if (SI_DOTNET_1x)
			addresses = Dns.Resolve(host).AddressList;
#else
			if (!IPAddress.TryParse(host, out ip))
			{
				addresses = Dns.GetHostEntry(host).AddressList;
			}
#endif

			if (ip == null && addresses != null)
			{
				// Try to find an IPv4 IP address
				for (int i = 0; i < addresses.Length; i++)
				{
					if (addresses[i].AddressFamily == 
						AddressFamily.InterNetwork)
					{
						ip = addresses[i];
					}
				}
			}

			return ip;
		}

		/// <summary>
		///   Tries to connect to the host and port passed to the
		///   TcpSocket constructor.
		/// </summary>
		/// <param name="timeout">
		///   The maximum time to wait until this method returns,
		///   even if the socket connection could not be established
		///   successfully.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method tries to connect to the host and port passed
		///   to the class constructor using the supplied timeout.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ArgumentException        The supplied timeout is less than 0.
		///   ObjectDisposedException  The underlying socket has been closed.
		///   SocketException          Connecting to the remote server failed.
		///   
		///   SecurityException        The caller does not have the required
		///                              permission to connect to a remote
		///                              server.
		///                            
		///   SmartInspectException    The specified timeout has been reached
		///                              and the connection could not be
		///                              established successfully.
		/// </table>
		/// </exception>
		/// -->

		public void Connect(int timeout)
		{
			if (timeout < 0)
			{
				// A timeout with a value less than 0 isn't allowed.
				throw new ArgumentException(INVALID_TIMEOUT);
			}
			else 
			{
				IAsyncResult ar = this.fSocket.BeginConnect(
						this.fEndPoint, null, this.fSocket
					);

				ar.AsyncWaitHandle.WaitOne(timeout, false);

				if (ar.IsCompleted) 
				{
					this.fSocket.EndConnect(ar);
				}
				else
				{
					throw new SmartInspectException(
						"The connection attempt failed, " + 
						"because the specified timeout has been reached."
					);
				}
			}
		}

		/// <summary>
		///   Returns the connection status of the underlying socket.
		/// </summary>

		public bool Connected
		{
			get { return this.fSocket.Connected; }
		}

		/// <summary>
		///   Closes the underlying socket.
		/// </summary>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ObjectDisposedException  The underlying socket has been
		///                              already closed.
		///                              
		///   SocketException          Shutting the underlying socket down
		///                              before closing it failed.
		/// </table>
		/// </exception>
		/// -->

		public void Close()
		{
			try 
			{
				if (Connected) 
				{
					// Shutdown the socket properly.
					this.fSocket.Shutdown(SocketShutdown.Both);
				}
			}
			finally 
			{
				this.fSocket.Close();
			}
		}

		/// <summary>
		///   Creates a NetworkStream instance using the underlying
		///   socket.
		/// </summary>
		/// <returns>The created NetworkStream instance.</returns>

		public NetworkStream GetStream()
		{
			return new NetworkStream(this.fSocket);
		}

		/// <summary>
		///   Gets or sets the send and receive timeout in milliseconds.
		///   The default timeout is 30 seconds.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Timeout values less than 0 are ignored.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ObjectDisposedException  The underlying socket has been
		///                              already closed.
		///                              
		///   SocketException          Setting the send and receive
		///                              timeout of the underlying socket
		///                              failed.
		/// </table>
		/// </exception>
		/// -->

		public int Timeout
		{
			get { return this.fTimeout; }

			set
			{
				if (value >= 0)
				{
					// Set the send timeout.
					this.fSocket.SetSocketOption(
							SocketOptionLevel.Socket, 
							SocketOptionName.SendTimeout, value
						);

					// Set the receive timeout.
					this.fSocket.SetSocketOption(
							SocketOptionLevel.Socket,
							SocketOptionName.ReceiveTimeout, value
						);

					this.fTimeout = value;
				}
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Close the underlying socket if disposing is set
				// to true.
				Close();
			}
		}

		/// <summary>
		///   Closes the underlying socket.
		/// </summary>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type           Condition
		///   +                        +
		///   ObjectDisposedException  The underlying socket has been
		///                              already closed.
		///                              
		///   SocketException          Shutting the underlying socket down
		///                              before closing it failed.
		/// </table>
		/// </exception>
		/// -->

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
