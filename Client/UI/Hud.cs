namespace Client.UI
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using Client.GameObjects;
	using Client.Graphics;
	using OpenTK;
	using OpenTK.Graphics;
	using OpenTK.Graphics.OpenGL;
	using Util;
	
	public class Hud
	{
		private const int _basePriority = 100;
		// Draw priority för alla HUD-element
		private int _renderCount, _updateCount;
		private long _rframeTime;
		private double _fps;
		private double[] _rdeltaMsFilter = new double[50];
		private double _lastUpdateTime;
		private double _ups;
		private double[] _updateTimeFilter = new double[10];
		
		public static SpriteTemplate Panel = new SpriteTemplate ("units", 64, 320, 16, 16, 0, 0, 8, 0);
		private SpriteTemplate _buildCursorTmp = new SpriteTemplate("units", 128, 192-32, 32, 32, 0);
		private SpriteTemplate _selectionHighlightTmp = new SpriteTemplate("units", 96, 80, 64, 64, 0);
		private SpriteTemplate _upgradeTmp = new SpriteTemplate("units", 32, 168, 24, 24, 0,0,0);
		private SpriteTemplate _sellTmp = new SpriteTemplate("units", 80, 168, 24, 24, 0,0,0);
		private Sprite[] _cursors = new Sprite[2];
		private Sprite _selectionHighlight;
		private Widget _upgradeButton, _sellButton;
		private Circle _pointerCircle, _selectedCircle;
		private HudBuildablePullout _bpullout;

		private HudToolbar _toolbar;

//		private string _message;
		//private long _messageTime;
		
		public SpriteTemplate Cursor;
		
		private TextLine _nextWaveLine, _msgLine, _gameLine,_rendererLine;
		private bool _showNextWaveLine;

		private Game _game;

		private  Color4 SellColor = new Color4(255,11,11,0);
		private  Color4 UpgradeColor = new Color4(246,233,(int)(185/1.5f),0);
		private Color4 SelectColor = new Color4(0.8f ,0.8f, 0.8f, 0.0f);
		private Color4 _buildCursorColor = new Color4(0.1f, 0.1f, 0.1f, 1.0f);

		private Collider _mousePointerCollider;
		private GameObject _hoverObject, _selectedObject;

		private bool _showUpgrade;
		private bool _showSell;

		public Hud (Game game)
		{
			_game = game;
			Renderer r = game.Application.Renderer;
//			_msgLine = new TextLine(r, r.Width-200, r.Height-PanelHeight/2-10, _basePriority);
			_gameLine = new TextLine(r, 8, 8, _basePriority);
			_rendererLine = new TextLine(r, 8, 28, _basePriority);
			_nextWaveLine = new TextLine(r, r.Width/2, r.Height/2-11, _basePriority+100);
			_showNextWaveLine = false;

			_toolbar = new HudToolbar(_game, r, _basePriority);

			_cursors[0] = new Sprite(_buildCursorTmp, Drawable.Flags.Colorize, _basePriority+40);
			_cursors[1] = new Sprite(null, Drawable.Flags.Colorize, _basePriority+41);
			_cursors[1][0].Frame = (byte)0;

			_selectionHighlight = new Sprite(_selectionHighlightTmp, 0, _basePriority);

			_upgradeButton = new Widget(_upgradeTmp, _game, _basePriority+1);
			_upgradeButton.SetFlags(WidgetFlags.LevelCoordinates);
			_sellButton = new Widget(_sellTmp, _game, _basePriority+1);
			_sellButton.SetFlags(WidgetFlags.LevelCoordinates);

			_upgradeButton[0].Color = new Color4(1.0f, 1.0f, 1.0f, 0.65f);
			_upgradeButton[1].Color = new Color4(1.0f, 1.0f, 1.0f, 0.0f);
			_sellButton[0].Color = new Color4(1.0f, 1.0f, 1.0f, 0.65f);
			_sellButton[1].Color = new Color4(1.0f, 1.0f, 1.0f, 0.0f);

			_pointerCircle = new Circle(0, 0, 0, 300);
			_pointerCircle.Color.A = 0.4f;
			_selectedCircle = new Circle(0,0,0, _basePriority-2);
			_selectedCircle.Color.A = 0.4f;

			_bpullout = new HudBuildablePullout(_game, r,_basePriority);

			_mousePointerCollider = Collider.Point(	0,
			                                       	0,
			                                       	delegate(Collider c, List<ObjectAndDistance<GameObject>> objects)
			                                       	{
														_hoverObject = objects.FirstOrDefault();

													},
													null );
			_mousePointerCollider.FilterType = typeof(Buildable);



			game.Application.Input.MouseDown += HandleMouseDown;
			game.Application.Input.KeyDown += HandleKeyDown;
        }

		public void WriteMessage(string s)
		{
		//	_msgLine.Text = s;
		//	_messageTime = _updateCount;
		}

		private void HandleMouseDown(int mouseX, int mouseY, OpenTK.Input.MouseButton button)
		{
			if(mouseY < _game.Application.Renderer.Height-HudToolbar.Height)
			{
				if(button == OpenTK.Input.MouseButton.Left)
				{
					if(_selectedObject != null)
					{
						if(_upgradeButton.IsMouseOver)
						{
							_game.Upgrade((Buildable)_selectedObject);
						}
						else if(_sellButton.IsMouseOver)
						{
							_game.Sell((Buildable)_selectedObject);
						}
					}
					else
					{
						if(_toolbar.SelectedIcon.CanBuy)
						{
							int bx = (int)_game.LevelMouseX/32;
							int by = (int)_game.LevelMouseY/32;
							
							if(!_game.BuildAt(bx, by, _toolbar.SelectedIcon.BuildType))
							{
								WriteMessage("Can't build there.");
							}
						}
						else
						{
							WriteMessage("Not enough cash!");
						}
					}
				}

				_selectedObject = null;
				var hoverObjs = _game.FindObjectsWithinRadius(null, _game.LevelMouseX, _game.LevelMouseY, 16.0f, typeof(Buildable));
				if(hoverObjs.Count > 0)
				{
					_selectedObject = hoverObjs[0].Object;
					HandleKeyDown(OpenTK.Input.Key.Number1);
				}
			}
			else
			{
				int iconIndex = (mouseX-32)/HudToolbar.IconSpacing;
				HandleKeyDown(OpenTK.Input.Key.Number1+iconIndex);
			}


		}
		
		private void HandleKeyDown(OpenTK.Input.Key key)
		{
			int iconi = (int)key-(int)OpenTK.Input.Key.Number1;
			
			if(iconi >= 0 && iconi < _toolbar.Icons.Length)
			{
				_toolbar.SelectedIconIndex = iconi;
			
				if(iconi > 0)
				{
					_selectedObject = null;
					WriteMessage("Click to build a " + _game.GetUpgradeInfo(_toolbar.SelectedIcon.BuildType,0).Caption + ".");
				}
				else
					WriteMessage("Click to select a unit."); 
			}
		}
		
		private Color4 PulseColor(Color4 c)
		{
			float s = (float)Math.Sin(_game.UpdateCount*Math.PI/15.0);
			float a = Util.Saturate((float)(0.75 + 0.25*s));
			c.R = a * c.R;
			c.G = a * c.G;
			c.B = a * c.B;
			return c;
		}

		private Color4 PulseColors(Color4 c, Color4 c2)
		{
			float a = Util.Saturate((float)(0.75 + 0.25*Math.Sin(_game.UpdateCount*Math.PI/20.0)));
			c.R = Util.Lerp(c.R, c2.R, a);
			c.G = Util.Lerp(c.G, c2.G, a);
			c.B = Util.Lerp(c.B, c2.B, a);
			return c;
		}

		private void RenderToolbar(Renderer r)
		{
			_toolbar.Render(r);

			_bpullout.X = _toolbar.SelectedIconIndex * HudToolbar.IconSpacing + HudToolbar.IconLeft - 28;
			_bpullout.Y = r.Height - HudToolbar.Height;
			_bpullout.Caption = null;

			if(_toolbar.SelectedIconIndex == 0)
			{
				if(_selectedObject != null)
				{
					var b = _selectedObject as Buildable;

					if(b != null)
					{
						if(_showSell)
						{
							_bpullout.Upgrade = b.CurrentUpgrade;
							_bpullout.Options = (BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab);
							_bpullout.CaptionIcon = 1;
							_bpullout.CaptionColor = SellColor;
							_bpullout.LineColor = Color4.White;
							_bpullout.Caption = "Sell";
						}
						else
						if(_showUpgrade && (b.CurrentUpgradeIndex+1) < b.Upgrades.Length)
						{
							_bpullout.Upgrade = b.Upgrades[b.CurrentUpgradeIndex+1];
							_bpullout.Options = BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab;
							_bpullout.CaptionIcon = 0;
							_bpullout.CaptionColor = UpgradeColor;
							_bpullout.LineColor = Color4.White;
							_bpullout.Caption = "Upgrade";
						}
						else
						{
							_bpullout.Upgrade = b.CurrentUpgrade;
							_bpullout.Options = (BuildablePulloutOptions.ShowAllRows|BuildablePulloutOptions.ShowIconTab) & ~BuildablePulloutOptions.ShowPrice;
							_bpullout.CaptionIcon = -1;
							_bpullout.CaptionColor = Color4.LightGray;
							_bpullout.LineColor = Color4.LightGray;

						}
					}
				}
				else
				{
					_bpullout.Upgrade = null;
					_bpullout.Options = _toolbar.SelectedIcon.PulloutOptions;
					_bpullout.CaptionIcon = -1;
					_bpullout.CaptionColor = Color4.White;
					_bpullout.LineColor = Color4.White;
				}

				_bpullout.TooExpensive = false;
			}
			else
			{
				_bpullout.Upgrade = _game.GetUpgradeInfo(_toolbar.SelectedIcon.BuildType,0);
				_bpullout.Options = _toolbar.SelectedIcon.PulloutOptions;
				_bpullout.CaptionIcon = -1;
				_bpullout.CaptionColor = Color4.White;
				_bpullout.LineColor = Color4.White;
				_bpullout.TooExpensive = !_toolbar.SelectedIcon.CanBuy;
			}

			_bpullout.Render(r);
		}

		private void UpdatePointer(Game game)
		{
			var r = game.Application.Renderer;
			int mx = game.Application.Input.MouseX;
			int my = game.Application.Input.MouseY;
			int mapx = ((int)game.LevelMouseX)/32;
			int mapy = ((int)game.LevelMouseY)/32;
			int cursorx = mapx * 32 + 16;
			int cursory = mapy * 32 + 16;
			_cursors[0][0].X = cursorx;
			_cursors[0][0].Y = cursory;
			_cursors[0][0].Flags &= ~SpriteFlags.Disable;

			{
				var b = _selectedObject as Buildable;

				if(b != null)
				{
					// Draw upgrade & sell buttons
					_upgradeButton.X = (int)b.X-32-12;
					_upgradeButton.Y = (int)b.Y-24;
					_sellButton.X = (int)b.X-32-12;
					_sellButton.Y = (int)b.Y;
					_upgradeButton.Update();
					_sellButton.Update();
					r.AddDrawable(_upgradeButton);
					r.AddDrawable(_sellButton);
					bool upgradeHover = _upgradeButton.IsMouseOver;
					bool sellHover = _sellButton.IsMouseOver;

					// Light up the selected unit
					_selectionHighlight[0].X = b.X;
					_selectionHighlight[0].Y = b.Y;
					_selectionHighlight[0].Color = sellHover ? PulseColor(SellColor) : upgradeHover ? UpgradeColor : SelectColor;
					_selectionHighlight[0].Color.R /= 1.6f;
					_selectionHighlight[0].Color.G /= 1.6f;
					_selectionHighlight[0].Color.B /= 1.6f;
					_selectionHighlight[0].Color.A = 0.0f;
					r.AddDrawable(_selectionHighlight);

					// Range circle around selected?
					bool showSelectedCircle = b.CurrentUpgrade.Range > 0;
					if(showSelectedCircle)
					{
						_selectedCircle.X = b.X;
						_selectedCircle.Y = b.Y;
						_selectedCircle.Radius = (float)b.CurrentUpgrade.Range;
						r.AddDrawable(_selectedCircle);
					}
				}
				else
					if(my < r.Height-HudToolbar.Height &&
					   !_game.HasBuilding(mapx,mapy) &&
					   _toolbar.SelectedIconIndex > 0)
					{
						_cursors[0][0].Frame = (byte)_toolbar.SelectedIcon.CursorFrame;
						var c = _toolbar.SelectedIcon.CanBuy ?  PulseColor(HudToolbar.SelectedBuildableIconColor) : PulseColor(new Color4(0.2f,0.0f,0.0f,0.5f));
						_cursors[0][0].Color = c;

						// Indicate firing radius by drawing a circle around the cursor.
						if(_toolbar.SelectedIcon.Range > 0)
						{
							_pointerCircle.X = cursorx;
							_pointerCircle.Y = cursory;
							_pointerCircle.Radius = (float)_toolbar.SelectedIcon.Range;
							r.AddDrawable(_pointerCircle);
						}

						// If the Buildable is a tower, it'll have a weapon which is a separate image. Draw it.
						if(_toolbar.SelectedIcon.WeaponIcon != null)
						{
							_cursors[1].Template = _toolbar.SelectedIcon.WeaponIcon.Template;
							_cursors[1][0].X = cursorx;
							_cursors[1][0].Y = cursory;
							_cursors[1][0].Color = c;
							r.AddDrawable(_cursors[1]);
						}

						r.AddDrawable(_cursors[0]);
					}
			}
		}

		public void ShowNextWaveWarning(string msg, int seconds)
		{
			_nextWaveLine.Text = msg + " Arriving in " + seconds + "...";
			_nextWaveLine.X = _game.Application.Renderer.Width/2-_nextWaveLine.Width/2;
			_showNextWaveLine = seconds >= 0;
		}

		public void Update()
		{
			_mousePointerCollider.X = _game.LevelMouseX;
			_mousePointerCollider.Y = _game.LevelMouseY;
			_game.AddCollider(_mousePointerCollider);
		}

		public void Render()
		{
			var client = _game.Application;
			var r = client.Renderer;

			if(_showNextWaveLine)
				r.AddDrawable(_nextWaveLine);

			RenderToolbar(r);			             
			
			UpdatePointer(_game);

			_gameLine.Text = string.Format("UPS: {1:0.00}, Objects: {0}", _game.ObjectCount, _ups);
			r.AddDrawable(_gameLine);
			
			_rendererLine.Text = string.Format("FPS: {2:0.00}, Drawables: {0}, Batches: {1}", r.DrawableCount, r.BatchCount, _fps); 
			r.AddDrawable(_rendererLine);
		}

		public void UpdateFps ()
		{
			long lastFrameTime = _rframeTime;
			_rframeTime = System.DateTime.Now.Ticks;
			double deltaMs = (_rframeTime - lastFrameTime) / 10000.0;
			_rdeltaMsFilter[(int) (_renderCount % _rdeltaMsFilter.Length)] = deltaMs;

			if ((_renderCount & 31) == 1) {
	    		_fps = 1000.0 / _rdeltaMsFilter.Average();
			}			 
			_renderCount++;
		}

		public void UpdateUps(double time)
		{
			time = System.DateTime.Now.Ticks;
			_updateTimeFilter[_updateCount % _updateTimeFilter.Length] = time - _lastUpdateTime;
			

			if ((_updateCount & 31) == 1) 
			{
	    		_ups = 1000.0/((time - _lastUpdateTime) / 10000.0);
			}			 
			_lastUpdateTime = time;
			_updateCount++;
		}

	}
}
