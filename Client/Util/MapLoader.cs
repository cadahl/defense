namespace Util
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;

	public class MapLoader
	{
		private List<int[]> _layers = new List<int[]> ();
		private List<int> _flattened;
		private List<Tsx> _tsxs = new List<Tsx> ();
		private Map _outMap;

		public class Tsx
		{
			public string _name;
			public string _image;
			public int _firstgid;
			public bool _include;
			public Dictionary<int, SpawnType> _spawnTypes = new Dictionary<int, SpawnType> ();
			public BlockType[] _blk = new BlockType[256];
		}

		public Map Map
		{
			get
			{
				return _outMap;
			}
		}

		public MapLoader(string path)
		{
			Console.WriteLine ("MapLoader: Loading level " + path);

			var d = XElement.Load(path);

			_outMap = new Map ((int)d.Attribute("width"), (int)d.Attribute("height"));
			
			// Load Tsx'es
			_tsxs = (from ts in d.Elements("tileset")
			                select LoadTsx(Path.Combine(Path.GetDirectoryName(path),(string)ts.Attribute("source")),
			                                (int)ts.Attribute("firstgid")-1)).ToList();


			// Load and store layers.
			_layers = (from l in d.Elements("layer")
			           select LoadTmxLayer(l)).ToList();

			// Handle spawns (will be replaced with _emptyGlobalTid)
			LoadAndRemoveSpawns ();
			
			// Flatten layers
			FlattenLayers ();
			
			// Remove unused tilesets
			RemoveUnusedTilesetsAndZeroBase ();
			
			// Output tileset paths
			foreach (Tsx tsx in _tsxs) 
			{
				if (tsx._include)
				{
					_outMap.TilemapNames.Add(tsx._image);
				}
			}
		}

		private void FlattenLayers()
		{
			_flattened = new List<int>(_layers[0]);

			for (int li = 1; li < _layers.Count; li++)
			{
				for (int bi = 0; bi < _outMap.Width * _outMap.Height; ++bi)
				{
					_flattened[bi] = _layers[li][bi] != 0 ? _layers[li][bi] : _flattened[bi];
				}
			}

			_layers = null;
		}

		private void RemoveUnusedTilesetsAndZeroBase()
		{
			List<Tsx> optlist = new List<Tsx> ();
			foreach (Tsx tsx in _tsxs) 
			{
				if(tsx._include)
				{
					optlist.Add(tsx);
				}
			}
			
			// Replace the old Tsx list
			_tsxs = optlist;

			foreach(int gid in _flattened)
			{
				Tsx tsx = null;
				int flatIndex = int.MaxValue;

				if (gid > 0)
				{
					int lastValidTsi = -1;
					for (int tsi = 0; tsi < _tsxs.Count; ++tsi)
					{
						if (gid >= _tsxs[tsi]._firstgid)
						{
							lastValidTsi = tsi;
						}
					}

					tsx = _tsxs[lastValidTsi];
					flatIndex = (int)Math.Max (0, ((int)gid - tsx._firstgid - 1));
				}

				_outMap.Blocks.Add(new MapBlock(flatIndex != int.MaxValue ? tsx._blk[flatIndex + 1] : BlockType.Empty,
				                                (ushort)flatIndex));
			}
		}

		private void LoadAndRemoveSpawns ()
		{
			bool foundBase = false;
			bool foundEnemy = false;
			for (int li = 0; li < _layers.Count; ++li) 
			{
				for (int i = 0; i < _outMap.Width * _outMap.Height; i++) 
				{
					int tx = i % _outMap.Width;
					int ty = i / _outMap.Width;
					int gid = _layers[li][i];
					
					int tsIndex = -1;
					for (int tsi = 0; tsi < _tsxs.Count; ++tsi) 
					{
						if (gid >= _tsxs[tsi]._firstgid) 
						{
							tsIndex = tsi;
						}
					}
					
					if (tsIndex < 0 || tsIndex >= _tsxs.Count)
					{
						Console.WriteLine ("MapLoader: Bad tsx index found for gid " + gid + " at " + tx + ", " + ty + ", index " + tsIndex);
					}
					
					int localTileId = gid - _tsxs[tsIndex]._firstgid;
					
					// Does the Tsx say this tile contains an object instance/generator?
					
					Dictionary<int, SpawnType> objSpawns = _tsxs[tsIndex]._spawnTypes;
					
					SpawnType spawnType = SpawnType.Nothing;
					if (!objSpawns.ContainsKey(localTileId)) 
					{
						if (!_tsxs[tsIndex]._include) 
						{
							_layers[li][i] = 0;
						}
						
						continue;
					} 
					else 
					{
						spawnType = objSpawns[localTileId];
					}
					
					if (spawnType != SpawnType.Nothing) 
					{
						if (spawnType == SpawnType.Base) 
						{
							foundBase = true;
						}
						else if (spawnType == SpawnType.Enemy) 
						{
							foundEnemy = true;
						}
						
						_layers[li][i] = 0;
						
//                    Console.WriteLine("Spawn " + spawnType + " at " + tx + "," + ty);
						_outMap.Spawns.Add(new MapSpawn (tx, ty, spawnType));
					}
				}
			}
			
			if (!foundBase) 
			{
				throw new Exception ("No base found.");
			}
			if (!foundEnemy) 
			{
				throw new Exception ("No enemy spawn point found.");
			}
		}

		private int[] LoadTmxLayer (XElement el)
		{
			if((int)el.Attribute("width") != _outMap.Width)
			{
				throw new Exception("Map layer isn't same width as map!");
			}
			
			XElement map = el.Elements("data").Where(d => (string)d.Attribute("encoding")=="base64" && (string)d.Attribute("compression")=="gzip").SingleOrDefault();
			
			if (map == null)
			{
				throw new Exception ("Can't find map data.");
			}

			byte[] bytes = Util.DecodeBase64(map.Value.Trim());
				
			if (bytes == null)
			{
				throw new Exception ("Decoded layer data is null.");
			}

			// The data can be gzip compressed.
			using(MemoryStream ms = new MemoryStream(bytes))
			using(GZipStream gzs = new GZipStream(ms,CompressionMode.Decompress))
			{
				byte[] unbytes = new byte[_outMap.Width * _outMap.Height * 4];
				int actuallyread = gzs.Read(unbytes, 0, (int)unbytes.Length);

				if(actuallyread != unbytes.Length)
					throw new Exception("Failed to uncompress all the expected layer data.");

				ms.Dispose();
				bytes = unbytes;
			}

			int[] layer = new int[_outMap.Width * _outMap.Height];
			for (int i = 0; i < _outMap.Width*_outMap.Height*4; ++i)
			{
				layer[i/4] |= bytes[i] << (8 * (i & 3));
			}

			return layer;
		}

		private Tsx LoadTsx (string path, int firstgid)
		{
			Console.WriteLine ("MapLoader: Loading tileset " + path);
			var d = XElement.Load(path);

			Tsx tsx = new Tsx();
			tsx._name = path;
			tsx._include = path.Trim().Length > 0 && !path.Contains("spawns");
			tsx._firstgid = firstgid;
			tsx._image = Path.GetFileNameWithoutExtension((string)d.Element("image").Attribute("source"));

			var btdefs =	from t in d.Elements("tile")
							from p in t.Element("properties").Elements("property")
							where (string)p.Attribute("name") == "blocktype"
							select new
							{
								Id = (int)t.Attribute("id") + 1,
								Type = (BlockType)Enum.Parse(typeof(BlockType), (string)p.Attribute("value"), true)
							};

			foreach(var btdef in btdefs)
			{
				tsx._blk[btdef.Id] = btdef.Type;
			}

			var stdefs =	from t in d.Elements("tile")
							from p in t.Element("properties").Elements("property")
							where (string)p.Attribute("name") == "spawn"
							select new
							{
								Id = (int)t.Attribute("id") + 1,
								Type = (SpawnType)Enum.Parse(typeof(SpawnType), (string)p.Attribute("value"), true)
							};

			foreach(var stdef in stdefs)
			{
				tsx._spawnTypes[stdef.Id] = stdef.Type;
			}

			return tsx;
		}
	}
}
