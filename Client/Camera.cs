namespace Client
{
	using System;
	using Client.Graphics;
	using Client.GameObjects;

	/**
 * Låter användaren styra runt en "kamera" och titta på leveln. Spectator-style.
 * @author aav
 */
	public class Camera
	{
		
		// Lite konstanter.
		private const float acceleration = 1.0f;
		private const float friction = 7.0f/16.0f;
		private const float maxVelocity = 7.0f;

		private float _x;
		private float _y;
		private float _xVelocity;
		private float _yVelocity;

		public Camera()
		{
		}

		public void Update (Game game)
		{
			Background bg = game.Application.Renderer.Backgrounds[0];
			Input input = game.Application.Input;
			
			// Lägg på acceleration om spelaren trycker i någon riktning, men bara upp till maximal hastighet.
			if (input.isDown (Key.CameraLeft))	_xVelocity = Math.Max (_xVelocity - acceleration, -maxVelocity);
			if (input.isDown (Key.CameraRight))	_xVelocity = Math.Min (_xVelocity + acceleration, maxVelocity);
			if (input.isDown (Key.CameraUp))	_yVelocity = Math.Max (_yVelocity - acceleration, -maxVelocity);
			if (input.isDown (Key.CameraDown))	_yVelocity = Math.Min (_yVelocity + acceleration, maxVelocity);
			
			// Om X-hastigheten är negativ (åt vänster), lägg på friktion åt höger. Annars tvärt om.
			if (_xVelocity < 0)	_xVelocity = Math.Min (_xVelocity + friction, 0);
			if (_xVelocity > 0)	_xVelocity = Math.Max (_xVelocity - friction, 0);
			
			// Om Y-hastigheten är negativ (uppåt), lägg på friktion neråt. Annars tvärt om.
			if (_yVelocity < 0)	_yVelocity = Math.Min (_yVelocity + friction, 0);
			if (_yVelocity > 0)	_yVelocity = Math.Max (_yVelocity - friction, 0);
			
			// Uppdatera positionen med hjälp av hastigheterna.
			_x = _x + _xVelocity;
			_y = _y + _yVelocity;
			
			// Kollidera mot vänsterkanten och nolla X-hastigheten.
			if (_x < 0) 
			{
				_x = 0;
				_xVelocity = 0;
			}
			// Kollidera mot högerkanten och nolla X-hastigheten.
			if (_x > bg.MaxHScroll) 
			{
				_x = bg.MaxHScroll;
				_xVelocity = 0;
			}
			// Kollidera mot överkanten och nolla Y-hastigheten.
			if (_y < 0) 
			{
				_y = 0;
				_yVelocity = 0;
			}
			// Kollidera mot nederkanten och nolla Y-hastigheten.
			if (_y > bg.MaxVScroll) 
			{
				_y = bg.MaxVScroll;
				_yVelocity = 0;
			}
			
			// Nu har vi en färdig position, så scrolla bakgrundslagret.
			bg.HScroll = _x;
			bg.VScroll = _y;
			game.Application.Renderer.Backgrounds[1].HScroll = _x;
			game.Application.Renderer.Backgrounds[1].VScroll = _y;
		}

/*		public void ScrollToObject (GameObject obj)
		{
			if (obj == null) {
				return;
			}
			
			// We check the player against a margin of 100 pixels
			const int xmargin = 200 * 16;
			const int ymargin = 160 * 16;
			
			// Get player object rectangle coordinates
			int left = obj.getX ();
			int top = obj.getY ();
			int right = left + obj.getWidth ();
			int bottom = top + obj.getHeight ();
			
			// Get layer 0 rectangle coordinates
			Background bg = game.Client.Renderer.getBackground (0);
			int sleft = bg.HScroll + xmargin;
			int stop = bg.VScroll + ymargin;
			int sright = sleft + Client.Renderer.Width * 16 - xmargin * 2;
			int sbottom = stop + Client.Renderer.Height * 16 - ymargin * 2;
			
			// Player too far left or right? Change horizontal scroll.
			// But don't allow both to be true at the same time.
			if (left < sleft) {
				bg.HScroll = bg.HScroll - (sleft - left);
			} else if (right > sright) {
				bg.HScroll = bg.HScroll + (right - sright);
			}
			
			// Player too far up or down? Change verical scroll.
			// But don't allow both to be true at the same time.
			if (top < stop) {
				bg.VScroll = bg.VScroll - (stop - top);
			} else if (bottom > sbottom) {
				bg.VScroll = bg.VScroll + (bottom - sbottom);
			}
		}
		*/
	}
	
	
}
