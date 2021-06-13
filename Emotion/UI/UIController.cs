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

        public void Update()
        {
            if (_updateLayout)
            {
                Debugger?.RecordNewPass(this);
                Measure(Engine.Renderer.DrawBuffer.Size);
                Layout(Vector2.Zero, Engine.Renderer.DrawBuffer.Size);
                _updateLayout = false;
            }

            if (_needPreload)
            {
                PreloadChildren();
                _needPreload = false;
            }
        }

        public override void Render(RenderComposer c)
        {
            base.Render(c);
            if (!DrawDebugGrid) return;

            var scaledGridSize = (int) Maths.RoundClosest(DebugGridSize * GetScale());
            for (var y = 0; y < Size.Y; y += scaledGridSize)
            {
                for (var x = 0; x < Size.X; x += scaledGridSize)
                {
                    c.RenderOutline(new Rectangle(x, y, scaledGridSize, scaledGridSize), Color.White * 0.2f);
                }
            }

            c.RenderLine(Position + new Vector2(Size.X / 2, 0), Position + new Vector2(Size.X / 2, Size.Y), Color.White * 0.8f);
            c.RenderLine(Position + new Vector2(0, Size.Y / 2), Position + new Vector2(Size.X, Size.Y / 2), Color.White * 0.8f);
        }

        #region Preloading

        public Task UILoadingThread { get; protected set; } = Task.CompletedTask;
        private List<UIBaseWindow> _windowsToLoad = new List<UIBaseWindow>();
        private bool _needPreload = true;

        public override void AddChild(UIBaseWindow child)
        {
            PreloadChild(child);
            base.AddChild(child);
        }

        private void PreloadChildren()
        {
            if (!UILoadingThread.IsCompleted) return;

            UILoadingThread = Task.Run(async () =>
            {
                for (var i = 0; i < _windowsToLoad.Count; i++)
                {
                    UIBaseWindow wnd = _windowsToLoad[i];
                    await wnd.Preload();
                }
            });
        }

        public void PreloadChild(UIBaseWindow child)
        {
            _windowsToLoad.Add(child);
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