
namespace Client.Sim
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
		private bool _fire;		
		private Sprite _gunSprite, _flashSprite;
		
 		public Machinegun(Game game, int x, int y) : base(game,x,y,"machinegun")
		{
			_fireTimer.Elapsed += HandleFireTimerElapsed;
			_targetingTimer.Elapsed += HandleFireTimerElapsed;
			
			CurrentUpgradeChanged += delegate 
			{
				_fireTimer.ResetValue = CurrentUpgrade.ReloadTime;
			};
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
	
				_fire = true;
				//_flashTimer.Reset();
			}
			else
			{
				_targetingTimer.Reset();
			}
		}
		
		public override ObjectStatePacket Update(long ticks)
		{
			base.Update(ticks);

			_fireTimer.Tick();
			_targetingTimer.Tick();
			
			var packet = new BuildableStatePacket(NewState());
			if(_fire)
			{
				packet.Flags |= BuildableStatePacket.FireFlag;
				_fire = false;
			}
			return packet;
		}
		
		public override void Render()
		{
/*			base.Render();

			_gunSprite[0].X = X;
			_gunSprite[0].Y = Y;
			_gunSprite[0].Angle = (ushort)Angle;
			_gunSprite[0].Frame = (byte)CurrentUpgrade.Id;
			_game.Application.Renderer.AddDrawable(_gunSprite);
*/		}
	}
}
