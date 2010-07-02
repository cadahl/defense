
namespace Client.Sim
{
	using System;
	using Client.Graphics;

	public class Wall : Buildable
	{
		public override float Radius {
			get {
				return 16.0f;
			}
		}
		
		public Wall (Game game, float x, float y) : base(game,"wall")
		{
			X = x;
			Y = y;
			Angle = 0;
			TypeId = "wall";
		}

		public override ObjectStatePacket Update(long ticks)
		{
			return new BuildableStatePacket(NewState());
		}
		
		public override void Render ()
		{
		/*	int bx = (int)X / 32;
			int by = (int)Y / 32;
			int mask = 	_game.GetBuilding(bx+1,by) is Wall ? 1 : 0;
			mask |=	_game.GetBuilding(bx-1,by) is Wall ? 2 : 0;
			mask |=	_game.GetBuilding(bx,by-1) is Wall ? 4 : 0;
			mask |=	_game.GetBuilding(bx,by+1) is Wall ? 8 : 0;
			_game.Application.Renderer.Backgrounds[1].SetTile (bx, by, mask, 416/32);
		*/}
	}
}
