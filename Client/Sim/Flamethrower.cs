namespace Client.Sim
{
	using System;
	using System.Drawing;
	using Client.Graphics;
	using Util;

	public class Flamethrower : TowerUnit
	{
		private static SpriteTemplate _flameTemplate = new SpriteTemplate 
		{ 
			TilemapName = "flame", 
			Rectangle = new Rectangle (15, 0, 34, 64), 
			Offset = new Point (34 / 2, 32), 
			FrameOffset = 64 
		};

		private Sprite _weapon, _flame;

		private class Flame
		{
			public float _x, _y, _startX, _startY, _endX, _endY;
			public int _frame, _tick, _angle, _angle2;
		}

		private Flame[] _flames = new Flame[100];

		public Flamethrower (Game game, int x, int y) : base(game, x, y, "flamethrower")
		{
			MaxAngleVelocity = 30;
			
			var u = game.Config.GetBuildableUpgrade (TypeId, 0);
			_weapon = new Sprite (u.SpriteTemplates["weapon"], 0, Priority.TowerWeapon);
			_flame = new Sprite (_flameTemplate, 0, Priority.TowerWeaponOverlay);
		}

		public void SpawnFlame ()
		{
			for (int i = 0; i < _flames.Length; ++i) {
				if (_flames[i] == null) {
					_flames[i] = new Flame ();
					
					float r = 150.0f;
					float dx = (float)Math.Cos (Math.PI * Angle / 2048.0);
					float dy = (float)Math.Sin (Math.PI * Angle / 2048.0);
					float endX = dx * r;
					float endY = dy * r;
					
					_flames[i]._frame = Util.RandomInt (0, 59);
					_flames[i]._tick = 0;
					_flames[i]._startX = X + dx * 8;
					_flames[i]._startY = Y + dy * 8;
					_flames[i]._endX = _flames[i]._startX + endX;
					_flames[i]._endY = _flames[i]._startY + endY;
					_flames[i]._angle = Angle - 1024;
					_flames[i]._angle2 = Util.RandomInt (-2048, 2047);
					break;
				}
			}
		}

		public override ObjectStatePacket Update (long ticks)
		{
			base.Update (ticks);
			
			if (Target != null && (_game.Application.Renderer.FrameCount % 5) == 0)
				SpawnFlame ();
			
			UpdateFlames ();
			
			return NewState();
		}

		public override void Render ()
		{
			base.Render ();
			
			_flame.Resize (_flames.Length);
			
			for (int i = 0; i < _flames.Length; ++i) {
				Flame flame = _flames[i];
				
				_flame[i].Flags |= SpriteFlags.Disable;
				
				if (flame == null)
					continue;
				
				_flame[i].Flags &= ~SpriteFlags.Disable;
				
				float progress = flame._tick / 100.0f;
				progress = Util.Clamp (0.0f, 1.0f, progress);
				flame._x = Util.Lerp (flame._startX, flame._endX, progress);
				flame._y = Util.Lerp (flame._startY, flame._endY, progress);
				
				float blue = progress / 0.3f;
				
				float scale = 0.2f + 0.9f * (flame._tick / 70.0f);
				scale += 0.1f * ((flame._angle2) / 2048.0f);
				float fade = (30 - Math.Max (0, flame._tick - 70)) / 30.0f;
				//float rotate = (40-Math.Max(0,flame._tick-60))/40.0f;
				
				_flame[i].X = flame._x;
				_flame[i].Y = flame._y;
				_flame[i].Frame = (byte)flame._frame;
				
				_flame[i].Angle = (ushort)Util.Lerp (flame._angle, flame._angle + flame._angle2, 1.0f - fade);
				_flame[i].Scale = scale;
				float darkening = progress * progress * 1.0f;
				_flame[i].Color.R = Util.Saturate (blue * fade) * 1.1f - darkening;
				_flame[i].Color.G = Util.Saturate (blue * fade) - darkening;
				_flame[i].Color.B = Util.Saturate (fade) * 1.0f - darkening;
				_flame[i].Color.A = 0.0f;
				
			}
			_game.Application.Renderer.AddDrawable (_flame);
			
			_weapon[0].X = X;
			_weapon[0].Y = Y;
			_weapon[0].Angle = (ushort)Angle;
			_weapon[0].Frame = (byte)CurrentUpgrade.Level;
			_game.Application.Renderer.AddDrawable (_weapon);
		}

		private void UpdateFlames ()
		{
			for (int i = 0; i < _flames.Length; ++i) {
				Flame flame = _flames[i];
				
				if (flame == null)
					continue;
				
				float progress = flame._tick / 100.0f;
				
				if (progress > 1.0f) {
					_flames[i] = null;
					continue;
				}
				
				// ...
				
				flame._tick++;
				
				if ((flame._tick & 2) != 0)
					flame._frame++;
				
				if (flame._frame > 15)
					flame._frame = 0;
			}
		}
	}
}
