namespace Client
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;
	using Client.GameObjects;
	using Client.Graphics;

	public partial class Game
	{
		public XElement Config;

		private void LoadConfig ()
		{
			Config = XElement.Load("data/game.xml");
		}
		
		public SpriteTemplate GetWidgetTemplate(string name)
		{
			var widget = Config.Element("hud").Elements("widget").Where(wi => (string)wi.Attribute("id") == name).Single();
			
			var st = new SpriteTemplate();
			st.TilemapName = "units";
			st.Rectangle.X = (int?)widget.Attribute("x") ?? st.Rectangle.X;
			st.Rectangle.Y = (int?)widget.Attribute("y") ?? st.Rectangle.Y;
			st.Rectangle.Width = (int?)widget.Attribute("w") ?? st.Rectangle.Width;
			st.Rectangle.Height = (int?)widget.Attribute("h") ?? st.Rectangle.Height;
			st.Offset.X = (int?)widget.Attribute("cx") ?? st.Offset.X;
			st.Offset.Y = (int?)widget.Attribute("cy") ?? st.Offset.Y;
			st.Centered = (bool?)widget.Attribute("center") ?? st.Centered;
			st.FrameOffset = (int?)widget.Attribute("frameoffset") ?? st.FrameOffset;
			
			return st;
		}
		
		private Dictionary<Type,Buildable.UpgradeInfo[]> _upgradeInfoCache = new Dictionary<Type, Buildable.UpgradeInfo[]>();
		
		public Buildable.UpgradeInfo[] GetUpgradeInfos(Type type)
		{
			if(type == null)
				return null;

			string typeName = type.Name.ToLowerInvariant();

			Buildable.UpgradeInfo[] upgrades;

			if(!_upgradeInfoCache.TryGetValue(type, out upgrades))
			{
				var buildable = Config.Element("units").Elements("buildable").Where(b => (string)b.Attribute("id") ==	typeName).Single();

				var stdict = buildable.Elements("sprite").ToDictionary( 	s => (string)s.Attribute("id"), 
				                                                       		s => new SpriteTemplate() 
				                                                       		{
																				TilemapName = "units",
												         					    Rectangle = new Rectangle((int)s.Attribute("sx"), (int)s.Attribute("sy"), (int)s.Attribute("sw"), (int)s.Attribute("sh")),
												                                Offset = new Point((int)s.Attribute("scx"),	(int)s.Attribute("scy"))
																			});
				
				upgrades = (
					from u in buildable.Elements("upgrade")
					select new Buildable.UpgradeInfo()
	               	{
						Id = (int)u.Attribute("id"),
						Caption = (string)u.Attribute("caption"),
						Price = (int)u.Attribute("price"),
						Damage = (int)u.Attribute("damage"),
						ReloadTime = (int)u.Attribute("reloadtime"),
						Range = (int)u.Attribute("range"),
						SpriteTemplates = stdict
					}).ToArray();

				_upgradeInfoCache[type] = upgrades;
			}
			
			return upgrades;
		}

		public Buildable.UpgradeInfo GetUpgradeInfo(Type type, int upgradeLevel)
		{
			var us = GetUpgradeInfos(type);

			if(us == null || upgradeLevel < 0 || upgradeLevel >= us.Length)
				return null;

			return us[upgradeLevel];
		}

	}
}
