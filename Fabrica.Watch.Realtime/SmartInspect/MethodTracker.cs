//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Simple helper class which is used for implementing the
	///   Session.TrackMethod feature.
	/// </summary>

	public class MethodTracker: IDisposable
	{
		private Level fLevel;
		private Session fSession;
		private string fMethodName;

		/// <summary>
		///   Creates and initializes a new MethodTracker instance and
		///   calls the EnterMethod method on the given Session with
		///   the specified arguments.
		/// </summary>
		/// <param name="level">
		///   The log level to pass to EnterMethod.
		///  </param>
		/// <param name="session">
		///	  The session to use for the EnterMethod call.
		/// </param>
		/// <param name="methodName">
		///	  The method name to pass to EnterMethod.
		///	</param>

		public MethodTracker(Level level, Session session,
			string methodName)
		{
			this.fLevel = level;
			this.fSession = session;
			this.fMethodName = methodName;
			this.fSession.EnterMethod(this.fLevel, this.fMethodName);
		}

		/// <summary>
		///   Just call LeaveMethod on the previously passed Session
		///   object with the previously given log level and method
		///   name.
		/// </summary>

		public void Dispose()
		{
			this.fSession.LeaveMethod(this.fLevel, this.fMethodName);
		}
	}
}
