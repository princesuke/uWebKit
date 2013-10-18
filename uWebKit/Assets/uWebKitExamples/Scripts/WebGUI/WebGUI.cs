/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using UWK;

/// <summary>
/// Minimal WebGUI using uWebKit and Unity GUI
/// </summary>
public class WebGUI : MonoBehaviour
{
	
	// URL 
	public string URL = "http://www.google.com";

	// position 
	public int X = 0;
	public int Y = 0;
	
	// dimensions
	public int Width = 1024;
	public int Height = 600; 
	
	// transparency
	public float Transparency = 100.0f;
	
	// the view itself
	public UWKView View;

	void Start ()
	{		
		// Create the view
		View = UWKCore.CreateView ("BasicWebGUI", URL, Width, Height);		
	} 
	
	void OnGUI ()
	{
		// Draw the view
		View.OnWebGUI(X, Y, Width, Height, Transparency);
	}
	
}
