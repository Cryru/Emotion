#region Using

using System.Collections.Generic;
using System.Numerics;

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
                for (int i = 0; i < _openWindows.Count; i++)
                {
                    _openWindows[i].Update();
                }
            }
        }

        public void Render()
        {
            for (int i = 0; i < _openWindows.Count; i++)
            {
                // Remove closed.
                if (!_openWindows[i].Open)
                    lock (_openWindows)
                    {
                        _openWindows.RemoveAt(i);
                        i--;
                        continue;
                    }

                // Overlap prevention on open. (kind of)
                Vector2 spawnOffset = new Vector2(10, 10);
                if (i != 0) spawnOffset = _openWindows[i - 1].Position + _openWindows[i - 1].Size;

                _openWindows[i].Render(spawnOffset);
            }
        }

        public void AddWindow(ImGuiWindow win)
        {
            lock (_openWindows)
            {
                win.Parent = this;
                _openWindows.Add(win);
            }
        }
    }
}