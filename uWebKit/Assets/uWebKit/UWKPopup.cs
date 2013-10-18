/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using UWK;
using System;

/// <summary>
/// Minimal WebPopup using uWebKit and Unity GUI (uWebKit Pro)
/// </summary> 
public class UWKPopup : MonoBehaviour
{

	// once the WebView has been created we'll be valid
	public bool Valid;

	// the view itself
	public UWKView View;

	// position and dimensions of View
	public float X;
	public float Y;

	public int Width = 1024;
	public int Height = 600;

	// the view area of the browser
	Rect windowRect;

	public bool Visible = true;
	
	int _currentWidth;
	int _currentHeight;
	
	int toolbarHeight = 24;

	void Awake ()
	{
		// ensure Core is up
		UWKCore.Init (false);
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
		
		X = windowRect.x = v.x;
		Y = windowRect.y = v.y;
	}


	// Use this for initialization
	void Start ()
	{
		_currentWidth = Width = View.Width;
		_currentHeight = Height = View.Height;
		
		windowRect = new Rect (X, Y, Width + 8, Height + 8 + toolbarHeight);
		
		Center ();
		
		
		Valid = true;
		
		View.Show (true);
		
		
	}


	// Update is called once per frame
	void Update ()
	{
		
		if (!Valid)
			return;
		
		if (!View.Visible)
			return;
		
		// Handle mouse input
		
		#if UNITY_EDITOR || !UNITY_IPHONE
		
		int x = (int) windowRect.x + 4;
		int y = (int) windowRect.y + 4 + toolbarHeight;
		
		View.OnGUIMouse(x, y);
		
		#endif
		
		
	}

	// Main Window function of browser, used to draw GUI
	void windowFunction (int windowID)
	{	
		
		if (GUI.Button (new Rect (windowRect.width - 28, 4,24,24), "X"))
		{
			Close();
			return;
		}
		
 		GUI.DragWindow(new Rect(0, 0, Width, toolbarHeight));
		
		
		View.DrawGUI (4, 4 + toolbarHeight, windowRect);
			
	}
	
	void Close ()
	{
		Valid = false;
		UWKCore.ClosePopup(this);
		
	}
	
	void OnGUI ()
	{
		
		if (!Valid)
			return;
		
		if (!Visible)
			return;
		
		GUI.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		
		windowRect = GUI.Window (View.windowId, windowRect, windowFunction, View.URL);
		
		// bring to front, popup is always front
		View.BringToFront();
		
		// Handle keyboard input
		Event e = Event.current;
		
		if (GUIUtility.keyboardControl == 0)
			if (e.isKey) {
				View.ProcessKey (e);
			}
		
		if (e.isKey)
			if (e.keyCode == KeyCode.Tab || e.character == '\t')
				e.Use ();
		
		if (Width != _currentWidth || Height != _currentHeight) {
			
			View.Height = Height;
			View.Width = Width;
			
			// can be clamped so set back
			_currentHeight = Height = View.Height;
			_currentWidth = Width = View.Width;
			
			windowRect.width = Width + 8;
			windowRect.height = Height + 8;
		}
		
	}
	
}
