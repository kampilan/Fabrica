//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   This class is used by the SmartInspect.Watch event.
	/// </summary>
	/// <!--
	/// <remarks>
	///   It has only one public class member named Watch. This member is
	///   a property, which just returns the sent packet.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public sealed class WatchEventArgs: System.EventArgs
	{
		private Watch fWatch;

		/// <summary>
		///   Creates and initializes a WatchEventArgs instance.
		/// </summary>
		/// <param name="watch">
		///   The Watch packet which caused the event.
		/// </param>

		public WatchEventArgs(Watch watch)
		{
			this.fWatch = watch;
		}

		/// <summary>
		///   This read-only property returns the Watch packet, which
		///   has just been sent.
		/// </summary>

		public Watch Watch
		{
			get { return this.fWatch; }
		}
	}
}
