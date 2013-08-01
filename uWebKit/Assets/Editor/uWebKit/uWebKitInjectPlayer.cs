using UnityEngine;
using UnityEditor;

using System.IO;
using System.Collections;

// This functionality is taken care of by PostprocessBuildPlayer on OSX

#if UNITY_STANDALONE_WIN

public class uWebKitInjectPlayer : ScriptableObject
{

	public static void CopyFolder (string sourceFolder, string destFolder)
	{
		if (!Directory.Exists (destFolder))
			Directory.CreateDirectory (destFolder);
		string[] files = Directory.GetFiles (sourceFolder);
		foreach (string file in files) {
			string name = Path.GetFileName (file);
			
			if (name.Contains (".meta"))
				continue;
			
			string dest = Path.Combine (destFolder, name);
			File.Copy (file, dest, true);
		}
		string[] folders = Directory.GetDirectories (sourceFolder);
		foreach (string folder in folders) {
			string name = Path.GetFileName (folder);
			string dest = Path.Combine (destFolder, name);
			CopyFolder (folder, dest);
		}
	}

	[MenuItem("uWebKit/Inject Player")]
	static void InjectPlayer ()
	{
		
		string path = EditorUtility.OpenFilePanel ("Select Player Executable", "", "exe");
				
		InjectPlayer( path, true, false );
	}
	
	[MenuItem("uWebKit/Inject Player (64 bit)")]
	static void InjectPlayer64 ()
	{
		
		string path = EditorUtility.OpenFilePanel ("Select 64 bit Player Executable", "", "exe");
				
		InjectPlayer( path, true, true );
	}
	
	public static void InjectPlayer( string path, bool display_success, bool x64 )
	{
		if (path.Length <= 0)
			return;
		
		string dataPath = Path.GetFileName (path);
		
		path = Path.GetDirectoryName (path);
		
		string copyPath;
		
		// removed OSX detection code that was faulty, todo: fix

		string winCopyPath;
		string winDataPath;
		
		if (x64) {
		
			winDataPath = dataPath;
			copyPath = "/Editor/uWebKit/Native/Windows64/Plugins/UWKPlugin.dll";
			winDataPath = path + "/" + winDataPath.Replace (".exe", "_Data"); 

			// This can happen when doing pure 64 bit build and no 32 bit built
			if (!Directory.Exists ((winDataPath + "\\Plugins").Replace("/", "\\")))
				Directory.CreateDirectory (winDataPath.Replace("/", "\\") + "\\Plugins");		
						
			winDataPath	+= "/Plugins/UWKPlugin.dll";
			
			winCopyPath = (Application.dataPath + copyPath).Replace("/", "\\");
			winDataPath = winDataPath.Replace("/", "\\");			
			
			File.Copy(winCopyPath, winDataPath, true); 
			
		}

		copyPath = "/Editor/uWebKit/Native/Windows";
		
		if (x64)
			copyPath = "/Editor/uWebKit/Native/Windows64/UWK64";
		
		
		winDataPath = dataPath;
		winDataPath = path + "/" + winDataPath.Replace (".exe", "_Data") + "/uWebKit/Native/Windows";
		
		winCopyPath = (Application.dataPath + copyPath).Replace("/", "\\");
		winDataPath = winDataPath.Replace("/", "\\");		
		
		try {
			if (System.IO.Directory.Exists (winDataPath))
				System.IO.Directory.Delete (winDataPath, true);
		
		} catch (System.IO.IOException e) {
			EditorUtility.DisplayDialog ("Player Injection Failed", e.Message, "Ok");
			return;
			
		}
		
		try {
			CopyFolder (winCopyPath, winDataPath);
		} catch (System.IO.IOException e) {
			EditorUtility.DisplayDialog ("Player Injection Failed", e.Message, "Ok");
			return;
		}
		if (x64) {
			winDataPath = dataPath;
			winDataPath = path + "/" + winDataPath.Replace (".exe", "_Data") + "/uWebKit/Native/Windows/uwkcore";
			winCopyPath = (Application.dataPath + "/Editor/uWebKit/Native/Windows/uwkcore").Replace("/", "\\");
			if (File.Exists(winCopyPath))
				File.Copy(winCopyPath, winDataPath.Replace("/", "\\"), true);
		}
		
		if( display_success ) {
			EditorUtility.DisplayDialog ("Player Injected", "uWebKit installed into selected player", "Ok");
		}
	}
}

#endif
