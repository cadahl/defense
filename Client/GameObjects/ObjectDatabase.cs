namespace Client.GameObjects
{
	using System;
	using System.Collections.Generic;
	using Util;
	
	public struct ObjectAndDistance<T> where T : GameObject
	{
		public ObjectAndDistance(T obj, float distance)
		{
			Distance = distance;
			Object = obj;
		}
		
		public static implicit operator T(ObjectAndDistance<T> a)
		{
			return a.Object;
		}
		
		public float Distance;
		public T Object;
	}
	
	public class ObjectDatabase
	{		
		protected List<GameObject> _objects = new List<GameObject> ();
		private List<GameObject> _objectAddList = new List<GameObject> ();
		private List<GameObject> _objectRemoveList = new List<GameObject> ();
		
		public ObjectDatabase ()
		{
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
							obj.Dispose();
							_objects.Remove(obj);
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
					obj.Update (updateCount);
				}
			}							
		}
		
		public bool DoesObjectExist(GameObject gob)
		{
			return _objects.Contains(gob);
		}
		
		public ICollection<GameObject> FindObjects(Type filterType)
		{
			List<GameObject> list = new List<GameObject>();

			foreach(GameObject gob in _objects)
			{
				if(gob.GetType().IsAssignableFrom(filterType))
				{
					list.Add(gob);
				}
			}			
			
			return list;
		}

		public List<ObjectAndDistance<GameObject>> FindObjectsWithinRadius(GameObject gob, float x, float y, float radius, Type filterType)
		{
			List<ObjectAndDistance<GameObject>> list = new List<ObjectAndDistance<GameObject>>();
			
			foreach(GameObject gob2 in _objects)
			{
				if(filterType.IsAssignableFrom(gob2.GetType()) && gob != gob2)
				{
					float dist = Util.Distance(x,y,gob2.X,gob2.Y);

					if(dist < radius)
					{
						list.Add(new ObjectAndDistance<GameObject>(gob2,dist));
					}
				}
			}
			
			return list;
		}
	
	}
}
