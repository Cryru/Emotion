using Microsoft.Xna.Framework;
using SoulEngine.Events;
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

            //Reset some settings.
            Context.Core.Window.IsBorderless = false;
            Context.GraphicsManager.IsFullScreen = false;

            //Check if using custom size from user, in the case of a resizable window.
            if (force == true || Settings.ResizableWindow == false)
            {
                //Check which screen mode we want to apply.
                switch (Settings.ScreenMode)
                {
                    case Enums.ScreenMode.Windowed:
                        //Setup the screen with the screen's size as the settings specified size.
                        Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                        Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                        Context.GraphicsManager.ApplyChanges();

                        //Center window, but only if not at boot.
                        Context.Core.Window.Position = new Point((int)Functions.GetScreenSize().X / 2 - Settings.WWidth / 2, 
                            (int)Functions.GetScreenSize().Y / 2 - Settings.WHeight / 2);
                        break;

                    case Enums.ScreenMode.Borderless:
                        //Remove the window borders.
                        Context.Core.Window.IsBorderless = true;

                        //Setup the screen with the screen's size as width and height.
                        Context.GraphicsManager.PreferredBackBufferWidth = (int)Functions.GetScreenSize().X;
                        Context.GraphicsManager.PreferredBackBufferHeight = (int)Functions.GetScreenSize().Y;
                        Context.GraphicsManager.ApplyChanges();

                        //Move the window to the top left.
                        Context.Core.Window.Position = new Point(0, 0);
                        break;

                    case Enums.ScreenMode.Fullscreen:
                        //Set the graphics device to fullscreen.
                        Context.GraphicsManager.IsFullScreen = true;

                        //Setup the screen with the screen's size as width and height.
                        Context.GraphicsManager.PreferredBackBufferWidth = (int)Functions.GetScreenSize().X;
                        Context.GraphicsManager.PreferredBackBufferHeight = (int)Functions.GetScreenSize().Y;
                        Context.GraphicsManager.ApplyChanges();
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
