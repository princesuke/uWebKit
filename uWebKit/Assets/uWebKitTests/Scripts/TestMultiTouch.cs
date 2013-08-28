using UnityEngine;
using System.Collections;

public class TestMultiTouch : MonoBehaviour
{
	
	public UWKView View0;
		
	float lastRefresh = 0.0f;

	// Use this for initialization
	void Start ()
	{
		View0 = UWKCore.CreateView ("TestView_0", 800, 600);
		View0.ViewCreated += viewCreated;	
		
	}
		
	void OnGUI ()
	{
		
		View0.OnWebGUI (0, 0, 800, 600, 100);
		
		if (GUI.Button(new Rect(0,0,256,128),"touchCount="+Input.touchCount))
		{
			Debug.Log("bump");
		}		
		
	}	
	
	// Update is called once per frame
	void Update ()
	{
		
		lastRefresh += Time.deltaTime;
		
		if (lastRefresh > 10.0f) 
		{			
			lastRefresh = 0;
			
			View0.Visible = !View0.Visible;
						
		}
	
	}
		
	void viewCreated (UWKView view)
	{
			
		view.LoadURL("http://www.unity3d.com");
		
		view.Show();
		
	}
}
