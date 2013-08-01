using System;
using UnityEngine;
using UWK;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Example of a web page generated in Unity, using the Javascript Bridge
/// </summary>
public class UnityInfoPage
{

	public static string HTML = "";

	// Generate the page HTML
	static UnityInfoPage ()
	{
		
		string[] props = new string[] { "platform", "unityVersion", "systemLanguage", "runInBackground", "isEditor", "dataPath", "persistentDataPath" };
		
		StringBuilder sb = new StringBuilder ();
		
		// Some nice CSS
		sb.Append (@"<html> <head>
		<style type=""text/css"">
		body
		{
			background-color: transparent;
		}
		h1
		{
		color:black;
		text-align:left;
		}
		p
		{
			font-family:""Times New Roman"";
			font-size:20px;
		}
		</style>
		</head>
		<body>");
		
		sb.Append ("<h1> UWebKit JavaScript Bridge Info </h1>");
		
		//sb.Append ("<img src=\"file:///C:/javaBridge.png\" /><br>");
		
		sb.Append ("<input type='button' value=\"Hello\" onclick='Unity.invoke(\"SayHello\", 1, \"Testing123\", \"45678\")' />");
		
		sb.Append (@"<table border=""1"">");
		
		foreach (string p in props) {
			sb.AppendFormat (@"
			<tr>
			<td>Unity.{0}</td>
			<td id = Unity_{0}></td>
			</tr>", p);
		}
		
		sb.Append ("</table>");
		
		sb.Append ("<script type='text/javascript'>");
		
		foreach (string p in props) {
			sb.AppendFormat ("document.getElementById('Unity_{0}').innerText = Unity.{0};", p);
		}
		
		sb.Append ("</script>");
		
		
		sb.AppendFormat ("<h4>This page generated in Unity on {0}</h4>", DateTime.UtcNow.ToLocalTime ());
		
		sb.Append ("</body> </html>");
		
		HTML = sb.ToString ();
		
	}

	// Example delegate called as a callback from Javascript on the page
	public static void OnSayHello (object sender, BridgeEventArgs args)
	{
		
		#if UNITY_EDITOR
			if (args.Args.Length == 3 && args.Args[0] == "1" && args.Args[1] == "Testing123" && args.Args[2] == "45678")
				EditorUtility.DisplayDialog ("Hello!", "The UWebKit JavaScript Bridge is alive and well!", "Awesome");
			else
				EditorUtility.DisplayDialog ("Hello!", "The UWebKit JavaScript Bridge callback was invoked, but args were wrong!", "Ok");
		#endif
		
	}

	static bool props = false;

	public static void SetProperties ()
	{
		
		if (props)
			return;
		
		props = true;
		
		// Bind Javascript callback to Unity.SayHello js function
		Bridge.BindCallback ("Unity", "SayHello", OnSayHello);
		
		// Export a bunch of unity variables to JavaScript properties which can then 
		// be accessed on pages
		Bridge.SetProperty ("Unity", "unityVersion", Application.unityVersion);
		
		Bridge.SetProperty ("Unity", "loadedLevel", Application.loadedLevel);
		Bridge.SetProperty ("Unity", "loadedLevelName", Application.loadedLevelName);
		Bridge.SetProperty ("Unity", "isLoadingLevel", Application.isLoadingLevel);
		Bridge.SetProperty ("Unity", "levelCount", Application.levelCount);
		Bridge.SetProperty ("Unity", "streamedBytes", Application.streamedBytes);
		
		Bridge.SetProperty ("Unity", "isPlaying", Application.isPlaying);
		Bridge.SetProperty ("Unity", "isEditor", Application.isEditor);
		Bridge.SetProperty ("Unity", "isWebPlayer", Application.isWebPlayer);
		
		Bridge.SetProperty ("Unity", "platform", Application.platform.ToString ());
		
		Bridge.SetProperty ("Unity", "runInBackground", Application.runInBackground);
		
		Bridge.SetProperty ("Unity", "dataPath", Application.dataPath);
		Bridge.SetProperty ("Unity", "persistentDataPath", Application.persistentDataPath);
		Bridge.SetProperty ("Unity", "temporaryCachePath", Application.temporaryCachePath);
		
		Bridge.SetProperty ("Unity", "srcValue", Application.srcValue);
		Bridge.SetProperty ("Unity", "absoluteURL", Application.absoluteURL);
		
		Bridge.SetProperty ("Unity", "webSecurityEnabled", Application.webSecurityEnabled);
		
		Bridge.SetProperty ("Unity", "targetFrameRate", Application.targetFrameRate);
		
		Bridge.SetProperty ("Unity", "systemLanguage", Application.systemLanguage.ToString ());
		
		Bridge.SetProperty ("Unity", "backgroundLoadingPriority", Application.backgroundLoadingPriority.ToString ());
		
		Bridge.SetProperty ("Unity", "internetReachability", Application.internetReachability.ToString ());
		
	}
}

