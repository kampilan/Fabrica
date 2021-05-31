//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;
using System.Threading;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   A configurable timer for monitoring and reloading SmartInspect
	///   configuration files on changes.
	/// </summary>
	/// <!--
	/// <remarks>
	///   Use this class to monitor and automatically reload SmartInspect
	///   configuration files. This timer periodically checks if the
	///   related configuration file has changed (by comparing the last
	///   write time) and automatically tries to reload the configuration
	///   properties. You can pass the SmartInspect object to configure,
	///   the name of the configuration file to monitor and the interval
	///   in which this timer should check for changes.
	/// 
	///   For information about SmartInspect configuration files, please
	///   refer to the documentation of the SmartInspect.LoadConfiguration
	///   method.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class ConfigurationTimer: IDisposable
	{
		private Timer fTimer;
		private object fLock;
		private SmartInspect fSmartInspect;
		private string fFileName;
		private DateTime fLastUpdate;

		/// <summary>
		///   Creates and initializes a new ConfigurationTimer object.
		/// </summary>
		/// <param name="smartInspect">
		///   The SmartInspect object to configure.
		/// </param>
		/// <param name="fileName">
		///   The name of the configuration file to monitor.
		/// </param>
		/// <param name="period">
		///   The milliseconds interval in which this timer should check
		///   for changes.
		/// </param>
		/// <!--
		/// <exception>
		/// <table>
		///   Exception Type               Condition
		///   +                            +
		///   ArgumentNullException        The smartInspect or fileName
		///                                  parameter is null.
		/// 
		///   ArgumentOutOfRangeException  The period parameter is negative
		///                                  and is not equal to Infinite.
		/// </table>
		/// </exception>
		/// -->

		public ConfigurationTimer(SmartInspect smartInspect,
			string fileName, int period)
		{
			if (smartInspect == null)
			{
				throw new ArgumentNullException("smartInspect");
			}

			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			this.fLock = new object();
			this.fFileName = fileName;
			this.fSmartInspect = smartInspect;

			if (GetFileAge(this.fFileName, out this.fLastUpdate))
			{
				this.fSmartInspect.LoadConfiguration(this.fFileName);
			}

			this.fTimer = new Timer(new TimerCallback(Callback),
				null, period, period);
		}

		private static bool GetFileAge(string fileName, 
			out DateTime age)
		{
			bool result = true;

			try
			{
				FileInfo info = new FileInfo(fileName);
				age = info.LastWriteTime;
			}
			catch (Exception)
			{
				age = DateTime.MinValue;
				result = false;
			}

			return result;
		}

		private void Callback(object state)
		{
			DateTime lastUpdate;

			if (!GetFileAge(this.fFileName, out lastUpdate))
			{
				return;
			}

			lock (this.fLock)
			{
				if (lastUpdate <= this.fLastUpdate)
				{
					return;
				}

				this.fLastUpdate = lastUpdate;
			}

			this.fSmartInspect.LoadConfiguration(this.fFileName);
		}

		/// <summary>
		///   Releases all resources of this ConfigurationTimer object
		///   and stops monitoring the SmartInspect configuration file for
		///   changes.
		/// </summary>

		public void Dispose()
		{
			lock (this.fLock)
			{
				if (this.fTimer != null)
				{
					this.fTimer.Dispose();
					this.fTimer = null;
				}
			}
		}
	}
}
