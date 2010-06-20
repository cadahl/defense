
using System;

namespace Client.Graphics
{
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	
	public class Circle : Drawable
	{
		public float X;
		public float Y;
		public float Radius;
		public Color4 Color = Color4.White;
		
		private static Material _material;
		private uint _priorityUid;
		private float _thickness = 0.0f;
		private float _aawidth = (float)Math.Sqrt(2.0f);

		public Circle (float x, float y, float radius, int priority) : base(0,priority)
		{
			X = x;
			Y = y;
			Radius = radius;
			_priorityUid = (uint)GetHashCode();
		}
		
		public override void BeginDraw (Renderer r)
		{
			if(_material == null)
			{
				_material = new Material(r, "data/circlevs.glsl", "data/circlefs.glsl");
			}

			GL.Enable(EnableCap.Blend);
			GL.Disable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			_material.Bind();
			float cx = X - r.Backgrounds[0].HScroll;
			float cy = Y - r.Backgrounds[0].VScroll;
			Vector3 p = new Vector3(cx,cy,Radius);
			_material.Uniform3("circle", ref p);
			
			Vector2 le = new Vector2(_thickness/2.0f,-_thickness/2.0f-_aawidth);
			Vector2 he = new Vector2(_thickness/2.0f+_aawidth,-_thickness/2.0f);
			_material.Uniform2("lowedge", ref le);
			_material.Uniform2("highedge", ref he);
			_material.Color4("color", ref Color);
		}
		
		public override ulong FinalPriority 
		{
			get 
			{
				return (ulong)(base.FinalPriority | _priorityUid);
			}
		}


		public override void EndDraw (Renderer r, int vertexCount)
		{
			GL.DrawArrays(BeginMode.Quads, 0, vertexCount);
			Material.Unbind();
		}
		
		public override unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount)
		{
			float cx = X - r.Backgrounds[0].HScroll;
			float cy = Y - r.Backgrounds[0].VScroll;

			float bbl = cx - Radius - _thickness/2.0f - _aawidth*2.0f;
			float bbr = cx + Radius + _thickness/2.0f + _aawidth*2.0f;
			float bbt = cy - Radius - _thickness/2.0f - _aawidth*2.0f;
			float bbb = cy + Radius + _thickness/2.0f + _aawidth*2.0f;

			int subdivs = 5;
			
			float suby = bbt;
			float xstep = (bbr-bbl)/subdivs;
			float ystep = (bbb-bbt)/subdivs;
			float halfdiagonal = (float)Math.Sqrt(xstep*xstep+ystep*ystep)*0.5f;
			
			Vertex* v = &vertexData[vertexCount];

			for(int y = 0; y < subdivs; ++y)
			{
				float subx = bbl;
				
				for(int x = 0; x < subdivs; ++x)
				{
					Vector2 subcenter = new Vector2(subx + xstep/2.0f - cx, suby + ystep/2.0f - cy);
					float subdistance = subcenter.Length;
					
					if(Math.Abs(subdistance-Radius) < halfdiagonal)
					{
						float subl = subx;
						float subr = subx+xstep;
						float subt = suby;
						float subb = suby+ystep;
						
						v->Position.X = v->TexCoord.X = subl;
						v->Position.Y = v->TexCoord.Y = subt;
						v->Position.Z = 0.0f;
						v++;
						v->Position.X = v->TexCoord.X = subr;
						v->Position.Y = v->TexCoord.Y = subt;
						v->Position.Z = 0.0f;
						v++;
						v->Position.X = v->TexCoord.X = subr;
						v->Position.Y = v->TexCoord.Y = subb;
						v->Position.Z = 0.0f;
						v++;
						v->Position.X = v->TexCoord.X = subl;
						v->Position.Y = v->TexCoord.Y = subb;
						v->Position.Z = 0.0f;
						v++;
						
						vertexCount += 4;
					}
					
					subx += xstep;
				}
				
				suby += ystep;
			}
			
			
		}
		


	}
}
