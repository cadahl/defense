namespace Client.Sim 
{
	using System;
	using System.Collections.Generic;
	using Client;
	using Util;
	
	public abstract class GameObject
	{
		private class Action
		{
			public Action(int delay, int interval, System.Func<long,bool> function)
			{
				_ticks = -delay;
				_function = function;
				_interval = Math.Max(1,interval);
			}
			
			public int _ticks;
			public int _interval;
			public System.Func<long,bool> _function;
		}

		private List<Action> _addedActions = new List<Action>();
		private List<Action> _removedActions = new List<Action>();
		private List<Action> _runningActions = new List<Action>();

		protected Game _game;
		public int Uid;
		public string TypeId;
	    public float X;
	    public float Y;
	    public abstract float Radius { get; }

		public int Angle { get;set; }
		public int UpdatePriority { get; private set; }
		
		//protected delegate bool ActionFunc(long time);

		private bool _disposed;

		protected void AddAction(int delay, int interval, System.Func<long,bool> func)
		{
			_addedActions.Add( new Action(delay, interval, func) );
		}
		
	    public GameObject(Game game, int updatePriority, string typeId) 
		{
	        UpdatePriority = updatePriority;
			_game = game;
			X = 0;
			Y = 0;
			TypeId = typeId;
	    }

	    public virtual ObjectStatePacket Update(long ticks)
		{
			if(_disposed)
			{
				throw new ObjectDisposedException("GameObject");
			}

			foreach(Action a in _addedActions)
			{
				_runningActions.Add(a);
			}
			_addedActions.Clear();
			
			foreach(Action a in _removedActions)
			{
				_runningActions.Remove(a);
			}
			_removedActions.Clear();
			
			foreach(Action a in _runningActions)
			{
				int oldInterval = a._ticks/a._interval;
				a._ticks++;
				int newInterval = a._ticks/a._interval;
				if(a._ticks >= 0 && newInterval > oldInterval)
				{
					if(!a._function(newInterval))
					{
						_removedActions.Add(a);
					}
				}
			}
			
			return null;
		}
		
		protected ObjectStatePacket NewState()
		{
			var osp = new ObjectStatePacket();
			osp.Uid = Uid;
			osp.TypeId = TypeId;
			osp.Position.X = X;
			osp.Position.Y = Y;
			osp.Angle = Angle;
			return osp;
		}
		
	    public virtual void Render() {}
	}
}
