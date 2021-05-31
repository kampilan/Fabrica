//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used to report any errors concerning the protocol classes.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This exception can be thrown by several Protocol methods
	///   like the Protocol.Connect, Protocol.Disconnect or
	///   Protocol.WritePacket methods when an error has occurred.
	///   
	///   See below for an example on how to obtain detailed information
	///   in the SmartInspect.Error event about the protocol which caused
	///   the error.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// <example>
	/// <code>
	/// // [C# Example]
	/// 
	/// using System;
	/// using Gurock.SmartInspect;
	///
	/// public class ErrorHandling
	/// {
	///		public static void EventHandler(object sender, ErrorEventArgs args)
	///		{
	///			Console.WriteLine(args.Exception);
	///
	///			if (args.Exception is ProtocolException)
	///			{
	///				ProtocolException pe = (ProtocolException) args.Exception;
	///
	///				// A ProtocolException provides additional information
	///				// about the occurred error besides the normal exception
	///				// message, like, for example, the name of the protocol
	///				// which caused this error.
	///
	///				Console.WriteLine(pe.ProtocolName);
	///				Console.WriteLine(pe.ProtocolOptions);
	///			}
	///		}
	///
	///		public static void Main(string[] args)
	///		{
	///			// Register our event handler for the error event.
	///			SiAuto.Si.Error += new ErrorEventHandler(EventHandler);
	///
	///			// And force a connection error.
	///			SiAuto.Si.Connections = @"file(filename=c:\\)";
	///			SiAuto.Si.Enabled = true;
	///		}
	/// }
	/// </code>
	/// 
	/// <code>
	/// ' [VB.NET Example]
	/// 
	/// Imports System
	/// Imports Gurock.SmartInspect
	///
	/// Module ErrorHandling
	///		Sub EventHandler(ByVal Sender As Object, ByVal Args As ErrorEventArgs)
	///			Console.WriteLine(Args.Exception)
	///
	///			If (TypeOf Args.Exception Is ProtocolException) Then
	///				Dim pe As ProtocolException = Args.Exception
	///
	///				' A ProtocolException provides additional information
	///				' about the occurred error besides the normal exception
	///				' message, like, for example, the name of the protocol
	///				' which caused this error.
	///
	///				Console.WriteLine(pe.ProtocolName)
	///				Console.WriteLine(pe.ProtocolOptions)
	///			End If
	///		End Sub
	///
	///		Sub Main()
	///			' Register our event handler for the error event.
	///			AddHandler SiAuto.Si.Error, AddressOf EventHandler
	///
	///			' And force a connection error.
	///			SiAuto.Si.Connections = "file(filename=c:\\)"
	///			SiAuto.Si.Enabled = True
	///		End Sub
	/// End Module
	/// </code>
	/// </example>
	/// -->

	[Serializable]
	public sealed class ProtocolException: SmartInspectException
	{
		private String fProtocolName;
		private String fProtocolOptions;

		/// <summary>
		///   Overloaded. Creates and initializes a ProtocolException
		///   instance.
		/// </summary>

		public ProtocolException(): base()
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ProtocolException
		///   instance with a custom error message.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>

		public ProtocolException(string message): base(message)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a ProtocolException
		///   instance with a custom error message and an inner exception.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>
		/// <param name="inner">
		///   The inner exception which is the cause for this exception.
		/// </param>

		public ProtocolException(string message, Exception inner):
			base(message, inner)
		{
		}

		private ProtocolException(SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				// Load protocol properties accordingly.
				this.fProtocolName = info.GetString("fProtocolName");
				this.fProtocolOptions = info.GetString("fProtocolOptions");
			}
		}

		/// <summary>
		///   Overridden. Supplies serialization data about this
		///   ProtocolException instance.
		/// </summary>
		/// <param name="info">Holds the serialized data.</param>
		/// <param name="context">
		///   Contains information about the source or destination.
		/// </param>

		[SecurityPermission(SecurityAction.Demand,
			 SerializationFormatter=true)]

		public override void GetObjectData(SerializationInfo info,
			StreamingContext context)
		{
			base.GetObjectData(info, context);

			if (info != null)
			{
				// Add our protocol properties to the serialization info.
				info.AddValue("fProtocolName", this.fProtocolName);
				info.AddValue("fProtocolOptions", this.fProtocolOptions);
			}
		}

		/// <summary>
		///   Represents the name of the protocol which threw
		///   this exception. A possible value would be "tcp".
		/// </summary>

		public String ProtocolName
		{
			get { return this.fProtocolName; }
			set { this.fProtocolName = value; }
		}

		/// <summary>
		///   Represents the options of the protocol which threw
		///   this exception. Can be empty if not set.
		/// </summary>

		public String ProtocolOptions
		{
			get { return this.fProtocolOptions; }
			set { this.fProtocolOptions = value; }
		}
	}
}
