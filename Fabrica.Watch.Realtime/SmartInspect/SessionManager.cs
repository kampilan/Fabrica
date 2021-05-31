//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

using System;
using System.Collections;

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Manages and configures Session instances.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class manages and configures a list of sessions. Sessions
	///   can be configured and added to the list with the Add method. To
	///   lookup a stored session, you can use Get. To remove an existing
	///   session from the list, call Delete.
	///   
	///   Stored sessions will be reconfigured if LoadConfiguration has
	///   been called and contains corresponding session entries.
	/// </remarks>
	/// <threadsafety>
	///   This class is fully threadsafe.
	/// </threadsafety>
	/// -->

	public class SessionManager
	{
		private const string PREFIX = "session.";

		private SessionDefaults fDefaults;
		private object fLock;
		private IDictionary fSessions;
		private IDictionary fSessionInfos;

		/// <summary>
		///   Creates and initializes a new SessionManager instance.
		/// </summary>

		public SessionManager()
		{
			this.fLock = new object();
			this.fDefaults = new SessionDefaults();

#if SI_DOTNET_1x
			this.fSessions = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);

			this.fSessionInfos = new Hashtable(
				CaseInsensitiveHashCodeProvider.Default,
				CaseInsensitiveComparer.Default);
#else
			this.fSessions = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);

			this.fSessionInfos = new Hashtable(
				StringComparer.CurrentCultureIgnoreCase);
