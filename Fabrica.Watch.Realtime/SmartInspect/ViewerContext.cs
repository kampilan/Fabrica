//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.IO;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Is the abstract base class for a viewer context. A viewer context
	///   is the library-side representation of a viewer in the Console.
	/// </summary>
	/// <!--
	/// <remarks>
	///   A viewer context contains a viewer ID and data which can be
	///   displayed in a viewer in the Console. Every viewer in the Console
	///   has a corresponding viewer context class in this library. A viewer
	///   context is capable of processing data and to format them in a way
	///   so that the corresponding viewer in the Console can display it.
	///   
	///   Viewer contexts provide a simple way to extend the functionality
	///   of the SmartInspect .NET library. See the Session.LogCustomContext
	///   method for a detailed example.
	/// </remarks>
	/// <threadsafety>
	///   This class is not guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public abstract class ViewerContext: System.IDisposable
	{
		private ViewerId fVi;

		/// <summary>
		///   Creates and initializes a ViewerContext instance.
		/// </summary>
		/// <param name="vi">The viewer ID to use.</param>

		protected ViewerContext(ViewerId vi)
		{
			this.fVi = vi;
		}

		/// <summary>
		///   Returns the viewer ID which specifies the viewer
		///   to use in the Console.
		/// </summary>

		public ViewerId ViewerId
		{
			get { return this.fVi; }
		}

		/// <summary>
		///   Returns the actual data which will be displayed in the
		///   viewer specified by the ViewerId property.
		/// </summary>

		public abstract Stream ViewerData { get; }

		/// <summary>
		///   Overloaded. Intended to release any resources.
		/// </summary>
		/// <param name="disposing">
		///   True if managed resources should be released and false
		///   otherwise.
		/// </param>

		protected abstract void Dispose(bool disposing);

		/// <summary>
		///   Overloaded. Releases any managed and unmanaged resources
		///   of this viewer context.
		/// </summary>

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
