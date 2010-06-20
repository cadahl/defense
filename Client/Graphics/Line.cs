namespace Client.Graphics
{
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;

	public class Line : Drawable
	{
		public Color4 Color = Color4.White;
		public float X, Y, X2, Y2;
		private float _a, _a2;

		public Line (float x, float y, float x2, float y2, float a, float a2, Flags flags, int priority) : base(flags, priority << 16)
		{
			X = x;
			Y = y;
			X2 = x2;
			Y2 = y2;
			_a = a;
			_a2 = a2;
		}

		public override void BeginDraw (Renderer r)
		{
			GL.Enable (EnableCap.Blend);
			GL.Disable (EnableCap.Texture2D);
		}

		public override void EndDraw(Renderer r, int vertexCount)
		{
			GL.DrawArrays(BeginMode.Lines, 0, vertexCount);
			
			GL.Enable (EnableCap.Texture2D);
		}
		
		public override unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount)
		{
			Background layer = r.Backgrounds[0];
			float x = X - layer.HScroll;
			float y = Y - layer.VScroll;
			float x2 = X2 - layer.HScroll;
			float y2 = Y2 - layer.VScroll;

			vertexData[vertexCount].Position.X = x;
			vertexData[vertexCount].Position.Y = y;
			vertexData[vertexCount].Color = new Color4( _a * Color.R, _a * Color.G, _a * Color.B, _a * Color.A);
			vertexCount++;
			vertexData[vertexCount].Position.X = x2;
			vertexData[vertexCount].Position.Y = y2;
			vertexData[vertexCount].Color = new Color4( _a2 * Color.R, _a2 * Color.G, _a2 * Color.B, _a2 * Color.A);
			vertexCount++;
		}
	}
}
