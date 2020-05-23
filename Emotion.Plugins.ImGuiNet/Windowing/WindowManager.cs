#region Using

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
// ReSharper disable InconsistentlySynchronizedField

#endregion

namespace Emotion.Plugins.ImGuiNet.Windowing
{
    public class WindowManager
    {
        private List<ImGuiWindow> _openWindows = new List<ImGuiWindow>();

        public void Update()
        {
            lock (_openWindows)
            {
                foreach (ImGuiWindow w in _openWindows)
                {
                    w.Update();
                }
            }
        }

        public void Render(RenderComposer composer)
        {
            for (var i = 0; i < _openWindows.Count; i++)
            {
                // Remove closed.
                if (!_openWindows[i].Open)
                    lock (_openWindows)
                    {
                        _openWindows[i].Dispose();
                        _openWindows.RemoveAt(i);
                        i--;
                        continue;
                    }

                // Overlap prevention on open. (kind of)
                var spawnOffset = new Vector2(10, 10);
                if (i != 0)
                {
                    ImGuiWindow prev = _openWindows[i - 1];
                    spawnOffset = prev.Position.X + prev.Size.X * 2 > Engine.Renderer.DrawBuffer.Size.X ? 
                        new Vector2(prev.Position.X, prev.Position.Y + prev.Size.Y + 10) : 
                        new Vector2(_openWindows[i - 1].Position.X + _openWindows[i - 1].Size.X + 10, _openWindows[i - 1].Position.Y);
                }

                _openWindows[i].Render(spawnOffset, composer);
            }
        }

        public void AddWindow(ImGuiWindow win)
        {
            lock (_openWindows)
            {
                // Check if unique.
                if (_openWindows.Any(window => window.Title == win.Title))
                {
                    return;
                }

                win.Parent = this;
                win.OnOpen();
                _openWindows.Add(win);
            }
        }
    }
}