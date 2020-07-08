#region Using

using Emotion.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using WinApi.Kernel32;

#endregion

// ReSharper disable once CheckNamespace
namespace Emotion.GLFW
{
    public static class Glfw
    {
        #region Vulkan

        public const int VkSuccess = 0;
        public const int VkNotReady = 1;
        public const int VkTimeout = 2;
        public const int VkEventSet = 3;
        public const int VkEventReset = 4;
        public const int VkIncomplete = 5;
        public const int VkErrorOutOfHostMemory = -1;
        public const int VkErrorOutOfDeviceMemory = -2;
        public const int VkErrorInitializationFailed = -3;
        public const int VkErrorDeviceLost = -4;
        public const int VkErrorMemoryMapFailed = -5;
        public const int VkErrorLayerNotPresent = -6;
        public const int VkErrorExtensionNotPresent = -7;
        public const int VkErrorFeatureNotPresent = -8;
        public const int VkErrorIncompatibleDriver = -9;
        public const int VkErrorTooManyObjects = -10;
        public const int VkErrorFormatNotSupported = -11;
        public const int VkErrorFragmentedPool = -12;
        public const int VkErrorSurfaceLostKhr = -1000000000;
        public const int VkErrorNativeWindowInUseKhr = -1000000001;
        public const int VkSuboptimalKhr = 1000001003;
        public const int VkErrorOutOfDateKhr = -1000001004;
        public const int VkErrorIncompatibleDisplayKhr = -1000003001;
        public const int VkErrorValidationFailedExt = -1000011001;
        public const int VkErrorInvalidShaderNv = -1000012000;
        public const int VkErrorOutOfPoolMemoryKhr = -1000069000;
        public const int VkResultBeginRange = -12;
        public const int VkResultEndRange = 5;
        public const int VkResultRangeSize = 18;
        public const int VkResultMaxEnum = 2147483647;

        #endregion

        #region Key

        public const int KeyUnknown = -1;
        public const int KeySpace = 32;
        public const int KeyApostrophe = 39;
        public const int KeyComma = 44;
        public const int KeyMinus = 45;
        public const int KeyPeriod = 46;
        public const int KeySlash = 47;
        public const int Key0 = 48;
        public const int Key1 = 49;
        public const int Key2 = 50;
        public const int Key3 = 51;
        public const int Key4 = 52;
        public const int Key5 = 53;
        public const int Key6 = 54;
        public const int Key7 = 55;
        public const int Key8 = 56;
        public const int Key9 = 57;
        public const int KeySemicolon = 59;
        public const int KeyEqual = 61;
        public const int KeyA = 65;
        public const int KeyB = 66;
        public const int KeyC = 67;
        public const int KeyD = 68;
        public const int KeyE = 69;
        public const int KeyF = 70;
        public const int KeyG = 71;
        public const int KeyH = 72;
        public const int KeyI = 73;
        public const int KeyJ = 74;
        public const int KeyK = 75;
        public const int KeyL = 76;
        public const int KeyM = 77;
        public const int KeyN = 78;
        public const int KeyO = 79;
        public const int KeyP = 80;
        public const int KeyQ = 81;
        public const int KeyR = 82;
        public const int KeyS = 83;
        public const int KeyT = 84;
        public const int KeyU = 85;
        public const int KeyV = 86;
        public const int KeyW = 87;
        public const int KeyX = 88;
        public const int KeyY = 89;
        public const int KeyZ = 90;
        public const int KeyLeftBracket = 91;
        public const int KeyBackslash = 92;
        public const int KeyRightBracket = 93;
        public const int KeyGraveAccent = 96;
        public const int KeyWorld1 = 161;
        public const int KeyWorld2 = 162;
        public const int KeyEscape = 256;
        public const int KeyEnter = 257;
        public const int KeyTab = 258;
        public const int KeyBackspace = 259;
        public const int KeyInsert = 260;
        public const int KeyDelete = 261;
        public const int KeyRight = 262;
        public const int KeyLeft = 263;
        public const int KeyDown = 264;
        public const int KeyUp = 265;
        public const int KeyPageUp = 266;
        public const int KeyPageDown = 267;
        public const int KeyHome = 268;
        public const int KeyEnd = 269;
        public const int KeyCapsLock = 280;
        public const int KeyScrollLock = 281;
        public const int KeyNumLock = 282;
        public const int KeyPrintScreen = 283;
        public const int KeyPause = 284;
        public const int KeyF1 = 290;
        public const int KeyF2 = 291;
        public const int KeyF3 = 292;
        public const int KeyF4 = 293;
        public const int KeyF5 = 294;
        public const int KeyF6 = 295;
        public const int KeyF7 = 296;
        public const int KeyF8 = 297;
        public const int KeyF9 = 298;
        public const int KeyF10 = 299;
        public const int KeyF11 = 300;
        public const int KeyF12 = 301;
        public const int KeyF13 = 302;
        public const int KeyF14 = 303;
        public const int KeyF15 = 304;
        public const int KeyF16 = 305;
        public const int KeyF17 = 306;
        public const int KeyF18 = 307;
        public const int KeyF19 = 308;
        public const int KeyF20 = 309;
        public const int KeyF21 = 310;
        public const int KeyF22 = 311;
        public const int KeyF23 = 312;
        public const int KeyF24 = 313;
        public const int KeyF25 = 314;
        public const int KeyKp0 = 320;
        public const int KeyKp1 = 321;
        public const int KeyKp2 = 322;
        public const int KeyKp3 = 323;
        public const int KeyKp4 = 324;
        public const int KeyKp5 = 325;
        public const int KeyKp6 = 326;
        public const int KeyKp7 = 327;
        public const int KeyKp8 = 328;
        public const int KeyKp9 = 329;
        public const int KeyKpDecimal = 330;
        public const int KeyKpDivide = 331;
        public const int KeyKpMultiply = 332;
        public const int KeyKpSubtract = 333;
        public const int KeyKpAdd = 334;
        public const int KeyKpEnter = 335;
        public const int KeyKpEqual = 336;
        public const int KeyLeftShift = 340;
        public const int KeyLeftControl = 341;
        public const int KeyLeftAlt = 342;
        public const int KeyLeftSuper = 343;
        public const int KeyRightShift = 344;
        public const int KeyRightControl = 345;
        public const int KeyRightAlt = 346;
        public const int KeyRightSuper = 347;
        public const int KeyMenu = 348;
        public const int KeyLast = 348;

        #endregion

        #region Mouse

        public const int Mouse1 = 0;
        public const int Mouse2 = 1;
        public const int Mouse3 = 2;
        public const int Mouse4 = 3;
        public const int Mouse5 = 4;
        public const int Mouse6 = 5;
        public const int Mouse7 = 6;
        public const int Mouse8 = 7;
        public const int MouseLast = Mouse8;
        public const int Left = Mouse1;
        public const int Right = Mouse2;
        public const int Middle = Mouse3;

        #endregion

        #region Joystick

        public const int Joystick1 = 0;
        public const int Joystick2 = 1;
        public const int Joystick3 = 2;
        public const int Joystick4 = 3;
        public const int Joystick5 = 4;
        public const int Joystick6 = 5;
        public const int Joystick7 = 6;
        public const int Joystick8 = 7;
        public const int Joystick9 = 8;
        public const int Joystick10 = 9;
        public const int Joystick11 = 10;
        public const int Joystick12 = 11;
        public const int Joystick13 = 12;
        public const int Joystick14 = 13;
        public const int Joystick15 = 14;
        public const int Joystick16 = 15;
        public const int JoystickLast = Joystick16;

        #endregion

        #region Error

        public const int ErrorNotInitialized = 65537;
        public const int ErrorNoCurrentContext = 65538;
        public const int ErrorInvalidEnum = 65539;
        public const int ErrorInvalidValue = 65540;
        public const int ErrorOutOfMemory = 65541;
        public const int ErrorApiUnavailable = 65542;
        public const int ErrorVersionUnavailable = 65543;
        public const int ErrorPlatformError = 65544;
        public const int ErrorFormatUnavailable = 65545;
        public const int ErrorNoWindowContext = 65546;

        #endregion

        #region Key Mods

        public const int ModShift = 1;
        public const int ModControl = 2;
        public const int ModAlt = 4;
        public const int ModSuper = 8;

        #endregion

        #region State

        public const int True = 1;
        public const int False = 0;
        public const int Release = 0;
        public const int Press = 1;
        public const int Repeat = 2;
        public const int Focused = 131073;
        public const int Iconified = 131074;
        public const int Resizable = 131075;
        public const int Visible = 131076;
        public const int Decorated = 131077;
        public const int AutoIconify = 131078;
        public const int Floating = 131079;
        public const int Maximized = 131080;
        public const int RedBits = 135169;
        public const int GreenBits = 135170;
        public const int BlueBits = 135171;
        public const int AlphaBits = 135172;
        public const int DepthBits = 135173;
        public const int StencilBits = 135174;
        public const int AccumRedBits = 135175;
        public const int AccumGreenBits = 135176;
        public const int AccumBlueBits = 135177;
        public const int AccumAlphaBits = 135178;
        public const int AuxBuffers = 135179;
        public const int Stereo = 135180;
        public const int Samples = 135181;
        public const int SrgbCapable = 135182;
        public const int RefreshRate = 135183;
        public const int Doublebuffer = 135184;
        public const int ClientApi = 139265;
        public const int ContextVersionMajor = 139266;
        public const int ContextVersionMinor = 139267;
        public const int ContextRevision = 139268;
        public const int ContextRobustness = 139269;
        public const int OpenglForwardCompat = 139270;
        public const int OpenglDebugContext = 139271;
        public const int OpenglProfile = 139272;
        public const int ContextReleaseBehavior = 139273;
        public const int ContextNoError = 139274;
        public const int ContextCreationApi = 139275;
        public const int NoApi = 0;
        public const int OpenglApi = 196609;
        public const int OpenglEsApi = 196610;
        public const int NoRobustness = 0;
        public const int NoResetNotification = 200705;
        public const int LoseContextOnReset = 200706;
        public const int OpenglAnyProfile = 0;
        public const int OpenglCoreProfile = 204801;
        public const int OpenglCompatProfile = 204802;
        public const int Cursor = 208897;
        public const int StickyKeys = 208898;
        public const int StickyMouseButtons = 208899;
        public const int CursorNormal = 212993;
        public const int CursorHidden = 212994;
        public const int CursorDisabled = 212995;
        public const int AnyReleaseBehavior = 0;
        public const int ReleaseBehaviorFlush = 217089;
        public const int ReleaseBehaviorNone = 217090;
        public const int NativeContextApi = 221185;
        public const int EglContextApi = 221186;
        public const int Connected = 262145;
        public const int Disconnected = 262146;
        public const int DontCare = -1;

        #endregion

        #region Version

        public const int VersionMajor = 3;
        public const int VersionMinor = 2;
        public const int VersionRevision = 1;

        #endregion

        #region Delegates

        /// <summary>  Opaque monitor object.</summary>
        /// <summary>  Opaque window object.</summary>
        /// <summary>  Opaque cursor object.</summary>
        /// <summary>  This is the function signature for error callback functions.</summary>
        /// <param name="error">An [error code](</param>
        /// <param name="description">A UTF-8 encoded string describing the error.</param>
        public delegate void ErrorFun(int error, string description);

        /// <summary>  This is the function signature for window position callback functions.</summary>
        /// <param name="window">The window that was moved.</param>
        /// <param name="xpos">
        /// The new x-coordinate, in screen coordinates, of the
        /// upper-left corner of the client area of the window.
        /// </param>
        /// <param name="ypos">
        /// The new y-coordinate, in screen coordinates, of the
        /// upper-left corner of the client area of the window.
        /// </param>
        public delegate void WindowPosFun(IntPtr window, int xpos, int ypos);

        /// <summary>  This is the function signature for window size callback functions.</summary>
        /// <param name="window">The window that was resized.</param>
        /// <param name="width">The new width, in screen coordinates, of the window.</param>
        /// <param name="height">The new height, in screen coordinates, of the window.</param>
        public delegate void WindowSizeFun(IntPtr window, int width, int height);

        /// <summary>  This is the function signature for window close callback functions.</summary>
        /// <param name="window">The window that the user attempted to close.</param>
        public delegate void WindowCloseFun(IntPtr window);

        /// <summary>  This is the function signature for window refresh callback functions.</summary>
        /// <param name="window">The window whose content needs to be refreshed.</param>
        public delegate void WindowRefreshFun(IntPtr window);

