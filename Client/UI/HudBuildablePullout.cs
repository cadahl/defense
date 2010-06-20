namespace Client.UI
{
	using System;
	using OpenTK;
	using OpenTK.Graphics;
	using Client.Graphics;
	using Client.GameObjects;
	using Util;
	
	[Flags]
	public enum BuildablePulloutOptions
	{
		ShowPrice = 1<<0,
		ShowDamage = 1<<1,
		ShowReloadTime = 1<<2,
		ShowRange = 1<<3,
		ShowCaption = 1<<4,
		ShowIconTab = 1<<5,
		ShowAllRows = ShowCaption|ShowPrice|ShowDamage|ShowReloadTime|ShowRange,
	}

	public class HudBuildablePullout
	{
		public Color4 Color = new Color4(0.07f, 0.07f, 0.07f, 1f);
		public Color4 CaptionColor = Color4.White;
		public Color4 LineColor = Color4.White;
		private static SpriteTemplate _infoIcons = new SpriteTemplate ("units", 0, 32, 22, 22, 0,0,22, 0);
		private static SpriteTemplate _captionIcons = new SpriteTemplate ("units", 0, 176, 16, 16, 0,0,0);
		private static Color4 _TooExpensiveTextColor = new Color4(235,35,35,255);

		public BuildablePulloutOptions Options = BuildablePulloutOptions.ShowAllRows;
		public int X, Y;
		public Buildable.UpgradeInfo Upgrade;
		public bool TooExpensive;
		public int CaptionIcon = -1;
		public string Caption = null;

		private Game _game;
		private int _basePriority;
		private Panel _panel;
		private Panel _iconPanel;
		private TextLine[] _text = new TextLine[5];
		private Sprite _icons;
		private Sprite _captionIcon;
		private Timer _stayUpTimer;
		private Timer _foldingTimer;

		public HudBuildablePullout(Game game, Renderer r, int basePriority)
		{
			_game = game;
			_basePriority = basePriority;
			
			_stayUpTimer = new Timer(TimerMode.CountDown, 60*3);
			_stayUpTimer.Elapsed += HandleStayUpTimerElapsed;
			_foldingTimer = new Timer(TimerMode.CountDown, 100);			
			_foldingTimer.Ticked += HandleFoldingTimerTicked;
			
			_panel = new Panel(_basePriority-1);
			_panel.Width = 96;
			_panel.Corners = Corners.Top|Corners.BottomRight;
			_panel.Color = Color;
			
			_iconPanel = new Panel(_basePriority-1);
			_iconPanel.Y = r.Height-HudToolbar.Height;
			_iconPanel.Width = 56;
			_iconPanel.Height = HudToolbar.Height;
			_iconPanel.Corners = Corners.Bottom;
			_iconPanel.Color = Color;

			_captionIcon = new Sprite(_captionIcons, Drawable.Flags.NoScroll, _basePriority+2);

			_icons = new Sprite(_infoIcons, Drawable.Flags.NoScroll, _basePriority+2);
			_icons.Resize(4);
			_icons[0].Frame = 3;
			_icons[1].Frame = 0;
			_icons[2].Frame = 2;
			_icons[3].Frame = 1;

			_text[0] = new TextLine(r,0,0,_basePriority+2);
			_text[1] = new TextLine(r,0,0,_basePriority+2);
			_text[2] = new TextLine(r,0,0,_basePriority+2);
			_text[3] = new TextLine(r,0,0,_basePriority+2);
			_text[4] = new TextLine(r,0,0,_basePriority+2);
		}

		private void HandleFoldingTimerTicked(Timer timer)
		{
		}
		
		private void HandleStayUpTimerElapsed (Timer timer)
		{
			_foldingTimer.ResetValue = _panel.Height;
			_foldingTimer.Mode = TimerMode.CountUp;
			_foldingTimer.Reset();
		}

		public void Render(Renderer r)
		{
			_stayUpTimer.Tick();
			_foldingTimer.Tick();
			
			const int minWidth = 96;
			const int leftMargin = 12;
			const int rightMargin = 12;
			const int topMargin = 12;
			const int bottomMargin = 8;
			const int leftTextMargin = 28;
			const int topTextMargin = 5;
			const int lineHeight = 24;
			
			int height = topMargin;
			if((Options & BuildablePulloutOptions.ShowCaption) != 0) height += lineHeight;
			if((Options & BuildablePulloutOptions.ShowPrice) != 0) height += lineHeight;
			if((Options & BuildablePulloutOptions.ShowDamage) != 0) height += lineHeight;
			if((Options & BuildablePulloutOptions.ShowReloadTime) != 0) height += lineHeight;
			if((Options & BuildablePulloutOptions.ShowRange) != 0) height += lineHeight;
			height += bottomMargin;

			int px = X;
			int py = Y - height;
			_panel.X = px;
			_panel.Y = py;
			_panel.Color = Color;
			_panel.Corners = Corners.Top | ((Options & BuildablePulloutOptions.ShowIconTab) != 0 ? Corners.BottomRight : Corners.Bottom);
			_panel.Height = height;
			r.AddDrawable(_panel);

			_iconPanel.X = px;
			_iconPanel.Y = py+height;
			_iconPanel.Color = Color;
			if((Options & BuildablePulloutOptions.ShowIconTab) != 0)
				r.AddDrawable(_iconPanel);

			var u = Upgrade;
			if(u != null)
			{
				r.AddDrawable(_icons);
			
				_text[0].Text = u.Price > 0 ? ""+u.Price : "";
				_text[1].Text = u.Damage > 0 ? u.Damage+" hp" : "";
				_text[2].Text = u.ReloadTime > 0 ? String.Format("{0:0.0}",((float)u.ReloadTime)/60.0f) + " s" : "";
				_text[3].Text = u.Range > 0 ? ""+u.Range : "";
				_text[4].Text = Caption ?? u.Caption;

				int y = 1;
				for(int i = 0; i < 4; ++i)
				{
					_icons[i].X = px+leftMargin;
					_icons[i].Y = py+topMargin+lineHeight*y;
					_text[i].X = px+leftMargin+leftTextMargin;
					_text[i].Y = py+topMargin+topTextMargin+lineHeight*y;
					
					if(((int)Options & (1<<i)) != 0)
					{
						r.AddDrawable(_text[i]);
						if(i<4) _icons[i].Flags &= ~SpriteFlags.Disable;
						y++;
					}
					else
					{
						if(i<4) _icons[i].Flags |= SpriteFlags.Disable;
					}
				}	
				

				_icons[0].Color = u.Price <= 0 ? Color4.Gray : (TooExpensive ? Color4.Gray : LineColor);
				_icons[1].Color = u.Damage <= 0 || TooExpensive ? Color4.Gray : LineColor;
				_icons[2].Color = u.ReloadTime <= 0 || TooExpensive ? Color4.Gray : LineColor;
				_icons[3].Color = u.Range <= 0 || TooExpensive ? Color4.Gray : LineColor;
				_text[0].Color = TooExpensive ? PulseColor(_TooExpensiveTextColor) : LineColor;
				_text[1].Color = TooExpensive ? Color4.Gray : LineColor;
				_text[2].Color = TooExpensive ? Color4.Gray : LineColor;
				_text[3].Color = TooExpensive ? Color4.Gray : LineColor;
				_text[4].Color = TooExpensive ? Color4.Gray : CaptionColor;
				
				_panel.Width = Math.Max(minWidth,_text[4].Width + leftMargin+rightMargin);
				//_panel.Corners = Corners.Top|Corners.BottomRight;
			}
			else
			{
				px--;
				_text[4].Text = "Select";
				_panel.Corners = Corners.Top;
				_panel.Width = _iconPanel.Width;
			}

			if(CaptionIcon >= 0)
			{
				_captionIcon[0].X = px+leftMargin+4;
				_captionIcon[0].Y = py+topMargin-2;
				_captionIcon[0].Frame = (byte)CaptionIcon;
				_captionIcon[0].Color.A = 0.0f;
				r.AddDrawable(_captionIcon);
				_text[4].X = (int)_captionIcon[0].X+22;
				_text[4].Y = py+topMargin;
			//	_text[4].Color = new Color4(246,233,185,255);

				_panel.Width += 28;
			}
			else
			{
				_text[4].X = px+leftMargin;
				_text[4].Y = py+topMargin;
			}
			r.AddDrawable(_text[4]);
		}
		
	
		private Color4 PulseColor(Color4 c)
		{
			float a = Util.Saturate((float)(0.75 + 0.25*Math.Sin(_game.UpdateCount*Math.PI/20.0)));
			c.R = a * c.R;
			c.G = a * c.G;
			c.B = a * c.B;
			return c;
		}
	}
}
