using UnityEngine;
using System.Collections;
using UWK;

/// <summary>
/// Simple menu for WebGUI Example
/// </summary>
public class WebGUIExample : MonoBehaviour
{
	WebGUI webGUI;

	// Use this for initialization
	void Start ()
	{		
		webGUI = GameObject.FindObjectOfType (typeof(WebGUI)) as WebGUI;
	}

	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnGUI ()
	{
		
		Rect brect = new Rect (0, 0, 120, 40);
		
		if (GUI.Button (brect, "Back")) {
			
			UWKCore.DestroyViewsOnLevelLoad = true;
			Application.LoadLevel ("ExampleLoader");
		}
		
		brect.y += 50;
		if (GUI.Button (brect, "Center Window")) {
			
			webGUI.X = Screen.width / 2 - webGUI.Width / 2;
			webGUI.Y = Screen.height / 2 - webGUI.Height / 2;
		}
		
	}
	
}
