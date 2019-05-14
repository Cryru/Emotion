using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rationale.Windows;

namespace Rationale.Interop
{
    public static class WindowManager
    {
        private static List<Window> _windows = new List<Window>();

        public static void AddWindow(Window window)
        {
            if(_windows.Any(x => x.Title == window.Title)) return;

            _windows.Add(window);
        }

        public static void DrawWindows()
        {
            for (int i = 0; i < _windows.Count; i++)
            {
                if (!_windows[i].Open)
                {
                    _windows.RemoveAt(i);
                    i--;
                    continue;
                }

                _windows[i].Draw();
            }
        }
    }
}
