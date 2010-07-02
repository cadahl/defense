namespace Client.Presentation
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Xml.Linq;
	using Client.Sim;
	using Client.Graphics;

	public class Config : Sim.BaseConfig
	{
		public IEnumerable<string> GetVehicleTypeIds()
		{
			return from v in Xml.Element("units").Elements("vehicle") select (string)v.Attribute("id");
		}

		public IEnumerable<string> GetBuildableTypeIds()
		{
			return from v in Xml.Element("units").Elements("buildable") select (string)v.Attribute("id");
		}
		
		public SpriteTemplate GetVehicleSpriteTemplate(string typeId)
		{
			var sprite = (from v in Xml.Element("units").Elements("vehicle")
			               from s in v.Elements("sprite")
			               where (string)v.Attribute("id") == typeId && (string)s.Attribute("id") == "vehicle"
			               select s)
							.Single();
			
			if(sprite == null)
				return null;
			
			return GetSpriteTemplate(sprite);
		}
		
	}
}
