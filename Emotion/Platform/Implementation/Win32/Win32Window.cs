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
    /// <inheritdoc />
    public sealed class Win32Window : Window
    {
        /// <summary>
        /// A handle to the native Win32 window DC.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <inheritdoc />
        public override Vector2 Position
        {
            get
            {
                var p = new Point();
                User32.ClientToScreen(Handle, ref p);
                return new Vector2(p.X, p.Y);
            }
            set
            {
                if (DisplayMode != DisplayMode.Windowed) return;

                // Get the size and scale it.
                Vector2 size = Size;
                Rect rect = GetFullWindowRect((int) size.X, (int) size.Y);

                // Set the size and position.
                User32.SetWindowPos(Handle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                    (int) value.X + rect.Left,
                    (int) value.Y + rect.Top,
                    0,
                    0,
                    WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOCOPYBITS | WindowPositionFlags.SWP_SHOWWINDOW | WindowPositionFlags.SWP_NOSIZE);
            }
        }

        /// <inheritdoc />
        public override Vector2 Size
        {
            get
            {
                User32.GetClientRect(Handle, out Rect rect);
                return new Vector2(rect.Right, rect.Bottom);
            }
            set
            {
                if (DisplayMode != DisplayMode.Windowed) return;

                // Scale the rect, and set it.
                Rect r = GetFullWindowRect((int) value.X, (int) value.Y);
                int width = r.Right - r.Left;
                int height = r.Bottom - r.Top;

                // Center on the monitor.
                Monitor monitor = _platform.GetMonitorOfWindow(this);
                if (monitor == null)
                {
                    Engine.Log.Warning("No monitor attached?", MessageSource.Win32);
                    return;
                }

                Vector2 center = monitor.Position + new Vector2(monitor.Width, monitor.Height) / 2 - new Vector2(width, height) / 2;

                User32.SetWindowPos(Handle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                    (int) center.X + r.Left,
                    (int) center.Y + r.Top,
                    r.Right - r.Left,
                    r.Bottom - r.Top,
                    WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_SHOWWINDOW
                );
            }
        }

        /// <inheritdoc />
        public override WindowState WindowState
        {
            get
            {
                var winLong = (long) User32.GetWindowLongPtr(Handle, WindowLongFlags.GWL_STYLE);

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
                        User32.ShowWindow(Handle, ShowWindowCommands.SW_MINIMIZE);
                        break;
                    case WindowState.Maximized:
                        User32.ShowWindow(Handle, ShowWindowCommands.SW_MAXIMIZE);
                        break;
                    case WindowState.Normal:
                        Focus();
                        User32.ShowWindow(Handle, ShowWindowCommands.SW_SHOWNORMAL);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public override DisplayMode DisplayMode
        {
            get => _mode;
            set
            {
                if (value == DisplayMode.Initial) return;
                if (value == _mode) return;
                _mode = value;

                if (_mode == DisplayMode.Fullscreen)
                    // Save window size for when exiting fullscreen.
                    _windowModeSize = Size;

                UpdateDisplayMode();
            }
        }

        private DisplayMode _mode = DisplayMode.Initial;
        private Vector2? _windowModeSize; // When entering a fullscreen mode the window size is stored here so it can be restored later.

        internal Win32Window(IntPtr handle, GraphicsContext context, PlatformBase platform) : base(platform)
        {
            Handle = handle;
            Context = context;
        }

        internal override void UpdateDisplayMode()
        {
            // Get the monitor
            Monitor monitor = _platform.GetMonitorOfWindow(this);
            if (monitor == null)
            {
                Engine.Log.Warning("No monitor attached?", MessageSource.Win32);
                return;
            }

            switch (_mode)
            {
                case DisplayMode.Windowed:
                    WindowStyles style = DetermineWindowStyle(this);
                    IntPtr res = User32.SetWindowLongPtr(Handle, WindowLongFlags.GWL_STYLE, (IntPtr) style);
                    if (res == IntPtr.Zero) Win32Platform.CheckError("Couldn't change display mode to windowed.", true);

                    if (_windowModeSize != null)
                    {
                        Size = (Vector2) _windowModeSize;
                        _windowModeSize = null;
                    }

                    // Center window on screen. (This will also apply the SetWindowLongPtr changes, and remove the topmost status when exiting out of fullscreen)
                    Position = monitor.Position + (new Vector2(monitor.Width, monitor.Height) / 2 - Size / 2);

                    break;
                case DisplayMode.Fullscreen:
                    IntPtr resp = User32.SetWindowLongPtr(Handle, WindowLongFlags.GWL_STYLE, (IntPtr) FULLSCREEN_STYLE);
                    if (resp == IntPtr.Zero)
                    {
                        Win32Platform.CheckError("Couldn't change display mode to fullscreen, couldn't apply window style.", true);
                        return;
                    }

                    bool successful = User32.SetWindowPos(
                        Handle, (IntPtr) HwndZOrder.HWND_NOTOPMOST,
                        (int) monitor.Position.X, (int) monitor.Position.Y,
                        monitor.Width, monitor.Height,
                        WindowPositionFlags.SWP_NOACTIVATE | WindowPositionFlags.SWP_NOCOPYBITS | WindowPositionFlags.SWP_FRAMECHANGED
                    );
                    if (!successful)
                        Win32Platform.CheckError("Couldn't change display mode to fullscreen, couldn't apply window rect.", true);

                    // Center cursor on screen/window.
                    User32.SetCursorPos((int) (monitor.Position.X + monitor.Width / 2), (int) (monitor.Position.Y + monitor.Height / 2));

                    break;
            }

            Context.SwapInternal = 1;
        }

        #region Window Native Helpers

        // Default styles.
        public const WindowStyles DEFAULT_WINDOW_STYLE = WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_SYSMENU | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_CAPTION |
                                                         WindowStyles.WS_MAXIMIZEBOX;

        public const WindowExStyles DEFAULT_WINDOW_STYLE_EX = WindowExStyles.WS_EX_APPWINDOW; // Things like "always top" can be configured here.
        public const WindowStyles FULLSCREEN_STYLE = WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_VISIBLE | WindowStyles.WS_OVERLAPPED;

        /// <summary>
        /// Transform the provided rectangle to fit the scale and position of the window.
        /// </summary>
        /// <param name="rect">The rect to transform.</param>
        /// <returns>The transformed rectangle.</returns>
        public Rect GetFullWindowRect(ref Rect rect)
        {
            uint dpi = Win32Platform.DEFAULT_DPI;

            if (Win32Platform.IsWindows10AnniversaryUpdateOrGreaterWin32)
                dpi = User32.GetDpiForWindow(Handle);

            GetFullWindowRect(DetermineWindowStyle(this), WindowExStyles.WS_EX_APPWINDOW, dpi, ref rect);
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
            if (Win32Platform.IsWindows10AnniversaryUpdateOrGreaterWin32)
                User32.AdjustWindowRectExForDpi(ref rect, style, false, styleEx, dpi);
            else
                User32.AdjustWindowRectEx(ref rect, style, false, styleEx);
        }

        public static void GetWindowContentScale(Win32Window window, out float scaleX, out float scaleY)
        {
            IntPtr monitor = User32.MonitorFromWindow(window.Handle, MonitorFlag.MONITOR_DEFAULTTONEAREST);
            Win32Monitor.GetMonitorContentScale(monitor, out scaleX, out scaleY);
        }

        internal static WindowStyles DetermineWindowStyle(Win32Window window)
        {
            return window.DisplayMode == DisplayMode.Fullscreen ? FULLSCREEN_STYLE : DEFAULT_WINDOW_STYLE;
        }

        #endregion

        public void Focus()
        {
            User32.BringWindowToTop(Handle);
            User32.SetForegroundWindow(Handle);
            User32.SetFocus(Handle);
        }
    }
}