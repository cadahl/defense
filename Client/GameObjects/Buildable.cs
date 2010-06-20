namespace Client.GameObjects
{
	using Client.Graphics;
	using System.Collections.Generic;
	
	public abstract class Buildable : GameObject
	{
		public class UpgradeInfo
		{
			public int Id;
			public string Caption;
			public int Price;
			public int Damage;
			public int ReloadTime;
			public int Range; 
			public Dictionary<string,SpriteTemplate> SpriteTemplates;
		}
		
		public UpgradeInfo[] Upgrades;

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
		
		public Buildable(Game game) : base(game,0) {}
		
		protected delegate void CurrentUpgradeChangedHandler();
		protected event CurrentUpgradeChangedHandler CurrentUpgradeChanged;
	}
}
