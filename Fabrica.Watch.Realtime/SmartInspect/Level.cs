//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the log level in the SmartInspect .NET library.
	/// </summary>
	/// <!--
	/// <remarks>
	///   Please see the SmartInspect.Level and SmartInspect.DefaultLevel
	///   properties for detailed examples and more information on how to
	///   use the Level enum.
	/// </remarks>
	/// -->

	public enum Level
	{
		/// <summary>
		///   Represents the Debug log level. This log level is mostly
		///   intended to be used in the debug and development process.
		/// </summary>

		Debug,

		/// <summary>
		///   Represents the Verbose log level. This log level is
		///   intended to track the general progress of applications at a
		///   fine-grained level.
		/// </summary>

		Verbose,

		/// <summary>
		///   Represents the Message log level. This log level is intended
		///   to track the general progress of applications at a
		///   coarse-grained level.
		/// </summary>

		Message,

		/// <summary>
		///   Represents the Warning log level. This log level designates
		///   potentially harmful events or situations.
		/// </summary>
		
		Warning,

		/// <summary>
		///   Represents the Error log level. This log level designates
		///   error events or situations which are not critical to the
		///   entire system. This log level thus describes recoverable
		///   or less important errors.
		/// </summary>

		Error,

		/// <summary>
		///   Represents the Fatal log level. This log level designates
		///   errors which are not recoverable and eventually stop the
		///   system or application from working.
		/// </summary>

		Fatal,

		/// <summary>
		///   This log level represents a special log level which is only
		///   used by the ControlCommand class and is not intended to be
		///   used directly.
		/// </summary>

		Control
	}
}
