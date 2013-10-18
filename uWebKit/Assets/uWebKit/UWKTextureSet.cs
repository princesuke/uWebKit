/******************************************
  * uWebKit 
  * (c) 2013 THUNDERBEAST GAMES, LLC
  * sales@uwebkit.com
*******************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

using Object = UnityEngine.Object;

namespace UWK
{
	/// <summary>
	/// Structure for communicating texture updates with native plugin
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct TextureInterop
	{
		public int Dirty;

		public IntPtr Pixels;

		public int X;
		public int Y;

		public int Width;
		public int Height;
	}

	/// <summary>
	/// When SmartRects are enabled, SubBuffers hold the update
	/// information for dirty subregions
	/// </summary>
	public class SubBuffer
	{
		public bool Active;

		public int X;
		public int Y;

		public int Width;
		public int Height;

		public Texture2D Texture;
		public Color32[] Pixels;
		public GCHandle PinnedPixels;
	}

	/// <summary>
	/// One or more textures that are used to draw a UWKView and 2D or 3D
	/// space.  The textures are updated on the fly in response to updates
	/// from the web core.
	/// </summary>
	public class TextureSet
	{

		public Texture2D BackBuffer;
		public Color32[] Pixels;
		public GCHandle PinnedPixels;

		public SubBuffer[,] SubBuffers;

		public int Width;
		public int Height;

		public bool SmartRects;

		public void Init (int width, int height, bool smartRects)
		{
			
			SmartRects = smartRects;
			
			// calculate the power of 2, which will be our texture size
			uint val = (uint)width, powof2 = 1;
			while (powof2 < val)
				powof2 <<= 1;
			
			Width = (int)powof2;
			
			val = (uint)height;
			powof2 = 1;
			while (powof2 < val)
				powof2 <<= 1;
			
			Height = (int)powof2;
			
			BackBuffer = new Texture2D ((int)Width, (int)Height, TextureFormat.ARGB32, false);
			Pixels = BackBuffer.GetPixels32 (0);
			PinnedPixels = GCHandle.Alloc (Pixels, GCHandleType.Pinned);
			
			if (smartRects) {
				
				SubBuffers = new SubBuffer[2, 2];
				
				int th = Height;
				int tw = Width;
				
				if (th > 1024)
					th = 1024;
				if (tw > 1024)
					tw = 1024;
				
				for (int i = 0; i < 2; i++) {
					for (int j = 0; j < 2; j++) {
						
						SubBuffer s = SubBuffers[i, j] = new SubBuffer ();
						
						s.Width = tw >> (i + 1);
						s.Height = th >> (i + 1);
						
						s.Texture = new Texture2D ((int)s.Width, (int)s.Height, TextureFormat.ARGB32, false);
						s.Pixels = s.Texture.GetPixels32 (0);
						s.PinnedPixels = GCHandle.Alloc (s.Pixels, GCHandleType.Pinned);
					}
				}
			}
			
		}

		/// <summary>
		/// Update the necessary textures based on changes to the page
		/// </summary>
		public void Update ()
		{
			TextureInterop b;
			
			// back buffer first
			b.Dirty = 0;
			b.X = b.Y = 0;
			b.Width = Width;
			b.Height = Height;
			b.Pixels = PinnedPixels.AddrOfPinnedObject ();
			
			
			if (Plugin.UpdateTexture (true, 0, 0, ref b))
				return; // partial update
			
			if (b.Dirty != 0) {
				
				// backbuffer is dirty, sub buffers don't figure into it (scrolling, fullscreen video, etc)
				BackBuffer.SetPixels32 (Pixels, 0);
				BackBuffer.Apply (false);
								
				if (SmartRects) {
					for (int i = 0; i < 2; i++)
						for (int j = 0; j < 2; j++)
							SubBuffers[i, j].Active = false;
				}
				
				
				return;
			}
			
			if (!SmartRects)
				return;
			
			
			// update the subbuffers
			for (int i = 0; i < 2; i++)
				for (int j = 0; j < 2; j++) {
					SubBuffer s = SubBuffers[i, j];
					s.Active = false;
					b.Dirty = 0;
					b.Pixels = s.PinnedPixels.AddrOfPinnedObject ();
					
					Plugin.UpdateTexture (false, i, j, ref b);
					
					if (b.Dirty == 0)
						continue;
					
					s.Active = true;
					
					s.X = b.X;
					s.Y = b.Y;
					
					s.Width = b.Width;
					s.Height = b.Height;
					
					s.Texture.SetPixels32 (s.Pixels);
					s.Texture.Apply (false);
					
					//Debug.Log("Updating SubBuffer " + i + " " + j);
					
				}
			
			
		}

		/// <summary>
		/// Release the associated texture and buffer data.
		/// It is critical to call this to avoid leaks
		/// </summary>
		public void Release ()
		{
			Pixels = null;
			
			if (PinnedPixels.IsAllocated) {
				PinnedPixels.Free ();
			}
			
			Object.DestroyImmediate (BackBuffer);
			BackBuffer = null;
			
			if (SmartRects) {
				
				for (int i = 0; i < 2; i++) {
					for (int j = 0; j < 2; j++) {
						SubBuffer s = SubBuffers[i, j];
						
						s.Pixels = null;
						if (s.PinnedPixels.IsAllocated)
							s.PinnedPixels.Free ();
						
						Object.DestroyImmediate (SubBuffers[i, j].Texture);
						
						s.Texture = null;
						
						SubBuffers[i, j] = null;
					}
				}
				
				SubBuffers = null;
			}
		}
		
	}
}
