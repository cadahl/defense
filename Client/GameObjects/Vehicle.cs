namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Client.Graphics;
	using OpenTK.Graphics;
	using Util;
	using System.Drawing;
	
	public class Vehicle : GameObject
	{
		private Sprite _vehicleSprite;
		private static SpriteTemplate DamagePuff = new SpriteTemplate 
		{ 
			TilemapName = "units", 
			Rectangle = new Rectangle(0, 80, 16, 16) 
		};

		private float _targetSpeed;
		private float _angleDampening, _angleDampening2;

		public float Speed, MaxSpeed = 0.7f;
		public float Acceleration = 0.05f;
		public float TurnDampening = 0.15f;

		public override float Radius {
			get {
				return 16.0f;
			}
		}
		
		public enum Thought
		{
			Normal,
			Overtaking,
			Slowing,
			Stopping
		}
		
		public Thought CurrentThought = Thought.Normal;
		
		private int _hitPoints, _maxHitPoints;
		public int HitPoints { get { return _hitPoints; } set { _hitPoints = value; if(value > _maxHitPoints) _maxHitPoints = value; } }
		public int MaxHitPoints { get { return _maxHitPoints; } }
		public int Reward { get; set; }
		private int _hurtTimer;
		private int _hurtTimerStart = 5;
		private Line _targetLine;

		public SpriteTemplate Template
		{
			set
			{
				_vehicleSprite = new Sprite(value, 0, Priority.Vehicle); 
			}
		}

 		public Vehicle(Game game) : base(game,0)
		{			
			_targetLine = new Line(0.0f,0.0f,0.0f,0.0f,1.0f,1.0f,0,300);
			StartDriving();
		}

		public void Damage(int points)
		{
			HitPoints -= points;

			_hurtTimer = _hurtTimerStart;
			
			
			float[] rnds = Util.RandomInCircle(6.0f);
			Generic puff = new Generic(_game, X+rnds[0], Y+rnds[1], DamagePuff);
			puff.Priority = Priority.VehicleHitEffect;
			puff.LifeTime = 20;
			puff.StartAngle = Util.RandomInt(0,4095);
			puff.EndAngle = (puff.StartAngle + Util.RandomSign() * Util.RandomInt(Angles.Pi/6, (int)(Angles.Pi/5))) & 4095;
			puff.StartScale = 1.0f;
			puff.EndScale = 1.4f;
			puff.StartColor = Color.FromArgb(255,Color.White);
			puff.EndColor = Color.FromArgb(0,Color.Black);
			_game.AddObject(puff);
		}
		
		public override void Update(long ticks)
		{
			base.Update(ticks);
			
			if(_hurtTimer > 0)
			    _hurtTimer--;
						
			if(HitPoints <= 0)
			{
				_game.Cash += Reward;
				_game.RemoveObject(this);
				_game.AddObject(new Explosion(_game,X,Y,ExplosionType.Smoky));
			}
					
		}
		
		public override void Render()
		{
			var r = _game.Application.Renderer;
			
			if(_game.ShowVehicleProbes)
				r.AddDrawable(_targetLine);
			
			_vehicleSprite[0].X = X;
			_vehicleSprite[0].Y = Y;
			_vehicleSprite[0].Angle = (ushort)((Angle-1024) & 4095);
			r.AddDrawable(_vehicleSprite);
			
			
			
			if(_hurtTimer > 0) 
			{
			    float flash = ((float)_hurtTimer)/_hurtTimerStart;
			  //  amount =(float) Math.sqrt(amount);
			    flash *= flash;
				
				float amount = (1.0f-((float)_hitPoints)/_maxHitPoints);//.Min(1.5f, _compoundFlash); 
				
				amount *= amount * amount;
				
				amount = Util.Lerp(0.0f, 1.5f, amount);
			    _vehicleSprite[0].Color = new Color4(1.0f * amount, flash * amount, flash * amount, 1.0f);
			    _vehicleSprite.SetFlags(Drawable.Flags.Colorize);
			}
			else
			{
				_vehicleSprite[0].Color = Color4.White;
				_vehicleSprite.ClearFlags(Drawable.Flags.Colorize);
			}

			
		}

		private void StartDriving()
		{
			// Set up driving thinker
			AddAction(	0,  delegate(long time) 
			              	{
				
								float dx, dy;
								Angles.AngleToDirection(Angle, out dx, out dy);
				
								float ahead = 0.0f;
								// Find where the navigation wants us to go next.
								int bx = ((int)(X+dx*ahead))/32;
								int by = ((int)(Y+dy*ahead))/32;
				
								if(_game.Navigation.IsBlocked(bx,by))
								{
									bx = ((int)X)/32;
									by = ((int)Y)/32;
								}
				
								int nextx, nexty;	
								_game.Navigation.FollowStraightPath((int)X,(int)Y, out nextx, out nexty);
								bool isTarget = _game.Navigation.IsTarget(bx, by);
				
								_targetLine.X = X;
								_targetLine.Y = Y;
								_targetLine.X2 = nextx;
								_targetLine.Y2 = nexty;
				
								// The vehicle will try to point toward this lookahead point. Currently it's just
								// the center of the "next" cell. 
								int targetAngle = Util.DeltasToAngle(nextx-X, nexty-Y);
					
								_angleDampening += (targetAngle-_angleDampening) * 0.1f;
								_angleDampening2 += (_angleDampening-_angleDampening2) * 0.1f;
								Angle = ((int)(Angle + (_angleDampening2-Angle) * 0.1f)) & 4095;
				
				
							//	float dx, dy;
								Angles.AngleToDirection(Angle, out dx, out dy);
				
								float nextxf, nextyf;
				
								nextxf = X + dx * Speed;
								nextyf = Y + dy * Speed;
								
								var tooCloseObjects =_game.FindObjectsWithinRadius(this,X,Y,40,typeof(Vehicle));
								var closeObjects =_game.FindObjectsWithinRadius(this,X,Y,55,typeof(Vehicle));
								var closeObjects2 =_game.FindObjectsWithinRadius(this,nextxf,nextyf,55,typeof(Vehicle));

								float colliderDist = float.MaxValue;
								Vehicle collider = null;
				
								foreach(var gob2 in closeObjects2)
								{
									foreach(var gob in closeObjects)
									{
										if(gob.Object == gob2.Object && gob2.Distance < gob.Distance && gob2.Distance < colliderDist)
										{							
											collider = (Vehicle)gob2.Object;
											colliderDist = gob2.Distance;
										}
									}
								}

								bool iWillDriveAnyway = true;
								//if(tooCloseObjects. != 0)
								{
									int thisMuchOnCourse = Math.Abs(Angles.Difference(Angle,targetAngle));
									foreach(var tco in tooCloseObjects)
									{
										var v = (Vehicle)tco.Object;
										if(v.CurrentThought != Thought.Stopping && Math.Abs(Angles.Difference(v.Angle, targetAngle)) < thisMuchOnCourse)
										{
											iWillDriveAnyway = false;
											break;
										}
									}
									
									if(!iWillDriveAnyway)
									{
										_targetSpeed = 0;
										CurrentThought = Thought.Stopping;
									}
								}
				
								if(iWillDriveAnyway)
								{
									if(collider != null)
									{	
										if(collider.MaxSpeed <= MaxSpeed)
										{
											CurrentThought = Thought.Slowing;
											_targetSpeed = ((Vehicle)collider).Speed;
										}
									}
									else
									{
										// Accelerate to reach max velocity and stay there (currently).
										_targetSpeed = Math.Min(MaxSpeed, _targetSpeed + Acceleration);
										CurrentThought = Thought.Normal;
									}
								}				
								
								Speed = Util.Lerp(Speed, _targetSpeed, 0.1f);
				
								X = nextxf;
								Y = nextyf;
				
					
								if((time % 10) == 0)
								{
								//	SpawnDust();
								}
				
								if(isTarget)
								{
									_game.AddObject(new Explosion(_game, X, Y, ExplosionType.Smoky));
									_game.RemoveObject(this);
									return false;
								}
				
								return true;
							});
		}
		
		private void SpawnDust()
		{
			Generic puff = new Generic(	_game, 
                        				X, 
                        				Y,
                        				Generic.Puff);
			puff.LifeTime = (int)Util.Lerp(40,60,Util.Random());
			int rnd = 50;//Util.RandomInt(220,245);
			puff.StartColor = Color.FromArgb(110, (int)(rnd*0.9), (int)(rnd*0.9), (int)(rnd*0.8));
			puff.EndColor = Color.FromArgb(0,rnd,rnd,rnd);
			puff.StartAngle = Util.RandomInt(0, 4095);
			puff.EndAngle = (puff.StartAngle + Util.RandomSign() * Util.RandomInt(Angles.Pi/3, (int)(Angles.Pi/2))) & 4095;
			puff.StartScale = 1.0f;
			puff.EndScale = Util.Lerp(1.0f,Speed*2.5f,Util.Random());
			puff.Priority = 4;

			_game.AddObject(puff);
		}

		
		/*
		private void SetNextTargetOLD(bool slowStart)
		{
			int blockx = ((int)X)/32;
			int blocky = ((int)Y)/32;
			int nextbx, nextby;	
			bool isBase;
			
			int nextbx2, nextby2;
			bool isBaseDummy;			
			
			_game.Navigation.NextMapCell(blockx, blocky, out nextbx, out nextby, out isBase);
			_game.Navigation.NextMapCell(nextbx, nextby, out nextbx2, out nextby2, out isBaseDummy);
			
			int nextAngle = Util.DeltasToAngle(nextbx2-nextbx, nextby2-nextby);
			
			{
				float sourceX = X;
				float sourceY = Y;
				float targetX = nextbx * 32 + 16;
				float targetY = nextby * 32 + 16;
				int sourceAngle = Angle;
				int targetAngle = Util.DeltasToAngle(nextbx-blockx, nextby-blocky);
				
				float skiddyness = (float)Util.Random(SkidMinimum,SkidMaximum);
				
				AddAction(	0, 
				           		delegate(long time) 
				              	{
									float k = Util.Saturate(time / Speed);
									float movek = k;
									bool tookHeavyTurn = false;
					
									if(Math.Abs(sourceAngle-nextAngle) >= Angles.Pi/2.5)
									{
										k = Util.Lerp(k,Util.Saturate(time / (CurveSpeed*skiddyness*1.1f)),k);
						
										if(slowStart)
										{
											movek = Util.Smoothstep(0.0f, 1.0f, k);
										}
										else
										{
											movek = Util.Lerp(k, 2.0f * Util.Smoothstep(-1.0f,1.0f,k)-1.0f,k);
										}
						
										Angle = Util.LerpAngle(sourceAngle, nextAngle, Util.Smoothstep(0.0f, 1.0f, movek) * skiddyness);
										tookHeavyTurn = true;
						
										if((time % 10) == 0)
										{
											Generic puff = new Generic(_game.newLocalObjUID(), 
							                            				_game, 
							                            				X, 
							                            				Y,
							                            				Generic.Puff);
											puff.LifeTime = 40;
											int rnd = 255;//Util.RandomBool()?0:255;//Util.RandomInt(220,245);
											puff.StartColor = Color.FromArgb(80,rnd,rnd,rnd);
											puff.EndColor = Color.FromArgb(0,rnd,rnd,rnd);
											puff.StartAngle = Util.RandomInt(0, 4095);
											puff.EndAngle = (puff.StartAngle + Util.RandomSign() * Util.RandomInt(Angles.Pi/5, Angles.Pi/4)) & 4095;
											puff.StartScale = 1.0f;
											puff.EndScale = 1.5f;
											puff.Priority = 4;
							
											_game.AddObject(puff);
										}
						
									}
									else
									{
										if(slowStart)
										{
											movek = Util.Lerp(Util.Smoothstep(0.0f,2.0f,k),k,k);
										}
								
										Angle = Util.LerpAngle(sourceAngle, nextAngle, Util.Smoothstep(0.0f, 1.0f, movek));
									}
					
									float newPathX = Util.Lerp(sourceX, targetX, movek);
									float newPathY = Util.Lerp(sourceY, targetY, movek);		
					
									float forwardx, forwardy;
									float d = Util.Distance(_oldPathX, _oldPathY, newPathX, newPathY);
									Angles.AngleToDirection(Angle, out forwardx, out forwardy);
					
									if(tookHeavyTurn)
									{
										X = newPathX;
										Y = newPathY;
									}
									else
									{
										X = Util.Lerp(X + forwardx * d, newPathX, 0.3f);
										Y = Util.Lerp(Y + forwardy * d, newPathY, 0.3f);
									}
					
									_oldPathX = newPathX;
									_oldPathY = newPathY;
					
									if(k == 1.0f)
									{
										if(isBaseDummy)
										{
											_game.AddObject(new Explosion(_game.newLocalObjUID(), _game, X, Y, Explosion.NORMAL));
											_game.RemoveObject(this);
										}
						
										SetNextTargetOLD(tookHeavyTurn);
										return false;
									}
					
									return true;
								});
				
			}
		}*/
		
		
			

	}
}
