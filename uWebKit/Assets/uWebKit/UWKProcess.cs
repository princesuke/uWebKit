/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UWK
{
	/// <summary>
	/// Class representing the web process
	/// Rendering web content in an external process keeps the 
	/// Unity memory space clean, utilization of additional processor cores, and 
	/// lets the Unity application recover from issues with web pages
	/// </summary>
	public class Process : System.Diagnostics.Process
	{
		
		/// <summary>
		/// Start the web process
		/// </summary>
		public bool Go ()
		{
#if (UNITY_IPHONE && !UNITY_EDITOR) || UNITY_WEBPLAYER
			
			return true;
			
#else
			if (UWKConfig.DebugProcess)
				return true;
			
			// start info passing in the game's process id as a parameter
			StartInfo.UseShellExecute = false;
			
			// Redirecting these on OSX causes problems
			//StartInfo.RedirectStandardOutput = true;
			//StartInfo.RedirectStandardError = true;
			
			string editorPath = "";
			
			if (Application.isEditor) {
				#if UNITY_STANDALONE_OSX || UNITY_IPHONE
				editorPath = "/Editor";
				#elif UNITY_STANDALONE_WIN
				editorPath = "\\Editor";
				#endif
			}
			
			#if UNITY_STANDALONE_OSX || UNITY_IPHONE
			StartInfo.FileName = Application.dataPath + editorPath + "/uWebKit/Native/Mac/UWKProcess.app/Contents/MacOS/UWKProcess";
			#elif UNITY_STANDALONE_WIN
			string filename = Application.dataPath + editorPath + "\\uWebKit\\Native\\Windows\\UWKProcess.exe";
			if (!File.Exists (filename)) {
				return false;
			}
			StartInfo.FileName = filename;
			#elif UNITY_EDITOR
			EditorUtility.DisplayDialog ("uWebKit Notification", "Please select Windows/Mac Standalone or iOS platform in Build Settings", "Ok");
			#endif
			
			StartInfo.CreateNoWindow = true;

#if UNITY_STANDALONE_OSX || UNITY_IPHONE
			try {
				if (!Start())
				{
					if (makeExecutable())
					{
						Start();
						return true;
					}
					else {
						return false;
					}
				}
			} catch (Exception e) {
				
				//Debug.Log(e);
				//Debug.Log("Executing chmod");
				
			  // attempt to chmod
				if (makeExecutable())
				{
					Start();
					return true;
				} else {
					throw(e);
				}
				
			}
			
#else 
			Start ();
#endif
			
			
			
			return true;
#endif			
		}

		/// <summary>
		/// Kill the web process
		/// </summary>
		public void KillIt ()
		{
#if !UNITY_IPHONE || UNITY_EDITOR
	
			bool hasExited = false;
			
#if !UNITY_WEBPLAYER
			hasExited = HasExited;
#endif
			if (hasExited)
				return;
			
			if (KillAttempted)
				return;
			
			KillAttempted = true;

#if !UNITY_WEBPLAYER			
			try {
				Kill ();
			} catch (Exception e) {
				Debug.Log (e);
			}
#endif
			
#endif			
		}

		/// <summary>
		/// Posts an exit command to the command queue
		/// </summary>
		public void Stop ()
		{
			
#if !UNITY_IPHONE || UNITY_EDITOR
			if (UWKConfig.DebugProcess)
				return;
			
			Command cmd = Command.NewCommand ("EXIT");
			cmd.Post ();
#endif			
		}
		
		public bool KillAttempted = false;
		
		bool makeExecutable ()
		{
			
			System.Diagnostics.Process chmod = new System.Diagnostics.Process ();
			
			chmod.StartInfo.UseShellExecute = false;
			
			string editorPath = "";
			
			if (Application.isEditor) {
				editorPath = "/Editor";
			}
			
			string exec = "\"" + Application.dataPath + editorPath + "/uWebKit/Native/Mac/UWKProcess.app/Contents/MacOS/UWKProcess" + "\"";
			
			chmod.StartInfo.FileName = "chmod";
			chmod.StartInfo.Arguments = "+x " + exec;
			
			try {
				bool b = chmod.Start ();
				if (!b)
					return false;
				
				chmod.WaitForExit ();
				
				return true;
				
			} catch (Exception e) {
				Debug.Log (e.Message);
				return false;
			}
				
		}
			
			
	}
		
}
	

