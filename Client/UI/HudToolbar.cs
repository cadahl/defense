namespace Client.UI
{
	using System;
	using OpenTK.Graphics;
	using Client.GameObjects;
	using Client.Graphics;

	public enum ToolbarEntries
	{
		None = 0,
		Wall = 1,
		Machinegun = 2,
		Cannon = 3,
		Flamethrower = 4,
		Count = 5,
	}

	public class ToolbarEntry
	{
		private Game _game;

		public ToolbarEntry(Game game, Type buildType, int priority)
		{
			_game = game;

			if(buildType != null)
			{
				BuildType = buildType;
				var u = _game.GetUpgradeInfo(buildType,0);

				SpriteTemplate st;
				if(u.SpriteTemplates.TryGetValue("weapon", out st))
				{
					WeaponIcon = new Sprite(st, Drawable.Flags.NoScroll | Drawable.Flags.Colorize, priority);
				}
			}
		}

		public bool CanBuy
		{
			get
			{
				var u = _game.GetUpgradeInfo(BuildType, 0);
				return u != null && u.Price <= _game.Cash;
			}
		}

		public int Range
		{
			get
			{
				var u = _game.GetUpgradeInfo(BuildType, 0);
				if(u != null)
					return u.Range;
				else
					return 0;
			}
		}

		public Type BuildType;
		public BuildablePulloutOptions PulloutOptions;
		public int CursorFrame;
		public Sprite WeaponIcon;
	}

	public class HudToolbar
	{
		public static int Height = 64;
		public static int IconSpacing = 64;
		public static int IconLeft = 48;

		private Game _game;
		private int _basePriority;
		private Sprite _baseIcon;
		private Panel _toolbarPanel;
		private TextLine _cashText;
		private Sprite[] _wpnIcons;

		public int SelectedIconIndex;
		public ToolbarEntry[] Icons;
		public ToolbarEntry SelectedIcon { get { return Icons[SelectedIconIndex]; } }

		public static Color4 PanelColor = new Color4(0.0f, 0.0f, 0.0f, .5f);
		public static Color4 SelectedBuildableIconColor = new Color4(.14f, .14f, .14f, 1f);
		public static Color4 TooExpensiveBuildableIconColor = new Color4(0.0f,0.0f,0.0f,.3f);
		public static Color4 SelectedMarkerIconColor = new Color4(0.2f, 0.4f, 0.1f, 1.0f);

		public HudToolbar (Game game, Renderer r, int basePriority)
		{
			_game = game;
			_basePriority = basePriority;

			_toolbarPanel = new Panel(_basePriority-2);
			_toolbarPanel.X = 0;
			_toolbarPanel.Y = r.Height-Height;
			_toolbarPanel.Width = r.Width;
			_toolbarPanel.Height = Height;
			_toolbarPanel.Color = PanelColor;
			_toolbarPanel.Corners = 0;

			_baseIcon = new Sprite(_game.GetWidgetTemplate("baseicon"), Drawable.Flags.NoScroll|Drawable.Flags.Colorize, _basePriority+1);
			_baseIcon.Resize((int)ToolbarEntries.Count);

			_cashText = new TextLine(r,0,0,basePriority+1);
			_cashText.Font = TextLine.CashFont;
			_cashText.Color = Color4.LightGoldenrodYellow;

			Icons = new ToolbarEntry[(int)ToolbarEntries.Count];
			Icons[(int)ToolbarEntries.None] = new ToolbarEntry(_game, null, _basePriority+2)
			{
				CursorFrame = 0,
				PulloutOptions = BuildablePulloutOptions.ShowCaption|BuildablePulloutOptions.ShowIconTab
			};
			Icons[(int)ToolbarEntries.Wall] = new ToolbarEntry(_game, typeof(Wall), _basePriority+2)
			{
				CursorFrame = 2,
				PulloutOptions = BuildablePulloutOptions.ShowCaption|BuildablePulloutOptions.ShowPrice|BuildablePulloutOptions.ShowIconTab
			};
			Icons[(int)ToolbarEntries.Machinegun] = new ToolbarEntry(_game, typeof(Machinegun), _basePriority+2)
			{
				CursorFrame = 1,
				PulloutOptions = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab,
			};
			Icons[(int)ToolbarEntries.Cannon] = new ToolbarEntry(_game, typeof(Cannon), _basePriority+2)
			{
				CursorFrame = 1,
				PulloutOptions = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab,
			};
			Icons[(int)ToolbarEntries.Flamethrower] = new ToolbarEntry(_game, typeof(Flamethrower), _basePriority+2)
			{
				CursorFrame = 1,
				PulloutOptions = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab,
			};
		}

		public void Render(Renderer r)
		{
			r.AddDrawable(_toolbarPanel);

			_cashText.Text = "$"+_game.Cash;
			_cashText.X = r.Width-_cashText.Width-16;
			_cashText.Y = r.Height-Height/2-11;
			r.AddDrawable(_cashText);

			for(int iconi = 0; iconi < Icons.Length; ++iconi)
			{
				bool isSelected = SelectedIconIndex == iconi;
				var u = _game.GetUpgradeInfo(Icons[iconi].BuildType,0);
				bool tooExpensive = u != null && u.Price > _game.Cash;
				
				Color4 c = isSelected ? (iconi==0 ? SelectedMarkerIconColor : SelectedBuildableIconColor) : Color4.Black;
				
				c = tooExpensive ? TooExpensiveBuildableIconColor : c;
				
				_baseIcon[iconi].X = IconLeft + IconSpacing * iconi; 
				_baseIcon[iconi].Y = _toolbarPanel.Y + Height/2;
				_baseIcon[iconi].Frame = (byte)Icons[iconi].CursorFrame;
				_baseIcon[iconi].Color = c;
				
				var wi = Icons[iconi].WeaponIcon;
				if(wi != null)
				{
					wi[0].X = _baseIcon[iconi].X;
					wi[0].Y = _baseIcon[iconi].Y;
					wi[0].Color = c;
					r.AddDrawable(wi);
				}					
			}	

			r.AddDrawable(_baseIcon);
		}
	}
}
