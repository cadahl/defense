
namespace Client.Sim
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Graphics;
	using Util;

	public abstract class TowerUnit : Buildable
	{
		private int _trackingTargetAngle;
		protected int MaxAngleVelocity = 60;

		protected GameObject Target;

		public override float Radius {
			get {
				return 16.0f;
			}
		}
		
		protected bool IsTargetInSight {
			get {
				if (Target == null)
					return false;
				
				// Get unit target vector
				float tdx = Target.X - X;
				float tdy = Target.Y - Y;
				float d = Util.Distance(X, Y, Target.X, Target.Y);
				tdx /= d;
				tdy /= d;
				
				// Perpendicular to the target-vector.
				float sidex = tdy;
				float sidey = -tdx;
				
				float targetRadius = 16f;
				
				float p1x = Target.X - sidex * targetRadius;
				float p1y = Target.Y - sidey * targetRadius;
				float p2x = Target.X + sidex * targetRadius;
				float p2y = Target.Y + sidey * targetRadius;
				
				int solidAngle = Angles.Difference(Util.DeltasToAngle(p1x - X, p1y - Y), Util.DeltasToAngle(p2x - X, p2y - Y));
				int targetAngle = Util.DeltasToAngle(tdx, tdy);
				
				return Angles.Difference(targetAngle, Angle) < solidAngle / 2;
			}
		}

		public TowerUnit(Game game, float x, float y, string typeId) : base(game, typeId)
		{
			X = x;
			Y = y;
			Angle = 0;
		}

		float CrossProduct(float x, float y, float x2, float y2)
		{
			return x * y2 - y * x2;
		}

		public override ObjectStatePacket Update(long ticks)
		{
			if ((Target != null && Util.Distance(X, Y, Target.X, Target.Y) > CurrentUpgrade.Range) || !_game.Contains(Target))
			{
				Target = null;
			}
			
			if (Target == null)
			{
				var nearest = _game.FindNearestObjectWithinRadius(this, X, Y, CurrentUpgrade.Range, typeof(Vehicle));
				Target = nearest != null ? nearest.Object : null;
			}
			
			if (Target != null)
			{
				float ax, ay;
				Angles.AngleToDirection(Angle, out ax, out ay);
				float tdx = Target.X - X;
				float tdy = Target.Y - Y;
				int actualTargetAngle = Util.DeltasToAngle(tdx, tdy);
				float cp = CrossProduct(ax, ay, tdx, tdy);
				int adiff = Angles.Difference(_trackingTargetAngle, actualTargetAngle) / 2;
				
				if (cp < 0)
				{
					_trackingTargetAngle -= Math.Min(adiff, MaxAngleVelocity);
				}
				else
				{
					_trackingTargetAngle += Math.Min(adiff, MaxAngleVelocity);
				}

				//_trackingTargetAngle = actualTargetAngle;
				
				Angle = _trackingTargetAngle & 4095;
			}
			
			return null;
		}

		public override void Render()
		{
		}
	}
}
