using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // Engine user setings.                                                     //
    //////////////////////////////////////////////////////////////////////////////
    public class Settings
    {
        //Info Declarations
        public static string Name = "Soul Engine"; //The name of the engine.
        public static string Ver = "1.0"; //The version of the engine.
        public static string GUID = "130F150C-0000-0000-0000-050E07090E05"; //The guid of the application. (Default Soul Engine - 130F150C-0000-0000-0000-050E07090E05)

        //Window Settings
        public static int win_width = 960; //The width of the window.
        public static int win_height = 540; //The height of the window.
        public static int game_width = 1280; //The width the game will be rendered at.
        public static int game_height = 720; //The height the game will be rendered at.
        public static string win_name = "NONE"; //The name of the window.
        public static bool win_fullscreen = false; //Whether the window should cover the whole screen. Functionally this is borderless windowed.
        public static bool win_renderMouse = true; //Wether the mouse should be rendered.
#if ANDROID
        public static DisplayOrientation win_orientation = DisplayOrientation.Portrait; //The orientation of the screen.
        public static bool win_hidebar = true; //Whether to hide the android notification's bar.
#endif

        //Debug Settings
        public static bool debug = true; //Enables debug mode.
        public static bool debugUpdate = true; //Toggles updating the debug text.
        public static bool debugLogging = true; //Enables the Soul "Log" module.

        //Other Settings
        public static Color fillcolor = Color.Black; //The color that the dummy area will be filled in.
        public static Color drawcolor = Color.CornflowerBlue; //The color that the drawing area will be filled in.
        public static bool sound = true; //Whether sound is on or off.
        public static Keys keyClosing = Keys.Escape; //The key used to close the application.
        public static bool displayFPS = true; //Whether to draw the current fps on the screen.

    }
}
