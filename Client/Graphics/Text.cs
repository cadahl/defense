namespace Client.Graphics
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;

	public class TextFont
	{
		public Dictionary<char,byte[]> Table;
		public SpriteTemplate Template;
	}

	public class TextLine : Drawable
	{
		public int X, Y;
		public int Width { get; private set; }
		public Color4 Color = Color4.White;
		
		public static TextFont NormalFont;
		public static TextFont CashFont;
		public TextFont Font;
		private Material _material;
		private string _text;
		private int[] _indices;
		private int[] _xs;
		
		public TextLine (Renderer r, int x, int y, int priority) : base(0, priority)
		{
			if(CashFont == null)
			{
				CashFont = new TextFont();
				CashFont.Template = new SpriteTemplate
				{ 
					TilemapName = "units", 
					Rectangle = new Rectangle(240, 384, 16, 21), 
					Centered = false
				};
				
				byte i = 0;
				CashFont.Table = new Dictionary<char, byte[]>()
				{
					{ '$', new byte[] { i++, 14 } },
					{ '0', new byte[] { i++, 13 } },
					{ '1', new byte[] { i++, 12 } },
					{ '2', new byte[] { i++, 12 } },
					{ '3', new byte[] { i++, 13 } },
					{ '4', new byte[] { i++, 13 } },
					{ '5', new byte[] { i++, 12 } },
					{ '6', new byte[] { i++, 13 } },
					{ '7', new byte[] { i++, 13 } },
					{ '8', new byte[] { i++, 12 } },
					{ '9', new byte[] { i++, 13 } }
				};
			}
			
			if(NormalFont == null)
			{
				NormalFont = new TextFont();
				NormalFont.Template = new SpriteTemplate 
				{
					TilemapName = "units", 
					Rectangle = new Rectangle(0, 464, 16, 16),
					Centered = false
				};

				byte i = 0;
				NormalFont.Table = new Dictionary<char, byte[]>()
				{
					{ 'a', new byte[] { i++, 5 } },
					{ 'b', new byte[] { i++, 6 } },
					{ 'c', new byte[] { i++, 5 } },
					{ 'd', new byte[] { i++, 6 } },
					{ 'e', new byte[] { i++, 6 } },
					{ 'f', new byte[] { i++, 5 } },
					{ 'g', new byte[] { i++, 6 } },
					{ 'h', new byte[] { i++, 6 } },
					{ 'i', new byte[] { i++, 2 } },
					{ 'j', new byte[] { i++, 3 } },
					{ 'k', new byte[] { i++, 6 } },
					{ 'l', new byte[] { i++, 2 } },
					{ 'm', new byte[] { i++, 11 } },
					{ 'n', new byte[] { i++, 6 } },
					{ 'o', new byte[] { i++, 6 } },
					{ 'p', new byte[] { i++, 6 } },
					{ 'q', new byte[] { i++, 6 } },
					{ 'r', new byte[] { i++, 4 } },
					{ 's', new byte[] { i++, 5 } },
					{ 't', new byte[] { i++, 5 } },
					{ 'u', new byte[] { i++, 6 } },
					{ 'v', new byte[] { i++, 7 } },
					{ 'w', new byte[] { i++, 10 } },
					{ 'x', new byte[] { i++, 7 } },
					{ 'y', new byte[] { i++, 7 } },
					{ 'z', new byte[] { i++, 5 } },
					{ 'A', new byte[] { i++, 9 } },
					{ 'B', new byte[] { i++, 7 } },
					{ 'C', new byte[] { i++, 7 } },
					{ 'D', new byte[] { i++, 8 } },
					{ 'E', new byte[] { i++, 5 } },
					{ 'F', new byte[] { i++, 6 } },
					{ 'G', new byte[] { i++, 8 } },
					{ 'H', new byte[] { i++, 8 } },
					{ 'I', new byte[] { i++, 5 } },
					{ 'J', new byte[] { i++, 5 } },
					{ 'K', new byte[] { i++, 7 } },
					{ 'L', new byte[] { i++, 6 } },
					{ 'M', new byte[] { i++, 10 } },
					{ 'N', new byte[] { i++, 8 } },
					{ 'O', new byte[] { i++, 9 } },
					{ 'P', new byte[] { i++, 6 } },
					{ 'Q', new byte[] { i++, 9 } },
					{ 'R', new byte[] { i++, 6 } },
					{ 'S', new byte[] { i++, 5 } },
					{ 'T', new byte[] { i++, 7 } },
					{ 'U', new byte[] { i++, 8 } },
					{ 'V', new byte[] { i++, 8 } },
					{ 'W', new byte[] { i++, 13 } },
					{ 'X', new byte[] { i++, 8 } },
					{ 'Y', new byte[] { i++, 7 } },
					{ 'Z', new byte[] { i++, 6 } },
					{ '0', new byte[] { i++, 6 } },
					{ '1', new byte[] { i++, 4 } },
					{ '2', new byte[] { i++, 7 } },
					{ '3', new byte[] { i++, 6 } },
					{ '4', new byte[] { i++, 7 } },
					{ '5', new byte[] { i++, 6 } },
					{ '6', new byte[] { i++, 6 } },
					{ '7', new byte[] { i++, 6 } },
					{ '8', new byte[] { i++, 6 } },
					{ '9', new byte[] { i++, 6 } },
					{ ',', new byte[] { i++, 2 } },
					{ '.', new byte[] { i++, 2 } },
					{ ':', new byte[] { i++, 2 } },
					{ '\"', new byte[] { i++, 5 } },
					{ '/', new byte[] { i++, 8 } },
					{ '_', new byte[] { i++, 6 } },
					{ '-', new byte[] { i++, 4 } },
					{ '!', new byte[] { i++, 2 } },
					{ '?', new byte[] { i++, 5 } },
					{ ';', new byte[] { i++, 2 } },
					{ '%', new byte[] { i++, 10 } },
					{ '$', new byte[] { i++, 6 } },
					{ '&', new byte[] { i++, 9 } },
					{ '#', new byte[] { i++, 0 } },
					{ '@', new byte[] { i++, 0 } },
					{ '(', new byte[] { i++, 0 } },
					{ ')', new byte[] { i++, 0 } },
					{ '[', new byte[] { i++, 0 } },
					{ ']', new byte[] { i++, 0 } },
					{ '{', new byte[] { i++, 0 } },
					{ '}', new byte[] { i++, 0 } },
					{ '=', new byte[] { i++, 0 } },
					{ '+', new byte[] { i++, 0 } },
					{ '\\', new byte[] { i++, 0 } },
					{ '\'', new byte[] { i++, 0 } },
					{ ' ', new byte[] { i++, 3 } },
				};
			}
			
			X = x;
			Y = y;
			Font = NormalFont;
			
			_material = r.GetMaterial("default");
		}
		
		public string Text
		{
			set
			{
				_text = value;
				
				_indices = new int[_text.Length];
				_xs = new int[_text.Length];
		
				
				int x = 0;
				for (int i = 0; i < _text.Length; ++i) 
				{
					int charw = 8;
					byte[] charInfo;
					
					if(Font.Table.TryGetValue(_text[i], out charInfo))
					{
						_indices[i] = charInfo[0];
						charw = charInfo[1];
					}
					
					_xs[i] = x;
					x += charw+1;
				}
				
				Width = x;
				
			}
		}
		
		public override void BeginDraw (Renderer r)
		{
			Tilemap tilemap;
			
			SpriteTemplate tmp = Font.Template;
			
			if(!r.Tilemaps.TryGetValue(tmp.TilemapName, out tilemap))
			{
				r.Tilemaps[tmp.TilemapName] = tilemap = new Tilemap (tmp.TilemapName);
			}
				
			GL.BindTexture (TextureTarget.Texture2D, tilemap.Texture);

			//Flags type = _flags & Drawable.Flags.TypeMask;
			
			GL.Enable (EnableCap.Blend);
			GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

			_material.Bind();
		}

		public override unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount)
		{
			SpriteTemplate tmp = Font.Template;
	
			Tilemap tilemap = r.Tilemaps[tmp.TilemapName];
			
			if(_text != null)
			for(int i = 0; i < _text.Length; ++i)
			{
				float x = X + _xs[i] - tmp.Offset.X;
				float y = Y - tmp.Offset.Y;
				float width = tmp.Rectangle.Width;
				float height = tmp.Rectangle.Height;
				
				// Calculate texcoords, with possible animation.
				int tx = tmp.Rectangle.X;
				int ty = tmp.Rectangle.Y;
				
				int frameOffset = Font.Template.FrameOffset;
				if(frameOffset == 0)
					frameOffset = Font.Template.Rectangle.Width;
				
				tx += _indices[i] * frameOffset;
				
				// If the animation would take us outside the right edge of the
				// texture, start on the next row.
				while (tx + width > tilemap.Width) 
				{
					tx -= (tilemap.Width / frameOffset) * frameOffset;
					ty += Font.Template.Rectangle.Height;
				}

				float s0 = tx * tilemap.PixelStepX;
				float t0 = ty * tilemap.PixelStepY;
				float s1 = (tx + width) * tilemap.PixelStepX;
				float t1 = (ty + height) * tilemap.PixelStepY;
				
				vertexData[vertexCount].Position.X = x;
				vertexData[vertexCount].Position.Y = y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = Color;
				vertexData[vertexCount].TexCoord.X = s0;
				vertexData[vertexCount].TexCoord.Y = t0;
				vertexData[vertexCount].TexCoord.Z = 0.0f;
				vertexCount++;

				vertexData[vertexCount].Position.X = x + width;
				vertexData[vertexCount].Position.Y = y;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = Color;
				vertexData[vertexCount].TexCoord.X = s1;
				vertexData[vertexCount].TexCoord.Y = t0;
				vertexData[vertexCount].TexCoord.Z = 0.0f;
				vertexCount++;

				vertexData[vertexCount].Position.X = x+width;
				vertexData[vertexCount].Position.Y = y+height;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = Color;
				vertexData[vertexCount].TexCoord.X = s1;
				vertexData[vertexCount].TexCoord.Y = t1;
				vertexData[vertexCount].TexCoord.Z = 0.0f;
				vertexCount++;

				vertexData[vertexCount].Position.X = x;
				vertexData[vertexCount].Position.Y = y+height;
				vertexData[vertexCount].Position.Z = 0.0f;
				vertexData[vertexCount].Color = Color;
				vertexData[vertexCount].TexCoord.X = s0;
				vertexData[vertexCount].TexCoord.Y = t1;
				vertexData[vertexCount].TexCoord.Z = 0.0f;
				vertexCount++;
			}
		}
		
		public override void EndDraw (Renderer r, int vertexCount)
		{
			GL.DrawArrays(BeginMode.Quads, 0, vertexCount);
		//	Material.Unbind();
		}
	}
}
