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
/// Example Browser script which shows integration with Unity's GUI system
/// </summary>
public class ExampleBrowser : MonoBehaviour
{

	// A Browser Tab
	struct Tab
	{
		// once the tab's WebView has been created we'll be valid
		public bool Valid;

		// A unique Id for this tab
		public uint Id;

		// the unique view name associated with this tab's view	
		public string ViewName;

		// the view itself
		public UWKView View;

		// the tab's currenly loaded URL
		public string URL;

		// the tab's title (from HTML)
		public string Title;

	}

	// position and dimensions of browser 
	public int X;
	public int Y;

	public int Width = 1024;
	public int Height = 600;

	// default tab URL
	public string URL = "http://www.google.com";

	string currentURL = "";

	int pageLoadProgress = 100;

	// the tabs (up to 4)
	Tab[] tabs = new Tab[4];
	int activeTab = 0;
	int numTabs = 0;
	uint tabId = 0;

	// on small screens we'll limit tabs
	int maxTabs = 4;
	bool smallScreen = false;

	// used to avoid duplicate mouse updates
	int lastMouseX = -1;
	int lastMouseY = -1;

	// whether the browser is currently bring sized
	bool sizing;

	// the rect of the browser as a whole
	Rect windowRect;
	
	// tables reuse this Id
	int unityWindowId = 1;

	// the view area of the browser
	Rect browserRect;

	// GUI textures (loaded from Resources/)
	Texture2D texHeader;
	Texture2D texFooter;
	Texture2D texBack;
	Texture2D texForward;
	Texture2D texReload;
	Texture2D texLogo;
	Texture2D texProgress;

	// Sling in and out controls
	public bool slingOut = false;
	public bool slingIn = false;
	float slingSpeed = 0.0f;

	// whether or not to show the Smart Rects being rendered
	// note that Smart Rects are disabled when transparency effects
	// are being used
	public bool showSmartRects = false;

	// Transparency setting of browser for compositing effects
	public float transparency {

		get { return _transparency; }

		set {
			
			_transparency = value;
			
			for (int i = 0; i < numTabs; i++) {
				tabs[i].View.Transparency = _transparency;
				tabs[i].View.SmartRects = _transparency >= 1.0f ? true : false;
			}
		}
	}

	public UWKView CurrentView {
		get { return tabs[activeTab].View; }
	}

	float _transparency = 1.0f;

	// The browser skin being used
	public GUISkin Skin;

	// Toggles the visibily of the browser
	public bool Visible {

		get { return visible; }


		set {
			visible = value;
			try {
				tabs[activeTab].View.Visible = value;
			} catch {
			}
		}
	}

	bool visible = true;

	// Sling the browser off the screen
	public void SlingOut ()
	{
		slingIn = false;
		slingOut = true;
		slingSpeed = 0;
	}


	// Sling the browser onto the srceen
	public void SlingIn ()
	{
		Visible = true;
		slingIn = true;
		slingOut = false;
		slingSpeed = 0;
	}


	// Load a URL into the active tab
	public void LoadURL (string url)
	{
		tabs[activeTab].View.LoadURL (url);
	}

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
		
