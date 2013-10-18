/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using UWK;
using System;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// UWKCore is responsible for creating/managing views and popup windows.
/// It is automatically created and added at runtime when your application
/// requests a web view to be created
/// </summary>
public class UWKCore : MonoBehaviour
{

	#region Public Interface

	/// <summary>
	/// Main initialization of web core, must be called before any UWKViews are created.
	/// </summary>
	public static void Init (bool inactivation)
	{
		if (sInstance != null)
			return;
		
		Application.runInBackground = true;
		
		string lang = Application.systemLanguage.ToString ();
		
		#if UNITY_EDITOR || !UNITY_IPHONE
		if (lang == "Chinese" || lang == "Japanese" || lang == "Korean")
			imeEnabled = true;
		#endif
		
		GameObject go = new GameObject ("UWKCore");
		
		UnityEngine.Object.DontDestroyOnLoad (go);
		
		UWKCore core = go.AddComponent<UWKCore> ();
		
		sInstance = core;
		#if UNITY_EDITOR
		activation = inactivation;
		#endif
		core.init ();
	}

	/// <summary>
	/// Unity 3.4 MonoDevelop on Windows chokes on default parameters, so we have an override
	/// </summary>
	public static void Init ()
	{
		Init (false);
	}

	/// <summary>
	/// The main method to create a UWKView, note that views with identical names
	/// are reused
	/// </summary>
	public static UWKView CreateView (string name, string URL, int width, int height, bool smartRects)
	{
		
		if (RuntimeError)
			return null;
		
		// ensure core is up
		UWKCore.Init ();
		
		UWKView view;
		if (viewLookup.TryGetValue (name, out view)) {
			//Debug.LogWarning ("View already exists " + name);
			return view;
		}
		
		if (StandardVersion) {
			if (viewLookup.Count >= 1) {
				Debug.LogError ("uWebKit Standard Version supports 1 web view");
				#if UNITY_EDITOR
				EditorUtility.DisplayDialog ("uWebKit Standard Version", "uWebKit Pro required for multiple web views", "Ok");
				#endif
				return null;
			}
			
		}
		
		view = CreateViewInternal (name, width, height, smartRects);
		view.URL = URL;
		return view;
		
	}
	
	public static int GetNumViews() 
	{
		return viewLookup.Count;
	}
	
	public static UWKView CreateView (string name, int width, int height, bool smartRects)
	{
		return CreateView (name, "", width, height, smartRects);
	}
	
	public static UWKView CreateView (string name, string URL, int width, int height)
	{
		return CreateView (name, URL, width, height, true);
	}
	
	public static UWKView CreateView (string name, int width, int height)
	{
		return CreateView (name, width, height, true);
	}

	private static UWKView CreateViewInternal (string name, int width, int height, bool smartRects)
	{
		
		GameObject go = new GameObject ("UWKView_" + name);
		
		UnityEngine.Object.DontDestroyOnLoad (go);
		
		UWKView view;
		
		viewLookup [name] = view = go.AddComponent<UWKView> ();
		view.Name = name;
		view.Width = width;
		view.Height = height;
		view.SmartRects = smartRects;
		
		if (!booted)
			view.enabled = false;
		
		GameObject core = GameObject.Find ("UWKCore");
		go.transform.parent = core.transform;
		
		return view;
		
	}

	private static Dictionary<string, UWKPopup> popups = new Dictionary<string, UWKPopup> ();

	private static void CreateJSPopup (string name, int width, int height)
	{
		UWKView view = CreateViewInternal (name, width, height, true);
		view.JSPopup = true;
		
		GameObject go = new GameObject ("UWKPopup_" + name);
		
		UnityEngine.Object.DontDestroyOnLoad (go);
		
		UWKPopup p = go.AddComponent<UWKPopup> ();
		p.View = view;
		
		popups [name] = p;
	}
	
	/// <summary>
	/// Closes the popup.
	/// </summary>
	public static void ClosePopup (UWKPopup p)
	{
		CloseJSPopupRequested (p);
	}
	
	/// <summary>
	/// Closes the JS popup, this is generated from the UWKProcess in response to a javascript close call
	/// </summary>
	private static void CloseJSPopupRequested (UWKPopup p)
	{
		if (!popups.ContainsValue (p)) {
			Debug.LogWarning ("Warning: CLoseJSPopupRequested called on mislinked popup");
			return;
		}
		
		p.View.Remove ();
		
		string k = null;
		foreach (string s in popups.Keys) {
			if (popups [s] == p) {
				k = s;
				break;
			}
		}
		
		popups.Remove (k);
		
		UnityEngine.Object.DestroyObject (p.gameObject);
		UnityEngine.Object.DestroyObject (p);
		
	}

