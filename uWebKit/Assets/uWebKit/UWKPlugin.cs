/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace UWK
{

	/// <summary>
	/// Plugin -> Managed calling delegate
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void PROCESSCB (IntPtr pcmd);

	/// <summary>
	/// C++ side only and only in process, only here for reference
	/// </summary>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void SUCCESSCB (ref Command cmd);
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void ERRORCB (ref Command cmd);

	/// <summary>
	/// Delegate for logging purposes (native -> Unity)
	/// </summary>
	public delegate void LOGCB (string message);

	/// <summary>
	/// Interop class representing the native uWebKit Unity plugin
	/// </summary>
	public class Plugin
	{

		/// <summary>
		/// Event handler for inbound commands
		/// </summary>
		public static event CommandProcessEventHandler ProcessInbound;

		/// <summary>
		/// Event handler for plugin generated commands that have returned once
		/// being processed by the native side
		/// </summary>
		public static event CommandProcessEventHandler ProcessReturn;

		/// <summary>
		/// Initializes the plugin and sets the logging system
		/// </summary>
		public static bool Init (LOGCB log)
		{
			
			#if !UNITY_IPHONE || UNITY_EDITOR
			try {
				UWK_SetLogCB (log);
			} catch {
				return false;
			}
			#endif
			
			bool initialized = UWK_Init ();
			
			if (!initialized)
				throw new Exception ("Unable to initialize UWKPlugin");
			
			#if !UNITY_IPHONE || UNITY_EDITOR
			UWK_SetProcessCB (ProcessCommand);
			#endif
			
			return true;
			
		}

		#if UNITY_IPHONE && !UNITY_EDITOR

		/// <summary>
		/// Ticks the plugin 
		/// </summary>
		public static void Update ()
		{
			Command cmd = new Command ();
			while (true) {
				int ret = UWK_Update (ref cmd);
				if (ret != 0)
					ProcessCommand (ref cmd);
				else
					break;
			}
			
		}
		#else
		/// <summary>
		/// Ticks the plugin 
		/// </summary>
		public static void Update ()
		{
			UWK_Update ();
		}

		#endif
		/// <summary>
		/// Shutdown and cleanup the plugin
		/// </summary>
		public static void Shutdown ()
		{
			UWK_Shutdown ();
		}

		/// <summary>
		/// Clears all commands and event handlers
		/// </summary>
		public static void ClearCommands ()
		{
			ProcessInbound = null;
			ProcessReturn = null;
			UWK_ClearCommands ();
		}

		/// <summary>
		/// Posts a command to the command queue and retrieves an id number
		/// </summary>
		public static void PostCommand (ref Command cmd)
		{
			cmd.id = UWK_PostCommand (ref cmd);
			
			if (cmd.id == 0)
				throw new Exception ("Unable to Post Command");
		}

		/// <summary>
		/// Processes either a return or inbound command
		/// </summary>
		public static void ProcessCommand (ref Command cmd)
		{
			
			if (cmd.src == Source.PLUGIN) {
				
				if (ProcessReturn != null)
					ProcessReturn (null, new CommandProcessEventArgs (cmd));
				
			} else {
				
				CommandProcessEventArgs args = new CommandProcessEventArgs (cmd);
				
				if (ProcessInbound != null) {
					ProcessInbound (null, args);
				}
				
				// post return, ensuring that retcode is valid
				if (args.Cmd.retCode == 0)
					args.Cmd.retCode = 1;
				
				PostCommand (ref args.Cmd);
			}
			
		}


		/// <summary>
		/// Processes either a return or inbound command
		/// </summary>
		public static void ProcessCommand (IntPtr pcmd)
		{
			Command cmd = (Command)Marshal.PtrToStructure (pcmd, typeof(Command));
			ProcessCommand (ref cmd);
		}

		/// <summary>
		/// Grabs texture data for a TextureSet's backbuffer or SubTexture
		/// </summary>
		public static bool UpdateTexture (bool isBackBuffer, int mip, int child, ref TextureInterop textureInterop)
		{
			return UWK_UpdateTexture (isBackBuffer, mip, child, ref textureInterop);
		}

		/// <summary>
		///  Retrieves a string allocated on the uWebKit memory paging system
		/// </summary>
		public static string GetString (int page, int sz)
		{
			byte[] bytes = new byte[sz];
			GetBytes (page, sz, bytes);
			
			System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding ();
			
			return encoding.GetString (bytes);
			
		}

		/// <summary>
		///  Retrieves raw bytes allocated on the uWebKit memory paging system
		/// </summary>
		public static bool GetBytes (int page, int sz, byte[] bytes)
		{
			GCHandle pinned = GCHandle.Alloc (bytes, GCHandleType.Pinned);
			
			bool r = UWK_CopyAndFree (page, pinned.AddrOfPinnedObject (), sz);
			
			pinned.Free ();
			
			return r;
			
		}

		/// <summary>
		///  Allocates a string on the uWebKit memory paging system
		/// </summary>
		public static int AllocateString (string value, ref int size)
		{
			System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding ();
			Byte[] bytes = encoding.GetBytes (value);
			
			Byte[] nbytes = new Byte[bytes.Length + 2];
			Array.Copy (bytes, nbytes, bytes.Length);
			nbytes[bytes.Length] = 0;
			nbytes[bytes.Length + 1] = 0;
			
			GCHandle pinned = GCHandle.Alloc (nbytes, GCHandleType.Pinned);
			
			int i = UWK_AllocateAndCopy (pinned.AddrOfPinnedObject (), nbytes.Length);
			
			pinned.Free ();
			
			size = nbytes.Length;
			
			if (i < 0)
				throw new Exception ("Error Allocating String");
			
			return i;
		}

		[DllImport("kernel32")]
		public static extern int LoadLibrary (string libraryName);

		#if UNITY_IPHONE && !UNITY_EDITOR
		const string _dllLocation = "__Internal";
		#else
		const string _dllLocation = "UWKPlugin";
		#endif

		// interop
		[DllImport(_dllLocation)]
		static extern void UWK_SetLogCB (LOGCB callback);

		[DllImport(_dllLocation)]
		static extern void UWK_SetProcessCB (PROCESSCB callback);

		[DllImport(_dllLocation)]
		static extern bool UWK_Init ();

		[DllImport(_dllLocation)]
		public static extern void UWK_InitProcess ();

		#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport(_dllLocation)]
		static extern int UWK_Update (ref Command cmd);
		#else
		[DllImport(_dllLocation)]
		static extern void UWK_Update ();
		#endif

		[DllImport(_dllLocation)]
		static extern void UWK_Shutdown ();

		[DllImport(_dllLocation)]
		static extern void UWK_ClearCommands ();

		[DllImport(_dllLocation, CharSet = CharSet.Ansi)]
		static extern uint UWK_PostCommand (ref Command command);
		
		// returns true if a partial update
		[DllImport(_dllLocation, CharSet = CharSet.Ansi)]
		static extern bool UWK_UpdateTexture (bool isBackBuffer, int mip, int child, ref TextureInterop textureInterop);
		
		[DllImport(_dllLocation, CharSet = CharSet.Ansi)]
		static extern int UWK_AllocateAndCopy (IntPtr addr, int size);

		[DllImport(_dllLocation, CharSet = CharSet.Ansi)]
		static extern bool UWK_CopyAndFree (int idx, IntPtr pout, int size);

#if !UNITY_IPHONE || UNITY_EDITOR		
		[DllImport(_dllLocation)]
		public static extern bool UWK_ProcessResponding ();
#endif
		
	}
	
}
