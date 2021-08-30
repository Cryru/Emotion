#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UIController : UIBaseWindow
    {
        protected static List<UIController> _activeControllers = new List<UIController>();

        public UIBaseWindow InputFocus;
        public UIBaseWindow MouseFocus;

        private bool[] _mouseFocusKeysHeld = new bool[Key.MouseKeyEnd - Key.MouseKeyStart];

        private bool _updatePreload = true;
        protected bool _updateLayout = true;
        protected bool _updateInputFocus = true;

        public UIController()
        {
            InputTransparent = false;
            Debugger = new UIDebugger();
            Engine.Host.OnResize += Host_OnResize;
            Engine.Host.OnMouseScroll += MouseScroll;
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

        public override void AddChild(UIBaseWindow child, int index = -1)
        {
            if (child == null) return;
            base.AddChild(child, index);
            child.AttachedToController(this);
        }

        public override void RemoveChild(UIBaseWindow win, bool evict = true)
        {
            base.RemoveChild(win, evict);
            win.DetachedFromController(this);
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
            public override void AddChild(UIBaseWindow child, int index = -1)
            {
                Children ??= new List<UIBaseWindow>();
                if (index != -1)
                    Children.Insert(index, child);
                else
                    Children.Add(child);
            }

            public override void RemoveChild(UIBaseWindow win, bool evict = true)
            {
                if (Children == null) return;
                if (evict) Children.Remove(win);
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
            _keepWindowsLoaded.AddChild(window, 0);
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
                _mouseFocusKeysHeld[key - Key.MouseKeyStart] = status != KeyStatus.Up;
                return MouseFocus.OnKey(key, status, Engine.Host.MousePosition);
            }

            return true;
        }

        private void MouseScroll(float scroll)
        {
            MouseFocus?.OnMouseScroll(scroll);
        }

        public void InvalidateInputFocus()
        {
            _updateInputFocus = true;
        }

        private void UpdateInputFocus()
        {
            UIBaseWindow newFocus = InputTransparent || !Visible ? null : FindInputFocusable(this);
            if (newFocus == this) newFocus = null;

            if (InputFocus != newFocus)
            {
                // Re-hook event to get up events on down presses.
                if (InputFocus != null)
                    Engine.Host.OnKey.RemoveListener(KeyboardFocusOnKey);
                InputFocus = newFocus;
                if (InputFocus != null)
                    Engine.Host.OnKey.AddListener(KeyboardFocusOnKey);

                // Kinda spammy.
                // Engine.Log.Info($"New input focus {InputFocus}", "UI");
            }

            _updateInputFocus = false;
        }

        private void UpdateMouseFocus()
        {
            Vector2 mousePos = Engine.Host.MousePosition;
            UIBaseWindow newMouseFocus = null;

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
                newMouseFocus = Engine.Host.HostPaused ? null : FindMouseInput(mousePos);
                if (newMouseFocus == this) newMouseFocus = null;
            }

            if (newMouseFocus != MouseFocus)
            {
                MouseFocus?.OnMouseLeft(mousePos);
                if (MouseFocus != null) Engine.Host.OnKey.RemoveListener(MouseFocusOnKey);
                MouseFocus = newMouseFocus;
                if (MouseFocus != null)
                    Engine.Host.OnKey.AddListener(MouseFocusOnKey);
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
            for (var i = 0; i < wnd.Children.Count; i++)
            {
                UIBaseWindow win = wnd.Children[i];
                if (!win.InputTransparent && win.Visible) return FindInputFocusable(win);
            }

            return wnd;
        }

        #endregion

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