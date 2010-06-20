namespace Client
{

	using System.Drawing;
	using System.Collections.Generic;

	public enum Key
	{
		CameraUp, CameraDown, CameraLeft, CameraRight, CameraZoomIn, CameraZoomOut
	}

	public class Input
	{
		public int MouseX, MouseY;
		private Dictionary<OpenTK.Input.Key, Key> _bindings = new Dictionary<OpenTK.Input.Key, Key>();
		private List<Key> _activeKeys = new List<Key> ();

		public delegate void KeyEventHandler(OpenTK.Input.Key key);
		public delegate void MouseEventHandler(int x, int y, OpenTK.Input.MouseButton button);
		public delegate void MouseWheelEventHandler(int delta);
		
		public event KeyEventHandler KeyDown;
		public event KeyEventHandler KeyUp;
		public event MouseEventHandler MouseDown;
		public event MouseEventHandler MouseUp;
		public event MouseWheelEventHandler MouseWheelChanged;
		
		public Input (OpenTK.GameWindow window)
		{
			window.Keyboard.KeyDown += HandleKeyDown;			
			window.Keyboard.KeyUp += HandleKeyUp;
			window.Mouse.ButtonDown += HandleMouseDown;
			window.Mouse.ButtonUp += HandleMouseUp;
			window.Mouse.Move += HandleMouseMove;
			window.Mouse.WheelChanged += HandleMouseWheelChanged;

			_bindings.Add(OpenTK.Input.Key.W, Key.CameraUp);
			_bindings.Add(OpenTK.Input.Key.S, Key.CameraDown);
			_bindings.Add(OpenTK.Input.Key.A, Key.CameraLeft);
			_bindings.Add(OpenTK.Input.Key.D,Key.CameraRight);
			_bindings.Add(OpenTK.Input.Key.KeypadPlus, Key.CameraZoomIn);
			_bindings.Add(OpenTK.Input.Key.KeypadMinus, Key.CameraZoomOut);
		}

		private void HandleMouseWheelChanged (object sender, OpenTK.Input.MouseWheelEventArgs e)
		{
			if(MouseWheelChanged != null)
				MouseWheelChanged(e.Delta);
		}

		public bool isDown (Key key)
		{
			return _activeKeys.Contains (key);
		}

		private void HandleKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			Key ourKey;
			if(_bindings.TryGetValue(e.Key, out ourKey))
			{
				if(!_activeKeys.Contains(ourKey))
					_activeKeys.Add(ourKey);
			}

			if(KeyDown != null)
				KeyDown(e.Key);
		}

		private void HandleKeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			Key ourKey;
			if(_bindings.TryGetValue(e.Key, out ourKey))
			{
				if(_activeKeys.Contains(ourKey))
					_activeKeys.Remove(ourKey);
			}

			if(KeyUp != null)
				KeyUp(e.Key);
		}

		private void HandleMouseDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
		{
			if(MouseDown != null)
				MouseDown(MouseX,MouseY,e.Button);
		}

		private void HandleMouseUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
		{
			if(MouseUp != null)
				MouseUp(MouseX,MouseY,e.Button);
		}

		private void HandleMouseMove(object sender, OpenTK.Input.MouseMoveEventArgs e)
		{
			MouseX = e.X;
			MouseY = e.Y;
		}
	}
}
