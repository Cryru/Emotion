using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulEngine.Enums;
using System.Collections.Generic;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The engine's user settings.
    /// </summary>
    public partial class Settings
    {
        #region "Drawing Settings"
        /// <summary>
        /// The FPS target, if below 0 the FPS isn't capped.
        /// </summary>
        public static float FPS = -1;
        /// <summary>
        /// Whether to synchronize the FPS to the screen's refresh rate. 
        /// Can be overwriten by GPU options.
        /// </summary>
        public static bool vSync = false;
        #endregion
        #region "Security Settings"
        /// <summary>
        /// The key that will be used to encrypt and decrypt files.
        /// </summary>
        public static string SecurityKey = "standardkey";
        /// <summary>
        /// If true, the engine will not run unless the meta.soul file exists and is correct. About 100ms slowdown.
        /// </summary>
        public static bool EnforceAssetIntegrity = false;
        /// <summary>
        /// The MD5 hash of the meta.soul file.
        /// </summary>
        public static string MetaMD5 = "";
        #endregion
        #region "Window Settings"
        /// <summary>
        /// The width of the window.
        /// </summary>
        public static int WWidth = 960;
        /// <summary>
        /// The height of the window.
        /// </summary>
        public static int WHeight = 540;
        /// <summary>
        /// The width the game will be rendered at.
        /// </summary>
        public static int Width = 960;
        /// <summary>
        /// The height the game will be rendered at.
        /// </summary>
        public static int Height = 540;
        /// <summary>
        /// The name of the window.
        /// </summary>
        public static string WName = "SoulEngine 2017";
        /// <summary>
        /// The way the engine should be displayed, the RefreshScreenSettings function must be used to apply changes.
        /// </summary>
        private static DisplayMode _DisplayMode = DisplayMode.Windowed;
        /// <summary>
        /// Whether the mouse should be rendered.
        /// </summary>
        public static bool RenderMouse = true;
        /// <summary>
        /// Whether the window can be resized, currently disables display modes other than windowed.
        /// </summary>
        public static bool ResizableWindow = false;
        #endregion
        #region "Other Settings"
        /// <summary>
        /// The color the screen will be cleared with.
        /// </summary>
        public static Color FillColor = Color.CornflowerBlue;
        /// <summary>
        /// Whether an external settings file should be loaded.
        /// </summary>
        public static bool settingsLoad = false;
        /// <summary>
        /// Whether to apply anti-aliasing.
        /// </summary>
        public static bool AntiAlias = false;
        /// <summary>
        /// Whether to stop drawing and updating when the game is not focused.
        /// </summary>
        public static bool PauseOnFocusLoss = true;
        /// <summary>
        /// Whether Jint scripting is enabled. Raises memory usage.
        /// </summary>
        public static bool Scripting = true;
        #endregion
        #region "Sound Settings"
        /// <summary>
        /// Whether sound is on or off.
        /// </summary>
        public static bool Sound = true;
        /// <summary>
        /// The sound volume.
        /// </summary>
        public static int Volume = 1;
        #endregion
        #region "Debug Settings"
        /// <summary>
        /// Enables debug mode.
        /// </summary>
        public static bool Debug = true;
        /// <summary>
        /// Draws rectangles around all objects.
        /// </summary>
        public static bool DrawBounds = false;
        /// <summary>
        /// Draws rectangles around all draw components.
        /// </summary>
        public static bool DrawTextureBounds = false;
        /// <summary>
        /// The port the debug socket will be hosted on.
        /// </summary>
        public static int DebugSocketPort = 2345;
        #endregion
        #region "Android Settings"
#if ANDROID
        /// <summary>
        /// The orientation of the screen.
        /// </summary>
        public static DisplayOrientation win_orientation = DisplayOrientation.Portrait;
        /// <summary>
        /// Whether to hide the android notification's bar.
        /// </summary>
        public static bool win_hidebar = true;
#endif
        #endregion
        #region "Network Settings"
        /// <summary>
        /// Whether networking is allowed, requires a restart.
        /// </summary>
        public static bool Networking = true;
        public static string IP = "";
        public static string Port = "";
        public static int Timeout = 10;
        #endregion
    }
}
