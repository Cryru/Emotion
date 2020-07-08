#region Using

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

#pragma warning disable CS1574
#pragma warning disable CS0419

// ReSharper disable once CheckNamespace
namespace Emotion.Platform.Implementation.GlfwImplementation.Native
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static partial class Glfw
    {
        /// <seealso cref="Hint.ClientApi" />
        public enum ClientApi
        {
            None = 0,
            OpenGL = 0x00030001,
            OpenGLES = 0x00030002
        }

        /// <summary>
        /// Connection events for inputs and monitors.
        /// </summary>
        /// <seealso cref="SetJoystickCallback(JoystickFunc)" />
        /// <seealso cref="SetMonitorCallback(MonitorFunc)" />
        public enum ConnectionEvent
        {
            Connected = 0x00040001,
            Disconnected = 0x00040002
        }

        /// <seealso cref="Hint.ContextCreationApi" />
        public enum ContextApi
        {
            Native = 0x00036001,
            EGL = 0x00036002
        }

        /// <seealso cref="Hint.ContextReleaseBehavior" />
        public enum ContextReleaseBehavior
        {
            Any = 0,
            Flush = 0x00035001,
            None = 0x00035002
        }

        /// <seealso cref="Hint.ContextRobustness" />
        public enum ContextRobustness
        {
            None = 0,
            NoResetNotification = 0x00031001,
            LoseContextOnReset = 0x00031002
        }

        /// <summary>
        /// Cursor modes.
        /// </summary>
        /// <seealso cref="SetInputMode" />
        public enum CursorMode
        {
            /// <summary>
            /// Makes the cursor visible and behaving normally.
            /// </summary>
            Normal = 0x00034001,

            /// <summary>
            /// Makes the cursor invisible when it is over the client area of the window but does
            /// not restrict the cursor from leaving.
            /// </summary>
            Hidden = 0x00034002,

            /// <summary>
            /// Hides and grabs the cursor, providing virtual and unlimited cursor movement. This is
            /// useful for implementing for example 3D camera controls.
            /// </summary>
            Disabled = 0x00034003
        }

        /// <summary>
        /// Standard cursor shapes.
        /// </summary>
        /// <seealso cref="SetCursor" />
        public enum CursorType
        {
            /// <summary>
            /// The regular arrow cursor.
            /// </summary>
            Arrow = 0x00036001,

            /// <summary>
            /// The text input I-beam cursor shape.
            /// </summary>
            Beam = 0x00036002,

            /// <summary>
            /// The crosshair shape.
            /// </summary>
            Crosshair = 0x00036003,

            /// <summary>
            /// The hand shape.
            /// </summary>
            Hand = 0x00036004,

            /// <summary>
            /// The horizontal resize arrow shape.
            /// </summary>
            ResizeX = 0x00036005,

            /// <summary>
            /// The vertical resize arrow shape.
            /// </summary>
            ResizeY = 0x00036006
        }

        /// <summary>
        /// Error codes.
        /// </summary>
        /// <seealso cref="SetErrorCallback" />
        public enum ErrorCode
        {
            /// <summary>
            ///     <para>GLFW has not been initialized.</para>
            ///     <para>
            ///     This occurs if a GLFW function was called that must not be called unless the
            ///     library is initialized.
            ///     </para>
            /// </summary>
            NotInitialized = 0x00010001,

            /// <summary>
            ///     <para>No context is current for this thread.</para>
            ///     <para>
            ///     This occurs if a GLFW function was called that needs and operates on the
            ///     current OpenGL or OpenGL ES context but no context is current on the calling thread.
            ///     One such function is <see cref="SwapInterval" />.
            ///     </para>
            /// </summary>
            NoCurrentContext = 0x00010002,

            /// <summary>
            /// One of the arguments to the function was an invalid enum value, for example
            /// requesting <see cref="Hint.RedBits" /> with
            /// <see cref="GetWindowAttrib(Window, WindowAttrib)" />.
            /// </summary>
            InvalidEnum = 0x00010003,

            /// <summary>
            /// One of the arguments to the function was an invalid value, for example requesting a
            /// non-existent OpenGL or OpenGL ES version like <c>2.7</c>. Requesting a valid but
            /// unavailable OpenGL or OpenGL ES version will instead result in a
            /// <see cref="VersionUnavailable" /> error.
            /// </summary>
            InvalidValue = 0x00010004,

            /// <summary>
            /// A memory allocation failed.
            /// </summary>
            OutOfMemory = 0x00010005,

            /// <summary>
            /// GLFW could not find support for the requested API on the system.
            /// </summary>
            ApiUnavailable = 0x00010006,

            /// <summary>
            /// The requested OpenGL or OpenGL ES version (including any requested context or
            /// framebuffer hints) is not available on this machine.
            /// </summary>
            VersionUnavailable = 0x00010007,

            /// <summary>
            /// A platform-specific error occurred that does not match any of the more specific
            /// categories.
            /// </summary>
            PlatformError = 0x00010008,

            /// <summary>
            ///     <para>The requested format is not supported or available.</para>
            ///     <para>
            ///     If emitted during window creation, the requested pixel format is not
            ///     supported.
            ///     </para>
            ///     <para>
            ///     If emitted when querying the clipboard, the contents of the clipboard could
            ///     not be converted to the requested format.
            ///     </para>
            /// </summary>
            FormatUnavailable = 0x00010009,

            /// <summary>
            /// A window that does not have an OpenGL or OpenGL ES context was passed to a function
            /// that requires it to have one.
            /// </summary>
            NoWindowContext = 0x0001000A
        }

        /// <summary>
        ///     <para>
        ///     These hints are set to their default values each time the library is initialized
        ///     with <see cref="Init" />, can be set individually with
        ///     <see cref="WindowHint(Hint, bool)" /> and reset all at once to their defaults with
        ///     <see cref="DefaultWindowHints" />.
        ///     </para>
        ///     <para>
        ///     Note that hints need to be set before the creation of the window and context you
        ///     wish to have the specified attributes.
        ///     </para>
        /// </summary>
        /// <seealso cref="WindowHint" />
        public enum Hint
        {
            /// <summary>
            /// Specifies whether the windowed mode window will be given input focus when created.
            /// This hint is ignored for full screen and initially hidden windows.
            /// </summary>
            Focused = 0x00020001,

            /// <summary>
            /// Specifies whether the windowed mode window will be resizable by the user. The
            /// window will still be resizable using the
            /// <see cref="SetWindowSize(Window, int, int)" /> function. This hint is ignored for
            /// full screen windows.
            /// </summary>
            Resizable = 0x00020003,

            /// <summary>
            /// Specifies whether the windowed mode window will be initially visible. This hint is
            /// ignored for full screen windows.
            /// </summary>
            Visible = 0x00020004,

            /// <summary>
            /// Specifies whether the windowed mode window will have window decorations such as a
            /// border, a close widget, etc. An undecorated window may still allow the user to
            /// generate close events on some platforms. This hint is ignored for full screen
            /// windows.
            /// </summary>
            Decorated = 0x00020005,

            /// <summary>
            /// Specifies whether the full screen window will automatically iconify and restore the
            /// previous video mode on input focus loss. This hint is ignored for windowed mode
            /// windows.
            /// </summary>
            AutoIconify = 0x00020006,

            /// <summary>
            /// Specifies whether the windowed mode window will be floating above other regular
            /// windows, also called topmost or always-on-top. This is intended primarily for
            /// debugging purposes and cannot be used to implement proper full screen windows. This
            /// hint is ignored for full screen windows.
            /// </summary>
            Floating = 0x00020007,

            /// <summary>
            /// Specifies whether the windowed mode window will be maximized when created. This hint
            /// is ignored for full screen windows.
            /// </summary>
            Maximized = 0x00020008,

            /// <summary>
            /// Specifies the desired bit depths of the red components of the default framebuffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            RedBits = 0x00021001,

            /// <summary>
            /// Specifies the desired bit depths of the green components of the default framebuffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            GreenBits = 0x00021002,

            /// <summary>
            /// Specifies the desired bit depths of the blue components of the default framebuffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            BlueBits = 0x00021003,

            /// <summary>
            /// Specifies the desired bit depths of the alpha components of the default framebuffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            AlphaBits = 0x00021004,

            /// <summary>
            /// Specifies the desired bit depths of the depth components of the default framebuffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            DepthBits = 0x00021005,

            /// <summary>
            /// Specifies the desired bit depths of the stencil components of the default
            /// framebuffer. <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            StencilBits = 0x00021006,

            /// <summary>
            /// Specifies the desired bit depths of the red components of the accumulation buffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            /// <remarks>
            /// Accumulation buffers are a legacy OpenGL feature and should not be used in
            /// new code.
            /// </remarks>
            AccumRedBits = 0x00021007,

            /// <summary>
            /// Specifies the desired bit depths of the green components of the accumulation buffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            /// <remarks>
            /// Accumulation buffers are a legacy OpenGL feature and should not be used in
            /// new code.
            /// </remarks>
            AccumGreenBits = 0x00021008,

            /// <summary>
            /// Specifies the desired bit depths of the blue components of the accumulation buffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            /// <remarks>
            /// Accumulation buffers are a legacy OpenGL feature and should not be used in
            /// new code.
            /// </remarks>
            AccumBlueBits = 0x00021009,

            /// <summary>
            /// Specifies the desired bit depths of the alpha components of the accumulation buffer.
            /// <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            /// <remarks>
            /// Accumulation buffers are a legacy OpenGL feature and should not be used in
            /// new code.
            /// </remarks>
            AccumAlphaBits = 0x0002100a,

            /// <summary>
            /// Specifies the desired number of auxiliary buffers. <see cref="DontCare" /> means the
            /// application has no preference.
            /// </summary>
            /// <remarks>
            /// Auxiliary buffers are a legacy OpenGL feature and should not be used in new
            /// code.
            /// </remarks>
            AuxBuffers = 0x0002100b,

            /// <summary>
            /// Specifies whether to use stereoscopic rendering. This is a hard constraint.
            /// </summary>
            Stereo = 0x0002100c,

            /// <summary>
            /// Specifies the desired number of samples to use for multisampling. Zero disables
            /// multisampling. <see cref="DontCare" /> means the application has no preference.
            /// </summary>
            Samples = 0x0002100d,

            /// <summary>
            /// Specifies whether the framebuffer should be sRGB capable. If supported, a created
            /// OpenGL context will support the <c>GL_FRAMEBUFFER_SRGB</c> enable, also called
            /// <c>GL_FRAMEBUFFER_SRGB_EXT</c>) for controlling sRGB rendering and a created OpenGL
            /// ES context will always have sRGB rendering enabled.
            /// </summary>
            sRGBCapable = 0x0002100e,

            /// <summary>
            /// Specifies whether the framebuffer should be double buffered. You nearly always want
            /// to use double buffering. This is a hard constraint.
            /// </summary>
            Doublebuffer = 0x00021010,

            /// <summary>
            /// Specifies the desired refresh rate for full screen windows. If set to
            /// <see cref="DontCare" />, the highest available refresh rate will be used. This hint
            /// is ignored for windowed mode windows.
            /// </summary>
            RefreshRate = 0x0002100f,

            /// <summary>
            /// Specifies which <see cref="Glfw.ClientApi" /> to create the context for.
            /// </summary>
            ClientApi = 0x00022001,

            /// <summary>
            /// Specifies the client API major version that the created context must be compatible
            /// with. The exact behavior of this hint depends on the requested client API.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///     <b>OpenGL:</b> Creation will fail if the OpenGL version of the created context
            ///     is less than the one requested. It is therefore perfectly safe to use the default of
            ///     version 1.0 for legacy code and you will still get backwards-compatible contexts of
            ///     version 3.0 and above when available. While there is no way to ask the driver for a
            ///     context of the highest supported version, GLFW will attempt to provide this when you
            ///     ask for a version 1.0 context, which is the default for these hints.
            ///     </para>
            ///     <para>
            ///     <b>OpenGL ES:</b> Creation will fail if the OpenGL ES version of the created
            ///     context is less than the one requested. Additionally, OpenGL ES 1.x cannot be
            ///     returned if 2.0 or later was requested, and vice versa. This is because OpenGL ES
            ///     3.x is backward compatible with 2.0, but OpenGL ES 2.0 is not backward compatible
            ///     with 1.x.
            ///     </para>
            /// </remarks>
            /// <seealso cref="ContextVersionMinor" />
            ContextVersionMajor = 0x00022002,

            /// <summary>
            /// Specifies the client API minor version that the created context must be compatible
            /// with. The exact behavior of this hint depends on the requested client API.
            /// </summary>
            /// <seealso cref="ContextVersionMajor" />
            ContextVersionMinor = 0x00022003,

            /// <summary>
            /// Specifies the client API revision version that the created context must be
            /// compatible with. The exact behavior of this hint depends on the requested client
            /// API.
            /// </summary>
            ContextRevision = 0x00022004,

            /// <summary>
            /// Specifies the <see cref="Glfw.ContextRobustness" /> to be used by the context.
            /// </summary>
            ContextRobustness = 0x00022005,

            /// <summary>
            /// Specifies whether the OpenGL context should be forward-compatible, i.e. one where
            /// all functionality deprecated in the requested version of OpenGL is removed. This
            /// must only be used if the requested OpenGL version is 3.0 or above. If OpenGL ES is
            /// requested, this hint is ignored.
            /// </summary>
            OpenglForwardCompat = 0x00022006,

            /// <summary>
            /// Specifies whether to create a debug OpenGL context, which may have additional error
            /// and performance issue reporting functionality. If OpenGL ES is requested, this hint
            /// is ignored.
            /// </summary>
            OpenglDebugContext = 0x00022007,

            /// <summary>
            /// Specifies which <see cref="OpenGLProfile" /> to create the context for. If requesting
            /// an OpenGL version below 3.2, <see cref="OpenGLProfile.Any" /> must be used. If OpenGL
            /// ES is requested, this hint is ignored.
            /// </summary>
            OpenglProfile = 0x00022008,

            /// <summary>
            /// Specifies the <see cref="Glfw.ContextReleaseBehavior" /> to be used by the context.
            /// If the behavior is <see cref="ContextReleaseBehavior.Any" />, the default behavior of
            /// the context creation API will be used. If the behavior is
            /// <see cref="ContextReleaseBehavior.Flush" />, the pipeline will be flushed whenever
            /// the context is released from being the current one. If the behavior is
            /// <see cref="ContextReleaseBehavior.None" />, the pipeline will not be flushed on
            /// release.
            /// </summary>
            ContextReleaseBehavior = 0x00022009,

            /// <summary>
            /// Specifies whether errors should be generated by the context. If enabled, situations
            /// that would have generated errors instead cause undefined behavior.
            /// </summary>
            ContextNoError = 0x0002200a,

            /// <summary>
            /// Specifies which <see cref="ContextApi" /> to use to create the context. This is a
            /// hard constraint. If no client API is requested, this hint is ignored.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///     <b>OSX:</b> The EGL API is not available on this platform and requests to use
            ///     it will fail.
            ///     </para>
            ///     <para>
            ///     <b>Wayland, Mir:</b> The EGL API is the native context creation API, so this
            ///     hint will have no effect.
            ///     </para>
            /// </remarks>
            ContextCreationApi = 0x0002200b
        }

        /// <summary>
        /// Input mode options.
        /// </summary>
        /// <seealso cref="SetInputMode" />
        public enum InputMode
        {
            Cursor = 0x00033001,
            StickyKeys = 0x00033002,
            StickyMouseButton = 0x00033003
        }

        /// <summary>
        /// Key and button actions.
        /// </summary>
        public enum InputState
        {
            /// <summary>
            /// The key or mouse button was released.
            /// </summary>
            Release = 0,

            /// <summary>
            /// The key or mouse button was pressed.
            /// </summary>
            Press = 1,

            /// <summary>
            /// The key was held down until it repeated.
            /// </summary>
            Repeat = 2
        }

        /// <summary>
        /// Joysticks.
        /// </summary>
        public enum Joystick
        {
            Joystick1 = 0,
            Joystick2 = 1,
            Joystick3 = 2,
            Joystick4 = 3,
            Joystick5 = 4,
            Joystick6 = 5,
            Joystick7 = 6,
            Joystick8 = 7,
            Joystick9 = 8,
            Joystick10 = 9,
            Joystick11 = 10,
            Joystick12 = 11,
            Joystick13 = 12,
            Joystick14 = 13,
            Joystick15 = 14,
            Joystick16 = 15,
            JoystickLast = Joystick16
        }

        /// <summary>
        ///     <para>Keyboard keys.</para>
        ///     <para>
        ///     These key codes are inspired by the USB HID Usage Tables v1.12 (p. 53-60), but
        ///     re-arranged to map to 7-bit ASCII for printable keys(function keys are put in the 256+
        ///     range).
        ///     </para>
        /// </summary>
        public enum KeyCode
        {
            Unknown = -1,

            // Printable keys
            Space = 32,
            Apostrophe = 39, // '
            Comma = 44, // ,
            Minus = 45, // -
            Period = 46, // .
            Slash = 47, // /
            Alpha0 = 48,
            Alpha1 = 49,
            Alpha2 = 50,
            Alpha3 = 51,
            Alpha4 = 52,
            Alpha5 = 53,
            Alpha6 = 54,
            Alpha7 = 55,
            Alpha8 = 56,
            Alpha9 = 57,
            SemiColon = 59, // ;
            Equal = 61, // =
            A = 65,
            B = 66,
            C = 67,
            D = 68,
            E = 69,
            F = 70,
            G = 71,
            H = 72,
            I = 73,
            J = 74,
            K = 75,
            L = 76,
            M = 77,
            N = 78,
            O = 79,
            P = 80,
            Q = 81,
            R = 82,
            S = 83,
            T = 84,
            U = 85,
            V = 86,
            W = 87,
            X = 88,
            Y = 89,
            Z = 90,
            LeftBracket = 91, // [
            Backslash = 92, // \
            RightBracket = 93, // ]
            GraveAccent = 96, // `
            World1 = 161, // Non-US #1
            World2 = 162, // Non-US #2

            // Function keys
            Escape = 256,
            Enter = 257,
            Tab = 258,
            Backspace = 259,
            Insert = 260,
            Delete = 261,
            Right = 262,
            Left = 263,
            Down = 264,
            Up = 265,
            PageUp = 266,
            PageDown = 267,
            Home = 268,
            End = 269,
            CapsLock = 280,
            ScrollLock = 281,
            NumLock = 282,
            PrintScreen = 283,
            Pause = 284,
            F1 = 290,
            F2 = 291,
            F3 = 292,
            F4 = 293,
            F5 = 294,
            F6 = 295,
            F7 = 296,
            F8 = 297,
            F9 = 298,
            F10 = 299,
            F11 = 300,
            F12 = 301,
            F13 = 302,
            F14 = 303,
            F15 = 304,
            F16 = 305,
            F17 = 306,
            F18 = 307,
            F19 = 308,
            F20 = 309,
            F21 = 310,
            F22 = 311,
            F23 = 312,
            F24 = 313,
            F25 = 314,
            Numpad0 = 320,
            Numpad1 = 321,
            Numpad2 = 322,
            Numpad3 = 323,
            Numpad4 = 324,
            Numpad5 = 325,
            Numpad6 = 326,
            Numpad7 = 327,
            Numpad8 = 328,
            Numpad9 = 329,
            NumpadDecimal = 330,
            NumpadDivide = 331,
            NumpadMultiply = 332,
            NumpadSubtract = 333,
            NumpadAdd = 334,
            NumpadEnter = 335,
            NumpadEqual = 336,
            LeftShift = 340,
            LeftControl = 341,
            LeftAlt = 342,
            LeftSuper = 343,
            RightShift = 344,
            RightControl = 345,
            RightAlt = 346,
            RightSuper = 347,
            Menu = 348
        }

        /// <summary>
        /// Modifier key flags.
        /// </summary>
        [Flags]
        public enum KeyMods
        {
            /// <summary>
            /// If this bit is set one or more Shift keys were held down.
            /// </summary>
            Shift = 0x0001,

            /// <summary>
            /// If this bit is set one or more Control keys were held down.
            /// </summary>
            Control = 0x0002,

            /// <summary>
            /// If this bit is set one or more Alt keys were held down.
            /// </summary>
            Alt = 0x0004,

            /// <summary>
            /// If this bit is set one or more Super keys were held down.
            /// </summary>
            Super = 0x0008
        }

        /// <summary>
        /// Mouse buttons.
        /// </summary>
        public enum MouseButton
        {
            Button1 = 0,
            Button2 = 1,
            Button3 = 2,
            Button4 = 3,
            Button5 = 4,
            Button6 = 5,
            Button7 = 6,
            Button8 = 7,
            ButtonLast = Button8,
            ButtonLeft = Button1,
            ButtonRight = Button2,
            ButtonMiddle = Button3
        }

        /// <seealso cref="Hint.OpenglProfile" />
        public enum OpenGLProfile
        {
            Any = 0,
            Core = 0x00032001,
            Compat = 0x00032002
        }

        public enum WindowAttrib
        {
            /// <summary>
            /// Indicates whether the specified window has input focus. Initial input focus is
            /// controlled by the window hint with the same name.
            /// </summary>
            Focused = 0x00020001,

            /// <summary>
            /// Indicates whether the specified window is iconified, whether by the user or with
            /// <see cref="IconifyWindow(Window)" />.
            /// </summary>
            Iconified = 0x00020002,

            /// <summary>
            /// Indicates whether the specified window is maximized, whether by the user or with
            /// <see cref="MaximizeWindow(Window)" />.
            /// </summary>
            Maximized = 0x00020008,

            /// <summary>
            /// Indicates whether the specified window is visible. Window visibility can be
            /// controlled with <see cref="ShowWindow(Window)" /> and
            /// <see cref="HideWindow(Window)" /> and initial visibility is controlled by the
            /// <see cref="Hint" /> with the same name.
            /// </summary>
            Visible = 0x00020004,

            /// <summary>
            /// Indicates whether the specified window is resizable by the user. This is set on
            /// creation with the <see cref="Hint" /> with the same name.
            /// </summary>
            Resizable = 0x00020003,

            /// <summary>
            /// Indicates whether the specified window has decorations such as a border, a close
            /// widget, etc. This is set on creation with the <see cref="Hint" /> with the same name.
            /// </summary>
            Decorated = 0x00020005,

            /// <summary>
            /// Indicates whether the specified window is floating, also called topmost or
            /// always-on-top. This is controlled by the <see cref="Hint" /> with the same name.
            /// </summary>
            Floating = 0x00020007
        }
    }
}