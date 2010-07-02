namespace Client.Sim
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;
	using Client.Sim;
	using Client.Graphics;

	public class BaseConfig
	{
		public XElement Xml { get; private set; }

		public BaseConfig()
		{
			Xml = XElement.Load("data/game.xml");
		}

		protected SpriteTemplate GetSpriteTemplate(XElement sprite)
		{
			var st = new SpriteTemplate();
			st.TilemapName = "units";
			st.Rectangle.X = (int?)sprite.Attribute("x") ?? st.Rectangle.X;
			st.Rectangle.Y = (int?)sprite.Attribute("y") ?? st.Rectangle.Y;
			st.Rectangle.Width = (int?)sprite.Attribute("w") ?? st.Rectangle.Width;
			st.Rectangle.Height = (int?)sprite.Attribute("h") ?? st.Rectangle.Height;
			st.Offset.X = (int?)sprite.Attribute("cx") ?? st.Offset.X;
			st.Offset.Y = (int?)sprite.Attribute("cy") ?? st.Offset.Y;
			st.Centered = (bool?)sprite.Attribute("center") ?? st.Centered;
			st.FrameOffset = (int?)sprite.Attribute("frameoffset") ?? st.FrameOffset;
			return st;
		}		

		public SpriteTemplate GetWidgetTemplate(string name)
		{
			var widget = Xml.Element("hud").Elements("widget").Where(wi => (string)wi.Attribute("id") == name).FirstOrDefault();
			
			if(widget == null)
				return null;
			
			return GetSpriteTemplate(widget);
		}
		
		private Dictionary<string,List<Buildable.UpgradeInfo>> _upgradeInfoCache = new Dictionary<string, List<Buildable.UpgradeInfo>>();
		
		public List<Buildable.UpgradeInfo> GetBuildableUpgrades(string typeName)
		{
			if(string.IsNullOrEmpty(typeName))
				return null;

			typeName = typeName.ToLowerInvariant();

			List<Buildable.UpgradeInfo> upgrades;

			if(!_upgradeInfoCache.TryGetValue(typeName, out upgrades))
			{
				var buildable = Xml.Element("units").Elements("buildable").Single(b => (string)b.Attribute("id") == typeName);

				var stdict = buildable.Elements("sprite").ToDictionary( 	s => (string)s.Attribute("id"), 
				                                                       		s => GetSpriteTemplate(s));

				var type = Type.GetType((string)buildable.Attribute("fullname"));
				
				upgrades = (
					from u in buildable.Elements("upgrade")
					select new Buildable.UpgradeInfo()
	               	{
						Level = (int)u.Attribute("id"),
						TypeId = typeName,
						Caption = (string)u.Attribute("caption"),
						Price = (int)u.Attribute("price"),
						Damage = (int)u.Attribute("damage"),
						ReloadTime = (int)u.Attribute("reloadtime"),
						Range = (int)u.Attribute("range"),
						SpriteTemplates = stdict,
						Type = type
					}).ToList();

				_upgradeInfoCache[typeName] = upgrades;
			}
			
			return upgrades;
		}

		public Buildable.UpgradeInfo GetBuildableUpgrade(string typeId, int upgradeLevel)
		{
			if(string.IsNullOrEmpty(typeId))
				return null;
			
			return GetBuildableUpgrades(typeId).SingleOrDefault(u => u.Level == upgradeLevel);
		}	
	}
	
	public class Config : BaseConfig
	{
	

	}
}