        /// <summary>  This is the function signature for window focus callback functions.</summary>
        /// <param name="window">The window that gained or lost input focus.</param>
        /// <param name="focused">
        /// `GLFW_TRUE` if the window was given input focus, or
        /// `GLFW_FALSE` if it lost it.
        /// </param>
        public delegate void WindowFocusFun(IntPtr window, int focused);

        /// <summary>
        /// This is the function signature for window iconify/restore callback
        /// functions.
        /// </summary>
        /// <param name="window">The window that was iconified or restored.</param>
        /// <param name="iconified">
        /// `GLFW_TRUE` if the window was iconified, or
        /// `GLFW_FALSE` if it was restored.
        /// </param>
        public delegate void WindowIconifyFun(IntPtr window, int iconified);

        /// <summary>
        /// This is the function signature for framebuffer resize callback
        /// functions.
        /// </summary>
        /// <param name="window">The window whose framebuffer was resized.</param>
        /// <param name="width">The new width, in pixels, of the framebuffer.</param>
        /// <param name="height">The new height, in pixels, of the framebuffer.</param>
        public delegate void FramebufferSizeFun(IntPtr window, int width, int height);

        /// <summary>  This is the function signature for mouse button callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="button">
        /// The [mouse button](
        /// released.
        /// </param>
        /// <param name="action">One of `GLFW_PRESS` or `GLFW_RELEASE`.</param>
        /// <param name="mods">
        /// Bit field describing which [modifier keys](
        /// held down.
        /// </param>
        public delegate void MouseButtonFun(IntPtr window, int button, int action, int mods);

        /// <summary>  This is the function signature for cursor position callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="xpos">
        /// The new cursor x-coordinate, relative to the left edge of
        /// the client area.
        /// </param>
        /// <param name="ypos">
        /// The new cursor y-coordinate, relative to the top edge of the
        /// client area.
        /// </param>
        public delegate void CursorPosFun(IntPtr window, double xpos, double ypos);

        /// <summary>  This is the function signature for cursor enter/leave callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="entered">
        /// `GLFW_TRUE` if the cursor entered the window's client
        /// area, or `GLFW_FALSE` if it left it.
        /// </param>
        public delegate void CursorEnterFun(IntPtr window, int entered);

        /// <summary>  This is the function signature for scroll callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="xoffset">The scroll offset along the x-axis.</param>
        /// <param name="yoffset">The scroll offset along the y-axis.</param>
        public delegate void ScrollFun(IntPtr window, double xoffset, double yoffset);

        /// <summary>  This is the function signature for keyboard key callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="key">The [keyboard key](</param>
        /// <param name="scancode">The system-specific scancode of the key.</param>
        /// <param name="action">`GLFW_PRESS`, `GLFW_RELEASE` or `GLFW_REPEAT`.</param>
        /// <param name="mods">
        /// Bit field describing which [modifier keys](
        /// held down.
        /// </param>
        public delegate void KeyFun(IntPtr window, int key, int scancode, int action, int mods);

        /// <summary>  This is the function signature for Unicode character callback functions.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="codepoint">The Unicode code point of the character.</param>
        public delegate void CharFun(IntPtr window, uint codepoint);

        /// <summary>
        /// This is the function signature for Unicode character with modifiers callback
        /// functions.  It is called for each input character, regardless of what
        /// modifier keys are held down.
        /// </summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="codepoint">The Unicode code point of the character.</param>
        /// <param name="mods">
        /// Bit field describing which [modifier keys](
        /// held down.
        /// </param>
        public delegate void CharModsFun(IntPtr window, uint codepoint, int mods);

        private unsafe delegate void UnmanagedDropFun(IntPtr window, int count, sbyte** paths);

        /// <summary>  This is the function signature for file drop callbacks.</summary>
        /// <param name="window">The window that received the event.</param>
        /// <param name="count">The number of dropped files.</param>
        /// <param name="paths">The UTF-8 encoded file and/or directory path names.</param>
        public delegate void DropFun(IntPtr window, int count, string[] paths);

        /// <summary>  This is the function signature for monitor configuration callback functions.</summary>
        /// <param name="monitor">The monitor that was connected or disconnected.</param>
        /// <param name="connectionEvent">One of `GLFW_CONNECTED` or `GLFW_DISCONNECTED`.</param>
        public delegate void MonitorFun(IntPtr monitor, int connectionEvent);

        /// <summary>
        /// This is the function signature for joystick configuration callback
        /// functions.
        /// </summary>
        /// <param name="joy">The joystick that was connected or disconnected.</param>
        /// <param name="connectionEvent">One of `GLFW_CONNECTED` or `GLFW_DISCONNECTED`.</param>
        public delegate void JoystickFun(int joy, int connectionEvent);

        #endregion

        #region Structs

        /// <summary>
        /// This describes a single video mode.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
        [StructLayout(LayoutKind.Sequential)]
        public struct VidMode
        {
            public readonly int Width;
            public readonly int Height;
            public readonly int RedBits;
            public readonly int GreenBits;
            public readonly int BlueBits;
            public readonly int RefreshRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct UnmanagedGammaRamp
        {
            internal ushort* red;
            internal ushort* green;
            internal ushort* blue;
            [MarshalAs(UnmanagedType.U4)] internal uint size;
        }

