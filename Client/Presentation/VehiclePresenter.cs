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
	
	public class SpecificVehiclePresenter
	{
		private Renderer _renderer;
		private Sprite _sprite;

		public SpecificVehiclePresenter(Renderer renderer, SpriteTemplate st)
		{
			_renderer = renderer;
			_sprite = new Sprite(st, Drawable.Flags.Colorize, Priority.Vehicle);
			_sprite.Clear();
			Console.WriteLine("new svp...");
		}
		
		public void BeginPresent()
		{
			_sprite.Clear();
		}

		public void EndPresent()
		{
			_renderer.AddDrawable(_sprite);
		}
		
		public void PresentOne(VehicleInstance vi)
		{
//			if(_game.ShowVehicleProbes)
//				r.AddDrawable(_targetLine);
			
			var spr = _sprite.Add();

			if((vi.Flags & Sim.VehicleStatePacket.HurtFlag) != 0)
			{
				vi.HurtTimer.Reset();
			}
			
			spr.X = vi.Position.X;
			spr.Y = vi.Position.Y;
			spr.Angle = (ushort)((vi.Angle-1024) & 4095);
		
			if(vi.HurtTimer.Progress > 0.0f)
			{
				float flash = (float)Math.Pow(1.0f-vi.HurtTimer.Progress, 2.0f);
				float amount = (float)Math.Pow((1.0f-((float)vi.HitPoints)/vi.MaxHitPoints), 3.0f);
				
				amount = Util.Lerp(0.0f, 1.5f, amount);
			    spr.Color = new Color4(1.0f * amount, flash * amount, flash * amount, 1.0f);
			}
			else
			{
				spr.Color = new Color4(0f,0f,0f,1f);
			}
		
			vi.HurtTimer.Tick();
		}
	}

	public class VehicleInstance : Sim.VehicleStatePacket
	{
		public SpecificVehiclePresenter Presenter;
		public Timer HurtTimer;
	}
	
	public class VehiclePresenter : IPresenter
	{
		public ICollection<string> ConfiguredTypeIds { get { return _configuredTypeIds; } }

		private Renderer _renderer;
		private Config _config;
		private Dictionary<string, SpecificVehiclePresenter> _presenters = new Dictionary<string, SpecificVehiclePresenter>();
		private HashSet<string> _configuredTypeIds;
		private Sprite _healthBars;
		private Dictionary<int, VehicleInstance> _instances = new Dictionary<int, VehicleInstance>();
		
		private static SpriteTemplate _healthBarTmp = new SpriteTemplate 
		{ 
			TilemapName = "units", 
			Rectangle = new Rectangle(944, 0, 32, 5), 
			Offset = new Point(16, 3), 
			FrameOffset = 7,
			VerticalAnimation = true
		};		
		
		public VehiclePresenter (Renderer renderer, Config config)
		{
			_renderer = renderer;
			_config = config;
			_configuredTypeIds = new HashSet<string>(_config.GetVehicleTypeIds());
			_healthBars = new Sprite(_healthBarTmp, 0, Priority.HealthBar);
			
			foreach(var typeId in _configuredTypeIds)
			{
				_presenters.Add(typeId, new SpecificVehiclePresenter(_renderer, _config.GetVehicleSpriteTemplate(typeId)));
			}
		}
		
		
		public void Present(IEnumerable<ObjectStatePair> ous, float k)
		{
			foreach(var p in _presenters)
			{
				p.Value.BeginPresent();
			}
			
			_healthBars.Clear();
			
			foreach(var osp in ous)
			{
				var cur = (Sim.VehicleStatePacket)osp.Current;
				var prev = (Sim.VehicleStatePacket)osp.Previous;
						
				if(cur == null)
				{
					_instances.Remove(prev.Uid);
					continue;
				}

				
				VehicleInstance vi;
				if(!_instances.TryGetValue(cur.Uid, out vi))
				{
					vi = new VehicleInstance()
			               	{
								Presenter = _presenters[cur.TypeId],
								HurtTimer = new Timer(TimerMode.CountDown, 5)
							};
					_instances.Add(cur.Uid, vi);
				}
				
				if(prev == null)
				{
					vi.Position = cur.Position;
					vi.Angle = cur.Angle;
					vi.HitPoints = cur.HitPoints;
					vi.MaxHitPoints = cur.MaxHitPoints;
				}
				else
				{
					vi.Position = Vector2.Lerp(prev.Position, cur.Position, k);
					vi.Angle =  Util.LerpAngle(prev.Angle, cur.Angle, k);
					vi.HitPoints = Util.Lerp(prev.HitPoints, cur.HitPoints, k);
					vi.MaxHitPoints = Util.Lerp(prev.MaxHitPoints, cur.MaxHitPoints, k);
				}

				vi.Presenter.PresentOne(vi);

				int hpFrame = (int)(29.0f * ((float)vi.HitPoints)/vi.MaxHitPoints);
				_healthBars.Add(vi.Position.X, vi.Position.Y-20, hpFrame);			
			}
			
			_renderer.AddDrawable(_healthBars);
			
			foreach(var p in _presenters)
			{
				p.Value.EndPresent();
			}
		}
		
	}
}

