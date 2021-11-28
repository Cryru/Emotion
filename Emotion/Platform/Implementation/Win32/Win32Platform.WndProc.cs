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
                    Resized(new Vector2(width, height));

                    return IntPtr.Zero;
                case WM.SETFOCUS:
                    FocusChanged(true);
                    return IntPtr.Zero;
                case WM.KILLFOCUS:
                    FocusChanged(false);
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
                case WM.CHAR:
                case WM.SYSCHAR:
                case WM.UNICHAR:

                    if (msg == WM.UNICHAR && (int) wParam == 0xFFFF)
                        // WM_UNICHAR is not sent by Windows, but is sent by some
                        // third-party input method engine
                        // Returning TRUE here announces support for this message
                        return (IntPtr) 1;

                    UpdateTextInput((char) wParam);
                    break;

                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                case WM.KEYUP:
                case WM.SYSKEYUP:
                    var virtualKey = (VirtualKey) wParam;
                    var lParamLong = (ulong) lParam;

                    bool up = ((lParamLong >> 31) & 1) != 0;
                    Key key = TranslateKey(virtualKey, lParamLong);
                    if (key == Key.Unknown) break;

                    // Don't handle shift keys. Check UpdatePlatform() for more info.
                    if (virtualKey == VirtualKey.SHIFT) break;

                    UpdateKeyStatus(key, !up);
                    break;

                case WM.MOUSEMOVE:

                    int x = NativeHelpers.LoWordS((uint) lParam);
                    int y = NativeHelpers.HiWordS((uint) lParam);
                    MousePosition = new Vector2(x, y);
                    return IntPtr.Zero;

                case WM.LBUTTONDOWN:
                case WM.RBUTTONDOWN:
                case WM.MBUTTONDOWN:
                case WM.XBUTTONDOWN:
                case WM.LBUTTONUP:
                case WM.RBUTTONUP:
                case WM.MBUTTONUP:
                case WM.XBUTTONUP:

                    ushort keySpecifier = NativeHelpers.HiWord((ulong) wParam);
                    var mouseKey = Key.Unknown;
                    var buttonDown = false;
                    mouseKey = msg switch
                    {
                        WM.LBUTTONDOWN => Key.MouseKeyLeft,
                        WM.LBUTTONUP => Key.MouseKeyLeft,
                        WM.RBUTTONDOWN => Key.MouseKeyRight,
                        WM.RBUTTONUP => Key.MouseKeyRight,
                        WM.MBUTTONDOWN => Key.MouseKeyMiddle,
                        WM.MBUTTONUP => Key.MouseKeyMiddle,
                        WM.XBUTTONDOWN when keySpecifier == 1 => Key.MouseKey4,
                        WM.XBUTTONUP when keySpecifier == 1 => Key.MouseKey4,
                        WM.XBUTTONDOWN when keySpecifier == 2 => Key.MouseKey5,
                        WM.XBUTTONUP when keySpecifier == 2 => Key.MouseKey5,

                        _ => mouseKey
                    };

                    if (msg == WM.LBUTTONDOWN || msg == WM.RBUTTONDOWN ||
                        msg == WM.MBUTTONDOWN || msg == WM.XBUTTONDOWN)
                        buttonDown = true;

                    var nonePressed = true;
                    for (int i = (int) Key.MouseKeyStart + 1; i < (int) Key.MouseKeyEnd; i++)
                    {
                        if (!_keys[i]) continue;
                        nonePressed = false;
                        break;
                    }

                    if (nonePressed) User32.SetCapture(_windowHandle);

                    UpdateKeyStatus(mouseKey, buttonDown);

                    nonePressed = true;
                    for (int i = (int) Key.MouseKeyStart + 1; i < (int) Key.MouseKeyEnd; i++)
                    {
                        if (!_keys[i]) continue;
                        nonePressed = false;
                        break;
                    }

                    if (nonePressed) User32.ReleaseCapture();

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