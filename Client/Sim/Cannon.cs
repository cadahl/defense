
namespace Client.Sim
{
	using System;
	using Client.Graphics;
	using OpenTK.Graphics;
	using Util;
	
	public class Cannon : TowerUnit
	{
		private Timer _fireTimer = new Timer(TimerMode.CountDown, 120/6);
	//	private Timer _flashTimer = new Timer(TimerMode.CountDown, 11);
		private Timer _targetingTimer = new Timer(TimerMode.CountDown, 1);

		private bool _fired;
	//	private Sprite _flashSprite, _gunSprite;
		
 		public Cannon(Game game, int x, int y) : base(game,x,y,"cannon")
		{
			_fireTimer.Elapsed += HandleFireOrTargetingTimerElapsed;
		//	_flashTimer.Ticked += Flashing;
			_targetingTimer.Elapsed += HandleFireOrTargetingTimerElapsed;

		/*	var u = game.Config.GetBuildableUpgrade(GetType(),0);
			_gunSprite = new Sprite(u.SpriteTemplates["weapon"], 0, Priority.TowerWeapon);
			_flashSprite = new Sprite(u.SpriteTemplates["flash"], 0, Priority.TowerWeaponOverlay);
			*/
			CurrentUpgradeChanged += delegate 
			{
				_fireTimer.ResetValue = CurrentUpgrade.ReloadTime;
			};
		}

		private void HandleFireOrTargetingTimerElapsed(Timer timer)
		{
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
					
			    _game.AddObject(new Bullet(_game, gunX, gunY, X + d * dirX, Y + d * dirY, CurrentUpgrade.Damage));
	
				_fired = true;
	//			_flashTimer.Reset();
			}
			else
			{
				// Try again in a few ticks.
				_targetingTimer.Reset();
			}
		}
		
		private void Flashing(Timer timer)
		{/*
			_flashSprite[0].X = X;
			_flashSprite[0].Y = Y;
			_flashSprite[0].Angle = (ushort)Angle;
			
			float amount = (float)Math.Pow(timer.Progress, 2);
   			_flashSprite[0].Color = new Color4(amount,amount,amount,0.0f);
			_game.Application.Renderer.AddDrawable(_flashSprite);*/
		}
		
		public override ObjectStatePacket Update(long ticks)
		{
			base.Update(ticks);

			// Tick the timers and handle events			
			_fireTimer.Tick();
			_targetingTimer.Tick();
			
			var packet = new BuildableStatePacket(NewState());
			
			if(_fired)
			{
				packet.Flags |= BuildableStatePacket.FireFlag;
				_fired = false;
			}
			
			return packet;
		}
		
		public override void Render()
		{
		/*	base.Render();

			_flashTimer.Tick();
			
			// Draw cannon
			_gunSprite[0].X = X;
			_gunSprite[0].Y = Y;
			_gunSprite[0].Angle = (ushort)Angle;
			_gunSprite[0].Frame = (byte)CurrentUpgrade.Id;
			_game.Application.Renderer.AddDrawable(_gunSprite);*/
		}
	}
}
