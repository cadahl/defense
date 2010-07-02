namespace Util
{
	using System;

	public enum TimerMode { CountDown = 0, CountUp = 1 }
	public class Timer
	{
		public delegate void EventHandler(Timer timer);
		public event EventHandler Elapsed;
		public event EventHandler Ticked;

		public TimerMode Mode { set; get; }
		
		public int ResetValue { get { return _resetValue; } set { _resetValue = value; } }
		
		public Timer(TimerMode mode)
		{
			Mode = mode;
			Reset();
		}

		public Timer(TimerMode mode, int resetValue)
		{
			Mode = mode;
			_resetValue = resetValue;
			Reset();
		}
		
		public void Reset()
		{
			Value = Mode == TimerMode.CountDown ? _resetValue : 0;
		}

		public int Value { get; private set; }

		public float Progress
		{
			get 
			{
				return ((float)Value)/_resetValue;
			}
		}
	
		public bool IsElapsed 
		{ 
			get 
			{ 
				return (Mode == TimerMode.CountDown && Value == 0) || (Mode == TimerMode.CountUp && Value == _resetValue);
			} 
		}

		public void Tick()
		{
			if(Mode == TimerMode.CountDown)
			{
				if(Value > 0)
					Value--;
			}
			else
			{
				if(Value < _resetValue)
					Value++;
			}

			if(IsElapsed)
			{
				if(Elapsed != null)
					Elapsed(this);
			}
			else
			{
				if(Ticked != null)
					Ticked(this);
			}
		}
	
		private int _resetValue;
	}
}
