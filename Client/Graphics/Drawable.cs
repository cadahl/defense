namespace Client.Graphics
{

	using System;

	/**
 * Base class for "drawables", objects that the renderer can sort and finally draw.
 * @author aav
 */
	public abstract class Drawable : IComparable<Drawable>
	{
		[Flags]
		public enum Flags : uint
		{
			NoScroll = 0x4,
		//	UseOffset = 0x8,
			Colorize = 0x10,
			TypeNormal = 0x000,
			TypeNoBlend = 0x100,
			TypeMask = 0xf00
		}

		protected Flags _flags;
		public int Priority { get; set; }

		public Drawable (Flags flags, int priority)
		{
			_flags = flags;
			Priority = priority;
			
			//Angle = 0;
			//Scale = 1f;
		}

		public abstract void BeginDraw (Renderer r);
		public abstract unsafe void Draw (Renderer r, Vertex* vertexData, ref int vertexCount);
		public abstract void EndDraw (Renderer r, int vertexCount);

		public virtual ulong FinalPriority 
		{
			get
			{
				//ulong type = (ulong)(_flags & Flags.TypeMask) >> 8;
				return (ulong)(((ulong)Priority) << 32);
			}
		}
		
		public int CompareTo (Drawable other)
		{
			return FinalPriority.CompareTo(other.FinalPriority);
		}

		public void SetFlags (Flags flags)
		{
			_flags |= flags;
		}

		public void ClearFlags (Flags flags)
		{
			_flags &= ~flags;
		}

	}
}
