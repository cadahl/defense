namespace Util
{
	using System;

	public class PidController
	{
		public float ProportionalGain=1.0f, IntegralGain=1.0f, DerivativeGain=1.0f;
		public float SetPoint;
		public float Value { get; private set; }
		private float _summedError, _prevError;

		public PidController (float initialValue)
		{
			Value = initialValue;
		}

		public PidController (float initialValue, float pg, float ig, float dg)
		{
			Value = initialValue;
			ProportionalGain = pg;
			IntegralGain = ig;
			DerivativeGain = dg;
		}
		
		public void Update()
		{
			float error = SetPoint-Value;

			float p = ProportionalGain * error;

			_summedError += error;
			float i = IntegralGain * _summedError;
			
			float d = DerivativeGain * (error - _prevError);
			_prevError = error;
			
			Value += p+i+d;
		}

	}
}
