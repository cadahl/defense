namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
	using System.Linq;
	using Util;
	
	public class ObjectAndDistance
	{
		public ObjectAndDistance(GameObject obj, float distance)
		{
			Debug.Assert(obj != null);
			Distance = distance;
			Object = obj;
		}
		
		
		public float Distance;
		public GameObject Object;
	}

	public class SpatialHash
	{
		private int _cellSize, _gridWidth, _gridHeight;
		private HashSet<GameObject>[] _grid;
		
		public SpatialHash(float width, float height, int cellSize)
		{
			_cellSize = cellSize;
			_gridWidth = (int)Math.Ceiling(width)/_cellSize+1;
			_gridHeight = (int)Math.Ceiling(height)/_cellSize+1;
		
			_grid = new HashSet<GameObject>[_gridWidth*_gridHeight];
			for(int i = 0; i < _grid.Length; ++i)
			{
				_grid[i] = new HashSet<GameObject>();
			}
		}
		
		public void Add(GameObject gob)
		{
			int startx = (int)(gob.X-gob.Radius) / _cellSize;
			int starty = (int)(gob.Y-gob.Radius) / _cellSize;
			int endx = (int)(gob.X+gob.Radius) / _cellSize;
			int endy = (int)(gob.Y+gob.Radius) / _cellSize;

			for(int y = starty; y <= endy; ++y)
			{
				for(int x = startx; x <= endx; ++x)
				{
					_grid[x + y * _gridWidth].Add(gob);
				}
			}
		}
		
		public void Remove(GameObject gob)
		{
			int startx = (int)(gob.X-gob.Radius) / _cellSize;
			int starty = (int)(gob.Y-gob.Radius) / _cellSize;
			int endx = (int)(gob.X+gob.Radius) / _cellSize;
			int endy = (int)(gob.Y+gob.Radius) / _cellSize;

			for(int y = starty; y <= endy; ++y)
			{
				for(int x = startx; x <= endx; ++x)
				{
					_grid[x + y * _gridWidth].Remove(gob);
				}
			}
		}

		public IEnumerable<GameObject> GetNearby(Rectangle rect)
		{
			HashSet<GameObject> result = new HashSet<GameObject>();
			int startx = rect.Left / _cellSize;
			int starty = rect.Top / _cellSize;
			int endx = rect.Right / _cellSize;
			int endy = rect.Bottom / _cellSize;

			for(int y = starty; y <= endy; ++y)
			{
				for(int x = startx; x <= endx; ++x)
				{
					var bucket = _grid[x + y * _gridWidth];
					foreach(var bucketObject in bucket)
						result.Add(bucketObject);
				}
			}
			
			return result;
		}
		
		public IEnumerable<GameObject> GetNearby(float x, float y, float radius)
		{
			return GetNearby(	new Rectangle(	(int)(x-radius), 
			                                    (int)(y-radius), 
			                                    (int)(radius*2), 
			                                    (int)(radius*2)));
		}
		
	}
	
	public class ObjectDatabase
	{		
		protected List<GameObject> _objects = new List<GameObject> ();
		private HashSet<GameObject> _objectAddList = new HashSet<GameObject> ();
		private HashSet<GameObject> _objectRemoveList = new HashSet<GameObject> ();
		private SpatialHash _spatialHash;
		
		public ObjectDatabase ()
		{
		}

		public void SetDimensions(float width, float height)
		{
			_spatialHash = new SpatialHash(width, height, 64);
		}
		
		public int ObjectCount { get { return _objects.Count; } }
		
		public void AddObject (GameObject obj)
		{
			lock(_objectAddList)
			{
				_objectAddList.Add(obj);
			}
		}

		public void RemoveObject (GameObject obj)
		{
			lock(_objectRemoveList)
			{
				_objectRemoveList.Add(obj);
			}
		}

		public void AddAndRemoveObjects()
		{
			// Some objects may request to be added/removed, so do all that now.
			// We do it here because the update loop above would get complicated
			// if objects appear and disappear while updating.

			lock(_objectRemoveList)
			{
				foreach(GameObject obj in _objectRemoveList)
				{
					if (obj != null) 
					{
						lock(_objects)
						{
							_spatialHash.Remove(obj);
							_objects.Remove(obj);
							obj.Dispose();
						}
					}
				}
				_objectRemoveList.Clear();
			}
			
			lock(_objectAddList)
			{
				foreach(GameObject obj in _objectAddList)
				{
					if (obj != null) 
					{
						lock(_objects)
						{
							_objects.Add(obj);
							_spatialHash.Add(obj);
						}
					}
				}
				_objectAddList.Clear ();
			}
		}
	
		protected void UpdateObjects(long updateCount)
		{
			// Let all objects update themselves.
			lock(_objects)
			{
				_objects.Sort(delegate(GameObject a, GameObject b) { return a.UpdatePriority.CompareTo(b.UpdatePriority); } );
				foreach (GameObject obj in _objects) 
				{
					_spatialHash.Remove(obj);
					obj.Update (updateCount);
					_spatialHash.Add(obj);
				}
			}							
		}
		
		public bool Contains(GameObject gob)
		{
			return _objects.Contains(gob);
		}
		
		public IEnumerable<GameObject> FindObjects(Type filterType)
		{
			return _objects.Where(o => filterType.IsAssignableFrom(o.GetType()));
		}

		public IEnumerable<ObjectAndDistance> FindObjectsWithinRadius(GameObject src, float x, float y, float radius, Type filterType)
		{
			var nearby = _spatialHash.GetNearby(x,y,radius);
			return from n in nearby 
					let dist = Util.Distance(x,y,n.X,n.Y)
					where 	n != src && 
							filterType.IsAssignableFrom(n.GetType()) &&
							dist < radius
					select new ObjectAndDistance(n,dist);							
		}
		
		public ObjectAndDistance FindNearestObjectWithinRadius(GameObject src, float x, float y, float radius, Type filterType)
		{
			return FindObjectsWithinRadius(src,x,y,radius,filterType).OrderBy(o => o.Distance).FirstOrDefault();
		}
	
	}
}
