using Microsoft.Xna.Framework;
using SoulEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Events
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Events hooked to system functions.
    /// </summary>
    class SystemEvents
    {
        /// <summary>
        /// Connects system events to tracker functions.
        /// </summary>
        public static void ConnectSystemEvents()
        {
            //Hook display mode change event.
            ESystem.Add(new Listen(EType.GAME_TICKSTART, DisplayModeChanged));
            ESystem.Add(new Listen("prevDisplayMode_update", prevDisplayMode_Update));
        }

        /// <summary>
        /// Tracks changes to the display mode.
        /// </summary>
        private static Enums.DisplayMode prevDisplayMode;
        private static void DisplayModeChanged()
        {
            //If a change to the display mode is detected raise the event.
            if (Settings.DisplayMode != prevDisplayMode) ESystem.Add(new Event(EType.WINDOW_DISPLAYMODE));
        }
        private static void prevDisplayMode_Update(Event e)
        {
            prevDisplayMode = (Enums.DisplayMode)e.Data;
        }
    }
}