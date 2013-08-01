using UnityEngine;
using System.Collections;

public delegate void FriendCubeTextureLoaded ();

public class FriendCube : MonoBehaviour
{

	public string URL;

	public FriendCubeTextureLoaded TextureLoaded;

	// Use this for initialization
	IEnumerator Start ()
	{
		
		
		WWW www = new WWW (URL);
		
		yield return www;
		
		if (TextureLoaded != null)
			TextureLoaded ();
		
		renderer.material.mainTexture = www.texture;
		
	}
	
}
