/******************************************
  * uWebKit 
  * (c) 2013 The Engine Co. LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UWK;

using Object = UnityEngine.Object;

#region Events
/// <summary>
/// ViewCreatedDelegate - Event fired when the view has been created 
/// and is ready for use
/// </summary>
public delegate void ViewCreatedDelegate (UWKView view);

/// <summary>
/// URLChangedDelegate - Event fired when the URL has been changed
/// either by user input or due to a page redirect
/// </summary>
public delegate void URLChangedDelegate (UWKView view,string url);

/// <summary>
///  TitleChangedDelegate - Event fired when the title of a web page has changed
/// </summary>
public delegate void TitleChangedDelegate (UWKView view,string title);

/// <summary>
/// LoadFinishedDelegate - Event fired once the page has been fully loaded
/// </summary>
public delegate void LoadFinishedDelegate (UWKView view);

/// <summary>
/// LoadProgressDelegate - Event fired as the page loads to show progress
/// </summary>
public delegate void LoadProgressDelegate (int progress);

#endregion

/// <summary>
/// UWKView encapsulates a WebKit WebView and is used to view and interact with the view's content.  
/// </summary>
public class UWKView : MonoBehaviour
{

	#region Inspector Fields

	/// <summary>
	/// The View's unique name as registered with the UWKCore
	/// </summary>
	public string Name;

	/// <summary>
	/// The current URL loaded into the UWKView, note that the LoadHTML and LoadURL methods should be 
	/// used for changing the view's URL
	/// </summary>
	public string URL;

	/// <summary>
	/// The active page's title string
	/// </summary>
	public string Title;

	/// <summary>
	/// The active page's icon (as seen in browser tab, etc)
	/// </summary>
	public Texture2D Icon;

	/// <summary>
	/// Whether this view is a Javascript popup
	/// </summary>
	public bool JSPopup = false;
	public bool TextInputActive = false;
	public Rect TextInputRect;
	public string TextInputType = "";

	#endregion

	#region Properties

	/// <summary>
	/// A convenience property for quick access to the UWKView's backbuffer texture
	/// Note that when using SmartRects, the backbuffer may have regions that are out of date
	/// and require the subregion textures to display properly, see UWKTextureSet
	/// </summary>
	public Texture2D MainTexture {
		get {
			if (TSet != null)
				return TSet.BackBuffer;
			
			return null;
		}
	}

	/// <summary>
	/// Sets whether or not the UWKView uses SmartRects (dirty rectangles).  SmartRects
	/// can greatly speed updates of large pages which feature small areas that are changing
	/// whilst most of the page remains static.  SmartRects are designed to work with 2D web content
	/// should be disabled when running web content mapped in 3D or if transparency is required
	/// SmartRects can be toggled on the fly
	/// </summary>
	public bool SmartRects {
		get { return _smartRects; }
		set {
			if (_smartRects != value) {
				_smartRects = value;
				_smartRectsDirty = true;
			}
		}
	}

	/// <summary>
	/// Controls whether the view is drawn
	/// </summary>
	public bool Visible {
		get { return _visible; }
		set {
			
			if (value == _visible)
				return;
			
			_visible = value;
			_visibleDirty = true;				
		}
	}

	/// <summary>
	/// Controls whether the view is alpha masked (body { background-color: transparent;} 
	/// </summary>
	public bool AlphaMask {
		get { return _alphaMask; }
		set {
			
			if (valid) {
				
				if (value == _alphaMask)
					return;
				
				_alphaMask = value;
				
				Command cmd = Command.NewCommand ("SMSK", Name, value ? 1 : 0);
				cmd.Post ();
			}
		}
	}


	/// <summary>
	/// A UWKView is invalid until the WebCore creates and initializes it.
	/// uWebKit uses an event pattern to notify the view once it is valid.
	/// Calls that interact with the view should be avoided until the view has
	/// been validated.
	/// </summary>
	public bool Valid {

		get { return valid; }

		set { valid = value; }
	}

	/// <summary>
	/// The Width of the WebView, note that the view is clamped to >= 128 and <=2048
	/// This may be increased in a future version of uWebKit, however as we need to internally resize
	/// to a pow2 texture, the next jump would be 2048
	/// </summary>
	public int Width {
		get { return width; }
		set {
			if (value > 2048)
				value = 2048;
			if (value < 128)
				value = 128;
			if (width != value) {
				width = value;
				sizeDirty = true;
			}
		}
	}

	/// <summary>
	/// The Height of the WebView, see notes for the Width property
	/// </summary>
	public int Height {
		get { return height; }
		set {
			if (value > 2048)
				value = 2048;
			if (value < 128)
				value = 128;
			if (height != value) {
				height = value;
				sizeDirty = true;
			}
		}
	}

	/// <summary>
	/// Controls whether the view is active, deactive views save CPU as they are not updated in native code
	/// and thus their texture data also doesn't need to be updated, this can be used to cull web content 
	/// for instance when a 3D object becomes invisible
	/// </summary>
	public bool Active {
		get { return active; }

		set {
			if (active != value) {
				activeDirty = true;
			}
			
			active = value;
		}
	}
	
	/// <summary>
	/// Gets or sets the transparency from 0.0 invisible to 1.0 fully opaque
	/// </summary>
	public float Transparency {
		get { return _transparency; }

		set {
			
			if (_transparency != value) {
				
				if (valid) {
					Command cmd = Command.NewCommand ("TRAN", Name, (int)(_transparency * 100));
					cmd.Post ();
				}
			}
			
			_transparency = value;
		}
	}
	
	/// <summary>
	/// Gets or sets the rectangle where the view is drawn on iOS
	/// </summary>
	public Rect MobileRect {
		get { return _mobileRect; }
		set {
			if (value != _mobileRect) {
				_mobileRect = value;
				_mobileRectDirty = true;
			}
		}
	}

	#endregion
	
	/// <summary>
	/// Gets or sets the window identifier which is used to control which view 
	/// receives mouse and keyboard input (see BringToFront)
	/// </summary>
	public int windowId {
		get { return _windowId; }
		set { _windowId = value; }
	}


	/// <summary>
	/// The TextureSet tied to this view, see UWKTextureSet.cs
	/// </summary>
	public TextureSet TSet;

	/// <summary>
	/// ContentWidth and ContentHeight are read only variables based on the dimensions of the content
	/// loaded into the WebView, if Width < ContentWidth or Height < ContentHeight the web view will 
	/// contain scrollbars
	/// </summary>
	public int ContentWidth;
	public int ContentHeight;

	/// <summary>
	/// If you would like the UWKView to resize to the actual size of the page contents,
	/// set ResizeToContents to true
	/// </summary>
	public bool ResizeToContents = false;

	#region Private Fields

	// Internal cache variables to minimize state changes and backing variables for properties
	bool sizeDirty = false;
	bool activeDirty = false;
	bool _smartRects = true;
	bool _smartRectsDirty = true;
	bool valid = false;
	int width = 512;
	int height = 512;
	new bool active = true;
	string _imeText = "";
	bool _mobileRectDirty;
	Rect _mobileRect;
	bool _alphaMask = false;
	float _transparency = 1.0f;
	bool _visible = true;
	bool _visibleDirty = true;
	static int sWindowId = 10000;
	public static int sFrontWindow = 10000;
	int _windowId = 0;
	
	Vector2 mousePos = new Vector2(-1000, -1000);

	#endregion

	/// <summary>
	/// Standard MonoBehavior Start method, creates the TextureSet and internal WebView
	/// note that WebViews are uniquely named and requests for a WebView with an identical view name
	/// will reuse that view
	/// </summary>
	void Start ()
	{
		TSet = new TextureSet ();
		TSet.Init (Width, Height, SmartRects);
		
		if (_windowId == 0) {
			_windowId = sWindowId;
			sWindowId++;
			
			// defaults to front
			BringToFront(false);
		}
		
		if (!JSPopup) {
			Command cmd = Command.NewCommand ("CRVW", Name, Width, Height, SmartRects ? 1 : 0);			
			cmd.Post ().Process += processCRVW;
		}
		
		// listen in on inbound commands
		Plugin.ProcessInbound += processInbound;
		
		_mobileRect = new Rect (0, 0, Width, Height);
		_mobileRectDirty = false;
		
		if (JSPopup)
			validate ();
		
	}

	// Update is called once per frame
	void Update ()
	{
		if (!Valid)
			return;
		
		if (sizeDirty) {
			// need to reuse this as with mpos, etc
			Command cmd = Command.NewCommand ("RSIZ", Name, width, height);
			cmd.Post ().Process += processResize;
			sizeDirty = false;
		}
		
		if (activeDirty) {
			Command cmd = Command.NewCommand ("SACT", Name, active ? 1 : 0);
			cmd.Post ();
			activeDirty = false;
		}
		
		if (_mobileRectDirty) {
			Command cmd = Command.NewCommand ("MRCT", Name, _mobileRect.x, _mobileRect.y, _mobileRect.width, _mobileRect.height);
			cmd.Post ();
			_mobileRectDirty = false;
		}
		
		if (_visibleDirty) {
			_visibleDirty = false;
			Command cmd = Command.NewCommand ("SHOW", Name, _visible ? 1 : 0);
			cmd.Post ();
		}
		
		if (_smartRectsDirty)
		{
			Command cmd = Command.NewCommand ("SSMR", Name);
			cmd.iParams [0] = _smartRects ? 1 : 0;
			cmd.numIParams = 1;
			cmd.Post ();
			_smartRectsDirty = false;
		}

		
	}

	public void DrawGUI (int x, int y)
	{
		
		DrawGUI (x, y, -1, -1);
		
	}

	public void DrawGUI (int x, int y, Rect r)
	{
		#if !UNITY_EDITOR && UNITY_IPHONE
		x += (int)r.x;
		y += (int)r.y;
		#endif
		DrawGUI (x, y);
		
	}

	/// <summary>
	/// Main 2D drawing method for a UWKView, draws the view at the given X, Y coords with width and height dimensions
	/// </summary>
	public void DrawGUI (int x, int y, int width, int height)
	{
		
		if (!valid || !Visible)
			return;
		
		if (width <= 0) {
			width = Width;
			height = Height;
		}
		
		#if !UNITY_EDITOR && UNITY_IPHONE
		MobileRect = new Rect (x, y, width, height);
		return;
		#endif
		
		int bw = TSet.BackBuffer.width;
		int bh = TSet.BackBuffer.height;
		
		Rect br = new Rect (x, y, bw, bh);
		GUI.DrawTexture (br, TSet.BackBuffer);
		
		if (!SmartRects)
			return;
		
		int activeMip0 = 0;
		int activeMip1 = 0;
		
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 2; j++) {
				SubBuffer s = TSet.SubBuffers [i, j];
				if (!s.Active)
					continue;
				
				Rect r = new Rect (x + s.X, y + s.Y, s.Width, s.Height);
				GUI.DrawTexture (r, s.Texture);
				
				if (i == 0)
					activeMip0++;
				else
					activeMip1++;
				
			}
		
		
		if (UWKCore.imeEnabled)
			DrawTextIME (x, y);
		//Debug.Log("Active SubTex Mip0: " + activeMip0 + ", Active SubTrex Mip1: " + activeMip1);
		
	}

	static List<int> sPopupIdStack = new List<int> ();
	
	public void BringToFront ()
	{
		BringToFront(true);
	}
	/// <summary>
	/// Brings the view to the top of the stack, the top view receives mouse and keyboard input 
	/// </summary>
	public void BringToFront (bool inGUI)
	{
		if (inGUI)
		{
			GUI.BringWindowToFront (_windowId);
			GUI.FocusWindow (_windowId);
		}
		
		if (sPopupIdStack.Count > 0)
		if (sPopupIdStack [0] == _windowId)
		{
			sFrontWindow = _windowId;
			return;
		}
		
		if (sPopupIdStack.Contains (_windowId)) {
			sPopupIdStack.Remove (_windowId);
			sPopupIdStack.Insert (0, _windowId);
		}
		else
			sPopupIdStack.Insert (0, _windowId);
		
		sFrontWindow = _windowId;
		
	}
	
	/// <summary>
	/// Pushes this view to the back of the view stack
	/// </summary>
	public void PushToBack ()
	{
		if (sPopupIdStack.Contains (_windowId)) {
			sPopupIdStack.Remove (_windowId);
			sPopupIdStack.Add (_windowId);
		}
		if (sPopupIdStack.Count != 0) {
			sFrontWindow = sPopupIdStack [0];
			GUI.BringWindowToFront (sFrontWindow);
			GUI.FocusWindow (sFrontWindow);
		}
	}

	void simulateKey (KeyCode keyCode, char ascii)
	{
		Command cmd;
		
		cmd = Command.NewCommand ("KEYD", Name);
		cmd.iParams [0] = (int)UWKKeys.KeyMap [keyCode];
		cmd.iParams [1] = 0;
		// mod
		cmd.numIParams = 2;
		cmd.SetSParam (1, ascii.ToString ());
		cmd.numSParams = 2;
		cmd.Post ();
		cmd = Command.NewCommand ("KEYU", Name);
		cmd.iParams [0] = (int)UWKKeys.KeyMap [keyCode];
		cmd.iParams [1] = 0;
		// mod
		cmd.numIParams = 2;
		cmd.SetSParam (1, ascii.ToString ());
		cmd.numSParams = 2;
		cmd.Post ();
	}
	
	/// <summary>
	/// Draws the text IME for Chinese, Japanese, Korean languages
	/// </summary>
	public void DrawTextIME (int x, int y)
	{
		if (TextInputActive) {
			bool enter = false;
			
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
				Event.current.Use ();
				enter = true;
			}
			
			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Tab)) {
				simulateKey (KeyCode.Tab, '\t');
				return;
			}
			
			
			GUI.SetNextControlName ("IMETextField");
			Rect t = new Rect (x + TextInputRect.x, y + TextInputRect.y, TextInputRect.width, TextInputRect.height);
			
			string currentIME = "";
			
			if (TextInputType != "password")
				currentIME = GUI.TextField (t, _imeText);
			else
				currentIME = GUI.PasswordField (t, _imeText, "*" [0]);
			
			if (currentIME != _imeText) {
				_imeText = currentIME;
				int size = 0;
				int i = Plugin.AllocateString (_imeText, ref size);
				
				Command cmd = Command.NewCommand ("IMES", Name, i, size);
				cmd.Post ();
			}
			
			GUI.FocusControl ("IMETextField");
			
			if (enter) {
				if (!String.IsNullOrEmpty (_imeText)) {
					Command cmd;
					
					int size = 0;
					int i = Plugin.AllocateString (_imeText, ref size);
					
					cmd = Command.NewCommand ("IMES", Name, i, size);
					cmd.Post ();
					
					simulateKey (KeyCode.Return, '\r');
				}
				
				_imeText = "";
				
				TextInputActive = false;
				
			}
		}
		
	}

	/// <summary>
	/// Remove a view and deregister it from the WebCore
	/// </summary>
	public void Remove ()
	{
		if (sPopupIdStack.Contains (_windowId)) {
			sPopupIdStack.Remove (_windowId);
		}
		
		if (sPopupIdStack.Count > 0) {
			sFrontWindow = sPopupIdStack [0];
		}
		
		
		Valid = false;
		
		Plugin.ProcessInbound -= processInbound;
		
		Command cmd = Command.NewCommand ("RMVW", Name);
		
		cmd.Post ().Process += processRMVW;
		
	}
	
	/// <summary>
	/// Hides the view
	/// </summary>
	public void Hide ()
	{
		Show (false);
	}
	
	/// <summary>
	/// Show the view
	/// </summary>
	public void Show ()
	{
		Show (true);
	}
	
	/// <summary>
	/// Hide or show the specified view depending on bool argument
	/// </summary>
	public void Show (bool show)
	{
		Visible = show;
	}


	/// <summary>
	/// Default handler which consumes return value, if you specify your own return value handler you much consume 
	/// the return string with Plugin.GetString as below, otherwise you may run out of buffer allocations
	/// </summary>
	void defaultEvalResultHandler (object sender, CommandProcessEventArgs args)
	{
		// ensure we consume return value
		Plugin.GetString (args.Cmd.iParams [0], args.Cmd.iParams [1]);
	}

	/// <summary>
	/// Evaluates the given Javascript on the loaded page.
	/// if you specify your own return value handler you must consume 
	/// the return string with Plugin.GetString as shown in the evalResult above, otherwise you may run out of buffer allocations
	/// </summary>
	public void EvaluateJavaScript (string script, CommandProcessEventHandler resultHandler)
	{
		int size = 0;
		int i = Plugin.AllocateString (script, ref size);
		
		Command cmd = Command.NewCommand ("EVJS", Name, i, size);
		CommandHandler handler = cmd.Post ();
		if (handler != null) {
			if (resultHandler != null)
				handler.Process += resultHandler;
			else
				handler.Process += defaultEvalResultHandler;
		}
	}
	
	public void EvaluateJavaScript (string script)
	{
		EvaluateJavaScript (script, null);
	}

	#region Input

	// used to avoid duplicate mouse updates
	int lastMouseX = -1;
	int lastMouseY = -1;
	
	/// <summary>
	/// Handles GUI mouse input including position, buttons, and scroll wheel for 
	/// 2D UnityGUI uWebKit integration
	/// </summary>
	public void OnGUIMouse (int xOffset, int yOffset)
	{
		if (sFrontWindow != _windowId)
			return;
		
		Vector3 mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		
		if (mousePos.x != lastMouseX || mousePos.y != lastMouseY) {
			lastMouseX = (int)mousePos.x;
			lastMouseY = (int)mousePos.y;
			SetMousePos (lastMouseX - xOffset, lastMouseY - yOffset);
		}
		
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		
		if (scroll != 0.0f) {
			OnScrollWheel (scroll);
		}
		
		for (int i = 0; i < 3; i++) {
			if (Input.GetMouseButtonDown (i)) {
				
				OnMouseButtonDown (lastMouseX - xOffset, lastMouseY - yOffset, i);
			}
			
			if (Input.GetMouseButtonUp (i)) {
				
				OnMouseButtonUp (lastMouseX - xOffset, lastMouseY - yOffset, i);
				
			}
			
		}
		
	}
	
	public void OnGUIMouse (float xOffset, float yOffset)
	{
		OnGUIMouse ((int)xOffset, (int)yOffset);
	}

	public void OnMouseButtonDown (int x, int y, int button)
	{
		if (sFrontWindow != _windowId)
			return;
		
		Command cmd = Command.NewCommand ("MPRS", Name, x, y, button);
		cmd.Post ();
	}

	public void OnMouseButtonUp (int x, int y, int button)
	{
		if (sFrontWindow != _windowId)
			return;
		
		Command cmd = Command.NewCommand ("MREL", Name, x, y, button);
		cmd.Post ();
	}

	public void OnScrollWheel (float scroll)
	{
		if (sFrontWindow != _windowId)
			return;
		
		Command cmd = Command.NewCommand ("MSCL", Name, (int)(scroll * 400));
		cmd.Post ();
	}

	public void SetMousePos (int x, int y)
	{
		if (sFrontWindow != _windowId)
			return;
		
		if (mousePos.x != x || mousePos.y != y) {
			mousePos.x = x;
			mousePos.y = y;
			Command cmd = Command.NewCommand ("MPOS", Name, x, y);
			cmd.Post ();
		}
	}
	
	/// <summary>
	/// Processes the key event
	/// </summary>
	public void ProcessKey (Event e)
	{
		Command cmd;
		
		// key event only consumed by front window
		if (sFrontWindow != _windowId)
			return;		
		
		if (TextInputActive)
			return;
		
		if (e.isKey) {
			
			int mod = 0;
			
			if (e.command) {
				mod |= 1;
			}
			
			if (e.alt)
				mod |= 8;
			
			if (e.control)
				mod |= 4;
			
			if (e.shift && mod != 0)
				mod |= 2;
			
			if (e.type == EventType.KeyDown) {
				
				cmd = Command.NewCommand ("KEYD", Name);
				
				if (e.character == 0 && (!UWKKeys.AsciiMap.ContainsKey (e.keyCode) || mod != 0)) {
					
					try {
						cmd.iParams [0] = (int)UWKKeys.KeyMap [e.keyCode];
					} catch (KeyNotFoundException) {
						return;
					}
					
					cmd.iParams [1] = mod;
					cmd.numIParams = 2;
					
				} else if (e.character != 0) {
					cmd.iParams [0] = 0;
					
					if (UWKKeys.KeyMap.ContainsKey (e.keyCode))
						cmd.iParams [0] = (int)UWKKeys.KeyMap [e.keyCode];
					
					cmd.iParams [1] = mod;
					cmd.numIParams = 2;
					
					cmd.SetSParam (1, e.character.ToString ());
					cmd.numSParams = 2;
				}
				
				if (cmd.numSParams == 1 && cmd.iParams[0] == 0)
					return;
				
				// Unity's keyboard handling is really rough
				// reveser engineer the keycode from the character
				if (!e.shift && mod == 0 && cmd.iParams[0] == 0) {
					
					string ascii = cmd.GetSParam(1);
					if (UWKKeys.AsciiMap.ContainsValue(ascii)) {
						
						foreach (KeyValuePair<KeyCode, string> pair in UWKKeys.AsciiMap) {
							
							if (ascii == pair.Value)
							{
								if (UWKKeys.KeyMap.ContainsKey(pair.Key))
									cmd.iParams[0] = (int) UWKKeys.KeyMap[pair.Key];
								
								break;
							}
							
    					}						
						
					}
				}
				
				// We're getting a 16777220 on enter key (in addition to a valid /r), todo, track this better	  	
	        	if (cmd.iParams [0] != 16777220)
					cmd.Post ();
				}
			
			if (e.type == EventType.KeyUp) {
				
				cmd = Command.NewCommand ("KEYU", Name);
				
				if (e.character == 0) {
					
					try {
						cmd.iParams [0] = (int)UWKKeys.KeyMap [e.keyCode];
					} catch (KeyNotFoundException) {
						return;
					}
					
					cmd.iParams [1] = mod;
					cmd.numIParams = 2;
					
				} else {
					cmd.iParams [0] = 0;
					cmd.iParams [1] = mod;
					cmd.numIParams = 2;
					cmd.SetSParam (1, e.character.ToString ());
					cmd.numSParams = 2;
				}
				
				cmd.Post ();
				
			}
			
		}
		
	}

	public void ProcessKeyboard (Event e)
	{
		if (e == null)
			return;
		
		if (GUIUtility.keyboardControl == 0)
		if (e.isKey) {
			ProcessKey (e);
		}
		
		if (e.isKey)
		if (e.keyCode == KeyCode.Tab || e.character == '\t')
			e.Use ();
	}
	
	public void OnWebGUI (int x, int y)	
	{
		OnWebGUI(x, y, -1, -1, 100.0f);
	}
	
	public void OnWebGUI (int x, int y, int width, int height)
	{
	   OnWebGUI(x, y, width, height, 100.0f);
	}
	
	/// <summary>
	/// Handles most GUI tasks of a uWebKit view in a UnityGUI context
	/// this method should be called from the OnGUI method of an associated MonoBehavior
	/// </summary>
	public void OnWebGUI (int x, int y, int width, int height, float transparency)
	{
		if (!Visible || !valid)
			return;
		
		if (width != -1)
			Width = width;
		if (height != -1)
			Height = height;
		
		Transparency = transparency / 100.0f;
		SmartRects = (transparency >= 100.0f && !AlphaMask) ? true : false;
		
		// Handle keyboard input
		Event e = Event.current;
		ProcessKeyboard (e);
		
		#if UNITY_EDITOR || !UNITY_IPHONE
		// Handle mouse input
		OnGUIMouse (x, y);
		#endif
		
		Color c = GUI.color;
		
		GUI.color = new Color (1.0f, 1.0f, 1.0f, Transparency);
		
		DrawGUI (x, y, Width, Height);
			
		GUI.color = c;
	}	

	#endregion

	#region Navigation
	/// <summary>
	/// Navigates the page forward in navigation history (if possible)
	/// </summary>
	public void Forward ()
	{
		Command cmd = Command.NewCommand ("HFWD", Name);
		cmd.Post ();
	}

	/// <summary>
	/// Navigates the page backwards in navigation history (if possible)
	/// </summary>
	public void Back ()
	{
		Command cmd = Command.NewCommand ("HBAK", Name);
		cmd.Post ();
	}
	
	/// <summary>
	/// Loads the specified text asset which contains HTML
	/// This can be used to load local web content
	/// </summary>
	public void LoadTextAssetHTML( TextAsset text )
	{
		LoadHTML(text.text);
	}

	/// <summary>
	/// Loads the specified HTML string directly in the view, can be used for generating web content on the fly
	/// </summary>
	public void LoadHTML (string HTML)
	{
		int size = 0;
		int i = Plugin.AllocateString (HTML, ref size);
		
		Command cmd = Command.NewCommand ("HTML", Name, i, size);
		cmd.Post ();
	}

	/// <summary>
	/// Navigate the view to the specified URL (http://, file://, etc)
	/// </summary>
	public void LoadURL (string url)
	{
		
		if (url == null || url.Length == 0)		
			return;
		
		
		if (UWKCore.StandardVersion) {
			if (url.ToLower ().Contains ("https://")) {
				Debug.LogError ("uWebKit Pro required for HTTPS protocol");
				LoadHTML (uWebKitStandard.HTTPS_HTML);
				return;
			}
		}
		
		URL = url.Replace (" ", "%20");
		
		if (!URL.Contains ("."))
			URL = "http://www.google.com/search?q=" + url;
		
		if (!URL.Contains ("://"))
			URL = "http://" + url;
		
		Command cmd = Command.NewCommand ("LOAD", Name);
		cmd.SpanSParams (1, URL);
		cmd.Post ();
	}
	#endregion

	#region Events
	public URLChangedDelegate URLChanged;
	public TitleChangedDelegate TitleChanged;
	public ViewCreatedDelegate ViewCreated;
	public LoadFinishedDelegate LoadFinished;
	public LoadProgressDelegate LoadProgress;

	/// <summary>
	/// Processes a resize of the UWKView, which can cause the backing textures to be 
	/// released/allocated.  This is an expensive operation and should be avoided if possible
	/// </summary>
	void processResize (object sender, CommandProcessEventArgs args)
	{
		Command cmd = args.Cmd;
		
		if (cmd.retCode > 0) {
			if (cmd.retCode == 2) {
				TSet.Release ();
				TSet = new TextureSet ();
				TSet.Init (Width, Height, SmartRects);
			} else {
			}
		} else
			Debug.Log ("Error Resizing View: " + Name);
	}

	/// <summary>
	/// Create the Texture2D from the page's icon
	/// </summary>
	void processIcon (ref Command cmd)
	{
		if (cmd.iParams [0] == -1)
			return;
		
		int sz = cmd.iParams [1];
		
		byte[] bytes = new byte[sz];
		
		if (!Plugin.GetBytes (cmd.iParams [0], sz, bytes))
			return;
		
		if (Icon != null)
			Object.DestroyImmediate (Icon);
		
		Icon = new Texture2D (4, 4);
		Icon.LoadImage (bytes);
		
	}

	/// <summary>
	/// Updates the view's texture data in response to a UPVW command from the webcore
	/// </summary>
	public void UpdateView ()
	{
		TSet.Update ();
	}

	/// <summary>
	/// Marks a UWKView as valid and ready for input/rendering
	/// </summary>
	void validate ()
	{
		Valid = true;

	}

	/// <summary>
	/// Inbound command event handler, responds to events generated by the native WebCore
	/// </summary>
	void processInbound (object sender, CommandProcessEventArgs args)
	{
		if (!Valid)
			return;
		
		Command cmd = args.Cmd;
		
		string name = cmd.GetSParam (0);
		
		if (name != Name)
			return;
		
		// Update the view
		if (cmd.fourcc == "UPVW") {
			UpdateView ();
		}
		
		// Update the associated view icon
		if (cmd.fourcc == "ICNC") {
			processIcon (ref cmd);
		}
		
		// Update the ContentSize of the web view
		if (cmd.fourcc == "UPCZ") {
			ContentWidth = cmd.iParams [0];
			ContentHeight = cmd.iParams [1];
			
			if (ResizeToContents && (Width != ContentWidth || Height != ContentHeight)) {
				Width = ContentWidth;
				Height = ContentHeight;
				
				sizeDirty = false;
			}
		}
		
		// Update the view's title
		if (cmd.fourcc == "TITC") {
			
			Title = cmd.GetSParam (1);
			
			if (TitleChanged != null)
				TitleChanged (this, Title);
			
		}
		
		// The progress of the page load to show progress bars, etc
		if (cmd.fourcc == "PROG") {
			
			if (LoadProgress != null)
				LoadProgress (cmd.iParams [0]);
			
		}
		
		
		// The WebView's content has finished loading
		if (cmd.fourcc == "LDFN") {
			
			if (LoadFinished != null)
				LoadFinished (this);
		}
		
		// The WebView's URL has changed either from user input or a redirect
		if (cmd.fourcc == "URLC") {
			string url = "";
			
			url = UWK.Plugin.GetString (cmd.iParams [0], cmd.iParams [1]);
			
			URL = url;
			if (URLChanged != null)
				URLChanged (this, url);
			
		}
		
		
		if (UWKCore.imeEnabled) {
			// ime out
			if (cmd.fourcc == "IMEO") {
				
				TextInputActive = false;
				
			}
			
			// ime in
			if (cmd.fourcc == "IMEI") {
				
				TextInputActive = true;
				
				TextInputType = cmd.GetSParam (1);
				
				if (cmd.iParams [1] != 0)
					_imeText = Plugin.GetString (cmd.iParams [0], cmd.iParams [1]);
				else
					_imeText = "";
				
				TextInputRect = new Rect (cmd.iParams [2], cmd.iParams [3], cmd.iParams [4], cmd.iParams [5]);
				
			}
		}
		
	}

	/// <summary>
	/// Event handler for view creation
	/// </summary>
	void processCRVW (object sender, CommandProcessEventArgs args)
	{
		Command cmd = args.Cmd;
		
		if (cmd.retCode > 0) {
			validate ();
			if (ViewCreated != null)
				ViewCreated (this);
		} else
			Debug.Log ("Error Creating View: " + Name);
		
		if (URL.Length != 0)
			LoadURL (URL);
	}

	/// <summary>
	/// Event handler in response to a remove request
	/// </summary>
	void processRMVW (object sender, CommandProcessEventArgs args)
	{
		
		if (TSet != null)
			TSet.Release ();
		
		Object.DestroyObject (this);
		Object.DestroyObject (this.gameObject);
	}

	#endregion

	#region Process Restart

	/// <summary>
	/// Invalidates all views, generally in the response to an issue with a UWKProcess restart
	/// </summary>
	public static void InvalidateAllViews ()
	{
		UWKView[] views = UWKView.FindObjectsOfType (typeof(UWKView)) as UWKView[];
		foreach (UWKView v in views)
			v.invalidate ();
	}

	/// <summary>
	/// Restarts all existing views, once the UWKProcess has restarted
	/// </summary>
	public static void RestartAllViews ()
	{
		UWKView[] views = UWKView.FindObjectsOfType (typeof(UWKView)) as UWKView[];
		foreach (UWKView v in views)
			v.restart ();
	}

	/// <summary>
	/// INTERNAL: Restart a view and reload the page it was on
	/// </summary>
	void restart ()
	{
		Plugin.ProcessInbound += processInbound;
		
		Command cmd = Command.NewCommand ("CRVW", Name, Width, Height, SmartRects ? 1 : 0);
		cmd.Post ().Process += processCRVW;
		
		if (URL != null && URL.Length > 0)
			LoadURL (URL);
	}

	/// <summary>
	/// Mark a view as invalid
	/// </summary>
	void invalidate ()
	{
		valid = false;
	}
	/// <summary>
	/// iOS only, captures a cached view for use as a texture mapped to a 3D model
	/// </summary>
	public void captureTexture ()
	{
		Command cmd = Command.NewCommand ("CAPT", Name, MainTexture.GetNativeTextureID (), MainTexture.width, MainTexture.height);
		cmd.Post ();
	}
	
	#endregion
	
	
}
