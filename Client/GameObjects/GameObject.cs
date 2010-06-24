namespace Client.GameObjects 
{
	using System;
	using System.Collections.Generic;
	using Client;
	using Util;
	
	public abstract class GameObject : IDisposable
	{
		private class Action
		{
			public Action(int delay, ActionFunc function)
			{
				_ticks = -delay;
				_function = function;
			}
			
			public int _ticks;
			public ActionFunc _function;
		}

		private List<Action> _addedActions = new List<Action>();
		private List<Action> _removedActions = new List<Action>();
		private List<Action> _runningActions = new List<Action>();

		protected Game _game;
	    public float X;
	    public float Y;
	    public abstract float Radius { get; }

		public int Angle { get;set; }
		public int UpdatePriority { get; private set; }
		
		protected delegate bool ActionFunc(long time);

		private bool _disposed;

		protected void AddAction(int delay, ActionFunc func)
		{
			_addedActions.Add( new Action(delay, func) );
		}
		
	    public GameObject(Game game, int updatePriority) 
		{
	        UpdatePriority = updatePriority;
			_game = game;
			X = 0;
			Y = 0;
	    }

		protected virtual void Dispose(bool disposing)
		{
		    // Use our disposed flag to allow us to call this method multiple times safely.
		    // This is a requirement when implementing IDisposable
		    if (!_disposed)
		    {
		        if (disposing)
		        {
		            // If we have any managed, IDisposable resources, Dispose of them here.
		            // In this case, we don't, so this was unneeded.
		            // Later, we will subclass this class, and use this section.
		        }

				// native
		    }

			// Mark us as disposed, to prevent multiple calls to dispose from having an effect,
		    // and to allow us to handle ObjectDisposedException
		  	_disposed = true;
		}

		public void Dispose()
	    {
	        // We start by calling Dispose(bool) with true
	        Dispose(true);
	        // Now suppress finalization for this object, since we've already handled our resource cleanup tasks
	        GC.SuppressFinalize(this);
	    }

	    public virtual void Update(long ticks)
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
				if(a._ticks++ >= 0)
				{
					if(!a._function(a._ticks))
					{
						_removedActions.Add(a);
					}
				}
			}
		}
		
	    public virtual void Render() {}
	}
}
