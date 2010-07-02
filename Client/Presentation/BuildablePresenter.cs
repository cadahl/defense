// 
// VehiclePresenter.cs
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
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Graphics;
	using Sim;
	using Util;
	using OpenTK;
	using OpenTK.Graphics;

	public class WallPresenter : ISpecificPresenter
	{
		private Renderer _renderer;
		private Background _bg;
		private bool _updateLayout;
		private bool[,] _walls;
		
		public WallPresenter(Renderer renderer)
		{
			_renderer = renderer;
			_bg = renderer.Backgrounds[1];
			_walls = new bool[_bg.Width,_bg.Height];
			Console.WriteLine("wall presenter+");
		}

		public void InstanceAdded(BuildableInstance instance)
		{
			_updateLayout = true;
		}

		public void InstanceRemoved(BuildableInstance instance)
		{
			int bx = (int)instance.Position.X/32;
			int by = (int)instance.Position.Y/32;
			_bg.ClearTile(bx, by);
			_walls[bx,by] = false;
			_updateLayout = true;
		}
		
		public void BeginPresent()
		{
		}

		public void EndPresent()
		{
			if(_updateLayout)
			{
				for(int by = 1; by < _bg.Height-1; ++by)
				{
					for(int bx = 1; bx < _bg.Width-1; ++bx)
					{
						int mask = 	_walls[bx+1,by] ? 1 : 0;
						mask |=	_walls[bx-1,by] ? 2 : 0;
						mask |=	_walls[bx,by-1] ? 4 : 0;
						mask |=	_walls[bx,by+1] ? 8 : 0;
						if(_walls[bx,by])
							_bg.SetTile (bx, by, mask, 416/32);
					}
				}
				
				_updateLayout = false;
			}
		}
		
		public void PresentOne(BuildableInstance instance)
		{
			if(_updateLayout)
			{
				int bx = (int)instance.Position.X/32;
				int by = (int)instance.Position.Y/32;
				_walls[bx,by] = true;
			}
		}
	}
	
	public class TowerPresenter : ISpecificPresenter
	{
		private Renderer _renderer;
		private Sprite _weaponSprite;
		private Sprite _flashSprite;

		public TowerPresenter(Renderer renderer, SpriteTemplate weaponTemplate, SpriteTemplate flashTemplate)
		{
			_renderer = renderer;
			_weaponSprite = new Sprite(weaponTemplate, 0, Priority.TowerWeapon);
			_flashSprite = new Sprite(flashTemplate, 0, Priority.TowerWeaponOverlay);
		}
		
		public void InstanceAdded(BuildableInstance ti)
		{
		//	_renderer.Backgrounds[1].SetTile((int)aop.Position.X / 32, (int)ti.Position.Y / 32, 5, 5);
		}
		
		public void InstanceRemoved(BuildableInstance ti)
		{
		}
		
		public void BeginPresent()
		{
			_weaponSprite.Clear();
			_flashSprite.Clear();
		}

		public void EndPresent()
		{
			_renderer.AddDrawable(_weaponSprite);
			_renderer.AddDrawable(_flashSprite);
		}
		
		public void PresentOne(BuildableInstance ti)
		{
			_renderer.Backgrounds[1].SetTile((int)ti.Position.X / 32, (int)ti.Position.Y / 32, 5, 5);
			
			var weapon = _weaponSprite.Add();

			if((ti.Flags & Sim.BuildableStatePacket.FireFlag) != 0)
			{
				ti.FlashTimer.Reset();
			}
			
			weapon.X = ti.Position.X;
			weapon.Y = ti.Position.Y;
			weapon.Angle = (ushort)ti.Angle;
			
			if(ti.FlashTimer.Progress > 0.0f)
			{
				var flash = _flashSprite.Add();
				flash.X = weapon.X;
				flash.Y = weapon.Y;
				flash.Angle = weapon.Angle;

				float amount = (float)Math.Pow(ti.FlashTimer.Progress, 2);
	   			flash.Color = new Color4(amount,amount,amount,0.0f);
			}
			
			ti.FlashTimer.Tick();
		}
	}

	public class BuildableInstance : Sim.BuildableStatePacket
	{
		public BuildableInstance(ISpecificPresenter presenter)
		{
			Presenter = presenter;
			FlashTimer = new Timer(TimerMode.CountDown, 11);
		}
		
		public ISpecificPresenter Presenter { get; private set; }
		public Timer FlashTimer { get; private set; }
	}

	public interface ISpecificPresenter
	{
		void InstanceAdded(BuildableInstance instance);
		void InstanceRemoved(BuildableInstance instance);
		void BeginPresent();
		void EndPresent();
		void PresentOne(BuildableInstance instance);
	}
	
	public class BuildablePresenter : IPresenter
	{
		public ICollection<string> ConfiguredTypeIds { get { return _configuredTypeIds; } }

		private Renderer _renderer;
		private Config _config;
		private Dictionary<string, ISpecificPresenter> _presenters = new Dictionary<string, ISpecificPresenter>();
		private HashSet<string> _configuredTypeIds;
		private Dictionary<int, BuildableInstance> _instances = new Dictionary<int, BuildableInstance>();
		
		public BuildablePresenter (Renderer renderer, Config config)
		{
			_renderer = renderer;
			_config = config;
			_configuredTypeIds = new HashSet<string>(_config.GetBuildableTypeIds());
		}
		
		public void OnAddObjectPacket(Sim.ObjectStatePacket added)
		{
			Console.WriteLine("ooa Buildable");
			ISpecificPresenter stp;
			if(!_presenters.TryGetValue(added.TypeId, out stp))
			{
				var u = _config.GetBuildableUpgrade(added.TypeId,0);

				if(added.TypeId == "wall")
				{
					stp = new WallPresenter(_renderer);
					_presenters.Add(added.TypeId, stp);
				}
				else
				{
					SpriteTemplate weaponTemplate, flashTemplate;
					
					if(u.SpriteTemplates.TryGetValue("weapon", out weaponTemplate) && 
					   u.SpriteTemplates.TryGetValue("flash",out flashTemplate))
					{
						stp = new TowerPresenter(_renderer, weaponTemplate, flashTemplate);
						_presenters.Add(added.TypeId, stp);
					}
				}
			}

			var instance = new BuildableInstance(stp);
			stp.InstanceAdded(instance);
			_instances.Add(added.Uid, instance);
		}
		
		public void OnRemoveObjectPacket(Sim.ObjectStatePacket removed)
		{
			BuildableInstance instance;
			if(_instances.TryGetValue(removed.Uid, out instance))
			{
				instance.Presenter.InstanceRemoved(instance);
				_instances.Remove(removed.Uid);
			}
		}
		
		public void Present(IEnumerable<ObjectStatePair> ous, float k)
		{
			foreach(var p in _presenters)
			{
				p.Value.BeginPresent();
			}
			
			foreach(var ou in ous)
			{
				if(ou.Current == null || string.IsNullOrEmpty(ou.Current.TypeId))
					continue;
			
				BuildableInstance ti;
				if(_instances.TryGetValue(ou.Current.Uid, out ti))
				{
					var cur = (Sim.BuildableStatePacket)ou.Current;
					var prev = (Sim.BuildableStatePacket)ou.Previous;
					
					ti.Flags = cur.Flags;
					ti.Position = Vector2.Lerp(prev.Position, cur.Position, k);
					ti.Angle =  Util.LerpAngle(prev.Angle, cur.Angle, k);
					
					ISpecificPresenter specp;
					if(_presenters.TryGetValue(cur.TypeId, out specp))
					{	
						specp.PresentOne(ti);
					}
				}					
			}
			
			foreach(var p in _presenters)
			{
				p.Value.EndPresent();
			}
		}
		
	}
}

