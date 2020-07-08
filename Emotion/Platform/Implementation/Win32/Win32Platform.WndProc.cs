#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Platform.Input;
using Emotion.Utility;
using WinApi;
using WinApi.User32;
using User32 = WinApi.User32.User32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32
{
    public partial class Win32Platform
    {
        private WindowProc _wndProcDelegate; // This needs to be assigned so the garbage collector doesn't collect it.

        /// <summary>
        /// Handler for messages.
        /// </summary>
        private unsafe IntPtr WndProc(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            if (_windowHandle == IntPtr.Zero)
            {
                // This is the message handling for the hidden helper window
                // and for a regular window during its initial creation

                switch (msg)
                {
                    case WM.CREATE:
                        if (IsWindows10AnniversaryUpdateOrGreaterWin32)
                            User32.EnableNonClientDpiScaling(hWnd);
                        break;
                    case WM.DISPLAYCHANGE:
                        PollMonitors();
                        break;
                    case WM.DEVICECHANGE:
                        break;
                }

                return User32.DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch (msg)
            {
                case WM.GETMINMAXINFO:
                    Rect windowR = GetFullWindowRect(0, 0);
                    // ReSharper disable once NotAccessedVariable
                    var mmi = (MinMaxInfo*) lParam;

                    int offX = windowR.Right - windowR.Left;
                    int offY = windowR.Bottom - windowR.Top;
                    mmi->MinTrackSize.X = (int) Engine.Configuration.RenderSize.X + offX;
                    mmi->MinTrackSize.Y = (int) Engine.Configuration.RenderSize.Y + offY;

                    return IntPtr.Zero;
                case WM.SIZE:

                    int width = NativeHelpers.LoWord((uint) lParam);
                    int height = NativeHelpers.HiWord((uint) lParam);

                    // Don't send resize event when minimized.
                    if (width != 0 && height != 0) OnResize.Invoke(new Vector2(width, height));

                    return IntPtr.Zero;
                case WM.SETFOCUS:
                    UpdateFocus(true);
                    return IntPtr.Zero;
                case WM.KILLFOCUS:
                    UpdateFocus(false);
                    return IntPtr.Zero;

                case WM.SYSCOMMAND:

                    switch ((int) wParam & 0xfff0)
                    {
                        case (int) SysCommand.SC_SCREENSAVE:
                        case (int) SysCommand.SC_MONITORPOWER:
                            if (DisplayMode == DisplayMode.Fullscreen)
                                // We are running in full screen mode, so disallow
                                // screen saver and screen blanking
                                return IntPtr.Zero;

                            break;

                        // User trying to access application menu using ALT?
                        case (int) SysCommand.SC_KEYMENU:
                            return IntPtr.Zero;
                    }

                    break;

                case WM.CLOSE:
                    IsOpen = false;
                    return IntPtr.Zero;

                // ------------------- INPUT -------------------
                case WM.INPUTLANGCHANGE:
                    PopulateKeyNames();
                    break;

                case WM.CHAR:
                case WM.SYSCHAR:
                case WM.UNICHAR:

                    if (msg == WM.UNICHAR && (int) wParam == 0xFFFF)
                        // WM_UNICHAR is not sent by Windows, but is sent by some
                        // third-party input method engine
                        // Returning TRUE here announces support for this message
                        return (IntPtr) 1;

                    OnTextInput.Invoke((char) wParam);

                    break;

                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                case WM.KEYUP:
                case WM.SYSKEYUP:

                    var lParamLong = (ulong) lParam;
                    Key key = TranslateKey((ulong) wParam, lParamLong);
                    if (key == Key.Unknown) break;

                    // Scan code can be used for debugging purposes.
                    //uint scanCode = NativeHelpers.HiWord(lParamLong) & (uint) 0x1ff;
                    bool up = (((ulong) lParam >> 31) & 1) != 0;

                    if (up && (uint) wParam == (uint) VirtualKey.SHIFT)
                    {
                        // HACK: Release both Shift keys on Shift up event, as when both
                        //       are pressed the first release does not emit any event
                        // NOTE: The other half of this is in Update()
                        UpdateKeyStatus(Key.LeftShift, false);
                        UpdateKeyStatus(Key.RightShift, false);
                    }
                    else
                    {
                        UpdateKeyStatus(key, !up);
                    }

                    break;

                case WM.MOUSEMOVE:

                    int x = NativeHelpers.LoWord((uint) lParam);
                    int y = NativeHelpers.HiWord((uint) lParam);

                    var pos = new Vector2(x, y);
                    MousePosition = Engine.Renderer != null ? Engine.Renderer.ScaleMousePosition(pos) : pos;
                    return IntPtr.Zero;

                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.XBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                case WM.XBUTTONUP:

                    var mouseKey = MouseKey.Unknown;
                    var buttonDown = false;
                    mouseKey = msg switch
                    {
                        WM.LBUTTONDOWN => MouseKey.Left,
                        WM.LBUTTONUP => MouseKey.Left,
                        WM.RBUTTONDOWN => MouseKey.Right,
                        WM.RBUTTONUP => MouseKey.Right,
                        WM.MBUTTONDOWN => MouseKey.Middle,
                        WM.MBUTTONUP => MouseKey.Middle,
                        _ => mouseKey
                    };

                    if (msg == WM.LBUTTONDOWN || msg == WM.RBUTTONDOWN ||
                        msg == WM.MBUTTONDOWN || msg == WM.XBUTTONDOWN)
                        buttonDown = true;

                    var nonePressed = true;
                    foreach (bool keyDown in _mouseKeys)
                    {
                        if (keyDown) nonePressed = false;
                    }

                    if (nonePressed) User32.SetCapture(_windowHandle);

                    UpdateMouseKeyStatus(mouseKey, buttonDown);

                    nonePressed = true;
                    foreach (bool keyDown in _mouseKeys)
                    {
                        if (keyDown) nonePressed = false;
                    }

                    if (nonePressed) User32.ReleaseCapture();

                    if (msg == WM.XBUTTONDOWN || msg == WM.XBUTTONUP)
                        return (IntPtr) 1;

                    return IntPtr.Zero;

                case WM.MOUSEWHEEL:

                    var scrollAmount = (short) NativeHelpers.HiWord((ulong) wParam);
                    UpdateScroll(scrollAmount / 120f);

                    return IntPtr.Zero;
            }

            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}