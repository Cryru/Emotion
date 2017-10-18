// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Raya.Enums;
using Raya.Primitives;

#endregion

namespace Soul.Engine
{
    /// <summary>
    /// Raya context global variables. Used for settings and others.
    /// </summary>
    public static class Settings
    {
        #region Window Settings

        /// <summary>
        /// The window's width.
        /// </summary>
        public static int WWidth
        {
            get { return _wwidth; }
            set
            {
                _wwidth = value;

                ApplySettings();
            }
        }

        private static int _wwidth = 960;

        /// <summary>
        /// The window's height.
        /// </summary>
        public static int WHeight
        {
            get { return _wheight; }
            set
            {
                _wheight = value;

                ApplySettings();
            }
        }

        private static int _wheight = 540;

        /// <summary>
        /// The window's title.
        /// </summary>
        public static string WTitle
        {
            get { return _wtitle; }
            set
            {
                _wtitle = value;

                ApplySettings();
            }
        }

        private static string _wtitle = "SoulEngine 2018";

        /// <summary>
        /// What mode the window should start in.
        /// </summary>
        public static WindowMode WindowMode
        {
            get { return _windowMode; }
            set
            {
                _windowMode = value;

                ApplySettings();
            }
        }

        private static WindowMode _windowMode = WindowMode.Window;

        #endregion

        #region Drawing Settings

        /// <summary>
        /// The width of the render area.
        /// </summary>
        public static int Width
        {
            get { return _width; }
            set
            {
                _width = value;

                ApplySettings();
            }
        }

        private static int _width = 960;

        /// <summary>
        /// The height of the render area.
        /// </summary>
        public static int Height
        {
            get { return _height; }
            set
            {
                _height = value;

                ApplySettings();
            }
        }

        private static int _height = 540;

        /// <summary>
        /// Whether vertical synchronization is enabled.
        /// </summary>
        public static bool VSync
        {
            get { return _vSync; }
            set
            {
                _vSync = value;

                ApplySettings();
            }
        }

        private static bool _vSync = true;

        /// <summary>
        /// The FPS cap.
        /// </summary>
        public static int FPSCap
        {
            get { return _fpsCap; }
            set
            {
                _fpsCap = value;

                ApplySettings();
            }
        }

        private static int _fpsCap = 60;

        /// <summary>
        /// Whether to render the mouse.
        /// </summary>
        public static bool RenderMouse
        {
            get { return _renderMouse; }
            set
            {
                _renderMouse = value;

                ApplySettings();
            }
        }

        private static bool _renderMouse = true;

        /// <summary>
        /// Whether to keep the mouse inside.
        /// </summary>
        public static bool ConstrainMouse
        {
            get { return _constrainMouse; }
            set
            {
                _constrainMouse = value;

                ApplySettings();
            }
        }

        private static bool _constrainMouse;

        #endregion

        #region Input Settings

        /// <summary>
        /// Whether holding down the key will repeat the key.
        /// </summary>
        public static bool KeyRepeat
        {
            get { return _keyRepeat; }
            set
            {
                _keyRepeat = value;

                ApplySettings();
            }
        }

        private static bool _keyRepeat = true;

        /// <summary>
        /// The joystick axis threshold. Any movements smaller than the threshold will be ignored.
        /// </summary>
        public static int JoystickThreshold
        {
            get { return _joystickThreshold; }
            set
            {
                _joystickThreshold = value;

                ApplySettings();
            }
        }

        private static int _joystickThreshold = 20;

        #endregion

        #region Other Settings

        /// <summary>
        /// Whether to pause the game when the window loses focus.
        /// </summary>
        public static bool PauseOnFocusLoss = false;

        #endregion

        public static void ApplySettings()
        {
            // Check if a native context has been established.
            if (Core.NativeContext == null || Core.NativeContext.Window == null) return;

            Core.NativeContext.Window.Size = new Vector2(Width, Height);
            Core.NativeContext.Window.RenderSize = new Vector2(WWidth, WHeight);
            Core.NativeContext.Window.Title = WTitle;
            Core.NativeContext.Window.Mode = WindowMode;
            Core.NativeContext.Window.VSync = VSync;
            Core.NativeContext.Window.FPSLimit = FPSCap;
            Core.NativeContext.Window.RenderMouse = RenderMouse;
            Core.NativeContext.Window.KeepMouseInside = ConstrainMouse;
            Core.NativeContext.Window.KeyRepeat = KeyRepeat;
            Core.NativeContext.Window.JoystickThreshold = JoystickThreshold;
        }
    }
}