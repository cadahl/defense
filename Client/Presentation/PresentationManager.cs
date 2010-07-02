// 
// PresentationManager.cs
//  
// Author:
//       carl <>
// 
// Copyright (c) 2010 carl
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Client.Presentation
{
	using System;
	using System.Drawing;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using OpenTK;
	using Graphics;
	using Util;
	
	public class ObjectStatePair
	{
		public Sim.ObjectStatePacket Current, Previous;
	}
	
	public interface IPresenter
	{
		ICollection<string> ConfiguredTypeIds { get; }

	
		void Present(IEnumerable<ObjectStatePair> ous, float k);
	}
	
	public partial class PresentationManager
	{
//		private Queue<Sim.AddObjectPacket> _addedObjects = new Queue<Sim.AddObjectPacket>();
//		private Queue<int> _removedObjects = new Queue<int>();
		private Dictionary<int,IPresenter> _presenterForUid = new Dictionary<int, IPresenter>();
		private List<IPresenter> _presenters = new List<IPresenter>();
		private Config _config;
		private Sim.StateUpdate _previousUpdate, _currentUpdate;
		private object _updateLock = new object();
		
		public PresentationManager (Renderer r, Sim.ObjectDatabase sim)
		{
			_config = new Config();
			_presenters.Add(new BuildablePresenter(r, _config));
			_presenters.Add(new VehiclePresenter(r, _config));
			
	/*		sim.ObjectAdded += oa => { lock(_addedObjects) { _addedObjects.Enqueue(oa); } };
			sim.ObjectRemoved += or => { lock(_removedObjects) { _removedObjects.Enqueue(or); } };
	*/		
			sim.Updated += update => 
			{
				lock(_updateLock)
				{
					_previousUpdate = _currentUpdate;
					_currentUpdate = update;
				}
			};
		}

		public void Present()
		{
		/*	lock(_removedObjects)
			{
				while(_removedObjects.Count > 0)
				{
					var uid = _removedObjects.Dequeue();
					
					IPresenter presenter;
					if(_presenterForUid.TryGetValue(uid, out presenter))
					{
						presenter.OnRemoveObjectPacket(uid);
						_presenterForUid.Remove(uid);
					}
				}
			}

			lock(_addedObjects)
			{
				while(_addedObjects.Count > 0)
				{
					var added = _addedObjects.Dequeue();
					
					var p = _presenters.SingleOrDefault(pr => pr.ConfiguredTypeIds.Contains(added.TypeId));
					
					if(p != null)
					{
						p.OnAddObjectPacket(added);
						_presenterForUid.Add(added.Uid, p);
					}
					else
					{
						Console.Error.WriteLine("No presenter found compatible with type id '" + added.TypeId + "'");
					}
				}
			}
			 */
			Sim.StateUpdate cu, pu;
			lock(_updateLock)
			{
				cu = _currentUpdate;
				pu = _previousUpdate;
			}

			//if(pu != null || cu != null)
			{
				float k = 0.0f;
				
				if(pu != null || cu != null)
					k = Util.Saturate((float)(((double)DateTime.Now.Ticks - pu.Timestamp)/(cu.Timestamp - pu.Timestamp)));

				Dictionary<int,ObjectStatePair> pairs = new Dictionary<int, ObjectStatePair>();

				foreach(var c in cu.UpdatedObjects)
				{
					ObjectStatePair osp;
					if(pairs.TryGetValue(c.Key, out osp))
					{
						osp.Current = c.Value;
					}
					else
						pairs.Add(c.Key, new ObjectStatePair { Current = c.Value });
				}

				foreach(var p in pu.UpdatedObjects)
				{
					ObjectStatePair osp;
					if(pairs.TryGetValue(p.Key, out osp))
					{
						osp.Previous = p.Value;
					}
					else
						pairs.Add(p.Key, new ObjectStatePair { Previous = p.Value });
				}

				foreach(var p in _presenters)
				{					
					p.Present(pairs.Values.Where( x => p.ConfiguredTypeIds.Contains(x.Current.TypeId)), k);
				}
			}
		}
		
		static IEnumerator<ObjectStatePair> GetUpdateIterator(IDictionary<int,Sim.ObjectStatePacket> pu, IDictionary<int,Sim.ObjectStatePacket> cu)
		{
			var puiter = pu.GetEnumerator();
			var cuiter = cu.GetEnumerator();
			
			while(cuiter.MoveNext() &&
			       puiter.MoveNext())
			{
				if(cuiter.Current.Key == puiter.Current.Key)
				{
					yield return new ObjectStatePair() { Current = cuiter.Current.Value, Previous = puiter.Current.Value };
				}

				if (cuiter.Current.Key < puiter.Current.Key)
				{
					do
					{
						yield return new ObjectStatePair() { Current = cuiter.Current.Value, Previous = null };
					} while(cuiter.MoveNext());
				}
				if (cuiter.Current.Key > puiter.Current.Key)
				{
					do
					{
						yield return new ObjectStatePair() { Current = null, Previous = puiter.Current.Value };
					} while(puiter.MoveNext());
				}
			}
		}
	}
}
