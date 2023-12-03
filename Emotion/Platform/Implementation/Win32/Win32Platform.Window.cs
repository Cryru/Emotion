#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Standard.Logging;
using WinApi;
using WinApi.User32;
using User32 = WinApi.User32.User32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32
{
    public partial class Win32Platform
    {
        /// <inheritdoc />
        public override WindowState WindowState
        {
            get
            {
                var winLong = (long) User32.GetWindowLongPtr(_windowHandle, WindowLongFlags.GWL_STYLE);

                if ((winLong & (long) WindowStyles.WS_MINIMIZE) != 0) return WindowState.Minimized;

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if ((winLong & (long) WindowStyles.WS_MAXIMIZE) != 0) return WindowState.Maximized;

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Minimized:
                        User32.ShowWindow(_windowHandle, ShowWindowCommands.SW_MINIMIZE);
                        break;
                    case WindowState.Maximized:
                        User32.ShowWindow(_windowHandle, ShowWindowCommands.SW_MAXIMIZE);
                        break;
                    case WindowState.Normal:
                        FocusWindow();
                        User32.ShowWindow(_windowHandle, ShowWindowCommands.SW_SHOWNORMAL);
                        break;
                }
            }
        }

        public void FocusWindow()
        {
            User32.BringWindowToTop(_windowHandle);
            User32.SetForegroundWindow(_windowHandle);
            User32.SetFocus(_windowHandle);
        }

        #region Window Native Helpers

        // Default styles.
        public const WindowStyles DEFAULT_WINDOW_STYLE = WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_SYSMENU | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_CAPTION |
                                                         WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_THICKFRAME;

        public const WindowExStyles DEFAULT_WINDOW_STYLE_EX = WindowExStyles.WS_EX_APPWINDOW; // Things like "always top" can be configured here.
        public const WindowStyles FULLSCREEN_STYLE = WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_VISIBLE | WindowStyles.WS_OVERLAPPED;

        /// <summary>
        /// Transform the provided rectangle to fit the scale and position of the window.
        /// </summary>
        /// <param name="rect">The rect to transform.</param>
        /// <returns>The transformed rectangle.</returns>
        public Rect GetFullWindowRect(ref Rect rect)
        {
            uint dpi = DEFAULT_DPI;

            if (IsWindows10AnniversaryUpdateOrGreaterWin32)
                dpi = User32.GetDpiForWindow(_windowHandle);

            GetFullWindowRect(DetermineWindowStyle(), WindowExStyles.WS_EX_APPWINDOW, dpi, ref rect);
            return rect;
        }

        public Rect GetFullWindowRect(int contentWidth, int contentHeight)
        {
            var rect = new Rect
            {
                Right = contentWidth,
                Bottom = contentHeight
            };

            GetFullWindowRect(ref rect);
            return rect;
        }

        public static void GetFullWindowRect(WindowStyles style, WindowExStyles styleEx, uint dpi, ref Rect rect)
        {
            if (IsWindows10AnniversaryUpdateOrGreaterWin32)
                User32.AdjustWindowRectExForDpi(ref rect, style, false, styleEx, dpi);
            else
                User32.AdjustWindowRectEx(ref rect, style, false, styleEx);
        }

        public void GetWindowContentScale(out float scaleX, out float scaleY)
        {
            IntPtr monitor = User32.MonitorFromWindow(_windowHandle, MonitorFlag.MONITOR_DEFAULTTONEAREST);
            Win32Monitor.GetMonitorContentScale(monitor, out scaleX, out scaleY);
        }

        internal WindowStyles DetermineWindowStyle()
        {
            return DisplayMode == DisplayMode.Fullscreen ? FULLSCREEN_STYLE : DEFAULT_WINDOW_STYLE;
        }

        #endregion

        protected override void SetPosition(Vector2 position)
        {
            if (DisplayMode != DisplayMode.Windowed) return;

            // Get the size and scale it.
            Vector2 size = Size;
            Rect rect = GetFullWindowRect((int) size.X, (int) size.Y);

            // Set the size and position.
            User32.SetWindowPos(_windowHandle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                (int) position.X + rect.Left,
                (int) position.Y + rect.Top,
                0,
                0,
                WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOCOPYBITS | WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOSIZE);
        }

        protected override Vector2 GetPosition()
        {
            var p = new Point();
            User32.ClientToScreen(_windowHandle, ref p);
            return new Vector2(p.X, p.Y);
        }

        protected override void SetSize(Vector2 value)
        {
            // Scale the rect, and set it.
            Rect r = GetFullWindowRect((int) value.X, (int) value.Y);
            int width = (int) value.X;// r.Right - r.Left;
            int height = (int) value.Y;// r.Bottom - r.Top;

            // Center on the monitor.
            Monitor monitor = GetMonitorOfWindow();
            if (monitor == null)
            {
                Engine.Log.Warning("No monitor attached?", MessageSource.Win32);
                return;
            }

            Vector2 center = monitor.Position + new Vector2(monitor.Width, monitor.Height) / 2 - new Vector2(width, height) / 2;

            User32.SetWindowPos(_windowHandle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                (int) center.X + r.Left,
                (int) center.Y + r.Top,
                r.Right - r.Left,
                r.Bottom - r.Top,
                WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_SHOWWINDOW
            );
        }

        protected override Vector2 GetSize()
        {
            User32.GetClientRect(_windowHandle, out Rect rect);
            return new Vector2(rect.Right, rect.Bottom);
        }

        protected override void UpdateDisplayMode()
        {
            // Get the monitor
            Monitor monitor = GetMonitorOfWindow();
            if (monitor == null)
            {
                Engine.Log.Warning("No monitor attached?", MessageSource.Win32);
                return;
            }

            switch (_mode)
            {
                case DisplayMode.Windowed:
                    WindowStyles style = DetermineWindowStyle();
                    IntPtr res = User32.SetWindowLongPtr(_windowHandle, WindowLongFlags.GWL_STYLE, (IntPtr) style);
                    if (res == IntPtr.Zero) CheckError("Couldn't change display mode to windowed.", true);

                    if (_windowModeSize != null)
                    {
                        Size = (Vector2) _windowModeSize;
                        _windowModeSize = null;
                    }

                    // Center window on screen. (This will also apply the SetWindowLongPtr changes, and remove the topmost status when exiting out of fullscreen)
                    Position = monitor.Position + (new Vector2(monitor.Width, monitor.Height) / 2 - Size / 2);

                    break;
                case DisplayMode.Fullscreen:
                    IntPtr resp = User32.SetWindowLongPtr(_windowHandle, WindowLongFlags.GWL_STYLE, (IntPtr) FULLSCREEN_STYLE);
                    if (resp == IntPtr.Zero)
                    {
                        CheckError("Couldn't change display mode to fullscreen, couldn't apply window style.", true);
                        return;
                    }

                    bool successful = User32.SetWindowPos(
                        _windowHandle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                        (int) monitor.Position.X, (int) monitor.Position.Y,
                        monitor.Width, monitor.Height,
                        WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOCOPYBITS | WindowPositionFlags.SWP_FRAMECHANGED
                    );
                    if (!successful)
                        CheckError("Couldn't change display mode to fullscreen, couldn't apply window rect.", true);

                    // Center cursor on screen/window.
                    User32.SetCursorPos((int) (monitor.Position.X + monitor.Width / 2), (int) (monitor.Position.Y + monitor.Height / 2));

                    break;
            }
        }
    }
}