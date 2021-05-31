//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Represents the type of a LogEntry packet. Instructs the Console
	///   to choose the correct icon and to perform additional actions,
	///   like, for example, enter a new method or draw a separator.
	/// </summary>

	public enum LogEntryType
	{
		/// <summary>
		///   Instructs the Console to draw a separator.
		/// </summary>

		Separator,

		/// <summary>
		///   Instructs the Console to enter a new method.
		/// </summary>

		EnterMethod,

		/// <summary>
		///   Instructs the Console to leave a method.
		/// </summary>

		LeaveMethod,

		/// <summary>
		///   Instructs the Console to reset the current call stack.
		/// </summary>

		ResetCallstack,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as simple
		///   message.
		/// </summary>

		Message = 100,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as warning
		///   message.
		/// </summary>

		Warning,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as error
		///   message.
		/// </summary>

		Error,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as internal
		///   error.
		/// </summary>

		InternalError,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as comment.
		/// </summary>

		Comment,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as a variable
		///   value.
		/// </summary>

		VariableValue,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as checkpoint.
		/// </summary>

		Checkpoint,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as debug
		///   message.
		/// </summary>

		Debug,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as verbose
		///   message.
		/// </summary>

		Verbose,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as fatal error
		///   message.
		/// </summary>

		Fatal,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as conditional
		///   message.
		/// </summary>

		Conditional,

		/// <summary>
		///   Instructs the Console to treat a Log Entry as assert
		///   message.
		/// </summary>

		Assert,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with text.
		/// </summary>

		Text = 200,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with binary data.
		/// </summary>

		Binary,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with a picture as data.
		/// </summary>

		Graphic,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with source code data.
		/// </summary>

		Source,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with object data.
		/// </summary>

		Object,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with web data.
		/// </summary>

		WebContent,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with system information.
		/// </summary>

		System,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with memory statistics.
		/// </summary>

		MemoryStatistic,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with a database result.
		/// </summary>

		DatabaseResult,

		/// <summary>
		///   Instructs the Console to treat the Log Entry as Log Entry
		///   with a database structure.
		/// </summary>

		DatabaseStructure
	}
}
