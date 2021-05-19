#region Using

using System;

#endregion

#pragma warning disable 1591 // No need to document enum members here
namespace Emotion.Platform.Input
{
    /// <summary>
    /// Input keys, forked from GLFW
    /// These key codes are inspired by the _USB HID Usage Tables v1.12_ (p. 53-60),
    /// but re-arranged to map to 7-bit ASCII for printable keys (function keys are put in the 256+ range).
    /// The naming of the key codes follow these rules:
    /// - The US keyboard layout is used
    /// - Names of printable alpha-numeric characters are used (e.g. "A", "R", "3", etc.)
    /// - For non-alphanumeric characters, camel case is used.
    /// - Keys that lack a clear US mapping are named "WORLD_x"
    /// - For non-printable keys, custom names are used (e.g. "F4", "BACKSPACE", etc.)
    /// - Mouse keys and gamepad keys are included.
    /// - Commonly used input axes are used as well.
    /// </summary>
    [Flags]
    public enum Key : short
    {
        Unknown = -1,

        KeyboardStart = 31,

        Space = 32,
        Apostrophe = 39, // '
        Comma = 44, // ,
        Minus = 45, // -
        Period = 46, // .
        Slash = 47, // /
        Num0 = 48,
        Num1 = 49,
        Num2 = 50,
        Num3 = 51,
        Num4 = 52,
        Num5 = 53,
        Num6 = 54,
        Num7 = 55,
        Num8 = 56,
        Num9 = 57,
        Semicolon = 59, // ;
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
        World1 = 161, // non-US, #1
        World2 = 162, // non-US, #2

        // Function keys
        Escape = 256,
        Enter = 257,
        Tab = 258,
        Backspace = 259,
        Insert = 260,
        Delete = 261,
        RightArrow = 262,
        LeftArrow = 263,
        DownArrow = 264,
        UpArrow = 265,
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

        // Keypad
        Kp0 = 320,
        Kp1 = 321,
        Kp2 = 322,
        Kp3 = 323,
        Kp4 = 324,
        Kp5 = 325,
        Kp6 = 326,
        Kp7 = 327,
        Kp8 = 328,
        Kp9 = 329,
        KpDecimal = 330,
        KpDivide = 331,
        KpMultiply = 332,
        KpSubtract = 333,
        KpAdd = 334,
        KpEnter = 335,
        KpEqual = 336,

        // System
        LeftShift = 340,
        LeftControl = 341,
        LeftAlt = 342,
        LeftSuper = 343,
        RightShift = 344,
        RightControl = 345,
        RightAlt = 346,
        RightSuper = 347,
        Menu = 348, // Windows key

        KeyboardLast = 349,

        // Non keyboard keys, these go after the last so that keyboard iteration over key codes can be maintained.

        // Mouse
        MouseKeyStart = 350,
        MouseKeyLeft = 351,
        MouseKeyRight = 352,
        MouseKeyMiddle = 353,
        MouseKey4 = 354,
        MouseKey5 = 355,
        MouseKeyEnd = 356,

        // XInput Gamepad
        GamepadStart = 357,
        GamepadY = 358,
        GamepadB = 359,
        GamepadA = 360,
        GamepadX = 361,
        GamepadStartButton = 362,
        GamepadSelect = 363,
        GamepadSpecial = 364,
        GamepadL1 = 365,
        GamepadL2Button = 366,
        GamepadR1 = 367,
        GamepadR2Button = 368,
        GamepadDPadUp = 369,
        GamepadDPadRight = 370,
        GamepadDPadDown = 371,
        GamepadDPadLeft = 372,
        GamepadL3 = 373,
        GamepadR3 = 374,

        GamepadDPadAxis = 375,
        GamepadLeftAxis = 376,
        GamepadRightAxis = 377,
        GamepadL2Axis = 378,
        GamepadR2Axis = 379,
        GamepadEnd = 380,

        Last = 381,

        // Axes (These will be ORed so they need to be powers of two)
        AxisLeftRight = 512,
        AxisUpDown = 1024,

        AxisAD = 2048,
        AxisWS = 4096,

        AxisArrowKeys = AxisLeftRight | AxisUpDown,
        AxisWASD = AxisAD | AxisWS,
        AxisPCMovement = AxisWASD | AxisArrowKeys,
    }
}