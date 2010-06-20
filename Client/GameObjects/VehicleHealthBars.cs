namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using Client.Graphics;

	public class VehicleHealthBars : GameObject
	{
		private static SpriteTemplate _healthBarTmp = new SpriteTemplate("units", 944, 0, 32, 5, 16, 3, 7, SpriteTemplate.ANIM_VERTICAL);
		private Sprite _sprite;
		
		public VehicleHealthBars(Game game) : base(game,100)
		{
			_sprite = new Sprite(_healthBarTmp, 0, Priority.HealthBar);
		}
		
		public override void Render ()
		{
			var vehicles = _game.FindObjects(typeof(Vehicle));

			_sprite.Resize(vehicles.Count);
			
			int vi = 0;
			
			foreach(Vehicle v in vehicles)
			{
				int hpFrame = (int)(29.0f * ((float)v.HitPoints)/v.MaxHitPoints);
				_sprite[vi].X = v.X;
				_sprite[vi].Y = v.Y-20;
				_sprite[vi].Frame = (byte)hpFrame;
				vi++;
			}
			
			_game.Client.Renderer.AddDrawable(_sprite);
		}

	}
}
