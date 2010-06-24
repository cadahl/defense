namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client;
	using Client.Graphics;
	using Util;

	public class InstantBullet : GameObject
	{
		private int _lifeTime, _maxLifeTime;
		private float _sourceX, _sourceY;
		private float _streakStart, _streakMid, _streakEnd;
		private int _damage;

		public override float Radius {
			get {
				return 16.0f;
			}
		}
		
		public InstantBullet (Game game, float x, float y, float sourceX, float sourceY, int damage) : base( game, 0)
		{
			X = x;
			Y = y;
			
			_sourceX = sourceX;
			_sourceY = sourceY;
			_damage = damage;

			// Slumpa animationshastigheten en aning. Mellan 4-6 ca.
			//_animSpeed = 1 + (int)(Util.Random () * 3);
			_maxLifeTime = 25;
			_lifeTime = 0;
			
			float p1 = 0.4f + (float)Util.Random () * 0.5f - 0.25f;
			// punkt 1: 0.15-
			float p2 = 0.9f + (float)Util.Random () * 0.3f - 0.2f;
			
			_streakStart = Math.Min (p1, p2);
			_streakEnd = Math.Max (p1, p2);
			_streakMid = _streakStart + (_streakEnd - _streakStart) * 0.7f;

			var c = Collider.LineFirst(sourceX, sourceY, x, y, HandleHit, this);
			c.FilterType = typeof(Vehicle);
			_game.AddCollider(c);
		}
		
		public void HandleHit(Collider c, IEnumerable<ObjectAndDistance> gobs)
		{
			Vehicle v = (Vehicle)gobs.Single().Object;
			if(v != null)
				v.Damage(_damage);
		}

		public override void Update(long ticks)
		{
			if (_lifeTime >= _maxLifeTime) 
			{
				_game.RemoveObject (this);
				return;
			}
			
			_lifeTime++;
		}
		
		public override void Render()
		{
			// Basera hur solid (ljusstark) linjen är på linjens längd (*1.2) minus Bulletens livstid
			float brightness = (float)Math.Max (0.0, 1.2 * (_streakEnd - _streakStart) - _lifeTime * 0.1);
			
			if(brightness >= 0.05)
			{
				// Första delen av linjen, genomskinlig -> solid
//				_game.Client.Renderer.DrawLine ((int)(_sourceX + (X - _sourceX) * _streakStart), (int)(_sourceY + (Y - _sourceY) * _streakStart), (int)(_sourceX + (X - _sourceX) * _streakMid), (int)(_sourceY + (Y - _sourceY) * _streakMid), 0.0f, brightness, _drawFlags, Priority.VehicleHitEffect);
			
				// Andra delen av linjen, solid -> genomskinlig
				//_game.Client.Renderer.DrawLine ((int)(_sourceX + (X - _sourceX) * _streakMid), (int)(_sourceY + (Y - _sourceY) * _streakMid), (int)(_sourceX + (X - _sourceX) * _streakEnd), (int)(_sourceY + (Y - _sourceY) * _streakEnd), brightness, 0.0f, _drawFlags, Priority.VehicleHitEffect);
			}
		}
	}
}
