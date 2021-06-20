#region Using

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Utility;

#endregion

namespace Emotion.UI
{
    public class UIController : UIBaseWindow
    {
        public bool DrawDebugGrid;
        public int DebugGridSize = 20;

        public UIController()
        {
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
                _updateLayout = false;
            }
        }

        protected override bool RenderInternal(RenderComposer c, ref Color windowColor)
        {
            base.RenderInternal(c, ref windowColor);
            if (!DrawDebugGrid) return true;

            var scaledGridSize = (int) Maths.RoundClosest(DebugGridSize * GetScale());
            for (var y = 0; y < Size.Y; y += scaledGridSize)
            {
                for (var x = 0; x < Size.X; x += scaledGridSize)
                {
                    c.RenderOutline(new Rectangle(x, y, scaledGridSize, scaledGridSize), Color.White * 0.2f);
                }
            }

            Vector2 posVec2 = Position.ToVec2();
            c.RenderLine(posVec2 + new Vector2(Size.X / 2, 0), posVec2 + new Vector2(Size.X / 2, Size.Y), Color.White * 0.8f);
            c.RenderLine(posVec2 + new Vector2(0, Size.Y / 2), posVec2 + new Vector2(Size.X, Size.Y / 2), Color.White * 0.8f);
            return true;
        }

        #region Preloading

        public Task UILoadingThread { get; protected set; } = Task.CompletedTask;
        private List<UIBaseWindow> _uiKeepLoaded = new List<UIBaseWindow>();
        private bool _needPreload = true;

        public override void AddChild(UIBaseWindow child)
        {
            AddUIToPreload(child);
            base.AddChild(child);
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