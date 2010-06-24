namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using Client.Graphics;

	public class VehicleHealthBars : GameObject
	{
		private static SpriteTemplate _healthBarTmp = new SpriteTemplate 
		{ 
			TilemapName = "units", 
			Rectangle = new Rectangle(944, 0, 32, 5), 
			Offset = new Point(16, 3), 
			FrameOffset = 7,
			VerticalAnimation = true
		};
		
		private Sprite _sprite;

		public override float Radius {
			get {
				return 0.0f;
			}
		}
		
		public VehicleHealthBars(Game game) : base(game,100)
		{
			_sprite = new Sprite(_healthBarTmp, 0, Priority.HealthBar);
		}
		
		public override void Render ()
		{
			var vehicles = _game.FindObjects(typeof(Vehicle));

			_sprite.Resize(0);

			foreach(Vehicle v in vehicles)
			{
				int hpFrame = (int)(29.0f * ((float)v.HitPoints)/v.MaxHitPoints);
				_sprite.Add(v.X, v.Y-20,hpFrame);
			}
			
			_game.Application.Renderer.AddDrawable(_sprite);
		}

	}
}