		windowRect.x = v.x;
		windowRect.y = v.y;
	}

	// Set the browser offscreen (without modifying visibility)
	public void SetOffscreen ()
	{
		windowRect.x = -windowRect.width;
	}


	// Use this for initialization
	void Start ()
	{
		
		// small screen, limit tabs
		if (Screen.width <= 960) {
			smallScreen = true;
			maxTabs = 2;
			
			Width = 800;
			Height = 500;
		}		
		
		// Create our view
		createTab ();
		setActiveTab (0);
				
		windowRect = new Rect (X, Y, Width + 8, Height + 138);
		
		Center ();
		
		texHeader = Resources.Load ("Browser/header", typeof(Texture2D)) as Texture2D;
		texFooter = Resources.Load ("Browser/footer", typeof(Texture2D)) as Texture2D;
		texBack = Resources.Load ("Browser/btnBack", typeof(Texture2D)) as Texture2D;
		texForward = Resources.Load ("Browser/btnForward", typeof(Texture2D)) as Texture2D;
		texReload = Resources.Load ("Browser/btnReload", typeof(Texture2D)) as Texture2D;
		texLogo = Resources.Load ("Browser/uWebKitIcon", typeof(Texture2D)) as Texture2D;
		
		Color c = new Color (0, 1, 0, .2f);
		texProgress = new Texture2D (32, 32);
		for (int x = 0; x < 32; x++)
			for (int y = 0; y < 32; y++)
				texProgress.SetPixel (x, y, c);
		
		texProgress.Apply ();
		
	}

	// Look up a tab by a given view
	int findTab (UWKView view)
	{
		for (int i = 0; i < numTabs; i++)
			if (tabs[i].View == view)
				return i;
		
		return -1;
	}
	
	
	// If you want to listen in on view finished add a delegate
	public LoadFinishedDelegate LoadFinished;


	// Delegate called when a tab's page is loaded
	void loadFinished (UWKView view)
	{
		// ensure 100%
		pageLoadProgress = 100;
		
		if (LoadFinished != null)
			LoadFinished(view);
	}

	void loadProgress (int progress)
	{
		pageLoadProgress = progress;
	}

	// Delegate called when a tab's (HTML) title has changed
	void titleChanged (UWKView view, string title)
	{
		int i = findTab (view);
		
		if (i >= 0)
			tabs[i].Title = title;
		
	}

	// Delegate called when a tab's URL has changed (redirects, etc)
	void urlChanged (UWKView view, string url)
	{
		int i = findTab (view);
		
		if (i >= 0)
			tabs[i].URL = url;
		
		if (i == activeTab)
			currentURL = url;
		
	}

	// Delegate called when a tab's view has been created and the tab is marked as valid
	void viewCreated (UWKView view)
	{
		int i = findTab (view);
		
		if (i >= 0) {
			tabs[i].Valid = true;
			tabs[i].View.LoadURL (URL);
			view.Transparency = _transparency;
		}
		
		view.ViewCreated -= viewCreated;
		
		view.Show (true);
		
	}


	// Create a new tab (requires Pro for > 1 WebView)
	void createTab ()
	{
		if (numTabs == 4)
			return;
		
		Tab t = tabs[numTabs];
		t.Valid = false;
		t.Id = tabId++;
		t.ViewName = "TabView" + t.Id;
		t.URL = URL;
		t.View = UWKCore.CreateView (t.ViewName, Width, Height, true);
		t.View.ViewCreated += viewCreated;
		t.View.URLChanged += urlChanged;
		t.View.TitleChanged += titleChanged;
		t.View.LoadFinished += loadFinished;
		t.View.LoadProgress += loadProgress;
		t.View.Visible = true;
		t.Title = "Tab " + t.Id;
		
		tabs[numTabs] = t;
		
		numTabs++;
		
	}

	// Sets the currently active tab
	void setActiveTab (int idx)
	{
		activeTab = idx;
		
		for (int i = 0; i < numTabs; i++) {
			if (i != idx) {
				tabs[i].View.Active = false;
				tabs[i].View.Hide ();
			} else {
				currentURL = tabs[i].URL;
				tabs[i].View.Active = true;
				tabs[i].View.Show (true);
				tabs[i].View.BringToFront(false);
			}
		}
		
	}

	// Update is called once per frame
	void Update ()
	{
		for (int i = 0; i < numTabs; i++) {
			tabs[i].Valid = tabs[i].View.Valid;
		}
		
		if (!tabs[activeTab].Valid)
			return;
		
		if (!visible)
			return;
		
		// Handle mouse input
		
		UWKView view = tabs[activeTab].View;
		
		#if UNITY_EDITOR || !UNITY_IPHONE
		
		int x = (int)browserRect.x + (int)windowRect.x;
		int y = (int)browserRect.y + (int)windowRect.y;
		
		Vector3 mousePos = Input.mousePosition;
		mousePos.y = Screen.height - mousePos.y;
		
		if (mousePos.x != lastMouseX || mousePos.y != lastMouseY) {
			lastMouseX = (int)mousePos.x;
			lastMouseY = (int)mousePos.y;
			view.SetMousePos (lastMouseX - x, lastMouseY - y);
		}
		
		float scroll = Input.GetAxis ("Mouse ScrollWheel");
		
		if (scroll != 0.0f) {
			view.OnScrollWheel (scroll);
		}
		
		for (int i = 0; i < 3; i++) {
			if (Input.GetMouseButtonDown (i)) {
				
				// Catch click in lower right for sizing
				if (lastMouseX >= windowRect.xMax - 8 && lastMouseX <= windowRect.xMax + 8)
					if (lastMouseY >= windowRect.yMax - 8 && lastMouseY <= windowRect.yMax + 8) {
						sizing = true;
					}
				
				
				if (!sizing)
					view.OnMouseButtonDown (lastMouseX - x, lastMouseY - y, i);
			}
			
			if (Input.GetMouseButtonUp (i)) {
				
				if (!sizing)
					view.OnMouseButtonUp (lastMouseX - x, lastMouseY - y, i);
				
				sizing = false;
			}
			
		}
		#endif
		
		// Update sling effect
		
		if (slingIn) {
			slingSpeed += Time.deltaTime + slingSpeed / 4;
			
			for (int i = 0; i < numTabs; i++) {
				if (i != activeTab)
					continue;
				
				tabs[i].View.Active = true;
			}
			
			
			Vector2 v = new Vector2 ();
			GetCenterPos (ref v);
			
			Vector2 distance = new Vector2 (Math.Abs (v.x - windowRect.x), Math.Abs (v.y - windowRect.y));
			Vector2 delta = new Vector2 (distance.x * Time.deltaTime, distance.y * Time.deltaTime);
			delta *= 4.0f;
			
			if (delta.x < 4)
				delta.x = 4;
			
			if (delta.y < 4)
				delta.y = 4;
			
			if (delta.x > distance.x)
				delta.x = distance.x;
			
			if (delta.y > distance.y)
				delta.y = distance.y;
			
			if (windowRect.x < v.x) {
				windowRect.x += (int)delta.x;
			}
			if (windowRect.x > v.x) {
				windowRect.x -= (int)delta.x;
			}
			
			if (windowRect.y < v.y) {
				windowRect.y += (int)delta.y;
			}
			if (windowRect.y > v.y) {
				windowRect.y -= (int)delta.y;
			}
			
			if (Math.Abs (windowRect.x - v.x) <= delta.x + 1)
				windowRect.x = v.x;
			
			if (Math.Abs (windowRect.y - v.y) <= delta.y + 1)
				windowRect.y = v.y;
			
			if (windowRect.x == v.x && windowRect.y == v.y) {
				slingIn = false;
			}
			
		}
		
		
		if (slingOut) {
			slingSpeed += Time.deltaTime + slingSpeed / 3;
			
			if (slingSpeed < 10)
				slingSpeed = 10;
			
			if (slingSpeed > 75)
				slingSpeed = 75;
			
			windowRect.x -= slingSpeed;
			if (-windowRect.x > windowRect.width + 10) {
				slingOut = false;
				Visible = false;
				
				for (int i = 0; i < numTabs; i++) {
					tabs[i].View.Active = false;
				}
				
			}
		}
		
	}
	
	void guiSmallTabs (ref GUIStyle buttonStyle)
	{
		UWKView view = tabs[activeTab].View;
		
		GUILayout.BeginHorizontal ();
		
		buttonStyle.normal.background = null;
		buttonStyle.active.background = null;
		buttonStyle.normal.textColor = new Color (.65f, .65f, .65f, 1.0f);
		buttonStyle.hover.textColor = new Color (.35f, .35f, .35f, 1.0f);
		
		GUILayoutOption width = GUILayout.MaxWidth (128);
		
		// Bookmark Buttons
		
		if (GUILayout.Button ("Google", buttonStyle, width))
			view.LoadURL ("http://www.google.com");
		
		if (GUILayout.Button ("YouTube", buttonStyle, width))
			view.LoadURL ("http://www.youtube.com");
		
		
		if (GUILayout.Button ("uWebKit", buttonStyle, width))
			view.LoadURL ("http://www.uwebkit.com/uwebkit");
		
		
		for (int i = 0; i < maxTabs; i++) {
			if (i < numTabs) {
				string title = tabs[i].Title;
				if (title.Length > 24) {
					title = title.Substring (0, 24);
					title += "...";
				}
				
				if (GUILayout.Button (title, GUILayout.MaxWidth (256)))
					setActiveTab (i);
			}
		}
		
		if (numTabs < maxTabs)
			if (GUILayout.Button ("+", GUILayout.MaxWidth (32))) {
				
				if (UWKCore.StandardVersion) {
					Debug.LogError ("uWebKit Standard Version supports 1 web view");
					view.LoadHTML (uWebKitStandard.ONEVIEW_HTML);
				} else {
					
					createTab ();
					setActiveTab (numTabs - 1);
				}
			}
		
		GUILayout.EndHorizontal ();
		
	}	

	void guiTabs (ref GUIStyle buttonStyle)
	{
		UWKView view = tabs[activeTab].View;
		
		GUILayout.BeginHorizontal ();
		
		buttonStyle.normal.background = null;
		buttonStyle.active.background = null;
		buttonStyle.normal.textColor = new Color (.65f, .65f, .65f, 1.0f);
		buttonStyle.hover.textColor = new Color (.35f, .35f, .35f, 1.0f);
		
		GUILayoutOption width = GUILayout.MaxWidth (128);
		
		// Bookmark Buttons
		
		if (GUILayout.Button ("Unity3D", buttonStyle, width))
			view.LoadURL ("http://unity3d.com/unity");
		
		if (GUILayout.Button ("Google", buttonStyle, width))
			view.LoadURL ("http://www.google.com");
		
		// Facebook requires HTTPS which requires Pro/Studio
		if (!UWKCore.StandardVersion) {
			if (GUILayout.Button ("Facebook", buttonStyle, width))
				view.LoadURL ("https://www.facebook.com");
		} else {
			if (GUILayout.Button ("Twitter", buttonStyle, width))
				view.LoadURL ("http://www.twitter.com");
		}
		
		if (GUILayout.Button ("YouTube", buttonStyle, width))
			view.LoadURL ("http://www.youtube.com");
		
		if (GUILayout.Button ("Google Maps", buttonStyle, width))
			view.LoadURL ("http://maps.google.com");
		
		
		if (GUILayout.Button ("Steam", buttonStyle, width))
			view.LoadURL ("http://store.steampowered.com/app/49800");
		
		if (UWKCore.imeEnabled && GUILayout.Button ("WoW China", buttonStyle, width))
			view.LoadURL ("http://www.battlenet.com.cn/wow/zh"); else if (GUILayout.Button ("Penny Arcade", buttonStyle, width))
			view.LoadURL ("http://www.penny-arcade.com");
		
		if (GUILayout.Button ("HTML5", buttonStyle, width))
			view.LoadURL ("http://www.w3.org/html/logo");
		
		
		if (GUILayout.Button ("uWebKit", buttonStyle, width))
			view.LoadURL ("http://uwebkit.com/uwebkit/features");
		
		
		if (GUILayout.Button ("Unity Info", buttonStyle, width)) {
			UnityInfoPage.SetProperties ();
			view.LoadHTML (UnityInfoPage.HTML);
		}
		
		GUILayout.EndHorizontal ();
		
		// tabs
		GUILayout.BeginHorizontal ();
		
		for (int i = 0; i < 4; i++) {
			if (i < numTabs) {
				string title = tabs[i].Title;
				if (title.Length > 24) {
					title = title.Substring (0, 24);
					title += "...";
				}
				
				UWKView v = tabs[i].View;
				
				if (GUILayout.Button (title, GUILayout.MaxWidth (256)))
					setActiveTab (i);
				
				Rect br = GUILayoutUtility.GetLastRect ();
				
				br.x += 8;
				br.y += 4;
				
				br.height = 16;
				br.width = 16;
				
				if (v.Icon != null) {
					GUI.DrawTexture (br, v.Icon);
				}
			}
		}
		
		if (numTabs < maxTabs)
			if (GUILayout.Button ("+", GUILayout.MaxWidth (32))) {
				
				if (UWKCore.StandardVersion) {
					Debug.LogError ("uWebKit Standard Version supports 1 web view");
					view.LoadHTML (uWebKitStandard.ONEVIEW_HTML);
				} else {
					
					createTab ();
					setActiveTab (numTabs - 1);
				}
			}
		
		
		GUILayout.EndHorizontal ();
		
	}

	// Main Window function of browser, used to draw GUI
	void windowFunction (int windowID)
	{
		GUI.skin = Skin;
		
		GUIStyle buttonStyle = new GUIStyle (GUI.skin.button);
		buttonStyle.padding = new RectOffset (2, 2, 2, 2);
		
		UWKView view = tabs[activeTab].View;
		
		GUI.color = new Color (1.0f, 1.0f, 1.0f, transparency);
		
		browserRect = new Rect (4, 118 + 8, Width, Height);
		
		Rect headerRect = new Rect (4, 4, Width, 118 + 4);
		GUI.DrawTexture (headerRect, texHeader);
		
		int titleHeight = 24;
		Rect titleRect = new Rect (0, 0, Width, titleHeight);
		
		GUI.DragWindow (titleRect);
		
		GUILayout.BeginVertical ();
		// Main Vertical
		GUILayout.BeginArea (new Rect (8, 4, Width, 118));
		
		GUILayout.BeginHorizontal ();
		
		GUILayout.BeginVertical ();
		
		// title
		Texture2D bxTex = GUI.skin.box.normal.background;
		GUI.skin.box.normal.background = null;
		GUI.skin.box.normal.textColor = new Color (.25f, .25f, .25f, 1.0f);
		GUILayout.Box (view.Title);
		GUI.skin.box.normal.background = bxTex;
		
		GUILayout.BeginHorizontal ();
		
		if (GUILayout.Button (texBack, buttonStyle, GUILayout.Width (texBack.width), GUILayout.Height (texBack.height)))
			view.Back ();
		
		if (GUILayout.Button (texForward, buttonStyle, GUILayout.Width (texForward.width), GUILayout.Height (texForward.height)))
			view.Forward ();
		
		if (GUILayout.Button (texReload, buttonStyle, GUILayout.Width (texReload.width), GUILayout.Height (texReload.height)))
			view.LoadURL (currentURL);
		
		
		bool nav = false;
		if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
			nav = true;
		
		GUI.SetNextControlName ("BrowserURL");
		
#if UNITY_IPHONE && !UNITY_EDITOR
		GUILayout.Label (currentURL, GUILayout.MaxWidth (Width - 196));
#else
		currentURL = GUILayout.TextField (currentURL, GUILayout.MaxWidth (Width - 196));
#endif

		
		if (pageLoadProgress != 100) {
			Rect urlRect = GUILayoutUtility.GetLastRect ();
			urlRect.width *= (float)pageLoadProgress / 100.0f;
			GUI.DrawTexture (urlRect, texProgress);
		}
		
		if (nav && GUI.GetNameOfFocusedControl () == "BrowserURL") {
			GUIUtility.keyboardControl = 0;
			view.LoadURL (currentURL);
		}
		
		GUILayout.EndHorizontal ();
		
		if (!smallScreen)
			guiTabs (ref buttonStyle);
		else
			guiSmallTabs (ref buttonStyle);
		
		GUILayout.EndVertical ();
		buttonStyle.normal.background = null;
		
		buttonStyle.normal.background = null;
		buttonStyle.hover.background = null;
		buttonStyle.active.background = null;
		buttonStyle.padding = new RectOffset (0, 0, 0, 0);
		
		if (GUILayout.Button (texLogo, buttonStyle, GUILayout.Width (84), GUILayout.Height (100))) {
			#if UNITY_IPHONE
			//SlingOut ();
			#endif
		}
		
		GUILayout.EndHorizontal ();
		
		GUILayout.EndArea ();
		
		GUILayout.EndVertical ();
		
		// End Main Vertical
		
		view.DrawGUI ((int)browserRect.x, (int)browserRect.y, windowRect);
		
		Rect footerRect = new Rect (4, browserRect.yMax, Width, 8);
		GUI.DrawTexture (footerRect, texFooter);
		
		// handle sizing
		if (sizing) {
			
			Width = lastMouseX - X;
			Height = lastMouseY - Y - 138;
			
			view.Width = Width;
			view.Height = Height;
			
			// can be clamped
			Width = view.Width;
			Height = view.Height;
			
			
			windowRect = new Rect (X, Y, Width + 8, Height + 138);
			
		} else {
			windowRect = new Rect (X, Y, Width + 8, Height + 138);
		}
		
		
	}

	void OnGUI ()
	{
		
		if (!visible)
			return;
		
		if (!tabs[activeTab].Valid)
			return;
		
		UWKView view = tabs[activeTab].View;
		
		GUI.skin = null;
		
		windowRect = GUILayout.Window (unityWindowId, windowRect, windowFunction, "");
				
		// Handle keyboard input
		Event e = Event.current;
		
		if (GUIUtility.keyboardControl == 0)
			if (e.isKey) {
				view.ProcessKey (e);
			}
		
		if (e.isKey)
			if (e.keyCode == KeyCode.Tab || e.character == '\t')
				e.Use ();
		
		
		// Handle sizing and clamping
		X = (int)windowRect.x;
		Y = (int)windowRect.y;
		
		if (view.Width != Width || view.Height != Height) {
			view.Height = Height;
			view.Width = Width;
			
			// can be clamped so set back
			Height = view.Height;
			Width = view.Width;
		}
		
		GUI.skin = null;
		
	}
	
}
