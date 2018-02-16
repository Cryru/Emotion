// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

#endregion

#region Using

using System;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

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
            get => _wwidth;
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
            get => _wheight;
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
            get => _wtitle;
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
        public static DisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                _displayMode = value;

                WindowManager.UpdateWindow();
            }
        }

        private static DisplayMode _displayMode = DisplayMode.Windowed;

        #endregion

        #region Drawing Settings

        /// <summary>
        /// The width of the render area. Must be set before setup.
        /// </summary>
        public static int Width = 960;

        /// <summary>
        /// The height of the render area. Must be set before setup.
        /// </summary>
        public static int Height = 540;

        /// <summary>
        /// The FPS target.
        /// </summary>
        public static int FPS = 0;

        /// <summary>
        /// The TPS target.
        /// </summary>
        public static int TPS = 60;

        /// <summary>
        /// Whether vertical synchronization is enabled.
        /// </summary>
        public static bool VSync
        {
            get => _vSync;
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
            get => _renderMouse;
            set
            {
                _renderMouse = value;

                ApplySettings();
            }
        }

        private static bool _renderMouse = true;

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
            Core.Context.IsMouseVisible = RenderMouse;
            Core.Context.IsFixedTimeStep = FPS > 0;
            Core.Context.TargetElapsedTime =
                FPS > 0 ? TimeSpan.FromMilliseconds(Math.Floor(1000f / FPS)) : TimeSpan.FromMilliseconds(1);
            Core.Context.GraphicsManager.SynchronizeWithVerticalRetrace = VSync;
            Core.Context.Window.Title = WTitle;
        }
    }
}