	/// <summary>
	/// Remove a UWKView from the core
	/// </summary>
	public static void RemoveView (UWKView view)
	{
		if (sInstance == null) {
			return;
		}
		
		if (GetView (view.Name) == null)
			return;
		
		viewLookup.Remove (view.Name);
		view.Remove ();
	}

	/// <summary>
	/// Gets the UWKView associated with the unique name
	/// </summary>
	public static UWKView GetView (string name)
	{
		if (sInstance == null) {
			return null;
		}
		
		UWKView view;
		if (viewLookup.TryGetValue (name, out view))
			return view;
		return null;
	}

	/// <summary>
	/// Clear the persistent cookies (saved session data) associated with this application
	/// </summary>
	public static void ClearCookies ()
	{
		Command cmd = Command.NewCommand ("CLCK");
		cmd.Post ();
	}

	#endregion

	#region Internal


	/// <summary>
	/// Called once per frame and mainly checks on the web rendering process status
	/// </summary>
	void Update ()
	{
		if (RuntimeError || ProcessError)
			return;
		
#if UNITY_EDITOR		
		if(EditorApplication.isCompiling) 
		{
			Debug.Log("Unity is recompiling scripts, play session ended.");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			return; 
		}
#endif		

#if (!UNITY_IPHONE || UNITY_EDITOR) && !UNITY_WEBPLAYER
		if (!UWKConfig.DebugProcess) {
			
			bool hasExited = process.HasExited;
			
			if (!restarting && !hasExited && !Plugin.UWK_ProcessResponding())
				unresponsiveTime += Time.deltaTime;
			else
				unresponsiveTime = 0;
			
			
			if ( !hasExited && unresponsiveTime > 16 && !process.KillAttempted) {
				
					Debug.Log ("Killing zombie UWKProcess");
				
					unresponsiveTime = 0;
				
					if (UWKProcessZombified != null)
						UWKProcessZombified ();
						
					UWKView.InvalidateAllViews ();
						
					processUp = false;
					process.KillIt ();
						
					Plugin.ClearCommands ();
			}
						
			else if (hasExited) {
					
				if (hasExited && !restarting) {
					Debug.Log ("Process has exited");
					
					restarting = true;
					
					if (UWKProcessWillRestart != null)
						UWKProcessWillRestart ();
					
						
					Plugin.ClearCommands ();
					Plugin.ProcessInbound += processInbound;
					Plugin.ProcessReturn += processReturn;
						
					process = new Process ();
					
					bool result = process.Go ();
	                       
					if (!result) {
						Debug.Log ("process.Go() == false");
					}
	
					if (result && UWKProcessRestarted != null)
						UWKProcessRestarted ();
	
					if (result && UWKProcessStarted != null)
						UWKProcessStarted ();
					
				}
			}
		}
#endif
			
		sInstance.update ();
	}

	void OnDestroy ()
	{
		Shutdown ();
	}

	private static void Shutdown ()
	{
		if (sInstance == null)
			return;
		
		sInstance.shutdown ();
		
		sInstance = null;
		
	}

	void processReturn (object sender, CommandProcessEventArgs args)
	{
		//Command cmd = args.Cmd;
		
	}

