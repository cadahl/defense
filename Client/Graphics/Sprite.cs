namespace Client.Graphics
{
	using System;
	using System.Collections.Generic;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using Util;

	[Flags]
	public enum SpriteFlags : byte
	{
		Disable = 1,
		HFlip = 2,
		VFlip = 4,
	}
	
	public class Sprite : Drawable
	{
		public SpriteTemplate Template { get; set; }
		
		public class Instance
		{
			public Instance()
			{
				Scale = 1f;
				Color = Color4.White;
			}
			
			public float X, Y;
			public float Scale;
			public Color4 Color;
			public ushort Angle;
			public SpriteFlags Flags;
			public byte Frame;
		}

		protected List<Instance> _instances = new List<Instance>();
		private static Material _material;
		
		public Sprite(SpriteTemplate st, Flags flags, int priority) : base(flags, priority)
		{
			Template = st;
			Add(0,0,0);
		}
		
		public void Add(float x, float y, int animationFrame)
		{
			Instance ins = new Instance();
			ins.X = x;
			ins.Y = y;
			ins.Frame = (byte)animationFrame;
			_instances.Add(ins);
		}

		public void Resize(int newCount)
		{
			while(_instances.Count < newCount)
				_instances.Add(new Instance());
			
			while(_instances.Count > newCount)
				_instances.RemoveAt(_instances.Count-1);
		}
		
		public int Count { get { return _instances.Count; } }
		
		public Instance this[int i]
		{
		    get { return _instances[i]; }
		    private set { _instances[i] = value; }
		}
		
		public override ulong FinalPriority
		{
			get
			{
				return (ulong)(base.FinalPriority | ((uint)Template.TilemapName.GetHashCode()));
			}
		}
		
		public override void BeginDraw(Renderer r)
		{
			Tilemap tilemap;
			
			if(!r.Tilemaps.TryGetValue(Template.TilemapName, out tilemap))
			{
				r.Tilemaps[Template.TilemapName] = tilemap = new Tilemap (Template.TilemapName);
			}
			
			GL.BindTexture (TextureTarget.Texture2D, tilemap.Texture);

			Flags type = _flags & Drawable.Flags.TypeMask;
			
			if(Drawable.Flags.TypeNoBlend == type)
			{
				GL.Disable (EnableCap.Blend);
			}
			else
			{
				GL.Enable (EnableCap.Blend);
				GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
			}

			if(_material == null)
			{
				_material = r.GetMaterial("sprite");
			}
			
			_material.Bind();
		}

		public override void EndDraw(Renderer r, int vertexCount)
		{
			GL.DrawArrays(BeginMode.Quads, 0, vertexCount);
		}
		
		public override unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount)
		{
			Tilemap tilemap = r.Tilemaps[Template.TilemapName];

			foreach(Instance ins in _instances)
			{
				if((ins.Flags & SpriteFlags.Disable) != 0)
					continue;
				
				float x = ins.X;
				float y = ins.Y;
				float width = Template.Rectangle.Width;
				float height = Template.Rectangle.Height;
				
				// If the object should be included in scrolling...
				if ((_flags & Drawable.Flags.NoScroll) == 0) 
				{
					// Apply offset so we're always positioned relative to the gameplay background.
					Background bg = r.Backgrounds[0];
					x -= (float)Math.Floor(bg.HScroll);
					y -= (float)Math.Floor(bg.VScroll);
				}
				
				// Calculate texcoords, with possible animation.
				int tx = Template.Rectangle.X;
				int ty = Template.Rectangle.Y;
				
				int frameOffset = (Template.FrameOffset > 0 ? Template.FrameOffset : Template.Rectangle.Width);

				if(Template.VerticalAnimation)
				{
					ty += ins.Frame * frameOffset;
				}
				else
				{
					tx += ins.Frame * frameOffset;
					
					// If the animation would take us outside the right edge of the
					// texture, start on the next row.
					while (tx + width > tilemap.Width) 
					{
						tx -= (tilemap.Width / frameOffset) * frameOffset;
						ty += Template.Rectangle.Height;
					}
				}
				
				float s0 = tx * tilemap.PixelStepX;
				float t0 = ty * tilemap.PixelStepY;
				float s1 = (tx + width) * tilemap.PixelStepX;
				float t1 = (ty + height) * tilemap.PixelStepY;
				
				// Flip the sprite horizontally?
				if ((ins.Flags & SpriteFlags.HFlip) != 0) 
				{
					float stmp = s0;
					s0 = s1;
					s1 = stmp;
				}
				
				// Flip the sprite vertically?
				if ((ins.Flags & SpriteFlags.VFlip) != 0) 
				{
					float ttmp = t0;
					t0 = t1;
					t1 = ttmp;
				}				
				
				Matrix4 m = Matrix4.CreateTranslation(x,y,0.0f);
				Matrix4 scalem = Matrix4.Scale(ins.Scale);
				Matrix4 rotm = Matrix4.Rotate(Quaternion.FromAxisAngle(Vector3.UnitZ, ins.Angle * (2.0f * (float)Math.PI) / 4096.0f));
				Matrix4.Mult(ref scalem, ref m, out m);
				Matrix4.Mult(ref rotm, ref m, out m);
				float ox = Template.Centered ? Template.Rectangle.Width/2 : Template.Offset.X;
				float oy = Template.Centered ? Template.Rectangle.Height/2 : Template.Offset.Y;
				Vector4 v00 = new Vector4(-ox, -oy, 0.0f, 1.0f);
				Vector4 v10 = new Vector4(width-ox, -oy, 0.0f, 1.0f);
				Vector4 v11 = new Vector4(width-ox, height-oy, 0.0f, 1.0f);
				Vector4 v01 = new Vector4(-ox, height-oy, 0.0f, 1.0f);
	
				Vector4.Transform(ref v00, ref m, out v00);
				Vector4.Transform(ref v10, ref m, out v10);
				Vector4.Transform(ref v11, ref m, out v11);
				Vector4.Transform(ref v01, ref m, out v01);

				float colorizeMode = (_flags & Drawable.Flags.Colorize) != 0 ? 1.0f : 0.0f;
				
				vertexData[vertexCount].Position.X = v00.X;
				vertexData[vertexCount].Position.Y = v00.Y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = ins.Color;
				vertexData[vertexCount].TexCoord.X = s0;
				vertexData[vertexCount].TexCoord.Y = t0;
				vertexData[vertexCount].TexCoord.Z = colorizeMode;
				vertexCount++;

				vertexData[vertexCount].Position.X = v10.X;
				vertexData[vertexCount].Position.Y = v10.Y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = ins.Color;
				vertexData[vertexCount].TexCoord.X = s1;
				vertexData[vertexCount].TexCoord.Y = t0;
				vertexData[vertexCount].TexCoord.Z = colorizeMode;
				vertexCount++;

				vertexData[vertexCount].Position.X = v11.X;
				vertexData[vertexCount].Position.Y = v11.Y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = ins.Color;
				vertexData[vertexCount].TexCoord.X = s1;
				vertexData[vertexCount].TexCoord.Y = t1;
				vertexData[vertexCount].TexCoord.Z = colorizeMode;
				vertexCount++;

				vertexData[vertexCount].Position.X = v01.X;
				vertexData[vertexCount].Position.Y = v01.Y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = ins.Color;
				vertexData[vertexCount].TexCoord.X = s0;
				vertexData[vertexCount].TexCoord.Y = t1;
				vertexData[vertexCount].TexCoord.Z = colorizeMode;
				vertexCount++;				
			}
		}
	}
}
