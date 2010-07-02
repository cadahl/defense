namespace Client.UI
{
	using System;
	using OpenTK.Graphics;
	using Client.Sim;
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

		public ToolbarEntry(Game game, string buildTypeId, int priority)
		{
			_game = game;

			if(!string.IsNullOrEmpty(buildTypeId))
			{
				//BuildTypeId = buildTypeId;
				Buildable = _game.Config.GetBuildableUpgrade(buildTypeId,0);

				SpriteTemplate st;
				if(Buildable.SpriteTemplates.TryGetValue("weapon", out st))
				{
					WeaponIcon = new Sprite(st, Drawable.Flags.NoScroll | Drawable.Flags.Colorize, priority);
				}
			}
		}


		//public string BuildTypeId { get; private set; }
		public BuildablePulloutOptions PulloutOptions;
		public int CursorFrame;
		public Sprite WeaponIcon { get; private set; }
		public Buildable.UpgradeInfo Buildable { get; private set; }
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
		public Buildable.UpgradeInfo SelectedBuildable { get { return SelectedIcon != null ? SelectedIcon.Buildable : null; } }

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

			_baseIcon = new Sprite(_game.Config.GetWidgetTemplate("baseicon"), Drawable.Flags.NoScroll|Drawable.Flags.Colorize, _basePriority+1);
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
			Icons[(int)ToolbarEntries.Wall] = new ToolbarEntry(_game, "wall", _basePriority+2)
			{
				CursorFrame = 2,
				PulloutOptions = BuildablePulloutOptions.ShowCaption|BuildablePulloutOptions.ShowPrice|BuildablePulloutOptions.ShowIconTab
			};
			Icons[(int)ToolbarEntries.Machinegun] = new ToolbarEntry(_game, "machinegun", _basePriority+2)
			{
				CursorFrame = 1,
				PulloutOptions = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab,
			};
			Icons[(int)ToolbarEntries.Cannon] = new ToolbarEntry(_game, "cannon", _basePriority+2)
			{
				CursorFrame = 1,
				PulloutOptions = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab,
			};
			Icons[(int)ToolbarEntries.Flamethrower] = new ToolbarEntry(_game, "flamethrower", _basePriority+2)
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
				var icon = Icons[iconi];
				var b = icon.Buildable;
				var u = b != null ? _game.Config.GetBuildableUpgrade(b.TypeId,0) : null;
				
				bool isSelected = SelectedIconIndex == iconi;
				
				Color4 c = isSelected ? (iconi == 0 ? SelectedMarkerIconColor : SelectedBuildableIconColor) : Color4.Black;
				c = _game.CanAfford(b) ? c : TooExpensiveBuildableIconColor;
				
				_baseIcon[iconi].X = IconLeft + IconSpacing * iconi; 
				_baseIcon[iconi].Y = _toolbarPanel.Y + Height/2;
				_baseIcon[iconi].Frame = (byte)icon.CursorFrame;
				_baseIcon[iconi].Color = c;
				
				if(icon.WeaponIcon != null)
				{
					icon.WeaponIcon[0].X = _baseIcon[iconi].X;
					icon.WeaponIcon[0].Y = _baseIcon[iconi].Y;
					icon.WeaponIcon[0].Color = c;
					r.AddDrawable(icon.WeaponIcon);
				}					
			}	

			r.AddDrawable(_baseIcon);
		}
	}
}
