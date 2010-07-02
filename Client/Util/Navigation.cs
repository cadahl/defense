namespace Util
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using OpenTK;


	public class Navigation
	{

		private Map _map;
		public NavNode[] Grid { get; private set; }
		private bool _dirty = true;
		private int _targetX, _targetY;
		private static int BlockerCost = 10000;
		private static int[] NeighborLookups = {-1,0, 0,-1, 1,0, 0,1, -1,-1, 1,-1, 1,1, -1,1};

		public struct NavNode
		{
			public int _nextIndex;
			public int _cost;
		}
		public Navigation (Map map, int targetX, int targetY)
		{
			_map = map;
			_targetX = targetX;
			_targetY = targetY;
			Console.WriteLine("Target x,y = " + targetX + ", " + targetY);
			
			if(!Rebuild(null))
				throw new InvalidOperationException("Could not build path grid for the map.");
		}

		public bool IsBlocked(int bx, int by)
		{
			NavNode node = Grid[bx+by*_map.Width];
			if(node._cost >= BlockerCost || node._nextIndex == -1 || IsTarget(bx,by))
				return true;
			
			return false;
		}
		
		public bool GetNext(int bx, int by, out int nextbx, out int nextby)
		{
			nextbx = bx;
			nextby = by;
			
			if(IsBlocked(bx,by))
				return false;
			
			NavNode node = Grid[bx+by*_map.Width];
			
			int nexti = node._nextIndex;
			nextbx = nexti % _map.Width;
			nextby = nexti / _map.Width;
			
			return true;
		}
		
		public bool SampleNext(int x, int y, out int nextx, out int nexty)
		{
			int bx = ((int)(x))/32;
			int by = ((int)(y))/32;
			float fracx = ((x)/32.0f);
			float fracy = ((y)/32.0f);
			fracx -= (float)Math.Floor(fracx);
			fracy -= (float)Math.Floor(fracy);
			
			int x00,x10,x11,x01,y00,y10,y11,y01;
			bool valid00 = GetNext(bx,by, out x00, out y00);
			bool valid10 = GetNext(bx+1,by, out x10, out y10);
			bool valid11 = GetNext(bx+1,by+1, out x11, out y11);
			bool valid01 = GetNext(bx,by+1, out x01, out y01);
			
			x00 = x00 * 32 + 16;
			x10 = x10 * 32 + 16;
			x11 = x11 * 32 + 16;
			x01 = x01 * 32 + 16;
			y00 = y00 * 32 + 16;
			y10 = y10 * 32 + 16;
			y11 = y11 * 32 + 16;
			y01 = y01 * 32 + 16;
			
			if(valid00 && valid10 && valid11 && valid01)
			{
				int xtop = Util.Lerp(x00,x10,fracx);
				int ytop = Util.Lerp(y00,y10,fracx);
				int xbtm = Util.Lerp(x01,x11,fracx);
				int ybtm = Util.Lerp(y01,y11,fracx);
				nextx = Util.Lerp(xtop,xbtm,fracy);
				nexty = Util.Lerp(ytop,ybtm,fracy);
				return true;
			}
			
			nextx = x00;
			nexty = y00;
			
			return valid00;
		}
		
		public bool IsTarget(int bx, int by)
		{
			return bx == _targetX && by == _targetY;
		}
		
		public bool FollowStraightPath(int x, int y, out int endx, out int endy)
		{
			endx = x;
			endy = y;

			if(IsBlocked(x/32,y/32))
				return false;
			
			GetNext(x/32, y/32, out endx, out endy);
			endx = endx * 32 + 16;
			endy = endy * 32 + 16;
			return true;
		/*
			int endx2;
			int endy2, sgndx=0, sgndy=0;
			bool first = true;
			while(SampleNext(endx, endy, out endx2, out endy2) &&
			      (first || ( Math.Sign(endx2-endx) == sgndx && Math.Sign(endy2-endy) == sgndy)))
			{
				first = false;
				sgndx = Math.Sign(endx2-endx);
				sgndy = Math.Sign(endy2-endy);
				endx = endx2;
				endy = endy2;
			}
		*/
			return true;
		}
		
		public bool NextMapCell(int x, int y, out int nextx, out int nexty)
		{
			NavNode node = Grid[x+y*_map.Width];
			int nexti = node._nextIndex;
			
			if(IsBlocked(x,y))
			{
				nextx = x;
				nexty = y;
				return false;
			}
			else
			{
				nextx = nexti % _map.Width;
				nexty = nexti / _map.Width;
				return true;
			}
			
		}
		
		private bool IsValidMove(NavNode[] grid, int ax, int ay, int bx, int by)
		{
			if(grid[bx + by * _map.Width]._cost == BlockerCost)
				return false;
			
			// Diagonal movement must NOT go past any blocker neighbors
			if(ax != bx && ay != by)
			{
				if(	grid[ax + by * _map.Width]._cost == BlockerCost ||
					grid[bx + ay * _map.Width]._cost == BlockerCost)
					return false;
			}

			return true;
		}
		
		public bool Rebuild(IEnumerable<Point> blockers)
		{
			NavNode[] newGrid = new NavNode[_map.Width * _map.Height]; 
			PriorityQueueB<int> open = new PriorityQueueB<int>( (a,b) => newGrid[a]._cost.CompareTo(newGrid[b]._cost) );
			open.Push(_targetX + _targetY * _map.Width);
			
			for(int i = 0; i < newGrid.Length; ++i)
			{
				newGrid[i]._cost = _map.Blocks[i].Type == BlockType.Solid ? BlockerCost : 0;
				newGrid[i]._nextIndex = -1;
			}
			
			if(blockers != null)
			{
				foreach(Point p in blockers)
				{
					newGrid[p.X + p.Y * _map.Width]._cost = BlockerCost;
				}
			}
			
			while(open.Count > 0)
			{
				// Get the lowest cost open node
				int nodeIndex = open.Pop();
				int nodeX = nodeIndex % _map.Width;
				int nodeY = nodeIndex / _map.Width;

				for(int i = 0; i < 8; ++i)
				{
					int nextNodeX = nodeX + NeighborLookups[i*2+0];
					int nextNodeY = nodeY + NeighborLookups[i*2+1];
					
					if(nextNodeX < 0 || nextNodeX >= _map.Width || nextNodeY < 0 || nextNodeY >= _map.Height)
						continue;
					
					int nextNodeIndex = nextNodeX + nextNodeY * _map.Width;
					
					int nextCost = newGrid[nodeIndex]._cost + (i >= 4 ? 14 : 10);
					
					if(IsValidMove(newGrid, nodeX, nodeY, nextNodeX, nextNodeY))
					{
						// If the neighbor is not already visited, and walkable
						if(newGrid[nextNodeIndex]._nextIndex == -1)
						{
							newGrid[nextNodeIndex]._cost = nextCost;
							newGrid[nextNodeIndex]._nextIndex = nodeIndex;

							// add it to the open queue
							open.Push(nextNodeIndex);
						}
						else // Update the neighbor if this is a shorter path
							if(nextCost < newGrid[nextNodeIndex]._cost)
							{
								newGrid[nextNodeIndex]._cost = nextCost;
								newGrid[nextNodeIndex]._nextIndex = nodeIndex;
							}
					}
				}
			}

			// Verify that all enemy spawn points can reach the target.
			if( 0 != _map.EnemySpawns.Count(sp => newGrid[sp.BlockX + sp.BlockY * _map.Width]._nextIndex < 0))
			{
				return false;
			}
			
			Grid = newGrid;
			
			return true;
		}
	}
}

