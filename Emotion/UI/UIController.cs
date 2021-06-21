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
        public UIController()
        {
            Color = new Color(0, 0, 0, 0);
            Debugger = new UIDebugger();
            Engine.Host.OnResize += Host_OnResize;
        }

        private void Host_OnResize(Vector2 obj)
        {
            InvalidateLayout();
            _needPreload = true;
        }

        public Task PreloadUI()
        {
            if (_needPreload)
            {
                PreloadChildren();
                _needPreload = false;
            }

            return UILoadingThread;
        }

        public void Update()
        {
            if (!PreloadUI().IsCompleted) return;

            if (_updateLayout)
            {
                Debugger?.RecordNewPass(this);
                Measure(Engine.Renderer.DrawBuffer.Size);
                Layout(Vector2.Zero, Engine.Renderer.DrawBuffer.Size);
            }

            if (_updateColor) CalculateColor();
        }

        #region Preloading

        public Task UILoadingThread { get; protected set; } = Task.CompletedTask;
        private List<UIBaseWindow> _uiKeepLoaded = new List<UIBaseWindow>();
        private List<UIBaseWindow> _uiKeepLoadedExplicit = new List<UIBaseWindow>();
        private bool _needPreload = true;

        public override void AddChild(UIBaseWindow child)
        {
            AddUIToPreload(child);
            base.AddChild(child);
        }

        public override void ClearChildren()
        {
            base.ClearChildren();
            _uiKeepLoaded.Clear();
            _uiKeepLoaded.AddRange(_uiKeepLoadedExplicit);
        }

        private void PreloadChildren()
        {
            if (!UILoadingThread.IsCompleted) return;

            UILoadingThread = Task.Run(async () =>
            {
                for (var i = 0; i < _uiKeepLoaded.Count; i++)
                {
                    UIBaseWindow wnd = _uiKeepLoaded[i];
                    await wnd.Preload();
                }
            });
        }

        public void AddUIToPreload(UIBaseWindow child)
        {
            _uiKeepLoadedExplicit.Add(child);
            _uiKeepLoaded.Add(child);
            _needPreload = true;
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