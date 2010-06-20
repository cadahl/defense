namespace Client
{
	using System;
	using System.Collections.Generic;
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
				                                                       		s => new SpriteTemplate("units",
												         					                        (int)s.Attribute("sx"),
												                            					    (int)s.Attribute("sy"),
												                                           			(int)s.Attribute("sw"),
												                                                    (int)s.Attribute("sh"),
												                                                    (int)s.Attribute("scx"),
																	                                (int)s.Attribute("scy"),0));
				
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
