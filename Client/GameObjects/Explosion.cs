namespace Client.GameObjects
{
	using System;
	using System.Drawing;
	using Client;
	using Client.Graphics;
	using Util;

	public enum ExplosionType
	{
		Smoky
	}
	
	public class Explosion : GameObject
	{
		private SpriteTemplate _template;
		private int _lifeTime;
		private float _animSpeed;
		private int _frameCount;
		private ExplosionType _type;
		private int _frame;

		private Sprite _sprite;
		
		public override float Radius {
			get {
				return 0f;
			}
		}
		
		public Explosion (Game game, float x, float y, ExplosionType type) : base(game, 0)
		{
			X = x;
			Y = y;
			_animSpeed = 4f;
			_type = type;
			
			switch (_type) {

			case ExplosionType.Smoky:
				_template = new SpriteTemplate () { TilemapName = "units", Rectangle = new Rectangle(0, 832, 96, 96) };
				_frameCount = 16;
				_animSpeed = 4f;
				_lifeTime = -6;
				break;
			}
			
			_sprite = new Sprite(_template, 0, Priority.Explosion);
			_sprite[0].Angle = (ushort)Util.RandomInt(0,4095);
			_sprite[0].Scale = Util.Random(0.6f, 1.0f);
		}

		public override void Update (long ticks)
		{
			// Animationsframe baserad på hur länge vi levat.
			_frame = (int)((float)_lifeTime / _animSpeed);

			// Om animationen har spelats klart, ta bort oss och hoppa ur.
			if (_frame >= _frameCount) 
			{
				_game.RemoveObject (this);
				return;
			}
			
			_lifeTime++;
		}
		
		public override void Render()
		{
			Renderer r = _game.Application.Renderer;
			_sprite[0].X = X;
			_sprite[0].Y = Y;
			_sprite[0].Frame = (byte)Math.Max(0,_frame);
			_sprite[0].Color.A = 0.5f;

			if(_type == ExplosionType.Smoky)
			{
				float color = 5.0f-4.0f*Util.Smoothstep(-6, 0, _lifeTime);
				float darken = 1.0f;//1.0f-Util.Smoothstep(0,20,_lifeTime);
				float fade = 1.0f-Util.Smoothstep(20,40,_lifeTime);
				_sprite[0].Scale = Util.Smoothstep(-6, 3, _lifeTime);
				_sprite[0].Color.R = color * darken;
				_sprite[0].Color.G = color * darken;
				_sprite[0].Color.B = color * darken;
				_sprite[0].Color.A *= fade;
			}
			
			r.AddDrawable(_sprite);
		}
	}
}
