
namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using Client.Graphics;
	using Util;

	public abstract class TowerUnit : Buildable
	{
		private int _trackingTargetAngle;
		protected int MaxAngleVelocity = 60;

		protected GameObject Target;

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

		public TowerUnit(Game game, float x, float y) : base(game)
		{
			X = x;
			Y = y;
			Width = 32;
			Height = 32;
			Angle = 0;
			_game.Application.Renderer.Backgrounds[1].SetTile((int)X / 32, (int)Y / 32, 5, 5);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if(disposing)
				{
					_game.Application.Renderer.Backgrounds[1].ClearTile((int)X / 32, (int)Y / 32);
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		float CrossProduct(float x, float y, float x2, float y2)
		{
			return x * y2 - y * x2;
		}

		public override void Update(long ticks)
		{
			if ((Target != null && Util.Distance(X, Y, Target.X, Target.Y) > CurrentUpgrade.Range) || !_game.DoesObjectExist(Target))
			{
				Target = null;
			}
			
			if (Target == null)
			{
				var targets = _game.FindObjectsWithinRadius(this, X, Y, CurrentUpgrade.Range, typeof(Vehicle));
				float closest = float.MaxValue;
				GameObject closestTarget = null;
				foreach (var v in targets)
				{
					if (v.Distance < closest)
					{
						closest = v.Distance;
						closestTarget = v.Object;
					}
				}
				
				Target = closestTarget;
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
				
				Angle = _trackingTargetAngle & 4095;
			}
		}

		public override void Render()
		{
			_game.Application.Renderer.Backgrounds[1].SetTile((int)X / 32, (int)Y / 32, 5, 5);
			
			// Draw base of tower
//			_game.Application.Renderer.DrawSprite(_base, X, Y, 0, 0, Priority.TowerBase);
		}
	}
}
