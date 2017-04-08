using Microsoft.Xna.Framework;
using SoulEngine.Events;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manages the game window, resolution and other related stuff.
    /// </summary>
    public static class WindowManager
    {

        /// <summary>
        /// Initializes the camera, screen adapter, applies various settings, and connects to events.
        /// </summary>
        public static void Initialize()
        {
            //Setup viewing object contexts.
            Context.Screen = new ScreenAdapter();
            Context.Camera = new Camera();

            //Hook a size changed event to the screen adapter's update, as changes to the window size will mess with it.
            ESystem.Add(new Listen(EType.WINDOW_SIZECHANGED, Context.Screen.Update));
            ESystem.Add(new Listen(EType.WINDOW_DISPLAYMODECHANGED, UpdateWindow));

            //Set the window to the setting's size if resizable.
            if (Settings.ResizableWindow == true)
            {
                Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                Context.GraphicsManager.ApplyChanges();
            }

            //Update the window.
            UpdateWindow();
        }

        /// <summary>
        /// Applies the screen settings, and regenerates a new camera and screen adapter.
        /// </summary>
        public static void UpdateWindow()
        {
            //Update the resizing setting.
            Context.Core.Window.AllowUserResizing = Settings.ResizableWindow;

            //Check if using custom size from user, as is the case when the window is resizable.
            if (Settings.ResizableWindow == false)
            {
                //Reset fullscreen and display border.
                if (Settings.DisplayMode != Enums.DisplayMode.BorderlessFullscreen) Context.Core.Window.IsBorderless = false;
                if (Settings.DisplayMode != Enums.DisplayMode.Fullscreen) Context.GraphicsManager.IsFullScreen = false;

                //Check which window mode we want to apply.
                switch (Settings.DisplayMode)
                {
                    case Enums.DisplayMode.Windowed:
                        //Setup the window with the screen's size as the settings specified size.
                        Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                        Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                        Context.GraphicsManager.ApplyChanges();

                        //Center window.
                        Context.Core.Window.Position = new Point((int)Functions.GetScreenSize().X / 2 - Settings.WWidth / 2,
                            (int)Functions.GetScreenSize().Y / 2 - Settings.WHeight / 2);
                        break;

                    case Enums.DisplayMode.Fullscreen:
                        //Set the graphics device to fullscreen.
                        Context.GraphicsManager.IsFullScreen = true;

                        //Setup the window with the screen's size as width and height.
                        Context.GraphicsManager.PreferredBackBufferWidth = (int)Functions.GetScreenSize().X;
                        Context.GraphicsManager.PreferredBackBufferHeight = (int)Functions.GetScreenSize().Y;
                        Context.GraphicsManager.ApplyChanges();
                        break;

                    case Enums.DisplayMode.BorderlessFullscreen:
                        //Remove the window borders.
                        Context.Core.Window.IsBorderless = true;

                        //Setup the screen with the screen's size as width and height.
                        Context.GraphicsManager.PreferredBackBufferWidth = (int)Functions.GetScreenSize().X;
                        Context.GraphicsManager.PreferredBackBufferHeight = (int)Functions.GetScreenSize().Y;
                        Context.GraphicsManager.ApplyChanges();

                        //Move the window to the top left.
                        Context.Core.Window.Position = new Point(0, 0);
                        break;
                }
            }
            else
            {
                //Check if window smaller than minimum.
                if (Context.Core.Window.ClientBounds.Width < Settings.WWidth ||
                    Context.Core.Window.ClientBounds.Height < Settings.WHeight)
                {
                    Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                    Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                    Context.GraphicsManager.ApplyChanges();
                }
            }

            //Send the new display mode to the system event which detects changes to the settings.
            ESystem.Add(new Event("prevDisplayMode_update", null, Settings.DisplayMode));
        }
    }
}