#endif
		}

		/// <summary>
		///   Loads the configuration properties of this session manager.
		/// </summary>
		/// <param name="config">
		///   The Configuration object to load the configuration from.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method loads the configuration of this session manager
		///   from the passed Configuration object. Sessions which have
		///   already been stored or will be added with Add will
		///   automatically configured with the new properties if the
		///   passed Configuration object contains corresponding session
		///   entries. Moreover, this method also loads the default session
		///   properties which will be applied to all sessions which are
		///   passed to Add.
		/// 
		///   Please see the SmartInspect.LoadConfiguration method for
		///   details on how session entries and session defaults look
		///   like.
		/// </remarks>
		/// -->

		public void LoadConfiguration(Configuration config)
		{
			lock (this.fLock)
			{
				this.fSessionInfos.Clear();
				LoadInfos(config);
				LoadDefaults(config);
			}
		}

		private void LoadDefaults(Configuration config)
		{
			this.fDefaults.Active = 
				config.ReadBoolean("sessiondefaults.active",
				this.fDefaults.Active);

			this.fDefaults.Level = 
				config.ReadLevel("sessiondefaults.level",
				this.fDefaults.Level);

			this.fDefaults.Color = 
				config.ReadColor("sessiondefaults.color",
				this.fDefaults.Color);
		}

		private void LoadInfos(Configuration config)
		{
			for (int i = 0; i < config.Count; i++)
			{
				string key = config.ReadKey(i);

				/* Do we have a session here? */

				if (key.Length < PREFIX.Length)
				{
					continue; /* No, too short */
				}

				string prefix = key.Substring(0, PREFIX.Length);

				if (!prefix.ToLower().Equals(PREFIX)) 
				{
					continue; /* No prefix match */
				}

				string suffix = key.Substring(PREFIX.Length);
				int index = suffix.LastIndexOf('.');

				if (index == -1)
				{
					continue;
				}

				string name = suffix.Substring(0, index);

				/* Duplicate session configuration entry? */

				if (this.fSessionInfos.Contains(name))
				{
					continue;
				}

				SessionInfo info = LoadInfo(name, config);
				this.fSessionInfos[name] = info;

				/* Do we need to update a related session? */

				Session session =
					(Session)this.fSessions[name];

				if (session != null)
				{
					Assign(session, info);
				}
			}
		}

		private SessionInfo LoadInfo(string name, Configuration config)
		{
			SessionInfo info = new SessionInfo();

			info.Name = name;
			info.HasActive = config.Contains(PREFIX + name + ".active");

			if (info.HasActive)
			{
				info.Active = config.ReadBoolean(PREFIX + name + ".active",
					true);
			}

			info.HasLevel = config.Contains(PREFIX + name + ".level");

			if (info.HasLevel)
			{
				info.Level = config.ReadLevel(PREFIX + name + ".level",
					Level.Debug);
			}

			info.HasColor = config.Contains(PREFIX + name + ".color");

			if (info.HasColor)
			{
				info.Color = config.ReadColor(PREFIX + name + ".color",
					Session.DEFAULT_COLOR);
			}

			return info;
		}

		/// <summary>
		///   Configures a passed Session instance and optionally saves it
		///   for later access.
		/// </summary>
		/// <param name="session">
		///   The session to configure and to save for later access, if
		///   desired.
		/// </param>
		/// <param name="store">
		///   Indicates if the passed session should be stored for later
		///   access.
		/// </param>
		/// <!--
		/// <remarks>
		///   This method configures the passed session with the default
		///   session properties as specified by the Defaults property.
		///   This default configuration can be overridden on a per-session
		///   basis by loading the session configuration with the
		///   LoadConfiguration method.
		/// 
		///   If the 'store' parameter is true, the passed session is stored
		///   for later access and can be retrieved with the Get method. To
		///   remove a stored session from the internal list, call Delete. 
		///   
		///   If this method is called multiple times with the same session
		///   name, then the Get method operates on the session which got
		///   added last. If the session parameter is null, this method does
		///   nothing.
		/// </remarks>
		/// -->

		public void Add(Session session, bool store)
		{
			if (session == null)
			{
				return;
			}

			lock (this.fLock)
			{
				this.fDefaults.Assign(session);

				if (store)
				{
					this.fSessions[session.Name] = session;
					session.IsStored = true;
				}

				Configure(session, session.Name);
			}
		}

		/// <summary>
		///   Returns a previously added session.
		/// </summary>
		/// <param name="name">
		///   The name of the session to lookup and return. Not allowed to
		///   be null.
		/// </param>
		/// <returns>
		///   The requested session or null if the supplied name is null
		///   or if the session is unknown.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This method returns a session which has previously been
		///   added with the Add method and can be identified by the
		///   supplied name parameter. If the requested session is unknown
		///   or if the name argument is null, this method returns null.
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

		public Session Get(string name)
		{
			if (name == null)
			{
				return null;
			}

			lock (this.fLock)
			{
				return (Session) this.fSessions[name];
			}
		}

		/// <summary>
		///   Gets the session associated with the specified session name.
		/// </summary>
		/// <param name="name">
		///   The name of the session to lookup and return. Not allowed to
		///   be null.
		/// </param>
		/// <returns>
		///   The requested session or null if the supplied name is
		///   null or if the session is unknown.
		/// </returns>
		/// <!--
		/// <remarks>
		///   This indexer returns the session which has previously been
		///   added with the Add method and can be identified by the
		///   specified session name. If the specified session is unknown
		///   or if the name parameter is null, null is returned. See Get
		///   for more information.
		/// </remarks>
		/// -->

		public Session this[string name]
		{
			get { return Get(name); }
		}

		private void Configure(Session session, string name)
		{
			SessionInfo info = 
				(SessionInfo) this.fSessionInfos[name];

			if (info != null)
			{
				Assign(session, info);
			}
		}

		private void Assign(Session session, SessionInfo info)
		{
			if (info.Active)
			{
				if (info.HasColor)
				{
					session.Color = info.Color;
				}

				if (info.HasLevel)
				{
					session.Level = info.Level;
				}

				if (info.HasActive)
				{
					session.Active = info.Active;
				}
			}
			else
			{
				if (info.HasActive)
				{
					session.Active = info.Active;
				}

				if (info.HasLevel)
				{
					session.Level = info.Level;
				}

				if (info.HasColor)
				{
					session.Color = info.Color;
				}
			}
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
		///   with the Add method. After this method returns, the Get method
		///   returns null when called with the same session name unless a
		///   different session with the same name has been added.
		///   
		///   This method does nothing if the supplied session argument is
		///   null.
		/// </remarks>
		/// -->

		public void Delete(Session session)
		{
			if (session == null)
			{
				return;
			}

			lock (this.fLock)
			{
				string name = session.Name;

				if (this.fSessions[name] == session)
				{
					this.fSessions.Remove(name);
				}
			}
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
		///   Get method to lookup the supplied session.
		/// </remarks>
		/// -->

		protected internal void Update(Session session, string to,
			string from)
		{
			if (session == null)
			{
				return;
			}

			if (from == null || to == null)
			{
				return;
			}

			lock (this.fLock)
			{
				if (this.fSessions[from] == session)
				{
					this.fSessions.Remove(from);
				}

				Configure(session, to);
				this.fSessions[to] = session;
			}
		}

		/// <summary>
		///   Clears the configuration of this session manager and removes
		///   all sessions from the internal lookup table.
		/// </summary>

		public void Clear()
		{
			lock (this.fLock)
			{
				this.fSessions.Clear();
				this.fSessionInfos.Clear();
			}
		}

		/// <summary>
		///   Specifies the default property values for new sessions.
		/// </summary>
		/// <!--
		/// <remarks>
		///   This property lets you specify the default property values
		///   for new sessions which will be passed to the Add method.
		///   Please see the Add method for details. For information about
		///   the available session properties, please refer to the
		///   documentation of the Session class.
		/// </remarks>
		/// -->

		public SessionDefaults Defaults
		{
			get { return this.fDefaults; }
		}
	}
}
