using UnityEngine;
using System.Collections;
using UWK;

/// <summary>
/// Simple menu for WebTexture Example
/// </summary>
public class WebTextureExample : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		
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
	}
	
}
