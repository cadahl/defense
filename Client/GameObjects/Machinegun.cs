
namespace Client.GameObjects
{
	using System;
	using Client;
	using Client.Graphics;
	using OpenTK.Graphics;
	using Util;

	
	public class Machinegun : TowerUnit
	{
		private Timer _fireTimer = new Timer(TimerMode.CountDown, 0 );
		private Timer _targetingTimer = new Timer(TimerMode.CountDown, 5);
		private Timer _flashTimer = new Timer(TimerMode.CountDown, 5);
		
		private Sprite _gunSprite, _flashSprite;
		
 		public Machinegun(Game game, int x, int y) : base(game,x,y)
		{
			_fireTimer.Elapsed += HandleFireTimerElapsed;
			_flashTimer.Ticked += HandleFlashTimerTicked;
			_targetingTimer.Elapsed += HandleFireTimerElapsed;
			
			var u = game.GetUpgradeInfo(GetType(),0);
			_gunSprite = new Sprite(u.SpriteTemplates["weapon"], 0, Priority.TowerWeapon);
			_flashSprite = new Sprite(u.SpriteTemplates["flash"], 0, Priority.TowerWeaponOverlay);

			CurrentUpgradeChanged += delegate 
			{
				_fireTimer.ResetValue = CurrentUpgrade.ReloadTime;
			};
		}

		void HandleFlashTimerTicked (Timer timer)
		{
			_flashSprite[0].X = X;
			_flashSprite[0].Y = Y;
			_flashSprite[0].Angle = (ushort)Angle;
			
			float amount = _flashTimer.Progress;
   			_flashSprite[0].Color = new Color4(amount,amount,amount,0.0f);
			_game.Client.Renderer.AddDrawable(_flashSprite);
		}

		void HandleFireTimerElapsed (Timer timer)
		{
			// Is the gun ready to fire and is the target in sight?
			if(_fireTimer.IsElapsed && IsTargetInSight)
			{
			    _fireTimer.Reset();
				
				// Fire!
				float d = Util.Distance(X,Y,Target.X,Target.Y);
				float dirX;
				float dirY;
				Angles.AngleToDirection(Angle,out dirX, out dirY);
			    float gunX = X + dirX * (32-8);
			    float gunY = Y + dirY * (32-8);
					
			    float[] hitPos = Util.RandomInCircle(5);

				_game.AddObject(new InstantBullet(_game, X + d * dirX + hitPos[0], Y + d * dirY + hitPos[1], gunX, gunY, CurrentUpgrade.Damage));
	
				_flashTimer.Reset();
			}
			else
			{
				_targetingTimer.Reset();
			}
		}
		
		public override void Update(long ticks)
		{
			base.Update(ticks);

			_fireTimer.Tick();
			_targetingTimer.Tick();
		}
		
		public override void Render()
		{
			base.Render();

			_flashTimer.Tick();

			_gunSprite[0].X = X;
			_gunSprite[0].Y = Y;
			_gunSprite[0].Angle = (ushort)Angle;
			_gunSprite[0].Frame = (byte)CurrentUpgrade.Id;
			_game.Client.Renderer.AddDrawable(_gunSprite);
		}
	}
}
