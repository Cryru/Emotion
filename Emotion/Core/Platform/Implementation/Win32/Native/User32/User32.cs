#nullable enable

#region Using

using Emotion;
using Emotion.Core.Platform.Implementation.Win32.Native;
using System.Runtime.InteropServices;
using System.Text;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native.User32;

public static class User32
{
    public const string LIBRARY_NAME = "user32";

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadIcon(nint hInstance, string lpIconName);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadIcon(nint hInstance, nint lpIconResource);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadCursor(nint hInstance, string lpCursorName);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadCursor(nint hInstance, nint lpCursorResource);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadImage(nint hInstance, string lpszName, ResourceImageType uType,
        int cxDesired, int cyDesired, LoadResourceFlags fuLoad);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadImage(nint hInstance, nint resourceId, ResourceImageType uType,
        int cxDesired, int cyDesired, LoadResourceFlags fuLoad);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadBitmap(nint hInstance, string lpBitmapName);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint LoadBitmap(nint hInstance, nint resourceId);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint BeginPaint(nint hwnd, out PaintStruct lpPaint);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern void EndPaint(nint hwnd, [In] ref PaintStruct lpPaint);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetDesktopWindow();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint MonitorFromPoint(Point pt, MonitorFlag dwFlags);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint MonitorFromWindow(nint hwnd, MonitorFlag dwFlags);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int DrawText(nint hdc, string lpString, int nCount, [In] ref Rect lpRect,
        uint uFormat);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool RegisterHotKey(nint hWnd, int id, KeyModifierFlags fsModifiers, VirtualKey vk);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint SendInput(uint nInputs, nint pInputs, int cbSize);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint SendInput(uint nInputs, [In] Input[] pInputs, int cbSize);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetTimer(nint hWnd, nint nIdEvent, uint uElapseMillis, TimerProc lpTimerFunc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool KillTimer(nint hwnd, nint uIdEvent);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ValidateRect(nint hWnd, [In] ref Rect lpRect);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ValidateRect(nint hWnd, nint lpRect);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InvalidateRect(nint hWnd, [In] ref Rect lpRect, bool bErase);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InvalidateRect(nint hWnd, nint lpRect, bool bErase);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetDC(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern int GetSystemMetrics(SystemMetrics nIndex);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, nint pvParam,
        SystemParamtersInfoFlags fWinIni);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref nint pvParam,
        SystemParamtersInfoFlags fWinIni);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetWindowDC(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint WindowFromDC(nint hdc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ReleaseDC(nint hwnd, nint hdc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InvertRect(nint hdc, [In] ref Rect lprc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetRectEmpty(out Rect lprc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AdjustWindowRect([In] [Out] ref Rect lpRect, WindowStyles dwStyle, bool hasMenu);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AdjustWindowRectEx([In] [Out] ref Rect lpRect, WindowStyles dwStyle, bool hasMenu,
        WindowExStyles dwExStyle);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-adjustwindowrectexfordpi
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool AdjustWindowRectExForDpi(ref Rect lpRect, WindowStyles dwStyle,
        bool bMenu, WindowExStyles dwExStyle, uint dpi);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool CopyRect(out Rect lprcDst, [In] ref Rect lprcSrc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IntersectRect(out Rect lprcDst, [In] ref Rect lprcSrc1,
        [In] ref Rect lprcSrc2);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool UnionRect(out Rect lprcDst, [In] ref Rect lprcSrc1,
        [In] ref Rect lprcSrc2);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsRectEmpty([In] ref Rect lprc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool PtInRect([In] ref Rect lprc, Point pt);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool OffsetRect([In] [Out] ref Rect lprc, int dx, int dy);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InflateRect([In] [Out] ref Rect lprc, int dx, int dy);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool FrameRect(nint hdc, [In] ref Rect lprc, nint hbr);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool FillRect(nint hdc, [In] ref Rect lprc, nint hbr);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern RegionType GetWindowRgn(nint hWnd, nint hRgn);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern RegionType GetWindowRgnBox(nint hWnd, out Rect lprc);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha,
        LayeredWindowAttributeFlag dwFlags);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool WinHelp(nint hWndMain, string lpszHelp, uint uCommand, uint dwData);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetWindowRgn(nint hWnd, nint hRgn, bool bRedraw);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InvalidateRgn(nint hWnd, nint hRgn, bool bErase);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetUpdateRect(nint hwnd, out Rect rect, bool bErase);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ValidateRgn(nint hWnd, nint hRgn);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetDCEx(nint hWnd, nint hrgnClip, DeviceContextFlags flags);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern void DisableProcessWindowsGhosting();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool UpdateLayeredWindow(nint hwnd, nint hdcDst,
        [In] ref Point pptDst, [In] ref Point psize, nint hdcSrc, [In] ref Point pptSrc, uint crKey,
        [In] ref BlendFunction pblend, uint dwFlags);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern int MapWindowPoints(nint hWndFrom, nint hWndTo, nint lpPoints, int cPoints);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ScreenToClient(nint hWnd, [In] [Out] ref Point lpPoint);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint WindowFromPhysicalPoint(Point point);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint WindowFromPoint(Point point);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsWindowVisible(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool OpenIcon(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsWindow(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsHungAppWindow(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsZoomed(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsIconic(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool LogicalToPhysicalPoint(nint hwnd, [In] [Out] ref Point point);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint ChildWindowFromPoint(nint hwndParent, Point point);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint ChildWindowFromPointEx(nint hwndParent, Point point,
        ChildWindowFromPointFlags flags);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetMenu(nint hwnd);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern MessageBoxResult MessageBox(nint hWnd, string lpText, string lpCaption, uint type);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern MessageBoxResult MessageBoxEx(nint hWnd, string lpText, string lpCaption, uint type,
        ushort wLanguageId);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetSystemMenu(nint hWnd, bool bRevert);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetThreadDesktop(nint hDesk);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetThreadDesktop(uint threadId);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool GetMonitorInfo(nint hMonitor, ref MonitorInfo lpmi);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool GetMonitorInfo(nint hMonitor, ref MonitorInfoEx lpmi);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-tounicode
    /// </summary>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int ToUnicode(
        uint virtualKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out] [MarshalAs(UnmanagedType.LPWStr)]
        StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags
    );

    #region Notification

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-registerdevicenotificationw
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern nint RegisterDeviceNotificationW(
        nint hRecipient,
        ref DevBroadcastDeviceInterfaceW notificationFilter,
        DeviceNotificationFlags flags
    );

    #endregion

    #region DPI Awareness

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-enablenonclientdpiscaling
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool EnableNonClientDpiScaling(nint hWnd);


    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setprocessdpiawarenesscontext
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool SetProcessDpiAwarenessContext(DpiAwarenessContext value);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/shellscalingapi/nf-shellscalingapi-setprocessdpiawareness
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool SetProcessDpiAwareness(ProcessDpiAwareness value);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-setprocessdpiaware
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool SetProcessDPIAware();

    #endregion

    #region Keyboard, Mouse & Input Method Functions

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool IsWindowEnabled(nint hWnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ReleaseCapture();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetCapture(nint hWnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetFocus();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetFocus(nint hWnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetActiveWindow(nint hWnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetActiveWindow();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool BlockInput(bool fBlockIt);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint WaitForInputIdle(nint hProcess, uint dwMilliseconds);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool DragDetect(nint hwnd, Point point);

    /// <summary>
    /// Converts the client-area coordinates of a specified point to screen coordinates.
    /// </summary>
    /// <param name="hwnd">Handle to the window to use for conversion.</param>
    /// <param name="point">The point converting.</param>
    /// <returns></returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ClientToScreen(nint hwnd, [In] [Out] ref Point point);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getdpiforwindow
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern uint GetDpiForWindow(nint hWnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ClipCursor([In] ref Rect rect);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ClipCursor(nint ptr);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool TrackMouseEvent([In] [Out] ref TrackMouseEventOptions lpEventTrack);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetLastInputInfo(out LastInputInfo plii);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern uint MapVirtualKey(uint uCode, VirtualKeyMapType uMapType);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern uint MapVirtualKey(VirtualKey uCode, VirtualKeyMapType uMapType);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern uint MapVirtualKeyEx(uint uCode, VirtualKeyMapType uMapType, nint dwhkl);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern uint MapVirtualKeyEx(VirtualKey uCode, VirtualKeyMapType uMapType, nint dwhkl);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern Win32KeyState GetAsyncKeyState(int vKey);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern Win32KeyState GetAsyncKeyState(VirtualKey vKey);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern Win32KeyState GetKeyState(int vKey);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern Win32KeyState GetKeyState(VirtualKey vKey);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetKeyboardState(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 256)]
        out byte[] lpKeyState);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetKeyboardState(nint lpKeyState);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetKeyboardState(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 256)] [In]
        byte[] lpKeyState);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetKeyboardState(nint lpKeyState);

    /// <summary>
    /// Retrieves information about the specified title bar.
    /// </summary>
    /// <param name="hwnd">A handle to the title bar whose information is to be retrieved.</param>
    /// <param name="pti">
    /// A pointer to a TITLEBARINFO structure to receive the information. Note that you must set the cbSize
    /// member to sizeof(TITLEBARINFO) before calling this function.
    /// </param>
    /// <returns></returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetTitleBarInfo(nint hwnd, nint pti);

    #endregion

    #region Cursor Functions

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetCursorPos(out Point point);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetPhysicalCursorPos(int x, int y);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetSystemCursor(nint cursor, SystemCursor id);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern int ShowCursor(bool bShow);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetCursor(nint hCursor);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint CopyCursor(nint hCursor);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetCursorInfo(ref CursorInfo info);

    #endregion

    #region Window Functions

    /// <summary>
    /// Enables you to produce special effects when showing or hiding windows. There are four types of animation: roll,
    /// slide, collapse or expand, and alpha-blended fade.
    /// </summary>
    /// <param name="hwnd">
    /// [Type: HWND]
    /// A handle to the window to animate.The calling thread must own this window.
    /// </param>
    /// <param name="dwTime">
    /// [Type: DWORD]
    /// The time it takes to play the animation, in milliseconds.Typically, an animation takes 200 milliseconds to play.
    /// </param>
    /// <param name="dwFlags">
    /// [Type: DWORD]
    /// The type of animation.This parameter can be one or more of the following values. Note that, by default, these flags
    /// take effect when showing a window. To take effect when hiding a window, use AW_HIDE and a logical OR operator with
    /// the appropriate flags.
    /// </param>
    /// <returns>
    /// [Type: BOOL]
    /// If the function succeeds, the return value is nonzero.
    /// If the function fails, the return value is zero. The function will fail in the following situations:
    /// If the window is already visible and you are trying to show the window.
    /// If the window is already hidden and you are trying to hide the window.
    /// If there is no direction specified for the slide or roll animation.
    /// When trying to animate a child window with AW_BLEND.
    /// If the thread does not own the window. Note that, in this case, AnimateWindow fails but GetLastError returns
    /// ERROR_SUCCESS.
    /// To get extended error information, call the GetLastError function.
    /// </returns>
    /// <remarks>
    /// To show or hide a window without special effects, use ShowWindow.
    /// When using slide or roll animation, you must specify the direction. It can be either AW_HOR_POSITIVE,
    /// AW_HOR_NEGATIVE, AW_VER_POSITIVE, or AW_VER_NEGATIVE.
    /// You can combine AW_HOR_POSITIVE or AW_HOR_NEGATIVE with AW_VER_POSITIVE or AW_VER_NEGATIVE to animate a window
    /// diagonally.
    /// The window procedures for the window and its child windows should handle any WM_PRINT or WM_PRINTCLIENT messages.
    /// Dialog boxes, controls, and common controls already handle WM_PRINTCLIENT. The default window procedure already
    /// handles WM_PRINT.
    /// If a child window is displayed partially clipped, when it is animated it will have holes where it is clipped.
    /// AnimateWindow supports RTL windows.
    /// Avoid animating a window that has a drop shadow because it produces visually distracting, jerky animations.
    /// </remarks>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AnimateWindow(nint hwnd, int dwTime, AnimateWindowFlags dwFlags);

    /// <summary>
    /// Retrieves a handle to the top-level window whose class name and window name match the specified strings. This
    /// function does not search child windows. This function does not perform a case-sensitive search.
    /// To search child windows, beginning with a specified child window, use the FindWindowEx function.
    /// </summary>
    /// <param name="lpClassName">
    /// [Type: LPCTSTR]
    /// The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The
    /// atom must be in the low-order word of lpClassName; the high-order word must be zero.
    /// If lpClassName points to a string, it specifies the window class name. The class name can be any name registered
    /// with RegisterClass or RegisterClassEx, or any of the predefined control-class names.
    /// If lpClassName is NULL, it finds any window whose title matches the lpWindowName parameter.
    /// </param>
    /// <param name="lpWindowName">
    /// [Type: LPCTSTR]
    /// The window name (the window's title). If this parameter is NULL, all window names match.
    /// </param>
    /// <returns>
    /// [Type: HWND]
    /// If the function succeeds, the return value is a handle to the window that has the specified class name and window
    /// name.
    /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// If the lpWindowName parameter is not NULL, FindWindow calls the GetWindowText function to retrieve the window name
    /// for comparison. For a description of a potential problem that can arise, see the Remarks for GetWindowText.
    /// </remarks>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint FindWindow(string lpClassName, string lpWindowName);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ShowWindow(nint hwnd, ShowWindowCommands nCmdShow);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool UpdateWindow(nint hwnd);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint CreateWindowEx(
        WindowExStyles dwExStyle,
        string lpClassName,
        string lpWindowName,
        WindowStyles dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        nint hwndParent,
        nint hMenu,
        nint hInstance,
        nint lpParam);


    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetWindowRect(nint hwnd, out Rect lpRect);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetClientRect(nint hwnd, out Rect lpRect);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint FindWindowEx(nint hwndParent, nint hwndChildAfter, string lpszClass,
        string lpszWindow);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetForegroundWindow();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetTopWindow();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetNextWindow(nint hwnd, uint wCmd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetWindow(nint hwnd, uint wCmd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AllowSetForegroundWindow(uint dwProcessId);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetForegroundWindow(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool BringWindowToTop(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetParent(nint hWndChild, nint hWndNewParent);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetWindowPos(nint hwnd, nint hWndInsertAfter, int x, int y, int cx, int cy,
        WindowPositionFlags flags);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(nint hWnd);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool SetWindowText(nint hwnd, string lpString);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, nint processId);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool MoveWindow(nint hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetWindowInfo(nint hwnd, [In] [Out] ref WindowInfo pwi);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool SetWindowPlacement(nint hWnd,
        [In] ref WindowPlacement lpwndpl);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetWindowPlacement(nint hWnd, out WindowPlacement lpwndpl);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool RedrawWindow(nint hWnd, [In] ref Rect lprcUpdate, nint hrgnUpdate,
        RedrawWindowFlags flags);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool DestroyWindow(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool CloseWindow(nint hwnd);

    #endregion

    #region Window Class Functions

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassEx([In] ref WindowClassEx lpwcx);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern ushort RegisterClassEx([In] ref WindowClassExBlittable lpwcx);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool UnregisterClass(string lpClassName, nint hInstance);

    // This static method is required because Win32 does not support
    // GetWindowLongPtr directly
    public static nint GetWindowLongPtr(nint hwnd, WindowLongFlags nIndex)
    {
        return nint.Size > 4
            ? GetWindowLongPtr_x64(hwnd, nIndex)
            : new nint(GetWindowLong(hwnd, nIndex));
    }

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    private static extern int GetWindowLong(nint hwnd, WindowLongFlags nIndex);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode, EntryPoint = "GetWindowLongPtr")]
    private static extern nint GetWindowLongPtr_x64(nint hwnd, WindowLongFlags nIndex);

    // This static method is required because Win32 does not support
    // GetWindowLongPtr directly
    public static nint SetWindowLongPtr(nint hwnd, WindowLongFlags nIndex, nint dwNewLong)
    {
        return nint.Size > 4
            ? SetWindowLongPtr_x64(hwnd, nIndex, dwNewLong)
            : new nint(SetWindowLong(hwnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    private static extern int SetWindowLong(nint hwnd, WindowLongFlags nIndex, int dwNewLong);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode, EntryPoint = "SetWindowLongPtr")]
    private static extern nint SetWindowLongPtr_x64(nint hwnd, WindowLongFlags nIndex, nint dwNewLong);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool GetClassInfoEx(nint hInstance, string lpClassName,
        out WindowClassExBlittable lpWndClass);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);

    public static nint GetClassLongPtr(nint hwnd, int nIndex)
    {
        return nint.Size > 4
            ? GetClassLongPtr_x64(hwnd, nIndex)
            : new nint(unchecked((int) GetClassLong(hwnd, nIndex)));
    }

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    private static extern uint GetClassLong(nint hWnd, int nIndex);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode, EntryPoint = "GetClassLongPtr")]
    private static extern nint GetClassLongPtr_x64(nint hWnd, int nIndex);


    public static nint SetClassLongPtr(nint hWnd, int nIndex, nint dwNewLong)
    {
        return nint.Size > 4
            ? SetClassLongPtr_x64(hWnd, nIndex, dwNewLong)
            : new nint(unchecked((int) SetClassLong(hWnd, nIndex, dwNewLong.ToInt32())));
    }

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    private static extern uint SetClassLong(nint hWnd, int nIndex, int dwNewLong);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode, EntryPoint = "SetClassLongPtr")]
    private static extern nint SetClassLongPtr_x64(nint hWnd, int nIndex, nint dwNewLong);

    #endregion

    #region Window Procedure Functions

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint DefWindowProc(nint hwnd, WM uMsg, nint wParam, nint lParam);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint CallWindowProc(WindowProc lpPrevWndFunc, nint hWnd, uint uMsg, nint wParam,
        nint lParam);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint CallWindowProc(nint lpPrevWndFunc, nint hWnd, uint uMsg, nint wParam,
        nint lParam);

    #endregion

    #region Message Functions

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool PeekMessage(out Message lpMsg, nint hWnd, uint wMsgFilterMin,
        uint wMsgFilterMax, PeekMessageFlags wRemoveMsg);

    /// <summary>
    /// Dispatches a message to a window procedure. It is typically used to dispatch a message retrieved by the GetMessage
    /// function.
    /// </summary>
    /// <param name="lpMsg">
    /// [Type: const MSG*]
    /// A pointer to a structure that contains the message.
    /// </param>
    /// <returns>
    /// [Type: LRESULT]
    /// The return value specifies the value returned by the window procedure.Although its meaning depends on the message
    /// being dispatched, the return value generally is ignored.
    /// </returns>
    /// <remarks>
    /// The MSG structure must contain valid message values. If the lpmsg parameter points to a WM_TIMER message and the
    /// lParam parameter of the WM_TIMER message is not NULL, lParam points to a function that is called instead of the
    /// window procedure.
    /// Note that the application is responsible for retrieving and dispatching input messages to the dialog box.Most
    /// applications use the main message loop for this. However, to permit the user to move to and to select controls by
    /// using the keyboard, the application must call IsDialogMessage.For more information, see Dialog Box Keyboard
    /// Interface.
    /// </remarks>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint DispatchMessage([In] ref Message lpMsg);

    /// <summary>
    /// Translates virtual-key messages into character messages. The character messages are posted to the calling thread's
    /// message queue, to be read the next time the thread calls the GetMessage or PeekMessage function.
    /// </summary>
    /// <param name="lpMsg">
    /// [Type: const MSG*]
    /// A pointer to an MSG structure that contains message information retrieved from the calling thread's message queue
    /// by using the GetMessage or PeekMessage function.
    /// </param>
    /// <returns>
    /// [Type: BOOL]
    /// If the message is translated(that is, a character message is posted to the thread's message queue), the return
    /// value is nonzero. If the message is WM_KEYDOWN, WM_KEYUP, WM_SYSKEYDOWN, or WM_SYSKEYUP, the return value is
    /// nonzero, regardless of the translation. If the message is not translated (that is, a character message is not
    /// posted to the thread's message queue), the return value is zero.
    /// </returns>
    /// <remarks>
    /// The TranslateMessage function does not modify the message pointed to by the lpMsg parameter.
    /// WM_KEYDOWN and WM_KEYUP combinations produce a WM_CHAR or WM_DEADCHAR message.WM_SYSKEYDOWN and WM_SYSKEYUP
    /// combinations produce a WM_SYSCHAR or WM_SYSDEADCHAR message.
    /// TranslateMessage produces WM_CHAR messages only for keys that are mapped to ASCII characters by the keyboard
    /// driver.
    /// If applications process virtual-key messages for some other purpose, they should not call TranslateMessage.For
    /// instance, an application should not call TranslateMessage if the TranslateAccelerator function returns a nonzero
    /// value.Note that the application is responsible for retrieving and dispatching input messages to the dialog box.Most
    /// applications use the main message loop for this. However, to permit the user to move to and to select controls by
    /// using the keyboard, the application must call IsDialogMessage.For more information, see Dialog Box Keyboard
    /// Interface.
    /// </remarks>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool TranslateMessage([In] ref Message lpMsg);

    /// <summary>
    /// Retrieves a message from the calling thread's message queue. The function dispatches incoming sent messages until a
    /// posted message is available for retrieval. Unlike GetMessage, the PeekMessage function does not wait for a message
    /// to be posted before returning.
    /// </summary>
    /// <param name="lpMsg">
    /// [Type: LPMSG]
    /// A pointer to an MSG structure that receives message information from the thread's message queue.
    /// </param>
    /// <param name="hwnd">
    /// [Type: HWND]
    /// A handle to the window whose messages are to be retrieved.The window must belong to the current thread.
    /// If hWnd is NULL, GetMessage retrieves messages for any window that belongs to the current thread, and any messages
    /// on the current thread's message queue whose hwnd value is NULL (see the MSG structure). Therefore if hWnd is NULL,
    /// both window messages and thread messages are processed.
    /// If hWnd is -1, GetMessage retrieves only messages on the current thread's message queue whose hwnd value is NULL,
    /// that is, thread messages as posted by PostMessage (when the hWnd parameter is NULL) or PostThreadMessage.
    /// </param>
    /// <param name="wMsgFilterMin">
    /// [Type: UINT] The integer value of the lowest message value to be retrieved.Use WM_KEYFIRST (0x0100) to specify the
    /// first keyboard message or WM_MOUSEFIRST(0x0200) to specify the first mouse message.
    /// Use WM_INPUT here and in wMsgFilterMax to specify only the WM_INPUT messages.
    /// If wMsgFilterMin and wMsgFilterMax are both zero, GetMessage returns all available messages (that is, no range
    /// filtering is performed).
    /// </param>
    /// <param name="wMsgFilterMax">
    /// [Type: UINT]
    /// The integer value of the highest message value to be retrieved.Use WM_KEYLAST to specify the last keyboard message
    /// or WM_MOUSELAST to specify the last mouse message.
    /// Use WM_INPUT here and in wMsgFilterMin to specify only the WM_INPUT messages.
    /// If wMsgFilterMin and wMsgFilterMax are both zero, GetMessage returns all available messages (that is, no range
    /// filtering is performed).
    /// </param>
    /// <returns>
    /// [Type: BOOL]
    /// If the function retrieves a message other than WM_QUIT, the return value is nonzero.
    /// If the function retrieves the WM_QUIT message, the return value is zero.
    /// If there is an error, the return value is -1. For example, the function fails if hWnd is an invalid window handle
    /// or lpMsg is an invalid pointer.To get extended error information, call GetLastError.
    /// </returns>
    /// <returns>
    /// An application typically uses the return value to determine whether to end the main message loop and exit the
    /// program.
    /// The GetMessage function retrieves messages associated with the window identified by the hWnd parameter or any of
    /// its children, as specified by the IsChild function, and within the range of message values given by the
    /// wMsgFilterMin and wMsgFilterMax parameters. Note that an application can only use the low word in the wMsgFilterMin
    /// and wMsgFilterMax parameters; the high word is reserved for the system.
    /// Note that GetMessage always retrieves WM_QUIT messages, no matter which values you specify for wMsgFilterMin and
    /// wMsgFilterMax.
    /// During this call, the system delivers pending, nonqueued messages, that is, messages sent to windows owned by the
    /// calling thread using the SendMessage, SendMessageCallback, SendMessageTimeout, or SendNotifyMessage function. Then
    /// the first queued message that matches the specified filter is retrieved. The system may also process internal
    /// events. If no filter is specified, messages are processed in the following order:
    /// Sent messages
    /// Posted messages
    /// Input (hardware) messages and system internal events
    /// Sent messages (again)
    /// WM_PAINT messages
    /// WM_TIMER messages
    /// To retrieve input messages before posted messages, use the wMsgFilterMin and wMsgFilterMax parameters.
    /// GetMessage does not remove WM_PAINT messages from the queue. The messages remain in the queue until processed.
    /// If a top-level window stops responding to messages for more than several seconds, the system considers the window
    /// to be not responding and replaces it with a ghost window that has the same z-order, location, size, and visual
    /// attributes. This allows the user to move it, resize it, or even close the application. However, these are the only
    /// actions available because the application is actually not responding. When in the debugger mode, the system does
    /// not generate a ghost window.
    /// </returns>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern int GetMessage(out Message lpMsg, nint hwnd, uint wMsgFilterMin,
        uint wMsgFilterMax);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern void PostQuitMessage(int nExitCode);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetMessageExtraInfo();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetMessageExtraInfo(nint lParam);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool WaitMessage();

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern nint SendMessage(nint hwnd, uint msg, nint wParam, nint lParam);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool SendNotifyMessage(nint hwnd, uint msg, nint wParam, nint lParam);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ReplyMessage(nint lResult);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool GetInputState();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint GetMessagePos();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint GetMessageTime();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool InSendMessage();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint GetQueueStatus(QueueStatusFlags flags);

    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool PostThreadMessage(uint threadId, uint msg, nint wParam, nint lParam);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changewindowmessagefilterex
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool ChangeWindowMessageFilterEx(nint hWnd, WM msg, ChangeWindowMessageFilterExAction action, ref ChangeFilter changeInfo);

    [DllImport(LIBRARY_NAME)]
    public static extern bool ChangeWindowMessageFilterEx(nint hWnd, WM msg, ChangeWindowMessageFilterExAction action, nint changeInfo);

    #endregion

    #region Clipboard Functions

    /// <summary>
    /// Opens the clipboard for examination and prevents other applications from modifying the clipboard content.
    /// </summary>
    /// <param name="hWndNewOwner">
    /// A handle to the window to be associated with the open clipboard. If this parameter is NULL,
    /// the open clipboard is associated with the current task.
    /// </param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool OpenClipboard(nint hWndNewOwner);

    /// <summary>
    /// Closes the clipboard.
    /// </summary>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool CloseClipboard();

    /// <summary>
    /// Retrieves data from the clipboard in a specified format. The clipboard must have been opened previously.
    /// </summary>
    /// <param name="uFormat">A clipboard format.</param>
    /// <returns>If the function succeeds, the return value is the handle to a clipboard object in the specified format.</returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint GetClipboardData(uint uFormat);

    /// <summary>
    /// Places data on the clipboard in a specified clipboard format. The window must be the current clipboard owner, and the
    /// application must have called the OpenClipboard function
    /// </summary>
    /// <param name="uFormat">The clipboard format</param>
    /// <param name="handle">A handle to the data in the specified format</param>
    /// <returns>If the function succeeds, the return value is the handle to the data.</returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetClipboardData(uint uFormat, nint handle);

    /// <summary>
    /// Empties the clipboard and frees handles to data in the clipboard. The function then assigns ownership of the clipboard
    /// to the window that currently has the clipboard open.
    /// </summary>
    /// <returns></returns>
    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool EmptyClipboard();

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern nint SetClipboardViewer(nint hWndNewViewer);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool ChangeClipboardChain(nint hWndRemove, nint hWndNewNext);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern bool AddClipboardFormatListener(nint hwnd);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern int GetPriorityClipboardFormat(nint paFormatPriorityList, int cFormats);

    [DllImport(LIBRARY_NAME, ExactSpelling = true)]
    public static extern uint EnumClipboardFormats(uint format);

    #endregion

    #region Display

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-enumdisplaydevicesw
    /// </summary>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplayDevicesW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpDevice,
        uint iDevNum,
        ref DisplayDeviceW lpDisplayDevice,
        uint dwFlags
    );

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-enumdisplaydevicesw
    /// </summary>
    [DllImport(LIBRARY_NAME, CharSet = CharSet.Unicode)]
    public static extern bool EnumDisplaySettingsW(
        string lpszDeviceName,
        int iModeNum,
        ref DeviceMode lpDevMode
    );

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-enumdisplaymonitors
    /// </summary>
    [DllImport(LIBRARY_NAME)]
    public static extern bool EnumDisplayMonitors(
        nint hdc,
        ref Rect a,
        MonitorEnumProc lpfnEnum,
        object dwData
    );

    [DllImport(LIBRARY_NAME)]
    public static extern int ChangeDisplaySettingsExW([MarshalAs(UnmanagedType.LPWStr)] string lpszDeviceName, ref DeviceMode lpDevMode, nint hwnd, int dwflags, object lParam);

    #endregion
}