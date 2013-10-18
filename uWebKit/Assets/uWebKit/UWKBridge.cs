/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using UWK;

namespace UWK
{
	public delegate void BridgeEventHandler (object sender,BridgeEventArgs e);


	/// <summary>
	/// Event Handler for JavaScript to Unity callbacks
	/// </summary>
	public class BridgeEventArgs : EventArgs
	{
		/// <summary>
		/// The name of the method to be called 
		/// </summary>
		public string MethodName;
		
		/// <summary>
		/// The arguments for the method call
		/// </summary>
		public string[] Args;

		public BridgeEventArgs (string methodName, string[] args)
		{
			this.MethodName = methodName;
			this.Args = args;
		}
		
	}

	/// <summary>
	/// Bridge objects expose functions and properities to JavaScript.
	/// Use the Bridge itself to set these
	/// </summary>
	public class BridgeObject
	{
		/// <summary>
		/// Invoke the specified methodName with parms.
		/// </summary>
		public void Invoke (string methodName, string[] parms)
		{
			
			string m = methodName.ToLower ();
			
			if (!callbacks.ContainsKey (m))
				return;
			
			callbacks [m] (this, new BridgeEventArgs (methodName, parms));
		}
		
		/// <summary>
		/// Bind the specified methodName and handler.
		/// </summary>
		public void Bind (string methodName, BridgeEventHandler handler)
		{
			string m = methodName.ToLower ();
			
			if (!callbacks.ContainsKey (m))
				callbacks [m] = handler;
			else
				callbacks [m] += handler;
		}

		Dictionary<string, BridgeEventHandler> callbacks = new Dictionary<string, BridgeEventHandler> ();
		
		/// <summary>
		/// The name of the bridge object, used to access it in Javascript
		/// </summary>
		public string Name;
		
		/// <summary>
		/// The properties accessible to Javascript
		/// </summary>
		public Dictionary<string, string> Properties = new Dictionary<string, string> ();
		
	}

	/// <summary>
	/// The JavaScript <-> Unity Bridge
	/// This static class is used to embed Javascript objects and values in the context of a loaded web page.
	/// It is also capable of receiving callbacks from Javascript on the page. 
	/// The bridge is persistent across pages and page loads.
	/// See UnityPageInfo.cs in Examples
	/// </summary>
	public static class Bridge
	{

		static Dictionary<string, BridgeObject> objects = new Dictionary<string, BridgeObject> ();

		static Bridge ()
		{
			Reset ();
			
			UWKCore.UWKProcessRestarted += Reset;
		}
		
		/// <summary>
		/// Resets the bridge in the case of a plugin reset
		/// </summary>
		public static void Reset ()
		{
			objects.Clear ();
			
			Plugin.ProcessInbound -= processInbound;
			Plugin.ProcessReturn -= processReturn;
			
			Plugin.ProcessInbound += processInbound;
			Plugin.ProcessReturn += processReturn;
			
		}
		
		/// <summary>
		/// Binds the event handler to the specified object and method name.
		/// For example: Bridge.BindCallback("MyObject", "doSomething", doSomething) 
		/// would bind the doSomething handler to be callable from Javascript with: 
		/// MyObject.invoke("DoSomething"); 
		/// See uWebKitExamlples/WebBrowser/UnityPageInfo.cs in examples
		/// </summary>
		public static void BindCallback (string objectName, string methodName, BridgeEventHandler handler)
		{
			BridgeObject bo;
			if (!objects.TryGetValue (objectName, out bo)) {
				bo = AddObject (objectName);
			}
			
			bo.Bind (methodName, handler);
		}
		
		/// <summary>
		/// Explicitly add an object (Note: if you just set a property on a non-existant object will be created)
		/// </summary>		 
		public static BridgeObject AddObject (string objectName)
		{
			if (objects.ContainsKey (objectName)) {
				Debug.LogWarning ("Object " + objectName + " already exists");
				return objects [objectName];
			}
			
			BridgeObject bo = objects [objectName] = new BridgeObject ();
			bo.Name = objectName;
			
			Command cmd = Command.NewCommand ("ADDO", objectName);
			cmd.Post ();
			
			return bo;
		}
		
		/// <summary>
		/// Retrieves a BridgeObject
		/// </summary>
		public static BridgeObject GetObject (string objectName)
		{
			BridgeObject bo;
			if (objects.TryGetValue (objectName, out bo))
				return bo;
			
			return null;
		}
		
		/// <summary>
		/// Set a property which will be accessible in Javascript.
		/// For example: Bridge.SetProperty("MyObject", "someValue", "Hello World!"); 
		/// would be accessible in Javascript as: 
		/// MyObject.someValue; 
		/// See uWebKitExamples/WebBrowser/UnityPageInfo.cs in examples 
		/// </summary>
		public static void SetProperty (string objectName, string propName, string value)
		{
			if (!objects.ContainsKey (objectName)) {
				AddObject (objectName);
			}
			
			BridgeObject bo = objects [objectName];
			
			string v;
			if (bo.Properties.TryGetValue (propName, out v))
			if (value == v)
				return;
			// save the trip
			// instant here
			bo.Properties [propName] = value;
			
			// and out to web process
			Command cmd = Command.NewCommand ("SETP", objectName, propName, value);
			cmd.Post ();
			
		}
		
		/// <summary>
		/// Override for int 
		/// </summary>
		public static void SetProperty (string objectName, string propName, int value)
		{
			SetProperty (objectName, propName, value.ToString ());
		}

		/// <summary>
		/// Override for float
		/// </summary>
		public static void SetProperty (string objectName, string propName, float value)
		{
			SetProperty (objectName, propName, value.ToString ());
		}
		
		/// <summary>
		/// Override for bool
		/// </summary>
		public static void SetProperty (string objectName, string propName, bool value)
		{
			SetProperty (objectName, propName, value.ToString ());
		}
		
		static void processInbound (object sender, CommandProcessEventArgs args)
		{
			Command cmd = args.Cmd;
			
			if (cmd.fourcc == "INVK") {
				string objectName = cmd.GetSParam (0);
				string methodName = cmd.GetSParam (1);
				
				List<string> parms = new List<string> ();
				
				int c = 1;
				for (int i = 0; i < cmd.iParams[0]; i++) {
					parms.Add (Plugin.GetString (cmd.iParams [c], cmd.iParams [c + 1]));
					c += 2;
				}
				
				string[] sparms = parms.ToArray ();
				
				BridgeObject bo;
				if (objects.TryGetValue (objectName, out bo)) {
					bo.Invoke (methodName, sparms);
				}
			}
			
		}
		
		/// <summary>
		/// default (empty) return value method
		/// </summary>
		static void processReturn (object sender, CommandProcessEventArgs args)
		{
			
		}
		
		
	}
}

