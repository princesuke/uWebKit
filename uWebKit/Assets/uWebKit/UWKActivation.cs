/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using UWK;

#if UNITY_EDITOR
using UnityEditor;
using System;

/// <summary>
/// Internal class used for product activation
/// </summary>
public class UWKActivation : MonoBehaviour
{

	void Awake ()
	{
		// ensure Core is up
		UWKCore.Init (true);
	}


	// activation
	bool activating = false;
	bool activateWindow = true;
	string activationCode = "";
	static Rect windowRect = new Rect (0, 0, 400, 300);
	bool pro = true;
	bool standard = false;

	bool showActivationMessage = false;

	void reset ()
	{
		activating = false;
		activateWindow = true;
		Center ();
	}

	// Get the center position
	public void GetCenterPos (ref Vector2 pos)
	{
		pos.x = Screen.width / 2 - windowRect.width / 2;
		pos.y = Screen.height / 2 - windowRect.height / 2;
	}

	// Center the browser on the screen
	public void Center ()
	{
		Vector2 v = new Vector2 ();
		
		GetCenterPos (ref v);
		
		windowRect.x = v.x;
		windowRect.y = v.y;
	}


	void processInbound (object sender, CommandProcessEventArgs args)
	{
		Command cmd = args.Cmd;
		
		// activation
		if (cmd.fourcc == "ACTR") {
			
			processACTR (cmd);
			
		}
	}


	void processACTR (Command cmd)
	{
		int code = cmd.iParams[0];
		
		if (!showActivationMessage)
			return;
		
		if (code == 0) {
			
			EditorUtility.DisplayDialog ("uWebKit Activation Failed", "Please contact sales@uwebkit.com for more information", "Ok");
			reset ();
			
		}
		
		if (code == 1) {
			activating = false;
			activateWindow = false;
			
			string txt = "Trial Activated";
			
			if (cmd.numIParams == 2) {
				txt = "Trial has " + cmd.iParams[1] + " days left";
			}
			
			string title = "uWebKit Standard Trial Activated";
			
			if (pro)
				title = "uWebKit Pro Trial Activated";
			
			
			EditorUtility.DisplayDialog (title, txt, "Ok");
			
			EditorApplication.ExecuteMenuItem ("Edit/Play");
		}
		
		// act1 or act2
		if (code == 2 || code == 3) {
			// passed
			activating = false;
			activateWindow = false;
			
			EditorUtility.DisplayDialog ("uWebKit Activated", "Thank you!", "Ok");
			
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			
		}
		
		if (code == 6) {
			// problem
			activating = false;
			activateWindow = false;
			
			EditorUtility.DisplayDialog ("uWebKit Activation", "There was an issue contacting the Activation Server.\n\nThe product is available, however you may be asked to activate again.", "Ok");
			
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			
		}
		
		if (code == 7) {
			EditorUtility.DisplayDialog ("uWebKit Activation", "This key is invalid, please check the key and try again.\n", "Ok");
		}
		
		if (code == 8) {
			activating = false;
			activateWindow = true;
			EditorUtility.DisplayDialog ("uWebKit Activation", "The key is invalid, please check the key and reactivate.\n", "Ok");
		}
		
		if (code == 4) {
			// no activations
			
			EditorUtility.DisplayDialog ("uWebKit Activation Failed", "Activation Count exceeded, please contact sales@uwebkit.com for more information", "Ok");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			
			
		}
		
		if (code == 5) {
			EditorUtility.DisplayDialog ("uWebKit Trial Expired", "Trial expired, please contact sales@uwebkit.com for more information", "Ok");
			Application.OpenURL ("http://www.uwebkit.com/uwebkit/store");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
		}
		
		
	}

	bool validateKey (string key)
	{
		if (!key.StartsWith ("T-") && !key.StartsWith ("P-") && !key.StartsWith ("S-")) {
			return false;
		}
		
		if (key.Length != 21)
			return false;
		
		int count = 0;
		foreach (char c in key)
			if (c == '-')
				count++;
		
		if (count != 4)
			return false;
		
		return true;
	}

