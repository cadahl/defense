namespace Util
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;

	public partial class Map
	{
		private class Tsx
		{
			public bool SpawnsOnly;
			public string TilemapName;
			public int FirstGid;
			public int LastGid;
			public Dictionary<int, SpawnType> SpawnTypes;
			public Dictionary<int,BlockType> BlockTypes;
		}

		public Map(string path)
		{
			Console.WriteLine ("MapLoader: Loading level " + path);

			var d = XElement.Load(path);

			Width = (int)d.Attribute("width");
			Height = (int)d.Attribute("height");
			
			
			Spawns = new List<MapSpawn>();
			
			// Load Tsx'es
			string dir = Path.GetDirectoryName(path);
			var tsxs = 	d.Elements("tileset")
						.Select( ts => LoadTsx( Path.Combine( dir, (string)ts.Attribute("source") ), (int)ts.Attribute("firstgid")-1) )
						.ToList();
			
			// Calculate lastGid for each tileset.
			for(int ti = 0; ti < tsxs.Count-1; ++ti)
			{
				tsxs[ti].LastGid = tsxs[ti+1].FirstGid-1;
			}
			

			// Load layers.
			var layers = (from l in d.Elements("layer")
			    	       select LoadTmxLayer(l)).ToList();			

			LoadAndRemoveSpawns (layers, tsxs);
			
			RemoveUnusedTilesetsAndZeroBase (FlattenLayers (layers), tsxs);
		}

		private List<int> FlattenLayers(List<List<int>> layers)
		{
			if(layers == null || layers.Count == 0)
				throw new Exception("FlattenLayers: No layers!");
			
			var flattened = new List<int>(layers[0]);
			
			for(int li = 1; li < layers.Count; ++li)
			{
				for (int bi = 0; bi < Width * Height; ++bi)
				{
					flattened[bi] = layers[li][bi] != 0 ? layers[li][bi] : flattened[bi];
				}
			}
			
			return flattened;
		}

		private void RemoveUnusedTilesetsAndZeroBase(List<int> flattened, List<Tsx> tsxs)
		{
			Blocks = new List<MapBlock>();

			var emptyBlock = new MapBlock(BlockType.Empty, 0);
			
			Blocks = (	from gid in flattened
						let tsx = tsxs.Find(t => t.SpawnsOnly && gid >= t.FirstGid && gid <= t.LastGid)
						let localId = (int)Math.Max (0, ((int)gid - tsx.FirstGid - 1))
						select gid == 0 ? emptyBlock : new MapBlock(tsx.BlockTypes[localId + 1], (ushort)localId)
			          ).ToList();

			// Output tileset paths
			TilemapNames = (from tsx in tsxs where tsx.SpawnsOnly select tsx.TilemapName).ToList();
		}

		private void LoadAndRemoveSpawns (List<List<int>> layers, List<Tsx> tsxs)
		{
			foreach(var layer in layers) 
			{
				for (int i = 0; i < Width * Height; i++) 
				{
					int tx = i % Width;
					int ty = i / Width;
					int gid = layer[i];
					
					var tsx = tsxs.Find(t => !t.SpawnsOnly && gid >= t.FirstGid && gid <= t.LastGid);
					if (tsx != null)
					{
						SpawnType spawnType = SpawnType.Nothing;
						if (tsx.SpawnTypes.TryGetValue(gid - tsx.FirstGid, out spawnType)) 
						{
							Spawns.Add(new MapSpawn (tx, ty, spawnType));
						}
						
						layer[i] = 0;
					}
				}
			}
			
			Base = Spawns.Find(s => s.Type == SpawnType.Base);
			if (Base == null) 
			{
				throw new Exception ("No base found.");
			}

			EnemySpawns = Spawns.FindAll(s => s.Type == SpawnType.Enemy);
				
			if (EnemySpawns == null || EnemySpawns.Count == 0) 
			{
				throw new Exception ("No enemy spawn point found.");
			}
		}

		private List<int> LoadTmxLayer (XElement el)
		{
			if((int)el.Attribute("width") != Width)
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

			// The data is gzip compressed.
			using(MemoryStream ms = new MemoryStream(bytes))
			using(GZipStream gzs = new GZipStream(ms,CompressionMode.Decompress))
			{
				byte[] unbytes = new byte[Width * Height * 4];
				int actuallyread = gzs.Read(unbytes, 0, (int)unbytes.Length);

				if(actuallyread != unbytes.Length)
					throw new Exception("Failed to uncompress all the expected layer data.");

				ms.Dispose();
				bytes = unbytes;
			}

			var layer = new List<int>();
			for (int i = 0; i < Width*Height; ++i)
			{
				layer.Add(BitConverter.ToInt32(bytes, i*4));
			}

			return layer;
		}
		
		private Tsx LoadTsx (string path, int firstgid)
		{
			Console.WriteLine ("MapLoader: Loading tileset " + path);
			var d = XElement.Load(path);

			Tsx tsx = new Tsx()
			{
				SpawnsOnly = path.Trim().Length > 0 && !path.Contains("spawns"),
				FirstGid = firstgid,
				LastGid = int.MaxValue,
				TilemapName = Path.GetFileNameWithoutExtension((string)d.Element("image").Attribute("source")),
	
				BlockTypes = (	from t in d.Elements("tile")
								from p in t.Element("properties").Elements("property")
								where (string)p.Attribute("name") == "blocktype"
								select new
								{
									Id = (int)t.Attribute("id") + 1,
									Type = (BlockType)Enum.Parse(typeof(BlockType), (string)p.Attribute("value"), true)
								})
								.ToDictionary(bt => bt.Id, bt => bt.Type),

				SpawnTypes = (	from t in d.Elements("tile")
								from p in t.Element("properties").Elements("property")
								where (string)p.Attribute("name") == "spawn"
								select new
								{
									Id = (int)t.Attribute("id") + 1,
									Type = (SpawnType)Enum.Parse(typeof(SpawnType), (string)p.Attribute("value"), true)
								})
								.Where(st => st.Type != SpawnType.Nothing)
								.ToDictionary(st => st.Id, st => st.Type)
			};
			
			return tsx;
		}
	}
}
