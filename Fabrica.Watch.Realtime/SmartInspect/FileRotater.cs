//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Responsible for the log file rotate management as used by the
	///   FileProtocol class.
	/// </summary>
	/// <!--
	/// <seealso cref="Gurock.SmartInspect.FileRotaterEventArgs"/>
	/// <seealso cref="Gurock.SmartInspect.FileRotaterEventHandler"/>
	/// <remarks>
	///   This class implements a flexible log file rotate management
	///   system. For a detailed description of how to use this class,
	///   please refer to the documentation of the Initialize and Update
	///   methods and the Mode property.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class FileRotater
	{
		private static DateTime DATETIME_MIN_VALUE = DateTime.MinValue;

		private FileRotate fMode;
		private int fTimeValue;

		/// <summary>
		///   Creates a new FileRotater instance with a default mode of
		///   FileRotate.None. Please refer to the Update and Initialize
		///   methods for additional information about this class.
		/// </summary>

		public FileRotater()
		{
			this.fMode = FileRotate.None;
		}

		/// <summary>
		///   Initializes this FileRotater object with a user-supplied
		///   timestamp.
		/// </summary>
		/// <param name="now">
		///   The user-specified timestamp to use to initialize this object.
		/// </param>
		/// <!--
		/// <remarks>
		///   Always call this method after creating a new FileRotater
		///   object and before calling the Update method the first time.
		///   For additional information please refer to the Update method.
		/// </remarks>
		/// -->

		public void Initialize(DateTime now)
		{
			this.fTimeValue = GetTimeValue(now);
		}

		private static DateTime GetMonday(DateTime date)
		{
			int days = 0;

			switch (date.DayOfWeek)
			{
				case DayOfWeek.Tuesday: days = -1; break;
				case DayOfWeek.Wednesday: days = -2; break;
				case DayOfWeek.Thursday: days = -3; break;
				case DayOfWeek.Friday: days = -4; break;
				case DayOfWeek.Saturday: days = -5; break;
				case DayOfWeek.Sunday: days = -6; break;
			}

		
			return date.AddDays(days);
		}

		private static int GetDays(DateTime date)
		{
			return (int) (date - DATETIME_MIN_VALUE).TotalDays;
		}

		private int GetTimeValue(DateTime now)
		{
			int timeValue;

			switch (this.fMode)
			{
				case FileRotate.Hourly:
					timeValue = GetDays(now) * 24 + now.Hour;
					break;
				case FileRotate.Daily:
					timeValue = GetDays(now);
					break;
				case FileRotate.Weekly:
					timeValue = GetDays(GetMonday(now));
					break;
				case FileRotate.Monthly:
					timeValue = now.Year * 12 + now.Month;
					break;
				default:
					timeValue = 0;
					break;
			}

			return timeValue;
		}

		/// <summary>
		///   Updates the date of this FileRotater object and returns
		///   whether the rotate state has changed since the last call to
		///   this method or to Initialize.
		/// </summary>
		/// <returns>
		///   True if the rotate state has changed since the last call to
		///   this method or to Initialize and false otherwise.
		/// </returns>
		/// <param name="now">The timestamp to update this object.</param>
		/// <!--
		/// <remarks>
		///   This method updates the internal date of this FileRotater
		///   object and returns whether the rotate state has changed since
		///   the last call to this method or to Initialize. Before calling
		///   this method, always call the Initialize method.
		/// </remarks>
		/// -->

		public bool Update(DateTime now)
		{
			int timeValue = GetTimeValue(now);

			if (timeValue != this.fTimeValue)
			{
				this.fTimeValue = timeValue;
				return true;
			}
			else 
			{
				return false;
			}
		}

		/// <summary>
		///   Represents the FileRotate mode of this FileRotater object.
		/// </summary>
		/// <remarks>
		///   Always call the Initialize method after changing this
		///   property to reinitialize this FileRotater object. For a
		///   complete list of available property values, please refer
		///   to the documentation of the FileRotate enum.
		/// </remarks>

		public FileRotate Mode
		{
			get { return this.fMode; }
			set { this.fMode = value; }
		}
	}
}
