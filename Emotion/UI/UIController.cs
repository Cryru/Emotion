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
        private bool _needPreload = true;
        protected bool _updateLayout = true;

        public UIController()
        {
            Debugger = new UIDebugger();
            Engine.Host.OnResize += Host_OnResize;
            KeepTemplatePreloaded(this);
        }

        private void Host_OnResize(Vector2 obj)
        {
            InvalidateLayout();
            NeedsPreloading();
        }

        public override void InvalidateLayout()
        {
            _updateLayout = true;
        }

        public void NeedsPreloading()
        {
            _needPreload = true;
        }

        public override async Task Preload()
        {
            await base.Preload();
            _needPreload = false;
        }

        protected override bool UpdateInternal()
        {
            if (!UILoadingThread.IsCompleted) return false;
            if (_needPreload) UpdatePreLoading();

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
            NeedsPreloading();
            base.AddChild(child, index);
            child.AttachedToController(this);
        }

        public override void RemoveChild(UIBaseWindow win, bool evict = true)
        {
            base.RemoveChild(win, evict);
            win.DetachedFromController(this);
        }

        #region Global Preloading

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