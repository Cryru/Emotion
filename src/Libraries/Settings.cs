using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Soul.IO;
using System.Collections.Generic;
using System.Linq;
using SoulEngine.Enums;
using SoulEngine.Events;

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
        #region "Declarations"
        /// <summary>
        /// The default settings file to be used when one doesn't exist or is corrupted.
        /// </summary>
        private static Dictionary<string, object> defaultFile = new Dictionary<string, object>();
        /// <summary>
        /// The way the engine should be displayed.
        /// </summary>
        public static DisplayMode DisplayMode
        {
            get
            {
                return _DisplayMode;
            }
            set
            {
                _DisplayMode = value;
                //Send a display mode changed event.
                ESystem.Add(new Event(EType.WINDOW_DISPLAYMODECHANGED, Context.Core));
            }
        }
        #endregion

        /// <summary>
        /// Populates the default file variable.
        /// </summary>
        public static void GenerateDefaultFile()
        {
            defaultFile.Add("FPS Target", FPS);
            defaultFile.Add("VSync", vSync);
            defaultFile.Add("Window Width", WWidth);
            defaultFile.Add("Window Height", WWidth);
            defaultFile.Add("Display Mode", DisplayMode);
            defaultFile.Add("Resizable", ResizableWindow);
            defaultFile.Add("Debug", Debug);
            defaultFile.Add("AA", AntiAlias);
            defaultFile.Add("Networking", Networking);
        }

        public static void ReadExternalSettings(string filePath)
        {
            //Check if we should read the file.
            if (settingsLoad == false) return;

            //The settings file is expected to a non encrypted Soul Managed File.
            MFile settingsFile = new MFile("UserContent\\settings.soul", defaultFile);

            //Read settings.
            settingsFile.AssignContent(ref FPS, "FPS Target");
            settingsFile.AssignContent(ref vSync, "VSync");
            settingsFile.AssignContent(ref WWidth, "Window Width");
            settingsFile.AssignContent(ref WHeight, "Window Height");
            settingsFile.AssignContent(ref _DisplayMode, "Display Mode");
            settingsFile.AssignContent(ref ResizableWindow, "Resizable");
            settingsFile.AssignContent(ref Debug, "Debug");
            settingsFile.AssignContent(ref AntiAlias, "AA");
            settingsFile.AssignContent(ref Networking, "Networking");
        }
    }
}
