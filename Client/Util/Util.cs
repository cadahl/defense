namespace Util
{
	using System;
	
	public class Angles
	{
		public static int Mask = 4095;
		
		public static int TwoPi = 4096;
		public static int Pi = 2048;
		
		public static void AngleToDirection(float a, out float dx, out float dy)
		{
			double aa = (2.0 * Math.PI * a) / TwoPi;
			dx = (float)Math.Cos(aa);
			dy = (float)Math.Sin(aa);
		}
		
		public static int Difference(int a, int a2)
		{
			if(a == a2)
				return 0;
			
			float dx,dy,dx2,dy2;
			AngleToDirection(a, out dx, out dy);
			AngleToDirection(a2, out dx2, out dy2);

			float dot = dx*dx2+dy*dy2;
			double aout = Math.Acos(dot);
			int aouti = ((int)(4096.0 * aout / (2.0*Math.PI)));
			
			return aouti;
		}
	}
	
	public class Util
	{
		public static float Clamp( float mn, float mx, float v)
		{
			return v > mx ? mx : (v < mn ? mn : v);
		}
		public static int Clamp( int mn, int mx, int v)
		{
			return v > mx ? mx : (v < mn ? mn : v);
		}
		public static float Saturate(float x)
		{
			return x > 0.0f ? (x < 1.0f ? x : 1.0f) : 0.0f;
		}
		
		public static int Lerp(int a, int b, float k)
		{
			return (int)(a * (1.0f-k) + b * k);
		}
		
		public static float Lerp(float a, float b, float k)
		{
			return (a * (1.0f-k) + b * k);
		}
		
		public static int LerpAngle(int a, int b, float k)
		{
			a &= Angles.Mask;
			b &= Angles.Mask;
			
			if(Math.Abs(a-b) >= Angles.Pi)
			{
				if(a < b)
					b -= Angles.TwoPi;
				else
					a -= Angles.TwoPi;
			}
			
			return Lerp(a,b,k) & Angles.Mask;
		}
		
		public static float Smoothstep(float edge0, float edge1, float x)
		{
		    // Scale, bias and saturate x to 0..1 range
		    x = Saturate((x - edge0) / (edge1 - edge0)); 
		    // Evaluate polynomial
		    return x*x*(3.0f-2.0f*x);
		}
		
		public static float Distance (float x0, float y0, float x1, float y1)
		{
			return (float)Math.Sqrt ((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
		}

		public static float[] RandomInCircle(float radius)
		{
			float[] v = new float[2];
			
			do 
			{
				v[0] = Random() * radius * 2.0f - radius;
				v[1] = Random() * radius * 2.0f - radius;

			} while(v[0] * v[0] + v[1] * v[1] > radius * radius);
			
			return v;
		}

		public static string EncodeBase64(byte[] data)
		{
			try {
				return Convert.ToBase64String(data);
			} catch (Exception e) {
				throw new Exception ("Error in base64Encode" + e.Message);
			}
		}
		
		public static byte[] DecodeBase64 (string data)
		{
			try {
				return Convert.FromBase64String(data);
			} catch (Exception e) {
				throw new Exception ("Error in base64Decode" + e.Message);
			}
		}
		
		public static int NextPow2(int n)
		{
			n = n - 1;
			n = n | (n >> 1);
			n = n | (n >> 2);
			n = n | (n >> 4);
			n = n | (n >> 8);
			n = n | (n >> 16);
			return n = n + 1;
		}

		private static Random _rng = new Random();
		
		public static float Random()
		{
			return (float)_rng.NextDouble();
		}
		
		public static float Random(float min, float max)
		{
			return (float)_rng.NextDouble() * (max-min) + min;
		}
		
		public static int RandomInt(int min, int max)
		{
			return _rng.Next(min,max);
		}
		
		public static int RandomSign()
		{
			return _rng.Next(1000) >= 500 ? 1 : -1;
		}

		public static bool RandomBool()
		{
			return _rng.Next(1000) >= 500;
		}
	
	    public static int DeltasToAngle(float dx, float dy) 
		{
			if (dx == 0 && dy == 0) 
			{
			    return -1; // default, ska inte hända
			}
			
			return (int)(Math.Atan2(dy, dx)*2048.0/Math.PI) & 4095;
	    }
		
		public static int AngleToFacingFraction(int angle) 
		{
			int octi = angle;
			octi += 2048 + 256;
			int frac = octi % (4096/8);
			return frac-256;
	    }

		public static bool LineIntersectsCircle(float x, float y, float x2, float y2, float cx, float cy, float cr)
		{
		  // Math link:
		  // http://mathworld.wolfram.com/Circle-LineIntersection.html

			x -= cx;
			y -= cy;
			x2 -= cx;
			y2 -= cy;
		
			float lineLength = Distance(x,y,x2,y2);
		  
			// Determinant
		  	float D = (x * y2) - (x2 * y);
		  
		  	float result = (cr * cr) * (lineLength * lineLength) - (D * D); 

			return result > 0;
		}		
	}
}
