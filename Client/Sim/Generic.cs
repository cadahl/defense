namespace Client.Sim
{
	using System.Drawing;
	using Client;
	using Client.Graphics;
	using OpenTK.Graphics;
	using Util;

	public class Generic : GameObject
	{
		public static SpriteTemplate Puff = new SpriteTemplate { TilemapName = "units", Rectangle = new Rectangle(36, 228, 32-4, 32-4) };
		public static SpriteTemplate Puff2 = new SpriteTemplate { TilemapName = "units", Rectangle = new Rectangle(40, 202, 16, 16) };

		public override float Radius {
			get {
				return 4.0f;
			}
		}
		
		public SpriteTemplate SpriteTemplate
		{
			set
			{
				_sprite = new Sprite(value, 0, _priority);
			}
		}
		
		public int Priority
		{
			set
			{
				_priority = value;
				_sprite.Priority = _priority;
			}
		}
		
		public int _priority=1;
		public int LifeTime=130;
		public int StartAngle=0, EndAngle=0;
		public float StartScale=1.0f, EndScale = 2.0f;
		public Color StartColor = Color.FromArgb(255,255,255,255), EndColor = Color.FromArgb(255,255,255,255);

		private Sprite _sprite;
		private int _ticks;
		
		public Generic(Game game, float x, float y, SpriteTemplate spriteTemplate) : base(game,0,"generic")
		{
			X = x;
			Y = y;
		//	_random = (float)Util.Random();
			SpriteTemplate = spriteTemplate;
		}

		public override ObjectStatePacket Update(long ticks)
		{
			if(_ticks++ >= LifeTime) 
			{
				_game.RemoveObject(this);
				return null;
			}
			
			return NewState();
		}
		
		public override void Render()
		{
			float k = ((float)_ticks) / LifeTime;
			_sprite[0].X = X;
			_sprite[0].Y = Y;
			_sprite[0].Scale = Util.Lerp(StartScale, EndScale, k);
			_sprite[0].Angle = (ushort)Util.LerpAngle(StartAngle, EndAngle, k);
			float alphak = k;//Util.Smoothstep(0.0f,1.0f,k);
			
			float alpha = Util.Lerp((float)StartColor.A, (float)EndColor.A, alphak);
			//if(alpha <= 0.5f)
			//	_game.RemoveObject(this);
			
			alpha /= 255.0f;
			
			_sprite[0].Color = new Color4(  Util.Lerp(StartColor.R/255.0f, EndColor.R/255.0f, k) * alpha,
											Util.Lerp(StartColor.G/255.0f, EndColor.G/255.0f, k) * alpha,
											Util.Lerp(StartColor.B/255.0f, EndColor.B/255.0f, k) * alpha,
			                              	alpha);
			_game.Application.Renderer.AddDrawable(_sprite);
		}
	}
}
