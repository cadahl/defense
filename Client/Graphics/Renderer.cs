namespace Client.Graphics
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	using Client;

	public class Renderer
	{
		public Dictionary<string, Tilemap> Tilemaps { get; private set; }
		public Dictionary<int, Background> Backgrounds { get; private set; }
		public Dictionary<string, Material> Materials { get; private set; }
		public GameWindow Window { get; private set; }
		public Dictionary<int,int> ShaderCache { get; private set; }
		
		public int FrameCount { get; private set; }
		public float BatchCount { get; private set; }
		public float DrawableCount { get; private set; }
		public float ZoomLevel { get; set; }

		public int Width 
		{
			get { return Window.ClientSize.Width; }
		}
		
		public int Height 
		{
			get { return Window.ClientSize.Height; }
		}

		public delegate void UpdateHandler (double time);
		public event UpdateHandler Update;

		public delegate void RenderHandler ();
		public event RenderHandler Render;

		private List<Drawable> _drawList;
		
		private VertexBuffer _vb;
		private RenderToTexture _rtt;		
		private Material _compositingMaterial;

		public Renderer()
		{
	        int sw = 1024;
	        int sh = 768;

			Window = new GameWindow(sw,sh,OpenTK.Graphics.GraphicsMode.Default,"Client",GameWindowFlags.Default,DisplayDevice.Default, 2, 1, OpenTK.Graphics.GraphicsContextFlags.Default);

			//Window.VSync = VSyncMode.On;
			Window.UpdateFrame += HandleWindowUpdateFrame;
			Window.RenderFrame += HandleWindowRenderFrame;

			Tilemaps = new Dictionary<string, Tilemap> ();
			Backgrounds = new Dictionary<int, Background> ();
			ShaderCache = new Dictionary<int, int>();
			Materials = new Dictionary<string, Material>();
			_drawList = new List<Drawable> ();

			_vb = new VertexBuffer();
			
			GL.ShadeModel (ShadingModel.Smooth);
			GL.Disable (EnableCap.CullFace);
			GL.Enable (EnableCap.Texture2D);
			GL.Disable (EnableCap.DepthTest);
			GL.DepthMask(false);
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			//GL.Enable (EnableCap.LineSmooth);
			//GL.Hint (HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.PixelStore (PixelStoreParameter.UnpackAlignment, 1);
			GL.ClearColor (0.3f, 0.3f, 0.3f, 1f);
			GL.CullFace (CullFaceMode.Back);
			GL.Disable(EnableCap.AlphaTest);
			
			_rtt = new RenderToTexture(Width, Height);
			_compositingMaterial = GetMaterial("distort");
			
			ZoomLevel = 1.0f;

			Window.Resize += (sender, e) => 
			{
				GL.Viewport (0, 0, Window.ClientSize.Width, Window.ClientSize.Height);
				GL.MatrixMode (MatrixMode.Projection);
				GL.LoadIdentity ();
				GL.Ortho (0, Window.ClientSize.Width*ZoomLevel, Window.ClientSize.Height*ZoomLevel, 0, -100.0, 100.0);
			};
			
			
			Window.Unload += (sender, e) =>
			{
				var textures = (from t in Tilemaps.Values select t.Texture).ToArray();
				GL.DeleteTextures(textures.Length, textures);
				
				foreach(var m in Materials.Values)
					m.Dispose();
				
				_rtt.Dispose();
				_compositingMaterial.Dispose();
			};
			
		}
		
		public Material GetMaterial(string name)
		{
			Material m = null;
			if(!Materials.TryGetValue(name, out m))
			{
				m = new Material(this, "data/"+name+"vs.glsl", "data/"+name+"fs.glsl");
				Materials[name] = m;
			}
			
			return m;
		}
		
		public void AddDrawable(Drawable d)
		{
			_drawList.Add(d);
		}

		public void RenderDrawables ()
		{
			_drawList.Sort ();
			DrawableCount = 0;
			BatchCount = 0;

			unsafe
			{
				Vertex* vertexData = null;
				int vertexCount = 0;
				
				{
					ulong lastMaterial = 0xFFFFFFFF;
					Drawable lastDrawable = null;
					foreach(Drawable d in _drawList) 
					{
						ulong material = d.FinalPriority & 0xFFFFFFFF;
						if(material != lastMaterial)
						{
							if(lastDrawable != null)
							{
								_vb.Unmap();
								lastDrawable.EndDraw(this, vertexCount);
							}
		
							vertexData = _vb.Map();
							vertexCount = 0;							
							
							d.BeginDraw(this);
		
							BatchCount++;
							lastMaterial = material;
						}
						
						d.Draw(this, vertexData, ref vertexCount);
						lastDrawable = d;
					}
					
					if(lastDrawable != null)
					{
						_vb.Unmap();
						lastDrawable.EndDraw(this, vertexCount);
					}
				}
			}
			

			DrawableCount = _drawList.Count;
			
			_drawList.Clear ();
		}

		public void RenderBackLayers (long frameCount)
		{
			foreach (KeyValuePair<int, Background> kvp in Backgrounds) 
			{
				if (kvp.Key < 100) 
				{
					kvp.Value.Draw (frameCount);
				}				
			}
		}

		public void RenderFrontLayers (long frameCount)
		{
			foreach (KeyValuePair<int, Background> kvp in Backgrounds) 
			{
				if (kvp.Key >= 100) 
				{
					kvp.Value.Draw (frameCount);
				}
			}
		}

		private void HandleWindowUpdateFrame (object sender, FrameEventArgs e)
		{
			Update(e.Time);
		}
		
		private void HandleWindowRenderFrame (object sender, FrameEventArgs e)
		{
			GL.MatrixMode (MatrixMode.Modelview);
			GL.LoadIdentity ();

			GL.Clear (ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);

			Render();
			
//			_rtt.Begin();
			
			RenderBackLayers (FrameCount);
			RenderDrawables ();
			RenderFrontLayers (FrameCount);
			
//			_rtt.End();
			
/*			GL.Disable(EnableCap.Blend);
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, _rtt.ColorTexture);
			_compositingMaterial.Bind();
			GL.Begin(BeginMode.Quads);
			GL.TexCoord2(_rtt.TextureCoords.X,_rtt.TextureCoords.W); GL.Vertex2(0,0);
			GL.TexCoord2(_rtt.TextureCoords.Z,_rtt.TextureCoords.W); GL.Vertex2(Width,0);
			GL.TexCoord2(_rtt.TextureCoords.Z,_rtt.TextureCoords.Y); GL.Vertex2(Width,Height);
			GL.TexCoord2(_rtt.TextureCoords.X,_rtt.TextureCoords.Y); GL.Vertex2(0,Height);
			GL.End();
			Material.Unbind();
			GL.BindTexture(TextureTarget.Texture2D, 0);
						 */
			
			FrameCount++;
			
			Window.SwapBuffers();
			//System.Threading.Thread.Sleep(1);
		}
		
		public void Run()
		{
			Window.VSync = VSyncMode.Off;
			Window.Run(60.0,60.0);
		}
	}
	
}
