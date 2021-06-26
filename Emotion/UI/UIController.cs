#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UIController : UIBaseWindow
    {
        protected bool _updateLayout = true;

        public UIController()
        {
            Debugger = new UIDebugger();
            Engine.Host.OnResize += Host_OnResize;
            KeepTemplatePreloaded(this);
        }

        public override void InvalidateLayout()
        {
            _updateLayout = true;
        }

        private void Host_OnResize(Vector2 obj)
        {
            InvalidateLayout();
            _needPreload = true;
        }

        protected override bool UpdateInternal()
        {
            UpdatePreLoading();
            if (!UILoadingThread.IsCompleted) return false;

            if (_updateLayout)
            {
                Debugger?.RecordNewPass(this);
                Measure(Engine.Renderer.DrawBuffer.Size);

                Rectangle r = GetLayoutSpace(Engine.Renderer.DrawBuffer.Size);
                Layout(r.Position, r.Size);
                _updateLayout = false;
            }

            return true;
        }

        public override void AddChild(UIBaseWindow child, int index = -1)
        {
            if (child == null) return;
            RequestPreload();
            base.AddChild(child, index);
            child.AttachedToController();
        }

        public override void RemoveChild(UIBaseWindow win, bool evict = true)
        {
            base.RemoveChild(win, evict);
            win.DetachedFromController();
        }

        #region Global Preloading

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
        private static bool _needPreload = true;

        public static Task PreloadUI()
        {
            UpdatePreLoading();
            return UILoadingThread;
        }

        public static void KeepTemplatePreloaded(UIBaseWindow window)
        {
            _keepWindowsLoaded.AddChild(window);
        }

        public static void RequestPreload()
        {
            _needPreload = true;
        }

        private static void UpdatePreLoading()
        {
            if (!_needPreload) return;
            if (!UILoadingThread.IsCompleted) return;
            UILoadingThread = _keepWindowsLoaded.Preload();
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