//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   SmartInspect is the most important class in the SmartInspect
	///   .NET library. It is an interface for the protocols, packets
	///   and sessions and is responsible for the error handling.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The SmartInspect class is the most important class in the
	///   SmartInspect .NET library. An instance of this class is able
	///   to write log messages to a file or to send them directly
	///   to the SmartInspect Console using TCP. You can control these
	///   connections by setting the Connections property. 
	///   
	///   The SmartInspect class offers several properties for controlling
	///   the logging behavior. Besides the Connections property there
	///   is the Enabled property which controls if log messages should be
	///   sent or not. Furthermore, the AppName property specifies the
	///   application name displayed in the SmartInspect Console. And last
	///   but not least, we have the Level and DefaultLevel properties
	///   which specify the log level of an SmartInspect object and its
	///   related sessions.
	///   
	///   Additionally, the SmartInspect class acts as parent for
	///   sessions, which contain the actual logging methods, like, for
	///   example, Session.LogMessage or Session.LogObject. It is possible
	///   and common that several different sessions have the same parent
	///   and thus share the same connections. The Session class contains
	///   dozens of useful methods for logging any kind of data. Sessions
	///   can even log variable watches, generate illustrated process and
	///   thread information or control the behavior of the SmartInspect
	///   Console. It is possible, for example, to clear the entire log in
	///   the Console by calling the Session.ClearLog method.
	///   
	///   To accomplish these different tasks the SmartInspect concept uses
	///   several different packets. The SmartInspect class manages these
	///   packets and logs them to its connections. It is possibility to
	///   register event handlers for every packet type which are called
	///   after a corresponding packet has been sent.
	///   
	///   The error handling in the SmartInspect .NET library is a
	///   little bit different than in other libraries. This library uses
	///   an event, the Error event, for reporting errors. We've chosen
	///   this way because a logging framework should not alter the behavior
	///   of an application by firing exceptions. The only exception you
	///   need to handle can be thrown by the Connections property if the
	///   supplied <link SmartInspect.Connections, connections string>
	///   contains errors.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class SmartInspect: System.IDisposable
	{
		private const string VERSION = "3.3.1.21";
		private const string CAPTION_NOT_FOUND =
			"No protocol could be found with the specified caption";
		private const string CONNECTIONS_NOT_FOUND = 
			"No connections string found";

		private Level fLevel;
		private Level fDefaultLevel;
		private ClockResolution fResolution;

		private SessionManager fSessions;
		private ProtocolVariables fVariables;
		private bool fEnabled;
		private object fLock;
		private string fConnections;
		private string fAppName;
		private bool fIsMultiThreaded;

		private string fHostName;
		private ArrayList fProtocols;

		/// <summary>
		///   Occurs before a packet is processed. Offers the opportunity
		///   to filter out packets.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Packet"/>
		/// <seealso cref="Gurock.SmartInspect.FilterEventHandler"/>
		/// <remarks>
		///   This event can be used if filtering of certain packets is
		///   needed. The event handlers are always called in the context
		///   of the thread which causes the event.
		/// 
		///   Please see the examples for more information on how to use
		///   this event.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect
		///   log statements to the event handlers can cause a presumably
		///   undesired recursive behavior.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// using Gurock.SmartInspect;
		/// 
		/// public class FilterEvent
		/// {
		///		static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///
		///			// Register an event handler for the Filter event
		///			SiAuto.Si.Filter += new FilterEventHandler(EventHandler);
		///
		///			// The second message will be canceled and not be logged.
		///			SiAuto.Main.LogMessage("Message");
		///			SiAuto.Main.LogMessage("Cancel Me");
		///		}
		///
		///		static void EventHandler(object sender, FilterEventArgs args)
		///		{
		///			// Is the supplied packet a Log Entry?
		///			LogEntry logEntry = args.Packet as LogEntry;
		///
		///			if (logEntry != null)
		///			{
		///				if (logEntry.Title.Equals("Cancel Me"))
		///				{
		///					args.Cancel = true;
		///				}
		///			}
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// Imports Gurock.SmartInspect
		///
		/// Module FilterEvent
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As FilterEventArgs)
		///			' Is the supplied packet a Log Entry?
		///			If (TypeOf Args.Packet Is LogEntry) Then
		///				Dim logEntry As LogEntry = Args.Packet
		///				If logEntry.Title.Equals("Cancel Me") Then
		///					Args.Cancel = True
		///				End If
		///			End If
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///
		///			' Register an event handler for the Filter event
		///			AddHandler SiAuto.Si.Filter, AddressOf EventHandler
		///
		///			' The second message will be canceled and not be logged.
		///			SiAuto.Main.LogMessage("Message")
		///			SiAuto.Main.LogMessage("Cancel Me")
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public event FilterEventHandler Filter;

		/// <summary>
		///   Occurs when a LogEntry packet is processed.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LogEntry"/>
		/// <seealso cref="Gurock.SmartInspect.LogEntryEventHandler"/>
		/// <remarks>
		///   This event can be used if custom processing of LogEntry
		///   packets is needed. The event handlers are always called in the
		///   context of the thread which causes the event.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect
		///   log statements to the event handlers can cause a presumably
		///   undesired recursive behavior. Also, if you specified that
		///   one or more connections of this SmartInspect object should
		///   operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>, you need to protect the passed
		///   LogEntry packet and its data by calling its <link Packet.Lock,
		///   Lock> and <link Packet.Unlock, Unlock> methods before and
		///   after processing.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// using Gurock.SmartInspect;
		///
		/// public class LogEntryEvent
		/// {
		///		public static void EventHandler(object sender, LogEntryEventArgs args)
		///		{
		///			System.Console.WriteLine(args.LogEntry.Title);
		///		}
		///
		///		public static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///			SiAuto.Si.LogEntry += new LogEntryEventHandler(EventHandler);
		///			SiAuto.Main.LogMessage("This is an event test!");
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// Imports Gurock.SmartInspect
		///
		/// Module LogEntryEvent
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As LogEntryEventArgs)
		///			System.Console.WriteLine(Args.LogEntry.Title)
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///			AddHandler SiAuto.Si.LogEntry, AddressOf EventHandler
		///			SiAuto.Main.LogMessage("This is an event test!")
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public event LogEntryEventHandler LogEntry;

		/// <summary>
		///   Occurs when a ControlCommand packet is processed.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ControlCommand"/>
		/// <seealso cref="Gurock.SmartInspect.ControlCommandEventHandler"/>
		/// <remarks>
		///   This event can be used if custom processing of ControlCommand
		///   packets is needed. The event handlers are always called in the
		///   context of the thread which causes the event.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect
		///   log statements to the event handlers can cause a presumably
		///   undesired recursive behavior. Also, if you specified that
		///   one or more connections of this SmartInspect object should
		///   operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>, you need to protect the passed
		///   ControlCommand packet and its data by calling its
		///   <link Packet.Lock, Lock> and <link Packet.Unlock, Unlock>
		///   methods before and after processing.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// using Gurock.SmartInspect;
		///
		/// public class ControlCommandEvent
		/// {
		///		public static void EventHandler(object sender, 
		///			ControlCommandEventArgs args)
		///		{
		///			System.Console.WriteLine(args.ControlCommand.ControlCommandType);
		///		}
		///
		///		public static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///			SiAuto.Si.ControlCommand += 
		///				new ControlCommandEventHandler(EventHandler);
		///			SiAuto.Main.ClearAll();
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// Imports Gurock.SmartInspect
		///
		/// Module ControlCommandEvent
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As ControlCommandEventArgs)
		///			System.Console.WriteLine(Args.ControlCommand.ControlCommandType)
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///			AddHandler SiAuto.Si.ControlCommand, AddressOf EventHandler
		///			SiAuto.Main.ClearAll()
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public event ControlCommandEventHandler ControlCommand;

		/// <summary>
		///   Occurs when a Watch packet is processed.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <seealso cref="Gurock.SmartInspect.WatchEventHandler"/>
		/// <remarks>
		///   This event can be used if custom processing of Watch packets
		///   is needed. The event handlers are always called in the context
		///   of the thread which causes the event.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect
		///   log statements to the event handlers can cause a presumably
		///   undesired recursive behavior. Also, if you specified that
		///   one or more connections of this SmartInspect object should
		///   operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>, you need to protect the passed
		///   Watch packet and its data by calling its <link Packet.Lock,
		///   Lock> and <link Packet.Unlock, Unlock> methods before and
		///   after processing.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		/// 
		/// using System;
		/// using Gurock.SmartInspect;
		///
		/// public class WatchEvent
		/// {
		///		public static void EventHandler(object sender, 
		///			WatchEventArgs args)
		///		{
		///			Console.WriteLine(args.Watch.Name + "=" + args.Watch.Value);
		///		}
		///
		///		public static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///			SiAuto.Si.Watch += new WatchEventHandler(EventHandler);
		///			SiAuto.Main.WatchInt("Integer", 23);
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// Imports Gurock.SmartInspect
		///
		/// Module WatchEvent
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As WatchEventArgs)
		///			System.Console.WriteLine(Args.Watch.Name & "=" & Args.Watch.Value)
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///			AddHandler SiAuto.Si.Watch, AddressOf EventHandler
		///			SiAuto.Main.WatchInt("Integer", 23)
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public event WatchEventHandler Watch;

		/// <summary>
		///   This event is fired after an error occurred.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ErrorEventHandler"/>
		/// <remarks>
		///   This event is fired when an error occurs. An error could be
		///   a connection problem or wrong permissions when writing log
		///   files, for example. Instead of throwing exceptions, this event
		///   is used for error reporting in the SmartInspect .NET library.
		///   The event handlers are always called in the context of the
		///   thread which caused the event. In <link Protocol.IsValidOption,
		///   asynchronous protocol mode>, this is not necessarily the thread
		///   that initiated the related log call.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect log
		///   statements or other code to the event handlers which can lead
		///   to the error event can cause a presumably undesired recursive
		///   behavior.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// using Gurock.SmartInspect;
		///
		/// public class ErrorHandling
		/// {
		///		public static void EventHandler(object sender, 
		///			ErrorEventArgs args)
		///		{
		///			System.Console.WriteLine(args.Exception);
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
		/// Imports Gurock.SmartInspect
		///
		/// Module ErrorHandling
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As ErrorEventArgs)
		///			System.Console.WriteLine(Args.Exception)
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

		public event ErrorEventHandler Error;

		/// <summary>
		///   Occurs when a ProcessFlow packet is processed.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ProcessFlow"/>
		/// <seealso cref="Gurock.SmartInspect.ProcessFlowEventHandler"/>
		/// <remarks>
		///   This event can be used if custom processing of ProcessFlow
		///   packets is needed. The event handlers are always called in the
		///   context of the thread which causes the event.
		///
		///   <b>Please note</b>: Keep in mind that adding SmartInspect
		///   log statements to the event handlers can cause a presumably
		///   undesired recursive behavior. Also, if you specified that
		///   one or more connections of this SmartInspect object should
		///   operate in <link Protocol.IsValidOption,
		///   asynchronous protocol mode>, you need to protect the passed
		///   ProcessFlow packet and its data by calling its
		///   <link Packet.Lock, Lock> and <link Packet.Unlock, Unlock>
		///   methods before and after processing.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// using Gurock.SmartInspect;
		///
		/// public class ProcessFlowEvent
		/// {
		///		public static void EventHandler(object sender, 
		///			ProcessFlowEventArgs args)
		///		{
		///			System.Console.WriteLine(args.ProcessFlow.Title);
		///		}
		///
		///		public static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///			SiAuto.Si.ProcessFlow += 
		///				new ProcessFlowEventHandler(EventHandler);
		///			SiAuto.Main.EnterThread("Main Thread");
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// Imports Gurock.SmartInspect
		///
		/// Module ProcessFlowEvent
		///		Sub EventHandler(ByVal Sender As Object, ByVal Args As ProcessFlowEventArgs)
		///			System.Console.WriteLine(Args.ProcessFlow.Title)
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///			AddHandler SiAuto.Si.ProcessFlow, AddressOf EventHandler
		///			SiAuto.Main.EnterThread("Main Thread")
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public event ProcessFlowEventHandler ProcessFlow;

		/// <summary>
		///   Creates and initializes a new instance of the SmartInspect
		///   class.
		/// </summary>
		/// <param name="appName">
		///   The application name used for Log Entries. It is usually
		///   set to the name of the application which creates this object.
		/// </param>

		public SmartInspect(string appName)
		{
			this.fLock = new object();

			// Initialize the remaining fields.

			// this.fEnabled = false; // Not needed. See FxCop.
			this.fProtocols = new ArrayList();
			this.fConnections = String.Empty;
			AppName = appName;

			// Initialize level fields
			this.fLevel = Level.Debug; // Allow all
			this.fDefaultLevel = Level.Message;

			try
			{
				// Try to get the NetBIOS name of this machine.
				this.fHostName = System.Environment.MachineName;
			}
			catch
			{
				// We couldn't get the NetBIOS name of this machine,
				// so we set the HostName to an empty string.
				this.fHostName = String.Empty;
			}

			this.fSessions = new SessionManager();
			this.fResolution = ClockResolution.Standard;
			this.fVariables = new ProtocolVariables();
		}

		/// <summary>
		///   Specifies the timestamp resolution mode for this SmartInspect
		///   object.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ClockResolution"/>
		/// <remarks>
		///   By changing this property, you can specify if this object
		///   should try to use high-resolution timestamps for LogEntry,
		///   Watch and ProcessFlow packets. High-resolution timestamps
		///   provide a microsecond resolution. Conversely, standard
		///   timestamps have a maximum resolution of 10-55 milliseconds.
		/// 
		///   The support for high-resolution timestamps depends on the
		///   System.Diagnostics.Stopwatch class introduced in .NET 2.0.
		///   SmartInspect can only use high-resolution timestamps if
		///   this class is available and if its IsHighResolution property
		///   returns true. In particular, this means that high-resolution
		///   timestamps are not available for .NET 1.1 and .NET 1.0.
		///   The System.Diagnostics.Stopwatch class internally uses the
		///   QueryPerformanceCounter and QueryPerformanceFrequency
		///   functions. For information about these functions, please see
		///   the Windows Platform SDK documentation.
		///   
		///   Additionally, <b>high-resolution timestamps are not intended
		///   to be used on production systems</b>. It is recommended to use
		///   them only during development and debugging. High-resolution
		///   timestamps can introduce several problems that are acceptable
		///   on development machines but normally not tolerable on production
		///   systems:
		/// 
		///   <table>
		///   Problem      Description
		///   +            +
		///   Performance  High-resolution timestamps can be a lot slower than
		///                 standard timestamps. This actually depends on the
		///                 concrete implementation of QueryPerformanceCounter
		///                 (i.e. which timer is used for the high-resolution
		///                 performance counter [PIT, PMT, TSC, HPET]), but in
		///                 general one can say that standard timestamps are a
		///                 lot faster to read.
		///
		///   Accuracy     High-resolution timestamps tend to deviate from the
		///                 system timer when seen over a longer period of time.
		///                 Depending on the particular QueryPerformanceCounter
		///                 implementation, it can happen that high-resolution
		///                 timestamps induce an error of milliseconds within a
		///                 few minutes only.
		///
		///   Reliability  Depending on the used timer, QueryPerformanceCounter
		///                 provides unreliable results under certain, not so
		///                 uncommon, circumstances. When the TSC timer is used,
		///                 multi-processor/multi-core systems or processors
		///                 with varying frequencies (like found in most modern
		///                 notebooks or desktop machines) are known to cause
		///                 several problems which make high-resolution
		///                 timestamps unsuitable for production usage.
		///   </table>
		/// 
		///   Due to the mentioned problems, this property defaults to using
		///   the standard timestamp resolution.
		/// </remarks>
		/// -->

		public ClockResolution Resolution
		{
			get { return this.fResolution; }
			set { this.fResolution = value; }
		}

		/// <summary>
		///   Returns the current date and time, optionally with a high
		///   resolution.
		/// </summary>
		/// <returns>The current date and time as DateTime value.</returns>
		/// <!--
		/// <remarks>
		///   If the Resolution property specifies using a high resolution
		///   for timestamps, this method tries to return a timestamp with a
		///   microsecond resolution.
		/// 
		///   The support for high-resolution timestamps depends on the
		///   System.Diagnostics.Stopwatch class introduced in .NET 2.0.
		///   This method can only return a high-resolution timestamp if
		///   this class is available and if its IsHighResolution property
		///   returns true. In particular, this means that high-resolution
		///   timestamps are not available for .NET 1.1 and .NET 1.0.
		///   The System.Diagnostics.Stopwatch class internally uses the
		///   QueryPerformanceCounter and QueryPerformanceFrequency
		///   functions. For information about these functions, please see
		///   the Windows Platform SDK documentation.
		/// 
		///   If high-resolution support is not available, this method
		///   simply returns DateTime.Now.
		/// </remarks>
		/// -->

		public virtual DateTime Now()
		{
			return Clock.Now(this.fResolution);
		}

		/// <summary>
		///   Returns the version number of the SmartInspect .NET library.
		/// </summary>
		/// <remarks>
		///   This static read-only property returns the version number of
		///   the SmartInspect .NET library. The returned string always has
		///   the form "MAJOR.MINOR.RELEASE.BUILD".
		/// </remarks>

		public static string Version
		{
			get { return VERSION; }
		}

		/// <summary>
		///   Represents the hostname of the sending machine.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LogEntry"/>
		/// <remarks>
		///   This read-only property returns the hostname of the current
		///   machine. The hostname helps you to identify Log Entries
		///   from different machines in the SmartInspect Console.
		/// </remarks>
		/// -->

		public string HostName
		{
			get { return this.fHostName; }
		}

		/// <summary>
		///   Represents the application name used for the Log Entries.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LogEntry"/>
		/// <remarks>
		///   The application name helps you to identify Log Entries from
		///   different applications in the SmartInspect Console. If you
		///   set this property to null, the application name will be empty
		///   when sending Log Entries.
		/// </remarks>
		/// -->

		public string AppName
		{
			get { return this.fAppName; }

			set
			{
				if (value == null)
				{
					this.fAppName = String.Empty;
				}
				else
				{
					this.fAppName = value;
				}

				UpdateProtocols();
			}
		}

		private void UpdateProtocols()
		{
			lock (this.fLock)
			{
				foreach (Protocol p in this.fProtocols)
				{
					p.AppName = this.fAppName;
					p.HostName = this.fHostName;
				}
			}
		}

		/// <summary>
		///   Represents the log level of this SmartInspect instance and its
		///   related sessions.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Level"/>
		/// <seealso cref="Gurock.SmartInspect.SmartInspect.DefaultLevel"/>
		/// <remarks>
		///   The Level property of this SmartInspect instance represents
		///   the log level used by its corresponding sessions to determine
		///   if information should be logged or not. The default value of
		///   this property is Level.Debug.
		///   
		///   Every method (except the Clear method family) in the Session
		///   class tests if its log level equals or is greater than the
		///   log level of its parent. If this is not the case, the methods
		///   return immediately and won't log anything.
		/// 
		///   The log level for a method in the Session class can either be
		///   specified explicitly by passing a Level argument or implicitly
		///   by using the <link DefaultLevel, default level>. Every method
		///   in the Session class which makes use of the parent's log level
		///   and does not take a Level argument, uses the <link DefaultLevel,
		///   default level> of its parent as log level.
		/// 
		///   For more information about the default level, please refer to
		///   the documentation of the DefaultLevel property.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		/// 
		/// using Gurock.SmartInspect;
		/// 
		/// public class Program
		/// {
		///		static void Method()
		///		{
		///			SiAuto.Main.EnterMethod(Level.Debug, "Method");
		///			try
		///			{
		///				// ...
		///			}
		///			finally
		///			{
		///				SiAuto.Main.LeaveMethod(Level.Debug, "Method");
		///			}
		///		}
		///
		///		static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///
		///			SiAuto.Si.Level = Level.Debug;
		///			Method(); // Logs EnterMethod and LeaveMethod calls.
		///
		///			SiAuto.Si.Level = Level.Message;
		///			Method(); // Ignores EnterMethod and LeaveMethod calls.
		///		}
		/// }
		/// </code>
		/// <code>
		/// ' [VB.NET Example]
		/// 
		/// Imports Gurock.SmartInspect
		///
		/// Module Program
		///		Sub Method()
		///			SiAuto.Main.EnterMethod(Level.Debug, "Method")
		///			Try
		///				' ...
		///			Finally
		///				SiAuto.Main.LeaveMethod(Level.Debug, "Method")
		///			End Try
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///
		///			SiAuto.Si.Level = Level.Debug
		///			Method() ' Logs EnterMethod and LeaveMethod calls.
		///
		///			SiAuto.Si.Level = Level.Message
		///			Method() ' Ignores EnterMethod and LeaveMethod calls.
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public Level Level
		{
			get { return this.fLevel; }
			set { this.fLevel = value; }
		}

		/// <summary>
		///   Represents the default log level of this SmartInspect
		///   instance and its related sessions.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Level"/>
		/// <seealso cref="Gurock.SmartInspect.SmartInspect.Level"/>
		/// <remarks>
		///   The DefaultLevel property of this SmartInspect instance
		///   represents the default log level used by its corresponding
		///   sessions. The default value of this property is Level.Message.
		/// 
		///   Every method in the Session class which makes use of the
		///   parent's <link Level, log level> and does not take a Level
		///   argument, uses the default level of its parent as log level.
		/// 
		///   For more information on how to use this property, please have
		///   a look at the following examples.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		/// 
		/// public class Program
		/// {
		///		static void Method()
		///		{
		///			SiAuto.Main.EnterMethod("Method");
		///			try
		///			{
		///				// ...
		///			}
		///			finally
		///			{
		///				SiAuto.Main.LeaveMethod("Method");
		///			}
		///		}
		///
		///		static void Main(string[] args)
		///		{
		///			SiAuto.Si.Enabled = true;
		///
		///			SiAuto.Si.Level = Level.Debug;
		///			SiAuto.Si.DefaultLevel = Level.Verbose;
		///
		///			// Since the EnterMethod and LeaveMethod calls do not
		///			// specify their log level explicitly (by passing a Level
		///			// argument), they use the default log level which has
		///			// just been set to Level.Verbose (see above). And since
		///			// the log level of the SiAuto.Si object is set to
		///			// Level.Debug, the EnterMethod and LeaveMethod calls will
		///			// be logged.
		///			Method();
		///
		///			SiAuto.Si.Level = Level.Message; // Switch to Level.Message
		///
		///			// Since EnterMethod and LeaveMethod still use Level.Verbose
		///			// as their log level and the log level of the SiAuto.Si
		///			// object is now set to Level.Message, the EnterMethod and
		///			// LeaveMethod calls will be ignored and not be logged.
		///			Method();
		///		}
		/// }
		/// </code>
		/// <code>
		/// ' [VB.NET Example]
		/// 
		/// Imports Gurock.SmartInspect
		///
		///	Module Program
		///		Sub Method()
		///			SiAuto.Main.EnterMethod("Method")
		///			Try
		///				' ...
		///			Finally
		///				SiAuto.Main.LeaveMethod("Method")
		///			End Try
		///		End Sub
		///
		///		Sub Main()
		///			SiAuto.Si.Enabled = True
		///
		///			SiAuto.Si.Level = Level.Debug
		///			SiAuto.Si.DefaultLevel = Level.Verbose
		///
		///			' Since the EnterMethod and LeaveMethod calls do not
		///			' specify their log level explicitly (by passing a Level
		///			' argument), they use the default log level which has
		///			' just been set to Level.Verbose (see above). And since
		///			' the log level of the SiAuto.Si object is set to
		///			' Level.Debug, the EnterMethod and LeaveMethod calls will
		///			' be logged.
		///			Method()
		///
		///			SiAuto.Si.Level = Level.Message ' Switch to Level.Message
		///
		///			' Since EnterMethod and LeaveMethod still use Level.Verbose
		///			' as their log level and the log level of the SiAuto.Si
		///			' object is now set to Level.Message, the EnterMethod and
		///			' LeaveMethod calls will be ignored and not be logged.
		///			Method()
		///		End Sub
		/// End Module
		/// </code>
		/// </example>
		/// -->

		public Level DefaultLevel
		{
			get { return this.fDefaultLevel; }
			set { this.fDefaultLevel = value; }
		}

		private void Connect()
		{
			// Here, we simply call the Connect method of
			// all protocol objects in our collection. If an
			// error occurs we call the Error event.

			for (int i = 0; i < this.fProtocols.Count; i++)
			{
				Protocol p = (Protocol) this.fProtocols[i];

				try
				{
					p.Connect();
				}
				catch (Exception e)
				{
					OnError(e);
				}
			}
		}

		private void Disconnect()
		{
			// Here, we simply call the Disconnect method of
			// all protocol objects in our collection. If an
			// error occurs we call the Error event.

			for (int i = 0; i < this.fProtocols.Count; i++)
			{
				Protocol p = (Protocol) this.fProtocols[i];

				try
				{
					p.Disconnect();
				}
				catch (Exception e)
				{
					OnError(e);
				}
			}
		}

		/// <summary>
		///   This property allows you to control if anything should be
		///   logged at all.
		/// </summary>
		/// <!--
		/// <remarks>
		///   If you set this property to true, all connections will try
		///   to connect to their destinations. For example, if the
		///   Connections property is set to "file(filename=c:\\log.sil)",
		///   the file "c:\\log.sil" will be opened to write all following
		///   packets to it. By setting this property to false, all
		///   connections will disconnect.
		///
		///   Additionally, every Session method evaluates if its parent
		///   is enabled and returns immediately if this is not the case.
		///   This guarantees that the performance hit is minimal when 
		///   logging is disabled. The default value of this property is
		///   false. You need to set this property to true before you can
		///   use the SmartInspect instance and its related sessions.
		/// 
		///   <b>Please note:</b> If one or more connections of this
		///   SmartInspect object operate in
		///   <link Protocol.IsValidOption, asynchronous protocol mode>,
		///   you must disable this object by setting this property to
		///   false before exiting your application to properly exit
		///   and cleanup the protocol related threads. Disabling this
		///   instance may block until the related protocol threads are
		///   finished.
		/// </remarks>
		/// -->

		public bool Enabled
		{
			get { return this.fEnabled; }

			set
			{
				lock (this.fLock)
				{
					if (value)
					{
						Enable();
					}
					else 
					{
						Disable();
					}
				}
			}
		}

		private void Enable()
		{
			if (!this.fEnabled)
			{
				this.fEnabled = true;
				Connect();
			}
		}

		private void Disable()
		{
			if (this.fEnabled)
			{
				this.fEnabled = false;
				Disconnect();
			}
		}

		private void CreateConnections(string connections)
		{
			this.fIsMultiThreaded = false; /* See below */

			try
			{
				/* Expand the connections string with previously
				 * set connection variables and parse it. */
				ConnectionsParser parser = new ConnectionsParser();
				parser.Parse(
					this.fVariables.Expand(connections), 
					new ConnectionsParserEventHandler(AddConnection)
				);
			}
			catch (Exception e)
			{
				RemoveConnections();
				throw new InvalidConnectionsException(e.Message);
			}
		}

		private void AddConnection(object sender, 
			ConnectionsParserEventArgs args)
		{
			Protocol protocol =
				ProtocolFactory.GetProtocol(args.Protocol, args.Options);
			protocol.Error += new ErrorEventHandler(ProtocolError);

			this.fProtocols.Add(protocol);

			if (protocol.Asynchronous)
			{
				this.fIsMultiThreaded = true;
			}

			protocol.AppName = this.fAppName;
			protocol.HostName = this.fHostName;
		}

		private void ProtocolError(object sender, ErrorEventArgs e)
		{
			/* This is the error event handler for connections which
			 * operate in asynchronous protocol mode. */
			OnError(e.Exception);
		}

		/// <summary>
		///   Overloaded. Loads the connections string from a file and
		///   enables this SmartInspect instance.
		/// </summary>
		/// <param name="fileName">
		///   The name of the file to load the connections string from.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LoadConnectionsException"/>
		/// <seealso cref="Gurock.SmartInspect.InvalidConnectionsException"/>
		/// <remarks>
		///   This method loads the <link SmartInspect.Connections,
		///   connections string> from a file. This file should be a plain
		///   text file containing a line like in the following example:
		///   
		///   <code>connections=file(filename=c:\\log.sil)</code>
		///   
		///   Empty, unrecognized lines and lines beginning with a ';'
		///   character are ignored. This version of the method enables
		///   logging automatically.
		///
		///   The Error event is used to notify the application if
		///   the specified file cannot be opened or does not contain a
		///   <link SmartInspect.Connections, connections string>. The
		///   <link SmartInspect.Connections, connections string> and the
		///   <link SmartInspect.Enabled, enabled status> of this instance
		///   are not changed if such an error occurs.
		///
		///   The Error event is also used if a connections string could be
		///   read but is found to be invalid. In this case, an instance of
		///   the InvalidConnectionsException exception type is passed to
		///   the Error event.
		///
		///   Calling this method with the fileName parameter set to null
		///   has no effect.
		///
		///   This method is useful for customizing the connections string
		///   after the deployment of an application. A typical use case
		///   for this method is the following scenario: imagine a customer
		///   who needs to send a log file to customer service to analyse
		///   a software problem. If the software in question uses this
		///   LoadConnections method, the customer service just needs to send
		///   a prepared connections file to the customer. To enable the
		///   logging, the customer now just needs to drop this file to the
		///   application's installation directory or any other predefined
		///   location.
		/// 
		///   See LoadConfiguration for a method which is not limited to
		///   loading the connections string, but is also capable of loading
		///   any other property of this object from a file.
		/// 
		///   The LoadConnections and LoadConfiguration methods are both
		///   capable of detecting the string encoding of the connections
		///   and configuration files. Please see the LoadConfiguration
		///   method for details.
		///   
		///   To automatically replace placeholders in a loaded connections
		///   string, you can use so called connection variables. Please
		///   have a look at the SetVariable method for more information.
		/// </remarks>
		/// -->

		public void LoadConnections(string fileName)
		{
			LoadConnections(fileName, false);
		}

		/// <summary>
		///   Overloaded. Loads the connections string from a file.
		/// </summary>
		/// <param name="fileName">
		///   The name of the file to load the connections string from.
		/// </param>
		/// <param name="doNotEnable">
		///   Specifies if this instance should not be enabled automatically.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LoadConnectionsException"/>
		/// <seealso cref="Gurock.SmartInspect.InvalidConnectionsException"/>
		/// <remarks>
		///   This method loads the <link SmartInspect.Connections,
		///   connections string> from a file. This file should be a plain
		///   text file containing a line like in the following example:
		///   
		///   <code>connections=file(filename=c:\\log.sil)</code>
		///   
		///   Empty, unrecognized lines and lines beginning with a ';'
		///   character are ignored. This version of the method enables
		///   logging automatically unless the doNotEnable parameter is
		///   true. Please note that the doNotEnable parameter has no
		///   effect if this SmartInspect instance is already enabled.
		///
		///   The Error event is used to notify the application if
		///   the specified file cannot be opened or does not contain a
		///   <link SmartInspect.Connections, connections string>. The
		///   <link SmartInspect.Connections, connections string> and the
		///   <link SmartInspect.Enabled, enabled status> of this instance
		///   are not changed if such an error occurs.
		///
		///   The Error event is also used if a connections string could be
		///   read but is found to be invalid. In this case, an instance of
		///   the InvalidConnectionsException exception type is passed to
		///   the Error event.
		/// 
		///   This version of the method accepts the doNotEnable parameter.
		///   If this parameter is set to true, the <link SmartInspect.Enabled,
		///   enabled status> is not changed. Otherwise this SmartInspect
		///   instance will be enabled. Calling this method with the fileName
		///   parameter set to null has no effect.
		///   
		///   This method is useful for customizing the connections string
		///   after the deployment of an application. A typical use case
		///   for this method is the following scenario: imagine a customer
		///   who needs to send a log file to customer service to analyse
		///   a software problem. If the software in question uses this
		///   LoadConnections method, the customer service just needs to send
		///   a prepared connections file to the customer. To enable the
		///   logging, the customer now just needs to drop this file to the
		///   application's installation directory or any other predefined
		///   location.
		/// 
		///   See LoadConfiguration for a method which is not limited to
		///   loading the connections string, but is also capable of loading
		///   any other property of this object from a file.
		/// 
		///   The LoadConnections and LoadConfiguration methods are both
		///   capable of detecting the string encoding of the connections
		///   and configuration files. Please see the LoadConfiguration
		///   method for details.
		///   
		///   To automatically replace placeholders in a loaded connections
		///   string, you can use so called connection variables. Please
		///   have a look at the SetVariable method for more information.
		/// </remarks>
		/// -->

		public void LoadConnections(string fileName, bool doNotEnable)
		{
			if (fileName == null)
			{
				return;
			}

			string connections = null;

			try
			{
				// Try to read the connections string.
				connections = ReadConnections(fileName);
			}
			catch (Exception e)
			{
				// Catch exceptions while trying to read the connections
				// string and fire the error event.
				OnError(e);
			}

			if (connections == null)
			{
				return; // No connections string has been found.
			}

			lock (this.fLock)
			{
				if (TryConnections(connections))
				{
					if (!doNotEnable)
					{
						Enable();
					}
				}
			}
		}

		private static string ReadConnections(string fileName)
		{
			try
			{
				Configuration config = new Configuration();
				try
				{
					config.LoadFromFile(fileName);
					if (config.Contains("connections"))
					{
						return config.ReadString("connections", null);
					}
				}
				finally
				{
					config.Clear();
				}

				throw new SmartInspectException(CONNECTIONS_NOT_FOUND);
			}
			catch (Exception e)
			{
				// Retrow the exception and include the fileName argument.
				throw new LoadConnectionsException(fileName, e.Message);
			}
		}

		/// <summary>
		///   Loads the properties and sessions of this SmartInspect instance
		///   from a configuration file.
		/// </summary>
		/// <param name="fileName">
		///   The name of the file to load the configuration from.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ConfigurationTimer"/>
		/// <seealso cref="Gurock.SmartInspect.LoadConfigurationException"/>
		/// <seealso cref="Gurock.SmartInspect.InvalidConnectionsException"/>
		/// <remarks>
		///   This method loads the properties and sessions of this
		///   SmartInspect object from a file. This file should be a plain
		///   text file containing key/value pairs. Each key/value pair is
		///   expected to be on its own line. Empty, unrecognized lines and
		///   lines beginning with a ';' character are ignored.
		///   
		///   The Error event is used to notify the caller if an error
		///   occurs while trying to load the configuration from the
		///   specified file. Such errors include I/O errors like trying to
		///   open a file which does not exist, for example.
		///
		///   The Error event is also used if the specified configuraton
		///   file contains an invalid connections string. In this case, an
		///   instance of the InvalidConnectionsException exception type is
		///   passed to the Error event.
		/// 
		///   Calling this method with the fileName parameter set to null
		///   has no effect.
		///
		///   This method is useful for loading the properties and sessions
		///   of this SmartInspect instance after the deployment of an
		///   application. A typical use case for this method is the following
		///   scenario: imagine a customer who needs to send a log file to
		///   customer service to analyse a software problem. If the software
		///   in question uses this LoadConfiguration method, the customer
		///   service just needs to send a prepared configuration file to
		///   the customer. Now, to load the SmartInspect properties from a
		///   file, the customer now just needs to drop this file to the
		///   application's installation directory or any other predefined
		///   location.
		/// 
		///   To monitor a SmartInspect configuration file for changes,
		///   please have a look at the ConfigurationTimer class.
		///   
		///   To automatically replace placeholders in a loaded connections
		///   string, you can use so called connection variables. Please
		///   have a look at the SetVariable method for more information.
		/// 
		///   The following table lists the recognized configuration values,
		///   the corresponding SmartInspect properties and their types:
		/// 
		///   <table>
		///   Value          Property       Type
		///   +              +              +
		///   appname        AppName        string  
		///   connections    Connections    string
		///   defaultlevel   DefaultLevel   Gurock.SmartInspect.Level
		///   enabled        Enabled        bool
		///   level          Level          Gurock.SmartInspect.Level
		///   </table>
		/// 
		///   In addition to these properties, this method also configures
		///   any stored sessions of this SmartInspect object. Sessions that
		///   have been stored or will be added with the AddSession method
		///   will be configured with the properties of the related session
		///   entry of the passed configuration file. Please see the example
		///   section for details on how sessions entries look like.
		/// 
		///   If no entries can be found in the configuration file for a
		///   newly added session, this session will use the default session
		///   properties. The default session properties can also be
		///   specified in the configuration file. Please note that the
		///   session defaults do not apply to the main session SiAuto.Main
		///   since this session has already been added before a
		///   configuration file can be loaded. The session defaults only
		///   apply to newly added sessions and do not affect existing
		///   sessions.
		/// 
		///   The case of the configuration properties doesn't matter. This
		///   means, it makes no difference if you specify 'defaultlevel'
		///   or 'DefaultLevel' as key, for example.
		/// 
		///   For a typical configuration file, please see the example
		///   below.
		/// 
		///   To support Unicode strings, both the LoadConnections and
		///   LoadConfiguration methods are capable of auto-detecting the
		///   string encoding if a BOM (Byte Order Mark) is given at the
		///   start of the file. The following table lists the supported
		///   encodings and the corresponding BOM identifiers.
		///
		///   <table>
		///   Encoding                BOM identifier
		///   +                       +
		///   UTF8                    0xEF, 0xBB, 0xBF
		///   Unicode                 0xFF, 0xFE
		///   Unicode big-endian      0xFE, 0xFF
		///   </table>
		/// 
		///   If no BOM is given, the text is assumed to be in the ASCII
		///   format. If the configuration file has been created or edited
		///   with the SmartInspect Configuration Builder, the file always
		///   has a UTF8 Byte Order Mark and Unicode strings are therefore
		///   handled automatically.
		/// </remarks>
		/// <example>
		/// <code>
		/// ; Specify the SmartInspect properties
		/// connections = file(filename=c:\\log.sil)
		/// enabled = true
		/// level = verbose
		/// defaultlevel = message
		/// appname = client
		/// 
		/// ; Then set the defaults for new sessions
		/// sessiondefaults.active = false
		/// sessiondefaults.level = message
		/// sessiondefaults.color = 0xffff7f
		/// 
		/// ; And finally configure some individual sessions
		/// session.main.level = verbose
		/// session.client.active = true
		/// session.client.color = 0x7fffff
		/// </code>
		/// </example>
		/// -->

		public void LoadConfiguration(string fileName)
		{
			if (fileName == null)
			{
				return;
			}

			Configuration config = new Configuration();

			try
			{
				try
				{
					config.LoadFromFile(fileName);
				}
				catch (Exception e)
				{
					OnError(new LoadConfigurationException(fileName, 
						e.Message));
					return;
				}

				lock (this.fLock)
				{
					ApplyConfiguration(config);
				}

				this.fSessions.LoadConfiguration(config);
			}
			finally 
			{
				config.Clear();
			}
		}

		private void ApplyConfiguration(Configuration config)
		{
			if (config.Contains("appname"))
			{
				this.fAppName =
					config.ReadString("appname", this.fAppName);
			}

			// The `enabled' configuration value needs to be handled special,
			// because its appearance and value have a direct impact on how
			// to treat the `connections' value and the order in which to
			// apply the values:
			//
			// If the `enabled' value is found, it is very important to
			// differentiate between the values true and false. If the
			// `enabled' value is false, the user obviously either wants
			// to disable this object or keep it disabled. To correctly
			// disable this SmartInspect instance, we need to do that before
			// the connections string is changed. Otherwise it can happen
			// that this SmartInspect instance temporarily uses the new
			// connections string (exactly in the case when it is already
			// enabled).
			//
			// Handling an `enabled' value of true is the other way round.
			// We cannot enable this SmartInspect instance before setting
			// the `connections' value, because this would cause this
			// SmartInspect instance to temporarily use its old connections
			// string.

			string connections = config.ReadString("connections", null);

			if (config.Contains("enabled"))
			{
				bool enabled = config.ReadBoolean("enabled", false);

				if (enabled)
				{
					TryConnections(connections);
					Enable();
				}
				else
				{
					Disable();
					TryConnections(connections);
				}
			}
			else
			{
				TryConnections(connections);
			}

			if (config.Contains("level"))
			{
				this.fLevel =
					config.ReadLevel("level", this.fLevel);
			}

			if (config.Contains("defaultlevel"))
			{
				this.fDefaultLevel =
					config.ReadLevel("defaultlevel", this.fDefaultLevel);
			}
		}

		/// <summary>
		///   Specifies all connections used by this SmartInspect instance.
		/// </summary>
		/// <!--
		/// <remarks>
		///   You can set multiple connections by separating the
		///   connections with commas. A connection consists of a protocol
		///   identifier like "file" plus optional protocol parameters in
		///   parentheses. If you, for example, want to log to a file, the
		///   Connections property must be set to "file()". You can specify
		///   the filename in the parentheses after the protocol identifier
		///   like this: "file(filename=\\"c:\\mylogfile.sil\\")". Please note
		///   that if the Enabled property is set to true, the connections
		///   try to connect to their destinations immediately. By default,
		///   no connections are used.
		///
		///   See the Protocol class for a list of available protocols and
		///   ProtocolFactory for a way to add your own custom protocols.
		///   Furthermore have a look at the LoadConnections and
		///   LoadConfiguration methods, which can load a connections string
		///   from a file. Also, for a class which assists in building
		///   connections strings, please refer to the documentation of the
		///   ConnectionsBuilder class.
		///   
		///   To automatically replace placeholders in the given connections
		///   string, you can use so called connection variables. Please
		///   have a look at the SetVariable method for more information.
		/// 
		///   Please note that an InvalidConnectionsException exception is
		///   thrown if an invalid connections string is supplied.
		/// </remarks>
		/// <exception>
		/// <table>
		///   Exception Type               Condition
		///   +                            +
		///   InvalidConnectionsException  Invalid syntax, unknown protocols
		///                                  or inexistent options.
		/// </table>
		/// </exception>
		/// <example>
		/// <code>
		/// SiAuto.Si.Connections = "";
		/// SiAuto.Si.Connections = "file()";
		/// SiAuto.Si.Connections = "file(filename=\\"log.sil\\", append=true)";
		/// SiAuto.Si.Connections = "file(append=true), tcp(host=\\"localhost\\")";
		/// SiAuto.Si.Connections = "file(), file(filename=\\"anotherlog.sil\\")";
		/// </code>
		/// </example>
		/// -->

		public string Connections
		{
			get { return this.fConnections; }

			set
			{
				lock (this.fLock)
				{
					ApplyConnections(value);
				}
			}
		}

		private void ApplyConnections(string connections)
		{
			// First remove the old connections.
			RemoveConnections();

			if (connections != null)
			{
				// Then create the new connections and assign the
				// connections string.
				CreateConnections(connections);
				this.fConnections = connections;

				if (this.fEnabled)
				{
					// This instance is currently enabled, so we can
					// try to connect now.
					Connect();
				}
			}
		}

		private bool TryConnections(string connections)
		{
			bool result = false;

			if (connections != null)
			{
				try
				{
					ApplyConnections(connections);
					result = true;
				}
				catch (InvalidConnectionsException e)
				{
					OnError(e);
				}
			}

			return result;
		}

		private void RemoveConnections()
		{
			Disconnect();
			this.fIsMultiThreaded = false; /* See CreateConnections */
			this.fProtocols.Clear();
			this.fConnections = String.Empty;
		}

		private Protocol FindProtocol(string caption)
		{
			for (int i = 0; i < this.fProtocols.Count; i++)
			{
				Protocol p = (Protocol) this.fProtocols[i];

				if (String.Compare(p.Caption, caption, true) == 0)
				{
					return p;
				}
			}

			return null;
		}

		/// <summary>
		///   Executes a custom protocol action of a connection.
		/// </summary>
		/// <param name="caption">
		///   The identifier of the connection. Not allowed to be null.
		/// </param>
		/// <param name="action">
		///   The action to execute by the requested connection.
		/// </param>
		/// <param name="state">
		///   An optional object which encapsulates additional protocol
		///   specific information about the custom action. Can be null.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Protocol.Dispatch"/>
		/// <seealso cref="Gurock.SmartInspect.Protocol.IsValidOption"/>
		/// <remarks>
		///   This method dispatches the action and state parameters to
		///   the connection identified by the caption argument. If no
		///   suitable connection can be found, the Error event is used.
		///   The Error event is also used if an exception is thrown in
		///   the custom protocol action.
		/// 
		///   The SmartInspect .NET library currently implements one custom
		///   protocol action in MemoryProtocol. The MemoryProtocol class
		///   is used for writing log packets to memory. On request, it
		///   can write its internal queue of packets to a user-supplied
		///   stream or Protocol object with a custom protocol action.
		///
		///   The request for executing the custom action and writing the
		///   queue can be initiated with this Dispatch method. Please see
		///   the example section below for details.
		/// 
		///   For more information about custom protocol actions, please
		///   refer to the Protocol.Dispatch method. Also have a look at
		///   the Protocol.IsValidOption method which explains how to set
		///   the caption of a connection.
		///  
		///   Please note that the custom protocol action is executed
		///   asynchronously if the requested connection operates in
		///   <link Protocol.IsValidOption, asynchronous protocol mode>.
		/// 
		///   If the supplied caption argument is null, this method does
		///   nothing and returns immediately.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		///
		/// // Set the connections string and enable logging. We do not
		/// // specify a caption for the memory connection and stick with
		/// // the default. By default, the caption of a connection is
		/// // set to the name of the protocol, in our case 'mem'.
		/// SiAuto.Si.Connections = "mem()";
		/// SiAuto.Si.Enabled = true;
		///
		/// ...
		/// 
		/// // Instrument your application with log statements as usual.
		/// SiAuto.Main.LogMessage("This is a message");
		/// SiAuto.Main.LogMessage("This is a message");
		///
		/// ...
		/// 
		/// // Then, in case of an unexpected event, for example, in a
		/// // global exception handler, you can write the entire queue
		/// // of packets of your memory protocol connection to a file
		/// // by using the Dispatch method.			
		/// using (Stream s = File.Create("log.sil"))
		/// {
		///		SiAuto.Si.Dispatch("mem", 0, s);
		/// }
		/// </code>
		/// 
		/// <code>
		/// // [C# Example]
		/// 
		/// ...
		/// 
		/// // Alternative Dispatch call with a Protocol object which
		/// // sends the queue content to a local Console via a named
		/// // pipe.
		/// using (Protocol p = new PipeProtocol())
		/// {
		///		// Optionally set some protocol options
		///		// p.Initialize(..);
		///		p.Connect();
		///		try
		///		{
		///			SiAuto.Si.Dispatch("mem", 0, p);
		///		}
		///		finally 
		///		{
		///			p.Disconnect();
		///		}
		/// }
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		///
		/// ' Set the connections string and enable logging. We do not
		/// ' specify a caption for the memory connection and stick with
		/// ' the default. By default, the caption of a connection is
		/// ' set to the name of the protocol, in our case 'mem'.
		/// SiAuto.Si.Connections = "mem()"
		/// SiAuto.Si.Enabled = True
		/// 
		/// ...
		/// 
		/// ' Instrument your application with log statements as usual.
		/// SiAuto.Main.LogMessage("This is a message")
		/// SiAuto.Main.LogMessage("This is a message")
		/// 
		/// ...
		/// 
		/// ' Then, in case of an unexpected event, for example, in a
		/// ' global exception handler, you can write the entire queue
		/// ' of packets of your memory protocol connection to a file
		/// ' by using the Dispatch method.			
		/// Using s As Stream = File.Create("log.sil")
		///		SiAuto.Si.Dispatch("mem", 0, s)
		/// End Using
		/// </code>
		/// 
		/// <code>
		/// ...
		/// 
		/// ' Alternative Dispatch call with a Protocol object which
		/// ' sends the queue content to a local Console via a named
		/// ' pipe.
		/// Using p As New PipeProtocol
		///		' Optionally set some protocol options
		///		' p.Initialize(..)
		///		p.Connect()
		///		Try
		///			SiAuto.Si.Dispatch("mem", 0, p)
		///		Finally
		///			p.Disconnect()
		///		End Try
		/// End Using
		/// </code>
		/// </example>
		/// -->

		public void Dispatch(string caption, int action, object state)
		{
			if (caption == null)
			{
				return; // No valid input
			}

			lock (this.fLock)
			{
				try
				{
					Protocol p = FindProtocol(caption);

					if (p == null)
					{
						throw new SmartInspectException(CAPTION_NOT_FOUND);
					}

					p.Dispatch(new ProtocolCommand(action, state));
				}
				catch (Exception e)
				{
					OnError(e);
				}
			}
		}

		/// <summary>
		///   Specifies the default property values for new sessions.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property lets you specify the default property values
		///   for new sessions which will be created by or passed to the
		///   AddSession method. Please see the AddSession method for more
		///   information. For information about the available session
		///   properties, please refer to the documentation of the Session
		///   class.
		/// </remarks>
		/// -->

		public SessionDefaults SessionDefaults
		{
			get { return this.fSessions.Defaults; }
		}

		/// <summary>
		///   Adds a new or updates an existing connection variable.
		/// </summary>
		/// <param name="key">
		///   The key of the connection variable.
		/// </param>
		/// <param name="value">
		///   The value of the connection variable.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method sets the value of a given connection variable.
		///   A connection variable is a placeholder for strings in the
		///   <link SmartInspect.Connections, connections string>. When
		///   setting a connections string (or loading it from a file
		///   with LoadConfiguration), any variables which have previously
		///   been defined with SetVariable are automatically replaced
		///   with their respective values.
		///   
		///   The variables in the connections string are expected to
		///   have the following form: $variable$.
		///   
		///   If a connection variable with the given key already exists,
		///   its value is overridden. To delete a connection variable,
		///   use UnsetVariable. This method does nothing if the key
		///   or value argument is null.
		///   
		///   Connection variables are especially useful if you load a
		///   connections string from a file and would like to handle
		///   some protocol options in your application instead of the
		///   configuration file.
		///   
		///   For example, if you encrypt log files, you probably do not
		///   want to specify the encryption key directly in your
		///   configuration file for security reasons. With connection
		///   variables, you can define a variable for the encryption
		///   key with SetVariable and then reference this variable in
		///   your configuration file. The variable is then automatically
		///   replaced with the defined value when loading the
		///   configuration file.
		///   
		///   Another example deals with the directory or path of a log
		///   file. If you include a variable in the path of your log
		///   file, you can later replace it in your application with
		///   the real value. This might come in handy if you want to
		///   write a log file to an environment specific value, such
		///   as an application data directory, for example.
		/// </remarks>
		/// <example>
		/// <code>
		/// // [C# Example]
		/// 
		/// // Define the variable "key" with the value "secret"
		/// SiAuto.Si.SetVariable("key", "secret");
		/// 
		/// ...
		/// 
		/// // And include the variable $key$ in the related connections
		/// // string (the connections string can either be set directly
		/// // or loaded from a file).
		/// file(encrypt="true", key="$key$")
		/// </code>
		/// 
		/// <code>
		/// ' [VB.NET Example]
		/// 
		/// ' Define the variable "key" with the value "secret"
		/// SiAuto.Si.SetVariable("key", "secret")
		/// 
		/// ...
		/// 
		/// ' And include the variable $key$ in the related connections
		/// ' string (the connections string can either be set directly
		/// ' or loaded from a file).
		/// file(encrypt="true", key="$key$")
		/// </code>
		/// </example>
		/// -->

		public void SetVariable(string key, string value)
		{
			if (key != null && value != null)
			{
				this.fVariables.Put(key, value);
			}
		}

		/// <summary>
		///   Returns the value of a connection variable.
		/// </summary>
		/// <param name="key">
		///   The key of the connection variable.
		/// </param>
		/// <returns>
		///   The value for the given connection variable or null if the
		///   connection variable is unknown.
		/// </returns>
		/// <!--
		/// <remarks>
		///   Please see the SetVariable method for more information
		///   about connection variables.
		/// </remarks>
		/// -->

		public string GetVariable(string key)
		{
			if (key == null)
			{
				return null;
			}
			else
			{
				return this.fVariables.Get(key);
			}
		}

		/// <summary>
		///   Unsets an existing connection variable.
		/// </summary>
		/// <param name="key">
		///   The key of the connection variable to delete.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method deletes the connection variable specified by the
		///   given key. Nothing happens if the connection variable doesn't
		///   exist or if the key argument is null.
		/// </remarks>
		/// -->

		public void UnsetVariable(string key)
		{
			if (key != null)
			{
				this.fVariables.Remove(key);
			}
		}

		/// <summary>
		///   Overloaded. Adds and returns a new Session instance with this
		///   SmartInspect object set as parent.
		/// </summary>
		/// <param name="sessionName">
		///   The name for the new session. Not allowed to be null.
		///  </param>
		/// <returns>
		///   The new Session instance or null if the supplied sessionName
		///   parameter is null.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method allocates a new session with this SmartInspect 
		///   instance set as parent and the supplied sessionName parameter
		///   set as session name. The returned session will be configured
		///   with the default session properties as specified by the
		///   SessionDefaults property. This default configuration can be
		///   overridden on a per-session basis by loading the session
		///   configuration with the LoadConfiguration method. Please see
		///   the LoadConfiguration documentation for details.
		///   
		///   This version of the method does not save the returned session
		///   for later access.
		/// </remarks>
		/// -->

		public Session AddSession(string sessionName)
		{
			return AddSession(sessionName, false);
		}

		/// <summary>
		///   Overloaded. Adds and returns a new Session instance with this
		///   SmartInspect object set as parent and optionally saves it for
		///   later access.
		/// </summary>
		/// <param name="sessionName">
		///   The name for the new session. Not allowed to be null.
		/// </param>
		/// <param name="store">
		///   Indicates if the newly created session should be stored for
		///   later access.
		/// </param>
		/// <returns>
		///   The new Session instance or null if the supplied sessionName
		///   parameter is null.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method allocates a new session with this SmartInspect 
		///   instance set as parent and the supplied sessionName parameter
		///   set as session name. The returned session will be configured
		///   with the default session properties as specified by the
		///   SessionDefaults property. This default configuration can be
		///   overridden on a per-session basis by loading the session
		///   configuration with the LoadConfiguration method. Please see
		///   the LoadConfiguration documentation for details.
		///   
		///   If the 'store' parameter is true, the created and returned
		///   session is stored for later access and can be retrieved with
		///   the GetSession method. To remove a created session from the
		///   internal list, call the DeleteSession method. 
		///   
		///   If this method is called multiple times with the same session
		///   name, then the GetSession method operates on the session which
		///   got added last. If the sessionName parameter is null, this
		///   method does nothing and returns null as well.
		/// </remarks>
		/// -->

		public Session AddSession(string sessionName, bool store)
		{
			if (sessionName == null)
			{
				return null;
			}

			Session session = new Session(this, sessionName);
			this.fSessions.Add(session, store);
			return session;
		}

		/// <summary>
		///   Overloaded. Adds an existing Session instance to the internal
		///   list of sessions and saves it for later access.
		/// </summary>
		/// <param name="session">The session to store.</param>
		/// <!--
		/// <remarks>
		///   This method adds the passed session to the internal list of
		///   sessions and saves it for later access. The passed session
		///   will be configured with the default session properties as
		///   specified by the SessionDefaults property. This default
		///   configuration can be overridden on a per-session basis by
		///   loading the session configuration with the LoadConfiguration
		///   method. Please see the LoadConfiguration documentation for
		///   details.
		/// 
		///   The passed session can later be retrieved with the GetSession
		///   method. To remove an added session from the internal list,
		///   call the DeleteSession method.
		/// </remarks>
		/// -->

		public void AddSession(Session session)
		{
			this.fSessions.Add(session, true);
		}

		/// <summary>
		///   Removes a session from the internal list of sessions.
		/// </summary>
		/// <param name="session">
		///   The session to remove from the lookup table of sessions. Not
		///   allowed to be null.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method removes a session which has previously been added
		///   with and returned by the AddSession method. After this method
		///   returns, the GetSession method returns null when called with
		///   the same session name unless a different session with the same
		///   name has been added.
		///   
		///   This method does nothing if the supplied session argument is
		///   null.
		/// </remarks>
		/// -->

		public void DeleteSession(Session session)
		{
			this.fSessions.Delete(session);
		}

		/// <summary>
		///   Returns a previously added session.
		/// </summary>
		/// <param name="sessionName">
		///   The name of the session to lookup and return. Not allowed to
		///   be null.
		/// </param>
		/// <returns>
		///   The requested session or null if the supplied sessionName is
		///   null or if the session is unknown.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns a session which has previously been
		///   added with the AddSession method and can be identified by
		///   the supplied sessionName argument. If the requested session
		///   is unknown or if the sessionName argument is null, this
		///   method returns null.
		///   
		///   Note that the behavior of this method can be unexpected in
		///   terms of the result value if multiple sessions with the same
		///   name have been added. In this case, this method returns the
		///   session which got added last and not necessarily the session
		///   which you expect. 
		///   
		///   Adding multiple sessions with the same name should therefore
		///   be avoided.
		/// </remarks>
		/// -->

		public Session GetSession(string sessionName)
		{
			return this.fSessions.Get(sessionName);
		}

		/// <summary>
		///   Gets the session associated with the specified session name.
		/// </summary>
		/// <param name="sessionName">
		///   The name of the session to lookup and return. Not allowed to
		///   be null.
		/// </param>
		/// <returns>
		///   The requested session or null if the supplied sessionName is
		///   null or if the session is unknown.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This indexer returns the session which has previously been
		///   added with the AddSession method and can be identified by the
		///   specified session name. If the specified session is unknown
		///   or the sessionName parameter is null, null is returned. See
		///   the GetSession method for more information.
		/// </remarks>
		/// -->

		public Session this[string sessionName]
		{
			get { return this.fSessions.Get(sessionName); }
		}

		/// <summary>
		///   Updates an entry in the internal lookup table of sessions.
		/// </summary>
		/// <param name="session">
		///   The session whose name has changed and whose entry should
		///   be updated.
		/// </param>
		/// <param name="to">The new name of the session.</param>
		/// <param name="from">The old name of the session.</param>
		/// <!--
		/// <remarks>
		///   Once the name of a session has changed, this method is called
		///   to update the internal session lookup table. The 'to' argument
		///   specifies the new name and 'from' the old name of the session.
		///   After this method returns, the new name can be passed to the
		///   GetSession method to lookup the supplied session.
		/// </remarks>
		/// -->

		protected internal void UpdateSession(Session session, 
			string to, string from)
		{
			this.fSessions.Update(session, to, from);
		}

		/// <summary>
		///   Invokes the SmartInspect.LogEntry event handlers.
		/// </summary>
		/// <param name="logEntry">
		///   The Log Entry which has just been processed.
		/// </param>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept the
		///   SmartInspect.LogEntry event.
		/// </remarks>
		/// -->

		protected virtual void OnLogEntry(LogEntry logEntry)
		{
			LogEntryEventHandler handler = LogEntry;

			if (handler != null)
			{
				handler(this, new LogEntryEventArgs(logEntry));
			}
		}

		/// <summary>
		///   Invokes the SmartInspect.ControlCommand event handlers.
		/// </summary>
		/// <param name="controlCommand">
		///   The Control Command which has just been processed.
		/// </param>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept the
		///   SmartInspect.ControlCommand event.
		/// </remarks>
		/// -->

		protected virtual void OnControlCommand(ControlCommand controlCommand)
		{
			ControlCommandEventHandler handler = ControlCommand;

			if (handler != null)
			{
				handler(this, new ControlCommandEventArgs(controlCommand));
			}
		}

		/// <summary>
		///   Invokes the SmartInspect.Watch event handlers.
		/// </summary>
		/// <param name="watch">
		///   The Watch which has just been processed.
		/// </param>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept the
		///   SmartInspect.Watch event.
		/// </remarks>
		/// -->

		protected virtual void OnWatch(Watch watch)
		{
			WatchEventHandler handler = Watch;

			if (handler != null)
			{
				handler(this, new WatchEventArgs(watch));
			}
		}

		/// <summary>
		///   Invokes the SmartInspect.Error event handlers.
		/// </summary>
		/// <param name="e">
		///   The occurred exception.
		/// </param>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept the
		///   SmartInspect.Error event.
		/// </remarks>
		/// -->

		protected virtual void OnError(Exception e)
		{
			ErrorEventHandler error = Error;

			if (error != null)
			{
				error(this, new ErrorEventArgs(e));
			}
		}

		/// <summary>
		///   Invokes the SmartInspect.ProcessFlow event handlers.
		/// </summary>
		/// <param name="processFlow">
		///   The Process Flow entry which has just been processed.
		/// </param>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept
		///   the SmartInspect.ProcessFlow event.
		/// </remarks>
		/// -->

		protected virtual void OnProcessFlow(ProcessFlow processFlow)
		{
			ProcessFlowEventHandler handler = ProcessFlow;

			if (handler != null)
			{
				handler(this, new ProcessFlowEventArgs(processFlow));
			}
		}

		/// <summary>
		///   Invokes the SmartInspect.Filter event handlers and determines
		///   if the supplied packet should be sent or not.
		/// </summary>
		/// <param name="packet">
		///   The packet which is about to be processed.
		/// </param>
		/// <returns>
		///   True if the supplied packet shall be filtered and thus not be
		///   sent and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   Derived classes can override this method to intercept the
		///   SmartInspect.Filter event.
		/// </remarks>
		/// -->

		protected virtual bool OnFilter(Packet packet)
		{
			FilterEventHandler handler = Filter;

			if (handler != null)
			{
				FilterEventArgs args = new FilterEventArgs(packet);
				handler(this, args);
				return args.Cancel;
			}

			/* Do not filter the packet. */
			return false;
		}

		private void ProcessPacket(Packet packet)
		{
			lock (this.fLock)
			{
				/* Iterate through all available connections and
				 * write the packet. We do not use an enumerator
				 * for performance reasons here. This saves one
				 * created object for each packet. */

				for (int i = 0; i < this.fProtocols.Count; i++)
				{
					try
					{
						Object o = this.fProtocols[i];
						((Protocol) o).WritePacket(packet);
					}
					catch (Exception e)
					{
						OnError(e);
					}
				}
			}
		}

		/// <summary>Logs a Log Entry.</summary>
		/// <param name="logEntry">The Log Entry to log.</param>
		/// <!--
		/// <remarks>
		///   After setting the application name and hostname of the
		///   supplied Log Entry, this method determines if the Log
		///   Entry should really be sent by invoking the OnFilter
		///   method. If the Log Entry passes the filter test, it will be
		///   logged and the SmartInspect.LogEntry event is fired.
		/// </remarks>
		/// -->

		public void SendLogEntry(LogEntry logEntry)
		{
			/* Initialize the log entry packet for safe multi-threaded
			 * access only if this SmartInspect object has one or more
			 * connections which operate in asynchronous protocol mode.
			 * Also see CreateConnections. */

			if (this.fIsMultiThreaded)
			{
				logEntry.ThreadSafe = true;
			}

			/* Then fill the properties we are responsible for. */
			logEntry.AppName = AppName;
			logEntry.HostName = HostName;

			try 
			{
				if (!OnFilter(logEntry))
				{
					ProcessPacket(logEntry);
					OnLogEntry(logEntry);
				}
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		/// <summary>Logs a Control Command.</summary>
		/// <param name="controlCommand">The Control Command to log.</param>
		/// <!--
		/// <remarks>
		///   At first, this method determines if the Control Command should
		///   really be sent by invoking the OnFilter method. If the Control
		///   Command passes the filter test, it will be logged and the
		///   SmartInspect.ControlCommand event is fired.
		/// </remarks>
		/// -->

		public void SendControlCommand(ControlCommand controlCommand)
		{
			/* Initialize the control command for safe multi-threaded
			 * access only if this SmartInspect object has one or more
			 * connections which operate in asynchronous protocol mode.
			 * Also see CreateConnections. */

			if (this.fIsMultiThreaded)
			{
				controlCommand.ThreadSafe = true;
			}

			try 
			{
				if (!OnFilter(controlCommand))
				{
					ProcessPacket(controlCommand);
					OnControlCommand(controlCommand);
				}
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		/// <summary>Logs a Process Flow entry.</summary>
		/// <param name="processFlow">The Process Flow entry to log.</param>
		/// <!--
		/// <remarks>
		///   After setting the hostname of the supplied Process Flow entry,
		///   this method determines if the Process Flow entry should really
		///   be sent by invoking the OnFilter method. If the Process Flow
		///   entry passes the filter test, it will be logged and the
		///   SmartInspect.ProcessFlow event is fired.
		/// </remarks>
		/// -->

		public void SendProcessFlow(ProcessFlow processFlow)
		{
			/* Initialize the process flow for safe multi-threaded
			 * access only if this SmartInspect object has one or more
			 * connections which operate in asynchronous protocol mode.
			 * Also see CreateConnections. */

			if (this.fIsMultiThreaded)
			{
				processFlow.ThreadSafe = true;
			}

			/* Then fill the properties we are responsible for. */
			processFlow.HostName = HostName;

			try 
			{
				if (!OnFilter(processFlow))
				{
					ProcessPacket(processFlow);
					OnProcessFlow(processFlow);
				}
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		/// <summary>Logs a Watch.</summary>
		/// <param name="watch">The Watch to log.</param>
		/// <!--
		/// <remarks>
		///   At first, this method determines if the Watch should really
		///   be sent by invoking the OnFilter method. If the Watch passes
		///   the filter test, it will be logged and the SmartInspect.Watch
		///   event is fired.
		/// </remarks>
		/// -->

		public void SendWatch(Watch watch)
		{
			/* Initialize the watch packet for safe multi-threaded
			 * access only if this SmartInspect object has one or more
			 * connections which operate in asynchronous protocol mode.
			 * Also see CreateConnections. */

			if (this.fIsMultiThreaded)
			{
				watch.ThreadSafe = true;
			}

			try 
			{
				if (!OnFilter(watch))
				{
					ProcessPacket(watch);
					OnWatch(watch);
				}
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (this.fLock)
				{
					this.fEnabled = false;
					RemoveConnections();
				}

				this.fSessions.Clear();
			}
		}

		/// <summary>
		///   Releases all resources of this SmartInspect object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method disconnects and removes all internal connections
		///   and disables this instance. Moreover, all previously stored
		///   sessions will be removed.
		/// </remarks>
		/// -->

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
