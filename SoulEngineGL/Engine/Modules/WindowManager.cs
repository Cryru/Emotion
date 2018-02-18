// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Soul.Engine.Enums;
using DisplayMode = Soul.Engine.Enums.DisplayMode;

#endregion

namespace Soul.Engine.Modules
{
    /// <summary>
    /// Manages the window scaling and camera.
    /// </summary>
    public class WindowManager
    {
        /// <summary>
        /// The current drawing location.
        /// </summary>
        public static DrawLocation CurrentLocation = DrawLocation.None;

        /// <summary>
        /// The 2D camera viewport.
        /// </summary>
        public static Camera Camera;

        /// <summary>
        /// Initializes the camera, screen adapter, and applies window settings.
        /// </summary>
        internal static void Setup()
        {
            // Setup camera.
            Camera = new Camera();

            Core.Context.Window.AllowUserResizing = false;

            Core.Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
            Core.Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
            Core.Context.GraphicsManager.ApplyChanges();

            UpdateWindow();
        }

        /// <summary>
        /// Applies the screen settings, and regenerates a new camera and screen adapter.
        /// </summary>
        internal static void UpdateWindow()
        {
            //Reset fullscreen and display border.
            if (Settings.DisplayMode != DisplayMode.BorderlessFullscreen) Core.Context.Window.IsBorderless = false;
            if (Settings.DisplayMode != DisplayMode.Fullscreen) Core.Context.GraphicsManager.IsFullScreen = false;

            //Check which window mode we want to apply.
            switch (Settings.DisplayMode)
            {
                case DisplayMode.Windowed:
                    //Setup the window with the screen's size as the settings specified size.
                    Core.Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                    Core.Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                    Core.Context.GraphicsManager.ApplyChanges();

                    //Center window.
                    Core.Context.Window.Position = new Point(
                        (int) GetScreenSize().X / 2 - Settings.WWidth / 2,
                        (int) GetScreenSize().Y / 2 - Settings.WHeight / 2);
                    break;

                case DisplayMode.Fullscreen:
                    //Set the graphics device to fullscreen.
                    Core.Context.GraphicsManager.IsFullScreen = true;

                    //Setup the window with the screen's size as width and height.
                    Core.Context.GraphicsManager.PreferredBackBufferWidth = (int) GetScreenSize().X;
                    Core.Context.GraphicsManager.PreferredBackBufferHeight = (int) GetScreenSize().Y;
                    Core.Context.GraphicsManager.ApplyChanges();
                    break;

                case DisplayMode.BorderlessFullscreen:
                    //Remove the window borders.
                    Core.Context.Window.IsBorderless = true;

                    //Setup the screen with the screen's size as width and height.
                    Core.Context.GraphicsManager.PreferredBackBufferWidth = (int) GetScreenSize().X;
                    Core.Context.GraphicsManager.PreferredBackBufferHeight = (int) GetScreenSize().Y;
                    Core.Context.GraphicsManager.ApplyChanges();

                    //Move the window to the top left.
                    Core.Context.Window.Position = new Point(0, 0);
                    break;
            }

            // Apply scaling.
            ApplyScaling();
        }

        /// <summary>
        /// Returns the size of the primary screen.
        /// </summary>
        public static Vector2 GetScreenSize()
        {
            return new Vector2(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
        }

        /// <summary>
        /// Returns the matrix of the screen.
        /// </summary>
        /// <returns>The matrix of the screen.</returns>
        public static Matrix GetScreenMatrix()
        {
            return Matrix.CreateScale((float)Core.Context.GraphicsDevice.Viewport.Width / Settings.Width,
                (float)Core.Context.GraphicsDevice.Viewport.Height / Settings.Height, 1.0f);
        }

        /// <summary>
        /// Applies letterbox/pillarbox scaling.
        /// </summary>
        public static void ApplyScaling()
        {
            Viewport viewport = Core.Context.GraphicsDevice.Viewport;

            // Get the scale ratio between the viewport and the game's resolution, based on whichever the bigger side is.
            float scale = MathHelper.Min((float) viewport.Width / Settings.Width,
                (float) viewport.Height / Settings.Height);

            // Scale the width and height based on the calculated scale.
            int width = (int) (scale * Settings.Width);
            int height = (int) (scale * Settings.Height);

            // Generate the new viewport by enlarging the smaller side as much as possible, and moving the viewport in order to center the visible area.
            Core.Context.GraphicsDevice.Viewport = new Viewport(
                viewport.Width / 2 - width / 2,
                viewport.Height / 2 - height / 2,
                width, height);
        }
    }
}