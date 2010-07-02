namespace Client.Sim
{
	using System;
	using System.Collections.Generic;
	using Client.Graphics;
	
	public abstract class Buildable : GameObject
	{
		public class UpgradeInfo
		{
			public int Level;
			public string TypeId;
			public string Caption;
			public int Price;
			public int Damage;
			public int ReloadTime;
			public int Range; 
			public Dictionary<string,SpriteTemplate> SpriteTemplates;
			public Type Type;
		}
		
		public List<UpgradeInfo> Upgrades;

		private int _currentUpgradeIndex;
		public int CurrentUpgradeIndex
		{
			get
			{
				return _currentUpgradeIndex;
			}
			set
			{
				_currentUpgradeIndex = value;

				var usc = CurrentUpgradeChanged;
				if(usc != null)
					usc();
			}
		}
		public UpgradeInfo CurrentUpgrade
		{ 
			get
			{
				return Upgrades[_currentUpgradeIndex];
			}
		}
		
		public Buildable(Game game, string typeId) : base(game,0,typeId) {}
		
		protected delegate void CurrentUpgradeChangedHandler();
		protected event CurrentUpgradeChangedHandler CurrentUpgradeChanged;
	}
}
