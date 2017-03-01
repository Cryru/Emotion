using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Soul.IO;
using System.Collections.Generic;
using System.Linq;

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
        #region "Variables"
        /// <summary>
        /// The default settings file to be used when one doesn't exist or is corrupted.
        /// </summary>
        private static Dictionary<string, object> defaultFile = new Dictionary<string, object>();
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
        }

        public static void ReadExternalSettings(string filePath)
        {
            //Check if we should read the file.
            if (settingsLoad == false) return;

            //The settings file is expected to a non encrypted Soul Managed File.
            MFile settingsFile = new MFile("Content\\settings.soul", defaultFile);

            //Read settings.
            settingsFile.AssignContent(ref FPS, "FPS Target");
            settingsFile.AssignContent(ref vSync, "VSync");
            settingsFile.AssignContent(ref WWidth, "Window Width");
            settingsFile.AssignContent(ref WHeight, "Window Height");
            settingsFile.AssignContent(ref DisplayMode, "Display Mode");
            settingsFile.AssignContent(ref ResizableWindow, "Resizable");
            bool traktor = true;
        }

        /// <summary>
        /// LEGACY
        /// Read the settings file if any and load the settings from it.
        /// The settings file by default should be in the same folder as the application and be named "settings.ini".
        /// </summary>
        public static void ReadSettings()
        {
            ////Check if loading the file is enabled.
            //if (loadSettingsFile == false) return;

            ////Check if a settings file exists.
            //if (File.Exists("settings.ini"))
            //{
            //    string[] fileData = IO.ReadFile("settings.ini", true);

            //    for (int i = 0; i < fileData.Length; i++)
            //    {
            //        try //In a try-catch in case something is not formatted properly.
            //        {
            //            //Get the data of the settings file. Key:Value
            //            string[] data = fileData[i].Split(':');

            //            //Check the key and assign the value.
            //            switch (data[0])
            //            {
            //                case "Width": //Resolution's Width
            //                    game_width = int.Parse(data[1]);
            //                    break;
            //                case "Height": //Resolution's Height
            //                    game_height = int.Parse(data[1]);
            //                    break;
            //                case "Win_Name": //Window's Name
            //                    win_name = data[1];
            //                    break;
            //                case "Debug": //Debug Mode.
            //                    debug = bool.Parse(data[1]);
            //                    break;
            //                case "FPSDisplay": //Resolution's Width
            //                    displayFPS = bool.Parse(data[1]);
            //                    break;
            //            }
            //        }
            //        catch
            //        { }
            //    }
            //}
        }
    }
}
