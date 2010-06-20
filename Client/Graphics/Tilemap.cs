namespace Client.Graphics 
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;
	using OpenTK.Graphics.OpenGL;
	using Util;
	
	public class Tilemap
	{
	    public int Width { get; private set; }
	    public int Height { get; private set; }
	    public int Texture { get; private set; }
	
	    public Tilemap(string path) 
	    {
	        Load("data/"+path+".png");
	    }
	
	    public float PixelStepX { get {
	        return 1.0f / Width;
	    }}
	
	    public float PixelStepY { get {
	        return 1.0f / Height;
	    }}
	
	    public float TileStepX { get {
	        return 32.0f / Width;
	    }}
	
	    public float TileStepY { get {
	        return 32.0f / Height;
	    }}
	
		private void Load( string path )
		{
			Bitmap bitmap = new Bitmap( Bitmap.FromFile(path) );
			Width = bitmap.Width;
			Height = bitmap.Height;

			Console.WriteLine("Tilemap: Loading " + path + " (" + Width + "x" + Height + ")...");

			// Not pow2? Then round up.
			if( (Width & (Width-1)) != 0 || (Height & (Height-1)) != 0 )
			{
				int width2 = Util.NextPow2(Width);
				int height2 = Util.NextPow2(Height);
				using( Bitmap bitmap2 = new Bitmap(width2,height2) )
				{
					for( int y = 0; y < Height; y++ )
					{
						for( int x = 0; x < Width; x++ )
						{
							bitmap2.SetPixel(x,y,bitmap.GetPixel(x,y));
						}
					}
					
					Width = width2;
					Height = height2;
					bitmap.Dispose();
					bitmap = bitmap2;
				}					

				Console.WriteLine("Tilemap: Warning, rounded up to " + Width + "x" + Height);
			}
				
			Texture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, Texture);

			BitmapData data = bitmap.LockBits( new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			bitmap.UnlockBits(data);
			bitmap.Dispose();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
		}
	}
}
