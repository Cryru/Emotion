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
        /// Whether to cap the FPS.
        /// </summary>
        public static bool capFPS = true;
        /// <summary>
        /// The FPS limit.
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
        public static int WWidth = 160;
        /// <summary>
        /// The height of the window.
        /// </summary>
        public static int WHeight = 90;
        /// <summary>
        /// The width the game will be rendered at.
        /// </summary>
        public static int Width = 100;
        /// <summary>
        /// The height the game will be rendered at.
        /// </summary>
        public static int Height = 50;
        /// <summary>
        /// The name of the window.
        /// </summary>
        public static string WName = "SoulEngine 2017";
        /// <summary>
        /// The way the engine should be displayed, the RefreshScreenSettings function must be used to apply changes.
        /// </summary>
        public static ScreenMode ScreenMode = ScreenMode.Windowed;
        /// <summary>
        /// Whether the mouse should be rendered.
        /// </summary>
        public static bool RenderMouse = true;
        /// <summary>
        /// Whether the window can be resized.
        /// </summary>
        public static bool ResizableWindow = true;
        #endregion
        #region "Other Settings"
        /// <summary>
        /// The color the screen will be cleared with.
        /// </summary>
        public static Color FillColor = Color.CornflowerBlue;
        /// <summary>
        /// Whether an external settings file should be loaded.
        /// </summary>
        public static bool settingsLoad = true;
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

        //TODO

        #region "Debug Settings"
        /// <summary>
        /// Enables debug mode.
        /// </summary>
        public static bool debug = true;
        /// <summary>
        /// Toggles updating the debug text.
        /// </summary>
        public static bool debugUpdate = true;
        /// <summary>
        /// Whether to draw the current fps on the screen.
        /// </summary>
        public static bool displayFPS = true;
        /// <summary>
        /// Whether to update the fps text.
        /// </summary>
        public static bool fpsUpdate = true;
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
    }
}
