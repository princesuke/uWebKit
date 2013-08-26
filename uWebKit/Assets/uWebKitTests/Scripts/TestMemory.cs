using UnityEngine;
using System.Collections;

public class TestMemory : MonoBehaviour
{
	
	public UWKView View0;
	
	string[] urls = new string[] {"http://www.cnn.com", "http://www.google.com", "http://www.microsoft.com", "http://www.apple.com", "http://slashdot.org", 
	"http://www.bluesnews.com", "http://unity3d.com", "http://www.github.com", "https://www.facebook.com", "http://www.twitter.com"};
	
	float lastRefresh = 0.0f;
	
	string currentUrl;
	
	int viewCounter = 0;
	
	// Use this for initialization
	void Start ()
	{
		View0 = UWKCore.CreateView ("TestView_0", 800, 600);
		View0.ViewCreated += viewCreated;	
		
	}
	
	int counter = 0;
	
	void updateURL (UWKView view)
	{
		
		view.LoadURL (urls [counter]);
		Debug.Log("Loading: " + urls[counter]);
		
		counter++;
		if (counter == urls.Length)
			counter = 0;
		
	}
	
	void OnGUI ()
	{
		
		View0.OnWebGUI (0, 0, 800, 600, 100);
		
	}	
	
	// Update is called once per frame
	void Update ()
	{
		
		lastRefresh += Time.deltaTime;
		
		if (lastRefresh > 10.0f) 
		{			
			lastRefresh = 0;
			
			UWKCore.RemoveView(View0);
			
			View0 = UWKCore.CreateView ("TestView_" + viewCounter++, 800, 600);
			View0.ViewCreated += viewCreated;	
			
		}
	
	}
		
	void viewCreated (UWKView view)
	{
		
		updateURL (view);
		
		view.Show();
		
	}
}
