#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UIController : UIBaseWindow
    {
        public UIBaseWindow InputFocus;

        private bool _updatePreload = true;
        protected bool _updateLayout = true;
        protected bool _updateInputFocus = true;

        public UIController()
        {
            Debugger = new UIDebugger();
            Engine.Host.OnResize += Host_OnResize;
            KeepTemplatePreloaded(this);
        }

        private void Host_OnResize(Vector2 obj)
        {
            InvalidateLayout();
            InvalidatePreload();
        }

        public override void InvalidateLayout()
        {
            _updateLayout = true;
        }

        public void InvalidatePreload()
        {
            _updatePreload = true;
        }

        public override async Task Preload()
        {
            await base.Preload();
            _updatePreload = false;
        }

        protected override bool UpdateInternal()
        {
            if (!UILoadingThread.IsCompleted) return false;
            if (_updatePreload) UpdatePreLoading();

            if (_updateLayout)
            {
                Debugger?.RecordNewPass(this);
                // 1. Measure the size of all windows.
                // Children are measured before parents in order for stretching to work.
                // Children are measured in index order. Layout rules are applied.
                Measure(Engine.Renderer.DrawBuffer.Size);

                // 2. Layout windows within their parents, starting with the controller taking up the full screen.
                // Sizes returned during measuring are used. Parents are positioned before children since
                // positions are absolute and not relative.
                Rectangle r = GetLayoutSpace(Engine.Renderer.DrawBuffer.Size);
                Layout(r.Position);
                _updateLayout = false;
            }

            if (_updateInputFocus) RecalculateInputFocus();

            return true;
        }

        public override void AddChild(UIBaseWindow child, int index = -1)
        {
            if (child == null) return;
            //InvalidatePreload();
            InvalidateInputFocus();
            base.AddChild(child, index);
            child.AttachedToController(this);
        }

        public override void RemoveChild(UIBaseWindow win, bool evict = true)
        {
            base.RemoveChild(win, evict);
            win.DetachedFromController(this);
        }

        #region Preloading

        // Controllers are added to this child. Preloading of all controllers is run when one of them is invalidated.
        // Other windows may be added with the user to be preloadaed. They will also cause all controllers to preload.
        public static Task UILoadingThread { get; protected set; } = Task.CompletedTask;
        private static PreloadWindowStorage _keepWindowsLoaded = new();

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
        }

        public static Task PreloadUI()
        {
            UpdatePreLoading();
            return UILoadingThread;
        }

        public static void KeepTemplatePreloaded(UIBaseWindow window)
        {
            _keepWindowsLoaded.AddChild(window);
        }

        private static void UpdatePreLoading()
        {
            if (!UILoadingThread.IsCompleted) return;
            Engine.Log.Warning("Preloading UI!", "");
            UILoadingThread = _keepWindowsLoaded.Preload();
        }

        #endregion

        #region Input

        private bool KeyboardFocusOnKey(Key key, KeyStatus status)
        {
            // It is possible to receive an input even while a recalculating is pending.
            if (_updateInputFocus && status == KeyStatus.Down)
            {
                RecalculateInputFocus();
                RecalculateMouseFocus();
            }

            if (!Visible) return true;
            if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) return true;
            if (InputFocus != null && InputFocus.Visible)
                return InputFocus.OnKey(key, status);

            return true;
        }

        public void InvalidateInputFocus()
        {
            _updateInputFocus = true;
        }

        private void RecalculateInputFocus()
        {
            UIBaseWindow newFocus = FindInputFocusable(this);
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

        private void RecalculateMouseFocus()
        {
            // todo
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