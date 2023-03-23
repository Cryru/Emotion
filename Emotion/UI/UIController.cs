#region Using

using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI
{
	public class UIController : UIBaseWindow
	{
		/// <summary>
		/// The priority for key events of this controller.
		/// </summary>
		public KeyListenerType KeyPriority = KeyListenerType.UI;

		/// <summary>
		/// List of all controllers running. Non disposed controllers will still appear here.
		/// It is maintained in order for controllers to not steal events from each other.
		/// </summary>
		protected static List<UIController> _activeControllers = new List<UIController>();

		/// <summary>
		/// The window that will receive keyboard key events.
		/// </summary>
		public UIBaseWindow? InputFocus { get; protected set; }

		/// <summary>
		/// Override for InputFocus due to internal logic such as focused text inputs.
		/// </summary>
		protected UIBaseWindow? _inputFocusManual;

		/// <summary>
		/// This is usually the first non-input transparent visible window but it can vary depending
		/// due to specific window logic.
		/// </summary>
		public UIBaseWindow? MouseFocus { get; protected set; }

		/// <summary>
		/// The currently open dropdown.
		/// </summary>
		public UIDropDown? DropDown { get; set; }

		protected bool[] _mouseFocusKeysHeld = new bool[Key.MouseKeyEnd - Key.MouseKeyStart];

		protected bool _updatePreload = true;
		protected bool _updateLayout = true;
		protected bool _updateInputFocus = true;

		public UIController()
		{
			InputTransparent = false;
			Debugger = new UIDebugger();
			Engine.Host.OnResize += Host_OnResize;
			KeepTemplatePreloaded(this);
			_activeControllers.Add(this);
		}

		public virtual void Dispose()
		{
			_activeControllers.Remove(this);
			StopPreloadTemplate(this);
			Engine.Host.OnResize -= Host_OnResize;
			if (InputFocus != null) Engine.Host.OnKey.RemoveListener(KeyboardFocusOnKey);
			if (MouseFocus != null) Engine.Host.OnKey.RemoveListener(MouseFocusOnKey);
		}

		private void Host_OnResize(Vector2 obj)
		{
			InvalidateLayout();
			InvalidatePreload();
		}

		public override void InvalidateLayout()
		{
			_updateLayout = true;
			_updateInputFocus = true;
		}

		public void InvalidatePreload()
		{
			_updatePreload = true;
		}

		protected override void AfterRenderChildren(RenderComposer c)
		{
#if false
            {
                if(MouseFocus != null) c.RenderOutline(MouseFocus.Bounds, Color.Red);
                c.RenderSprite(new Rectangle(Engine.Host.MousePosition.X, Engine.Host.MousePosition.Y, 1, 1), Color.Pink);
            }
#endif

			base.AfterRenderChildren(c);
		}

		protected override bool UpdateInternal()
		{
			if (!_loadingThread.IsCompleted) return false;
			if (_updatePreload) UpdateLoading();
			if (_updateLayout) UpdateLayout();
			if (_updateInputFocus) UpdateInputFocus();
			UpdateMouseFocus();
			return true;
		}

		protected void UpdateLayout()
		{
			_updateLayout = false;

			Debugger?.RecordNewPass(this);
			// 1. Measure the size of all windows.
			// Children are measured before parents in order for stretching to work.
			// Children are measured in index order. Layout rules are applied.
			Size = Engine.Renderer.DrawBuffer.Size;
			Measure(Size);

			// 2. Layout windows within their parents, starting with the controller taking up the full screen.
			// Sizes returned during measuring are used. Parents are positioned before children since
			// positions are absolute and not relative.
			Vector2 pos = CalculateContentPos(Vector2.Zero, Engine.Renderer.DrawBuffer.Size, Rectangle.Empty);
			Layout(pos);
		}

		public override void AddChild(UIBaseWindow? child)
		{
			if (child == null) return;
			base.AddChild(child);
			child.AttachedToController(this);
		}

		public override void RemoveChild(UIBaseWindow? child, bool evict = true)
		{
			if (child == null) return;
			base.RemoveChild(child, evict);
			child.DetachedFromController(this);
			InvalidateInputFocus();
		}

		#region Loading

		public Task PreloadUI()
		{
			if (!_loadingThread.IsCompleted) return _loadingThread;
			UpdateLoading();
			return _loadingThread;
		}

		private Task _loadingThread = Task.CompletedTask;
		private UILoadingContext _loadingContext = new UILoadingContext();

		protected void UpdateLoading()
		{
			if (!_loadingThread.IsCompleted) return;
			CheckLoadContent(_loadingContext);
			_loadingThread = Task.Run(_loadingContext.LoadWindows);
			_updatePreload = false;

			// If one controller is loading, check global.
			GlobalLoadUI();
		}

		/// <summary>
		/// You can keep windows globally loaded.
		/// </summary>
		private class PreloadWindowStorage : UIBaseWindow
		{
			public override void AddChild(UIBaseWindow child)
			{
				Children ??= new List<UIBaseWindow>();
				Children.Add(child);
			}

			public override void RemoveChild(UIBaseWindow child, bool evict = true)
			{
				if (Children == null) return;
				if (evict) Children.Remove(child);
			}
		}

		private static object _globalLoadingLock = new object();
		private static Task _globalLoadingThread = Task.CompletedTask;
		private static PreloadWindowStorage _keepWindowsLoaded = new();
		private static UILoadingContext _globalLoadingContext = new UILoadingContext();

		public static Task GlobalLoadUI()
		{
			lock (_globalLoadingLock)
			{
				if (!_globalLoadingThread.IsCompleted) return _globalLoadingThread;
				_keepWindowsLoaded.CheckLoadContent(_globalLoadingContext);
				_globalLoadingThread = Task.Run(_globalLoadingContext.LoadWindows);
			}

			return _globalLoadingThread;
		}

		public static void KeepTemplatePreloaded(UIBaseWindow window)
		{
			_keepWindowsLoaded.AddChild(window);
		}

		public static void StopPreloadTemplate(UIBaseWindow window)
		{
			_keepWindowsLoaded.RemoveChild(window);
		}

		#endregion

		#region Input

		private bool KeyboardFocusOnKey(Key key, KeyStatus status)
		{
			// It is possible to receive an input even while a recalculating is pending.
			if (_updateInputFocus && status == KeyStatus.Down)
			{
				UpdateInputFocus();
				UpdateMouseFocus();
			}

			if (!Visible) return true;
			if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) return true;
			if (InputFocus != null && InputFocus.VisibleAlongTree())
				return InputFocus.OnKey(key, status, Engine.Host.MousePosition);

			return true;
		}

		private bool MouseFocusOnKey(Key key, KeyStatus status)
		{
			if (_updateInputFocus && status == KeyStatus.Down)
			{
				UpdateInputFocus();
				UpdateMouseFocus();
			}

			if (!Visible) return true;

			if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && MouseFocus != null)
			{
				_mouseFocusKeysHeld[key - Key.MouseKeyStart] = status == KeyStatus.Down;

				if (_inputFocusManual != null && !MouseFocus.IsWithin(_inputFocusManual) && status == KeyStatus.Down)
				{
					bool isDropDown = _inputFocusManual is UIDropDown;
					SetInputFocus(null);
					if (isDropDown) return false;
				}

				return MouseFocus.OnKey(key, status, Engine.Host.MousePosition);
			}

			return true;
		}

		public void InvalidateInputFocus()
		{
			_updateInputFocus = true;
		}

		public void SetInputFocus(UIBaseWindow? win, bool searchTree = false)
		{
			UIBaseWindow? focusable = searchTree && win != null ? FindInputFocusable(win) : win;
			_inputFocusManual = focusable;

			UpdateInputFocus();
		}

		private void UpdateInputFocus()
		{
			UIBaseWindow? newFocus;
			if (InputTransparent || !Visible)
			{
				newFocus = null;
			}
			else if (_inputFocusManual != null && _inputFocusManual.Visible && !_inputFocusManual.InputTransparent && _inputFocusManual.Controller == this)
			{
				newFocus = _inputFocusManual;
			}
			else
			{
				_inputFocusManual = null;
				newFocus = FindInputFocusable(this);
			}

			if (newFocus == this) newFocus = null;

			if (InputFocus != newFocus)
			{
				// Re-hook event to get up events on down presses.
				if (InputFocus != null)
				{
					Engine.Host.OnKey.RemoveListener(KeyboardFocusOnKey);
					InputFocus.InputFocusChanged(false);
				}

				InputFocus = newFocus;

				if (InputFocus != null)
				{
					Engine.Host.OnKey.AddListener(KeyboardFocusOnKey, KeyPriority);
					InputFocus.InputFocusChanged(true);
				}

				// Kinda spammy.
				// Engine.Log.Info($"New input focus {InputFocus}", "UI");
			}

			_updateInputFocus = false;
		}

		private void UpdateMouseFocus()
		{
			Vector2 mousePos = Engine.Host.MousePosition;
			UIBaseWindow? newMouseFocus = null;

			// If currently holding down a mouse button don't change the mouse focus if it is still valid.
			if (MouseFocus != null && MouseFocus.Visible && !MouseFocus.InputTransparent)
				for (var i = 0; i < _mouseFocusKeysHeld.Length; i++)
				{
					if (_mouseFocusKeysHeld[i])
					{
						newMouseFocus = MouseFocus;
						break;
					}
				}

			if (newMouseFocus == null)
			{
				newMouseFocus = Engine.Host.HostPaused || InputTransparent ? null : FindMouseInput(mousePos);

				// If the controller is focused that means there is no focus.
				// But if there is a dropdown open we want to capture the dismiss click so we keep it as a focus.
				if (newMouseFocus == this && _inputFocusManual == null) newMouseFocus = null;
			}

			if (newMouseFocus != MouseFocus)
			{
				MouseFocus?.OnMouseLeft(mousePos);
				if (MouseFocus != null) Engine.Host.OnKey.RemoveListener(MouseFocusOnKey);
				MouseFocus = newMouseFocus;
				if (MouseFocus != null) Engine.Host.OnKey.AddListener(MouseFocusOnKey, KeyPriority);
				MouseFocus?.OnMouseEnter(mousePos);

				// This is very spammy.
				//Engine.Log.Info($"New mouse input focus {newMouseFocus}", "UI");
			}
			else
			{
				MouseFocus?.OnMouseMove(mousePos);
			}
		}

		protected static UIBaseWindow FindInputFocusable(UIBaseWindow wnd)
		{
			if (wnd.Children == null) return wnd;
			for (int i = wnd.Children.Count - 1; i >= 0; i--)
			{
				UIBaseWindow win = wnd.Children[i];
				if (!win.InputTransparent && win.Visible) return FindInputFocusable(win);
			}

			return wnd;
		}

		#endregion

		/// <summary>
		/// Get a list if window id and bounds. Used for debugging.
		/// </summary>
		public (string, Rectangle)[] GetLayoutDescription()
		{
			var layout = new List<(string, Rectangle)>
			{
				(Id, new Rectangle(Position, Size))
			};

			foreach (UIBaseWindow child in this)
			{
				layout.Add((child.Id, new Rectangle(child.Position, child.Size)));
			}

			return layout.ToArray();
		}
	}
}