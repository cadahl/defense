namespace Client
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using Util;
	using Client.GameObjects;
	using Client.Graphics;

	public class Wave : ICloneable
	{
		public object Clone ()
		{
			return MemberwiseClone();
		}

		public string Text;
		public string Unit;
		public int SpawnInterval;
		public int NextWaveDelay;
		public int Count;
		public float HpMultiplier; 
	}

	public class WaveGenerator
	{
		public delegate void WaveEventHandler(Wave current, Wave next, int extra);
		public event WaveEventHandler WaveCountdown;
		public event WaveEventHandler WaveStarted;

		public Wave CurrentWave, NextWave;
		private Game _game;
		private IEnumerator<Wave> _waves;
		private Timer _nextWaveTimer, _nextSpawnTimer;
		private float _cumulativeDifficulty = 1.0f;

		public WaveGenerator (Game game)
		{
			_game = game;

			LoadWaves();

			_cumulativeDifficulty = .5f;

			_nextWaveTimer = new Timer(TimerMode.CountDown, 1);
			_nextWaveTimer.Elapsed += delegate
			{
				if(!_waves.MoveNext())
				{
					LoadWaves();
					_waves.MoveNext();
				}

				CurrentWave = NextWave;
				NextWave = (Wave)_waves.Current.Clone();

				_nextSpawnTimer.ResetValue = CurrentWave.SpawnInterval * 60;
				_nextWaveTimer.ResetValue = CurrentWave.NextWaveDelay * 60;
				_nextWaveTimer.Reset();

				if(WaveStarted != null)
					WaveStarted(CurrentWave, NextWave, 0);

				_cumulativeDifficulty += 0.35f;
			};
			_nextWaveTimer.Ticked += delegate(Timer timer)
			{
				if(timer.Value < 10*60)
				{
					if(WaveCountdown != null)
						WaveCountdown(CurrentWave, NextWave, timer.Value/60+1);
				}
			};

			_nextSpawnTimer = new Timer(TimerMode.CountDown, 1);
			_nextSpawnTimer.Elapsed += delegate
			 {
				if(CurrentWave.Count > 0)
				{
					CurrentWave.Count--;

					var es = _game.Map.Spawns.Where(s => s.Type == SpawnType.Enemy).First();
					SpawnUnit(CurrentWave.Unit, es.BlockX*32+16, es.BlockY*32+16, _cumulativeDifficulty);

					_nextSpawnTimer.Reset();
				}
			};
		}

		private void LoadWaves()
		{
			_waves = (
				from w in _game.Config.Element("waves").Elements("wave")
				select new Wave()
				{
					Text = (string)w.Attribute("text"),
					Unit = (string)w.Attribute("unit"),
					SpawnInterval = (int)w.Attribute("interval"),
					Count = (int)w.Attribute("count"),
					HpMultiplier = (float)w.Attribute("hpmult"),
					NextWaveDelay = (int)w.Attribute("nextwavedelay")
				}).GetEnumerator();

			_waves.MoveNext();
			NextWave = (Wave)_waves.Current.Clone();
		}

		private void SpawnUnit(string name, float x, float y, float difficulty)
		{
			_game.AddObject((	from v in _game.Config.Element("units").Elements("vehicle")
					            where (string)v.Attribute("id") == name
					            select new Vehicle(_game)
					            {
									X = x,
									Y = y,
									Template = new SpriteTemplate("units",
					                                              (int)v.Attribute("sx"),
					                                              (int)v.Attribute("sy"),
					                                              (int)v.Attribute("sw"),
					                                              (int)v.Attribute("sh")),
									MaxSpeed = (float)v.Attribute("speed"),
									HitPoints = (int)((int)v.Attribute("hp") * difficulty),
									Reward = (int)v.Attribute("reward")
								}).Single());
		}

		public void Update()
		{
			_nextWaveTimer.Tick();
			_nextSpawnTimer.Tick();
		}
	}
}
