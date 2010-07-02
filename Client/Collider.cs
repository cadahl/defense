namespace Client
{
	using System;
	using System.Collections.Generic;
	using Client.Sim;
	
	public delegate void CollisionHandler(Collider c, IEnumerable<ObjectAndDistance> objects);
	
	public enum ColliderType
	{
		Point, Circle, LineFirst, LineAll
	}
	
	public class Collider
	{
		public static Collider Point(float x, float y, CollisionHandler handler, Object tag)
		{
			return new Collider()
			{
				Type = ColliderType.Point,
				X = x,
				Y = y,
				Handler = handler,
				Tag = tag,
				FilterType = typeof(GameObject)
			};
		}
		
		public static Collider Circle(float x, float y, float radius, CollisionHandler handler, Object tag)
		{
			return new Collider()
			{
				Type = ColliderType.Circle,
				X = x,
				Y = y,
				X2 = radius,
				Handler = handler,
				Tag = tag,
				FilterType = typeof(GameObject)
			};
		}
		
		public static Collider LineFirst(float x, float y, float x2, float y2, CollisionHandler handler, Object tag)
		{
			return new Collider()
			{
				Type = ColliderType.LineFirst,
				X = x,
				Y = y,
				X2 = x2,
				Y2 = y2,
				Handler = handler,
				Tag = tag,
				FilterType = typeof(GameObject)
			};
		}
		
		public static Collider LineAll(float x, float y, float x2, float y2, CollisionHandler handler, Object tag)
		{
			return new Collider()
			{
				Type = ColliderType.LineAll,
				X = x,
				Y = y,
				X2 = x2,
				Y2 = y2,
				Handler = handler,
				Tag = tag,
				FilterType = typeof(GameObject)
			};
		}
		
		public ColliderType Type;
		public float X, Y, X2, Y2;
		public CollisionHandler Handler;
		public Object Tag;
		public Type FilterType;
	}
}
