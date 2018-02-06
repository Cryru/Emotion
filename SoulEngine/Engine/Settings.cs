// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using


#endregion

using System;
using OpenTK;
using Breath.Enums;

namespace Soul.Engine
{
    /// <summary>
    /// Engine settings. Most are applied only on Core setup.
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
        /// What mode the window should be in.
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

        private static WindowMode _windowMode = WindowMode.Windowed;

        #endregion

        #region First Run Settings

        /// <summary>
        /// The width of the render area. Must be set before setup.
        /// </summary>
        public static int Width = 960;

        /// <summary>
        /// The height of the render area. Must be set before setup.
        /// </summary>
        public static int Height = 540;

        /// <summary>
        /// The FPS target. Must be set before setup. Leave at 0 - BUG
        /// </summary>
        public static int FPS = 0;

        /// <summary>
        /// The TPS target. Must be set before setup.
        /// </summary>
        public static int TPS = 60;

        #endregion

        #region Drawing Settings

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

        /// <summary>
        /// The time to wait before ending a script.
        /// </summary>
        public static TimeSpan ScriptTimeout = new TimeSpan(0, 0, 1);

        /// <summary>
        /// The scene to use as a loading screen.
        /// </summary>
        public static Type LoadingScene = null;

        #endregion

        /// <summary>
        /// Apply runtime settings.
        /// </summary>
        internal static void ApplySettings()
        {
            if (Core.BreathWin == null) return;

            Core.BreathWin.Width = WWidth;
            Core.BreathWin.Height = WHeight;
            Core.BreathWin.VSync = VSync ? VSyncMode.On : VSyncMode.Off;
            Core.BreathWin.CursorVisible = RenderMouse;
            Core.BreathWin.Title = WTitle;

            Core.BreathWin.ChangeWindowMode(WindowMode);
        }

#if DEBUG

        #region Debug Settings

        /// <summary>
        /// Whether debugging aggressively.
        /// </summary>
        internal static bool AggressiveDebug = true;

        #endregion

#endif
    }
}