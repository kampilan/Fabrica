//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Runtime.Serialization;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used to report errors concerning the connections string in the
	///   SmartInspect class.
	/// </summary>
	/// <!--
	/// <remarks>
	///   An invalid syntax, unknown protocols or inexistent options in the
	///   <link SmartInspect.Connections, connections string> will result in
	///   an InvalidConnectionsException exception. This exception type is
	///   used by the Connections property of the SmartInspect class.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	[Serializable]
	public sealed class InvalidConnectionsException: System.Exception
	{
		/// <summary>
		///   Overloaded. Creates and initializes an InvalidConnectionsException
		///   instance.
		/// </summary>

		public InvalidConnectionsException(): base()
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes an InvalidConnectionsException
		///   instance with a custom error message.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>

		public InvalidConnectionsException(string message): base(message)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes an InvalidConnectionsException
		///   instance with a custom error message and an inner exception.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>
		/// <param name="inner">
		///   The inner exception which is the cause for this exception.
		/// </param>

		public InvalidConnectionsException(string message, Exception inner):
			base(message, inner)
		{
		}

		private InvalidConnectionsException(SerializationInfo info,
			StreamingContext context): base(info, context)
		{
		}
	}
}
