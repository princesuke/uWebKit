using UnityEngine;
using System.Collections;

public class TestView : MonoBehaviour
{
	
	public UWKView View0;
	public UWKView View1;
	public UWKView View2;
	public UWKView View3;
	public UWKView View4;
	public UWKView View5;
	public UWKView View6;
	public UWKView View7;
	string[] urls = new string[] {"http://www.cnn.com", "http://www.google.com", "http://www.microsoft.com", "http://www.apple.com", "http://slashdot.org", 
	"http://www.bluesnews.com", "http://unity3d.com", "http://www.github.com", "https://www.facebook.com", "http://www.twitter.com"};
	float lastRefresh = 0.0f;
	
	// Use this for initialization
	void Start ()
	{
		
		// create 8 views all of which will use 1024x1024 backing textures
		View0 = UWKCore.CreateView ("TestView0", 800, 600);
		View1 = UWKCore.CreateView ("TestView1", 800, 600);
		View2 = UWKCore.CreateView ("TestView2", 800, 600);
		View3 = UWKCore.CreateView ("TestView3", 800, 600);
		View4 = UWKCore.CreateView ("TestView4", 800, 600);
		View5 = UWKCore.CreateView ("TestView5", 800, 600);
		View6 = UWKCore.CreateView ("TestView6", 800, 600);
		View7 = UWKCore.CreateView ("TestView7", 800, 600);
		
		View0.ViewCreated += viewCreated;
		View1.ViewCreated += viewCreated;
		View2.ViewCreated += viewCreated;
		View3.ViewCreated += viewCreated;
		View4.ViewCreated += viewCreated;
		View5.ViewCreated += viewCreated;
		View6.ViewCreated += viewCreated;
		View7.ViewCreated += viewCreated;
	
	}
	
	int counter = 0;
	
	void updateURL (UWKView view)
	{
		
		view.LoadURL (urls [counter]);
		Debug.Log("Loading: " + urls[counter]);
		
		counter ++;
		if (counter == urls.Length)
			counter = 0;
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		lastRefresh += Time.deltaTime;
		if (lastRefresh > 2.0f) {
			
			lastRefresh = 0;
			
			int x = (int)(Random.value * 7);
			
			UWKView view = null;
			switch (x) {
			case 0:
				view = View0;
				break;
			case 1:
				view = View1;
				break;
			case 2:
				view = View2;
				break;
			case 3:
				view = View3;
				break;
			case 4:
				view = View4;
				break;
			case 5:
				view = View5;
				break;
			case 6:
				view = View6;
				break;
			case 7:
				view = View7;
				break;
				
			}
			updateURL(view);
			
		}
			
	
	}
	
	void OnGUI ()
	{
		
		int x = (int) Input.mousePosition.x;
		int y = Screen.height - (int) Input.mousePosition.y;
		
		x /= 256;
		y /= 256;
		
		if (x > 3) x = 3;
			
		if (y > 1) y = 1;
		
		int idx = x + y * 4;

		UWKView view = null;
		switch (idx) {
		case 0:
			view = View0;
			break;
		case 1:
			view = View1;
			break;
		case 2:
			view = View2;
			break;
		case 3:
			view = View3;
			break;
		case 4:
			view = View4;
			break;
		case 5:
			view = View5;
			break;
		case 6:
			view = View6;
			break;
		case 7:
			view = View7;
			break;
		}
		
		if (view != null && view.Valid)
			view.BringToFront(false);
				
		// Draw the views
		View0.OnWebGUI (0, 0, 256, 256);
		View1.OnWebGUI (256, 0, 256, 256);
		View2.OnWebGUI (512, 0, 256, 256);
		View3.OnWebGUI (768, 0, 256, 256);
		View4.OnWebGUI (0, 256, 256, 256);
		View5.OnWebGUI (256, 256, 256, 256);
		View6.OnWebGUI (512, 256, 256, 256);
		View7.OnWebGUI (768, 256, 256, 256);
		
	}
	
	void viewCreated (UWKView view)
	{
		
		updateURL (view);
		
	}
}
