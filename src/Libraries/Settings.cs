using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using Soul.IO;
using System.Collections.Generic;
using System.Linq;
using SoulEngine.Enums;
using SoulEngine.Events;
using System;
using SoulEngine.Modules;

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
        #region "Reactive Settings"
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
                if (value == _DisplayMode) return;

                _DisplayMode = value;

                // Trigger the display changed event, if a window manager is loaded.
                if (Context.Core.isModuleLoaded<WindowManager>()) Context.Core.Module<WindowManager>().triggerDisplayChanged();
            }
        }
        /// <summary>
        /// The FPS target, if below 0 the FPS isn't capped.
        /// </summary>
        public static float FPS
        {
            get
            {
                return _FPS;
            }
            set
            {
                if (value == _FPS) return;

                _FPS = value;

                //Trigger the display mode changed event.
                Context.Core.IsFixedTimeStep = value > 0 ? true : false; //Check whether to cap FPS based on the fps target.
                Context.Core.TargetElapsedTime = value > 0 ? TimeSpan.FromMilliseconds(Math.Floor(1000f / value)) : TimeSpan.FromSeconds(1);
            }
        }
        /// <summary>
        /// The width of the window.
        /// </summary>
        public static int WWidth
        {
            get
            {
                return _WWidth;
            }
            set
            {
                if (value == _WWidth) return;

                _WWidth = value;

                // Trigger the display changed event, if a window manager is loaded.
                if (Context.Core.isModuleLoaded<WindowManager>()) Context.Core.Module<WindowManager>().triggerDisplayChanged();
            }
        }
        /// <summary>
        /// The height of the window.
        /// </summary>
        public static int WHeight
        {
            get
            {
                return _WHeight;
            }
            set
            {
                if (value == _WHeight) return;

                _WHeight = value;

                // Trigger the display changed event, if a window manager is loaded.
                if (Context.Core.isModuleLoaded<WindowManager>()) Context.Core.Module<WindowManager>().triggerDisplayChanged();
            }
        }
        /// <summary>
        /// The name of the window.
        /// </summary>
        public static string WName
        {
            get
            {
                return _WName;
            }
            set
            {
                if (value == _WName) return;

                _WName = value;

                Context.Core.Window.Title = value;
            }
        }
        /// <summary>
        /// Whether the mouse should be rendered.
        /// </summary>
        public static bool RenderMouse
        {
            get
            {
                return _RenderMouse;
            }
            set
            {
                if (value == _RenderMouse) return;

                _RenderMouse = value;

                Context.Core.IsMouseVisible = value;
            }
        }
        #endregion
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
            defaultFile.Add("Sound", Sound);
            defaultFile.Add("Volume", Volume);
            defaultFile.Add("AA", AntiAlias);
            defaultFile.Add("Networking", Networking);
        }

        public static void ReadExternalSettings(string filePath)
        {
            //Check if we should read the file.
            if (ExternalSettings == false) return;

            //The settings file is expected to a non encrypted Soul Managed File.
            MFile settingsFile = new MFile("UserContent\\settings.soul", defaultFile);

            //Read settings.
            settingsFile.AssignContent(ref _FPS, "FPS Target");
            settingsFile.AssignContent(ref vSync, "VSync");
            settingsFile.AssignContent(ref _WWidth, "Window Width");
            settingsFile.AssignContent(ref _WHeight, "Window Height");
            settingsFile.AssignContent(ref _DisplayMode, "Display Mode");
            settingsFile.AssignContent(ref ResizableWindow, "Resizable");
            settingsFile.AssignContent(ref Sound, "Sound");
            settingsFile.AssignContent(ref Volume, "Volume");
            settingsFile.AssignContent(ref AntiAlias, "AA");
            settingsFile.AssignContent(ref Networking, "Networking");
        }
    }
}
