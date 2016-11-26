using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev - TheCryru@gmail.com                     //
    //                                                                          //
    // For any questions and issues: https://github.com/Cryru/SoulEngine        //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The engine's user settings.
    /// </summary>
    public partial class Settings
    {
        #region "Window Settings"
        /// <summary>
        /// The width of the window.
        /// </summary>
        public static int win_width = 960;
        /// <summary>
        /// The height of the window.
        /// </summary>
        public static int win_height = 540;
        /// <summary>
        /// The width the game will be rendered at.
        /// </summary>
        public static int game_width = 1280;
        /// <summary>
        /// The height the game will be rendered at.
        /// </summary>
        public static int game_height = 720;
        /// <summary>
        /// The name of the window.
        /// </summary>
        public static string win_name = "NONE";
        /// <summary>
        /// Whether the window should cover the whole screen. Functionally this is borderless windowed.
        /// </summary>
        public static bool win_fullscreen = false;
        /// <summary>
        /// Whether the mouse should be rendered.
        /// </summary>
        public static bool win_renderMouse = true;
        #endregion
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
        #region "Screen Settings"
        /// <summary>
        /// The screen to load first.
        /// Do not initliaze any objects in it.
        /// </summary>
        public static Objects.Screen StartScreen = new StartScreen();
        #endregion
        #region "Other Settings"
        /// <summary>
        /// The color that the dummy area will be filled in.
        /// </summary>
        public static Color fillcolor = Color.Black;
        /// <summary>
        /// The color that the drawing area will be filled in.
        /// </summary>
        public static Color drawcolor = Color.CornflowerBlue;
        /// <summary>
        /// Whether sound is on or off.
        /// </summary>
        public static bool sound = true;
        /// <summary>
        /// The key to used to close the application, except alt+F4.
        /// </summary>
        public static Keys keyClosing = Keys.Escape;
        /// <summary>
        /// Enables or disables the ability for the settings file to overwrite settings.
        /// </summary>
        public static bool loadSettingsFile = true;
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
