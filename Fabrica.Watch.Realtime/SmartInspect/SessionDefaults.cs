//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using Fabrica.Utilities.Drawing;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Specifies the default property values for newly created
	///   sessions.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class is used by the SmartInspect class to customize the
	///   default property values for newly created sessions. Sessions
	///   that will be created by or passed to the AddSession method of
	///   the SmartInspect class will be automatically configured with
	///   the values of the SmartInspect.SessionDefaults property.
	/// </remarks>
	/// <threadsafety>
	///   This class is guaranteed to be threadsafe.
	/// </threadsafety>
	/// -->

	public class SessionDefaults
	{
		private bool fActive;
		private Color fColor;
		private Level fLevel;
		private object fLock;

		/// <summary>
		///   Creates and initializes a new SessionDefaults instance.
		/// </summary>

		public SessionDefaults()
		{
			this.fLock = new object();
			this.fActive = true;
			this.fColor = Session.DEFAULT_COLOR;
			this.fLevel = Level.Debug;
		}

		/// <summary>
		///   Specifies the default Active property for newly created
		///   sessions.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Please see Session.Active for general information about the
		///   active status of sessions.
		/// </remarks>
		/// -->

		public bool Active
		{
			get { return this.fActive; }
			set { this.fActive = value; }
		}

		/// <summary>
		///   Specifies the default Color property for newly created
		///   sessions.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Please see Session.Color for general information about the
		///   background color of sessions.
		/// </remarks>
		/// -->

		public Color Color
		{
			get 
			{
				lock (this.fLock)
				{
					return this.fColor; /* Not atomic */
				}
			}

			set 
			{
				lock (this.fLock)
				{
					this.fColor = value; /* Not atomic */
				}
			}
		}

		/// <summary>
		///   Specifies the default Level property for newly created
		///   sessions.
		/// </summary>
		/// <!--
		/// <remarks>
		///   Please see Session.Level for general information about the
		///   log level of sessions.
		/// </remarks>
		/// -->

		public Level Level
		{
			get { return this.fLevel; }
			set { this.fLevel = value; }
		}

		internal void Assign(Session session)
		{
			session.Active = Active;
			session.Level = Level;
			session.Color = Color;
		}
	}
}
