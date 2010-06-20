namespace Client
{
	using System;
	using Client.Graphics;
	using Util;

	public partial class Game
	{
		private static Sprite _navDebugSprite = new Sprite(new SpriteTemplate("units", 0, 256, 32, 32, 0, 0, 0), 0, 2);
		
		private void RenderNavigationMap(Renderer r)
		{
			_navDebugSprite.Resize(Map.Width * Map.Height);

			int i = 0;
			for(int mapy = 0; mapy < Map.Height; ++mapy)
			{
				for(int mapx = 0; mapx < Map.Width; ++mapx, ++i)
				{
					int arrowIndex = 4;
					int nextx, nexty;

					if(Navigation.IsTarget(mapx, mapy))
					{
						arrowIndex = 9;
					}
					else
						if(Navigation.GetNext(mapx, mapy, out nextx, out nexty))
						{
							arrowIndex = 1 + Math.Sign(nextx-mapx) + (Math.Sign(nexty-mapy) + 1) * 3;
						}

					_navDebugSprite[i].X = mapx*32;
					_navDebugSprite[i].Y = mapy*32;
					_navDebugSprite[i].Frame = (byte)arrowIndex;
					_navDebugSprite[i].Color = new OpenTK.Graphics.Color4(128,128,128,128);
				}
			}

			r.AddDrawable(_navDebugSprite);
		}
	}
}
