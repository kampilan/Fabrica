//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Logs all kind of data and variables to the SmartInspect Console
	///   or to a log file.
	/// </summary>
	/// <!--
	/// <remarks>
	///   The Session class offers dozens of useful methods for sending
	///   any kind of data with the assistance of its <link Session.Parent,
	///   parent>. Sessions can send simple messages, warnings, errors and
	///   more complex things like pictures, objects, exceptions, system
	///   information and much more. They are even able to send variable
	///   watches, generate illustrated process and thread information
	///   or control the behavior of the SmartInspect Console. It is
	///   possible, for example, to clear the entire log in the Console by
	///   calling the ClearLog method.
	///   
	///   Please note that log methods of this class do nothing and return
	///   immediately if the session is currently <link Active, inactive>,
	///   its <link Session.Parent, parent> is <link SmartInspect.Enabled,
	///   disabled> or the <link SmartInspect.Level, log level> is not
	///   sufficient.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class Session
	{
		internal static Color
			DEFAULT_COLOR = Color.FromArgb(0xff, 0x05, 0x00, 0x00);

		private Level fLevel;
		private string fName;
	    private string fCorrelationId;            
        private int fCheckpointCounter;
		private SmartInspect fParent;
		private Color fColor;
		private bool fActive;
		private IDictionary fCounter;
		private IDictionary fCheckpoints;
		private bool fIsStored;

		/// <summary>
		///   Creates and initializes a new Session instance with the
		///   default color and the specified parent and name.
		/// </summary>
		/// <param name="parent">The parent of the new session.</param>
		/// <param name="name">The name of the new session.</param>

		public Session(SmartInspect parent, string name)
		{
			// Initialize the remaining fields.

			this.fParent = parent;
			this.fActive = true; // Active by default
			// this.fCheckpointCounter = 0; // Not needed. See FxCop.

			if (name == null)
			{
				this.fName = String.Empty;
			}
			else 
			{
				this.fName = name;
			}

		    fCorrelationId = "";

			ResetColor();

#if SI_DOTNET_1x
			this.fCounter = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
			this.fCheckpoints = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
#else
			this.fCounter = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);
			this.fCheckpoints = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);