	void windowFunction (int windowID)
	{
		Rect titleRect = new Rect (0, 0, 400, 24);
		
		if (!activating) {
			
			GUILayout.BeginVertical ();
			
			Color previousColor = GUI.color;
			
			GUI.color = Color.cyan;
			
			GUILayout.Label ("IMPORTANT: Please note that if you are behind a proxy, edit uWebKit/UWKConfig.cs to set proxy settings");
			
			GUI.color = previousColor;
			
			GUILayout.Space (8);			
			
			GUILayout.BeginHorizontal ();
			
			GUILayout.Label ("Activation Code", GUILayout.Width (96));
			
			activationCode = GUILayout.TextField (activationCode, 64, GUILayout.Width (280)).Trim ();
			
			// we're catching p on command-p to run scene
			if (activationCode.StartsWith ("p")) {
				activationCode = "";
			}
			
			GUILayout.EndHorizontal ();
			
			if (activationCode.StartsWith ("T-")) {
				
				GUILayout.Space (20);
				
				bool nstandard = GUILayout.Toggle (standard, " Select for uWebKit Standard Trial");
				
				bool npro = GUILayout.Toggle (pro, " Select for uWebKit Pro Trial");
				
				if (standard != nstandard) {
					if (nstandard) {
						standard = true;
						pro = false;
					}
				} else if (pro != npro) {
					if (npro) {
						standard = false;
						pro = true;
					}
				}
				
				GUILayout.Space (10);
				
				if (GUILayout.Button ("Compare uWebKit Standard and Pro Features", GUILayout.Height (32))) {
					
					Application.OpenURL ("http://uwebkit.com/uwebkit/compare");
				}
				
				GUILayout.Space (20);
				
			} else {
				GUILayout.Space (64);
			}			
			
			GUILayout.BeginHorizontal ();
			
			if (GUILayout.Button ("Activate", GUILayout.MaxWidth (164), GUILayout.Height (64))) {
				
				if (!String.IsNullOrEmpty (UWKCore.ProductKey)) {
					if (UWKCore.ProductKey.StartsWith ("T-")) {
						if (activationCode.StartsWith ("T-")) {
							if (UWKCore.ProductKey != activationCode) {
								EditorUtility.DisplayDialog ("uWebKit Trial", "This installation of uWebKit has been activated with a different Trial Key.\n" + "If you need a trial extension, please contact sales@uwebkit.com with your Trial Key", "Ok");
								EditorApplication.ExecuteMenuItem ("Edit/Play");
							}
						}
					}
					
					
				}
				
				if (!validateKey (activationCode)) {
					EditorUtility.DisplayDialog ("uWebKit Activation", "This key is invalid, please check the key and try again.\n", "Ok");
				} else {
					showActivationMessage = true;
					Command ncmd = Command.NewCommand ("ACTV", activationCode, pro ? 1 : 0);
					ncmd.Post ();
					activating = true;
				}
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Purchase", GUILayout.MaxWidth (92), GUILayout.Height (64))) {
				
				Application.OpenURL ("http://www.uwebkit.com/uwebkit/store");
				
			}
			
			GUILayout.FlexibleSpace ();
			
			if (GUILayout.Button ("Get Trial Key", GUILayout.MaxWidth (92), GUILayout.Height (64))) {
				
				Application.OpenURL ("http://uwebkit.com/download");
				
			}
			
			
			GUILayout.EndHorizontal ();
			
			GUILayout.EndVertical ();
			
		} else {
			GUILayout.Label ("Activating... Please Wait");
		}
		
		GUI.DragWindow (titleRect);
		
	}

	void OnGUI ()
	{
		if (activateWindow)
			windowRect = GUILayout.Window (-1, windowRect, windowFunction, "uWebKit Activation");
	}

	// Use this for initialization
	void Start ()
	{
		
		// listen in on inbound commands
		Plugin.ProcessInbound += processInbound;
		
		Center ();
	}

	// Update is called once per frame
	bool askChange = false;
	void Update ()
	{
		
		if (!askChange) {
			
#if UNITY_WEBPLAYER		
			EditorUtility.DisplayDialog ("uWebKit Activation", "Please switch Build Settings to PC/Mac Standalone before activating", "OK");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			askChange = true;
			return;
#endif
			
			if (!String.IsNullOrEmpty (UWKCore.ProductKey)) {
				
				askChange = true;
				
				if (EditorUtility.DisplayDialog ("uWebKit Activation", "This installation of uWebKit is already activated.\nWould you like to change your activation?\n(If not, please load another scene)", "OK", "Cancel")) {
					
				} else {
					showActivationMessage = false;
					EditorApplication.ExecuteMenuItem ("Edit/Play");
				}
			}
		}
		
		
	}
}

#endif
