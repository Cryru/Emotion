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
        /// Applies the screen settings, and regenerates a new camera and screen adapter.
        /// Triggered when the size of the window changes. Can force an update if event data is "Force".
        /// </summary>
        /// <param name="e">The event this is triggered by.</param>
        public static void RefreshScreenSettings(Event e)
        {
            //Check if forcing an update.
            bool force = false;
            if (e.Data != null && ((string) e.Data).ToLower() == "force") force = true;

            //Check if using custom size from user, as is the case when the window is resizable.
            if (force == true || Settings.ResizableWindow == false)
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
                if(Context.Core.Window.ClientBounds.Width < Settings.WWidth ||
                    Context.Core.Window.ClientBounds.Height < Settings.WHeight)
                {
                    Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                    Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                    Context.GraphicsManager.ApplyChanges();
                }
            }

            //Update the screen adapter, as this event fucks with the viewport.
            Context.Screen.Update();
        }
    }
}
