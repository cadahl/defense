namespace Client
{
	using System;
	using System.Linq;
	using System.Xml.Linq;
	using System.Collections.Generic;
	using System.Drawing;
	using Client.Graphics;
	using Client.GameObjects;
	using Util;

	public partial class Game : GameObjects.ObjectDatabase
	{
		public Application Application	{ get; private set; }

		public Map Map;
		public int Cash;
		public Navigation Navigation { get; private set; }
		public bool ShowNavigationMap { get; private set; }
		public bool ShowVehicleProbes { get; private set; }
		public long UpdateCount { get; private set; }

		public Buildable GetBuilding(int bx, int by)
		{
			return _buildings[bx + by * Map.Width];
		}


		private List<Buildable> _buildings;
		private List<Point> _blockers;
		private Camera _camera;
		private List<Collider> _colliders = new List<Collider> ();
		private UI.Hud _hud;
		private MapSpawn _baseSpawn;
		private List<MapSpawn> _enemySpawns;
		private WaveGenerator _waveGenerator;

		public Game(Application app)
		{
			Application = app;
			LoadConfig();

			_hud = new UI.Hud (this);

			LoadMap("data/testmap6.tmx");
			_buildings = new List<Buildable>(Enumerable.Repeat((Buildable)null, Map.Width*Map.Height));
			_blockers = new List<Point>();
			
			_baseSpawn = Map.Spawns.Find(sp => sp.Type == SpawnType.Base);
			_enemySpawns = Map.Spawns.FindAll(sp => sp.Type == SpawnType.Enemy);
			
			Navigation = new Navigation(Map, _baseSpawn.BlockX, _baseSpawn.BlockY);
			
			AddObject( new VehicleHealthBars(this));		

			_waveGenerator = new WaveGenerator(this);

			_waveGenerator.WaveCountdown += delegate(Wave current, Wave next, int extra)
			{
				_hud.ShowNextWaveWarning(next.Text, extra);
			};
			_waveGenerator.WaveStarted += delegate(Wave current, Wave next, int extra)
			{
				_hud.ShowNextWaveWarning("",-1);
			};

			// Load game mode settings
			string modeName = "normal";
			try
			{
				var mode = Config.Elements("mode").Where(m => (string)m.Attribute("id") == modeName).Single();

				Cash = (int)mode.Attribute("startcash");
			}
			catch(InvalidOperationException ioe)
			{
				Console.WriteLine("Game: Couldn't load mode settings.");
			}


			app.Input.KeyDown += delegate(OpenTK.Input.Key key)
			{
				if(key == OpenTK.Input.Key.F1)
				{
					ShowNavigationMap = !ShowNavigationMap;
				}
				else if(key == OpenTK.Input.Key.F2)
				{
					ShowVehicleProbes = !ShowVehicleProbes;
				}
			};
			
			app.Input.MouseWheelChanged += delegate(int delta)
			{
				app.Renderer.ZoomLevel = Util.Clamp(1.0f,2.0f,app.Renderer.ZoomLevel+delta/10.0f);
			};
		}
				
		private void LoadMap(string path)
		{
			_camera = new Camera();
						
			// Load map.
			Map = new MapLoader(path).Map;

			// Tell renderer to load the tilemaps.
			var r = Application.Renderer;
			foreach (string tilemapName in Map.TilemapNames)
			{
				r.Tilemaps[tilemapName] = new Tilemap(tilemapName);
			}
			
			r.Backgrounds[0] = new Background (r, Map);
			r.Backgrounds[1] = new Background (r, "units", Map.Width, Map.Height);
			r.Backgrounds[1].Blended = true;
		}

		public void AddCollider(Collider col)
		{
			lock(_colliders)
			{
				_colliders.Add(col);
			}
		}
		
		public void Update (double time)
		{
			if (Map == null)
			{
				return;
			}

			_waveGenerator.Update();
			_hud.Update();

			CheckHits();

			_camera.Update (this);

			AddAndRemoveObjects();
			UpdateObjects(UpdateCount);
			
			_hud.UpdateUps(time);
			
			UpdateCount++;			
		}

		private void CheckHits()
		{
			foreach(Collider c in _colliders)
			{


				List<ObjectAndDistance<GameObject>> hitObjects;
				switch(c.Type)
				{
				case ColliderType.Point:
					hitObjects = FindObjectsWithinRadius(null, c.X, c.Y, 16, c.FilterType);
					
					if(c.Handler != null && hitObjects.Count > 0)
						c.Handler(c, hitObjects);
					
					break;
					
				case ColliderType.Circle:
					hitObjects = FindObjectsWithinRadius(null, c.X, c.Y, c.X2 + 16, c.FilterType);

					if(c.Handler != null && hitObjects.Count > 0)
						c.Handler(c, hitObjects);
					
					break;
					
				case ColliderType.LineFirst:
				
					GameObject closest = null;
					float closestDist = float.MaxValue;
	
					List<ObjectAndDistance<GameObject>> hits = new List<ObjectAndDistance<GameObject>>();
					
					foreach(GameObject v in _objects)
					{
						if(!(c.FilterType.IsAssignableFrom(v.GetType())))
							continue;
			
						if(Util.LineIntersectsCircle(c.X, c.Y, c.X2, c.Y2, v.X, v.Y, 16.0f))
						{
							float d = Util.Distance(c.X, c.Y, v.X, v.Y);
							if(d < closestDist)
							{
								closest = v;
								closestDist = d;
							}
						}
					}
					
					if(closest != null)
						hits.Add(new ObjectAndDistance<GameObject>(closest, 0.0f));
					
					if(c.Handler != null && hits.Count > 0)
						c.Handler(c, hits);
					
					break;
				case ColliderType.LineAll:
/*				
					foreach(GameObject v in _objects)
					{
						if(!(v is Vehicle))
							continue;
			
						if(Util.LineIntersectsCircle(c.X, c.Y, c.X2, c.Y2, v.X, v.Y, 16.0f))
						{
							hits.Add(v);
						}
					}
					
					if(c.Handler != null && hits.Count > 0)
						c.Handler(c, hits);
				*/	break;
				}
			}
			
			_colliders.Clear();
		}
		
		public bool HasBuilding(int bx, int by)
		{
			int i = bx + by*Map.Width;
			return _buildings[i] != null;
		}
		
		public bool BuildAt(int bx, int by, Type type)
		{
			int i = bx + by*Map.Width;
			
			if(_buildings[i] == null)
			{
				_blockers.Add(new Point(bx,by));
				
				if(Navigation.Rebuild(_blockers))
				{
					_buildings[i] = (Buildable)System.Activator.CreateInstance(type, new object[] { this, bx*32+16, by*32+16 });
					_buildings[i].Upgrades = GetUpgradeInfos(type);
					_buildings[i].CurrentUpgradeIndex = 0;
					AddObject(_buildings[i]);
					Cash -= _buildings[i].CurrentUpgrade.Price;
					return true;
				}
				else
				{
					_blockers.RemoveAt(_blockers.Count-1);
				}
			}

			return false;
		}

		public void Upgrade(Buildable building)
		{
			int i = _buildings.FindIndex(b => b == building);

			if(i < 0)
				return;

			int bx = i % Map.Width;
			int by = i / Map.Width;

			if(building.CurrentUpgradeIndex < building.Upgrades.Length-1)
			{
				_buildings[i].CurrentUpgradeIndex++;
				Cash -= _buildings[i].CurrentUpgrade.Price;
			}
		}

		public void Sell(Buildable building)
		{
			int i = _buildings.FindIndex(b => b == building);

			if(i < 0)
				return;

			int bx = i % Map.Width;
			int by = i / Map.Width;

			_blockers.Remove(new Point(bx,by));

			Console.WriteLine("sell...");

			if(Navigation.Rebuild(_blockers))
			{
				Console.WriteLine("sell!");
				RemoveObject(_buildings[i]);
				Cash += _buildings[i].CurrentUpgrade.Price;
				_buildings[i] = null;
			}
			else
			{
				Console.WriteLine("no sell");
				_blockers.Add(new Point(bx,by));
			}
		}
	
		public void Render()
		{
			lock(_objects)
			{
				foreach (GameObject obj in _objects) 
				{
					obj.Render();
				}
			}

			Renderer r = Application.Renderer;

			if(ShowNavigationMap)
			{
				RenderNavigationMap(r);
			}

			// Update HUD GUI
			_hud.UpdateFps();
			_hud.Render ();
		}
		
		
		public float LevelMouseX
		{
			get 
			{
				Background bg = Application.Renderer.Backgrounds[0];
				return Application.Input.MouseX + bg.HScroll;
			}
		}

		public float LevelMouseY
		{
			get 
			{
				Background bg = Application.Renderer.Backgrounds[0];
				return Application.Input.MouseY + bg.VScroll;
			}
		}
	}
}

