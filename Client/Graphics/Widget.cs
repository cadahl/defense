
namespace Client.Graphics
{
	using System;
	using Client.UI;

	public enum WidgetFlags
	{
		LevelCoordinates = 1
	}

	public class Widget : Sprite
	{
		public void SetFlags(WidgetFlags flags)
		{
			if((flags & WidgetFlags.LevelCoordinates) != 0)
				base._flags &= ~Drawable.Flags.NoScroll;
		}

		public void ClearFlags(WidgetFlags flags)
		{
			if((flags & WidgetFlags.LevelCoordinates) != 0)
				base._flags |= Drawable.Flags.NoScroll;
		}

		public int X
		{
			set
			{
				_instances[0].X = _instances[1].X = value;
			}
			get
			{
				return (int)_instances[0].X;
			}
		}

		public int Y
		{
			set
			{
				_instances[0].Y = _instances[1].Y = value;
			}
			get
			{
				return (int)_instances[0].Y;
			}
		}

		public int Width { get { return Template.Width; } }
		public int Height { get { return Template.Height; } }

		private Game _game;


		public Widget(SpriteTemplate template, Game game, int priority) : base(template, 0, priority)
		{
			_game = game;
			Add(0,0,1);
		}

		public void Update()
		{
			if(IsMouseOver)
				_instances[1].Flags &= ~SpriteFlags.Disable;
			else
				_instances[1].Flags |= SpriteFlags.Disable;
		}

		public bool IsMouseOver
		{
			get
			{
				int left = X;
				int top = Y;
				int mx = _game.Application.Input.MouseX;
				int my = _game.Application.Input.MouseY;

				if((_flags & Drawable.Flags.NoScroll) == 0)
				{
					mx = (int)_game.LevelMouseX;
					my = (int)_game.LevelMouseY;
				}

				if((_flags & 0) != 0)
				{
					left -= Template.OffsetX;
					top -= Template.OffsetY;
				}

				int right = left + Template.Width;
				int bottom = top + Template.Height;

				return mx >= left && mx < right && my >= top && my < bottom;
			}
		}

	}
}
