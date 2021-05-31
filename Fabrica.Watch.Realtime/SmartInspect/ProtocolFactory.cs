//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Creates Protocol instances and registers custom protocols.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is responsible for creating instances of Protocol
	///   subclasses and registering custom protocol implementations. To
	///   add a custom protocol, please have a look at the documentation
	///   and example of the RegisterProtocol method.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class ProtocolFactory
	{
		private static Type fProtocolType = typeof(Protocol);
		private static Hashtable fProtocols;
		private const string PROTOCOL_NOT_FOUND = 
			"The requested protocol is unknown";

		static ProtocolFactory()
		{
			fProtocols = Hashtable.Synchronized(new Hashtable());
//			RegisterProtocol("pipe", typeof(PipeProtocol));
			RegisterProtocol("file", typeof(FileProtocol));
			RegisterProtocol("mem", typeof(MemoryProtocol));
			RegisterProtocol("tcp", typeof(TcpProtocol));
			RegisterProtocol("text", typeof(TextProtocol));
		}

		private static Protocol CreateInstance(Type type)
		{
			try 
			{
				return (Protocol) Activator.CreateInstance(type);
			}
			catch (Exception e) 
			{
				throw new SmartInspectException(e.Message);
			}
		}

		/// <summary>
		///   Creates an instance of a Protocol subclass. 
		/// </summary>
		/// <param name="name">The protocol name to search for.</param>
		/// <param name="options">
		///   The options to apply to the new Protocol instance. Can be
		///   null.
		/// </param>
		/// <returns>A new instance of a Protocol subclass.</returns>
		/// <!--
		/// <remarks>
		///   This method tries to create an instance of a Protocol subclass
		///   using the name parameter. If you, for example, specify "file"
		///   as name parameter, this method returns an instance of the
		///   FileProtocol class. If the creation of such an instance has
		///   been successful, the supplied options will be applied to
		///   the protocol.
		///   
		///   For a list of available protocols, please refer to the Protocol
		///   class. Additionally, to add your own custom protocol, please
		///   have a look at the RegisterProtocol method.
		///   
		///   Please note that if the name argument is null, then the
		///   return value of this method is null as well.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type         Condition
		///   +                      +
		///   SmartInspectException  Unknown protocol or invalid options
		///                            syntax.
		/// </table>
		/// </exception>
		/// -->

		public static Protocol GetProtocol(string name, string options)
		{
			if (name == null)
			{
				return null;
			}

			Type type = (Type) fProtocols[name.Trim().ToLower()];

			if (type != null)
			{
				Protocol protocol = CreateInstance(type);
				protocol.Initialize(options);
				return protocol;
			}
			else
			{
				throw new SmartInspectException(PROTOCOL_NOT_FOUND);
			}
		}

		/// <summary>
		///   Registers a custom protocol implementation to the SmartInspect
		///   .NET library.
		/// </summary>
		/// <param name="name">
		///   The name of the custom protocol to register.
		/// </param>
		/// <param name="type">
		///   The type of your custom protocol. It needs to be a class
		///   derived from the Protocol class.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method enables you to register your own custom protocols.
		///   This can be used to extend the built-in capabilities of the
		///   SmartInspect .NET library. To add your own protocol, derive
		///   your custom protocol class from Protocol, choose a name and
		///   pass this name and the type to this method. After registering
		///   your protocol, you are able to use it in the
		///   <link SmartInspect.Connections, connections string> just like
		///   any other (standard) protocol.
		///
		///   If one of the supplied arguments is null or the supplied type
		///   is not derived from the Protocol class then no custom protocol
		///   is added.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		/// 
		/// using System;
		/// using Gurock.SmartInspect;
		/// 
		/// class StdoutProtocol: Protocol
		/// {
		///		// Implement the abstract methods and handle your protocol
		///		// specific options ..
		/// }
		///
		/// public class Program
		/// {
		///		public static void Main(string[] args)
		///		{
		///			ProtocolFactory.RegisterProtocol("stdout",
		///				typeof(StdoutProtocol));
		///			SiAuto.Si.Connections = "stdout()";
		///			SiAuto.Si.Enabled = true;
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		/// 
		/// Imports Gurock.SmartInspect
		///
		/// Public Class StdoutProtocol
		///		Inherits Protocol
		///		' Implement the abstract methods and handle your protocol
		///		' specific options ...
		/// End Class
		///
		/// Module Program
		///		Sub Main()
		///			ProtocolFactory.RegisterProtocol("stdout", _
		///				GetType(StdoutProtocol))
		///			SiAuto.Si.Connections = "stdout()"
		///			SiAuto.Si.Enabled = True
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public static void RegisterProtocol(string name, Type type)
		{
			if (name != null && type != null)
			{
				if (fProtocolType.IsAssignableFrom(type))
				{
					fProtocols[name.Trim().ToLower()] = type;
				}
			}
		}
	}
}
