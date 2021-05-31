//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used to report errors concerning the SmartInspect.LoadConfiguration
	///   method.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This exception is used to report errors concerning the
	///   SmartInspect.LoadConfiguration method. This method is able to load
	///   the SmartInspect properties from a file. Therefore errors can occur
	///   when trying to load properties from an inexistent file or when the
	///   file can not be opened for reading, for example.
	///
	///   If such an error occurs, an instance of this class will be passed
	///   to the SmartInspect.Error event. Please note, that, if a connections
	///   string can be read while loading the configuration file, but is
	///   found to be invalid then this exception type will not be used. In
	///   this case, the SmartInspect.LoadConfiguration method will use the
	///   InvalidConnectionsException exception instead.
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
	///			if (args.Exception is LoadConfigurationException)
	///			{
	///				LoadConfigurationException le =
	///					(LoadConfigurationException) args.Exception;
	///
	///				// A LoadConfigurationException provides additional
	///				// information about the occurred error besides the
	///				// normal exception message. It contains the name of
	///				// the file which caused the exception while trying
	///				// to read the configuration from it.
	///
	///				Console.WriteLine(le.FileName);
	///			}
	///		}
	///
	///		public static void Main(string[] args)
	///		{
	///			// Register our event handler for the error event.
	///			SiAuto.Si.Error += new ErrorEventHandler(EventHandler);
	///
	///			// Force an error event by passing a name of a file
	///			// which does not exist to the LoadConfiguration method.
	///			SiAuto.Si.LoadConfiguration("Inexistent.sic");
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
	///			If (TypeOf Args.Exception Is LoadConfigurationException) Then
	///				Dim le As LoadConfigurationException = Args.Exception
	///
	///				' A LoadConfigurationException provides additional
	///				' information about the occurred error besides the
	///				' normal exception message. It contains the name of
	///				' the file which caused the exception while trying to
	///				' read the configuration from it.
	///
	///				Console.WriteLine(le.FileName)
	///			End If
	///		End Sub
	///
	///		Sub Main()
	///			' Register our event handler for the error event.
	///			AddHandler SiAuto.Si.Error, AddressOf EventHandler
	///
	///			' Force an error event by passing a name of a file
	///			' which does not exist to the LoadConfiguration method.
	///			SiAuto.Si.LoadConfiguration("Inexistent.sic")
	///		End Sub
	/// End Module
	/// </code>
	/// </example>
	/// -->

	[Serializable]
	public sealed class LoadConfigurationException : SmartInspectException
	{
		private string fFileName;

		/// <summary>
		///   Overloaded. Creates and initializes a LoadConfigurationException
		///   instance.
		/// </summary>

		public LoadConfigurationException(): base()
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a LoadConfigurationException
		///   instance with a custom error message.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>

		public LoadConfigurationException(string message): base(message)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a LoadConfigurationException
		///   instance with a custom error message and an inner exception.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>
		/// <param name="inner">
		///   The inner exception which is the cause for this exception.
		/// </param>

		public LoadConfigurationException(string message, Exception inner): 
			base(message, inner)
		{
		}

		private LoadConfigurationException(SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
			if (info != null)
			{
				// Load the filename accordingly.
				this.fFileName = info.GetString("fFileName");
			}
		}

		/// <summary>
		///   Overloaded. Creates and initializes a LoadConfigurationException
		///   instance with a custom error message. Lets you specify the name
		///   of the file which caused this exception. 
		/// </summary>
		/// <param name="fileName">
		///   The name of the file which caused this exception.
		/// </param>
		/// <param name="message">
		///   The error message which describes the exception.
		/// </param>

		public LoadConfigurationException(string fileName, string message): 
			this(message)
		{
			this.fFileName = fileName;
		}

		/// <summary>
		///   Overridden. Supplies serialization data about this
		///   LoadConfigurationException instance.
		/// </summary>
		/// <param name="info">Holds the serialized data.</param>
		/// <param name="context">
		///   Contains information about the source or destination.
		/// </param>

		[SecurityPermission(SecurityAction.Demand,
			 SerializationFormatter = true)]

		public override void GetObjectData(SerializationInfo info,
			StreamingContext context)
		{
			base.GetObjectData(info, context);

			if (info != null)
			{
				// Add the filename to the serialization info.
				info.AddValue("fFileName", this.fFileName);
			}
		}

		/// <summary>
		///   Specifies the name of the file which caused this exception
		///   while trying to load the SmartInspect properties from it.
		/// </summary>

		public string FileName
		{
			get { return this.fFileName; }
			set { this.fFileName = value; }
		}
	}
}
