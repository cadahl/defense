namespace Client.Graphics 
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using OpenTK.Graphics.OpenGL;
	using Util;

	public class Background 
	{
	    private Renderer _r;
	    private float _hscroll;
	    private float _vscroll;
	    private int _width;
	    private int _height;
	    private List<MapBlock> _blocks;
	    private List<string> _tilemapNames;
		private Material _material;
		
		public bool Blended { get; set; }
	
	    public Background(Renderer r, Map m) 
		{
	        _r = r;
	        _hscroll = 0.0f;
	        _vscroll = 0.0f;
	        _width = m.Width;
	        _height = m.Height;
	
	        _blocks = m.Blocks;
	        _tilemapNames = m.TilemapNames;
			
			_material = new Material(r, "data/defaultvs.glsl", "data/defaultfs.glsl");
	    }
	
	    public Background(Renderer r, string tilemapName, int width, int height) 
		{
	        _r = r;
	        _hscroll = 0.0f;
	        _vscroll = 0.0f;
	        _width = width;
	        _height = height;
	
			_blocks = new List<MapBlock>(Enumerable.Repeat(MapBlock.Empty, width*height));
	        _tilemapNames = new List<string>();
			_tilemapNames.Add(tilemapName);
			
			_material = new Material(r, "data/defaultvs.glsl", "data/defaultfs.glsl");
	    }

		public void ClearTile(int x, int y)
		{
			_blocks[x + y * _width] = MapBlock.Empty;
		}
		
		public void SetTile(int x, int y, int tx, int ty)
		{
			_blocks[x + y * _width] = new MapBlock(BlockType.Solid, (ushort)(tx + ty * 16));
		}
		
		public float HScroll
		{
		    set 
			{
	        	if(value > 0 && value <= MaxHScroll)
	            	_hscroll = value;
	    	}
		    get 
			{
		        return _hscroll;
		    }
		}
		
		public float VScroll
		{
		    set 
			{
	        	if(value > 0 && value <= MaxVScroll)
	            	_vscroll = value;
	    	}
		    get 
			{
		        return _vscroll;
		    }
		}
	
	    public float MaxHScroll 
		{
			get 
			{
		        return _width * 32.0f - _r.Width;
			}
	    }

	    public float MaxVScroll 
		{
			get 
			{
		        return _height * 32.0f - _r.Height;
			}
	    }
		
	    public void Draw(long frameCount) 
		{
			GL.Enable(EnableCap.Texture2D);

			if(Blended)
				GL.Enable(EnableCap.Blend);
			else
				GL.Disable(EnableCap.Blend);
			
			
			_material.Bind();
				
	        DrawTiles();
			
			Material.Unbind();
	    }
	    
	    private void DrawTiles() 
		{
	        int hwholescroll = (int)(Math.Floor(_hscroll) / 32.0f);
	        int hpixelscroll = ((int)Math.Floor(_hscroll)) & 31;
	        int vwholescroll = (int)(Math.Floor(_vscroll) / 32.0f);
	        int vpixelscroll = ((int)Math.Floor(_vscroll)) & 31;
	
	        for(int tmi = 0; tmi < _tilemapNames.Count; ++tmi) 
			{
				Tilemap tilemap = null;
				if(!_r.Tilemaps.TryGetValue(_tilemapNames[tmi], out tilemap))
					continue;
				
				GL.BindTexture(TextureTarget.Texture2D, tilemap.Texture);
	            GL.Begin(BeginMode.Quads);
	            
				for (int y = 0; y <= (_r.Height / 32) + 1; y++) 
				{
	                for (int x = 0; x <= (_r.Width / 32) + 1; x++) 
					{
	                    int xwrapped = (x + hwholescroll) % _width;
	                    int ywrapped = (y + vwholescroll) % _height;
	                    MapBlock block = _blocks[ywrapped * _width + xwrapped];
	
	                    if(block.Type == BlockType.Empty)
							continue;
	
	                    int tx = block.TileIndex & 0xF;
	                    int ty = (block.TileIndex >> 4) & 0xF;
	
	                    float s0 = tx * tilemap.TileStepX;
	                    float t0 = ty * tilemap.TileStepY;
	                    float s1 = (tx + 1) * tilemap.TileStepX;
	                    float t1 = (ty + 1) * tilemap.TileStepY;
	
	                    int sx0 = (x * 32) - hpixelscroll;
	                    int sy0 = (y * 32) - vpixelscroll;
	                    int sx1 = (x * 32) - hpixelscroll + 32;
	                    int sy1 = (y * 32) - vpixelscroll + 32;
	                    
						GL.Color4(1.0f,1.0f,1.0f,1.0f); GL.TexCoord2(s0, t0); GL.Vertex2(sx0, sy0);
	                    GL.Color4(1.0f,1.0f,1.0f,1.0f); GL.TexCoord2(s1, t0); GL.Vertex2(sx1, sy0);
	                    GL.Color4(1.0f,1.0f,1.0f,1.0f); GL.TexCoord2(s1, t1); GL.Vertex2(sx1, sy1);
	                    GL.Color4(1.0f,1.0f,1.0f,1.0f); GL.TexCoord2(s0, t1); GL.Vertex2(sx0, sy1);
	                }
	            }
	            GL.End();
	        }
	    }
	}
}
