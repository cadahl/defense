namespace Client.Graphics
{
	using System;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;

	[Flags]
	public enum Corners : int
	{
		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 4,
		BottomRight = 8,
		Top = TopLeft|TopRight,
		Bottom = BottomLeft|BottomRight,
		All = Top|Bottom,
		Left = TopLeft|BottomLeft,
		Right = TopRight|BottomRight,
	}
	
	public class Panel : Drawable
	{
		public float X 		{ get { return _x; } set { _x = value; _dirty = true; } }
		public float Y 		{ get { return _y; } set { _y = value; _dirty = true; } }
		public int Width 		{ get { return _width; } set { _width = value; _dirty = true; } }
		public int Height 		{ get { return _height; } set { _height = value; _dirty = true; } }
		public Color4 Color 	{ get { return _color; } set { _color = value; _dirty = true; } }
		public Corners Corners { get { return _corners; } set { _corners = value; _dirty = true; } }

		private float _x, _y;
		private int _width, _height;
		private Color4 _color = Color4.White;
		private Corners _corners = Corners.All;
		private Vertex[] _verts = new Vertex[9*4];
		private bool _dirty = true;
		
		private static Material _material;
		private static float _psx, _psy;
		
		public Panel (int priority) : base(Drawable.Flags.NoScroll,priority)
		{
		}

		public override void BeginDraw (Renderer r)
		{
			Tilemap tilemap = r.Tilemaps["units"];
			
			if(_material == null)
			{
				_material = r.GetMaterial("default");
				
				_psx = tilemap.PixelStepX;
				_psy = tilemap.PixelStepY;
				_material.SetTilemap("colortexture",tilemap);
			}

			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, tilemap.Texture);
			_material.Bind();
		}
		/*
		public override ulong FinalPriority 
		{
			get 
			{
				return (ulong)(base.FinalPriority | _priorityUid);
			}
		}
*/

		public override void EndDraw (Renderer r, int vertexCount)
		{
			GL.DrawArrays(BeginMode.Quads, 0, vertexCount);
//			Material.Unbind();
		}
		
		private void SetQuad(int qi, int x0, int y0, int x1, int y1, int u0, int v0, int u1, int v1)
		{
			_verts[qi*4+0].Position.X = x0;
			_verts[qi*4+0].Position.Y = y0;
			_verts[qi*4+0].Position.Z = 0.0f;
			_verts[qi*4+0].Color = Color;
			_verts[qi*4+0].TexCoord.X = u0*_psx;
			_verts[qi*4+0].TexCoord.Y = v0*_psy;
			_verts[qi*4+0].TexCoord.Z = 0.0f;
			
			_verts[qi*4+1].Position.X = x1;
			_verts[qi*4+1].Position.Y = y0;
			_verts[qi*4+1].Position.Z = 0.0f;
			_verts[qi*4+1].Color = Color;
			_verts[qi*4+1].TexCoord.X = u1*_psx;
			_verts[qi*4+1].TexCoord.Y = v0*_psy;
			_verts[qi*4+1].TexCoord.Z = 0.0f;

			_verts[qi*4+2].Position.X = x1;
			_verts[qi*4+2].Position.Y = y1;
			_verts[qi*4+2].Position.Z = 0.0f;
			_verts[qi*4+2].Color = Color;
			_verts[qi*4+2].TexCoord.X = u1*_psx;
			_verts[qi*4+2].TexCoord.Y = v1*_psy;
			_verts[qi*4+2].TexCoord.Z = 0.0f;
			
			_verts[qi*4+3].Position.X = x0;
			_verts[qi*4+3].Position.Y = y1;
			_verts[qi*4+3].Position.Z = 0.0f;
			_verts[qi*4+3].Color = Color;
			_verts[qi*4+3].TexCoord.X = u0*_psx;
			_verts[qi*4+3].TexCoord.Y = v1*_psy;
			_verts[qi*4+3].TexCoord.Z = 0.0f;
		}
		
		public override unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount)
		{
			if(_dirty)
			{
				_dirty = false;
				
				int xi = (int)X;
				int yi = (int)Y;
				int wi = (int)Width;
				int hi = (int)Height;
				int x0 = xi;
				int x1 = xi + 16;
				int x2 = xi + wi - 16;
				int x3 = xi + wi;
				int y0 = yi;
				int y1 = yi + 16;
				int y2 = yi + hi - 16;
				int y3 = yi + hi;

				SetQuad(1, x1, y0, x2, y1, 80, 325, 80, 325);
				SetQuad(3, x0, y1, x1, y2, 80, 325, 80, 325);
				SetQuad(4, x1, y1, x2, y2, 80, 325, 80, 325);
				SetQuad(5, x2, y1, x3, y2, 80, 325, 80, 325);
				SetQuad(7, x1, y2, x2, y3, 80, 325, 80, 325);

				if((_corners & Corners.TopLeft) != 0) SetQuad(0, x0, y0, x1, y1, 64, 320, 80, 336); else SetQuad(0, x0, y0, x1, y1, 80, 325, 80, 325);
				if((_corners & Corners.TopRight) != 0) SetQuad(2, x2, y0, x3, y1, 80, 320, 96, 336); else SetQuad(2, x2, y0, x3, y1, 80, 325, 80, 325);
				if((_corners & Corners.BottomLeft) != 0) SetQuad(6, x0, y2, x1, y3, 96, 320, 112, 336); else SetQuad(6, x0, y2, x1, y3, 80, 325, 80, 325);
				if((_corners & Corners.BottomRight) != 0) SetQuad(8, x2, y2, x3, y3, 112, 320, 128, 336); else SetQuad(8, x2, y2, x3, y3, 80, 325, 80, 325);
			}

			for(int i = 0; i < 9*4; ++i)
				vertexData[vertexCount++] = _verts[i];
		}
		


	}
}
