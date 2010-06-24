namespace Client.Graphics
{
	using System.Drawing;
	
	public class SpriteTemplate
	{
		public string TilemapName;
		public Rectangle Rectangle;
		public Point Offset;
		public int FrameOffset = 0;
		public bool VerticalAnimation = false;
		public bool Centered = true;
	}
}
