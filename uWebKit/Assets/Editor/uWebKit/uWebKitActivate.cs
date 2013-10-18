/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

//#define UWK_ASSETSTORE_BUILD	

using UnityEngine;
using UnityEditor;

#if !UWK_ASSETSTORE_BUILD

public class uWebKitActivate : ScriptableObject {
	
	
    [MenuItem ("uWebKit/Activate")]
    static void Activate() {
		
		if (EditorApplication.isPlaying)
		{
 			EditorApplication.isPaused = false;
        	EditorApplication.isPlaying = false;			
		}
		
		Debug.Log(EditorApplication.currentScene);
			
		EditorApplication.OpenScene("Assets/Editor/uWebKit/Activation/UWKActivationScene.unity");
		EditorApplication.ExecuteMenuItem("Edit/Play");
		
		return;
		
		
    }
	
    [MenuItem ("uWebKit/Purchase")]
    static void Purchase() {
		
		
		Application.OpenURL("http://www.uwebkit.com/uwebkit/store");
		return;
		
		
    }
        
}

#endif