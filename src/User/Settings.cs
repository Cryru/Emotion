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
        #region "Engine Settings"
        /// <summary>
        /// The FPS target.
        /// </summary>
        public static float FPS = 60;
        /// <summary>
        /// Whether to synchronize the FPS to the screen's refresh rate. 
        /// Overwrites the 'capFPS' setting. 
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
        #endregion
        #region "Primary User Settings"
        /// <summary>
        /// Whether sound is on or off.
        /// </summary>
        public static bool Sound = true;
        /// <summary>
        /// The sound volume of each sound channel.
        /// </summary>
        public static List<int> SoundChannelsVolume;
        #endregion
        #region "Debug Settings"
        /// <summary>
        /// Enables debug mode.
        /// </summary>
        public static bool Debug = true;
        #endregion

        //TODO: ANDROID SETTINGS
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
    }
}
