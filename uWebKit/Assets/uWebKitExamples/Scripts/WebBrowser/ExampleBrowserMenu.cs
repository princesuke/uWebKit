using UnityEngine;
using System.Collections;
using UWK;
using System.Collections.Generic;


/// <summary>
/// Simple control menu for the Browser Example
/// </summary>
public class ExampleBrowserMenu : MonoBehaviour
{

	ExampleBrowser browser;

	bool sling = false;
	
	bool invert = true;
	
	string jquery;
	
	List<UWKView> jqueryLoaded = new List<UWKView>();

	// Use this for initialization
	void Start ()
	{
		
		browser = GameObject.FindObjectOfType (typeof(ExampleBrowser)) as ExampleBrowser;
		
		// register to listen in on when a view has finished loading
		browser.LoadFinished += loadFinished;
		
		// load up minimal jquery
		TextAsset ta = Resources.Load("Browser/jquery.min", typeof(TextAsset)) as TextAsset;
		jquery = ta.text;
		
	}

	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void loadFinished (UWKView view)
	{
		if (jqueryLoaded.Contains(view))
			jqueryLoaded.Remove(view);
	}
	
	void evalResult (object sender, CommandProcessEventArgs args)
	{
		Command cmd = args.Cmd;
		
		string result = Plugin.GetString(cmd.iParams[0], cmd.iParams[1]);
		
		Debug.Log(result);
	}

	void OnGUI ()
	{
		Rect brect = new Rect (0, 0, 120, 40);
		
		if (GUI.Button (brect, "Sling Browser")) {
			
			sling = !sling;
			
			if (sling)
				browser.SlingOut ();
			else
				browser.SlingIn ();
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "Transparency -")) {
			var v = browser.transparency;
			v -= .1f;
			if (v < 0.1f)
				v = 0.1f;
			browser.transparency = v;
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "Transparency +")) {
			var v = browser.transparency;
			v += .1f;
			if (v > 1.0f)
				v = 1.0f;
			browser.transparency = v;
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "Toggle Alpha Mask")) {
			browser.CurrentView.AlphaMask = !browser.CurrentView.AlphaMask;
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "Eval Javascript")) {
			
			// Inject minimal jquery into loaded frame 
			// this should be done once the view has finished loading
			// which can be setup using the load finished delegate
			// for example purposes, we have it tied to the GUI button here
			// we also cache the load here so that we don't do this every time the
			// user clicks
			if (!jqueryLoaded.Contains(browser.CurrentView))
			{
				browser.CurrentView.EvaluateJavaScript(jquery);
				jqueryLoaded.Add(browser.CurrentView);
			}
			
			// example with return value
			browser.CurrentView.EvaluateJavaScript("document.title;", evalResult);
			
			// use jquery to rotate images
			if (invert)
				browser.CurrentView.EvaluateJavaScript("$('img').each( function () { $(this).css('-webkit-transition', '-webkit-transform 2s'); $(this).css('-webkit-transform', 'rotate(180deg)') } )");
			else
				browser.CurrentView.EvaluateJavaScript("$('img').each( function () { $(this).css('-webkit-transition', '-webkit-transform 2s'); $(this).css('-webkit-transform', 'rotate(0deg)') } )");
			
				
			invert = !invert;
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "View SmartRects")) {
			browser.showSmartRects = !browser.showSmartRects;
			Command cmd = Command.NewCommand ("DSMR");
			cmd.iParams[0] = browser.showSmartRects ? 1 : 0;
			cmd.Post ();
		}
		
		
		brect.y += 50;
		if (GUI.Button (brect, "Back")) {
			UWKCore.DestroyViewsOnLevelLoad = true;
			Application.LoadLevel ("ExampleLoader");
		}
		
	}
}
