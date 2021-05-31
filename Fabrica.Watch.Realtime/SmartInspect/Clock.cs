//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Diagnostics;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Provides access to the current date and time, optionally with
	///   a high resolution.
	/// </summary>
	/// <!--
	/// <seealso cref="Gurock.SmartInspect.ClockResolution"/>
	/// <remarks>
	///   See Now for a method which returns the current date and time,
	///   optionally with a very high resolution. See Calibrate for a
	///   method which can synchronize the high-resolution timer with the
	///   system clock.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class Clock
	{
#if !(SI_DOTNET_1x)
		private const int CALIBRATE_ROUNDS = 5;

		private static bool fSupported;
		private static long fOffset;
		private static double fFrequency;

		static Clock()
		{
			if (Stopwatch.IsHighResolution)
			{
				fSupported = true;
				fFrequency = Stopwatch.Frequency / 10000000.0;
				fOffset = GetOffset();
			}
		}

		private static long GetTicks()
		{
			long timestamp = Stopwatch.GetTimestamp();
			return (long) (timestamp / fFrequency);
		}

		private static long GetOffset()
		{
			return DateTime.Now.Ticks - GetTicks();
		}
#endif

		/// <summary>
		///   Returns the current date and time, optionally with a high
		///   resolution.
		/// </summary>
		/// <param name="resolution">
		///   Specifies the desired resolution mode for the returned
		///   timestamp.
		/// </param>
		/// <returns>The current date and time as DateTime value.</returns>
		/// <!--
		/// <seealso cref="Gurock.SmartInspect.ClockResolution"/>
		/// <remarks>
		///   If ClockResolution.High is passed as value for the resolution
		///   argument, this method tries to return a timestamp with a
		///   microsecond resolution.
		/// 
		///   The support for high-resolution timestamps depends on the
		///   System.Diagnostics.Stopwatch class introduced in .NET 2.0.
		///   This method can only return a high-resolution timestamp if
		///   this class is available and if its IsHighResolution property
		///   returns true. In particular, this means that high-resolution
		///   timestamps are not available for .NET 1.1 and .NET 1.0.
		/// 
		///   Additionally, high-resolution timestamps are not intended to
		///   be used on production systems. It is recommended to use them
		///   only during development and debugging. Please see
		///   SmartInspect.Resolution for details.
		/// 
		///   If high-resolution support is not available, this method
		///   simply returns DateTime.Now.
		/// </remarks>
		/// -->

		public static DateTime Now(ClockResolution resolution)
		{
#if (SI_DOTNET_1x)
			return DateTime.Now;
#else
			if (resolution == ClockResolution.High && fSupported)
			{
				return new DateTime(GetTicks() + fOffset);
			}
			else
			{
				return DateTime.Now;
			}
#endif
		}

#if (!SI_DOTNET_1x)
		private static long DoCalibrate()
		{
			long ticks = DateTime.Now.Ticks;
			while (ticks == DateTime.Now.Ticks) ;
			return GetOffset();
		}

		private static long GetMedian(long[] array)
		{
			Array.Sort(array);
			if ((array.Length & 1) == 1)
			{
				return array[(array.Length - 1) / 2];
			}
			else 
			{
				long n = array[(array.Length / 2) - 1];
				long m = array[array.Length / 2];
				return (n + m) / 2;
			}
		}
#endif

		/// <summary>
		///   Calibrates the high-resolution timer and synchronizes it
		///   with the system clock.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Use this method to calibrate the high-resolution timer and
		///   to improve the timer synchronization with the system clock. 
		///   
		///   Background: Without calling this method before calling Now
		///   in high-resolution mode, Now returns a value which is only
		///   loosely synchronized with the system clock. The returned
		///   value might differ by a few milliseconds. This can usually
		///   safely be ignored for a single process application, but may
		///   be an issue for distributed interacting applications with
		///   multiple processes. In this case, calling Calibrate once on
		///   application startup might be necessary to improve the system
		///   clock synchronization of each process in order to get
		///   comparable timestamps across all processes.
		///   
		///   Note that calling this method is quite costly, it can easily
		///   take 50 milliseconds, depending on the system clock timer
		///   resolution of the underlying operation system. Also note that
		///   the general limitations (see SmartInspect.Resolution) of
		///   high-resolution timestamps still apply after calling this
		///   method.
		/// -->
		
		public static void Calibrate()
		{
#if (!SI_DOTNET_1x)
			if (!fSupported)
			{
				return;
			}

			long[] rounds = new long[CALIBRATE_ROUNDS];

			for (int i = 0; i < CALIBRATE_ROUNDS; i++)
			{
				rounds[i] = DoCalibrate();
			}

			fOffset = GetMedian(rounds);
#endif
		}
	}
}