	void boot (ref Command cmd)
	{
		
		//Debug.Log("WebKit version: " + cmd.GetSParam (0));
		//Debug.Log("Qt version: " + cmd.GetSParam (1));
		
		ProductKey = cmd.GetSParam (2);
		
		int error = cmd.iParams [0];
		
		ProductTrial = cmd.iParams [3] == 1 ? true : false;
		
		if (error != 0) {
			Debug.Log ("Error starting uWebKit");
			
		}
		
		if (cmd.iParams [2] == 2) {
			//need to activate
			#if UNITY_EDITOR
			if (!EditorApplication.currentScene.Contains ("UWKActivationScene")) {
				EditorUtility.DisplayDialog ("uWebKit Activation Required", "Please select uWebKit/Activate from the Editor menu", "Ok");
				EditorApplication.ExecuteMenuItem ("Edit/Play");
			}
			#endif
		}
		
		if (cmd.iParams [1] == 0)
			StandardVersion = true;
		
		processUp = true;
		
		if (restarting) {
			restarting = false;
			UWKView.RestartAllViews ();
		}
		
		booted = true;
		
		#if UNITY_EDITOR
		if (StandardVersion)
			if (GetNumViews() > 1)
			{
				EditorUtility.DisplayDialog ("uWebKit Standard Version", "uWebKit Pro required for multiple web views", "Ok");
			}
		#endif
		
		
		// enable all views created before boot
		foreach (UWKView view in viewLookup.Values)
			view.enabled = true;
		
		cmd = Command.NewCommand ("ALWP", AllowJavascriptPopups ? 1 : 0);
		cmd.Post ();
		
		// set proxy if any
		if (UWKConfig.ProxyEnabled) {
			if (UWKConfig.ProxyHostname.Length > 0) {
				cmd = Command.NewCommand ("PRXY");
				cmd.numSParams = 1;
				cmd.SetSParam (0, UWKConfig.ProxyHostname);
				
				cmd.numIParams = 1;
				cmd.iParams [0] = UWKConfig.ProxyPort;
				
				if (UWKConfig.ProxyUsername.Length > 0) {
					cmd.SetSParam (cmd.numSParams++, UWKConfig.ProxyUsername);
					
					if (UWKConfig.ProxyPassword.Length > 0) {
						cmd.SetSParam (cmd.numSParams++, UWKConfig.ProxyPassword);
					}
				}
				
				// post proxy info
				cmd.Post ();
			}
		}
		
		// set auth if any
		if (UWKConfig.AuthEnabled && UWKConfig.AuthUsername.Length > 0 && UWKConfig.AuthPassword.Length > 0 ) {
			
			cmd = Command.NewCommand ("AUTH");
			cmd.numSParams = 2;
			cmd.SetSParam (0, UWKConfig.AuthUsername);
			cmd.SetSParam (1, UWKConfig.AuthPassword);
			cmd.Post ();		
		}
		
	}

	void processInbound (object sender, CommandProcessEventArgs args)
	{
		Command cmd = args.Cmd;
		
		if (cmd.fourcc == "PRUP") {
			Plugin.UWK_InitProcess ();
		}
		
		// save cookies
		if (cmd.fourcc == "LDFN") {
			Command ncmd = Command.NewCommand ("SVCK");
			ncmd.Post ();
		}
		
		if (cmd.fourcc == "BOOT") {
			boot (ref cmd);
		}
		
		if (cmd.fourcc == "JLOG" && ShowJavascriptErrors) {
			
			string message = Plugin.GetString (cmd.iParams [0], cmd.iParams [1]);
			int lineNumber = cmd.iParams [2];
			string sourceId = Plugin.GetString (cmd.iParams [3], cmd.iParams [4]);
			
			Debug.Log ("Javascript: " + lineNumber + " : " + sourceId + " :  " + message);
		}
		
		// activation, we're still interested as the activation script may not be running
		if (cmd.fourcc == "ACTR") {
			
			processACTR (cmd);
			
		}
		
		if (cmd.fourcc == "POPU") {
			
			string name = Plugin.GetString (cmd.iParams [0], cmd.iParams [1]);
			
			int width = cmd.iParams [2];
			int height = cmd.iParams [3];
			
			CreateJSPopup (name, width, height);
			
		}
		
		if (cmd.fourcc == "POPC") {
			
			string name = Plugin.GetString (cmd.iParams [0], cmd.iParams [1]);
			
			if (!popups.ContainsKey (name)) {
				// this can happen if we close Unity side, we'll still get the call from the close slot
				// we could remove the slot native side to avoid this
				//Debug.LogWarning("Warning: CLoseJSPopupRequested called on missing popup");
				
			} else
				CloseJSPopupRequested (popups [name]);
			
		}
		
		
	}

	void init ()
	{
		if (!Plugin.Init (log)) {
			RuntimeError = true;
			if (!Application.isEditor) {
				Debug.LogError ("\n\n*** ERROR ***\nuWebKit Plugin failed to load.\n\nUnity Pro is required.\n\n");
				Application.Quit ();
			} else {
				#if UNITY_EDITOR
				EditorUtility.DisplayDialog ("uWebKit Plugin Error", "uWebKit Plugin failed to load.\n\nUnity Pro is required.\n", "Ok");
				EditorApplication.ExecuteMenuItem ("Edit/Play");
				#endif
				return;
			}
		}
		
		// listen in on inbound commands
		Plugin.ProcessInbound += processInbound;
		// listen to return commands
		Plugin.ProcessReturn += processReturn;
		
		process = new Process ();
		bool result = process.Go ();
		if (!result) {
			// likely the result of not injecting the player
			ProcessError = true;
		} else if (UWKProcessStarted != null) {
			UWKProcessStarted ();
		}

		
		
	}

