namespace Client.Graphics
{
	using System;
	using System.Runtime.InteropServices;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex
	{
		public Vertex(Vector3 pos, Color4 color, Vector3 texcoord)
		{
			Position = pos;
			Color = color;
			TexCoord = texcoord;
		}
		
	    public Vector3 Position;
		public Color4 Color;
	    public Vector3 TexCoord;
	 
	    public static readonly int Stride = Marshal.SizeOf(default(Vertex));
	}
	 
	public sealed class VertexBuffer
	{
	    private int _id;
	 
		public static int MaxQuads = 4000;
		
	    public int Id
	    {
	        get 
	        {
	            // Create an id on first use.
	            if (_id == 0)
	            {
	                OpenTK.Graphics.GraphicsContext.Assert();
	                GL.GenBuffers(1, out _id);
	                if (_id == 0)
	                    throw new Exception("Could not create VBO.");
					
				    GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
					GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (10240 * Vertex.Stride), IntPtr.Zero, BufferUsageHint.StreamDraw);

	            }
	 
	            return _id;
	        }
	    }
	 
	    public VertexBuffer()
	    { 
		}
		
	    public unsafe Vertex* Map()
	    {
	        GL.EnableClientState(ArrayCap.VertexArray);
	        GL.EnableClientState(ArrayCap.ColorArray);
	        GL.EnableClientState(ArrayCap.TextureCoordArray);
	 
	        GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
	        GL.VertexPointer(3, VertexPointerType.Float, Vertex.Stride, new IntPtr(0));
	        GL.ColorPointer(4, ColorPointerType.Float, Vertex.Stride, new IntPtr(Vector3.SizeInBytes));
	        GL.TexCoordPointer(3, TexCoordPointerType.Float, Vertex.Stride, new IntPtr(Vector3.SizeInBytes + Vector4.SizeInBytes));
			
            // Tell OpenGL to discard old VBO when done drawing it and reserve memory _now_ for a new buffer.
            // without this, GL would wait until draw operations on old VBO are complete before writing to it
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (4 * MaxQuads * Vertex.Stride), IntPtr.Zero, BufferUsageHint.StreamDraw);
			
			IntPtr vertexDataPtr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
			return (Vertex*)vertexDataPtr.ToPointer();
	    }

		public void Unmap()
		{
			GL.UnmapBuffer(BufferTarget.ArrayBuffer);
		}
		
	}
}
