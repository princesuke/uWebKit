/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public delegate void FBResponseDelegate (object response);

public struct FBFriend
{
	public string Id;
	public string Name;
}

/// <summary>
/// Example Facebook API class which can be built upon
/// </summary>
public class FacebookAPI : MonoBehaviour {
	
	// uWebKit example Facebook App, you can set these to your own values
	public static string AppId = "138823316204078";
	public static string RedirectUri = "http://uwk.uwebkit.com/examples/facebook.html";
	public static string AppURL =  "https://www.facebook.com/apps/application.php?id=138823316204078";
	
	// Once the user has logged into FB and accepted the App permission
	// this will be a valid token which can be used to interact with FB
	public static string AccessToken = "";
	
	class Request
	{
		public string Url;
		public string TextResult;
		public ArrayList Result;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	public static string ReplaceFirstOccurrance(string original, string oldValue, string newValue)
	{
	    if (String.IsNullOrEmpty(original))
	        return String.Empty;
	    if (String.IsNullOrEmpty(oldValue))
	        return original;
	    if (String.IsNullOrEmpty(newValue))
	        newValue = String.Empty;
	    
		int loc = original.IndexOf(oldValue);
		
		if (loc < 0)
			return original;
		
	    return original.Remove(loc, oldValue.Length).Insert(loc, newValue);
	}	
	
	IEnumerator request(Request req)
	{
		return request(req, null);
	}
	
	IEnumerator request(Request req, byte[] postData)
	{
		
		string url = req.Url.Replace("APP_ID", AppId);
		url = url.Replace("ACCESS_TOKEN", AccessToken);
		url = url.Replace(" ", "%20");	
				
		WWW www = new WWW(url, postData);
    	yield return www;
		
		if (www.error != null)
		{
			Debug.Log(url + "\n" + www.error);
			yield break;
		}
		
		req.TextResult = www.text;
		
		try {
			Hashtable ftable = (Hashtable) MiniJSON.JsonDecode(www.text);
		
			req.Result = (ArrayList) ftable["data"];
		} catch (Exception e)
		{
			Debug.LogWarning(e.Message);
		}
		
	}

	/// <summary>
	/// Gets list of FB friends including their FB id and name
	/// </summary>
	public IEnumerator GetFriends(FBResponseDelegate del)
	{
		
		Request req = new Request();
		
		req.Url = "https://graph.facebook.com/me/friends?access_token=ACCESS_TOKEN";
		
		yield return StartCoroutine(request(req));
		
		List<FBFriend> friends = new List<FBFriend>();
		
		for (int i = 0; i < req.Result.Count; i++)
		{
			Hashtable friend = req.Result[i] as Hashtable;
			
			FBFriend fbf = new FBFriend();
			
			fbf.Id = (string) friend["id"] as string;
			fbf.Name = (string) friend["name"] as string;
			friends.Add(fbf);
		}
		
		
		if (del != null)
			del(friends);
					
	}
	
	public IEnumerator GetFriends()
	{
		return GetFriends(null);
	}
		
	/// <summary>
	/// Loads a FB Profile picture into a Texture2D
	/// </summary>
	public static IEnumerator GetProfileTexture(string id, FBResponseDelegate del)
	{
		string url = "https://graph.facebook.com/" + id + "/picture";
		
		WWW www = new WWW(url);
	
		yield return www;
		
		if (del != null)		
			del( www.texture ); 		
		
		
	}
		
}
