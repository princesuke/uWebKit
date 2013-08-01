using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Example of using UWKCore across multiple scene loads (and unloads)
/// Please note to view this sample, the included example scenes (including the ExampleLoader scene) 
/// must be added to the Build Settings 
/// </summary>
public class ExampleLoader : MonoBehaviour
{

	void Awake ()
	{
		// ensure Core is up
		UWKCore.Init ();
	}

	// Use this for initialization
	void Start ()
	{
		
		int count = 6;
		if (UWKCore.StandardVersion)
			count = 5;
		
		if (Application.levelCount < count) {
			
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog ("Example Loader", "This example features dynamic scene loading and thus requires the example scenes (including the ExampleLoader scene) be added to the Build Settings", "Ok");
			EditorApplication.ExecuteMenuItem ("Edit/Play");
			#endif
		}
		
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnGUI ()
	{
		
		int x = Screen.width / 2 - 90;
		int y = Screen.height / 2 - 400;
		
		if (y < 0)
			y = 0;
		
		int buttonWidth = 180;
		
		GUI.BeginGroup (new Rect (x, y, buttonWidth, 600));
		
		Rect brect = new Rect (0, 0, buttonWidth, 60);
		if (GUI.Button (brect, "Example 1 - Web Browser")) {
			Application.LoadLevel ("Example1Browser");
		}
		
		brect.y += 80;
		if (GUI.Button (brect, "Example 2 - Web GUI")) {
			Application.LoadLevel ("Example2WebGUI");
		}
		
		brect.y += 80;
		if (GUI.Button (brect, "Example 3 - Scene")) {
			Application.LoadLevel ("Example3Scene");
		}
		
		
		brect.y += 80;
		if (GUI.Button (brect, "Example 4 - Web Texture")) {
			Application.LoadLevel ("Example4WebTexture");
		}
		
		// Facebook requires https for apps
		if (!UWKCore.StandardVersion) {
			brect.y += 80;
			if (GUI.Button (brect, "Example 5 - Facebook")) {
				Application.LoadLevel ("Example5Facebook");
			}
		}
		
		brect.y += 80;
		if (GUI.Button (brect, "Clear Cookies")) {
			UWKCore.ClearCookies ();
		}
		
		brect.y += 80;
		if (GUI.Button (brect, "Quit")) {
			Application.Quit ();
		}
		
		GUI.EndGroup ();
		
	}
	
}
