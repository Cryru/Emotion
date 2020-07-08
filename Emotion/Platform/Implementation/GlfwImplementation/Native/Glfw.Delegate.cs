// ReSharper disable once CheckNamespace
namespace Emotion.Platform.Implementation.GlfwImplementation.Native
{
    using System.Runtime.InteropServices;

    public static partial class Glfw
    {
        /// <summary>
        /// The function signature for Unicode character callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="codepoint">The Unicode code point of the character.</param>
        /// <seealso cref="SetCharCallback(Window, CharFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CharFunc([MarshalAs(UnmanagedType.Struct)] Window window, uint codepoint);

        /// <summary>
        /// The function signature for Unicode character with modifiers callbacks. It is called for
        /// each input character, regardless of what modifier keys are held down.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="codepoint">The Unicode code point of the character.</param>
        /// <param name="mods">Bit field describing which <see cref="KeyMods"/> were held
        /// down.</param>
        /// <seealso cref="SetCharModsCallback(Window, CharModsFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CharModsFunc([MarshalAs(UnmanagedType.Struct)] Window window, uint codepoint, KeyMods mods);

        /// <summary>
        /// The function signature for cursor enter/leave callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="entered"><c>true</c> if the cursor entered the window's client area, or
        /// <c>false</c> if it left it.</param>
        /// <seealso cref="SetCursorEnterCallback(Window, CursorEnterFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CursorEnterFunc([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Bool)] bool entered);

        /// <summary>
        /// The function signature for cursor position callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="xpos">The new cursor x-coordinate, relative to the left edge of the client
        /// area.</param>
        /// <param name="ypos">The new cursor y-coordinate, relative to the top edge of the client
        /// area.</param>
        /// <seealso cref="SetCursorPosCallback(Window, CursorPosFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CursorPosFunc([MarshalAs(UnmanagedType.Struct)] Window window, double xpos, double ypos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="count"></param>
        /// <param name="paths"></param>
        /// <seealso cref="SetDropCallback(Window, DropFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DropFunc([MarshalAs(UnmanagedType.Struct)] Window window, int count, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 1)] string[] paths);

        /// <summary>
        /// The function signature for error callbacks.
        /// </summary>
        /// <param name="error">An <see cref="ErrorCode"/>.</param>
        /// <param name="description">A UTF-8 encoded string describing the error.</param>
        /// <seealso cref="SetErrorCallback(ErrorFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void ErrorFunc(ErrorCode error, string description);

        /// <summary>
        /// The function signature for framebuffer resize callbacks.
        /// </summary>
        /// <param name="window">The window whose framebuffer was resized.</param>
        /// <param name="width">The new width, in pixels, of the framebuffer.</param>
        /// <param name="height">The new height, in pixels, of the framebuffer.</param>
        /// <seealso cref="SetFramebufferSizeCallback(Window, FramebufferSizeFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void FramebufferSizeFunc([MarshalAs(UnmanagedType.Struct)] Window window, int width, int height);

        /// <summary>
        /// The function signature for keyboard key callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="key">The <see cref="KeyCode"/> that was pressed or released.</param>
        /// <param name="scancode">The system-specific scancode of the key.</param>
        /// <param name="state">One of <see cref="InputState"/>.</param>
        /// <param name="mods">Bit field describing which <see cref="KeyMods"/> were held
        /// down.</param>
        /// <seealso cref="SetKeyCallback(Window, KeyFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void KeyFunc([MarshalAs(UnmanagedType.Struct)] Window window, KeyCode key, int scancode, InputState state, KeyMods mods);

        /// <summary>
        /// The function signature for joystick configuration callbacks.
        /// </summary>
        /// <param name="joy">The joystick that was connected or disconnected.</param>
        /// <param name="evt">One of <see cref="ConnectionEvent"/>.</param>
        /// <seealso cref="SetJoystickCallback(JoystickFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void JoystickFunc(Joystick joy, ConnectionEvent evt);

        /// <summary>
        /// The function signature for monitor configuration callbacks.
        /// </summary>
        /// <param name="monitor">The monitor that was connected or disconnected.</param>
        /// <param name="evt">One of <see cref="ConnectionEvent"/>.</param>
        /// <seealso cref="SetMonitorCallback(MonitorFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MonitorFunc([MarshalAs(UnmanagedType.Struct)] Monitor monitor, ConnectionEvent evt);

        /// <summary>
        /// The function signature for mouse button callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="button">The <see cref="MouseButton"/> that was pressed or released.</param>
        /// <param name="state">One of <see cref="InputState.Press"/> or
        /// <see cref="InputState.Release"/></param>
        /// <param name="mods">Bit field describing which <see cref="KeyMods"/> were held
        /// down.</param>
        /// <seealso cref="SetMouseButtonCallback(Window, MouseButtonFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MouseButtonFunc([MarshalAs(UnmanagedType.Struct)] Window window, MouseButton button, InputState state, KeyMods mods);

        /// <summary>
        /// The function signature for scroll callbacks.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="xoffset">The scroll offset along the x-axis.</param>
        /// <param name="yoffset">The scroll offset along the y-axis.</param>
        /// <seealso cref="SetScrollCallback(Window, CursorPosFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ScrollFunc([MarshalAs(UnmanagedType.Struct)] Window window, double xoffset, double yoffset);

        /// <summary>
        /// The function signature for window close callbacks.
        /// </summary>
        /// <param name="window">The window that the user attempted to close.</param>
        /// <seealso cref="SetWindowCloseCallback(Window, WindowCloseFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowCloseFunc([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// The function signature for window focus/defocus callbacks.
        /// </summary>
        /// <param name="window">The window that gained or lost input focus.</param>
        /// <param name="focused"><c>true</c> if the window was given input focus, or <c>false</c>
        /// if it lost it.</param>
        /// <seealso cref="SetWindowFocusCallback(Window, WindowFocusFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowFocusFunc([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Bool)] bool focused);

        /// <summary>
        /// The function signature for window iconify/restore callbacks.
        /// </summary>
        /// <param name="window">The window that was iconified or restored.</param>
        /// <param name="focused"><c>true</c> if the window was iconified, or <c>false</c> if it was
        /// restored.</param>
        /// <seealso cref="SetWindowIconifyCallback(Window, WindowIconifyFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowIconifyFunc([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Bool)] bool focused);

        /// <summary>
        /// The function signature for window position callbacks.
        /// </summary>
        /// <param name="window">The window that was moved.</param>
        /// <param name="xpos">The new x-coordinate, in screen coordinates, of the upper-left corner
        /// of the client area of the window.</param>
        /// <param name="ypos">The new y-coordinate, in screen coordinates, of the upper-left corner
        /// of the client area of the window.</param>
        /// <seealso cref="SetWindowPosCallback(Window, WindowPosFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowPosFunc([MarshalAs(UnmanagedType.Struct)] Window window, int xpos, int ypos);

        /// <summary>
        /// The function signature for window content refresh callbacks.
        /// </summary>
        /// <param name="window">The window whose content needs to be refreshed.</param>
        /// <seealso cref="SetWindowRefreshCallback(Window, WindowRefreshFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowRefreshFunc([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// The function signature for window resize callbacks.
        /// </summary>
        /// <param name="window">The window that was resized.</param>
        /// <param name="width">The new width, in screen coordinates, of the window.</param>
        /// <param name="height">The new height, in screen coordinates, of the window.</param>
        /// <seealso cref="SetWindowSizeCallback(Window, WindowSizeFunc)"/>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WindowSizeFunc([MarshalAs(UnmanagedType.Struct)] Window window, int width, int height);
    }
}
