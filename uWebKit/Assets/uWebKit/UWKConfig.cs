using UnityEngine;
using System.Collections;

namespace UWK
{
	
	// UWKConfig is used to configure uWebKit Javascript settings
	// as well as setup any proxy information required
	public static class UWKConfig
	{
		
		/// <summary>
		/// Enable to show JavaScript errors on page
		/// </summary>
		public static bool ShowJavascriptErrors = true;

		/// <summary>
		/// Allow Javascript to create new Popup views (uWebKit Pro only)
		/// </summary>
		public static bool AllowJavascriptPopups = true;
		
		//  Set proxy information here
		
		/// <summary>
		/// enable disable proxy support
		/// </summary>
		public static bool ProxyEnabled = false;

		/// <summary>
		/// The proxy hostname.
		/// </summary>
		public static string ProxyHostname = "";
		/// <summary>
		/// The proxy port.
		/// </summary>
		public static int ProxyPort = 0;

		// define if necessary, leave empty otherwise
		
		/// <summary>
		/// The proxy username.
		/// </summary>
		public static string ProxyUsername = "";
		
		/// <summary>
		/// The proxy password.
		/// </summary>
		public static string ProxyPassword = "";
		
		//  Set Auth information here
		
		public static bool AuthEnabled = false;
		
		public static string AuthUsername = "";
		
		public static string AuthPassword = "";
		
		
		// Enable to debug the UWKProcess (this should always be off for iOS builds)				
#if ((!UNITY_IPHONE && !UNITY_WEBPLAYER) || UNITY_EDITOR)
		public static bool DebugProcess = false;
#else
		private static bool DebugProcess = false;
#endif
		
		
	}
	
}
