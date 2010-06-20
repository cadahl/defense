namespace Util
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Xml;

	public enum BlockType
	{
		Empty = 0,
		Floor = 1,
		NoBuild = 2,
		Solid = 3
	}

	public static class BlockTypeExtensions
	{

	}

	public struct MapBlock
	{
		public ushort TileIndex;
		public BlockType Type;

		public static MapBlock Empty = new MapBlock(BlockType.Empty, 0);

		public MapBlock(BlockType type, ushort tileIndex)
		{
			TileIndex = tileIndex;
			Type = type;
		}

		public bool IsEmpty
		{
			get
			{
				return Type == BlockType.Empty;
			}
		}
	}

	public enum SpawnType
	{
		Nothing = 0,
		Enemy = 1,
		Base = 2
	}

	public class MapSpawn
	{
	    public int BlockX { get; private set; }
	    public int BlockY { get; private set; }
	    public SpawnType Type { get; private set; }
	
	    public MapSpawn(int bx, int @by, SpawnType type) 
		{
	        BlockX = bx;
	        BlockY = @by;
	        Type = type;
	    }

	}

	public class Map
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public List<MapBlock> Blocks  { get; private set; }
		public List<string> TilemapNames { get; private set; }
		public List<MapSpawn> Spawns { get; private set; }

		public Map (int width, int height)
		{
			Width = width;
			Height = height;
			Blocks = new List<MapBlock>();//Enumerable.Repeat(MapBlock.Empty, Width*Height));
			TilemapNames = new List<string>();
			Spawns = new List<MapSpawn>();
		}
	}
}
