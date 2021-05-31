//
// <!-- Copyright (C) 2003-2010 Gurock Software GmbH. All rights reserved. -->
//

namespace Fabrica.Watch.SmartInspect
{
	/// <summary>
	///   Provides automatically created objects for using the SmartInspect
	///   and Session classes.
	/// </summary>
	/// <!--
	/// <remarks>
	///   This class provides a static property called Si of type SmartInspect.
	///   Furthermore a Session instance named Main with Si as parent is ready
	///   to use. The SiAuto class is especially useful if you do not want to 
	///   create SmartInspect and Session instances by yourself.
	///   
	///   The <link SmartInspect.Connections, connections string> of Si is
	///   set to "pipe(reconnect=true, reconnect.interval=1s)", the
	///   <link SmartInspect.AppName, application name> to "Auto" and the
	///   <link Session.Name, session name> of Main to "Main".
	///   
	///   <b>Please note that the default connections string has been
	///   changed in SmartInspect 3.0</b>. In previous versions, the default
	///   connections string was set to "tcp()".
	/// </remarks>
	/// <threadsafety>
	///   The public static members of this class are threadsafe.
	/// </threadsafety>
	/// <example>
	/// <code>
	/// // [C# Example]
	///
	/// using Gurock.SmartInspect;
	///
	/// public class SiAutoExample
	/// {
	///		public static void Main(string[] args)
	///		{
	///			SiAuto.Si.Enabled = true;
	///			SiAuto.Main.EnterProcess("SiAutoExample");
	///			try
	///			{
	///				.
	///				.
	///				.
	///			}
	///			finally
	///			{
	///				SiAuto.Main.LeaveProcess("SiAutoExample");
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
	/// Module SiAutoExample
	///		Sub Main()
	///			SiAuto.Si.Enabled = True
	///			SiAuto.Main.EnterProcess("SiAutoExample")
	///			Try
	///				.
	///				.
	///				.
	///			Finally
	///				SiAuto.Main.LeaveProcess("SiAutoExample")
	///			End Try
	///		End Sub
	/// End Module
	/// </code>
	/// </example>
	/// -->

	public sealed class SiAuto
	{
		private const string APPNAME = "Auto";
		private const string CONNECTIONS = 
			"pipe(reconnect=true, reconnect.interval=1s)";
		private const string SESSION = "Main";

		private static Session fMain;
		private static SmartInspect fSi;

		private SiAuto() {}

		static SiAuto()
		{
			fSi = new SmartInspect(APPNAME);
			fSi.Connections = CONNECTIONS;
			fMain = fSi.AddSession(SESSION, true);
		}

		/// <summary>
		///   Automatically created SmartInspect instance.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The <link SmartInspect.Connections, connections string> is set
		///   to "pipe(reconnect=true, reconnect.interval=1s)". Please see
		///   Protocol.IsValidOption for information on the used options. The
		///   <link SmartInspect.AppName, application name> is set to "Auto".
		///
		///   <b>Please note that the default connections string has been
		///   changed in SmartInspect 3.0</b>. In previous versions, the
		///   default connections string was set to "tcp()".
		/// </remarks>
		/// -->

		public static SmartInspect Si
		{
			get { return fSi; }
		}

		/// <summary>
		///   Automatically created Session instance.
		/// </summary>
		/// <!--
		/// <remarks>
		///   The <link Session.Name, session name> is set to "Main" and
		///   the <link Session.Parent, parent> to SiAuto.Si.
		/// </remarks>
		/// -->

		public static Session Main
		{
			get { return fMain; }
		}
	}
}
