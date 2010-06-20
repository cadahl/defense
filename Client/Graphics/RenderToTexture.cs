namespace Client.Graphics
{
	using System;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;

	public class RenderToTexture : IDisposable
	{
		private bool _fallback;
		private int _subWidth, _subHeight;
		private int _width, _height;
		private uint _fboHandle;
		private bool _nonPowerOfTwo;
		public uint ColorTexture;


        // Track whether Dispose has been called.
        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Clean up what we allocated before exiting
                    if (ColorTexture != 0)
                        GL.DeleteTextures(1, ref ColorTexture);

                    if (_fboHandle != 0)
                        GL.DeleteFramebuffers(1, ref _fboHandle);

                    _disposed = true;
                }
            }
        }
		
		public Vector4 TextureCoords
		{
			get 
			{
				return new Vector4(0.0f, 0.0f, ((float)_subWidth)/_width, ((float)_subHeight)/_height);
			}
		}
		
		public RenderToTexture (int width, int height)
		{
			_nonPowerOfTwo = GL.GetString(StringName.Extensions).Contains("ARB_texture_non_power_of_two");
			Console.WriteLine("Using non-pow2 textures: " + _nonPowerOfTwo);
			
			_subWidth = width;
			_subHeight = height;
			_width = _nonPowerOfTwo? width : Util.Util.NextPow2(width);
			_height = _nonPowerOfTwo? height : Util.Util.NextPow2(height);
			
			_fallback = true;
			
			if(_fallback)
			{
				GL.GenTextures( 1, out ColorTexture );
				GL.BindTexture( TextureTarget.Texture2D, ColorTexture );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge );
				GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero );
				GL.BindTexture( TextureTarget.Texture2D, 0 ); // prevent feedback, reading and writing to the same image is a bad idea
			}
			else
			{
				// Create Color Texture
				GL.GenTextures( 1, out ColorTexture );
				GL.BindTexture( TextureTarget.Texture2D, ColorTexture );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge );
				GL.TexParameter( TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1 );
				GL.TexImage2D( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero );
				 
				// test for GL Error here (might be unsupported format)
				 
				GL.BindTexture( TextureTarget.Texture2D, 0 ); // prevent feedback, reading and writing to the same image is a bad idea
				 
				// Create Depth Renderbuffer
//				GL.GenRenderbuffers( 1, out _depthRenderBuffer );
//				GL.BindRenderbuffer( RenderbufferTarget.Renderbuffer, _depthRenderBuffer );
//				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, (RenderbufferStorage)All.DepthComponent32, _width, _height);
				 
				// test for GL Error here (might be unsupported format)
	 
				// Create a FBO and attach the textures
				GL.GenFramebuffers( 1, out _fboHandle );
				GL.BindFramebuffer( FramebufferTarget.Framebuffer, _fboHandle );
				GL.FramebufferTexture2D( FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorTexture, 0 );
//				GL.FramebufferRenderbuffer( FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthRenderBuffer );
	 
				// now GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer ) can be called, check the end of this page for a snippet.
	
				if(!CheckFboStatus())
					throw new Exception("Bah");
			}
		}
		
		
		public void Begin()
		{
			if(!_fallback)
			{
				GL.BindFramebuffer( FramebufferTarget.Framebuffer, _fboHandle );
				GL.PushAttrib( AttribMask.ViewportBit ); // stores GL.Viewport() parameters
				GL.Viewport( 0, 0, _subWidth, _subHeight );
			}
		}

		public void End()
		{
			if(_fallback)
			{
				GL.BindTexture(TextureTarget.Texture2D, ColorTexture);
				GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 0, 0, _width, _height, 0);
			}
			else
			{
				GL.PopAttrib( ); // restores GL.Viewport() parameters
				GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 ); // return to visible framebuffer
			}
		}
		
		private bool CheckFboStatus( )
        {
            switch ( GL.CheckFramebufferStatus( FramebufferTarget.Framebuffer ) )
            {
            case FramebufferErrorCode.FramebufferComplete:
                {
                    Console.WriteLine( "FBO: The framebuffer is complete and valid for rendering." );
                    return true;
                }
            case FramebufferErrorCode.FramebufferIncompleteAttachment:
                {
                    Console.WriteLine( "FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume." );
                    break;
                }
            case FramebufferErrorCode.FramebufferIncompleteMissingAttachment:
                {
                    Console.WriteLine( "FBO: There are no attachments." );
                    break;
                }
            /* case  FramebufferErrorCode.GL_FRAMEBUFFER_INCOMPLETE_DUPLICATE_ATTACHMENT_EXT: 
                 {
                     Console.WriteLine("FBO: An object has been attached to more than one attachment point.");
                     break;
                 }*/
/*            case FramebufferErrorCode.FramebufferIncompleteDimensions:
                {
                    Console.WriteLine( "FBO: Attachments are of different size. All attachments must have the same width and height." );
                    break;
                }
            case FramebufferErrorCode.FramebufferIncompleteFormats:
                {
                    Console.WriteLine( "FBO: The color attachments have different format. All color attachments must have the same format." );
                    break;
                }
  */          case FramebufferErrorCode.FramebufferIncompleteDrawBuffer:
                {
                    Console.WriteLine( "FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment." );
                    break;
                }
            case FramebufferErrorCode.FramebufferIncompleteReadBuffer:
                {
                    Console.WriteLine( "FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment." );
                    break;
                }
            case FramebufferErrorCode.FramebufferUnsupported:
                {
                    Console.WriteLine( "FBO: This particular FBO configuration is not supported by the implementation." );
                    break;
                }
            default:
                {
                    Console.WriteLine( "FBO: Status unknown. (yes, this is really bad.)" );
                    break;
                }
            }
            return false;
        }		
	}
}