        /// <summary>
        /// This describes the gamma ramp for a monitor.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GammaRamp
        {
            public ushort[] Red;
            public ushort[] Green;
            public ushort[] Blue;
            public uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct UnmanagedImage
        {
            internal int width;
            internal int height;
            internal byte* pixels;
        }

        /// <summary>
        /// This describes an image to be used for a cursor or window icon.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Image
        {
            public int Width;
            public int Height;
            public byte[] Pixels;
        }

        #endregion

        #region Internal Delegates

        private delegate int GlfwInitInternal();

        private static GlfwInitInternal _glfwInit;

        private delegate IntPtr GetVersionStringInternal();

        private static GetVersionStringInternal _getVersionString;

        private delegate IntPtr GetMonitorsInternal(out int count);

        private static GetMonitorsInternal _getMonitors;

        private delegate IntPtr GetVideoModesInternal(IntPtr monitor, out int count);

        private static GetVideoModesInternal _getVideoModes;

        private delegate IntPtr GetMonitorNameInternal(IntPtr monitor);

        private static GetMonitorNameInternal _getMonitorName;

        private delegate IntPtr GetVideoModeInternal(IntPtr monitor);

        private static GetVideoModeInternal _getVideoMode;

        private unsafe delegate UnmanagedGammaRamp* GetGammaRampInternal(IntPtr monitor);

        private static GetGammaRampInternal _getGammaRamp;

        private delegate void SetGammaRampInternal(IntPtr monitor, IntPtr ramp);

        private static SetGammaRampInternal _setGammaRamp;

        private delegate void SetWindowIconInternal(IntPtr window, int count, IntPtr images);

        private static SetWindowIconInternal _setWindowIcon;

        private delegate IntPtr GetKeyNameInternal(int key, int scancode);

        private static GetKeyNameInternal _getKeyName;

        private delegate IntPtr CreateCursorInternal(IntPtr image, int xhot, int yhot);

        private static CreateCursorInternal _createCursor;

        private delegate void SetDropCallbackInternal(IntPtr window, UnmanagedDropFun cbfun);

        private static SetDropCallbackInternal _setDropCallback;

        private unsafe delegate byte* GetJoystickButtonsInternal(int joy, out int count);

        private static GetJoystickButtonsInternal _getJoystickButtons;

        private unsafe delegate float* GetJoystickAxesInternal(int joy, out int count);

        private static GetJoystickAxesInternal _getJoystickAxes;

        private delegate IntPtr GetClipboardStringInternal(IntPtr window);

        private static GetClipboardStringInternal _getClipboardString;

        private delegate IntPtr GetJoystickNameInternal(int joy);

        private static GetJoystickNameInternal _getJoystickName;

        private delegate IntPtr GetRequiredInstanceExtensionsInternal(out uint count);

        private static GetRequiredInstanceExtensionsInternal _getRequiredInstanceExtensions;

        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate int SetErrorCallbackInternal(ErrorFun cbfun);

        public delegate void TerminateInternal();

        /// <param name="major">Where to store the major version number.</param>
        /// <param name="minor">Where to store the minor version number.</param>
        /// <param name="rev">Where to store the revision number.</param>
        public delegate void GetVersionInternal(out int major, out int minor, out int rev);

        public delegate IntPtr GetPrimaryMonitorInternal();

        /// <param name="monitor">The monitor to query.</param>
        /// <param name="xpos">Where to store the monitor x-coordinate.</param>
        /// <param name="ypos">Where to store the monitor y-coordinate.</param>
        public delegate void GetMonitorPosInternal(IntPtr monitor, out int xpos, out int ypos);

        /// <param name="monitor">The monitor to query.</param>
        /// <param name="widthMm">
        /// Where to store the width, in millimetres, of the monitor's display
        /// area.
        /// </param>
        /// <param name="heightMm">
        /// Where to store the height, in millimetres, of the monitor's
        /// display area.
        /// </param>
        public delegate void GetMonitorPhysicalSizeInternal(IntPtr monitor, out int widthMm, out int heightMm);

        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetMonitorCallbackInternal(MonitorFun cbfun);

        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="gamma">The desired exponent.</param>
        public delegate void SetGammaInternal(IntPtr monitor, float gamma);

        public delegate void DefaultWindowHintsInternal();

        /// <param name="hint">The window hint to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        public delegate void WindowHintInternal(int hint, int value);

        /// <param name="width">
        /// The desired width, in screen coordinates, of the window. This must
        /// be greater than zero.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the window. This must
        /// be greater than zero.
        /// </param>
        /// <param name="title">The initial, UTF-8 encoded window title.</param>
        /// <param name="monitor">
        /// The monitor to use for full screen mode, or <c>null</c> for
        /// windowed mode.
        /// </param>
        /// <param name="share">
        /// The window whose context to share resources with, or <c>null</c> to
        /// not share resources.
        /// </param>
        public delegate IntPtr CreateWindowInternal(int width, int height, string title, IntPtr monitor, IntPtr share);

        /// <param name="window">The window to destroy.</param>
        public delegate void DestroyWindowInternal(IntPtr window);

        /// <param name="window">The window to query.</param>
        public delegate int WindowShouldCloseInternal(IntPtr window);

        /// <param name="window">The window whose flag to change.</param>
        /// <param name="value">The new value.</param>
        public delegate void SetWindowShouldCloseInternal(IntPtr window, int value);

        /// <param name="window">The window whose title to change.</param>
        /// <param name="title">The UTF-8 encoded window title.</param>
        public delegate void SetWindowTitleInternal(IntPtr window, string title);

        /// <param name="window">The window to query.</param>
        /// <param name="xpos">
        /// Where to store the x-coordinate of the upper-left corner of the
        /// client area.
        /// </param>
        /// <param name="ypos">
        /// Where to store the y-coordinate of the upper-left corner of the
        /// client area.
        /// </param>
        public delegate void GetWindowPosInternal(IntPtr window, out int xpos, out int ypos);

        /// <param name="window">The window to query.</param>
        /// <param name="xpos">The x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="ypos">The y-coordinate of the upper-left corner of the client area.</param>
        public delegate void SetWindowPosInternal(IntPtr window, int xpos, int ypos);

        /// <param name="window">The window whose size to retrieve.</param>
        /// <param name="width">
        /// Where to store the width, in screen coordinates, of the client
        /// area.
        /// </param>
        /// <param name="height">
        /// Where to store the height, in screen coordinates, of the client
        /// area.
        /// </param>
        public delegate void GetWindowSizeInternal(IntPtr window, out int width, out int height);

        /// <param name="window">The window to set limits for.</param>
        /// <param name="minwidth">
        /// The minimum width, in screen coordinates, of the client area, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="minheight">
        /// The minimum height, in screen coordinates, of the client area,
        /// or <see cref="DontCare" />.
        /// </param>
        /// <param name="maxwidth">
        /// The maximum width, in screen coordinates, of the client area, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="maxheight">
        /// The maximum height, in screen coordinates, of the client area,
        /// or <see cref="DontCare" />.
        /// </param>
        public delegate void SetWindowSizeLimitsInternal(IntPtr window, int minwidth, int minheight, int maxwidth, int maxheight);

        /// <param name="window">The window to set limits for.</param>
        /// <param name="numer">
        /// The numerator of the desired aspect ratio, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="denom">
        /// The denominator of the desired aspect ratio, or
        /// <see cref="DontCare" />.
        /// </param>
        public delegate void SetWindowAspectRatioInternal(IntPtr window, int numer, int denom);

        /// <param name="window">The window to resize.</param>
        /// <param name="width">
        /// The desired width, in screen coordinates, of the window client
        /// area.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the window client
        /// area.
        /// </param>
        public delegate void SetWindowSizeInternal(IntPtr window, int width, int height);

        /// <param name="window">The window whose framebuffer to query.</param>
        /// <param name="width">Where to store the width, in pixels, of the framebuffer.</param>
        /// <param name="height">Where to store the height, in pixels, of the framebuffer.</param>
        public delegate void GetFramebufferSizeInternal(IntPtr window, out int width, out int height);

        /// <param name="window">The window whose frame size to query.</param>
        /// <param name="left">
        /// Where to store the size, in screen coordinates, of the left edge of
        /// the window frame.
        /// </param>
        /// <param name="top">
        /// Where to store the size, in screen coordinates, of the top edge of the
        /// window frame.
        /// </param>
        /// <param name="right">
        /// Where to store the size, in screen coordinates, of the right edge of
        /// the window frame.
        /// </param>
        /// <param name="bottom">
        /// Where to store the size, in screen coordinates, of the bottom edge
        /// of the window frame.
        /// </param>
        public delegate void GetWindowFrameSizeInternal(IntPtr window, out int left, out int top, out int right, out int bottom);

        /// <param name="window">The window to iconify.</param>
        public delegate void IconifyWindowInternal(IntPtr window);

        /// <param name="window">The window to restore.</param>
        public delegate void RestoreWindowInternal(IntPtr window);

        /// <param name="window">The window to maximize.</param>
        public delegate void MaximizeWindowInternal(IntPtr window);

        /// <param name="window">The window to make visible.</param>
        public delegate void ShowWindowInternal(IntPtr window);

        /// <param name="window">The window to hide.</param>
        public delegate void HideWindowInternal(IntPtr window);

        /// <param name="window">The window to give input focus.</param>
        public delegate void FocusWindowInternal(IntPtr window);

        /// <param name="window">The window to query.</param>
        public delegate IntPtr GetWindowMonitorInternal(IntPtr window);

        /// <param name="window">The window whose monitor, size or video mode to set.</param>
        /// <param name="monitor">
        /// The desired monitor, or IntPtr.Zero to set windowed
        /// mode.
        /// </param>
        /// <param name="xpos">
        /// The desired x-coordinate of the upper-left corner of the client
        /// area.
        /// </param>
        /// <param name="ypos">
        /// The desired y-coordinate of the upper-left corner of the client
        /// area.
        /// </param>
        /// <param name="width">
        /// The desired with, in screen coordinates, of the client area or video
        /// mode.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the client area or
        /// video mode.
        /// </param>
        /// <param name="refreshRate">
        /// The desired refresh rate, in Hz, of the video mode, or
        /// <see cref="DontCare" />.
        /// </param>
        public delegate void SetWindowMonitorInternal(IntPtr window, IntPtr monitor, int xpos, int ypos, int width, int height, int refreshRate);

        /// <param name="window">The window to query.</param>
        /// <param name="attrib">The window attribute whose value to return.</param>
        public delegate int GetWindowAttribInternal(IntPtr window, int attrib);

        /// <param name="window">The window whose pointer to set.</param>
        /// <param name="pointer">The new value.</param>
        public delegate void SetWindowUserPointerInternal(IntPtr window, IntPtr pointer);

        /// <param name="window">The window whose pointer to return.</param>
        public delegate IntPtr GetWindowUserPointerInternal(IntPtr window);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowPosCallbackInternal(IntPtr window, WindowPosFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowSizeCallbackInternal(IntPtr window, WindowSizeFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowCloseCallbackInternal(IntPtr window, WindowCloseFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowRefreshCallbackInternal(IntPtr window, WindowRefreshFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowFocusCallbackInternal(IntPtr window, WindowFocusFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetWindowIconifyCallbackInternal(IntPtr window, WindowIconifyFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetFramebufferSizeCallbackInternal(IntPtr window, FramebufferSizeFun cbfun);

        public delegate void PollEventsInternal();

        public delegate void WaitEventsInternal();

        /// <param name="timeout">The maximum amount of time, in seconds, to wait.</param>
        public delegate void WaitEventsTimeoutInternal(double timeout);

        public delegate void PostEmptyEventInternal();

        /// <param name="window">The window to query.</param>
        /// <param name="mode">One of the input states.</param>
        public delegate int GetInputModeInternal(IntPtr window, int mode);

        /// <param name="window">The window whose input mode to set.</param>
        /// <param name="mode">One of the input states.</param>
        /// <param name="value">The new value of the specified input mode.</param>
        public delegate void SetInputModeInternal(IntPtr window, int mode, int value);

        /// <param name="window">The desired window.</param>
        /// <param name="key">
        /// The desired keyboard key. <see cref="KeyUnknown" /> is not a valid
        /// key for this function.
        /// </param>
        public delegate int GetKeyInternal(IntPtr window, int key);

        /// <param name="window">The desired window.</param>
        /// <param name="button">The desired mouse button.</param>
        public delegate int GetMouseButtonInternal(IntPtr window, int button);

        /// <param name="window">The desired window.</param>
        /// <param name="xpos">
        /// Where to store the cursor x-coordinate, relative to the left edge of
        /// the client area.
        /// </param>
        /// <param name="ypos">
        /// Where to store the cursor y-coordinate, relative to the to top edge
        /// of the client area.
        /// </param>
        public delegate void GetCursorPosInternal(IntPtr window, out double xpos, out double ypos);

        /// <param name="window">The desired window.</param>
        /// <param name="xpos">
        /// The desired x-coordinate, relative to the left edge of the client
        /// area.
        /// </param>
        /// <param name="ypos">
        /// The desired y-coordinate, relative to the top edge of the client
        /// area.
        /// </param>
        public delegate void SetCursorPosInternal(IntPtr window, double xpos, double ypos);

        /// <param name="shape">One of the standard shapes.</param>
        public delegate IntPtr CreateStandardCursorInternal(int shape);

        /// <param name="cursor">The cursor object to destroy.</param>
        public delegate void DestroyCursorInternal(IntPtr cursor);

        /// <param name="window">The window to set the cursor for.</param>
        /// <param name="cursor">The cursor to set.</param>
        public delegate void SetCursorInternal(IntPtr window, IntPtr cursor);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetKeyCallbackInternal(IntPtr window, KeyFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetCharCallbackInternal(IntPtr window, CharFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetCharModsCallbackInternal(IntPtr window, CharModsFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetMouseButtonCallbackInternal(IntPtr window, MouseButtonFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetCursorPosCallbackInternal(IntPtr window, CursorPosFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetCursorEnterCallbackInternal(IntPtr window, CursorEnterFun cbfun);

        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetScrollCallbackInternal(IntPtr window, ScrollFun cbfun);

        /// <param name="joy">The joystick to query.</param>
        public delegate int JoystickPresentInternal(int joy);

        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public delegate void SetJoystickCallbackInternal(JoystickFun cbfun);

        /// <param name="window">The window that will own the clipboard contents.</param>
        /// <param name="str">A UTF-8 encoded string.</param>
        public delegate void SetClipboardStringInternal(IntPtr window, string str);

        public delegate double GetTimeInternal();

        /// <param name="time">The new value, in seconds.</param>
        public delegate void SetTimeInternal(double time);

        public delegate ulong GetTimerValueInternal();

        public delegate ulong GetTimerFrequencyInternal();

        /// <param name="window">
        /// The window whose context to make current, or
        /// IntPtr.Zero to detach the current context.
        /// </param>
        public delegate void MakeContextCurrentInternal(IntPtr window);

        public delegate IntPtr GetCurrentContextInternal();

        /// <param name="window">The window whose buffers to swap.</param>
        public delegate void SwapBuffersInternal(IntPtr window);

        /// <param name="interval">
        /// The minimum number of screen updates to wait for until the
        /// buffers are swapped by <see cref="SwapBuffers" />.
        /// </param>
        public delegate void SwapIntervalInternal(int interval);

        /// <param name="extension">The ASCII encoded name of the extension.</param>
        public delegate int ExtensionSupportedInternal(string extension);

        /// <param name="procname">The ASCII encoded name of the function.</param>
        public delegate IntPtr GetProcAddressInternal(string procname);

        public delegate int VulkanSupportedInternal();

        public delegate bool JoystickIsGamepadInternal(int id);

        #endregion

        #region Function Handles

        /// <summary>
        ///     <para>
        ///     This function sets the error callback, which is called with an error code and a
        ///     human-readable description each time a GLFW error occurs.
        ///     </para>
        ///     <para>
        ///     The error callback is called on the thread where the error occurred. If you are
        ///     using GLFW from multiple threads, your error callback needs to be written
        ///     accordingly.
        ///     </para>
        ///     <para>
        ///     Because the description string may have been generated specifically for that
        ///     error, it is not guaranteed to be valid after the callback has returned. If you wish to
        ///     use it after the callback returns, you need to make a copy.
        ///     </para>
        ///     <para>
        ///     Once set, the error callback remains set even after the library has been
        ///     terminated.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        public static SetErrorCallbackInternal SetErrorCallback;

        /// <summary>
        ///     <para>
        ///     This function initializes the GLFW library. Before most GLFW functions can be
        ///     used, GLFW must be initialized, and before an application terminates GLFW should be
        ///     terminated in order to free any resources allocated during or after
        ///     initialization.
        ///     </para>
        ///     <para>
        ///     If this function fails, it calls <see cref="Terminate" /> before returning. If it
        ///     succeeds, you should call <see cref="Terminate" /> before the application exits.
        ///     </para>
        ///     <para>
        ///     Additional calls to this function after successful initialization but before
        ///     termination will return <c>true</c> immediately.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if successful, or <c>false</c> if an error occurred.</returns>
        /// <remarks>
        /// <strong>OSX:</strong> This function will change the current directory of the application
        /// to the <c>Contents/Resources</c> subdirectory of the application's bundle, if present.
        /// </remarks>
        /// <seealso cref="Terminate" />
        public static TerminateInternal Terminate;

        /// <summary>
        /// This function retrieves the major, minor and revision numbers of the GLFW library. It is
        /// intended for when you are using GLFW as a shared library and want to ensure that you are
        /// using the minimum required version.
        /// </summary>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        /// <seealso cref="GetVersionString" />
        public static GetVersionInternal GetVersion;

        /// <summary>
        /// This function returns the primary monitor. This is usually the monitor where elements
        /// like the task bar or global menu bar are located.
        /// </summary>
        /// <returns>
        /// The primary monitor, or <c>null</c> if no monitors were found or if an error
        /// occurred.
        /// </returns>
        /// <remarks>
        /// The primary monitor is always first in the array returned by <see cref="GetMonitors" />.
        /// </remarks>
        /// <seealso cref="GetMonitors" />
        public static GetPrimaryMonitorInternal GetPrimaryMonitor;

        /// <summary>
        /// This function returns the position, in screen coordinates, of the upper-left corner of
        /// the specified monitor.
        /// </summary>
        public static GetMonitorPosInternal GetMonitorPos;

        /// <summary>
        ///     <para>
        ///     This function returns the size, in millimetres, of the display area of the
        ///     specified monitor.
        ///     </para>
        ///     <para>
        ///     Some systems do not provide accurate monitor size information, either because the
        ///     monitor <a href="https://en.wikipedia.org/wiki/Extended_display_identification_data">EDID</a>
        ///     data is incorrect or because the driver does not report it accurately.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// <strong>Win32: </strong> Calculates the returned physical size from the current
        /// resolution and system DPI instead of querying the monitor EDID data.
        /// </remarks>
        public static GetMonitorPhysicalSizeInternal GetMonitorPhysicalSize;

        /// <summary>
        /// This function sets the monitor configuration callback, or removes the currently set
        /// callback. This is called when a monitor is connected to or disconnected from the system.
        /// </summary>
        public static SetMonitorCallbackInternal SetMonitorCallback;

        /// <summary>
        /// This function generates a 256-element gamma ramp from the specified exponent and then
        /// calls <see cref="SetGammaRamp(IntPtr, ref GammaRamp)" /> with it. The value must be a finite
        /// number greater than zero.
        /// </summary>
        public static SetGammaInternal SetGamma;

        /// <summary>
        /// This function resets all window hints to their default values.
        /// </summary>
        /// <seealso cref="WindowHint" />
        public static DefaultWindowHintsInternal DefaultWindowHints;

        /// <summary>
        ///     <para>
        ///     This function sets hints for the next call to
        ///     <see cref="CreateWindow" />. The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint" /> or
        ///     <see cref="DefaultWindowHints" />, or until the library is terminated.
        ///     </para>
        ///     <para>This function does not check whether the specified hint values are valid.</para>
        ///     <para>
        ///     If you set hints to invalid values this will instead be reported by the next call
        ///     to <see cref="CreateWindow" />.
        ///     </para>
        /// </summary>
        /// <seealso cref="DefaultWindowHints" />
        public static WindowHintInternal WindowHint;

        /// <summary>
        ///     <para>
        ///     This function creates a window and its associated OpenGL or OpenGL ES context.
        ///     Most of the options controlling how the window and its context should be created are
        ///     specified with window hints.
        ///     </para>
        ///     <para>
        ///     Successful creation does not change which context is current. Before you can use
        ///     the newly created context, you need to make it current.
        ///     </para>
        ///     <para>
        ///     The created window, framebuffer and context may differ from what you requested, as
        ///     not all parameters and hints are hard constraints. This includes the size of the window,
        ///     especially for full screen windows. To query the actual attributes of the created
        ///     window, framebuffer and context see <see cref="GetWindowAttrib" />,
        ///     <see cref="GetWindowSize" /> and
        ///     <see cref="GetFramebufferSize" />.
        ///     </para>
        ///     <para>
        ///     To create a full screen window, you need to specify the monitor the window will
        ///     cover. If no monitor is specified, the window will be windowed mode. Unless you have a
        ///     way for the user to choose a specific monitor, it is recommended that you pick the
        ///     primary monitor.
        ///     </para>
        ///     <para>
        ///     For full screen windows, the specified size becomes the resolution of the window's
        ///     <em>desired video mode</em>. As long as a full screen window is not iconified, the
        ///     supported video mode most closely matching the desired video mode is set for the
        ///     specified monitor.
        ///     </para>
        ///     <para>
        ///     By default, newly created windows use the placement recommended by the window
        ///     system. To create the window at a specific position, make it initially invisible using
        ///     the <see cref="Visible" /> hint, set its position and then show it.
        ///     </para>
        ///     <para>
        ///     As long as at least one full screen window is not iconified, the screensaver is
        ///     prohibited from starting.
        ///     </para>
        ///     <para>
        ///     Window systems put limits on window sizes. Very large or very small window
        ///     dimensions may be overridden by the window system on creation. Check the actual size
        ///     after creation.
        ///     </para>
        ///     <para>
        ///     The swap interval is not set during window creation and the initial value may vary
        ///     depending on driver settings and defaults.
        ///     </para>
        /// </summary>
        /// <returns>
        /// The handle of the created window, or <c>null</c> if an error
        /// occurred.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///     <strong>Win32:</strong> Window creation will fail if the Microsoft GDI software
        ///     OpenGL implementation is the only one available.
        ///     </para>
        ///     <para>
        ///     <strong>Win32:</strong> If the executable has an icon resource named
        ///     <c>GLFW_ICON</c>, it will be set as the initial icon for the window. If no such icon is
        ///     present, the <c>IDI_WINLOGO</c> icon will be used instead. To set a different icon, use
        ///     <see cref="SetWindowIcon(IntPtr, int, Image[])" />.
        ///     </para>
        ///     <para>
        ///     <strong>Win32:</strong>The context to share resources with must not be current on
        ///     any other thread.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> The GLFW window has no icon, as it is not a document window,
        ///     but the dock icon will be the same as the application bundle's icon. For more
        ///     information on bundles, see the
        ///     <a href="https://developer.apple.com/library/mac/documentation/CoreFoundation/Conceptual/CFBundles/">
        ///     Bundle Programming Guide
        ///     </a>
        ///     in the Mac Developer Library.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> The first time a window is created the menu bar is populated
        ///     with common commands like Hide, Quit and About. The About entry opens a minimal about
        ///     dialog with information from the application's bundle.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> On OS X 10.10 and later the window frame will not be
        ///     rendered at full resolution on Retina displays unless the <c>NSHighResolutionCapable</c>
        ///     key is enabled in the application bundle's <c>Info.plist</c>. For more information, see
        ///     <a
        ///         href="https://developer.apple.com/library/mac/documentation/GraphicsAnimation/Conceptual/HighResolutionOSX/Explained/Explained.html">
        ///     High Resolution Guidelines for OS X
        ///     </a>
        ///     in the Mac Developer Library.
        ///     </para>
        ///     <para>
        ///     <strong>X11:</strong> Some window managers will not respect the placement of
        ///     initially hidden windows
        ///     </para>
        ///     <para>
        ///     <strong>X11:</strong> Due to the asynchronous nature of X11, it may take a moment
        ///     for a window to reach its requested state.This means you may not be able to query the
        ///     final size, position or other attributes directly after window creation.
        ///     </para>
        /// </remarks>
        /// <seealso cref="DestroyWindow" />
        public static CreateWindowInternal CreateWindow;

        /// <summary>
        ///     <para>
        ///     This function destroys the specified window and its context. On calling this
        ///     function, no further callbacks will be called for that window.
        ///     </para>
        ///     <para>
        ///     If the context of the specified window is current on the main thread, it is
        ///     detached before being destroyed.
        ///     </para>
        /// </summary>
        /// <seealso cref="CreateWindow" />
        public static DestroyWindowInternal DestroyWindow;

        /// <summary>
        /// This function returns the value of the close flag of the specified window.
        /// </summary>
        /// <returns>The value of the close flag.</returns>
        public static WindowShouldCloseInternal WindowShouldClose;

        /// <summary>
        /// This function sets the value of the close flag of the specified window. This can be used
        /// to override the user's attempt to close the window, or to signal that it should be
        /// closed.
        /// </summary>
        public static SetWindowShouldCloseInternal SetWindowShouldClose;

        /// <summary>
        /// This function sets the window title, encoded as UTF-8, of the specified window.
        /// </summary>
        /// <remarks>
        /// <strong>OSX:</strong> The window title will not be updated until the next time you
        /// process events.
        /// </remarks>
        public static SetWindowTitleInternal SetWindowTitle;

        /// <summary>
        /// This function retrieves the position, in screen coordinates, of the upper-left corner of
        /// the client area of the specified window.
        /// </summary>
        /// <seealso cref="SetWindowPos" />
        public static GetWindowPosInternal GetWindowPos;

        /// <summary>
        ///     <para>
        ///     This function sets the position, in screen coordinates, of the upper-left corner
        ///     of the client area of the specified windowed mode window. If the window is a full screen
        ///     window, this function does nothing.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to move an already visible window unless
        ///     you have very good reasons for doing so, as it will confuse and annoy the user.
        ///     </para>
        ///     <para>
        ///     The window manager may put limits on what positions are allowed. GLFW cannot and
        ///     should not override these limits.
        ///     </para>
        /// </summary>
        /// <seealso cref="GetWindowPos" />
        public static SetWindowPosInternal SetWindowPos;

        /// <summary>
        /// This function retrieves the size, in screen coordinates, of the client area of the
        /// specified window.If you wish to retrieve the size of the framebuffer of the window in
        /// pixels, see <see cref="GetFramebufferSize" />.
        /// </summary>
        /// <seealso cref="SetWindowSize" />
        public static GetWindowSizeInternal GetWindowSize;

        /// <summary>
        ///     <para>
        ///     This function sets the size limits of the client area of the specified window. If
        ///     the window is full screen, the size limits only take effect once it is made windowed. If
        ///     the window is not resizable, this function does nothing.
        ///     </para>
        ///     <para>
        ///     The size limits are applied immediately to a windowed mode window and may cause it
        ///     to be resized.
        ///     </para>
        ///     <para>
        ///     The maximum dimensions must be greater than or equal to the minimum dimensions and
        ///     all must be greater than or equal to zero.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// If you set size limits and an aspect ratio that conflict, the results are undefined.
        /// </remarks>
        /// <seealso cref="SetWindowAspectRatio" />
        public static SetWindowSizeLimitsInternal SetWindowSizeLimits;

        /// <summary>
        ///     <para>
        ///     This function sets the required aspect ratio of the client area of the specified
        ///     window. If the window is full screen, the aspect ratio only takes effect once it is
        ///     made windowed.  If the window is not resizable, this function does nothing.
        ///     </para>
        ///     <para>
        ///     The aspect ratio is specified as a numerator and a denominator and both values
        ///     must be greater than zero. For example, the common 16:9 aspect ratio is specified as 16
        ///     and 9, respectively.
        ///     </para>
        ///     <para>
        ///     If the numerator and denominator is set to <see cref="DontCare" /> then the aspect
        ///     ratio limit is disabled.
        ///     </para>
        ///     <para>
        ///     The aspect ratio is applied immediately to a windowed mode window and may cause it
        ///     to be resized.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// If you set size limits and an aspect ratio that conflict, the results are undefined.
        /// </remarks>
        /// <seealso cref="SetWindowSizeLimits" />
        public static SetWindowAspectRatioInternal SetWindowAspectRatio;

        /// <summary>
        ///     <para>
        ///     This function sets the size, in screen coordinates, of the client area of the
        ///     specified window.
        ///     </para>
        ///     <para>
        ///     For full screen windows, this function updates the resolution of its desired video
        ///     mode and switches to the video mode closest to it, without affecting the window's
        ///     context. As the context is unaffected, the bit depths of the framebuffer remain
        ///     unchanged.
        ///     </para>
        ///     <para>
        ///     If you wish to update the refresh rate of the desired video mode in addition to
        ///     its resolution, see @ref glfwSetWindowMonitor.
        ///     </para>
        ///     <para>
        ///     The window manager may put limits on what sizes are allowed.GLFW cannot and should
        ///     not override these limits.
        ///     </para>
        /// </summary>
        /// <seealso cref="GetWindowSize" />
        /// <seealso cref="SetWindowMonitor" />
        public static SetWindowSizeInternal SetWindowSize;

        /// <summary>
        /// This function retrieves the size, in pixels, of the framebuffer of the specified window.
        /// If you wish to retrieve the size of the window in screen coordinates, see
        /// <see cref="GetWindowSize" />.
        /// </summary>
        /// <seealso cref="SetFramebufferSizeCallback" />
        public static GetFramebufferSizeInternal GetFramebufferSize;

        /// <summary>
        ///     <para>
        ///     This function retrieves the size, in screen coordinates, of each edge of the frame
        ///     of the specified window. This size includes the title bar, if the window has one. The
        ///     size of the frame may vary depending on the window - related hints used to create
        ///     it.
        ///     </para>
        ///     <para>
        ///     Because this function retrieves the size of each window frame edge and not the
        ///     offset along a particular coordinate axis, the retrieved values will always be zero or
        ///     positive.
        ///     </para>
        /// </summary>
        public static GetWindowFrameSizeInternal GetWindowFrameSize;

        /// <summary>
        ///     <para>
        ///     This function iconifies (minimizes) the specified window if it was previously
        ///     restored. If the window is already iconified, this function does nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, the original monitor resolution
        ///     is restored until the window is restored.
        ///     </para>
        /// </summary>
        /// <seealso cref="RestoreWindow" />
        /// <seealso cref="MaximizeWindow" />
        public static IconifyWindowInternal IconifyWindow;

        /// <summary>
        ///     <para>
        ///     This function restores the specified window if it was previously iconified
        ///     (minimized) or maximized.If the window is already restored, this function does
        ///     nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, the resolution chosen for the
        ///     window is restored on the selected monitor.
        ///     </para>
        /// </summary>
        /// <seealso cref="IconifyWindow" />
        /// <seealso cref="MaximizeWindow" />
        public static RestoreWindowInternal RestoreWindow;

        /// <summary>
        ///     <para>
        ///     This function maximizes the specified window if it was previously not maximized.
        ///     If the window is already maximized, this function does nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, this function does
        ///     nothing.
        ///     </para>
        /// </summary>
        /// <seealso cref="IconifyWindow" />
        /// <seealso cref="RestoreWindow" />
        public static MaximizeWindowInternal MaximizeWindow;

        /// <summary>
        /// This function makes the specified window visible if it was previously hidden. If the
        /// window is already visible or is in full screen mode, this function does nothing.
        /// </summary>
        /// <seealso cref="HideWindow" />
        public static ShowWindowInternal ShowWindow;

        /// <summary>
        /// This function hides the specified window if it was previously visible. If the window is
        /// already hidden or is in full screen mode, this function does nothing.
        /// </summary>
        /// <seealso cref="ShowWindow" />
        public static HideWindowInternal HideWindow;

        /// <summary>
        ///     <para>
        ///     This function brings the specified window to front and sets input focus. The
        ///     window should already be visible and not iconified.
        ///     </para>
        ///     <para>
        ///     By default, both windowed and full screen mode windows are focused when initially
        ///     created. Set the <see cref="Focused" /> hint to disable this behavior.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to steal focus from other applications
        ///     unless you are certain that is what the user wants. Focus stealing can be extremely
        ///     disruptive.
        ///     </para>
        /// </summary>
        public static FocusWindowInternal FocusWindow;

        /// <summary>
        /// This function returns the handle of the monitor that the specified window is in full
        /// screen on.
        /// </summary>
        /// <returns>
        /// The monitor, or IntPtr.Zero if the window is in windowed mode or
        /// an error occurred.
        /// </returns>
        /// <seealso cref="SetWindowMonitor" />
        public static GetWindowMonitorInternal GetWindowMonitor;

        /// <summary>
        ///     <para>
        ///     This function sets the monitor that the window uses for full screen mode or, if
        ///     the monitor is IntPtr.Zero, makes it windowed mode.
        ///     </para>
        ///     <para>
        ///     When setting a monitor, this function updates the width, height and refresh rate
        ///     of the desired video mode and switches to the video mode closest to it. The window
        ///     position is ignored when setting a monitor.
        ///     </para>
        ///     <para>
        ///     When the monitor is IntPtr.Zero, the position, width and height
        ///     are used to place the window client area. The refresh rate is ignored when no monitor is
        ///     specified.
        ///     </para>
        ///     <para>
        ///     If you only wish to update the resolution of a full screen window or the size of a
        ///     windowed mode window, see <see cref="SetWindowSize" />"/>.
        ///     </para>
        ///     <para>
        ///     When a window transitions from full screen to windowed mode, this function
        ///     restores any previous window settings such as whether it is decorated, floating,
        ///     resizable, has size or aspect ratio limits, etc...
        ///     </para>
        /// </summary>
        /// <seealso cref="GetWindowMonitor" />
        /// <seealso cref="SetWindowSize" />
        public static SetWindowMonitorInternal SetWindowMonitor;

        /// <summary>
        /// This function returns the value of an attribute of the specified window or its OpenGL or
        /// OpenGL ES context.
        /// </summary>
        /// <returns>The value of the attribute, or <c>false</c> if an error occurred.</returns>
        /// <remarks>
        /// Framebuffer related hints are not window attributes.
        /// </remarks>
        public static GetWindowAttribInternal GetWindowAttrib;

        /// <summary>
        /// This function sets the user-defined pointer of the specified window. The current value
        /// is retained until the window is destroyed. The initial value is <c>IntPtr.Zero</c>.
        /// </summary>
        /// <seealso cref="GetWindowUserPointer" />
        public static SetWindowUserPointerInternal SetWindowUserPointer;

        /// <summary>
        /// This function sets the user-defined pointer of the specified window. The initial value
        /// is <c>IntPtr.Zero</c>.
        /// </summary>
        /// <returns>The user-defined pointer of the specified window.</returns>
        /// <seealso cref="SetWindowUserPointer" />
        public static GetWindowUserPointerInternal GetWindowUserPointer;

        /// <summary>
        /// This function sets the position callback of the specified window, which is called when
        /// the window is moved. The callback is provided with the screen position of the upper-left
        /// corner of the client area of the window.
        /// </summary>
        public static SetWindowPosCallbackInternal SetWindowPosCallback;

        /// <summary>
        /// This function sets the size callback of the specified window, which is called when the
        /// window is resized. The callback is provided with the size, in screen coordinates, of the
        /// client area of the window.
        /// </summary>
        public static SetWindowSizeCallbackInternal SetWindowSizeCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the close callback of the specified window, which is called
        ///     when the user attempts to close the window, for example by clicking the close widget in
        ///     the title bar.
        ///     </para>
        ///     <para>
        ///     The close flag is set before this callback is called, but you can modify it at any
        ///     time with <see cref="SetWindowShouldClose" />.
        ///     </para>
        ///     <para>The close callback is not triggered by <see cref="DestroyWindow" />.</para>
        /// </summary>
        /// <remarks>
        /// <strong>OSX:</strong> Selecting Quit from the application menu will trigger the close
        /// callback for all windows.
        /// </remarks>
        public static SetWindowCloseCallbackInternal SetWindowCloseCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the refresh callback of the specified window, which is called
        ///     when the client area of the window needs to be redrawn, for example if the window has
        ///     been exposed after having been covered by another window.
        ///     </para>
        ///     <para>
        ///     On compositing window systems such as Aero, Compiz or Aqua, where the window
        ///     contents are saved off-screen, this callback may be called only very infrequently or
        ///     never at all.
        ///     </para>
        /// </summary>
        public static SetWindowRefreshCallbackInternal SetWindowRefreshCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the focus callback of the specified window, which is called
        ///     when the window gains or loses input focus.
        ///     </para>
        ///     <para>
        ///     After the focus callback is called for a window that lost input focus, synthetic
        ///     key and mouse button release events will be generated for all such that had been
        ///     pressed. For more information, see <see cref="SetKeyCallback" /> and
        ///     <see cref="SetMouseButtonCallback" />.
        ///     </para>
        /// </summary>
        public static SetWindowFocusCallbackInternal SetWindowFocusCallback;

        /// <summary>
        /// This function sets the iconification callback of the specified window, which is called
        /// when the window is iconified or restored.
        /// </summary>
        public static SetWindowIconifyCallbackInternal SetWindowIconifyCallback;

        /// <summary>
        /// This function sets the framebuffer resize callback of the specified window, which is
        /// called when the framebuffer of the specified window is resized.
        /// </summary>
        public static SetFramebufferSizeCallbackInternal SetFramebufferSizeCallback;

        /// <summary>
        ///     <para>
        ///     This function processes only those events that are already in the event queue and
        ///     then returns immediately. Processing events will cause the window and input callbacks
        ///     associated with those events to be called.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain events are sent directly to the application without
        ///     going through the event queue, causing callbacks to be called outside of a call to one
        ///     of the event processing functions.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <seealso cref="WaitEvents" />
        /// <seealso cref="WaitEventsTimeout" />
        public static PollEventsInternal PollEvents;

        /// <summary>
        ///     <para>
        ///     This function puts the calling thread to sleep until at least one event is
        ///     available in the event queue. Once one or more events are available, it behaves exactly
        ///     like <see cref="PollEvents" />, i.e. the events in the queue are processed and the
        ///     function then returns immediately. Processing events will cause the window and input
        ///     callbacks associated with those events to be called.
        ///     </para>
        ///     <para>
        ///     Since not all events are associated with callbacks, this function may return
        ///     without a callback having been called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain callbacks may be called outside of a call to one of the
        ///     event processing functions.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <seealso cref="PollEvents" />
        /// <seealso cref="WaitEventsTimeout" />
        public static WaitEventsInternal WaitEvents;

        /// <summary>
        ///     <para>
        ///     This function puts the calling thread to sleep until at least one event is
        ///     available in the event queue, or until the specified timeout is reached. Once one or
        ///     more events are available, it behaves exactly like <see cref="PollEvents" />, i.e. the
        ///     events in the queue are processed and the function then returns immediately. Processing
        ///     events will cause the window and input callbacks associated with those events to be
        ///     called.
        ///     </para>
        ///     <para>
        ///     Since not all events are associated with callbacks, this function may return
        ///     without a callback having been called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain callbacks may be called outside of a call to one of the
        ///     event processing functions.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <seealso cref="PollEvents" />
        /// <seealso cref="WaitEvents" />
        public static WaitEventsTimeoutInternal WaitEventsTimeout;

        /// <summary>
        ///     <para>
        ///     This function posts an empty event from the current thread to the event queue,
        ///     causing <see cref="WaitEvents" /> or <see cref="WaitEventsTimeout" /> to
        ///     return.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        /// </summary>
        public static PostEmptyEventInternal PostEmptyEvent;

        /// <summary>
        /// This function returns the value of an input option for the specified window.
        /// </summary>
        /// <returns>The value of an input option for the specified window.</returns>
        public static GetInputModeInternal GetInputMode;

        /// <summary>
        ///     <para>This function sets an input mode option for the specified window.</para>
        ///     <para>
        ///     If the mode is <see cref="Cursor" />, the value must be one of the
        ///     following cursor modes:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///         <see cref="CursorNormal" /> makes the cursor visible and behaving
        ///         normally.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorHidden" /> makes the cursor invisible when it is over the
        ///         client area of the window but does not restrict the cursor from leaving.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorDisabled" /> hides and grabs the cursor, providing
        ///         virtual and unlimited cursor movement.This is useful for implementing for example 3D
        ///         camera controls.
        ///         </item>
        ///     </list>
        ///     <para>
        ///     If the mode is <see cref="StickyKeys" />, the value must be either
        ///     <c>true</c> to enable sticky keys, or <c>false</c> to disable it. If sticky keys are
        ///     enabled, a key press will ensure that <see cref="GetKey" /> returns
        ///     <see cref="Press" /> the next time it is called even if the key had been
        ///     released before the call. This is useful when you are only interested in whether keys
        ///     have been pressed but not when or in which order.
        ///     </para>
        ///     <para>
        ///     If the mode is <see cref="StickyMouseButtons" />, the value must be either
        ///     <c>true</c> to enable sticky mouse buttons, or <c>false</c> to disable it. If sticky
        ///     mouse buttons are enabled, a mouse button press will ensure that
        ///     <see cref="GetMouseButton" /> returns <see cref="Press" /> the
        ///     next time it is called even if the mouse button had been released before the call. This
        ///     is useful when you are only interested in whether mouse buttons have been pressed but
        ///     not when or in which order.
        ///     </para>
        /// </summary>
        /// <seealso cref="GetInputMode" />
        public static SetInputModeInternal SetInputMode;

        /// <summary>
        ///     <para>
        ///     This function returns the last state reported for the specified key to the
        ///     specified window. The returned state is one of <see cref="Press" /> or
        ///     <see cref="Release" />. The higher-level action
        ///     <see cref="Repeat" /> is only reported to the key callback.
        ///     </para>
        ///     <para>
        ///     If the <see cref="StickyKeys" /> input mode is enabled, this function
        ///     returns <see cref="Press" /> the first time you call it for a key that was
        ///     pressed, even if that key has already been released.
        ///     </para>
        ///     <para>
        ///     The key functions deal with physical keys, with key tokens named after their use
        ///     on the standard US keyboard layout. If you want to input text, use the Unicode character
        ///     callback instead.
        ///     </para>
        ///     <para>
        ///     The modifier key bit masks are not key tokens and cannot be used with this
        ///     function.
        ///     </para>
        ///     <para><strong>Do not use this function</strong> to implement text input.</para>
        /// </summary>
        /// <returns>
        /// One of <see cref="Press" /> or
        /// <see cref="Release" />.
        /// </returns>
        public static GetKeyInternal GetKey;

        /// <summary>
        ///     <para>
        ///     This function returns the last state reported for the specified mouse button to
        ///     the specified window. The returned state is one of <see cref="Press" /> or
        ///     <see cref="Release" />.
        ///     </para>
        ///     <para>
        ///     If the <see cref="StickyKeys" /> input mode is enabled, this function
        ///     <see cref="Press" /> the first time you call it for a mouse button that was
        ///     pressed, even if that mouse button has already been released.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if the button was pressed, <c>false</c> otherwise.</returns>
        public static GetMouseButtonInternal GetMouseButton;

        /// <summary>
        ///     <para>
        ///     This function returns the position of the cursor, in screen coordinates, relative
        ///     to the upper-left corner of the client area of the specified window.
        ///     </para>
        ///     <para>
        ///     If the cursor is disabled (with <see cref="CursorDisabled" />) then the cursor
        ///     position is unbounded and limited only by the minimum and maximum values of a
        ///     <c>double</c>.
        ///     </para>
        ///     <para>
        ///     The coordinate can be converted to their integer equivalents with the <c>floor</c>
        ///     function. Casting directly to an integer type works for positive coordinates, but fails
        ///     for negative ones.
        ///     </para>
        /// </summary>
        /// <seealso cref="SetCursorPos" />
        public static GetCursorPosInternal GetCursorPos;

        /// <summary>
        ///     <para>
        ///     This function sets the position, in screen coordinates, of the cursor relative to
        ///     the upper-left corner of the client area of the specified window. The window must have
        ///     input focus. If the window does not have input focus when this function is called, it
        ///     fails silently.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to implement things like camera
        ///     controls. GLFW already provides the <see cref="CursorDisabled" /> cursor mode
        ///     that hides the cursor, transparently re-centers it and provides unconstrained cursor
        ///     motion. See <see cref="SetInputMode" /> for more
        ///     information.
        ///     </para>
        ///     <para>
        ///     If the cursor mode is <see cref="CursorDisabled" /> then the cursor
        ///     position is unconstrained and limited only by the minimum and maximum values of a
        ///     <c>double</c>.
        ///     </para>
        /// </summary>
        /// <seealso cref="GetCursorPos" />
        public static SetCursorPosInternal SetCursorPos;

        /// <summary>
        /// Returns a cursor with a standard shape, that can be set for a window with
        /// <see cref="CreateCursor(Image, int, int)" />.
        /// </summary>
        /// <returns>A new cursor ready to use.</returns>
        public static CreateStandardCursorInternal CreateStandardCursor;

        /// <summary>
        /// This function destroys a cursor previously created with
        /// <see cref="CreateCursor(Image, int, int)" />. Any remaining cursors will be destroyed by
        /// <see cref="Terminate" />.
        /// </summary>
        /// <seealso cref="CreateCursor(Image, int, int)" />
        public static DestroyCursorInternal DestroyCursor;

        /// <summary>
        ///     <para>
        ///     This function sets the cursor image to be used when the cursor is over the client
        ///     area of the specified window.The set cursor will only be visible when the cursor mode
        ///     of the window is <see cref="CursorNormal" />.
        ///     </para>
        ///     <para>
        ///     On some platforms, the set cursor may not be visible unless the window also has
        ///     input focus.
        ///     </para>
        /// </summary>
        public static SetCursorInternal SetCursor;

        /// <summary>
        ///     <para>
        ///     This function sets the key callback of the specified window, which is called when
        ///     a key is pressed, repeated or released.
        ///     </para>
        ///     <para>
        ///     The key functions deal with physical keys, with layout independent key tokens
        ///     named after their values in the standard US keyboard layout. If you want to input text,
        ///     use the <see cref="SetCharCallback" /> instead.
        ///     </para>
        ///     <para>
        ///     When a window loses input focus, it will generate synthetic key release events for
        ///     all pressed keys. You can tell these events from user-generated events by the fact that
        ///     the synthetic ones are generated after the focus loss event has been processed, i.e.
        ///     after the <see cref="SetWindowFocusCallback" /> has been
        ///     called.
        ///     </para>
        ///     <para>
        ///     The scancode of a key is specific to that platform or sometimes even to that
        ///     machine. Scancodes are intended to allow users to bind keys that don't have a GLFW key
        ///     token. Such keys have <c>key</c> set to <see cref="KeyUnknown" />, their state is
        ///     not saved and so it cannot be queried with <see cref="GetKey" />.
        ///     </para>
        ///     <para>
        ///     Sometimes GLFW needs to generate synthetic key events, in which case the scancode
        ///     may be zero.
        ///     </para>
        /// </summary>
        public static SetKeyCallbackInternal SetKeyCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the character callback of the specified window, which is called
        ///     when a Unicode character is input.
        ///     </para>
        ///     <para>
        ///     The character callback is intended for Unicode text input. As it deals with
        ///     characters, it is keyboard layout dependent, whereas
        ///     <see cref="SetKeyCallback" /> is not. Characters do not map 1:1 to
        ///     physical keys, as a key may produce zero, one or more characters. If you want to know
        ///     whether a specific physical key was pressed or released, see the key callback
        ///     instead.
        ///     </para>
        ///     <para>
        ///     The character callback behaves as system text input normally does and will not be
        ///     called if modifier keys are held down that would prevent normal text input on that
        ///     platform, for example a Super (Command) key on OS X or Alt key on Windows. There is
        ///     <see cref="SetCharModsCallback" /> that receives these
        ///     events.
        ///     </para>
        /// </summary>
        public static SetCharCallbackInternal SetCharCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the character with modifiers callback of the specified window,
        ///     which is called when a Unicode character is input regardless of what modifier keys are
        ///     used.
        ///     </para>
        ///     <para>
        ///     The character with modifiers callback is intended for implementing custom Unicode
        ///     character input. For regular Unicode text input, use
        ///     <see cref="SetCharCallback" />. Like the character callback, the
        ///     character with modifiers callback deals with characters and is keyboard layout
        ///     dependent. Characters do not map 1:1 to physical keys, as a key may produce zero, one or
        ///     more characters.If you want to know whether a specific physical key was pressed or
        ///     released, see <see cref="SetKeyCallback" /> instead.
        ///     </para>
        /// </summary>
        public static SetCharModsCallbackInternal SetCharModsCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the mouse button callback of the specified window, which is
        ///     called when a mouse button is pressed or released.
        ///     </para>
        ///     <para>
        ///     When a window loses input focus, it will generate synthetic mouse button release
        ///     events for all pressed mouse buttons. You can tell these events from user-generated
        ///     events by the fact that the synthetic ones are generated after the focus loss event has
        ///     been processed, i.e. after <see cref="SetWindowFocusCallback" />
        ///     has been called.
        ///     </para>
        /// </summary>
        public static SetMouseButtonCallbackInternal SetMouseButtonCallback;

        /// <summary>
        /// This function sets the cursor position callback of the specified window, which is called
        /// when the cursor is moved.The callback is provided with the position, in screen
        /// coordinates, relative to the upper-left corner of the client area of the window.
        /// </summary>
        public static SetCursorPosCallbackInternal SetCursorPosCallback;

        /// <summary>
        /// This function sets the cursor boundary crossing callback of the specified window, which
        /// is called when the cursor enters or leaves the client area of the window.
        /// </summary>
        public static SetCursorEnterCallbackInternal SetCursorEnterCallback;

        /// <summary>
        ///     <para>
        ///     This function sets the scroll callback of the specified window, which is called
        ///     when a scrolling device is used, such as a mouse wheel or scrolling area of a
        ///     touchpad.
        ///     </para>
        ///     <para>
        ///     The scroll callback receives all scrolling input, like that from a mouse wheel or
        ///     a touchpad scrolling area.
        ///     </para>
        /// </summary>
        public static SetScrollCallbackInternal SetScrollCallback;

        /// <summary>
        /// This function returns whether the specified joystick is present.
        /// </summary>
        /// <returns><c>true</c> if the joystick is present, or <c>false</c> otherwise.</returns>
        public static JoystickPresentInternal JoystickPresent;

        /// <summary>
        /// This function sets the joystick configuration callback, or removes the currently set
        /// callback. This is called when a joystick is connected to or disconnected from the
        /// system.
        /// </summary>
        public static SetJoystickCallbackInternal SetJoystickCallback;

        /// <summary>
        /// This function sets the system clipboard to the specified, UTF-8 encoded string.
        /// </summary>
        /// <seealso cref="GetClipboardString(IntPtr)" />
        public static SetClipboardStringInternal SetClipboardString;

        /// <summary>
        ///     <para>
        ///     This function returns the value of the GLFW timer. Unless the timer has been set
        ///     using <see cref="SetTime" />, the timer measures time elapsed since GLFW was
        ///     initialized.
        ///     </para>
        ///     <para>
        ///     The resolution of the timer is system dependent, but is usually on the order of a
        ///     few micro- or nanoseconds. It uses the highest-resolution monotonic time source on each
        ///     supported platform.
        ///     </para>
        /// </summary>
        /// <returns>The current value, in seconds, or zero if an error occurred.</returns>
        public static GetTimeInternal GetTime;

        /// <summary>
        /// This function sets the value of the GLFW timer. It then continues to count up from that
        /// value. The value must be a positive finite number less than or equal to 18446744073.0
        /// which is approximately 584.5 years.
        /// </summary>
        /// <remarks>
        /// The upper limit of the timer is calculated as <c>floor((2^64 - 1) / 10^9)</c> and is due
        /// to implementations storing nanoseconds in 64 bits. The limit may be increased in the
        /// future.
        /// </remarks>
        public static SetTimeInternal SetTime;

        /// <summary>
        /// This function returns the current value of the raw timer, measured in <c>1/frequency</c>
        /// seconds. To get the frequency, call <see cref="GetTimerFrequency" />.
        /// </summary>
        /// <returns>The value of the timer, or zero if an error occurred.</returns>
        /// <seealso cref="GetTimerFrequency" />
        public static GetTimerValueInternal GetTimerValue;

        /// <summary>
        /// This function returns the frequency, in Hz, of the raw timer.
        /// </summary>
        /// <returns>The frequency of the timer, in Hz, or zero if an error occurred</returns>
        /// <seealso cref="GetTimerValue" />
        public static GetTimerFrequencyInternal GetTimerFrequency;

        /// <summary>
        ///     <para>
        ///     This function makes the OpenGL or OpenGL ES context of the specified window
        ///     current on the calling thread. A context can only be made current on a single thread at
        ///     a time and each thread can have only a single current context at a time.
        ///     </para>
        ///     <para>
        ///     By default, making a context non-current implicitly forces a pipeline flush. On
        ///     machines that support <c>GL_KHR_context_flush_control</c>, you can control whether a
        ///     context performs this flush by setting the <see cref="ContextReleaseBehavior" />
        ///     window hint.
        ///     </para>
        ///     <para>
        ///     The specified window must have an OpenGL or OpenGL ES context. Specifying a window
        ///     without a context will generate a <see cref="ErrorNoWindowContext" /> error.
        ///     </para>
        /// </summary>
        /// <seealso cref="GetCurrentContext" />
        public static MakeContextCurrentInternal MakeContextCurrent;

        /// <summary>
        /// This function returns the window whose OpenGL or OpenGL ES context is current on the
        /// calling thread.
        /// </summary>
        /// <returns>
        /// The window whose context is current, or IntPtr.Zero if no
        /// window's context is current.
        /// </returns>
        /// <seealso cref="MakeContextCurrent" />
        public static GetCurrentContextInternal GetCurrentContext;

        /// <summary>
        ///     <para>
        ///     This function swaps the front and back buffers of the specified window when
        ///     rendering with OpenGL or OpenGL ES. If the swap interval is greater than zero, the GPU
        ///     driver waits the specified number of screen updates before swapping the buffers.
        ///     </para>
        ///     <para>
        ///     The specified window must have an OpenGL or OpenGL ES context. Specifying a window
        ///     without a context will generate a <see cref="ErrorNoWindowContext" /> error.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// <strong>EGL:</strong> The context of the specified window must be current on the
        /// calling thread.
        /// </remarks>
        public static SwapBuffersInternal SwapBuffers;

        /// <summary>
        ///     <para>
        ///     This function sets the swap interval for the current OpenGL or OpenGL ES context,
        ///     i.e. the number of screen updates to wait from the time
        ///     <see cref="SwapBuffers" /> was called before swapping the buffers and returning.
        ///     This is sometimes called <em>vertical synchronization</em>,
        ///     <em>
        ///     vertical retrace
        ///     synchronization
        ///     </em>
        ///     or just <em>vsync</em>.
        ///     </para>
        ///     <para>
        ///     Contexts that support either of the <c>WGL_EXT_swap_control_tear</c> and
        ///     <c>GLX_EXT_swap_control_tear</c> extensions also accept negative swap intervals, which
        ///     allow the driver to swap even if a frame arrives a little bit late.
        ///     </para>
        ///     <para>
        ///     You can check for the presence of these extensions using
        ///     <see cref="ExtensionSupported" />. For more information about swap tearing, see
        ///     the extension specifications.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorNoCurrentContext" /> error.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This function is not called during context creation, leaving the swap interval set
        ///     to whatever is the default on that platform. This is done because some swap interval
        ///     extensions used by GLFW do not allow the swap interval to be reset to zero once it has
        ///     been set to a non-zero value.
        ///     </para>
        ///     <para>
        ///     Some GPU drivers do not honor the requested swap interval, either because of a
        ///     user setting that overrides the application's request or due to bugs in the
        ///     driver.
        ///     </para>
        /// </remarks>
        /// <seealso cref="SwapBuffers" />
        public static SwapIntervalInternal SwapInterval;

        /// <summary>
        ///     <para>
        ///     This function returns whether the specified API extension is supported by the
        ///     current OpenGL or OpenGL ES context. It searches both for client API extension and
        ///     context creation API extensions.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorNoCurrentContext" /> error.
        ///     </para>
        ///     <para>
        ///     As this functions retrieves and searches one or more extension strings each call,
        ///     it is recommended that you cache its results if it is going to be used frequently. The
        ///     extension strings will not change during the lifetime of a context, so there is no
        ///     danger in doing this.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if the extension is available, or <c>false</c> otherwise.</returns>
        /// <seealso cref="GetProcAddress" />
        public static ExtensionSupportedInternal ExtensionSupported;

        /// <summary>
        ///     <para>
        ///     This function returns the address of the specified OpenGL or OpenGL ES core or
        ///     extension function, if it is supported by the current context.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorNoCurrentContext" /> error.
        ///     </para>
        /// </summary>
        /// <returns>The address of the function, or <c>null</c> if an error occurred.</returns>
        /// <remarks>
        ///     <para>
        ///     The address of a given function is not guaranteed to be the same between
        ///     contexts.
        ///     </para>
        ///     <para>
        ///     This function may return a non-<c>null</c> address despite the associated version
        ///     or extension not being available. Always check the context version or extension string
        ///     first.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ExtensionSupported" />
        public static GetProcAddressInternal GetProcAddress;

        /// <summary>
        /// This function returns whether the Vulkan loader has been found. This check is performed
        /// by <see cref="Init" />.
        /// </summary>
        /// <returns><c>true</c> if Vulkan is available, or <c>false</c> otherwise.</returns>
        public static VulkanSupportedInternal VulkanSupported;

        //public static JoystickIsGamepadInternal JoystickIsGamepad;

        //public delegate IntPtr GetGamepadNameInternal(int id);
        //public static GetGamepadNameInternal GetGamepadName;

        //public struct GamepadState
        //{
        //    public byte A;
        //    public byte B;
        //    public byte X;
        //    public byte Y;
        //    public byte LeftBumper;
        //    public byte RightBumper;
        //    public byte Back;
        //    public byte Start;
        //    public byte Guide;
        //    public byte LeftThumb;
        //    public byte RightThumb;
        //    public byte Up;
        //    public byte Right;
        //    public byte Down;
        //    public byte Left;

        //    public float AxisLeftX;
        //    public float AxisLeftY;
        //    public float AxisRightX;
        //    public float AxisRightY;
        //    public float AxisLeftTrigger;
        //    public float AxixRightTrigger;
        //}

        //public delegate int GetGamepadStateInternal(int id, ref GamepadState state);
        //public static GetGamepadStateInternal GetGamepadState;

        #endregion

        #region Functions

        /// <summary>
        ///     <para>
        ///     This function initializes the GLFW library. Before most GLFW functions can be
        ///     used, GLFW must be initialized, and before an application terminates GLFW should be
        ///     terminated in order to free any resources allocated during or after
        ///     initialization.
        ///     </para>
        ///     <para>
        ///     If this function fails, it calls <see cref="Terminate" /> before returning. If it
        ///     succeeds, you should call <see cref="Terminate" /> before the application exits.
        ///     </para>
        ///     <para>
        ///     Additional calls to this function after successful initialization but before
        ///     termination will return <c>true</c> immediately.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if successful, or <c>false</c> if an error occurred.</returns>
        /// <remarks>
        /// <strong>OSX:</strong> This function will change the current directory of the application
        /// to the <c>Contents/Resources</c> subdirectory of the application's bundle, if present.
        /// </remarks>
        /// <seealso cref="Terminate" />
        public static int Init(IntPtr lib)
        {
            // Load all library functions.
            GetFuncPointer(lib, "glfwInit", ref _glfwInit);
            GetFuncPointer(lib, "glfwSetErrorCallback", ref SetErrorCallback);
            GetFuncPointer(lib, "glfwTerminate", ref Terminate);
            GetFuncPointer(lib, "glfwGetVersion", ref GetVersion);
            GetFuncPointer(lib, "glfwGetVersionString", ref _getVersionString);
            GetFuncPointer(lib, "glfwGetMonitors", ref _getMonitors);
            GetFuncPointer(lib, "glfwGetPrimaryMonitor", ref GetPrimaryMonitor);
            GetFuncPointer(lib, "glfwGetMonitorPos", ref GetMonitorPos);
            GetFuncPointer(lib, "glfwGetMonitorPhysicalSize", ref GetMonitorPhysicalSize);
            GetFuncPointer(lib, "glfwGetMonitorName", ref _getMonitorName);
            GetFuncPointer(lib, "glfwSetMonitorCallback", ref SetMonitorCallback);
            GetFuncPointer(lib, "glfwGetVideoModes", ref _getVideoModes);
            GetFuncPointer(lib, "glfwGetVideoMode", ref _getVideoMode);
            GetFuncPointer(lib, "glfwSetGamma", ref SetGamma);
            GetFuncPointer(lib, "glfwGetGammaRamp", ref _getGammaRamp);
            GetFuncPointer(lib, "glfwSetGammaRamp", ref _setGammaRamp);
            GetFuncPointer(lib, "glfwDefaultWindowHints", ref DefaultWindowHints);
            GetFuncPointer(lib, "glfwWindowHint", ref WindowHint);
            GetFuncPointer(lib, "glfwCreateWindow", ref CreateWindow);
            GetFuncPointer(lib, "glfwDestroyWindow", ref DestroyWindow);
            GetFuncPointer(lib, "glfwWindowShouldClose", ref WindowShouldClose);
            GetFuncPointer(lib, "glfwSetWindowShouldClose", ref SetWindowShouldClose);
            GetFuncPointer(lib, "glfwSetWindowTitle", ref SetWindowTitle);
            GetFuncPointer(lib, "glfwSetWindowIcon", ref _setWindowIcon);
            GetFuncPointer(lib, "glfwGetWindowPos", ref GetWindowPos);
            GetFuncPointer(lib, "glfwSetWindowPos", ref SetWindowPos);
            GetFuncPointer(lib, "glfwGetWindowSize", ref GetWindowSize);
            GetFuncPointer(lib, "glfwSetWindowSizeLimits", ref SetWindowSizeLimits);
            GetFuncPointer(lib, "glfwSetWindowAspectRatio", ref SetWindowAspectRatio);
            GetFuncPointer(lib, "glfwSetWindowSize", ref SetWindowSize);
            GetFuncPointer(lib, "glfwGetFramebufferSize", ref GetFramebufferSize);
            GetFuncPointer(lib, "glfwGetWindowFrameSize", ref GetWindowFrameSize);
            GetFuncPointer(lib, "glfwIconifyWindow", ref IconifyWindow);
            GetFuncPointer(lib, "glfwRestoreWindow", ref RestoreWindow);
            GetFuncPointer(lib, "glfwMaximizeWindow", ref MaximizeWindow);
            GetFuncPointer(lib, "glfwShowWindow", ref ShowWindow);
            GetFuncPointer(lib, "glfwHideWindow", ref HideWindow);
            GetFuncPointer(lib, "glfwFocusWindow", ref FocusWindow);
            GetFuncPointer(lib, "glfwGetWindowMonitor", ref GetWindowMonitor);
            GetFuncPointer(lib, "glfwSetWindowMonitor", ref SetWindowMonitor);
            GetFuncPointer(lib, "glfwGetWindowAttrib", ref GetWindowAttrib);
            GetFuncPointer(lib, "glfwSetWindowUserPointer", ref SetWindowUserPointer);
            GetFuncPointer(lib, "glfwGetWindowUserPointer", ref GetWindowUserPointer);
            GetFuncPointer(lib, "glfwSetWindowPosCallback", ref SetWindowPosCallback);
            GetFuncPointer(lib, "glfwSetWindowSizeCallback", ref SetWindowSizeCallback);
            GetFuncPointer(lib, "glfwSetWindowCloseCallback", ref SetWindowCloseCallback);
            GetFuncPointer(lib, "glfwSetWindowRefreshCallback", ref SetWindowRefreshCallback);
            GetFuncPointer(lib, "glfwSetWindowFocusCallback", ref SetWindowFocusCallback);
            GetFuncPointer(lib, "glfwSetWindowIconifyCallback", ref SetWindowIconifyCallback);
            GetFuncPointer(lib, "glfwSetFramebufferSizeCallback", ref SetFramebufferSizeCallback);
            GetFuncPointer(lib, "glfwPollEvents", ref PollEvents);
            GetFuncPointer(lib, "glfwWaitEvents", ref WaitEvents);
            GetFuncPointer(lib, "glfwWaitEvents", ref WaitEvents);
            GetFuncPointer(lib, "glfwPostEmptyEvent", ref PostEmptyEvent);
            GetFuncPointer(lib, "glfwGetInputMode", ref GetInputMode);
            GetFuncPointer(lib, "glfwSetInputMode", ref SetInputMode);
            GetFuncPointer(lib, "glfwGetKeyName", ref _getKeyName);
            GetFuncPointer(lib, "glfwGetKey", ref GetKey);
            GetFuncPointer(lib, "glfwGetMouseButton", ref GetMouseButton);
            GetFuncPointer(lib, "glfwGetCursorPos", ref GetCursorPos);
            GetFuncPointer(lib, "glfwSetCursorPos", ref SetCursorPos);
            GetFuncPointer(lib, "glfwCreateCursor", ref _createCursor);
            GetFuncPointer(lib, "glfwCreateStandardCursor", ref CreateStandardCursor);
            GetFuncPointer(lib, "glfwDestroyCursor", ref DestroyCursor);
            GetFuncPointer(lib, "glfwSetCursor", ref SetCursor);
            GetFuncPointer(lib, "glfwSetKeyCallback", ref SetKeyCallback);
            GetFuncPointer(lib, "glfwSetCharCallback", ref SetCharCallback);
            GetFuncPointer(lib, "glfwSetCharModsCallback", ref SetCharModsCallback);
            GetFuncPointer(lib, "glfwSetMouseButtonCallback", ref SetMouseButtonCallback);
            GetFuncPointer(lib, "glfwSetCursorPosCallback", ref SetCursorPosCallback);
            GetFuncPointer(lib, "glfwSetCursorEnterCallback", ref SetCursorEnterCallback);
            GetFuncPointer(lib, "glfwSetScrollCallback", ref SetScrollCallback);
            GetFuncPointer(lib, "glfwSetDropCallback", ref _setDropCallback);
            GetFuncPointer(lib, "glfwJoystickPresent", ref JoystickPresent);
            GetFuncPointer(lib, "glfwGetJoystickAxes", ref _getJoystickAxes);
            GetFuncPointer(lib, "glfwGetJoystickButtons", ref _getJoystickButtons);
            GetFuncPointer(lib, "glfwGetJoystickName", ref _getJoystickName);
            GetFuncPointer(lib, "glfwSetJoystickCallback", ref SetJoystickCallback);
            GetFuncPointer(lib, "glfwSetClipboardString", ref SetClipboardString);
            GetFuncPointer(lib, "glfwGetClipboardString", ref _getClipboardString);
            GetFuncPointer(lib, "glfwGetTime", ref GetTime);
            GetFuncPointer(lib, "glfwSetTime", ref SetTime);
            GetFuncPointer(lib, "glfwGetTimerValue", ref GetTimerValue);
            GetFuncPointer(lib, "glfwGetTimerFrequency", ref GetTimerFrequency);
            GetFuncPointer(lib, "glfwMakeContextCurrent", ref MakeContextCurrent);
            GetFuncPointer(lib, "glfwGetCurrentContext", ref GetCurrentContext);
            GetFuncPointer(lib, "glfwSwapBuffers", ref SwapBuffers);
            GetFuncPointer(lib, "glfwSwapInterval", ref SwapInterval);
            GetFuncPointer(lib, "glfwExtensionSupported", ref ExtensionSupported);
            GetFuncPointer(lib, "glfwGetProcAddress", ref GetProcAddress);
            GetFuncPointer(lib, "glfwVulkanSupported", ref VulkanSupported);
            GetFuncPointer(lib, "glfwGetRequiredInstanceExtensions", ref _getRequiredInstanceExtensions);
            //GetFuncPointer(lib, "glfwJoystickIsGamepad", ref JoystickIsGamepad);
            //GetFuncPointer(lib, "glfwGetGamepadName", ref GetGamepadName);
            //GetFuncPointer(lib, "glfwGetGamepadState", ref GetGamepadState);

            // Run glfwInit.
            return _glfwInit();
        }

        /// <summary>
        /// Load a function from the library.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type to load the function as.</typeparam>
        /// <param name="lib">Pointer to the library.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="funcDel">The delegate variable which to load the function into.</param>
        private static void GetFuncPointer<TDelegate>(IntPtr lib, string name, ref TDelegate funcDel)
        {
            IntPtr func = Kernel32Methods.GetProcAddress(lib, name);
            if (func != IntPtr.Zero) funcDel = Marshal.GetDelegateForFunctionPointer<TDelegate>(func);
        }

        /// <summary>
        ///     <para>
        ///     This function returns the compile-time generated version string of the GLFW
        ///     library binary. It describes the version, platform, compiler and any platform-specific
        ///     compile-time options. It should not be confused with the OpenGL or OpenGL ES version
        ///     string, queried with <c>glGetString</c>.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use the version string</strong> to parse the GLFW library version.
        ///     The <see cref="GetVersion" /> function provides the version of
        ///     the running library binary in numerical format.
        ///     </para>
        /// </summary>
        /// <returns>The ASCII encoded GLFW version string.</returns>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        /// <seealso cref="GetVersion" />
        public static string GetVersionString()
        {
            return Marshal.PtrToStringAnsi(_getVersionString());
        }

        /// <summary>
        /// This function returns an array of handles for all currently connected monitors. The
        /// primary monitor is always first in the returned array. If no monitors were found, this
        /// function returns <c>null</c>.
        /// </summary>
        /// <returns>
        /// An array of monitor handles, or <c>null</c> if no monitors were found or if an
        /// error occurred.
        /// </returns>
        /// <seealso cref="GetPrimaryMonitor" />
        public static IntPtr[] GetMonitors(out int count)
        {
            IntPtr rawMonitors = _getMonitors(out int num);
            count = num;
            IntPtr[] monitors = new IntPtr[num];
            Marshal.Copy(rawMonitors, monitors, 0, num);
            return monitors;
        }

        /// <summary>
        /// This function returns a human-readable name, encoded as UTF-8, of the specified monitor.
        /// The name typically reflects the make and model of the monitor and is not guaranteed to
        /// be unique among the connected monitors.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The UTF-8 encoded name of the monitor, or <c>null</c> if an error</returns>
        public static string GetMonitorName(IntPtr monitor)
        {
            return Marshal.PtrToStringAnsi(_getMonitorName(monitor));
        }

        /// <summary>
        /// This function returns an array of all video modes supported by the specified monitor.
        /// The returned array is sorted in ascending order, first by color bit depth (the sum of
        /// all channel depths) and then by resolution area (the product of width and height).
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="count">The number of supported video modes.</param>
        /// <returns>An array of video modes, or <c>null</c> if an error occurred.</returns>
        /// <seealso cref="GetVideoMode(IntPtr)" />
        public static VidMode[] GetVideoModes(IntPtr monitor, out int count)
        {
            IntPtr rawVidModes = _getVideoModes(monitor, out int num);
            count = num;
            VidMode[] vidModes = new VidMode[num];
            for (int i = 0; i < vidModes.Length; i++)
            {
                vidModes[i] = Marshal.PtrToStructure<VidMode>(rawVidModes + i * Marshal.SizeOf<VidMode>());
            }

            return vidModes;
        }

        /// <summary>
        /// This function returns the current video mode of the specified monitor. If you have
        /// created a full screen window for that monitor, the return value will depend on whether
        /// that window is iconified.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current mode of the monitor, or <c>null</c> if an error occurred.</returns>
        /// <seealso cref="GetVideoModes(IntPtr, out int)" />
        public static VidMode GetVideoMode(IntPtr monitor)
        {
            return Marshal.PtrToStructure<VidMode>(_getVideoMode(monitor));
        }

        /// <summary>
        /// This function returns the current gamma ramp of the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current gamma ramp, or <c>null</c> if an error occurred.</returns>
        public static GammaRamp GetGammaRamp(IntPtr monitor)
        {
            unsafe
            {
                UnmanagedGammaRamp* internalRamp = _getGammaRamp(monitor);

                GammaRamp ramp = new GammaRamp
                {
                    Red = new ushort[internalRamp->size],
                    Green = new ushort[internalRamp->size],
                    Blue = new ushort[internalRamp->size],
                    Size = internalRamp->size
                };

                for (uint i = 0; i < ramp.Size; i++)
                {
                    ramp.Red[i] = internalRamp->red[i];
                    ramp.Green[i] = internalRamp->green[i];
                    ramp.Blue[i] = internalRamp->blue[i];
                }

                return ramp;
            }
        }

        /// <summary>
        /// This function sets the current gamma ramp for the specified monitor. The original gamma
        /// ramp for that monitor is saved by GLFW the first time this function is called and is
        /// restored by <see cref="Terminate" />.
        /// </summary>
        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="ramp">The gamma ramp to use.</param>
        /// <remarks>
        /// Gamma ramp sizes other than 256 are not supported by all platforms or graphics hardware
        /// (<strong>Win32</strong> requires a 256 gamma ramp size).
        /// </remarks>
        public static void SetGammaRamp(IntPtr monitor, ref GammaRamp ramp)
        {
            unsafe
            {
                fixed (ushort* rampRed = ramp.Red, rampGreen = ramp.Green, rampBlue = ramp.Blue)
                {
                    UnmanagedGammaRamp internalRamp = new UnmanagedGammaRamp {red = rampRed, green = rampGreen, blue = rampBlue, size = ramp.Size};
                    _setGammaRamp(monitor, new IntPtr(&internalRamp));
                }
            }
        }

        /// <summary>
        ///     <para>This function sets the icon of the specified window.</para>
        ///     <para>
        ///     The desired image sizes varies depending on platform and system settings. The
        ///     selected images will be rescaled as needed. Good sizes include 16x16, 32x32 and
        ///     48x48.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose icon to set.</param>
        /// <param name="count">The number of images to create the icon from.</param>
        /// <param name="images">The images to create the icon from.</param>
        /// <remarks>
        /// <strong>OSX:</strong> The GLFW window has no icon, as it is not a document window, so
        /// this function does nothing. The dock icon will be the same as the application bundle's
        /// icon. For more information on bundles, see the
        /// <a href="https://developer.apple.com/library/mac/documentation/CoreFoundation/Conceptual/CFBundles/">
        /// Bundle Programming Guide
        /// </a>
        /// in the Mac Developer Library.
        /// </remarks>
        public static void SetWindowIcon(IntPtr window, int count, Image[] images)
        {
            unsafe
            {
                UnmanagedImage[] internalImages = new UnmanagedImage[images.Length];
                for (int i = 0; i < internalImages.Length; i++)
                {
                    fixed (byte* pixels = images[i].Pixels)
                    {
                        internalImages[i] = new UnmanagedImage {width = images[i].Width, height = images[i].Height, pixels = pixels};
                    }
                }

                fixed (UnmanagedImage* arrayPtr = internalImages)
                {
                    _setWindowIcon(window, count, new IntPtr(arrayPtr));
                }
            }
        }

        /// <summary>
        ///     <para>
        ///     This function returns the localized name of the specified printable key. This is
        ///     intended for displaying key bindings to the user.
        ///     </para>
        ///     <para>
        ///     If the key is <see cref="KeyUnknown" />, the scancode is used instead,
        ///     otherwise the scancode is ignored. If a non-printable key or (if the key is
        ///     <see cref="KeyUnknown" />) a scancode that maps to a non-printable key is specified,
        ///     this function returns <c>null</c>.
        ///     </para>
        ///     <para>This behavior allows you to pass in the arguments passed to the key callback without modification.</para>
        ///     <para>The printable keys are:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="KeyApostrophe" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyComma" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyMinus" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyPeriod" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeySlash" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeySemicolon" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyEqual" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyLeftBracket" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyRightBracket" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyBackslash" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyWorld1" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyWorld2" />
        ///         </item>
        ///         <item><see cref="Key0" /> to <see cref="Key9" /></item>
        ///         <item><see cref="KeyA" /> to <see cref="KeyZ" /></item>
        ///         <item><see cref="KeyKp0" /> to <see cref="KeyKp9" /></item>
        ///         <item>
        ///             <see cref="KeyKpDecimal" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyKpDivide" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyKpMultiply" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyKpSubtract" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyKpAdd" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyKpEqual" />
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="key">The key to query, or <see cref="KeyUnknown" />.</param>
        /// <param name="scancode">The scancode of the key to query.</param>
        /// <returns>The localized name of the key, or <c>null</c>.</returns>
        public static string GetKeyName(int key, int scancode)
        {
            return Marshal.PtrToStringAnsi(_getKeyName(key, scancode));
        }

        /// <summary>
        ///     <para>
        ///     Creates a new custom cursor image that can be set for a window with
        ///     <see cref="SetCursor" />. The cursor can be destroyed with
        ///     <see cref="DestroyCursor" />. Any remaining cursors are destroyed by
        ///     <see cref="Terminate" />.
        ///     </para>
        ///     <para>
        ///     The pixels are 32-bit, little-endian, non-premultiplied RGBA, i.e. eight bits per
        ///     channel.  They are arranged canonically as packed sequential rows, starting from the
        ///     top-left corner.
        ///     </para>
        ///     <para>
        ///     The cursor hotspot is specified in pixels, relative to the upper-left corner of
        ///     the cursor image. Like all other coordinate systems in GLFW, the X-axis points to the
        ///     right and the Y-axis points down.
        ///     </para>
        /// </summary>
        /// <param name="image">The desired cursor image.</param>
        /// <param name="xhot">The desired x-coordinate, in pixels, of the cursor hotspot.</param>
        /// <param name="yhot">The desired y-coordinate, in pixels, of the cursor hotspot.</param>
        /// <returns>The handle of the created cursor.</returns>
        /// <seealso cref="DestroyCursor" />
        /// <seealso cref="CreateStandardCursor" />
        public static IntPtr CreateCursor(Image image, int xhot, int yhot)
        {
            unsafe
            {
                fixed (byte* pixels = image.Pixels)
                {
                    UnmanagedImage internalImage = new UnmanagedImage {width = image.Width, height = image.Height, pixels = pixels};
                    return _createCursor(new IntPtr(&internalImage), xhot, yhot);
                }
            }
        }

        private static UnmanagedDropFun _currentDropFun;

        /// <summary>
        /// This function sets the file drop callback of the specified window, which is called when
        /// one or more dragged files are dropped on the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="cbfun">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetDropCallback(IntPtr window, DropFun cbfun)
        {
            unsafe
            {
                void DropFun(IntPtr win, int count, sbyte** paths)
                {
                    string[] pathArray = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        pathArray[i] = new string(*(paths + i));
                    }

                    cbfun(win, count, pathArray);
                }

                //Prevents DropFun from being garbage collected
                _currentDropFun = DropFun;

                _setDropCallback(window, _currentDropFun);
            }
        }

        /// <summary>
        ///     <para>
        ///     This function returns the values of all axes of the specified joystick. Each
        ///     element in the array is a value between -1.0 and 1.0.
        ///     </para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <param name="count">The number of axis values.</param>
        /// <returns>
        /// An array of axis values, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static float[] GetJoystickAxes(int joy, out int count)
        {
            unsafe
            {
                float* arrayPtr = _getJoystickAxes(joy, out count);
                float[] axes = new float[count];

                for (int i = 0; i < count; i++)
                {
                    axes[i] = *arrayPtr;
                    arrayPtr++;
                }

                return axes;
            }
        }

        /// <summary>
        ///     <para>This function returns the state of all buttons of the specified joystick.</para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <param name="count">The number of button states.</param>
        /// <returns>
        /// An array of button states, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static byte[] GetJoystickButtons(int joy, out int count)
        {
            unsafe
            {
                byte* arrayPtr = _getJoystickButtons(joy, out count);
                byte[] buttonStates = new byte[count];

                for (int i = 0; i < count; i++)
                {
                    buttonStates[i] = *arrayPtr;
                    arrayPtr++;
                }

                return buttonStates;
            }
        }

        /// <summary>
        ///     <para>
        ///     This function returns the name, encoded as UTF-8, of the specified
        ///     joystick.
        ///     </para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <returns>
        /// The UTF-8 encoded name of the joystick, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static string GetJoystickName(int joy)
        {
            return Marshal.PtrToStringAnsi(_getJoystickName(joy));
        }

        /// <summary>
        /// This function returns the contents of the system clipboard, if it contains or is
        /// convertible to a UTF-8 encoded string. If the clipboard is empty or if its contents
        /// cannot be converted, <c>null</c> is returned and a
        /// <see cref="ErrorFormatUnavailable" /> error is generated.
        /// </summary>
        /// <param name="window">The window that will request the clipboard contents.</param>
        /// <returns>
        /// The contents of the clipboard as a UTF-8 encoded string, or <c>null</c> if an
        /// error occurred.
        /// </returns>
        /// <seealso cref="SetClipboardString" />
        public static string GetClipboardString(IntPtr window)
        {
            return Marshal.PtrToStringAnsi(_getClipboardString(window));
        }

        public static string[] GetRequiredInstanceExtensions(out uint count)
        {
            IntPtr arrayPtr = _getRequiredInstanceExtensions(out count);
            string[] requiredInstanceExtensions = new string[count];

            for (int i = 0; i < count; i++)
            {
                requiredInstanceExtensions[i] = Marshal.PtrToStringAnsi(arrayPtr + i);
            }

            return requiredInstanceExtensions;
        }

        #endregion
    }
}