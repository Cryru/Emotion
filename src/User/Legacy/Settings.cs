using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Legacy
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // This code is part of the SoulEngine backwards compatibility layer.       //
    // Original Repository: https://github.com/Cryru/SoulEngine-2016            //
    //////////////////////////////////////////////////////////////////////////////
    public static class Settings
    {
        public static bool debug = false;
        public static bool debugUpdate = true;

        public static bool displayFPS = false;
        public static bool fpsUpdate = false;

        public static int game_width
        {
            get
            {
                return SoulEngine.Settings.Width;
            }
        }
        public static int game_height
        {
            get
            {
                return SoulEngine.Settings.Height;
            }
        }

        public static int win_width
        {
            get
            {
                return SoulEngine.Settings.WWidth;
            }
        }
        public static int win_height
        {
            get
            {
                return SoulEngine.Settings.WHeight;
            }
        }

        public static Color fillcolor
        {
            get
            {
                return Color.Black;
            }
        }
        public static Color drawcolor
        {
            get
            {
                return SoulEngine.Settings.FillColor;
            }
        }

        public static bool win_fullscreen
        {
            get
            {
                return SoulEngine.Settings.DisplayMode == Enums.DisplayMode.Fullscreen;
            }
            set
            {
                SoulEngine.Settings.DisplayMode = Enums.DisplayMode.Fullscreen;
            }
        }

        public static Objects.Screen StartScreen;
    }
}