	void update ()
	{
		if (!RuntimeError)
			Plugin.Update ();
	}

	void shutdown ()
	{
		if (!RuntimeError) {
			process.Stop ();
			Plugin.Shutdown ();
		}
		
	}

	static void log (string message)
	{
		Debug.Log (message);
	}
	
	/// <summary>
	/// Gets a value indicating whether the web process in running.
	/// </summary>
	public static bool ProcessUp {

		get { return processUp; }
	}

	bool restarting = false;
	static bool processUp = false;
	public static bool DestroyViewsOnLevelLoad = false;

	void OnLevelWasLoaded (int level)
	{
		
		if (DestroyViewsOnLevelLoad) {
			foreach (UWKView view in new List<UWKView> (viewLookup.Values)) {
				RemoveView (view);
			}
			
			viewLookup = new Dictionary<string, UWKView> ();
		}
		
		DestroyViewsOnLevelLoad = false;
	}

	void processACTR (Command cmd)
	{
		int code = cmd.iParams [0];
		
		
		
		if (code == 0 || code == 8) {
			
			#if UNITY_EDITOR
			if (!activation) {
				EditorUtility.DisplayDialog ("uWebKit Activation Error", "Please reactivate with a valid key or contact sales@uwebkit.com for more information", "Ok");
				EditorApplication.ExecuteMenuItem ("Edit/Play");
			}
			#endif
		}
		
		if (code == 1) {
			if (cmd.numIParams == 2)
				Debug.Log ("Trial has " + cmd.iParams [1] + " days left");
		}
		
		// act1 or act2
		if (code == 2 || code == 3 || code == 6) {
			//Debug.Log("Activation Passed: " + code);
		}
		
		
		if (code == 4) {
			#if UNITY_EDITOR
			if (!activation) {
				EditorUtility.DisplayDialog ("uWebKit Activation", "Activation Count Exceeded.  Please contact sales@uwebkit.com for more information", "Ok");
				EditorApplication.ExecuteMenuItem ("Edit/Play");
			}
			#endif
			
		}
		
		if (code == 5) {
			#if UNITY_EDITOR
			if (!activation) {
				EditorUtility.DisplayDialog ("uWebKit Trial Expired", "This trial version of uWebKit has expired.\n\nPlease select uWebKit/Activation from the Editor Menu to enter your uWebKit Standard or Pro key", "Ok");
				EditorApplication.ExecuteMenuItem ("Edit/Play");
			}
			#endif
		}
		
	}

	void OnGUI ()
	{
		if (ProcessError) {
			GUI.Label (new Rect (0, 0, Screen.width, Screen.height), "uWebKit process missing, have you injected the player?\n\n" + "Select uWebKit/Inject Player from the Unity Editor Window");
		}
	}

	Process process;
	public static bool ProcessError = false;
	public static bool RuntimeError = false;
	public static bool imeEnabled = false;
	static UWKCore sInstance;
	static Dictionary<string, UWKView> viewLookup = new Dictionary<string, UWKView> ();
	public static bool StandardVersion = false;
	public static bool ProductTrial = false;
	public static bool ActivationRequired = false;
	public static string ProductKey = "";
	static bool booted = false;
	
	static float unresponsiveTime = 0.0f;

	// set in UWKConfig.cs
	private static bool ShowJavascriptErrors = true;
	private static bool AllowJavascriptPopups = true;

	static UWKCore ()
	{
		ShowJavascriptErrors = UWKConfig.ShowJavascriptErrors;
		AllowJavascriptPopups = UWKConfig.AllowJavascriptPopups;
	}


	#if UNITY_EDITOR
	static bool activation = false;
	#endif
	#endregion
	
	public static UWKProcessStartedDelegate UWKProcessStarted;
	public static UWKProcessZombifiedDelegate UWKProcessZombified;
	public static UWKProcessWillRestartDelegate UWKProcessWillRestart;
	public static UWKProcessRestartedDelegate UWKProcessRestarted;	
	
}

public delegate void UWKProcessStartedDelegate ();

public delegate void UWKProcessZombifiedDelegate ();

public delegate void UWKProcessWillRestartDelegate ();

public delegate void UWKProcessRestartedDelegate ();

