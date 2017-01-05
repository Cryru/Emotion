using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Engine functions.
    /// </summary>
    static class Functions
    {
        #region "Screen Functions"
        /// <summary>
        /// Returns the size of the primary physical screen.
        /// </summary>
        public static Vector2 GetScreenSize()
        {
            return new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        }
        /// <summary>
        /// Applies the screen settings, and regenerates a new camera and screen adapter.
        /// </summary>
        public static void RefreshScreenSettings()
        {
            //Reset some settings.
            Context.Engine.Window.IsBorderless = false;
            Context.graphics.IsFullScreen = false;

            //Check which screen mode we want to apply.
            switch (Settings.ScreenMode)
            {
                case Enums.ScreenMode.Windowed:
                    //Setup the screen with the screen's size as the settings specified size.
                    Context.graphics.PreferredBackBufferWidth = Settings.WWidth;
                    Context.graphics.PreferredBackBufferHeight = Settings.WHeight;
                    Context.graphics.ApplyChanges();

                    //Center window.
                    Context.Engine.Window.Position = new Point((int)GetScreenSize().X / 2 - Settings.WWidth / 2, (int)GetScreenSize().Y / 2 - Settings.WHeight / 2);
                    break;

                case Enums.ScreenMode.Borderless:
                    //Remove the window borders.
                    Context.Engine.Window.IsBorderless = true;

                    //Setup the screen with the screen's size as width and height.
                    Context.graphics.PreferredBackBufferWidth = (int)GetScreenSize().X;
                    Context.graphics.PreferredBackBufferHeight = (int)GetScreenSize().Y;
                    Context.graphics.ApplyChanges();

                    //Move the window to the top left.
                    Context.Engine.Window.Position = new Point(0, 0);
                    break;

                case Enums.ScreenMode.Fullscreen:
                    //Set the graphics device to fullscreen.
                    Context.graphics.IsFullScreen = true;

                    //Setup the screen with the screen's size as width and height.
                    Context.graphics.PreferredBackBufferWidth = (int)GetScreenSize().X;
                    Context.graphics.PreferredBackBufferHeight = (int)GetScreenSize().Y;
                    Context.graphics.ApplyChanges();
                    break;
            }


            //Set up the screen adapter.
            Context.Screen = new BoxingViewportAdapter(Context.Engine.Window, Context.graphics.GraphicsDevice, Settings.Width, Settings.Height);
            //Set up the camera.
            Context.Camera = new Camera2D(Context.Screen);        
        }
        #endregion
    }
}
