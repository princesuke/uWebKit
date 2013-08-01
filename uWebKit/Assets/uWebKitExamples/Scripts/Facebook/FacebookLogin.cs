using UnityEngine;
using System.Collections;
using System;

using UWK;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Example Facebook Login which integrates with a FB App
/// Also a good example of cookie session data being used
/// as user won't be prompted if they choose to stay logged in
/// </summary>
public class FacebookLogin : MonoBehaviour
{

	// The name of the login webview
	string viewName = "FriendCubeView";

	// the view itself
	UWKView view;

	// the view will be valid once it has been created
	bool valid = false;

	// used to control visbility
	bool visible = true;

	// the dimensions and position of the login view
	Rect windowRect;

	// the width and height of the login view
	int width, height;

	// Used to catch when mouse has changed so we avoid duplicate updates
	int lastMouseX = -1;
	int lastMouseY = -1;

	// Used to catch the app token and kickstart the demo proper
	bool go = false;

	void Awake ()
	{
		// ensure Core is up
		UWKCore.Init ();
	}

	// Use this for initialization
	void Start ()
	{
		width = height = 512;
		windowRect = new Rect (0, 0, width, height);
		center ();
		
		// Create the View
		view = UWKCore.CreateView (viewName, width, height, false);
		
		// Add delegates to handle when the view had been created or the URL has been changed
		view.ViewCreated += viewCreated;
		view.URLChanged += urlChanged;
		
	}

	// Update is called once per frame
	void Update ()
	{
		
		// Facebook Connect requires HTTPS, which is a Pro feature
		if (UWKCore.StandardVersion) {
			
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog ("uWebKit Pro Required", "Facebook Connect requires HTTPS, which is a Pro feature", "Ok");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			#endif
			
			return;
		}
		
		if (width != view.Width)
		{
			view.Width = width;
			view.Height = height;
		}
		
		
		// can happen on a texture resize
		if (renderer != null)
			if (renderer.material.mainTexture == null)
				renderer.material.mainTexture = view.MainTexture;
		
		if (guiTexture != null)
			if (guiTexture.texture == null)
				guiTexture.texture = view.MainTexture;

		
		if (!visible || !valid)
			return;
		
		// Handle mouse input
		
		int x = (int)windowRect.x;
		int y = (int)windowRect.y;
		
		Vector3 mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		
		if (mousePos.x != lastMouseX || mousePos.y != lastMouseY) {
			lastMouseX = (int)mousePos.x;
			lastMouseY = (int)mousePos.y;
			view.SetMousePos (lastMouseX - x, lastMouseY - y);
		}
		
		for (int i = 0; i < 3; i++) {
			if (Input.GetMouseButtonDown (i)) {
				
				view.OnMouseButtonDown (lastMouseX - x, lastMouseY - y, i);
			}
			
			if (Input.GetMouseButtonUp (i)) {
				
				view.OnMouseButtonUp (lastMouseX - x, lastMouseY - y, i);
			}
		}
		
	}

	void windowFunction (int windowID)
	{
		
		// Simple window Function that allows us to easily move the login window around
		GUI.color = Color.white;
		
		GUI.DragWindow (new Rect (0, 0, width, 28));
		
#if UNITY_EDITOR || !UNITY_IPHONE		
		view.DrawGUI (0, 0);
#else
		Rect mrect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height);
		view.MobileRect = mrect;
#endif
		
		
	}


	void OnGUI ()
	{
		if (!valid || !visible)
			return;
		
		windowRect = GUI.Window (1, windowRect, windowFunction, "");
		
		// Handle keyboard input
		Event e = Event.current;
		
		if (e.isKey) {
			view.ProcessKey (e);
			e.Use ();
		}
		
	}

	void center ()
	{
		windowRect.x = Screen.width / 2 - width / 2;
		windowRect.y = Screen.height / 2 - height / 2;
	}
	
	void loadFinished (UWKView view)
	{
#if UNITY_IPHONE && !UNITY_EDITOR		
		view.captureTexture();
#endif
	}

	/// <summary>
	/// Delegate to handle URL changes on the view, this is used to catch the access_token once the
	/// user has logged into FB and accepted permissions
	/// </summary>
	void urlChanged (UWKView view, string url)
	{
		if (go == false && url.Contains ("access_token=")) {
			
			go = true;
			
			int index = url.IndexOf ("access_token=") + 13;
			
			// store the access token
			FacebookAPI.AccessToken = url.Substring (index, url.IndexOf ("&", index) - index);
			
			// Resize the view dynamically as it is mapped to the plane in this example
			// and we want a larger view, this will be caught in the Update
			width = 1024;
			height = 1024;
			windowRect = new Rect (0, 0, width, height);
			center ();
			
			// deregister our delegate as we're not interested anymore
			view.URLChanged -= urlChanged;
			
			view.LoadFinished += loadFinished;
			
			// load the facebook home page (using https)
			view.LoadURL ("https://www.facebook.com");
			
			// make the login screen invisible
			visible = false;
			view.Hide();
			
			// notify attached components that we have received the access token!
			gameObject.SendMessage ("OnAccessTokenReceived", view, SendMessageOptions.DontRequireReceiver);
			
		}
		
		
	}

	// Delegate to handle view creation, loads the FB login request
	void viewCreated (UWKView view)
	{
		valid = true;
		
		// App Login Dialog request, note that we use display=popup
		string loginURL = String.Format ("https://www.facebook.com/dialog/oauth/?client_id={0}&redirect_uri={1}&response_type=token&display=popup", FacebookAPI.AppId, FacebookAPI.RedirectUri);
		
		view.LoadURL (loginURL);
		
		view.ViewCreated -= viewCreated;
		
		view.Show ();		
		
	}
	
}
