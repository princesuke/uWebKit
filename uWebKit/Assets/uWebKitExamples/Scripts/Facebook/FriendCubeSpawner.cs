using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawns cubes mapped with Facebook friend profile pics
/// </summary>
public class FriendCubeSpawner : MonoBehaviour
{
	FacebookAPI facebook;

	List<FBFriend> friends;

	bool textureLoading = false;
	float x = -5.0f;
	float y = -5.0f;
	
	int maxCubes = 50;
	
	// Use this for initialization
	void Start ()
	{
		facebook = gameObject.GetComponent<FacebookAPI> () as FacebookAPI;
	}

	void OnGUI ()
	{
		
		Rect brect = new Rect (0, 0, 120, 40);
		
		if (GUI.Button (brect, "Back")) {
			UWKCore.DestroyViewsOnLevelLoad = true;
			Application.LoadLevel ("ExampleLoader");
		}
	}

	void textureLoaded ()
	{
		textureLoading = false;
	}

	// Update is called once per frame
	void Update ()
	{
		
		if (friends == null)
			return;
		
		if (friends.Count != 0 && maxCubes > 0 && !textureLoading) {
			string id = friends[0].Id;
			friends.RemoveAt (0);
			
			maxCubes--;
			
			textureLoading = true;
			
			var cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
			Rigidbody body = cube.AddComponent<Rigidbody> () as Rigidbody;
			body.mass = .1f;
			
			FriendCube fcube = cube.AddComponent<FriendCube> ();
			fcube.URL = "http://graph.facebook.com/" + id + "/picture";
			
			fcube.TextureLoaded += textureLoaded;
			
			cube.transform.position = new Vector3 (x, 10, y);
			
			x += 1.0f;
			
			if (x > 5) {
				
				y += 1.0f;
				
				if (y > 5)
					y = -5;
				
				x = -5.0f;
			}
		}
		
	}

	public void gotFriends (object response)
	{
		
		friends = response as List<FBFriend>;
	}


	void OnAccessTokenReceived (UWKView view)
	{
		//Transform t = Instantiate(FPSController, new Vector3(0,5,5), Quaternion.identity) as Transform;
		
		GameObject fps = GameObject.Find ("First Person Controller");
		
		if (fps != null) {
			fps.transform.position = new Vector3 (0, 5, 5);
			fps.transform.rotation = Quaternion.identity;
			fps.transform.Rotate (0, 180, 0);
		}
		
		// create the FB plane		
		GameObject plane = GameObject.CreatePrimitive (PrimitiveType.Plane);
		plane.transform.localScale = new Vector3 (2, 2, 2);
		WebTexture texture = plane.AddComponent<WebTexture> () as WebTexture;
		texture.View = view;
		texture.KeyboardEnabled = false;
		texture.MouseEnabled = false;
		texture.Rotate = false;
		
		StartCoroutine (facebook.GetFriends (gotFriends));
	}
}
