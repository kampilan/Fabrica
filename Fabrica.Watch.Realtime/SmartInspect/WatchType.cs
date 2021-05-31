//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the type of a Watch packet. The type of a Watch
	///   specifies its variable type. 
	/// </summary>
	/// <!--
	/// <remarks>
	///   For example, if a Watch packet has a type of WatchType.String,
	///   the represented variable is treated as string in the Console.
	/// </remarks>
	/// -->

	public enum WatchType
	{
		/// <summary>
		///   Instructs the Console to treat a Watch value as char.
		/// </summary>

		Char,

		/// <summary>
		///   Instructs the Console to treat a Watch value as string.
		/// </summary>

		String,

		/// <summary>
		///   Instructs the Console to treat a Watch value as integer.
		/// </summary>

		Integer,

		/// <summary>
		///   Instructs the Console to treat a Watch value as float.
		/// </summary>

		Float,

		/// <summary>
		///   Instructs the Console to treat a Watch value as boolean.
		/// </summary>

		Boolean,

		/// <summary>
		///   Instructs the Console to treat a Watch value as address.
		/// </summary>

		Address,

		/// <summary>
		///   Instructs the Console to treat a Watch value as timestamp.
		/// </summary>

		Timestamp,

		/// <summary>
		///   Instructs the Console to treat a Watch value as object.
		/// </summary>

		Object
	}
}
