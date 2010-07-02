namespace Client.Sim
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using Client;
	using Client.Graphics;
	using Util;

	public class Bullet : GameObject
	{
		private static SpriteTemplate _bulletTmp = new SpriteTemplate { TilemapName="units", Rectangle = new Rectangle(118, 7, 13, 3) };
		private float _k;
		private float _v;
		private float _sourceX;
		private float _sourceY;
		private float _targetX;
		private float _targetY;
		private Sprite _sprite;
		private int _damage;

		public override float Radius {
			get {
				return 4.0f;
			}
		}
		
		public Bullet (Game game, float x, float y, float targetX, float targetY, int damage) : base(game,0,"bullet")
		{
			X = x;
			Y = y;
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

		public override ObjectStatePacket Update (long ticks)
		{
			if(_k > 1.0f)
			{
				_game.RemoveObject (this);
				return null;
			}

			_k += _v;
			X = _sourceX + (_targetX - _sourceX) * _k;
			Y = _sourceY + (_targetY - _sourceY) * _k;

			if((ticks % 2) == 0)
			{
				var c = Collider.Point(X,Y,HandleHits,this);
				c.FilterType = typeof(Vehicle);
				_game.AddCollider(c);
			}
			
			return NewState();
		}

		public void HandleHits(Collider c, IEnumerable<ObjectAndDistance> gobs)
	    {
			foreach(var gob in gobs)
			{
				Vehicle v = (Vehicle)gob.Object;
				v.Damage(_damage);

				_game.RemoveObject(this);
			}

		}
		
		public override void Render ()
		{
			_sprite[0].X = X;
			_sprite[0].Y = Y;
			_sprite[0].Angle = (ushort)Util.DeltasToAngle(_targetX-_sourceX, _targetY-_sourceY);
			_game.Application.Renderer.AddDrawable(_sprite);
		}
	}
}