#endif
		}


        public Session( SmartInspect parent, string name, string correlationId )
        {
            // Initialize the remaining fields.

            this.fParent = parent;
            this.fActive = true; // Active by default
            // this.fCheckpointCounter = 0; // Not needed. See FxCop.

            if (name == null)
            {
                this.fName = String.Empty;
            }
            else
            {
                this.fName = name;
            }

            fCorrelationId = correlationId;

            ResetColor();

#if SI_DOTNET_1x
			this.fCounter = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
			this.fCheckpoints = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
#else
            this.fCounter = new Hashtable(
                StringComparer.CurrentCultureIgnoreCase);
            this.fCheckpoints = new Hashtable(
                StringComparer.CurrentCultureIgnoreCase);
#endif
        }




		/// <summary>
		///   Indicates if this session is stored in the session tracking
		///   list of its Parent.
		/// </summary>
		/// <returns>
		///   True if this session is stored in the session tracking list
		///   and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   See the SmartInspect.GetSession and SmartInspect.AddSession
		///   methods for more information about session tracking.
		/// </remarks>
		/// -->

		protected internal bool IsStored
		{
			get { return this.fIsStored; }
			set { this.fIsStored = value; }
		}

		/// <summary>
		///   Represents the log level of this Session object.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Each Session object can have its own log level. A log message
		///   is only logged if its log level is greater than or equal to
		///   the log level of a session and the session Parent. Log levels
		///   can thus be used to limit the logging output to important
		///   messages only.
		/// </remarks>
		/// -->

		public Level Level
		{
			get { return this.fLevel; }
			set { this.fLevel = value; }
		}

		/// <summary>
		///   Represents the session name used for Log Entries.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The session name helps you to identify Log Entries from
		///   different sessions in the SmartInspect Console. If you set
		///   this property to null, the session name will be empty when
		///   sending Log Entries.
		/// </remarks>
		/// -->

		public string Name
		{
			get { return this.fName; }

			set	
			{
				string name;

				if (value == null)
				{
					name = String.Empty;
				}
				else 
				{
					name = value;
				}

				if (this.fIsStored)
				{
					this.fParent.UpdateSession(this, name, this.fName);
				}

				this.fName = name;
			}
		}

		/// <summary>
		///   Represents the parent of the session.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The parent of a session is a SmartInspect instance. It is
		///   responsible for sending the packets to the SmartInspect Console
		///   or for writing them to a file. If the <link SmartInspect.Enabled,
		///   Enabled> property of the parent is false, all logging methods
		///   of this class will return immediately and do nothing.
		/// </remarks>
		/// -->

		public SmartInspect Parent
		{
			get { return this.fParent; }
		}

		/// <summary>
		///   Specifies if the session is currently active.  
		/// </summary>
		/// <!--
		/// <remarks>
		///   If this property is set to false, all logging methods of this
		///   class will return immediately and do nothing. Please note that
		///   the <link Parent, parent> of this session also needs to be 
		///   <link SmartInspect.Enabled, enabled> in order to log information.
		///   
		///   This property is especially useful if you are using multiple
		///   sessions at once and want to deactivate a subset of these
		///   sessions. To deactivate all your sessions, you can use the
		///   <link SmartInspect.Enabled, Enabled> property of the <link
		///   Parent, parent>.
		/// </remarks>
		/// -->

		public bool Active
		{
			get { return this.fActive; }
			set { this.fActive = value; }
		}

		/// <summary>
		///   Overloaded. Indicates if information can be logged for a
		///   certain log level or not.  
		/// </summary>
		/// <param name="level">The log level to check for.</param>
		/// <returns>
		///   True if information can be logged and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method is used by the logging methods in this class
		///   to determine if information should be logged or not. When
		///   extending the Session class by adding new log methods to a
		///   derived class it is recommended to call this method first.
		/// </remarks>
		/// -->

		public bool IsOn(Level level)
		{
			return
				this.fActive &&
				this.fParent.Enabled &&
				level >= this.fLevel &&
				level >= this.fParent.Level;				
		}

		/// <summary>
		///   Overloaded. Indicates if information can be logged or
		///   not.
		/// </summary>
		/// <returns>
		///   True if information can be logged and false otherwise.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method is used by the logging methods in this class
		///   to determine if information should be logged or not. When
		///   extending the Session class by adding new log methods to a
		///   derived class it is recommended to call this method first.
  		/// </remarks>
		/// -->

		public bool IsOn()
		{
			return this.fActive && this.fParent.Enabled;
		}

		/// <summary>
		///   Represents the background color in the SmartInspect Console 
		///   of this session.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The session color helps you to identify Log Entries from
		///   different sessions in the SmartInspect Console by changing
		///   the background color.
		/// </remarks>
		/// -->

		public Color Color
		{
			get { return this.fColor; }
			set { this.fColor = value; }
		}

		/// <summary>
		///   Resets the session color to its default value.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The default color of a session is transparent.
		/// </remarks>
		/// -->

		public void ResetColor()
		{
			Color = DEFAULT_COLOR;
		}

		/// <summary>
		///   Overloaded. Logs a simple separator with the default log
		///   level.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method instructs the Console to draw a separator.
		///   A separator is intended to group related <link LogEntry,
		///   Log Entries> and to separate them visually from others. This
		///   method can help organizing Log Entries in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSeparator()
		{
			LogSeparator(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Logs a simple separator with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   This method instructs the Console to draw a separator.
		///   A separator is intended to group related <link LogEntry,
		///   Log Entries> and to separate them visually from others. This
		///   method can help organizing Log Entries in the Console.
		/// </remarks>
		/// -->

		public void LogSeparator(Level level)
		{
			if (IsOn(level))
			{
				SendLogEntry(level, null, LogEntryType.Separator, 
					ViewerId.None);
			}
		}

		/// <summary>
		///   Overloaded. Resets the call stack by using the default
		///   log level.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method instructs the Console to reset the call stack
		///   generated by the EnterMethod and LeaveMethod methods. It
		///   is especially useful if you want to reset the indentation
		///   in the method hierarchy without clearing all <link LogEntry,
		///   Log Entries>.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void ResetCallstack()
		{
			ResetCallstack(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Resets the call stack by using a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   This method instructs the Console to reset the call stack
		///   generated by the EnterMethod and LeaveMethod methods. It
		///   is especially useful if you want to reset the indentation
		///   in the method hierarchy without clearing all <link LogEntry,
		///   Log Entries>.
		/// </remarks>
		/// -->

		public void ResetCallstack(Level level)
		{
			if (IsOn(level))
			{
				SendLogEntry(level, null, LogEntryType.ResetCallstack, 
					ViewerId.None);
			}
		}

		/// <summary>
		///   Overloaded. Enters a method by using the default log level.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new 
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterMethod(string methodName)
		{
			EnterMethod(this.fParent.DefaultLevel, methodName);
		}

		/// <summary>
		///   Overloaded. Enters a method by using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new 
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		/// </remarks>
		/// -->

		public void EnterMethod(Level level, string methodName)
		{
			if (IsOn(level))
			{
				// Send two packets.
				SendLogEntry(level, methodName, LogEntryType.EnterMethod, 
					ViewerId.Title);
				SendProcessFlow(level, methodName, ProcessFlowType.EnterMethod);
			}
		}

		/// <summary>
		///   Overloaded. Enters a method by using the default log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterMethod(string methodNameFmt, object[] args)
		{
			EnterMethod(this.fParent.DefaultLevel, methodNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Enters a method by using a custom log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// </remarks>
		/// -->

		public void EnterMethod(Level level, string methodNameFmt,
			object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					EnterMethod(level, String.Format(methodNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("EnterMethod: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Enters a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterMethod(object instance, string methodName)
		{
			EnterMethod(this.fParent.DefaultLevel, instance, methodName);
		}

		/// <summary>
		///   Overloaded. Enters a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// </remarks>
		/// -->

		public void EnterMethod(Level level, object instance, string methodName)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					// The supplied instance is null.
					LogInternalError("EnterMethod: instance argument is null");
				}
				else
				{
					string type = instance.GetType().FullName;
					EnterMethod(level, type + "." + methodName);
				}
			}
		}

		/// <summary>
		///   Overloaded. Enters a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterMethod(object instance, string methodNameFmt, 
			object[] args)
		{
			EnterMethod(this.fParent.DefaultLevel, instance, methodNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Enters a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterMethod method notifies the Console that a new
		///   method has been entered. The Console includes the method in
		///   the method hierarchy. If this method is used consequently, a
		///   full call stack is visible in the Console which helps locating
		///   bugs in the source code. Please see the LeaveMethod method as
		///   the counter piece to EnterMethod.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// </remarks>
		/// -->

		public void EnterMethod(Level level, object instance,
			string methodNameFmt, object[] args)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					// The supplied instance is null.
					LogInternalError("EnterMethod: instance argument is null");
				}
				else
				{
					try
					{
						EnterMethod(
								level,
								instance.GetType().FullName + "." +
								String.Format(methodNameFmt, args)
							);
					}
					catch (Exception e)
					{
						// The String.Format method raised an exception.
						LogInternalError("EnterMethod: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Tracks a method by using the default log level.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(string methodName)
		{
			return TrackMethod(this.fParent.DefaultLevel, methodName);
		}

		/// <summary>
		///   Overloaded. Tracks a method by using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(Level level, string methodName)
		{
			if (IsOn(level))
			{
				return new MethodTracker(level, this, methodName);
			}
			else 
			{
				return null;
			}
		}

		/// <summary>
		///   Overloaded. Tracks a method by using the default log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(string methodNameFmt, object[] args)
		{
			return TrackMethod(this.fParent.DefaultLevel, methodNameFmt,
				args);
		}

		/// <summary>
		///   Overloaded. Tracks a method by using a custom log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(Level level, string methodNameFmt,
			object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					return TrackMethod(level, String.Format(methodNameFmt,
						args));
				}
				catch (Exception e)
				{
					LogInternalError("TrackMethod: " + e.Message);
				}
			}

			return null;
		}

		/// <summary>
		///   Overloaded. Tracks a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(object instance, string methodName)
		{
			return TrackMethod(this.fParent.DefaultLevel, instance,
				methodName);
		}

		/// <summary>
		///   Overloaded. Tracks a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(Level level, object instance, 
			string methodName)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					LogInternalError("TrackMethod: instance argument is null");
				}
				else
				{
					string type = instance.GetType().FullName;
					return TrackMethod(level, type + "." + methodName);
				}
			}

			return null;
		}

		/// <summary>
		///   Overloaded. Tracks a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(object instance,
			string methodNameFmt, object[] args)
		{
			return TrackMethod(this.fParent.DefaultLevel, instance,
				methodNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Tracks a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The TrackMethod method notifies the Console that a new
		///   method has been entered. The returned MethodTracker object
		///   can be wrapped in a using statement and then automatically
		///   calls LeaveMethod on disposal.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// </remarks>
		/// -->

		public MethodTracker TrackMethod(Level level, object instance,
			string methodNameFmt, object[] args)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					LogInternalError("TrackMethod: instance argument is null");
				}
				else
				{
					try
					{
						return TrackMethod(
								level,
								instance.GetType().FullName + "." +
								String.Format(methodNameFmt, args)
							);
					}
					catch (Exception e)
					{
						LogInternalError("TrackMethod: " + e.Message);
					}
				}
			}

			return null;
		}

		/// <summary>
		///   Overloaded. Leaves a method by using the default log level.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveMethod(string methodName)
		{
			LeaveMethod(this.fParent.DefaultLevel, methodName);
		}

		/// <summary>
		///   Overloaded. Leaves a method by using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		/// </remarks>
		/// -->

		public void LeaveMethod(Level level, string methodName)
		{
			if (IsOn(level))
			{
				// Send two packets.
				SendLogEntry(level, methodName, LogEntryType.LeaveMethod,
					ViewerId.Title);
				SendProcessFlow(level, methodName, ProcessFlowType.LeaveMethod);
			}
		}

		/// <summary>
		///   Overloaded. Leaves a method by using the default log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveMethod(string methodNameFmt, object[] args)
		{
			LeaveMethod(this.fParent.DefaultLevel, methodNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Leaves a method by using a custom log level.
		///   The method name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of a format string and the
		///   related array of arguments.
		/// </remarks>
		/// -->

		public void LeaveMethod(Level level, string methodNameFmt,
			object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					LeaveMethod(level, String.Format(methodNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LeaveMethod: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Leaves a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveMethod(object instance, string methodName)
		{
			LeaveMethod(this.fParent.DefaultLevel, instance, methodName);
		}

		/// <summary>
		///   Overloaded. Leaves a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied methodName argument.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of the FullName of the type
		///   of the supplied instance parameter, followed by a dot and the
		///   supplied methodName argument.
		/// </remarks>
		/// -->

		public void LeaveMethod(Level level, object instance, string methodName)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					// The supplied instance is null.
					LogInternalError("LeaveMethod: instance argument is null");
				}
				else
				{
					string type = instance.GetType().FullName;
					LeaveMethod(level, type + "." + methodName);
				}
			}
		}

		/// <summary>
		///   Overloaded. Leaves a method by using the default log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveMethod(object instance, string methodNameFmt, 
			object[] args)
		{
			LeaveMethod(this.fParent.DefaultLevel, instance, methodNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Leaves a method by using a custom log level.
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot
		///   and the supplied format string and its related array of
		///   arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="instance">
		///   The class name of this instance and a dot will be prepended
		///   to the method name.
		/// </param>
		/// <param name="methodNameFmt">
		///   The format string to create the name of the method.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveMethod method notifies the Console that a method
		///   has been left. The Console closes the current method in the
		///   method hierarchy. If this method is used consequently, a full
		///   call stack is visible in the Console which helps locating bugs
		///   in the source code. Please see the EnterMethod method as the
		///   counter piece to LeaveMethod.
		///   
		///   The resulting method name consists of the FullName of the
		///   type of the supplied instance parameter, followed by a dot and
		///   the supplied format string and its related array of arguments.
		/// </remarks>
		/// -->

		public void LeaveMethod(Level level, object instance,
			string methodNameFmt, object[] args)
		{
			if (IsOn(level))
			{
				if (instance == null)
				{
					// The supplied instance is null.
					LogInternalError("LeaveMethod: instance argument is null");
				}
				else
				{
					try
					{
						LeaveMethod(
								level,
								instance.GetType().FullName + "." +
								String.Format(methodNameFmt, args)
							);
					}
					catch (Exception e)
					{
						// The String.Format method raised an exception.
						LogInternalError("LeaveMethod: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Enters a new thread by using the default log
		///   level.
		/// </summary>
		/// <param name="threadName">The name of the thread.</param>
		/// <!--
		/// <remarks>
		///   The EnterThread method notifies the Console that a new
		///   thread has been entered. The Console displays this thread in
		///   the Process Flow toolbox. If this method is used consequently,
		///   all threads of a process are displayed. Please see the
		///   LeaveThread method as the counter piece to EnterThread.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterThread(string threadName)
		{
			EnterThread(this.fParent.DefaultLevel, threadName);
		}

		/// <summary>
		///   Overloaded. Enters a new thread by using a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="threadName">The name of the thread.</param>
		/// <!--
		/// <remarks>
		///   The EnterThread method notifies the Console that a new
		///   thread has been entered. The Console displays this thread in
		///   the Process Flow toolbox. If this method is used consequently,
		///   all threads of a process are displayed. Please see the
		///   LeaveThread method as the counter piece to EnterThread.
		/// </remarks>
		/// -->

		public void EnterThread(Level level, string threadName)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, threadName, ProcessFlowType.EnterThread);
			}
		}

		/// <summary>
		///   Overloaded. Enters a new thread by using the default log
		///   level. The thread name consists of a format string and the
		///   related array of arguments.
		/// </summary>
		/// <param name="threadNameFmt">
		///   The format string to create the name of the thread.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterThread method notifies the Console that a new
		///   thread has been entered. The Console displays this thread in
		///   the Process Flow toolbox. If this method is used consequently,
		///   all threads of a process are displayed. Please see the
		///   LeaveThread method as the counter piece to EnterThread.
		/// 
		///   The resulting thread name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterThread(string threadNameFmt, params object[] args)
		{
			EnterThread(this.fParent.DefaultLevel, threadNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Enters a new thread by using a custom log
		///   level. The thread name consists of a format string and the
		///   related array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="threadNameFmt">
		///   The format string to create the name of the thread.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterThread method notifies the Console that a new
		///   thread has been entered. The Console displays this thread in
		///   the Process Flow toolbox. If this method is used consequently,
		///   all threads of a process are displayed. Please see the
		///   LeaveThread method as the counter piece to EnterThread.
		/// 
		///   The resulting thread name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterThread(Level level, string threadNameFmt,
			params object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					EnterThread(level, String.Format(threadNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("EnterThread: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Leaves a thread by using the default log
		///   level.
		/// </summary>
		/// <param name="threadName">The name of the thread.</param>
		/// <!--
		/// <remarks>
		///   The LeaveThread method notifies the Console that a thread
		///   has been finished. The Console displays this change in the
		///   Process Flow toolbox. Please see the EnterThread method as
		///   the counter piece to LeaveThread.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveThread(string threadName)
		{
			LeaveThread(this.fParent.DefaultLevel, threadName);
		}

		/// <summary>
		///   Overloaded. Leaves a thread by using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="threadName">The name of the thread.</param>
		/// <!--
		/// <remarks>
		///   The LeaveThread method notifies the Console that a thread
		///   has been finished. The Console displays this change in the
		///   Process Flow toolbox. Please see the EnterThread method as
		///   the counter piece to LeaveThread.
		/// </remarks>
		/// -->

		public void LeaveThread(Level level, string threadName)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, threadName, ProcessFlowType.LeaveThread);
			}
		}

		/// <summary>
		///   Overloaded. Leaves a thread by using the default log level.
		///   The thread name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="threadNameFmt">
		///   The format string to create the name of the thread.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveThread method notifies the Console that a thread
		///   has been finished. The Console displays this change in the
		///   Process Flow toolbox. Please see the EnterThread method as
		///   the counter piece to LeaveThread.
		///   
		///   The resulting thread name consists of a format string and the
		///   related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveThread(string threadNameFmt, params object[] args)
		{
			LeaveThread(this.fParent.DefaultLevel, threadNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Leaves a thread by using a custom log level.
		///   The thread name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="threadNameFmt">
		///   The format string to create the name of the thread.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveThread method notifies the Console that a thread
		///   has been finished. The Console displays this change in the
		///   Process Flow toolbox. Please see the EnterThread method as
		///   the counter piece to LeaveThread.
		///   
		///   The resulting thread name consists of a format string and the
		///   related array of arguments.
		/// </remarks>
		/// -->

		public void LeaveThread(Level level, string threadNameFmt,
			params object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					LeaveThread(level, String.Format(threadNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LeaveThread: " + e.Message);
				}
			}		
		}

		/// <summary>
		///   Overloaded. Enters a new process by using the default log
		///   level and the parent's application name as process name.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterProcess()
		{
			EnterProcess(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Enters a new process by using a custom log
		///   level and the parent's application name as process name.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		/// </remarks>
		/// -->

		public void EnterProcess(Level level)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, Parent.AppName, 
					ProcessFlowType.EnterProcess);
				SendProcessFlow(level, "Main Thread", 
					ProcessFlowType.EnterThread);
			}
		}

		/// <summary>
		///   Overloaded. Enters a new process by using the default
		///   log level.
		/// </summary>
		/// <param name="processName">The name of the process.</param>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterProcess(string processName)
		{
			EnterProcess(this.fParent.DefaultLevel, processName);
		}

		/// <summary>
		///   Overloaded. Enters a new process by using a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="processName">The name of the process.</param>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		/// </remarks>
		/// -->

		public void EnterProcess(Level level, string processName)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, processName, 
					ProcessFlowType.EnterProcess);
				SendProcessFlow(level, "Main Thread", 
					ProcessFlowType.EnterThread);
			}
		}

		/// <summary>
		///   Overloaded. Enters a process by using the default log level.
		///   The process name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="processNameFmt">
		///   The format string to create the name of the process.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		///   
		///   The resulting process name consists of a format string and
		///   the related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void EnterProcess(string processNameFmt, params object[] args)
		{
			EnterProcess(this.fParent.DefaultLevel, processNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Enters a process by using a custom log level.
		///   The process name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="processNameFmt">
		///   The format string to create the name of the process.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The EnterProcess method notifies the Console that a new
		///   process has been entered. The Console displays this process
		///   in the Process Flow toolbox. Please see the LeaveProcess
		///   method as the counter piece to EnterProcess.
		///   
		///   The resulting process name consists of a format string and
		///   the related array of arguments.
		/// </remarks>
		/// -->

		public void EnterProcess(Level level, string processNameFmt,
			params object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					EnterProcess(level, String.Format(processNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("EnterProcess: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Leaves a process by using the default log level
		///   and the parent's application name as process name.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveProcess()
		{
			LeaveProcess(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Leaves a process by using a custom log level
		///   and the parent's application name as process name.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		/// </remarks>
		/// -->

		public void LeaveProcess(Level level)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, "Main Thread", 
					ProcessFlowType.LeaveThread);
				SendProcessFlow(level, Parent.AppName, 
					ProcessFlowType.LeaveProcess);
			}
		}

		/// <summary>
		///   Overloaded. Leaves a process by using the default log level.
		/// </summary>
		/// <param name="processName">The name of the process.</param>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveProcess(string processName)
		{
			LeaveProcess(this.fParent.DefaultLevel, processName);
		}

		/// <summary>
		///   Overloaded. Leaves a process by using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="processName">The name of the process.</param>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		/// </remarks>
		/// -->

		public void LeaveProcess(Level level, string processName)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, "Main Thread", 
					ProcessFlowType.LeaveThread);
				SendProcessFlow(level, processName, 
					ProcessFlowType.LeaveProcess);
			}
		}

		/// <summary>
		///   Overloaded. Leaves a process by using the default log level.
		///   The process name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="processNameFmt">
		///   The format string to create the name of the process.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		///   
		///   The resulting process name consists of a format string and
		///   the related array of arguments.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LeaveProcess(string processNameFmt, params object[] args)
		{
			LeaveProcess(this.fParent.DefaultLevel, processNameFmt, args);
		}

		/// <summary>
		///   Overloaded. Leaves a process by using a custom log level.
		///   The process name consists of a format string and the related
		///   array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="processNameFmt">
		///   The format string to create the name of the process.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   The LeaveProcess method notifies the Console that a process
		///   has finished. The Console displays this change in the Process
		///   Flow toolbox. Please see the EnterProcess method as the
		///   counter piece to LeaveProcess.
		///   
		///   The resulting process name consists of a format string and
		///   the related array of arguments.
		/// </remarks>
		/// -->

		public void LeaveProcess(Level level, string processNameFmt,
			params object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					LeaveProcess(level, String.Format(processNameFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LeaveProcess: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a colored message with the default log
		///   level.
		/// </summary>
		/// <param name="color">The background color in the Console.</param>
		/// <param name="title">The message to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogColored(Color color, string title)
		{
			LogColored(this.fParent.DefaultLevel, color, title);
		}

		/// <summary>
		///   Overloaded. Logs a colored message with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="color">The background color in the Console.</param>
		/// <param name="title">The message to log.</param>

		public void LogColored(Level level, Color color, string title)
		{
			if (IsOn(level))
			{
				SendLogEntry(level, title, LogEntryType.Message, 
					ViewerId.Title, color, null);
			}
		}

		/// <summary>
		///   Overloaded. Logs a colored message with the default log
		///   level. The message is created with a format string and a
		///   related array of arguments.
		/// </summary>
		/// <param name="color">The background color in the Console.</param>
		/// <param name="titleFmt">
		///   A format string to create the message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the message.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogColored(Color color, string titleFmt,
			params object[] args)
		{
			LogColored(this.fParent.DefaultLevel, color, titleFmt, args);
		}

		/// <summary>
		///   Overloaded. Logs a colored message with a custom log
		///   level. The message is created with a format string and a
		///   related array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="color">The background color in the Console.</param>
		/// <param name="titleFmt">
		///   A format string to create the message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the message.
		/// </remarks>
		/// -->

		public void LogColored(Level level, Color color, string titleFmt,
			params object[] args)
		{
			if (IsOn(level))
			{
				try
				{
					LogColored(level, color, String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogColored: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a debug message with a log level of
		///   Level.Debug.
		/// </summary>
		/// <param name="title">The message to log.</param>

		public void LogDebug(string title)
		{
			if (IsOn(Level.Debug))
			{
				SendLogEntry(Level.Debug, title, LogEntryType.Debug, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a debug message with a log level of
		///   Level.Debug. The message is created with a format string
		///   and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create the message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the message.
		/// </remarks>
		/// -->

		public void LogDebug(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Debug))
			{
				try
				{
					LogDebug(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogDebug: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a verbose message with a log level of
		///   Level.Verbose.
		/// </summary>
		/// <param name="title">The message to log.</param>

		public void LogVerbose(string title)
		{
			if (IsOn(Level.Verbose))
			{
				SendLogEntry(Level.Verbose, title, LogEntryType.Verbose, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a verbose message with a log level of
		///   Level.Verbose. The message is created with a format string
		///   and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create the message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the message.
		/// </remarks>
		/// -->

		public void LogVerbose(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Verbose))
			{
				try
				{
					LogVerbose(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogVerbose: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a simple message with a log level of
		///   Level.Message.
		/// </summary>
		/// <param name="title">The message to log.</param>

		public void LogMessage(string title)
		{
			if (IsOn(Level.Message))
			{
				SendLogEntry(Level.Message, title, LogEntryType.Message, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a simple message with a log level of
		///   Level.Message. The message is created with a format string
		///   and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create the message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the message.
		/// </remarks>
		/// -->

		public void LogMessage(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Message))
			{
				try
				{
					LogMessage(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogMessage: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a warning message with a log level of
		///   Level.Warning.
		/// </summary>
		/// <param name="title">The warning to log.</param>

		public void LogWarning(string title)
		{
			if (IsOn(Level.Warning))
			{
				SendLogEntry(Level.Warning, title, LogEntryType.Warning, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a warning message with a log level of
		///   Level.Warning. The warning message is created with a format
		///   string and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create the warning.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the warning message.
		/// </remarks>
		/// -->

		public void LogWarning(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Warning))
			{
				try
				{
					LogWarning(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogWarning: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs an error message with a log level of
		///   Level.Error.
		/// </summary>
		/// <param name="title">
		///   A string which describes the error.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method is ideally used in error handling code such as
		///   exception handlers. If this method is used consequently, it
		///   is easy to troubleshoot and solve bugs in applications or
		///   configurations. See LogException for a similar method.
		/// </remarks>
		/// -->

		public void LogError(string title)
		{
			if (IsOn(Level.Error))
			{
				SendLogEntry(Level.Error, title, LogEntryType.Error, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs an error message with a log level of
		///   Level.Error. The error message is created with a format
		///   string and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create a description of the error.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the error message.
		///   
		///   This method is ideally used in error handling code such as
		///   exception handlers. If this method is used consequently, it
		///   is easy to troubleshoot and solve bugs in applications or
		///   configurations. See LogException for a similar method.
		/// </remarks>
		/// -->

		public void LogError(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Error))
			{
				try
				{
					LogError(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogError: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a fatal error message with a log level of
		///   Level.Fatal.
		/// </summary>
		/// <param name="title">
		///   A string which describes the fatal error.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method is ideally used in error handling code such as
		///   exception handlers. If this method is used consequently, it
		///   is easy to troubleshoot and solve bugs in applications or
		///   configurations. See LogError for a method which does not
		///   describe fatal but recoverable errors.
		/// </remarks>
		/// -->

		public void LogFatal(string title)
		{
			if (IsOn(Level.Fatal))
			{
				SendLogEntry(Level.Fatal, title, LogEntryType.Fatal, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a fatal error message with a log level of
		///   Level.Fatal. The error message is created with a format
		///   string and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create a description of the fatal error.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the fatal error message.
		///   
		///   This method is ideally used in error handling code such as
		///   exception handlers. If this method is used consequently, it
		///   is easy to troubleshoot and solve bugs in applications or
		///   configurations. See LogError for a method which does not
		///   describe fatal but recoverable errors.
		/// </remarks>
		/// -->

		public void LogFatal(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Fatal))
			{
				try
				{
					LogFatal(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogFatal: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs an internal error with a log level of
		///   Level.Error.
		/// </summary>
		/// <param name="title">
		///   A string which describes the internal error.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs an internal error. Such errors can occur
		///   if session methods are invoked with invalid arguments. For
		///   example, if you pass an invalid format string to LogMessage,
		///   the exception will be caught and an internal error with the
		///   exception message will be sent.
		///
		///   This method is also intended to be used in derived classes
		///   to report any errors in your own methods.
		/// </remarks>
		/// -->

		protected void LogInternalError(string title)
		{
			if (IsOn(Level.Error))
			{
				SendLogEntry(Level.Error, title, LogEntryType.InternalError, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs an internal error with a log level of
		///   Level.Error. The error message is created with a format
		///   string and a related array of arguments.
		/// </summary>
		/// <param name="titleFmt">
		///   A format string to create a description of the internal error.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs an internal error. Such errors can occur
		///   if session methods are invoked with invalid arguments. For
		///   example, if you pass an invalid format string to LogMessage,
		///   the exception will be caught and an internal error with the
		///   exception message will be sent.
		///
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed
		///   to the String.Format method and the resulting string will be
		///   the error message.
		///   
		///   This method is also intended to be used in derived classes
		///   to report any errors in your own methods.
		/// </remarks>
		/// -->

		protected void LogInternalError(string titleFmt, params object[] args)
		{
			if (IsOn(Level.Error))
			{
				try
				{
					LogInternalError(String.Format(titleFmt, args));
				}
				catch (Exception e)
				{
					// The String.Format method raised an exception.
					LogInternalError("LogInternalError: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Increments the default checkpoint counter and logs
		///   a message with the default log level.
		/// </summary>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Session.ResetCheckpoint"/>
		/// <remarks>
		///   This method increments a checkpoint counter and then logs
		///   a message using "Checkpoint #N" as title. The initial value
		///   of the checkpoint counter is 0. You can use the
		///   ResetCheckpoint method to reset the counter to 0 again.
		///
		///   This method is useful, for example, for tracking loops. If
		///   AddCheckpoint is called for each iteration of a loop, it is
		///   easy to follow the execution of the loop in question. This
		///   method can also be used in recursive methods to understand
		///   the execution flow. Furthermore, you can use it to highlight
		///   important parts of your source code. See LogSeparator for a
		///   method with a similar intention.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void AddCheckpoint()
		{
			AddCheckpoint(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Increments the default checkpoint counter and logs
		///   a message with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Session.ResetCheckpoint"/>
		/// <remarks>
		///   This method increments a checkpoint counter and then logs
		///   a message using "Checkpoint #N" as title. The initial value
		///   of the checkpoint counter is 0. You can use the
		///   ResetCheckpoint method to reset the counter to 0 again.
		///
		///   This method is useful, for example, for tracking loops. If
		///   AddCheckpoint is called for each iteration of a loop, it is
		///   easy to follow the execution of the loop in question. This
		///   method can also be used in recursive methods to understand
		///   the execution flow. Furthermore, you can use it to highlight
		///   important parts of your source code. See LogSeparator for a
		///   method with a similar intention.
		/// </remarks>
		/// -->

		public void AddCheckpoint(Level level)
		{
			if (IsOn(level))
			{
				int counter =
					Interlocked.Increment(ref this.fCheckpointCounter);

				string title = "Checkpoint #" + counter; 
				SendLogEntry(level, title, LogEntryType.Checkpoint, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Increments the counter of a named checkpoint and
		///   logs a message with the default log level.
		/// </summary>
		/// <param name="name">
		///   The name of the checkpoint to increment.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method increments the counter for the given checkpoint
		///   and then logs a message using "%checkpoint% #N" as title where
		///   %checkpoint% stands for the name of the checkpoint and N for
		///   the incremented counter value. The initial value of the counter
		///   for a given checkpoint is 0. You can use the ResetCheckpoint
		///   method to reset the counter to 0 again.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void AddCheckpoint(string name)
		{
			AddCheckpoint(this.fParent.DefaultLevel, name, null);
		}

		/// <summary>
		///   Overloaded. Increments the counter of a named checkpoint and
		///   logs a message with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">
		///   The name of the checkpoint to increment.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method increments the counter for the given checkpoint
		///   and then logs a message using "%checkpoint% #N" as title where
		///   %checkpoint% stands for the name of the checkpoint and N for
		///   the incremented counter value. The initial value of the counter
		///   for a given checkpoint is 0. You can use the ResetCheckpoint
		///   method to reset the counter to 0 again.
		/// </remarks>
		/// -->

		public void AddCheckpoint(Level level, string name)
		{
			AddCheckpoint(level, name, null);
		}

		/// <summary>
		///   Overloaded. Increments the counter of a named checkpoint and
		///   logs a message with the default log level and an optional
		///   message.
		/// </summary>
		/// <param name="name">
		///   The name of the checkpoint to increment.
		/// </param>
		/// <param name="details">
		///   An optional message to include in the resulting log entry.
		///   Can be null.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method increments the counter for the given checkpoint
		///   and then logs a message using "%checkpoint% #N" as title where
		///   %checkpoint% stands for the name of the checkpoint and N for
		///   the incremented counter value. The initial value of the counter
		///   for a given checkpoint is 0. Specify the details parameter to
		///   include an optional message in the resulting log entry. You
		///   can use the ResetCheckpoint method to reset the counter to 0
		///   again. 
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void AddCheckpoint(string name, string details)
		{
			AddCheckpoint(this.fParent.DefaultLevel, name, details);
		}

		/// <summary>
		///   Overloaded. Increments the counter of a named checkpoint and
		///   logs a message with a custom log level and an optional
		///   message.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">
		///   The name of the checkpoint to increment.
		/// </param>
		/// <param name="details">
		///   An optional message to include in the resulting log entry.
		///   Can be null.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method increments the counter for the given checkpoint
		///   and then logs a message using "%checkpoint% #N" as title where
		///   %checkpoint% stands for the name of the checkpoint and N for
		///   the incremented counter value. The initial value of the counter
		///   for a given checkpoint is 0. Specify the details parameter to
		///   include an optional message in the resulting log entry. You
		///   can use the ResetCheckpoint method to reset the counter to 0
		///   again. 
		/// </remarks>
		/// -->

		public void AddCheckpoint(Level level, string name, string details)
		{
			if (IsOn(level))
			{
				if (name == null)
				{
					LogInternalError("AddCheckpoint: name argument is null");
					return;
				}

				int value;

				lock (this.fCheckpoints)
				{
					if (this.fCheckpoints.Contains(name))
					{
						value = (int) this.fCheckpoints[name];
					}
					else 
					{
						value = 0;
					}

					value++;
					this.fCheckpoints[name] = value;
				}

				StringBuilder sb = new StringBuilder();
				sb.Append(name);
				sb.Append(" #");
				sb.Append(value);

				if (details != null)
				{
					sb.Append(" (");
					sb.Append(details);
					sb.Append(")");
				}

				string title = sb.ToString();
				SendLogEntry(level, title, LogEntryType.Checkpoint, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Resets the default checkpoint counter.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method resets the checkpoint counter to 0. The checkpoint
		///   counter is used by the AddCheckpoint method.
		/// </remarks>
		/// -->

		public void ResetCheckpoint()
		{
			this.fCheckpointCounter = 0;
		}

		/// <summary>
		///   Overloaded. Resets the counter of a named checkpoint.
		/// </summary>
		/// <param name="name">The name of the checkpoint to reset.</param>
		/// <!--
		/// <remarks>
		///   This method resets the counter of the given named checkpoint.
		///   Named checkpoints can be incremented and logged with the
		///   AddCheckpoint method.
		/// </remarks>
		/// -->

		public void ResetCheckpoint(string name)
		{
			if (name == null)
			{
				LogInternalError("ResetCheckpoint: name argument is null");
				return;
			}

			lock (this.fCheckpoints)
			{
				this.fCheckpoints.Remove(name);
			}
		}

		/// <summary>
		///   Overloaded. Logs a assert message if a condition is false with
		///   a log level of Level.Error.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="title">The title of the Log Entry.</param>
		/// <!--
		/// <remarks>
		///   An assert message is logged if this method is called with a
		///   condition parameter of the value false. No <link LogEntry,
		///   Log Entry> is generated if this method is called with a
		///   condition parameter of the value true.
		///
		///   A typical usage of this method would be to test if a variable
		///   is not set to null before you use it. To do this, you just need
		///   to insert a LogAssert call to the code section in question with
		///   "instance != null" as first parameter. If the reference is null
		///   and thus the expression evaluates to false, a message is logged.
		/// </remarks>
		/// -->

		public void LogAssert(bool condition, string title)
		{
			if (IsOn(Level.Error))
			{
				if (!condition)
				{
					SendLogEntry(Level.Error, title, LogEntryType.Assert,
						ViewerId.Title);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs an assert message if a condition is false with
		///   a log level of Level.Error. The assert message is created with
		///   a format string and a related array of arguments.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="titleFmt">
		///   The format string to create the title of the Log Entry.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   An assert message is logged if this method is called with a
		///   condition parameter of the value false. No <link LogEntry,
		///   Log Entry> is generated if this method is called with a
		///   condition parameter of the value true.
		///
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed to
		///   the String.Format method and the resulting string will be the
		///   assert message.
		///   
		///   A typical usage of this method would be to test if a variable
		///   is not set to null before you use it. To do this, you just need
		///   to insert a LogAssert call to the code section in question with
		///   "instance != null" as first parameter. If the reference is null
		///   and thus the expression evaluates to false, a message is logged.
		/// </remarks>
		/// -->

		public void LogAssert(bool condition, string titleFmt,
			params object[] args)
		{
			if (IsOn(Level.Error))
			{
				if (!condition)
				{
					try
					{
						string title = String.Format(titleFmt, args);
						SendLogEntry(Level.Error, title, LogEntryType.Assert,
							ViewerId.Title);
					}
					catch (Exception e)
					{
						// The String.Format method raised an exception.
						LogInternalError("LogAssert: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs whether a variable is assigned or not with
		///   the default log level.
		/// </summary>
		/// <param name="title">The title of the variable.</param>
		/// <param name="instance">
		///   The variable which should be checked for null.
		/// </param>
		/// <!--
		/// <remarks>
		///   If the instance argument is null, then ": Not assigned",
		///   otherwise ": Assigned" will be appended to the title before
		///   the <link LogEntry, Log Entry> is sent.
		///
		///   This method is useful to check source code for null references
		///   in places where you experienced or expect problems and want to
		///   log possible null references.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogAssigned(string title, object instance)
		{
			LogAssigned(this.fParent.DefaultLevel, title, instance);
		}

		/// <summary>
		///   Overloaded. Logs whether a variable is assigned or not with
		///   a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title of the variable.</param>
		/// <param name="instance">
		///   The variable which should be checked for null.
		/// </param>
		/// <!--
		/// <remarks>
		///   If the instance argument is null, then ": Not assigned",
		///   otherwise ": Assigned" will be appended to the title before
		///   the <link LogEntry, Log Entry> is sent.
		///
		///   This method is useful to check source code for null references
		///   in places where you experienced or expect problems and want to
		///   log possible null references.
		/// </remarks>
		/// -->

		public void LogAssigned(Level level, string title, object instance)
		{
			if (IsOn(level))
			{
				if (instance != null)
				{
					LogMessage(title + ": Assigned");
				}
				else
				{
					LogMessage(title + ": Not assigned");
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a conditional message with the default log
		///   level.
		/// </summary>
		/// <param name="condition">The condition to evaluate.</param>
		/// <param name="title">The title of the conditional message.</param>
		/// <!--
		/// <remarks>
		///   This method only sends a message if the passed 'condition'
		///   argument evaluates to true. If 'condition' is false, this
		///   method has no effect and nothing is logged. This method is
		///   thus the counter piece to LogAssert.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogConditional(bool condition, string title)
		{
			LogConditional(this.fParent.DefaultLevel, condition, title);
		}

		/// <summary>
		///   Overloaded. Logs a conditional message with the default log
		///   level. The message is created with a format string and a
		///   related array of arguments.
		/// </summary>
		/// <param name="condition">The condition to evaluate.</param>
		/// <param name="titleFmt">
		///   The format string to create the conditional message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method only sends a message if the passed 'condition'
		///   argument evaluates to true. If 'condition' is false, this
		///   method has no effect and nothing is logged. This method is
		///   thus the counter piece to LogAssert.
		/// 
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed to
		///   the String.Format method and the resulting string will be the
		///   conditional message.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogConditional(bool condition, string titleFmt, 
			params object[] args)
		{
			LogConditional(this.fParent.DefaultLevel, condition, titleFmt,
				args);
		}

		/// <summary>
		///   Overloaded. Logs a conditional message with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="condition">The condition to evaluate.</param>
		/// <param name="title">The title of the conditional message.</param>
		/// <!--
		/// <remarks>
		///   This method only sends a message if the passed 'condition'
		///   argument evaluates to true. If 'condition' is false, this
		///   method has no effect and nothing is logged. This method is
		///   thus the counter piece to LogAssert.
		/// </remarks>
		/// -->

		public void LogConditional(Level level, bool condition, 
			string title)
		{
			if (IsOn(level))
			{
				if (condition)
				{
					SendLogEntry(level, title, LogEntryType.Conditional,
						ViewerId.Title);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a conditional message with a custom log
		///   level. The message is created with a format string and a
		///   related array of arguments.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="condition">The condition to evaluate.</param>
		/// <param name="titleFmt">
		///   The format string to create the conditional message.
		/// </param>
		/// <param name="args">
		///   The array of arguments for the format string.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method only sends a message if the passed 'condition'
		///   argument evaluates to true. If 'condition' is false, this
		///   method has no effect and nothing is logged. This method is
		///   thus the counter piece to LogAssert.
		/// 
		///   This version of the method accepts a format string and a
		///   related array of arguments. These parameters will be passed to
		///   the String.Format method and the resulting string will be the
		///   conditional message.
		/// </remarks>
		/// -->

		public void LogConditional(Level level, bool condition,
			string titleFmt, params object[] args)
		{
			if (IsOn(level))
			{
				if (condition)
				{
					try
					{
						string title = String.Format(titleFmt, args);
						SendLogEntry(level, title, LogEntryType.Conditional,
							ViewerId.Title);
					}
					catch (Exception e)
					{
						LogInternalError("LogConditional: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a bool value with the default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a boolean variable.
		///   A title like "name = True" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBool(string name, bool value)
		{
			LogBool(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a bool value with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a boolean variable.
		///   A title like "name = True" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogBool(Level level, string name, bool value)
		{
			if (IsOn(level))
			{
				string title = name + " = " + (value ? "True" : "False");
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a byte value with the default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a byte variable.
		///   A title like "name = 23" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogByte(string name, byte value)
		{
			LogByte(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a byte value with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a byte variable.
		///   A title like "name = 23" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogByte(Level level, string name, byte value)
		{
			LogByte(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a byte value with an optional hexadecimal
		///   representation and default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a byte variable.
		///   If you set the includeHex argument to true then the
		///   hexadecimal representation of the supplied variable value
		///   is included as well.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogByte(string name, byte value, bool includeHex)
		{
			LogByte(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a byte value with an optional hexadecimal
		///   representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a byte variable.
		///   If you set the includeHex argument to true then the
		///   hexadecimal representation of the supplied variable value
		///   is included as well.
		/// </remarks>
		/// -->

		public void LogByte(Level level, string name, byte value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				StringBuilder title = new StringBuilder();

				title.Append(name);
				title.Append(" = ");
				title.Append(value);

				if (includeHex)
				{
					title.Append(" (0x");
					title.Append(value.ToString("x2"));
					title.Append(")");
				}

				SendLogEntry(level, title.ToString(), 
					LogEntryType.VariableValue, ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a short integer value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a short integer
		///   variable. A title like "name = 23" will be displayed
		///   in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogShort(string name, short value)
		{
			LogShort(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a short integer value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a short integer
		///   variable. A title like "name = 23" will be displayed
		///   in the Console.
		/// </remarks>
		/// -->

		public void LogShort(Level level, string name, short value)
		{
			LogShort(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a short integer value with an optional
		///   hexadecimal representation and default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a short integer
		///   variable. If you set the includeHex argument to true then
		///   the hexadecimal representation of the supplied variable
		///   value is included as well.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogShort(string name, short value, bool includeHex)
		{
			LogShort(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a short integer value with an optional
		///   hexadecimal representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a short integer
		///   variable. If you set the includeHex argument to true then
		///   the hexadecimal representation of the supplied variable
		///   value is included as well.
		/// </remarks>
		/// -->

		public void LogShort(Level level, string name, short value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				StringBuilder title = new StringBuilder();

				title.Append(name);
				title.Append(" = ");
				title.Append(value);

				if (includeHex)
				{
					title.Append(" (0x");
					title.Append(value.ToString("x4"));
					title.Append(")");
				}

				SendLogEntry(level, title.ToString(), 
					LogEntryType.VariableValue, ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs an integer value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an integer
		///   variable. A title like "name = 23" will be displayed
		///   in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogInt(string name, int value)
		{
			LogInt(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an integer value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an integer
		///   variable. A title like "name = 23" will be displayed
		///   in the Console.
		/// </remarks>
		/// -->

		public void LogInt(Level level, string name, int value)
		{
			LogInt(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs an integer value with an optional hexadecimal
		///   representation and default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an integer variable.
		///   If you set the includeHex argument to true then the
		///   hexadecimal representation of the supplied variable value
		///   is included as well.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogInt(string name, int value, bool includeHex)
		{
			LogInt(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs an integer value with an optional hexadecimal
		///   representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an integer variable.
		///   If you set the includeHex argument to true then the
		///   hexadecimal representation of the supplied variable value
		///   is included as well.
		/// </remarks>
		/// -->

		public void LogInt(Level level, string name, int value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				StringBuilder title = new StringBuilder();

				title.Append(name);
				title.Append(" = ");
				title.Append(value);

				if (includeHex)
				{
					title.Append(" (0x");
					title.Append(value.ToString("x8"));
					title.Append(")");
				}

				SendLogEntry(level, title.ToString(), 
					LogEntryType.VariableValue, ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a long integer value with the default
		///   log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a long integer
		///   variable. A title like "name = 23" will be displayed in
		///   the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogLong(string name, long value)
		{
			LogLong(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a long integer value with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a long integer
		///   variable. A title like "name = 23" will be displayed in
		///   the Console.
		/// </remarks>
		/// -->

		public void LogLong(Level level, string name, long value)
		{
			LogLong(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a long integer value with an optional
		///   hexadecimal representation and default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a long integer
		///   variable. If you set the includeHex argument to true then
		///   the hexadecimal representation of the supplied variable
		///   value is included as well.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogLong(string name, long value, bool includeHex)
		{
			LogLong(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a long integer value with an optional
		///   hexadecimal representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a long integer
		///   variable. If you set the includeHex argument to true then
		///   the hexadecimal representation of the supplied variable
		///   value is included as well.
		/// </remarks>
		/// -->

		public void LogLong(Level level, string name, long value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				StringBuilder title = new StringBuilder();

				title.Append(name);
				title.Append(" = ");
				title.Append(value);

				if (includeHex)
				{
					title.Append(" (0x");
					title.Append(value.ToString("x16"));
					title.Append(")");
				}

				SendLogEntry(level, title.ToString(), 
					LogEntryType.VariableValue, ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a float value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a float variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogFloat(string name, float value)
		{
			LogFloat(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a float value with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a float variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogFloat(Level level, string name, float value)
		{
			if (IsOn(level))
			{
				string title = name + " = " + value;
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a double value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a double variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDouble(string name, double value)
		{
			LogDouble(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a double value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a double variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogDouble(Level level, string name, double value)
		{
			if (IsOn(level))
			{
				string title = name + " = " + value;
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a decimal value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a decimal variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDecimal(string name, decimal value)
		{
			LogDecimal(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a decimal value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a decimal variable.
		///   A title like "name = 3.1415" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogDecimal(Level level, string name, decimal value)
		{
			if (IsOn(level))
			{
				string title = name + " = " + value;
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a char value with the default log level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a char variable.
		///   A title like "name = 'c'" will be displayed in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogChar(string name, char value)
		{
			LogChar(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a char value with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a char variable.
		///   A title like "name = 'c'" will be displayed in the Console.
		/// </remarks>
		/// -->

		public void LogChar(Level level, string name, char value)
		{
			if (IsOn(level))
			{
				string title = name + " = '" + value + "'";
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a string value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a string variable.
		///   A title like "name = "string"" will be displayed in the
		///   Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogString(string name, string value)
		{
			LogString(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a string value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a string variable.
		///   A title like "name = "string"" will be displayed in the
		///   Console.
		/// </remarks>
		/// -->

		public void LogString(Level level, string name, string value)
		{
			if (IsOn(level))
			{
				string title = name + " = \"" + value + "\"";
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs a DateTime value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a DateTime variable.
		///   A title like "name = 26.11.2004 16:47:49" will be displayed
		///   in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDateTime(string name, DateTime value)
		{
			LogDateTime(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a DateTime value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of a DateTime variable.
		///   A title like "name = 26.11.2004 16:47:49" will be displayed
		///   in the Console.
		/// </remarks>
		/// -->

		public void LogDateTime(Level level, string name, DateTime value)
		{
			if (IsOn(level))
			{
				string title = name + " = " + value;
				SendLogEntry(level, title, LogEntryType.VariableValue, 
					ViewerId.Title);
			}
		}

		/// <summary>
		///   Overloaded. Logs an object value with the default log
		///   level.
		/// </summary>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an object. The title
		///   to display in the Console will consist of the name and the
		///   return value of the ToString method of the object.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogObjectValue(string name, object value)
		{
			LogObjectValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an object value with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The variable name.</param>
		/// <param name="value">The variable value.</param>
		/// <!--
		/// <remarks>
		///   This method logs the name and value of an object. The title
		///   to display in the Console will consist of the name and the
		///   return value of the ToString method of the object.
		/// </remarks>
		/// -->

		public void LogObjectValue(Level level, string name, object value)
		{
			if (IsOn(level))
			{
				if (value == null)
				{
					LogInternalError("LogObjectValue: value argument is null");
				}
				else
				{
					string title = name + " = " + value.ToString();
					SendLogEntry(level, title, LogEntryType.VariableValue, 
						ViewerId.Title);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a bool variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The bool value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogBool method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, bool value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a bool variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The bool value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogBool method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, bool value)
		{
			LogBool(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a byte variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The byte value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogByte method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, byte value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a byte variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The byte value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogByte method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, byte value)
		{
			LogByte(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a short integer
		///   variable with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">
		///   The short integer value of the variable.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogShort method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, short value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a short integer
		///   variable with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">
		///   The short integer value of the variable.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogShort method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, short value)
		{
			LogShort(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of an integer
		///   variable with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The integer value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogInt method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, int value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of an integer
		///   variable with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The integer value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogInt method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, int value)
		{
			LogInt(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a long integer
		///   variable with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">
		///   The long integer value of the variable.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogLong method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, long value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a long integer
		///   variable with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">
		///   The long integer value of the variable.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogLong method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, long value)
		{
			LogLong(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a float variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The float value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogFloat method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, float value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a float variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The float value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogFloat method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, float value)
		{
			LogFloat(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a double variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The double value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDouble method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, double value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a double variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The double value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDouble method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, double value)
		{
			LogDouble(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a decimal variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The decimal value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDecimal method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, decimal value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a decimal variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The decimal value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDecimal method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, decimal value)
		{
			LogDecimal(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a char variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The char value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogChar method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, char value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a char variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The char value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogChar method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, char value)
		{
			LogChar(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a string variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The string value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogString method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, string value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a string variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The string value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogString method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, string value)
		{
			LogString(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a DateTime variable
		///   with the default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The DateTime value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDateTime method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, DateTime value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of a DateTime variable
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The DateTime value of the variable.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogDateTime method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, DateTime value)
		{
			LogDateTime(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of an object with the
		///   default log level.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The object to log.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogObjectValue method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogValue(string name, object value)
		{
			LogValue(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs the name and value of an object with
		///   a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the variable.</param>
		/// <param name="value">The object to log.</param>
		/// <!--
		/// <remarks>
		///   This method just calls the LogObjectValue method.
		/// </remarks>
		/// -->

		public void LogValue(Level level, string name, object value)
		{
			LogObjectValue(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a custom viewer context with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="ctx">
		///   The viewer context which holds the actual data and the
		///   appropriate viewer ID.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ViewerContext"/>
		/// <remarks>
		///   This method can be used to extend the capabilities of the
		///   SmartInspect .NET library. You can assemble a so called viewer
		///   context and thus can send custom data to the SmartInspect
		///   Console. Furthermore, you can choose the viewer in which your
		///   data should be displayed. Every viewer in the Console has
		///   a corresponding viewer context class in this library. 
		/// 
		///   Have a look at the ViewerContext class and its derived classes
		///   to see a list of available viewer context classes.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomContext(string title, LogEntryType lt, 
			ViewerContext ctx)
		{
			LogCustomContext(this.fParent.DefaultLevel, title, lt, ctx);
		}

		/// <summary>
		///   Overloaded. Logs a custom viewer context with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="ctx">
		///   The viewer context which holds the actual data and the
		///   appropriate viewer ID.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ViewerContext"/>
		/// <remarks>
		///   This method can be used to extend the capabilities of the
		///   SmartInspect .NET library. You can assemble a so called viewer
		///   context and thus can send custom data to the SmartInspect
		///   Console. Furthermore, you can choose the viewer in which your
		///   data should be displayed. Every viewer in the Console has
		///   a corresponding viewer context class in this library.
		/// 
		///   Have a look at the ViewerContext class and its derived classes
		///   to see a list of available viewer context classes.
		/// </remarks>
		/// -->

		public void LogCustomContext(Level level, string title, 
			LogEntryType lt, ViewerContext ctx)
		{
			if (IsOn(level))
			{
				if (ctx == null)
				{
					LogInternalError("LogCustomContext: ctx argument is null");
				}
				else 
				{
					SendContext(level, title, lt, ctx);
				}
			}
		}

		/* Internal Send methods go here. */

		private void SendContext(Level level, string title, LogEntryType lt, 
			ViewerContext ctx)
		{
			SendLogEntry(level, title, lt, ctx.ViewerId, Color, 
				ctx.ViewerData);
		}

		private void SendLogEntry(Level level, string title, LogEntryType lt,
			ViewerId vi)
		{
			SendLogEntry(level, title, lt, vi, Color, null);
		}

		private void SendLogEntry(Level level, string title, LogEntryType lt,
			ViewerId vi, Color color, Stream data)
		{
			LogEntry logEntry = new LogEntry(lt, vi);

			// Set the properties we already know.
			logEntry.Timestamp = this.fParent.Now();
			logEntry.Title = title;
			logEntry.SessionName = this.fName; // Our session name.
		    logEntry.CorrelationId = fCorrelationId;
            logEntry.Data = data;
			logEntry.Level = level;

			if (color == DEFAULT_COLOR)
			{
				logEntry.Color = color; /* Transparent */
			}
			else 
			{
				int rgb = color.ToArgb() & 0xffffff;
				logEntry.Color = Color.FromArgb(rgb);
			}

			// Then send the new Log Entry.
			this.fParent.SendLogEntry(logEntry);
		}

		private void SendControlCommand(ControlCommandType ct, Stream data)
		{
			ControlCommand controlCommand = new ControlCommand(ct);
			controlCommand.Data = data;
			controlCommand.Level = Level.Control;
			this.fParent.SendControlCommand(controlCommand);
		}

		private void SendWatch(Level level, string name, string value, 
			WatchType wt)
		{
			Watch watch = new Watch(wt);
			watch.Timestamp = this.fParent.Now();
			watch.Name = name;
			watch.Value = value;
			watch.Level = level;
			this.fParent.SendWatch(watch);
		}

		private void SendProcessFlow(Level level, string title, 
			ProcessFlowType pt)
		{
			ProcessFlow processFlow = new ProcessFlow(pt);
			processFlow.Timestamp = this.fParent.Now();
			processFlow.Title = title;
			processFlow.Level = level;
			this.fParent.SendProcessFlow(processFlow);
		}

		/// <summary>
		///   Overloaded. Logs a text using a custom Log Entry type and
		///   viewer ID and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="text">The text to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console handles
		///   the text content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomText(string title, string text,
			LogEntryType lt, ViewerId vi)
		{
			LogCustomText(this.fParent.DefaultLevel, title, text, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs a text using a custom Log Entry type and
		///   viewer ID and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="text">The text to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console handles
		///   the text content.
		/// </param>

		public void LogCustomText(Level level, string title, string text, 
			LogEntryType lt, ViewerId vi)
		{
			if (IsOn(level))
			{
				using (TextContext ctx = new TextContext(vi))
				{
					try
					{
						ctx.LoadFromText(text);
						SendContext(level, title, lt, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogCustomText: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a file using a custom
		///   Log Entry type and viewer ID and default log level.
		/// </summary>
		/// <param name="fileName">The file to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the file content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied file using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the file is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		///   
		///   This version of the method uses the fileName argument as
		///   title to display in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomFile(string fileName, LogEntryType lt,
			ViewerId vi)
		{
			LogCustomFile(this.fParent.DefaultLevel, fileName, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file using a custom
		///   Log Entry type and viewer ID and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">The file to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the file content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied file using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the file is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		///   
		///   This version of the method uses the fileName argument as
		///   title to display in the Console.
		/// </remarks>
		/// -->

		public void LogCustomFile(Level level, string fileName, 
			LogEntryType lt, ViewerId vi)
		{
			LogCustomFile(level, fileName, fileName, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file using a custom
		///   Log Entry type, viewer ID and title and default log level.
		/// </summary>
		/// <param name="fileName">The file to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the file content.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied file using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the file is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomFile(string title, string fileName,
			LogEntryType lt, ViewerId vi)
		{
			LogCustomFile(this.fParent.DefaultLevel, title, fileName, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file using a custom
		///   Log Entry type, viewer ID and title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">The file to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the file content.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied file using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the file is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// </remarks>
		/// -->

		public void LogCustomFile(Level level, string title, string fileName, 
			LogEntryType lt, ViewerId vi)
		{
			if (IsOn(level))
			{
				using (BinaryContext ctx = new BinaryContext(vi))
				{
					try 
					{
						ctx.LoadFromFile(fileName);
						SendContext(level, title, lt, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogCustomFile: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a stream using a custom Log
		///   Entry type and viewer ID and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the stream content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied stream using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the stream is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomStream(string title, Stream stream,
			LogEntryType lt, ViewerId vi)
		{
			LogCustomStream(this.fParent.DefaultLevel, title, stream, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs the content of a stream using a custom Log
		///   Entry type and viewer ID and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the stream content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied stream using a
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the stream is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// </remarks>
		/// -->

		public void LogCustomStream(Level level, string title, Stream stream, 
			LogEntryType lt, ViewerId vi)
		{
			if (IsOn(level))
			{
				using (BinaryContext ctx = new BinaryContext(vi))
				{
					try 
					{
						ctx.LoadFromStream(stream);
						SendContext(level, title, lt, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogCustomStream: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a reader using a custom Log
		///   Entry type and viewer ID and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the reader content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied reader using 
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the reader is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCustomReader(string title, TextReader reader,
			LogEntryType lt, ViewerId vi)
		{
			LogCustomReader(this.fParent.DefaultLevel, title, reader, lt, vi);
		}

		/// <summary>
		///   Overloaded. Logs the content of a reader using a custom Log
		///   Entry type and viewer ID and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to log.</param>
		/// <param name="lt">The custom Log Entry type.</param>
		/// <param name="vi">
		///   The custom viewer ID which specifies the way the Console
		///   handles the reader content.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied reader using 
		///   custom Log Entry type and viewer ID. The parameters control
		///   the way the content of the reader is displayed in the Console.
		///   Thus you can extend the functionality of the SmartInspect
		///   .NET library with this method.
		/// </remarks>
		/// -->

		public void LogCustomReader(Level level, string title, 
			TextReader reader, LogEntryType lt, ViewerId vi)
		{
			if (IsOn(level))
			{
				using (TextContext ctx = new TextContext(vi))
				{
					try 
					{
						ctx.LoadFromReader(reader);
						SendContext(level, title, lt, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogCustomReader: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a string with the default log level and
		///   displays it in a read-only text field.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="text">The text to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogText(string title, string text)
		{
			LogText(this.fParent.DefaultLevel, title, text);
		}

		/// <summary>
		///   Overloaded. Logs a string with a custom log level and displays
		///   it in a read-only text field.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="text">The text to log.</param>

		public void LogText(Level level, string title, string text)
		{
			LogCustomText(level, title, text, LogEntryType.Text, 
				ViewerId.Data);
		}

		/// <summary>
		///   Overloaded. Logs a text file with the default log level and
		///   displays the content in a read-only text field.
		/// </summary>
		/// <param name="fileName">The file to log.</param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as
		///   title to display in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogTextFile(string fileName)
		{
			LogTextFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a text file with a custom log level and
		///   displays the content in a read-only text field.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">The file to log.</param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as
		///   title to display in the Console.
		/// </remarks>
		/// -->

		public void LogTextFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Text, 
				ViewerId.Data);
		}

		/// <summary>
		///   Overloaded. Logs a text file and displays the content in a
		///   read-only text field using a custom title and default log
		///   level.
		/// </summary>
		/// <param name="fileName">The file to log.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogTextFile(string title, string fileName)
		{
			LogTextFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a text file and displays the content in a
		///   read-only text field using a custom title and custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">The file to log.</param>
		/// <param name="title">The title to display in the Console.</param>

		public void LogTextFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Text, 
				ViewerId.Data);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   displays the content in a read-only text field.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogTextStream(string title, Stream stream)
		{
			LogTextStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and displays
		///   the content in a read-only text field.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to log.</param>

		public void LogTextStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Text, 
				ViewerId.Data);
		}

		/// <summary>
		///   Overloaded. Logs a reader with the default log level and
		///   displays the content in a read-only text field.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogTextReader(string title, TextReader reader)
		{
			LogTextReader(this.fParent.DefaultLevel, title, reader);
		}

		/// <summary>
		///   Overloaded. Logs a reader with a custom log level and
		///   displays the content in a read-only text field.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to log.</param>

		public void LogTextReader(Level level, string title, TextReader reader)
		{
			LogCustomReader(level, title, reader, LogEntryType.Text, 
				ViewerId.Data);
		}

		/// <summary>
		///   Overloaded. Logs HTML code with the default log level and
		///   displays it in a web browser.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="html">The HTML source code to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the supplied HTML source code. The source
		///   code is displayed as a website in the web viewer of the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogHtml(string title, string html)
		{
			LogHtml(this.fParent.DefaultLevel, title, html);
		}

		/// <summary>
		///   Overloaded. Logs HTML code with a custom log level and
		///   displays it in a web browser.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="html">The HTML source code to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the supplied HTML source code. The source
		///   code is displayed as a website in the web viewer of the Console.
		/// </remarks>
		/// -->

		public void LogHtml(Level level, string title, string html)
		{
			LogCustomText(level, title, html, LogEntryType.WebContent, 
				ViewerId.Web);
		}

		/// <summary>
		///   Overloaded. Logs an HTML file with the default log level and
		///   displays the content in a web browser.
		/// </summary>
		/// <param name="fileName">The HTML file to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied file. The
		///   source code is displayed as a website in the web viewer of the
		///   Console. 
		///  
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogHtmlFile(string fileName)
		{
			LogHtmlFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs an HTML file with a custom log level and
		///   displays the content in a web browser.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">The HTML file to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied file. The
		///   source code is displayed as a website in the web viewer of the
		///   Console. 
		///  
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogHtmlFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.WebContent, 
				ViewerId.Web);
		}

		/// <summary>
		///   Overloaded. Logs an HTML file and displays the content in a
		///   web browser using a custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">The HTML file to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied file. The
		///   source code is displayed as a website in the web viewer of the
		///   Console. 
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogHtmlFile(string title, string fileName)
		{
			LogHtmlFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs an HTML file and displays the content in a
		///   web browser using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">The HTML file to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied file. The
		///   source code is displayed as a website in the web viewer of the
		///   Console. 
		/// </remarks>
		/// -->

		public void LogHtmlFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.WebContent, 
				ViewerId.Web);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   displays the content in a web browser.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied stream.
		///   The source code is displayed as a website in the web viewer of
		///   the Console. 
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogHtmlStream(string title, Stream stream)
		{
			LogHtmlStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and displays
		///   the content in a web browser.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied stream.
		///   The source code is displayed as a website in the web viewer of
		///   the Console. 
		/// </remarks>
		/// -->

		public void LogHtmlStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.WebContent, 
				ViewerId.Web);
		}

		/// <summary>
		///   Overloaded. Logs a reader with the default log level and
		///   displays the content in a web browser.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied reader.
		///   The source code is displayed as a website in the web viewer of
		///   the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogHtmlReader(string title, TextReader reader)
		{
			LogHtmlReader(this.fParent.DefaultLevel, title, reader);
		}

		/// <summary>
		///   Overloaded. Logs a reader with a custom log level and displays
		///   the content in a web browser.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">The reader to display.</param>
		/// <!--
		/// <remarks>
		///   This method logs the HTML source code of the supplied reader.
		///   The source code is displayed as a website in the web viewer of
		///   the Console.
		/// </remarks>
		/// -->

		public void LogHtmlReader(Level level, string title, TextReader reader)
		{
			LogCustomReader(level, title, reader, LogEntryType.WebContent, 
				ViewerId.Web);
		}

		/// <summary>
		///   Overloaded. Logs a byte array with the default log level and
		///   displays it in a hex viewer.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="buffer">
		///   The byte array to display in the hex viewer.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBinary(string title, byte[] buffer)
		{
			LogBinary(this.fParent.DefaultLevel, title, buffer);
		}

		/// <summary>
		///   Overloaded. Logs a byte array with a custom log level and
		///   displays it in a hex viewer.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="buffer">
		///   The byte array to display in the hex viewer.
		/// </param>

		public void LogBinary(Level level, string title, byte[] buffer)
		{
			if (IsOn(level))
			{
				using (BinaryViewerContext ctx = new BinaryViewerContext())
				{
					try
					{
						ctx.AppendBytes(buffer);
						SendContext(level, title, LogEntryType.Binary, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogBinary: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a byte array with the default log level and
		///   displays it in a hex viewer.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="buffer">The buffer to display data from.</param>
		/// <param name="offset">
		///   The byte offset of buffer at which to display data from.
		/// </param>
		/// <param name="count">The amount of bytes to display.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBinary(string title, byte[] buffer, int offset,
			int count)
		{ 
			LogBinary(this.fParent.DefaultLevel, title, buffer, offset, count);
		}

		/// <summary>
		///   Overloaded. Logs a byte array with a custom log level and
		///   displays it in a hex viewer.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="buffer">The buffer to display data from.</param>
		/// <param name="offset">
		///   The byte offset of buffer at which to display data from.
		/// </param>
		/// <param name="count">The amount of bytes to display.</param>

		public void LogBinary(Level level, string title, byte[] buffer, 
			int offset, int count)
		{
			if (IsOn(level))
			{
				using (BinaryViewerContext ctx = new BinaryViewerContext())
				{
					try
					{
						ctx.AppendBytes(buffer, offset, count);
						SendContext(level, title, LogEntryType.Binary, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogBinary: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a binary file with the default log level and
		///   displays its content in a hex viewer.
		/// </summary>
		/// <param name="fileName">
		///   The binary file to display in a hex viewer.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBinaryFile(string fileName)
		{
			LogBinaryFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a binary file with a custom log level and
		///   displays its content in a hex viewer.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The binary file to display in a hex viewer.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the supplied fileName as
		///   title to display in the Console.
		/// </remarks>
		/// -->

		public void LogBinaryFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Binary, 
				ViewerId.Binary);
		}

		/// <summary>
		///   Overloaded. Logs a binary file and displays its content in
		///   a hex viewer using a custom title and default log level.
		/// </summary>
		/// <param name="fileName">
		///   The binary file to display in a hex viewer.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBinaryFile(string title, string fileName)
		{
			LogBinaryFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a binary file and displays its content in
		///   a hex viewer using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The binary file to display in a hex viewer.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>

		public void LogBinaryFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Binary, 
				ViewerId.Binary);
		}

		/// <summary>
		///   Overloaded. Logs a binary stream with the default log level and
		///   displays its content in a hex viewer.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The binary stream to display in a hex viewer.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBinaryStream(string title, Stream stream)
		{
			LogBinaryStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a binary stream with a custom log level and
		///   displays its content in a hex viewer.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The binary stream to display in a hex viewer.
		/// </param>

		public void LogBinaryStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Binary, 
				ViewerId.Binary);
		}

		/// <summary>
		///   Overloaded. Logs a bitmap with the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="bitmap">The bitmap to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->
/*
		public void LogBitmap(string title, Bitmap bitmap)
		{
			LogBitmap(this.fParent.DefaultLevel, title, bitmap);
		}

		/// <summary>
		///   Overloaded. Logs a bitmap with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="bitmap">The bitmap to log.</param>

		public void LogBitmap(Level level, string title, Bitmap bitmap)
		{
			if (IsOn(level))
			{
				if (bitmap == null)
				{
					LogInternalError("LogBitmap: bitmap argument is null");
				}
				else
				{
					using (MemoryStream ms = new MemoryStream())
					{
						try 
						{
							bitmap.Save(ms, ImageFormat.Bmp);
							ms.Position = 0;
							LogBitmapStream(level, title, ms);
						}
						catch (Exception e)
						{
							LogInternalError("LogBitmap:" + e.Message);
						}
					}
				}
			}
		}
*/
		/// <summary>
		///   Overloaded. Logs a bitmap file with the default log level and
		///   displays it in the Console.
		/// </summary>
		/// <param name="fileName">
		///   The bitmap file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBitmapFile(string fileName)
		{
			LogBitmapFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a bitmap file with a custom log level and
		///   displays it in the Console.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The bitmap file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the supplied fileName as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogBitmapFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Graphic, 
				ViewerId.Bitmap);
		}

		/// <summary>
		///   Overloaded. Logs a bitmap file and displays it in the Console
		///   using a custom title and default log level.
		/// </summary>
		/// <param name="fileName">
		///   The bitmap file to display in the Console.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBitmapFile(string title, string fileName)
		{
			LogBitmapFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a bitmap file and displays it in the Console
		///   using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The bitmap file to display in the Console.
		/// </param>
		/// <param name="title">The title to display in the Console.</param>

		public void LogBitmapFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Graphic, 
				ViewerId.Bitmap);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   interprets its content as a bitmap.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as bitmap.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogBitmapStream(string title, Stream stream)
		{
			LogBitmapStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and
		///   interprets its content as a bitmap.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as bitmap.</param>

		public void LogBitmapStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Graphic, 
				ViewerId.Bitmap);
		}

		/// <summary>
		///   Overloaded. Logs a JPEG file with the default log level and
		///   displays it in the Console.
		/// </summary>
		/// <param name="fileName">
		///   The JPEG file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogJpegFile(string fileName)
		{
			LogJpegFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a JPEG file with a custom log level and
		///   displays it in the Console.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The JPEG file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogJpegFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Graphic, 
				ViewerId.Jpeg);
		}

		/// <summary>
		///   Overloaded. Logs a JPEG file and displays it in the Console
		///   using a custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The JPEG file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogJpegFile(string title, string fileName)
		{
			LogJpegFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a JPEG file and displays it in the Console
		///   using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The JPEG file to display in the Console.
		/// </param>

		public void LogJpegFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Graphic, 
				ViewerId.Jpeg);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   interprets its content as JPEG image.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as JPEG image.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogJpegStream(string title, Stream stream)
		{
			LogJpegStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and
		///   interprets its content as JPEG image.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as JPEG image.</param>

		public void LogJpegStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Graphic, 
				ViewerId.Jpeg);
		}

		/// <summary>
		///   Overloaded. Logs a Windows icon with the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="icon">The Windows icon to log.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->
/*
		public void LogIcon(string title, Icon icon)
		{
			LogIcon(this.fParent.DefaultLevel, title, icon);
		}

		/// <summary>
		///   Overloaded. Logs a Windows icon with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="icon">The Windows icon to log.</param>

		public void LogIcon(Level level, string title, Icon icon)
		{
			if (IsOn(level))
			{
				if (icon == null)
				{
					LogInternalError("LogIcon: icon argument is null");
				}
				else
				{
					using (MemoryStream ms = new MemoryStream())
					{
						try 
						{
							icon.Save(ms);
							ms.Position = 0;
							LogIconStream(level, title, ms);
						}
						catch (Exception e)
						{
							LogInternalError("LogIcon:" + e.Message);
						}
					}
				}
			}
		}
*/
		/// <summary>
		///   Overloaded. Logs a Windows icon file using the default log
		///   level and displays it in the Console.
		/// </summary>
		/// <param name="fileName">
		///   The Windows icon file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogIconFile(string fileName)
		{
			LogIconFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a Windows icon file using a custom log level
		///   and displays it in the Console.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The Windows icon file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogIconFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Graphic, 
				ViewerId.Icon);
		}

		/// <summary>
		///   Overloaded. Logs a Windows icon file and displays it in the
		///   Console using a custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The Windows icon file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogIconFile(string title, string fileName)
		{
			LogIconFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a Windows icon file and displays it in the
		///   Console using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The Windows icon file to display in the Console.
		/// </param>

		public void LogIconFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Graphic, 
				ViewerId.Icon);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   interprets its content as Windows icon.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as Windows icon.</param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogIconStream(string title, Stream stream)
		{
			LogIconStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and
		///   interprets its content as Windows icon.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">The stream to display as Windows icon.</param>

		public void LogIconStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Graphic, 
				ViewerId.Icon);
		}

		/// <summary>
		///   Overloaded. Logs a Windows Metafile file with the default log
		///   level and displays it in the Console.
		/// </summary>
		/// <param name="fileName">
		///   The Windows Metafile file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogMetafileFile(string fileName)
		{
			LogMetafileFile(this.fParent.DefaultLevel, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a Windows Metafile file with a custom log
		///   level and displays it in the Console.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The Windows Metafile file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogMetafileFile(Level level, string fileName)
		{
			LogCustomFile(level, fileName, LogEntryType.Graphic, 
				ViewerId.Metafile);
		}

		/// <summary>
		///   Overloaded. Logs a Windows Metafile file and displays it in
		///   the Console using a custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The Windows Metafile file to display in the Console.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogMetafileFile(string title, string fileName)
		{
			LogMetafileFile(this.fParent.DefaultLevel, title, fileName);
		}

		/// <summary>
		///   Overloaded. Logs a Windows Metafile file and displays it in
		///   the Console using a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The Windows Metafile file to display in the Console.
		/// </param>

		public void LogMetafileFile(Level level, string title, string fileName)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Graphic, 
				ViewerId.Metafile);
		}

		/// <summary>
		///   Overloaded. Logs a stream with the default log level and
		///   interprets its content as Windows Metafile image.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream to display as Windows Metafile image.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogMetafileStream(string title, Stream stream)
		{
			LogMetafileStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs a stream with a custom log level and
		///   interprets its content as Windows Metafile image.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream to display as Windows Metafile image.
		/// </param>

		public void LogMetafileStream(Level level, string title, Stream stream)
		{
			LogCustomStream(level, title, stream, LogEntryType.Graphic, 
				ViewerId.Metafile);
		}

		/// <summary>
		///   Overloaded. Logs a string containing SQL source code with the
		///   default log level. The SQL source code is displayed with syntax
		///   highlighting in the Console.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="source">The SQL source code to log.</param>
		/// <!--
		/// <remarks>
		///   This method displays the supplied SQL source code with syntax
		///   highlighting in the Console.
		///
		///   It is especially useful to debug or track dynamically generated
		///   SQL source code.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSql(string title, string source)
		{
			LogSql(this.fParent.DefaultLevel, title, source);
		}

		/// <summary>
		///   Overloaded. Logs a string containing SQL source code with a
		///   custom log level. The SQL source code is displayed with syntax
		///   highlighting in the Console.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="source">The SQL source code to log.</param>
		/// <!--
		/// <remarks>
		///   This method displays the supplied SQL source code with syntax
		///   highlighting in the Console.
		///
		///   It is especially useful to debug or track dynamically generated
		///   SQL source code.
		/// </remarks>
		/// -->

		public void LogSql(Level level, string title, string source)
		{
			LogSource(level, title, source, SourceId.Sql);
		}

		/// <summary>
		///   Overloaded. Logs source code that is displayed with syntax
		///   highlighting in the Console using the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="source">The source code to log.</param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the supplied source code with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSource(string title, string source, SourceId id)
		{
			LogSource(this.fParent.DefaultLevel, title, source, id);
		}

		/// <summary>
		///   Overloaded. Logs source code that is displayed with syntax
		///   highlighting in the Console using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="source">The source code to log.</param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the supplied source code with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// </remarks>
		/// -->

		public void LogSource(Level level, string title, string source, 
			SourceId id)
		{
			LogCustomText(level, title, source, LogEntryType.Source, 
				(ViewerId) id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file as source code with
		///   syntax highlighting using the default log level.
		/// </summary>
		/// <param name="fileName">
		///   The name of the file which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the source file with syntax highlighting
		///   in the Console. The type of the source code can be specified by
		///   the 'id' argument. Please see the SourceId enum for information
		///   on the supported source code types.
		///   
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSourceFile(string fileName, SourceId id)
		{
			LogSourceFile(this.fParent.DefaultLevel, fileName, id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file as source code with
		///   syntax highlighting using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="fileName">
		///   The name of the file which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the source file with syntax highlighting
		///   in the Console. The type of the source code can be specified by
		///   the 'id' argument. Please see the SourceId enum for information
		///   on the supported source code types.
		///   
		///   This version of the method uses the fileName argument as title
		///   to display in the Console.
		/// </remarks>
		/// -->

		public void LogSourceFile(Level level, string fileName, SourceId id)
		{
			LogCustomFile(level, fileName, LogEntryType.Source, (ViewerId) id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file as source code with
		///   syntax highlighting using a custom title and default log
		///   level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The name of the file which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the source file with syntax highlighting
		///   in the Console. The type of the source code can be specified by
		///   the 'id' argument. Please see the SourceId enum for information
		///   on the supported source code types.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSourceFile(string title, string fileName, SourceId id)
		{
			LogSourceFile(this.fParent.DefaultLevel, title, fileName, id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a file as source code with
		///   syntax highlighting using a custom title and custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="fileName">
		///   The name of the file which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the source file with syntax highlighting
		///   in the Console. The type of the source code can be specified by
		///   the 'id' argument. Please see the SourceId enum for information
		///   on the supported source code types.
		/// </remarks>
		/// -->

		public void LogSourceFile(Level level, string title, string fileName, 
			SourceId id)
		{
			LogCustomFile(level, title, fileName, LogEntryType.Source, 
				(ViewerId) id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a stream as source code with
		///   syntax highlighting using the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the content of a stream with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSourceStream(string title, Stream stream, SourceId id)
		{
			LogSourceStream(this.fParent.DefaultLevel, title, stream, id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a stream as source code with
		///   syntax highlighting using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the content of a stream with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// </remarks>
		/// -->

		public void LogSourceStream(Level level, string title, Stream stream, 
			SourceId id)
		{
			LogCustomStream(level, title, stream, LogEntryType.Source, 
				(ViewerId) id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a reader as source code with
		///   syntax highlighting using the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">
		///   The reader which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the content of a reader with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSourceReader(string title, TextReader reader,
			SourceId id)
		{
			LogSourceReader(this.fParent.DefaultLevel, title, reader, id);
		}

		/// <summary>
		///   Overloaded. Logs the content of a reader as source code with
		///   syntax highlighting using a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">
		///   The reader which contains the source code.
		/// </param>
		/// <param name="id">Specifies the type of source code.</param>
		/// <!--
		/// <remarks>
		///   This method displays the content of a reader with syntax
		///   highlighting in the Console. The type of the source code can be
		///   specified by the 'id' argument. Please see the SourceId enum for
		///   information on the supported source code types.
		/// </remarks>
		/// -->

		public void LogSourceReader(Level level, string title, 
			TextReader reader, SourceId id)
		{
			LogCustomReader(level, title, reader, LogEntryType.Source, 
				(ViewerId) id);
		}

		/// <summary>
		///   Overloaded. Logs fields and properties of an object with the
		///   default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="instance">
		///   The object whose public fields and properties should be
		///   logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all field and property names and their
		///   current values of an object. These key/value pairs will be
		///   displayed in the Console in an object inspector like viewer.
		///
		///   This version of the method logs only the public fields and
		///   properties of the supplied object.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogObject(string title, object instance)
		{
			LogObject(this.fParent.DefaultLevel, title, instance);
		}

		/// <summary>
		///   Overloaded. Logs fields and properties of an object with a
		///   custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="instance">
		///   The object whose public fields and properties should be
		///   logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all field and property names and their
		///   current values of an object. These key/value pairs will be
		///   displayed in the Console in an object inspector like viewer.
		///
		///   This version of the method logs only the public fields and
		///   properties of the supplied object.
		/// </remarks>
		/// -->

		public void LogObject(Level level, string title, object instance)
		{
			LogObject(level, title, instance, false);
		}

		/// <summary>
		///   Overloaded. Logs fields and properties of an object with the
		///   default log level. Lets you specify if non public members should
		///   also be logged.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="instance">
		///   The object whose fields and properties should be logged.
		/// </param>
		/// <param name="nonPublic">
		///   Specifies if non public members should also be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all field and property names and their
		///   current values of an object. These key/value pairs will be
		///   displayed in the Console in an object inspector like viewer.
		///   
		///   You can specify if non public or only public members should
		///   be logged by setting the nonPublic argument to true or false,
		///   respectively.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogObject(string title, object instance, bool nonPublic)
		{
			LogObject(this.fParent.DefaultLevel, title, instance, nonPublic);
		}

		/// <summary>
		///   Overloaded. Logs fields and properties of an object with a
		///   custom log level. Lets you specify if non public members should
		///   also be logged.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="instance">
		///   The object whose fields and properties should be logged.
		/// </param>
		/// <param name="nonPublic">
		///   Specifies if non public members should also be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all field and property names and their
		///   current values of an object. These key/value pairs will be
		///   displayed in the Console in an object inspector like viewer.
		///   
		///   You can specify if non public or only public members should
		///   be logged by setting the nonPublic argument to true or false,
		///   respectively.
		/// </remarks>
		/// -->

		public void LogObject(Level level, string title, object instance, 
			bool nonPublic)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (instance == null)
			{
				LogInternalError("LogObject: instance argument is null");
				return;
			}

			using (InspectorViewerContext ctx = new InspectorViewerContext())
			{
				Type t = instance.GetType();
				ArrayList list = new ArrayList();

				// Get all fields
				FieldInfo[] fields;
				if (nonPublic)
				{
					fields = t.GetFields(
							BindingFlags.Instance | BindingFlags.Public |
							BindingFlags.NonPublic
						);

					if (fields == null)
					{
						LogInternalError("LogObject: Permissions are missing");
						return;
					}
				}
				else
				{
					fields = t.GetFields();
				}

				// Store fields
				if (fields.Length > 0)
				{
					StringBuilder sb = new StringBuilder();

					foreach (FieldInfo f in fields)
					{
						try
						{
							sb.Append(ctx.EscapeItem(f.Name));
							sb.Append("=");
							sb.Append(ctx.EscapeItem(
								ObjectRenderer.RenderObject(f.GetValue(instance)))
							);
						}
						catch
						{
							sb.Append("<not accessible>");
						}

						list.Add(sb.ToString());
						sb.Length = 0;
					}

					list.Sort();

					// Begin a new group and append the
					// list to the inspector context.

					ctx.StartGroup("Fields");
					foreach (object o in list)
					{
						ctx.AppendLine((string) o);
					}

					list.Clear();
				}

				// Get all properties
				PropertyInfo[] properties;
				if (nonPublic)
				{
					properties = t.GetProperties(
							BindingFlags.Instance | BindingFlags.Public |
							BindingFlags.NonPublic
						);

					if (properties == null)
					{
						LogInternalError("LogObject: Permissions are missing");
						return;
					}
				}
				else
				{
					properties = t.GetProperties();
				}

				// Store properties
				if (properties.Length > 0)
				{
					StringBuilder sb = new StringBuilder();

					foreach (PropertyInfo p in properties)
					{
						try
						{
							sb.Append(ctx.EscapeItem(p.Name));
							sb.Append("=");
							sb.Append(ctx.EscapeItem(
								ObjectRenderer.RenderObject(p.GetValue(instance, null)))
							);
						}
						catch
						{
							sb.Append("<not accessible>");
						}

						list.Add(sb.ToString());
						sb.Length = 0;
					}

					list.Sort();

					// Begin a new group and append the
					// list to the inspector context.

					ctx.StartGroup("Properties");
					foreach (object o in list)
					{
						ctx.AppendLine((string) o);
					}

					list.Clear();
				}

				SendContext(level, title, LogEntryType.Object, ctx);
			}
		}

		/// <summary>
		///   Acts as an event handler for the Application.ThreadException
		///   event.
		/// </summary>
		/// <param name="sender">The object which fired the event.</param>
		/// <param name="e">The arguments for the event.</param>
		/// <!--
		/// <remarks>
		///   This method acts as an event handler for the
		///   System.Windows.Forms.Application.ThreadException event
		///   to catch unhandled exceptions in Windows Forms applications.
		///   
		///   This event handler logs the unhandled exception using
		///   the LogException method. Furthermore, this exception
		///   <link LogEntry, Log Entry> will be surrounded by calls
		///   to the EnterMethod and LeaveMethod methods for a better
		///   identification in the Console.
		/// </remarks>
		/// <example>
		/// <code>
		///   // [C# Example]
		///   Application.ThreadException +=
		///     new ThreadExceptionEventHandler(SiAuto.Main.ThreadExceptionHandler);
		/// </code>
		/// 
		/// <code>
		///   ' [VB.NET Example]
		///   AddHandler Application.ThreadException, _
		///     AddressOf SiAuto.Main.ThreadExceptionHandler
		/// </code>
		/// </example>
		/// -->

		public void ThreadExceptionHandler(object sender, 
			ThreadExceptionEventArgs e)
		{
			if (IsOn(Level.Error))
			{
				EnterMethod(Level.Error, "ThreadExceptionHandler");
				try 
				{
					if (e == null)
					{
						// Shouldn't happen, but to be sure ..
						LogInternalError("ThreadExceptionHandler: e argument is null");
					}
					else
					{
						// Just call LogException here.
						LogException(e.Exception);
					}
				}
				finally 
				{
					LeaveMethod(Level.Error, "ThreadExceptionHandler");
				}
			}
		}

		/// <summary>
		///   Acts as an event handler for the AppDomain.UnhandledException
		///   event.
		/// </summary>
		/// <param name="sender">The object which fired the event.</param>
		/// <param name="e">The arguments for the event.</param>
		/// <!--
		/// <remarks>
		///   This method acts as an event handler for the
		///   System.AppDomain.UnhandledException event to catch
		///   unhandled exceptions in console applications.
		///   
		///   This event handler logs the unhandled exception using the
		///   LogException method. Furthermore, this exception <link LogEntry,
		///   LogEntry> will be surrounded by calls to the EnterMethod and
		///   LeaveMethod methods for a better identification in the Console.
		/// </remarks>
		/// <example>
		/// <code>
		///   // [C# Example]
		///   AppDomain.CurrentDomain.UnhandledException += 
		///	    new UnhandledExceptionEventHandler(SiAuto.Main.UnhandledExceptionHandler);
		/// </code>
		/// 
		/// <code>
		///   ' [VB.NET Example[
		///   AddHandler AppDomain.CurrentDomain.UnhandledException, _
		///     AddressOf SiAuto.Main.UnhandledExceptionHandler
		/// </code>
		/// </example>
		/// -->
		
		public void UnhandledExceptionHandler(object sender, 
			UnhandledExceptionEventArgs e)
		{
			if (IsOn(Level.Error))
			{
				EnterMethod(Level.Error, "UnhandledExceptionHandler");
				try 
				{
					if (e == null)
					{
						// Shouldn't happen, but to be sure ..
						LogInternalError("UnhandledExceptionHandler: e argument is null");
						return;
					}

					Exception ex = e.ExceptionObject as Exception;

					if (ex == null)
					{
						// Shouldn't happen, but the supplied exception
						// object of the event arguments isn't an exception.

						LogInternalError(
							"UnhandledExceptionHandler: e doesn't have an exception"
						);
					}
					else
					{
						// Just call LogException here.
						LogException(ex);
					}
				}
				finally
				{
					LeaveMethod(Level.Error, "UnhandledExceptionHandler");
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the current stack trace with the default
		///   log level.
		/// </summary>
		/// <!-- 
		/// <remarks>
		///   This method logs the current stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes that are currently on the stack. Furthermore,
		///   the filename, line and columns numbers are included.
		///
		///   See LogStackTrace for a more general method which can handle
		///   any stack trace.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentStackTrace()
		{
			if (IsOn(this.fParent.DefaultLevel))
			{
				StackTrace strace = new StackTrace(1, true);
				LogStackTrace(this.fParent.DefaultLevel, 
					"Current stack trace", strace);
			}
		}

		/// <summary>
		///   Overloaded. Logs the current stack trace with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!-- 
		/// <remarks>
		///   This method logs the current stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes that are currently on the stack. Furthermore,
		///   the filename, line and columns numbers are included.
		///
		///   See LogStackTrace for a more general method which can handle
		///   any stack trace.
		/// </remarks>
		/// -->

		public void LogCurrentStackTrace(Level level)
		{
			if (IsOn(level))
			{
				StackTrace strace = new StackTrace(1, true);
				LogStackTrace(level, "Current stack trace", strace);
			}
		}

		/// <summary>
		///   Overloaded. Logs the current stack trace with a custom
		///   title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <!-- 
		/// <remarks>
		///   This method logs the current stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes that are currently on the stack. Furthermore,
		///   the filename, line and columns numbers are included.
		///
		///   See LogStackTrace for a more general method which can handle
		///   any stack trace.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentStackTrace(string title)
		{
			if (IsOn(this.fParent.DefaultLevel))
			{
				StackTrace strace = new StackTrace(1, true);
				LogStackTrace(this.fParent.DefaultLevel, title, strace);
			}
		}

		/// <summary>
		///   Overloaded. Logs the current stack trace with a custom
		///   title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!-- 
		/// <remarks>
		///   This method logs the current stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes that are currently on the stack. Furthermore,
		///   the filename, line and columns numbers are included.
		///
		///   See LogStackTrace for a more general method which can handle
		///   any stack trace.
		/// </remarks>
		/// -->

		public void LogCurrentStackTrace(Level level, string title)
		{
			if (IsOn(level))
			{
				StackTrace strace = new StackTrace(1, true);
				LogStackTrace(level, title, strace);
			}
		}

		/// <summary>
		///   Overloaded. Logs a stack trace with the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="strace">The StackTrace instance to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the supplied stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes lists. Furthermore the filename, line and
		///   columns numbers are included.
		///
		///   See LogCurrentStackTrace for a method which logs the current
		///   stack trace.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogStackTrace(string title, StackTrace strace)
		{
			LogStackTrace(this.fParent.DefaultLevel, title, strace);
		}

		/// <summary>
		///   Overloaded. Logs a stack trace with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="strace">The StackTrace instance to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the supplied stack trace. The resulting
		///   <link LogEntry, Log Entry> contains all methods including the
		///   related classes lists. Furthermore the filename, line and
		///   columns numbers are included.
		///
		///   See LogCurrentStackTrace for a method which logs the current
		///   stack trace.
		/// </remarks>
		/// -->

		public void LogStackTrace(Level level, string title, StackTrace strace)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (strace == null)
			{
				LogInternalError("LogStackTrace: strace argument is null");
				return;
			}

			using (ListViewerContext ctx = new ListViewerContext())
			{
				try
				{
					StringBuilder sb = new StringBuilder(256);

					for (int i = 0; i < strace.FrameCount; i++)
					{
						StackFrame sf = strace.GetFrame(i);

						// Include the class name in the stack frame.
						sb.Append(sf.GetMethod().DeclaringType.FullName);
						sb.Append(".");
						sb.Append(sf.ToString());

						ctx.AppendLine(sb.ToString());
						sb.Length = 0;
					}

					SendContext(level, title, LogEntryType.Text, ctx);
				}
				catch(Exception e) 
				{
					LogInternalError("LogStackTrace: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the tables of a DataSet instance with the
		///   default log level.
		/// </summary>
		/// <param name="dataSet">
		///   The DataSet instance whose tables should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all tables of a DataSet instance by calling
		///   the LogDataTable method for every table.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataSet(DataSet dataSet)
		{
			LogDataSet(this.fParent.DefaultLevel, dataSet);
		}

		/// <summary>
		///   Overloaded. Logs the tables of a DataSet instance with a
		///   custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="dataSet">
		///   The DataSet instance whose tables should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all tables of a DataSet instance by calling
		///   the LogDataTable method for every table.
		/// </remarks>
		/// -->

		public void LogDataSet(Level level, DataSet dataSet)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (dataSet == null)
			{
				LogInternalError("LogDataSet: dataSet argument is null");
			}
			else
			{
				DataTableCollection tables = dataSet.Tables;
				if (tables == null || tables.Count == 0)
				{
					// Send an informative message that this
					// dataset doesn't contain any tables at all.
					LogMessage("The supplied DataSet object contains 0 tables.");
				}
				else
				{
					if (tables.Count != 1)
					{
						// Send an informative message about the amount of
						// tables in this dataset unless this dataset contains
						// only one table. In the case of one table, this method
						// will behave like any other method which sends only
						// one Log Entry.

						LogMessage(
								"The supplied DataSet object contains {0} tables. " +
								"The tables are listed below.", tables.Count
							);
					}

					foreach (DataTable table in tables)
					{
						// Iterate through the entire table collection
						// and log every table using the LogDataTable method.
						LogDataTable(level, table.TableName, table);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a table with the default log
		///   level.
		/// </summary>
		/// <param name="table">The table to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied table.
		///
		///   LogDataTable is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   This version of the method uses the value of the TableName
		///   property as title to display in the Console. See LogDataSet
		///   for a method which can handle more than one table at once.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataTable(DataTable table)
		{
			LogDataTable(this.fParent.DefaultLevel, table);
		}

		/// <summary>
		///   Overloaded. Logs the content of a table with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="table">The table to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied table.
		///
		///   LogDataTable is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   This version of the method uses the value of the TableName
		///   property as title to display in the Console. See LogDataSet
		///   for a method which can handle more than one table at once.
		/// </remarks>
		/// -->

		public void LogDataTable(Level level, DataTable table)
		{
			if (IsOn(level))
			{
				if (table == null)
				{
					LogInternalError("LogDataTable: table argument is null");
				}
				else
				{
					// Just call the LogDataView method with the
					// default view of the supplied table. Use the
					// name of the table as title for the Console.
					LogDataView(level, table.TableName, table.DefaultView);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a table with a custom
		///   title and default log level.
		/// </summary>
		/// <param name="table">The table to log.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied table.
		///
		///   LogDataTable is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   See LogDataSet for a method which can handle more than one
		///   table at once.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataTable(string title, DataTable table)
		{
			LogDataTable(this.fParent.DefaultLevel, title, table);
		}

		/// <summary>
		///   Overloaded. Logs the content of a table with a custom
		///   title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="table">The table to log.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied table.
		///
		///   LogDataTable is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   See LogDataSet for a method which can handle more than one
		///   table at once.
		/// </remarks>
		/// -->

		public void LogDataTable(Level level, string title, DataTable table)
		{
			if (IsOn(level))
			{
				if (table == null)
				{
					LogInternalError("LogDataTable: table argument is null");
				}
				else
				{
					// Just call the LogDataView method with
					// the default view of the supplied table.
					LogDataView(level, title, table.DefaultView);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the table schemas of a DataSet instance
		///   with the default log level.
		/// </summary>
		/// <param name="dataSet">
		///   The DataSet instance whose table schemas should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all table schemas of a DataSet instance
		///   by calling the LogDataTableSchema method for every table.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataSetSchema(DataSet dataSet)
		{
			LogDataSetSchema(this.fParent.DefaultLevel, dataSet);
		}

		/// <summary>
		///   Overloaded. Logs the table schemas of a DataSet instance
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="dataSet">
		///   The DataSet instance whose table schemas should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs all table schemas of a DataSet instance
		///   by calling the LogDataTableSchema method for every table.
		/// </remarks>
		/// -->

		public void LogDataSetSchema(Level level, DataSet dataSet)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (dataSet == null)
			{
				LogInternalError("LogDataSetSchema: dataSet argument is null");
			}
			else
			{
				DataTableCollection tables = dataSet.Tables;
				if (tables == null || tables.Count == 0)
				{
					// Send an informative message that this
					// dataset doesn't contain any tables at all.
					LogMessage("The supplied DataSet object contains 0 tables.");
				}
				else
				{
					if (tables.Count != 1)
					{
						// Send an informative message about the amount of
						// tables in this dataset unless this dataset contains
						// only one table. In the case of one table, this method
						// will behave like any other method which sends only one
						// Log Entry.

						LogMessage(
								"The supplied DataSet object contains {0} tables. " +
								"The schemas are listed below.", tables.Count
							);
					}

					foreach (DataTable table in tables)
					{
						// Iterate through the entire table collection and log
						// every table schema using the LogDataTableSchema method.
						LogDataTableSchema(level, table.TableName, table);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the schema of a table with the default log
		///   level.
		/// </summary>
		/// <param name="table">The table whose schema should be logged.</param>
		/// <!--
		/// <remarks>
		///   This method logs the schema of the supplied table. A table
		///   schema contains the properties of every column in the table,
		///   including the name, type and more.
		///
		///   LogDataTableSchema is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw schema of query results.
		///
		///   This version of the method uses the value of the TableName
		///   property as title to display in the Console. See LogDataSetSchema
		///   for a method which can handle more than one table at once.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataTableSchema(DataTable table)
		{
			LogDataTableSchema(this.fParent.DefaultLevel, table);
		}

		/// <summary>
		///   Overloaded. Logs the schema of a table with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="table">The table whose schema should be logged.</param>
		/// <!--
		/// <remarks>
		///   This method logs the schema of the supplied table. A table
		///   schema contains the properties of every column in the table,
		///   including the name, type and more.
		///
		///   LogDataTableSchema is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw schema of query results.
		///
		///   This version of the method uses the value of the TableName
		///   property as title to display in the Console. See LogDataSetSchema
		///   for a method which can handle more than one table at once.
		/// </remarks>
		/// -->

		public void LogDataTableSchema(Level level, DataTable table)
		{
			if (IsOn(level))
			{
				if (table == null)
				{
					LogInternalError("LogDataTableSchema: table argument is null");
				}
				else
				{
					// Use the table name as title.
					LogDataTableSchema(level, table.TableName, table);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the schema of a table with a custom
		///   title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="table">
		///   The table whose schema should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the schema of the supplied table. A table
		///   schema contains the properties of every column in the table,
		///   including the name, type and more.
		///
		///   LogDataTableSchema is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw schema of query results.
		///
		///   See LogDataSetSchema for a method which can handle more than
		///   one table at once.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataTableSchema(string title, DataTable table)
		{
			LogDataTableSchema(this.fParent.DefaultLevel, title, table);
		}

		/// <summary>
		///   Overloaded. Logs the schema of a table with a custom
		///   title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="table">
		///   The table whose schema should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method logs the schema of the supplied table. A table
		///   schema contains the properties of every column in the table,
		///   including the name, type and more.
		///
		///   LogDataTableSchema is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw schema of query results.
		///
		///   See LogDataSetSchema for a method which can handle more than
		///   one table at once.
		/// </remarks>
		/// -->

		public void LogDataTableSchema(Level level, string title, 
			DataTable table)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (table == null)
			{
				LogInternalError("LogDataTableSchema: table argument is null");
				return;
			}

			DataColumnCollection columns = table.Columns;

			if (columns == null)
			{
				LogInternalError("LogDataTableSchema: table is empty");
				return;
			}

			using (TableViewerContext ctx = new TableViewerContext())
			{
				try
				{
					// Write the header first.
					ctx.AppendHeader(
							"Name, Type, \"Max Length\", \"Default Value\", " +
							"\"Allow Null\", Unique, \"Read Only\", " + 
							"\"Auto Increment\", \"Primary Key\""
						);

					// And then the columns.
					foreach (DataColumn column in columns)
					{
						ctx.BeginRow();
						try 
						{
							ctx.AddRowEntry(column.ColumnName);
							ctx.AddRowEntry(column.DataType.FullName);
							ctx.AddRowEntry(column.MaxLength);

							if (column.DefaultValue != null)
							{
								ctx.AddRowEntry(column.DefaultValue.ToString());
							}
							else 
							{
								ctx.AddRowEntry("<not set>");
							}

							ctx.AddRowEntry(column.AllowDBNull);
							ctx.AddRowEntry(column.Unique);
							ctx.AddRowEntry(column.ReadOnly);

							if (column.AutoIncrement)
							{
								// This column auto increments its values, format
								// the seed and step like in the following example:
								// True (Seed = 1, Step = 1)
								ctx.AddRowEntry(String.Format(
										"\"True (Seed = {0}, Step = {1})\"",
										column.AutoIncrementSeed, 
										column.AutoIncrementStep
									));
							}
							else
							{
								// This column doesn't auto increment
								// its value, we simply write False here.
								ctx.AddRowEntry("False");
							}

							bool isPrimaryKey = false;
							DataColumn[] pkeys = table.PrimaryKey;

							if (pkeys != null)
							{
								// Now we need to determine if this column is
								// a primary key. There is no other possibility than
								// to iterate through the entire primary key array.
								foreach (DataColumn pkey in pkeys)
								{
									if (pkey.Equals(column))
									{
										isPrimaryKey = true;
										break;
									}
								}
							}

							ctx.AddRowEntry(isPrimaryKey);
						}
						finally 
						{
							ctx.EndRow();
						}
					}

					SendContext(level, title, LogEntryType.DatabaseStructure, 
						ctx);
				}
				catch (Exception e)
				{
					LogInternalError("LogDataTableSchema: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a data view with the default
		///   log level.
		/// </summary>
		/// <param name="dataview">The data view to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied data view.
		///
		///   LogDataView is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   This version of the method uses the value of the TableName
		///   property of the underlying table as title to display in the
		///   Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDataView(DataView dataview)
		{
			LogDataView(this.fParent.DefaultLevel, dataview);
		}

		/// <summary>
		///   Overloaded. Logs the content of a data view with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="dataview">The data view to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied data view.
		///
		///   LogDataView is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///
		///   This version of the method uses the value of the TableName
		///   property of the underlying table as title to display in the
		///   Console.
		/// </remarks>
		/// -->

		public void LogDataView(Level level, DataView dataview)
		{
			if (IsOn(level))
			{
				if (dataview == null || dataview.Table == null)
				{
					LogInternalError("LogDataView: dataview is null");
				}
				else
				{
					DataTable table = dataview.Table;

					if (table == null)
					{
						LogInternalError("LogDataView: table is null");
					}
					else 
					{
						// Use the table name as title.
						LogDataView(level, table.TableName, dataview);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a data view with a
		///   custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="dataview">The data view to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied data view.
		///
		///   LogDataView is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		///	</remarks>
		/// -->

		public void LogDataView(string title, DataView dataview)
		{
			LogDataView(this.fParent.DefaultLevel, title, dataview);
		}

		/// <summary>
		///   Overloaded. Logs the content of a data view with a
		///   custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="dataview">The data view to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs the content of the supplied data view.
		///
		///   LogDataView is especially useful in database applications
		///   with lots of queries. It gives you the possibility to see the
		///   raw query results.
		///	</remarks>
		/// -->

		public void LogDataView(Level level, string title, DataView dataview)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (dataview == null || dataview.Table == null)
			{
				LogInternalError("LogDataView: dataview or related table is null");
				return;
			}

			DataColumnCollection columns = dataview.Table.Columns;

			if (columns == null)
			{
				LogInternalError("LogDataView: table argument contains no columns");
				return;
			}

			using (TableViewerContext ctx = new TableViewerContext())
			{
				try
				{
					ctx.BeginRow();
					try
					{
						// We need to write the headers of the
						// table, that means, the names of the columns.
						for (int i = 0, count = columns.Count; i < count; i++)
						{
							ctx.AddRowEntry(columns[i].ColumnName);
						}
					}
					finally 
					{
						ctx.EndRow();
					}

					// After we've written the table header, we
					// can now write the whole dataview content.
					foreach (DataRowView rowview in dataview)
					{
						ctx.BeginRow();
						try 
						{
							// For every row in the view we need to iterate through
							// the columns and need to extract the related field value.
							for (int i = 0, count = columns.Count; i < count; i++)
							{
								object field = rowview[i];

								if (field != null)
								{
									// Add the field to the current row.
									ctx.AddRowEntry(field.ToString());
								}
							}
						}
						finally
						{
							ctx.EndRow();
						}
					}

					SendContext(level, title, LogEntryType.DatabaseResult, ctx);
				}
				catch (Exception e)
				{
					LogInternalError("LogDataView: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of an exception with a log level
		///   of Level.Error.
		/// </summary>
		/// <param name="e">The exception to log.</param>
		/// <!--
		/// <remarks>
		///   This method extracts the exception message and stack trace
		///   from the supplied exception and logs an error with this data.
		///   It is especially useful if you place calls to this method in
		///   exception handlers. See LogError for a more general method
		///   with a similar intention.
		///
		///   This version of the method uses the exception message as
		///   title to display in the Console.
		/// </remarks>
		/// -->

		public void LogException(Exception e)
		{
			if (IsOn(Level.Error))
			{
				if (e == null)
				{
					LogInternalError("LogException: e argument is null");
				}
				else
				{
					LogException(e.Message, e);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of an exception with a custom
		///   title and a log level of Level.Error.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="e">The exception to log.</param>
		/// <!--
		/// <remarks>
		///   This method extracts the exception message and stack trace
		///   from the supplied exception and logs an error with this data.
		///   It is especially useful if you place calls to this method in
		///   exception handlers. See LogError for a more general method
		///   with a similar intention.
		/// </remarks>
		/// -->

		public void LogException(string title, Exception e)
		{
			if (IsOn(Level.Error))
			{
				if (e == null)
				{
					LogInternalError("LogException: e argument is null");
				}
				else
				{
					using (DataViewerContext ctx = new DataViewerContext())
					{
						try
						{
							ctx.LoadFromText(e.ToString());
							SendContext(Level.Error, title, LogEntryType.Error,
								ctx);
						}
						catch
						{
							LogInternalError("LogException: " + e.Message);
						}
					}
				}
			}
		}

        /// <summary>
        ///   Overloaded. Logs the content of an exception with a custom
        ///   title and a log level of Level.Error.
        /// </summary>
        /// <param name="level">The log level of this method call.</param>
        /// <param name="title">The title to display in the Console.</param>
        /// <param name="e">The exception to log.</param>
        /// <!--
        /// <remarks>
        ///   This method extracts the exception message and stack trace
        ///   from the supplied exception and logs an error with this data.
        ///   It is especially useful if you place calls to this method in
        ///   exception handlers. See LogError for a more general method
        ///   with a similar intention.
        /// </remarks>
        /// -->
        public void LogException(Level level, string title, Exception e)
        {
            if (IsOn(Level.Error))
            {
                if (e == null)
                {
                    LogInternalError("LogException: e argument is null");
                }
                else
                {
                    using (DataViewerContext ctx = new DataViewerContext())
                    {
                        try
                        {
                            ctx.LoadFromText(e.ToString());
                            SendContext(level, title, LogEntryType.Error,
                                ctx);
                        }
                        catch
                        {
                            LogInternalError("LogException: " + e.Message);
                        }
                    }
                }
            }
        }


		/// <summary>
		///   Overloaded. Logs the content of an enumerable with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="e">The enumerable to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied enumerable and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in
		///   the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogEnumerable(string title, IEnumerable e)
		{
			LogEnumerable(this.fParent.DefaultLevel, title, e);
		}

		/// <summary>
		///   Overloaded. Logs the content of an enumerable with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="e">The enumerable  to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied enumerable and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in
		///   the Console.
		/// </remarks>
		/// -->

		public void LogEnumerable(Level level, string title, IEnumerable e)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (e == null)
			{
				LogInternalError("LogEnumerable: e argument is null");
			}
			else
			{
				using (ListViewerContext ctx = new ListViewerContext())
				{
					try
					{
						foreach (object o in e)
						{
							if (o == e)
							{
								ctx.AppendLine("<cycle>");
							}
							else
							{
								ctx.AppendLine(ObjectRenderer.RenderObject(o));
							}
						}

						SendContext(level, title, LogEntryType.Text, ctx);
					}
					catch (Exception ex)
					{
						LogInternalError("LogEnumerable: " + ex.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a collection with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="c">The collection to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied collection and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in
		///   the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCollection(string title, ICollection c)
		{
			LogEnumerable(this.fParent.DefaultLevel, title, c);
		}

		/// <summary>
		///   Overloaded. Logs the content of a collection with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="c">The collection to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied collection and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in
		///   the Console.
		/// </remarks>
		/// -->

		public void LogCollection(Level level, string title, ICollection c)
		{
			LogEnumerable(level, title, c);
		}

		/// <summary>
		///   Overloaded. Logs the content of a dictionary with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="d">The dictionary to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied dictionary and
		///   <link ObjectRenderer.RenderObject, renders> every key/value
		///   pair into a string. These pairs will be displayed in a 
		///   key/value viewer in the Console.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogDictionary(string title, IDictionary d)
		{
			LogDictionary(this.fParent.DefaultLevel, title, d);
		}

		/// <summary>
		///   Overloaded. Logs the content of a dictionary with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="d">The dictionary to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied dictionary and
		///   <link ObjectRenderer.RenderObject, renders> every key/value
		///   pair into a string. These pairs will be displayed in a 
		///   key/value viewer in the Console.
		/// </remarks>
		/// -->

		public void LogDictionary(Level level, string title, IDictionary d)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (d == null)
			{
				LogInternalError("LogDictionary: d argument is null");
			}
			else
			{
				using (ValueListViewerContext ctx = new ValueListViewerContext())
				{
					try 
					{
						foreach (object key in d.Keys)
						{
							object value = d[key];
	
							ctx.AppendKeyValue(
								key == d ? "<cycle>" : ObjectRenderer.RenderObject(key),
								value == d ? "<cycle>" : ObjectRenderer.RenderObject(value)
							);
						}

						SendContext(level, title, LogEntryType.Text, ctx);
					}
					catch (Exception e)
					{
						LogInternalError("LogDictionary: " + e.Message);
					}
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of an array with the default log
		///   level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="array">The array to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied array and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in the
		///   Console. See LogCollection for a more general method which can
		///   handle any kind of collection.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogArray(string title, params object[] array)
		{
			LogArray(this.fParent.DefaultLevel, title, array);
		}

		/// <summary>
		///   Overloaded. Logs the content of an array with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="array">The array to log.</param>
		/// <!--
		/// <remarks>
		///   This method iterates through the supplied array and
		///   <link ObjectRenderer.RenderObject, renders> every element into
		///   a string. These elements will be displayed in a listview in the
		///   Console. See LogCollection for a more general method which can
		///   handle any kind of collection.
		/// </remarks>
		/// -->

		public void LogArray(Level level, string title, params object[] array)
		{
			LogCollection(level, title, array);
		}

		/// <summary>
		///   Overloaded. Logs information about the current thread with
		///   the default log level.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method logs information about the current thread. This
		///   includes its name, its current state and more.
		///
		///   LogCurrentThread is especially useful in a multi-threaded
		///   program like in a network server application. By using this
		///   method you can easily track all threads of a process and
		///   obtain detailed information about them.
		///
		///   See LogThread for a more general method which can handle any
		///   thread.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentThread()
		{
			LogCurrentThread(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Logs information about the current thread with
		///   a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the current thread. This
		///   includes its name, its current state and more.
		///
		///   LogCurrentThread is especially useful in a multi-threaded
		///   program like in a network server application. By using this
		///   method you can easily track all threads of a process and
		///   obtain detailed information about them.
		///
		///   See LogThread for a more general method which can handle any
		///   thread.
		/// </remarks>
		/// -->

		public void LogCurrentThread(Level level)
		{
			if (IsOn(level))
			{
				Thread thread = Thread.CurrentThread;
				string title = thread.Name;

				if (title == null || title.Length == 0)
				{
					// The thread name hasn't been set, so
					// use the current thread id as title suffix.
#if (SI_DOTNET_1x)
					title = "Id = " + AppDomain.GetCurrentThreadId();
#else
					title = "Id = " + Thread.CurrentThread.ManagedThreadId;
#endif
				}
				else
				{
					// Add the current thread id to the
					// thread name (surrounded by parentheses).
#if (SI_DOTNET_1x)
					title += " (" + AppDomain.GetCurrentThreadId() + ")";
#else
					title += " (" + Thread.CurrentThread.ManagedThreadId + ")";
#endif
				}

				// Just call the LogThread method with the current
				// thread and its name or/and its id as title.
				LogThread(level, "Current thread: " + title, thread);
			}
		}

		/// <summary>
		///   Overloaded. Logs information about the current thread with
		///   a custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the current thread. This
		///   includes its name, its current state and more.
		///
		///   LogCurrentThread is especially useful in a multi-threaded
		///   program like in a network server application. By using this
		///   method you can easily track all threads of a process and
		///   obtain detailed information about them.
		///
		///   See LogThread for a more general method which can handle
		///   any thread.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentThread(string title)
		{
			LogCurrentThread(this.fParent.DefaultLevel, title);
		}

		/// <summary>
		///   Overloaded. Logs information about the current thread with
		///   a custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the current thread. This
		///   includes its name, its current state and more.
		///
		///   LogCurrentThread is especially useful in a multi-threaded
		///   program like in a network server application. By using this
		///   method you can easily track all threads of a process and
		///   obtain detailed information about them.
		///
		///   See LogThread for a more general method which can handle
		///   any thread.
		/// </remarks>
		/// -->

		public void LogCurrentThread(Level level, string title)
		{
			if (IsOn(level))
			{
				LogThread(level, title, Thread.CurrentThread);
			}
		}

		/// <summary>
		///   Overloaded. Logs information about a thread with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="thread">The thread to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the supplied thread. This
		///   includes its name, its current state and more.
		///
		///   LogThread is especially useful in a multi-threaded program
		///   like in a network server application. By using this method you
		///   can easily track all threads of a process and obtain detailed
		///   information about them.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogThread(string title, Thread thread)
		{
			LogThread(this.fParent.DefaultLevel, title, thread);
		}

		/// <summary>
		///   Overloaded. Logs information about a thread with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="thread">The thread to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the supplied thread. This
		///   includes its name, its current state and more.
		///
		///   LogThread is especially useful in a multi-threaded program
		///   like in a network server application. By using this method you
		///   can easily track all threads of a process and obtain detailed
		///   information about them.
		/// </remarks>
		/// -->

		public void LogThread(Level level, string title, Thread thread)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (thread == null)
			{
				LogInternalError("LogThread: thread argument is null");
				return;
			}

			using (ValueListViewerContext ctx = new ValueListViewerContext())
			{
				try
				{
					ctx.AppendKeyValue("Name", thread.Name);
					ctx.AppendKeyValue("Alive", thread.IsAlive);

					if (thread.IsAlive)
					{
						ctx.AppendKeyValue("Priority", thread.Priority.ToString());
						ctx.AppendKeyValue("Background", thread.IsBackground);
					}

					ctx.AppendKeyValue("ThreadPool Thread", thread.IsThreadPoolThread);
					ctx.AppendKeyValue("State", thread.ThreadState.ToString());

					SendContext(level, title, LogEntryType.Text, ctx);
				}
				catch (Exception e)
				{
					LogInternalError("LogThread: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs information about the current application
		///   domain and its setup with the default log level.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This version of the method uses the value of the FriendlyName
		///   property of the current domain as title. See LogAppDomain for
		///   a more general method which can handle any app domain.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentAppDomain()
		{
			LogCurrentAppDomain(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Logs information about the current application
		///   domain and its setup with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   This version of the method uses the value of the FriendlyName
		///   property of the current domain as title. See LogAppDomain for
		///   a more general method which can handle any app domain.
		/// </remarks>
		/// -->

		public void LogCurrentAppDomain(Level level)
		{
			if (IsOn(level))
			{
				AppDomain domain = AppDomain.CurrentDomain;
				LogAppDomain(level, "Current app domain: " + domain.FriendlyName, 
					domain);
			}
		}

		/// <summary>
		///   Overloaded. Logs information about the current application
		///   domain and its setup with a custom title and default log
		///   level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   See LogAppDomain for a more general method which can handle
		///   any app domain.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogCurrentAppDomain(string title)
		{
			LogCurrentAppDomain(this.fParent.DefaultLevel, title);
		}

		/// <summary>
		///   Overloaded. Logs information about the current application
		///   domain and its setup with a custom title and custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   See LogAppDomain for a more general method which can handle
		///   any app domain.
		/// </remarks>
		/// -->

		public void LogCurrentAppDomain(Level level, string title)
		{
			if (IsOn(level))
			{
				LogAppDomain(level, title, AppDomain.CurrentDomain);
			}
		}

		/// <summary>
		///   Overloaded. Logs information about an application domain and
		///   its setup with the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="domain">The application domain to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the supplied application
		///   domain and its setup.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogAppDomain(string title, AppDomain domain)
		{
			LogAppDomain(this.fParent.DefaultLevel, title, domain);
		}

		/// <summary>
		///   Overloaded. Logs information about an application domain and
		///   its setup with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="domain">The application domain to log.</param>
		/// <!--
		/// <remarks>
		///   This method logs information about the supplied application
		///   domain and its setup.
		/// </remarks>
		/// -->

		public void LogAppDomain(Level level, string title, AppDomain domain)
		{
			if (!IsOn(level))
			{
				return;
			}

			if (domain == null)
			{
				LogInternalError("LogAppDomain: domain argument is null");
				return;
			}

		}

		/// <summary>
		///   Overloaded. Logs the content of a StringBuilder instance 
		///   with the default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="sb">
		///   The StringBuilder instance whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied StringBuilder instance will be
		///   displayed in a read-only text field.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogStringBuilder(string title, StringBuilder sb)
		{
			LogStringBuilder(this.fParent.DefaultLevel, title, sb);
		}

		/// <summary>
		///   Overloaded. Logs the content of a StringBuilder instance 
		///   with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="sb">
		///   The StringBuilder instance whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied StringBuilder instance will be
		///   displayed in a read-only text field.
		/// </remarks>
		/// -->

		public void LogStringBuilder(Level level, string title, 
			StringBuilder sb)
		{
			if (IsOn(level))
			{
				if (sb == null)
				{
					LogInternalError("LogStringBuilder: sb argument is null");
				}
				else
				{
					LogText(level, title, sb.ToString());
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs information about the system with the default
		///   log level.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The logged information include the version of the operating
		///   system, the .NET framework version and more. This method is
		///   useful for logging general information at the program startup.
		///   This guarantees that the support staff or developers have
		///   general information about the execution environment.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSystem()
		{
			LogSystem(this.fParent.DefaultLevel);
		}

		/// <summary>
		///   Overloaded. Logs information about the system with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <!--
		/// <remarks>
		///   The logged information include the version of the operating
		///   system, the .NET framework version and more. This method is
		///   useful for logging general information at the program startup.
		///   This guarantees that the support staff or developers have
		///   general information about the execution environment.
		/// </remarks>
		/// -->

		public void LogSystem(Level level)
		{
			LogSystem(level, "System information");
		}

		/// <summary>
		///   Overloaded. Logs information about the system using a
		///   custom title and default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   The logged information include the version of the operating
		///   system, the .NET framework version and more. This method is
		///   useful for logging general information at the program startup.
		///   This guarantees that the support staff or developers have
		///   general information about the execution environment.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogSystem(string title)
		{
			LogSystem(this.fParent.DefaultLevel, title);
		}

		/// <summary>
		///   Overloaded. Logs information about the system using a
		///   custom title and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <!--
		/// <remarks>
		///   The logged information include the version of the operating
		///   system, the .NET framework version and more. This method is
		///   useful for logging general information at the program startup.
		///   This guarantees that the support staff or developers have
		///   general information about the execution environment.
		/// </remarks>
		/// -->

		public void LogSystem(Level level, string title)
		{
			if (!IsOn(level))
			{
				return;
			}

			using (InspectorViewerContext ctx = new InspectorViewerContext())
			{
				try
				{
					OperatingSystem os = Environment.OSVersion;

					ctx.StartGroup("Operating System");
					ctx.AppendKeyValue("Name", os.Platform.ToString());
					ctx.AppendKeyValue("Version", os.Version.ToString());

					ctx.StartGroup("User");
					ctx.AppendKeyValue("Name", Environment.UserName);
					ctx.AppendKeyValue("Current directory",
						Environment.CurrentDirectory);

					ctx.StartGroup(".NET");
					ctx.AppendKeyValue("Version", Environment.Version.ToString());

					SendContext(level, title, LogEntryType.System, ctx);
				}
				catch (Exception e)
				{
					LogInternalError("LogSystem: " + e.Message);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs the content of a binary stream with the
		///   default log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied binary stream will be displayed in
		///   a read-only hex editor.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogStream(string title, Stream stream)
		{
			LogStream(this.fParent.DefaultLevel, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs the content of a binary stream with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="stream">
		///   The stream whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied binary stream will be displayed in
		///   a read-only hex editor.
		/// </remarks>
		/// -->

		public void LogStream(Level level, string title, Stream stream)
		{
			LogBinaryStream(level, title, stream);
		}

		/// <summary>
		///   Overloaded. Logs the content of a text reader with the default
		///   log level.
		/// </summary>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">
		///   The reader whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied text reader will be displayed in
		///   a read-only text field.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void LogReader(string title, TextReader reader)
		{
			LogReader(this.fParent.DefaultLevel, title, reader);
		}

		/// <summary>
		///   Overloaded. Logs the content of a text reader with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title to display in the Console.</param>
		/// <param name="reader">
		///   The reader whose content should be logged.
		/// </param>
		/// <!--
		/// <remarks>
		///   The content of the supplied text reader will be displayed in
		///   a read-only text field.
		/// </remarks>
		/// -->

		public void LogReader(Level level, string title, TextReader reader)
		{
			LogTextReader(level, title, reader);
		}

		/// <summary>
		///   Clears all Log Entries in the Console.
		/// </summary>

		public void ClearLog()
		{
			if (IsOn())
			{
				SendControlCommand(ControlCommandType.ClearLog, null);
			}
		}

		/// <summary>
		///   Clears all Watches in the Console.
		/// </summary>

		public void ClearWatches()
		{
			if (IsOn())
			{
				SendControlCommand(ControlCommandType.ClearWatches, null);
			}
		}

		/// <summary>
		///   Clears all AutoViews in the Console.
		/// </summary>

		public void ClearAutoViews()
		{
			if (IsOn())
			{
				SendControlCommand(ControlCommandType.ClearAutoViews, null);
			}
		}

		/// <summary>
		///   Resets the whole Console.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This method resets the whole Console. This means that all
		///   Watches, Log Entries, Process Flow entries and AutoViews
		///   will be deleted.
		/// </remarks>
		/// -->

		public void ClearAll()
		{
			if (IsOn())
			{
				SendControlCommand(ControlCommandType.ClearAll, null);
			}
		}

		/// <summary>
		///   Clears all Process Flow entries in the Console.
		/// </summary>

		public void ClearProcessFlow()
		{
			if (IsOn())
			{
				SendControlCommand(ControlCommandType.ClearProcessFlow, null);
			}
		}

		/// <summary>
		///   Resets a named counter to its initial value of 0.
		/// </summary>
		/// <param name="name">The name of the counter to reset.</param>
		/// <!--
		/// <remarks>
		///   This method resets the integer value of a named counter to 0
		///   again. If the supplied counter is unknown, this method has no
		///   effect. Please refer to the IncCounter and DecCounter methods
		///   for more information about named counters.
		/// </remarks>
		/// -->

		public void ResetCounter(string name)
		{
			if (name == null)
			{
				LogInternalError("ResetCounter: name argument is null");
				return;
			}

			lock (this.fCounter)
			{
				this.fCounter.Remove(name);
			}
		}

		private int UpdateCounter(string name, bool increment)
		{
			int value;

			lock (this.fCounter)
			{
				if (this.fCounter.Contains(name))
				{
					 value = (int) this.fCounter[name];
				}
				else 
				{
					value = 0;
				}

				if (increment)
				{
					value++;
				}
				else 
				{
					value--;
				}

				this.fCounter[name] = value;
			}

			return value;
		}

		/// <summary>
		///   Overloaded. Increments a named counter by one and automatically
		///   sends its name and value as integer watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the counter to log.</param>
		/// <!--
		/// <remarks>
		///   The Session class tracks a list of so called named counters.
		///   A counter has a name and a value of type integer. This method
		///   increments the value for the specified counter by one and then
		///   sends a normal integer watch with the name and value of the
		///   counter. The initial value of a counter is 0. To reset the
		///   value of a counter to 0 again, you can call ResetCounter.
		///
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		///   
		///   See DecCounter for a method which decrements the value of a
		///   named counter instead of incrementing it.
		/// </remarks>
		/// -->

		public void IncCounter(string name)
		{
			IncCounter(this.fParent.DefaultLevel, name);
		}

		/// <summary>
		///   Overloaded. Increments a named counter by one and automatically
		///   sends its name and value as integer watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the counter to log.</param>
		/// <!--
		/// <remarks>
		///   The Session class tracks a list of so called named counters.
		///   A counter has a name and a value of type integer. This method
		///   increments the value for the specified counter by one and then
		///   sends a normal integer watch with the name and value of the
		///   counter. The initial value of a counter is 0. To reset the
		///   value of a counter to 0 again, you can call ResetCounter.
		///
		///   See DecCounter for a method which decrements the value of a
		///   named counter instead of incrementing it.
		/// </remarks>
		/// -->

		public void IncCounter(Level level, string name)
		{
			if (IsOn(level))
			{
				if (name == null)
				{
					LogInternalError("IncCounter: name argument is null");
				}
				else 
				{
					int value = UpdateCounter(name, true);
					SendWatch(level, name, Convert.ToString(value),
						WatchType.Integer);
				}
			}
		}

		/// <summary>
		///   Overloaded. Decrements a named counter by one and automatically
		///   sends its name and value as integer watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the counter to log.</param>
		/// <!--
		/// <remarks>
		///   The Session class tracks a list of so called named counters.
		///   A counter has a name and a value of type integer. This method
		///   decrements the value for the specified counter by one and then
		///   sends a normal integer watch with the name and value of the
		///   counter. The initial value of a counter is 0. To reset the
		///   value of a counter to 0 again, you can call ResetCounter.
		///
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		///   
		///   See IncCounter for a method which increments the value of a
		///   named counter instead of decrementing it.
		/// </remarks>
		/// -->

		public void DecCounter(string name)
		{
			DecCounter(this.fParent.DefaultLevel, name);
		}

		/// <summary>
		///   Overloaded. Decrements a named counter by one and automatically
		///   sends its name and value as integer watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the counter to log.</param>
		/// <!--
		/// <remarks>
		///   The Session class tracks a list of so called named counters.
		///   A counter has a name and a value of type integer. This method
		///   decrements the value for the specified counter by one and then
		///   sends a normal integer watch with the name and value of the
		///   counter. The initial value of a counter is 0. To reset the
		///   value of a counter to 0 again, you can call ResetCounter.
		///
		///   See IncCounter for a method which increments the value of a
		///   named counter instead of decrementing it.
		/// </remarks>
		/// -->

		public void DecCounter(Level level, string name)
		{
			if (IsOn(level))
			{
				if (name == null)
				{
					LogInternalError("DecCounter: name argument is null");
				}
				else
				{
					int value = UpdateCounter(name, false);
					SendWatch(level, name, Convert.ToString(value),
						WatchType.Integer);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a boolean Watch with the default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchBool(string name, bool value)
		{
			WatchBool(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a boolean Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchBool(Level level, string name, bool value)
		{
			if (IsOn(level))
			{
				string v = value ? "True" : "False";
				SendWatch(level, name, v, WatchType.Boolean);
			}
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchByte(string name, byte value)
		{
			WatchByte(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchByte(Level level, string name, byte value)
		{
			WatchByte(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with an optional hexadecimal
		///   representation and default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a byte Watch. You can specify if a
		///   hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchByte(string name, byte value, bool includeHex)
		{
			WatchByte(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with an optional hexadecimal
		///   representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a byte Watch. You can specify if a
		///   hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// </remarks>
		/// -->

		public void WatchByte(Level level, string name, byte value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				string v = Convert.ToString(value);
				
				if (includeHex)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(" (0x");
					sb.Append(value.ToString("x2"));
					sb.Append(")");
					v += sb.ToString();
				}

				SendWatch(level, name, v, WatchType.Integer);
			}
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with the default
		///   log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchShort(string name, short value)
		{
			WatchShort(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchShort(Level level, string name, short value)
		{
			WatchShort(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with an optional
		///   hexadecimal representation and default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a short integer Watch. You can specify
		///   if a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchShort(string name, short value, bool includeHex)
		{
			WatchShort(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with an optional
		///   hexadecimal representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a short integer Watch. You can specify
		///   if a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// </remarks>
		/// -->

		public void WatchShort(Level level, string name, short value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				string v = Convert.ToString(value);
				
				if (includeHex)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(" (0x");
					sb.Append(value.ToString("x4"));
					sb.Append(")");
					v += sb.ToString();
				}

				SendWatch(level, name, v, WatchType.Integer);
			}
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchInt(string name, int value)
		{
			WatchInt(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchInt(Level level, string name, int value)
		{
			WatchInt(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with an optional
		///   hexadecimal representation and default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs an integer Watch. You can specify if
		///   a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchInt(string name, int value, bool includeHex)
		{
			WatchInt(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with an optional
		///   hexadecimal representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs an integer Watch. You can specify if
		///   a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// </remarks>
		/// -->

		public void WatchInt(Level level, string name, int value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				string v = Convert.ToString(value);
				
				if (includeHex)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(" (0x");
					sb.Append(value.ToString("x8"));
					sb.Append(")");
					v += sb.ToString();
				}

				SendWatch(level, name, v, WatchType.Integer);
			}
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchLong(string name, long value)
		{
			WatchLong(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchLong(Level level, string name, long value)
		{
			WatchLong(level, name, value, false);
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with an optional
		///   hexadecimal representation and default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a long integer Watch. You can specify
		///   if a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchLong(string name, long value, bool includeHex)
		{
			WatchLong(this.fParent.DefaultLevel, name, value, includeHex);
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with an optional
		///   hexadecimal representation and custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to dispay as Watch value.</param>
		/// <param name="includeHex">
		///   Indicates if a hexadecimal representation should be included.
		/// </param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method logs a long integer Watch. You can specify
		///   if a hexadecimal representation should be included as well
		///   by setting the includeHex parameter to true.
		/// </remarks>
		/// -->

		public void WatchLong(Level level, string name, long value, 
			bool includeHex)
		{
			if (IsOn(level))
			{
				string v = Convert.ToString(value);
				
				if (includeHex)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(" (0x");
					sb.Append(value.ToString("x16"));
					sb.Append(")");
					v += sb.ToString();
				}

				SendWatch(level, name, v, WatchType.Integer);
			}
		}

		/// <summary>
		///   Overloaded. Logs a float Watch with the default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchFloat(string name, float value)
		{
			WatchFloat(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a float Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchFloat(Level level, string name, float value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, Convert.ToString(value), 
					WatchType.Float);
			}
		}

		/// <summary>
		///   Overloaded. Logs a double Watch with the default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchDouble(string name, double value)
		{
			WatchDouble(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a double Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchDouble(Level level, string name, double value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, Convert.ToString(value), 
					WatchType.Float);
			}
		}

		/// <summary>
		///   Overloaded. Logs a decimal Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchDecimal(string name, decimal value)
		{
			WatchDecimal(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a decimal Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchDecimal(Level level, string name, decimal value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, Convert.ToString(value), 
					WatchType.Float);
			}
		}

		/// <summary>
		///   Overloaded. Logs a char Watch with the default log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchChar(string name, char value)
		{
			WatchChar(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a char Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchChar(Level level, string name, char value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, Convert.ToString(value), 
					WatchType.Char);
			}
		}

		/// <summary>
		///   Overloaded. Logs a string Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchString(string name, string value)
		{
			WatchString(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a string Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchString(Level level, string name, string value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, value, WatchType.String);
			}
		}

		/// <summary>
		///   Overloaded. Logs a DateTime Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchDateTime(string name, DateTime value)
		{
			WatchDateTime(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a DateTime Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// -->

		public void WatchDateTime(Level level, string name, DateTime value)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, Convert.ToString(value), 
					WatchType.Timestamp);
			}
		}

		/// <summary>
		///   Overloaded. Logs an object Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   The value of the resulting Watch is the return value of the
		///   ToString method of the supplied object.
		///   
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void WatchObject(string name, object value)
		{
			WatchObject(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an object Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">The value to display as Watch value.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   The value of the resulting Watch is the return value of the
		///   ToString method of the supplied object.
		/// </remarks>
		/// -->

		public void WatchObject(Level level, string name, object value)
		{
			if (IsOn(level))
			{
				if (value == null)
				{
					LogInternalError("WatchObject: value argument is null");
				}
				else 
				{
					SendWatch(level, name, value.ToString(), WatchType.Object);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a bool Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The bool value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchBool method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, bool value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a bool Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The bool value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchBool method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, bool value)
		{
			WatchBool(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The byte value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchByte method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, byte value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a byte Watch with a custom log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The byte value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchByte method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, byte value)
		{
			WatchByte(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with the default
		///   log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The short integer value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchShort method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, short value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a short integer Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The short integer value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchShort method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, short value)
		{
			WatchShort(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The int value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchInt method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, int value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an integer Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The int value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchInt method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, int value)
		{
			WatchInt(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with the default
		///   log level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The long integer value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchLong method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, long value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a long integer Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The long integer value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchLong method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, long value)
		{
			WatchLong(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a float Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The float value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchFloat method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, float value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a float Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The float value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchFloat method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, float value)
		{
			WatchFloat(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a double Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The double value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDouble method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, double value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a double Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The double value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDouble method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, double value)
		{
			WatchDouble(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a decimal Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The decimal value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDecimal method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, decimal value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a decimal Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The decimal value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDecimal method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, decimal value)
		{
			WatchDecimal(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a char Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The char value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchChar method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, char value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a char Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The char value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchChar method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, char value)
		{
			WatchChar(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a string Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The string value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchString method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, string value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a string Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The string value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchString method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, string value)
		{
			WatchString(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a DateTime Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The DateTime value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDateTime method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, DateTime value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a DateTime Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The DateTime value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchDateTime method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, DateTime value)
		{
			WatchDateTime(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an object Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The object value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchObject method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void Watch(string name, object value)
		{
			Watch(this.fParent.DefaultLevel, name, value);
		}

		/// <summary>
		///   Overloaded. Logs an object Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the Watch.</param>
		/// <param name="value">
		///   The object value to display as Watch value.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method just calls the WatchObject method.
		/// </remarks>
		/// -->

		public void Watch(Level level, string name, object value)
		{
			WatchObject(level, name, value);
		}

		/// <summary>
		///   Overloaded. Logs a custom Log Entry with the default log
		///   level.
		/// </summary>
		/// <param name="title">The title of the new Log Entry.</param>
		/// <param name="lt">The Log Entry type to use.</param>
		/// <param name="vi">The Viewer ID to use.</param>
		/// <param name="data">Optional data stream which can be null.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LogEntry"/>
		/// <remarks>
		///   This method is useful for implementing custom Log Entry
		///   methods. For example, if you want to display some information
		///   in a particular way in the Console, you can just create a
		///   simple method which formats the data in question correctly and
		///   logs them using this SendCustomLogEntry method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void SendCustomLogEntry(string title, LogEntryType lt,
			ViewerId vi, Stream data)
		{
			SendCustomLogEntry(this.fParent.DefaultLevel, title, lt, vi, data);
		}

		/// <summary>
		///   Overloaded. Logs a custom Log Entry with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">The title of the new Log Entry.</param>
		/// <param name="lt">The Log Entry type to use.</param>
		/// <param name="vi">The Viewer ID to use.</param>
		/// <param name="data">Optional data stream which can be null.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.LogEntry"/>
		/// <remarks>
		///   This method is useful for implementing custom Log Entry
		///   methods. For example, if you want to display some information
		///   in a particular way in the Console, you can just create a
		///   simple method which formats the data in question correctly and
		///   logs them using this SendCustomLogEntry method.
		/// </remarks>
		/// -->

		public void SendCustomLogEntry(Level level, string title, 
			LogEntryType lt, ViewerId vi, Stream data)
		{
			if (IsOn(level))
			{
				if (data != null)
				{
					// Use the LogCustomStream method, because the
					// supplied stream needs to be processed correctly.
					LogCustomStream(level, title, data, lt, vi);
				}
				else 
				{
					SendLogEntry(level, title, lt, vi, Color, null);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a custom Control Command with the default
		///   log level.
		/// </summary>
		/// <param name="ct">The Control Command type to use.</param>
		/// <param name="data">Optional data stream which can be null.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ControlCommand"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void SendCustomControlCommand(ControlCommandType ct,
			Stream data)
		{
			SendCustomControlCommand(this.fParent.DefaultLevel, ct, data);
		}

		/// <summary>
		///   Overloaded. Logs a custom Control Command with a custom
		///   log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="ct">The Control Command type to use.</param>
		/// <param name="data">Optional data stream which can be null.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ControlCommand"/>
		/// -->

		public void SendCustomControlCommand(Level level, 
			ControlCommandType ct, Stream data)
		{
			if (IsOn(level))
			{
				if (data != null)
				{
					try 
					{
						// Take care of the stream position.
						long oldPosition = 0;

						if (data.CanSeek)
						{
							// Save original stream position.
							oldPosition = data.Position;
							data.Position = 0;
						}

						try 
						{
							SendControlCommand(ct, data);
						}
						finally 
						{
							if (data.CanSeek)
							{
								// Restore stream position.
								data.Position = oldPosition;
							}
						}
					}
					catch (Exception e)
					{
						LogInternalError(
							"SendCustomControlCommand: " + e.Message
						);
					}
				}
				else 
				{
					SendControlCommand(ct, null);
				}
			}
		}

		/// <summary>
		///   Overloaded. Logs a custom Watch with the default log
		///   level.
		/// </summary>
		/// <param name="name">The name of the new Watch.</param>
		/// <param name="value">The value of the new Watch.</param>
		/// <param name="wt">The Watch type to use.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method is useful for implementing custom Watch methods.
		///   For example, if you want to track the status of an instance of
		///   a specific class, you can just create a simple method which
		///   extracts all necessary information about this instance and logs
		///   them using this SendCustomWatch method.
		/// 
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void SendCustomWatch(string name, string value, WatchType wt)
		{
			SendCustomWatch(this.fParent.DefaultLevel, name, value, wt);
		}

		/// <summary>
		///   Overloaded. Logs a custom Watch with a custom log
		///   level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="name">The name of the new Watch.</param>
		/// <param name="value">The value of the new Watch.</param>
		/// <param name="wt">The Watch type to use.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.Watch"/>
		/// <remarks>
		///   This method is useful for implementing custom Watch methods.
		///   For example, if you want to track the status of an instance of
		///   a specific class, you can just create a simple method which
		///   extracts all necessary information about this instance and logs
		///   them using this SendCustomWatch method.
		/// </remarks>
		/// -->

		public void SendCustomWatch(Level level, string name, string value, 
			WatchType wt)
		{
			if (IsOn(level))
			{
				SendWatch(level, name, value, wt);
			}
		}

		/// <summary>
		///   Overloaded. Logs a custom Process Flow entry with a
		///   custom log level.
		/// </summary>
		/// <param name="title">
		///   The title of the new Process Flow entry.
		/// </param>
		/// <param name="pt">The Process Flow type to use.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ProcessFlow"/>
		/// <remarks>
		///   This method uses the <link SmartInspect.DefaultLevel,
		///   default level> of the session's <link Parent, parent> as log
		///   level. For more information, please refer to the documentation
		///   of the <link SmartInspect.DefaultLevel, DefaultLevel> property
		///   of the SmartInspect class.
		/// </remarks>
		/// -->

		public void SendCustomProcessFlow(string title, ProcessFlowType pt)
		{
			SendCustomProcessFlow(this.fParent.DefaultLevel, title, pt);
		}

		/// <summary>
		///   Overloaded. Logs a custom Process Flow entry with the
		///   default log level.
		/// </summary>
		/// <param name="level">The log level of this method call.</param>
		/// <param name="title">
		///   The title of the new Process Flow entry.
		/// </param>
		/// <param name="pt">The Process Flow type to use.</param>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ProcessFlow"/>
		/// -->

		public void SendCustomProcessFlow(Level level, string title, 
			ProcessFlowType pt)
		{
			if (IsOn(level))
			{
				SendProcessFlow(level, title, pt);
			}
		}
	}
}
