namespace Client.Graphics 
{

public class SpriteTemplate
{
    public string TilemapName { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int OffsetX { get; private set; }
    public int OffsetY { get; private set; }
    public int Flags { get; private set; }
	public int AnimationOffset { get; private set; }

    public const int ANIM_VERTICAL = 0x01; // If set, seqIndex will add a vertical offset for animations instead of horizontal.

    public SpriteTemplate(string tilemapName, int x, int y, int w, int h, int offsetX, int offsetY, int animationOffset, int flags) 
	{
        TilemapName = tilemapName;
        X = x;
        Y = y;
        Width = w;
        Height = h;
		OffsetX = offsetX;
		OffsetY = offsetY;
		Flags = flags;
		
		if(animationOffset == 0)
		{
			if((Flags & ANIM_VERTICAL) != 0)
				AnimationOffset = Height;
			else
				AnimationOffset = Width;
		}
		else
			AnimationOffset = animationOffset;
    }

    public SpriteTemplate(string tilemapName, int x, int y, int w, int h, int offsetX, int offsetY, int flags) : this(tilemapName,x,y,w,h,offsetX,offsetY,0,flags)
	{
	}
		
    public SpriteTemplate(string tilemapName, int x, int y, int w, int h, int flags) : this(tilemapName,x,y,w,h,w/2,h/2,flags)
	{
	}

    public SpriteTemplate(string tilemapName, int x, int y, int w, int h) : this(tilemapName,x,y,w,h,0) 
	{
    }

    public SpriteTemplate() : this("",0,0,16,16)
	{
    }
}
}
