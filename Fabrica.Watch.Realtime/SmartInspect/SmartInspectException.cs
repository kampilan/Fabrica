//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Runtime.Serialization;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Used internally to report any kind of error.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This is the base class for several exceptions which are mainly
	///   used for internal error reporting. However, it can be useful
	///   to have a look at its derived classes, LoadConnectionsException
	///   and ProtocolException, which provide additional information
	///   about occurred errors besides the normal exception message.
	///   
	///   This can be useful if you need to obtain more information about
	///   a particular error in the SmartInspect.Error event.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	[Serializable]
	public class SmartInspectException: System.Exception
	{
		/// <summary>
		///   Overloaded. Creates and initializes a SmartInspectException
		///   instance.
		/// </summary>

		public SmartInspectException(): base()
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a SmartInspectException
		///   instance with a custom error message.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>

		public SmartInspectException(string message): base(message)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a SmartInspectException
		///   instance with a custom error message and an inner exception.
		/// </summary>
		/// <param name="message">
		///   The error message which describes this exception.
		/// </param>
		/// <param name="inner">
		///   The inner exception which is the cause for this exception.
		/// </param>

		public SmartInspectException(string message, Exception inner):
			base(message, inner)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a SmartInspectException
		///   instance with serialized data.
		/// </summary>
		/// <param name="info">Holds the serialized data.</param>
		/// <param name="context">
		///   Contains information about the source or destination.
		/// </param>

		protected SmartInspectException(SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///   Overloaded. Creates and initializes a SmartInspectException
		///   instance with a custom error message which is assembled
		///   with a format string and a related array of arguments.
		/// </summary>
		/// <param name="format">
		///   The format string to create a description of this exception.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>

		public SmartInspectException(string format, params object[] args):
			this(String.Format(format, args))
		{
		}
	}
}
