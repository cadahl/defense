namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using Client;
	using Client.Graphics;
	using Util;

	public class Bullet : GameObject
	{
		private static SpriteTemplate _bulletTmp = new SpriteTemplate ("units", 118, 7, 13, 3, 0);
		private float _k;
		private float _v;
		private float _sourceX;
		private float _sourceY;
		private float _targetX;
		private float _targetY;
		private Sprite _sprite;
		private int _damage;
		
		public Bullet (Game game, float x, float y, float targetX, float targetY, int damage) : base(game,0)
		{
			X = x;
			Y = y;
			Width = 2;
			Height = 2;
			_damage = damage;

			float d = Util.Distance(x,y,targetX,targetY);
			_sourceX = x;
			_sourceY = y;
			_targetX = targetX;
			_targetY = targetY;
			_k = 0.0f;
			_v = 12.0f / d;
				
			_sprite = new Sprite(_bulletTmp, 0, 2);			
		}

		public override void Update (long ticks)
		{
			if(_k > 1.0f)
			{
				_game.RemoveObject (this);
				return;
			}

			_k += _v;
			X = _sourceX + (_targetX - _sourceX) * _k;
			Y = _sourceY + (_targetY - _sourceY) * _k;

			var c = Collider.Point(X,Y,HandleHits,this);
			c.FilterType = typeof(Vehicle);
			_game.AddCollider(c);
		}

		public void HandleHits(Collider c, List<ObjectAndDistance<GameObject>> gobs)
	    {
			foreach(var gob in gobs)
			{
				Vehicle v = (Vehicle)gob.Object;
				v.Damage(_damage);
			}

			_game.RemoveObject(this);
		}
		
		public override void Render ()
		{
			_sprite[0].X = X;
			_sprite[0].Y = Y;
			_sprite[0].Angle = (ushort)Util.DeltasToAngle(_targetX-_sourceX, _targetY-_sourceY);
			_game.Client.Renderer.AddDrawable(_sprite);
		}
	}
}
