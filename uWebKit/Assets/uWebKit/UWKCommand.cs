/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace UWK
{

	/// <summary>
	/// A UWK.Command is either generated via the Plugin (Unity) or via the native Web process
	/// </summary>
	public enum Source
	{
		PLUGIN = 0,
		PROCESS = 1
	}

	/// <summary>
	/// uWebKit uses a Command structure to pass commands, events, and data <-> the web core.
	/// This idiom is used so that the system can readily use multiple cores and to avoid
	/// interfering with Unity's rendering and game logic.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct Command
	{

		/// <summary>
		/// Allocate a new command with the given fourcc and variable number of int/string parameters
		/// </summary>
		public static Command NewCommand (string fourcc, params object[] parms)
		{
			Command cmd = new Command ();
			cmd.Init ();
			cmd.fourcc = fourcc;
			
			foreach (object o in parms) {
				if (o.GetType () == typeof(int)) {
					cmd.iParams[cmd.numIParams++] = (int)o;
				} else if (o.GetType () == typeof(float)) {
					cmd.iParams[cmd.numIParams++] = (int) ((float) o );
				} else if (o.GetType () == typeof(string)) {
					cmd.SetSParam (cmd.numSParams++, (string)o);
				} else {
					throw new Exception ("Unknown command parameter type: " + o.GetType ());
				}
			}
			
			return cmd;
		}
		/// <summary>
		/// Initializes a command which will be sent to the uWebKit process
		/// </summary>
		public void Init ()
		{
			this.iParams = new int[16];
			this.sParams = new byte[16 * 256 * 2];
			this.src = Source.PLUGIN;
		}


		/// <summary>
		/// Posts a command to the command queue for processing
		/// </summary>
		public CommandHandler Post ()
		{
			Plugin.PostCommand (ref this);
			
			if (id != 0) {
				CommandHandler h = new CommandHandler (ref this);
				return h;
			}
			
			return null;
		}
		
		/// <summary>
		/// Retrieve the commands string parameter at the specified index
		/// </summary>
		public string GetSParam (int index)
		{
			int startIndex = index * 256 * 2;
			int length = 0;
			
			while ((sParams[startIndex + length] != 0 || sParams[startIndex + length + 1] != 0) && length < 256)
				length += 2;
			
			if (length == 256)
				throw new Exception ("sParam is unterminated");
			
			System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding ();
			return encoding.GetString (sParams, startIndex, length);
		}
		
		/// <summary>
		/// Sets the commands index parameter to the specified string
		/// </summary>
		public void SetSParam (int index, string value)
		{
			int startIndex = index * 256 * 2;
			
			System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding ();
			Byte[] bytes = encoding.GetBytes (value);
			
			Array.Copy (bytes, 0, sParams, startIndex, bytes.Length);
			sParams[startIndex + bytes.Length] = 0;
			sParams[startIndex + bytes.Length + 1] = 0;
			
		}

		/// <summary>
		/// This function is deprecated in favor of Plugin.GetString and Plugin.AllocateString
		/// </summary>
		public void SpanSParams (int startIndex, string s)
		{
			numSParams = startIndex;
			
			for (int i = 0; i < s.Length;) {
				string ss = s.Substring (i, s.Length - i < 250 ? s.Length - i : 250);
				
				System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding ();
				Byte[] bytes = encoding.GetBytes (ss);
				
				int idx = numSParams * 256 * 2;
				
				Array.Copy (bytes, 0, sParams, idx, bytes.Length);
				sParams[idx + bytes.Length] = 0;
				sParams[idx + bytes.Length + 1] = 0;
				
				numSParams++;
				
				i += 250;
			}
		}

		/// <summary>
		/// Unique ID of this command, valid once the Command has been posted
		/// </summary>
		public uint id;

		/// <summary>
		/// FOURCC value which is used to designate the type of command
		/// </summary>
		[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 5)]
		public string fourcc;

		/// <summary>
		/// The origin of the command, Plugin (Unity) or Native 
		/// </summary>
		public Source src;

		/// <summary>
		/// Array of integer values passes as arguments or return values
		/// </summary>
		[MarshalAsAttribute(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 16)]
		public int[] iParams;

		/// <summary>
		/// Number of active iParams
		/// </summary>
		public int numIParams;

		/// <summary>
		/// Array of [16][256] string values passes as arguments or return values
		/// </summary>
		/// <remarks>
		/// IMPORTANT NOTE: Marshalling char[] truncates on first '/0', so use bytes
		/// </remarks>		
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16 * 256 * 2)]
		private byte[] sParams;

		/// <summary>
		/// Number of active sParams
		/// </summary>
		public int numSParams;

		/// <summary>
		/// Return code: < 0 error, 0 == unprocessed, > 0 = success
		/// </summary>
		public int retCode;

		// C++ side only
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public readonly SUCCESSCB cbSuccess;
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public readonly ERRORCB cbError;

		// if valid, instance who owns this message
		public IntPtr pthis;
		
		
	}

	public delegate void CommandProcessEventHandler (object sender, CommandProcessEventArgs e);
	
	
	/// <summary>
	/// Event arguments for CommandProcessEvent
	/// </summary>
	public class CommandProcessEventArgs : EventArgs
	{
		public CommandProcessEventArgs (Command cmd)
		{
			this.Cmd = cmd;
		}

		public Command Cmd;
	}
	/// <summary>
	/// Wraps a Command to prevent boxing/unboxing of structure in event handling
	/// </summary>
	public class CommandHandler
	{

		Command cmd;

		public CommandHandler (ref Command cmd)
		{
			this.cmd = cmd;
			Plugin.ProcessReturn += OnProcessReturn;
		}

		public event CommandProcessEventHandler Process;

		public void OnProcessReturn (object sender, CommandProcessEventArgs args)
		{
			if (args.Cmd.id != cmd.id)
				return;
			
			Plugin.ProcessReturn -= OnProcessReturn;
			
			if (Process != null)
				Process (null, args);
			
		}
		
	}
	
}
