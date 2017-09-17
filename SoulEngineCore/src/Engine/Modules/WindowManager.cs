using Microsoft.Xna.Framework;
using SoulEngine.Events;
using SoulEngine.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Modules
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Manages the game window, resolution and other related stuff.
    /// </summary>
    public class WindowManager : IModule
    {
        #region Objects
        /// <summary>
        /// The screen's viewport.
        /// </summary>
        public ScreenAdapter Screen;
        /// <summary>
        /// The 2D camera viewport.
        /// </summary>
        public Camera Camera;
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the window's size changes. This only happens with a resizable window.
        /// </summary>
        public event EventHandler<EventArgs> OnSizeChanged;
        /// <summary>
        /// Triggered when the window's display mode changes.
        /// </summary>
        public event EventHandler<EventArgs> OnDisplayModeChanged;
        #endregion

        /// <summary>
        /// Initializes the camera, screen adapter, applies various settings, and connects to events.
        /// </summary>
        public bool Initialize()
        {
            // Setup view objects.
            Screen = new ScreenAdapter();
            Camera = new Camera();

#if !ANDROID
            // Set the window to the setting's size if resizable.
            if (Settings.ResizableWindow == true)
            {
                Context.GraphicsManager.PreferredBackBufferWidth = Settings.WWidth;
                Context.GraphicsManager.PreferredBackBufferHeight = Settings.WHeight;
                Context.GraphicsManager.ApplyChanges();
            }

            // Update the window.
            UpdateWindow();

            // Hook up to events.
            Context.Core.Window.ClientSizeChanged += Window_SizeChanged;
#else
            // Update the window.
            UpdateWindow();

            Settings.WWidth = Context.GraphicsManager.PreferredBackBufferWidth;
            Settings.WHeight = Context.GraphicsManager.PreferredBackBufferHeight;

            int temp = Settings.Width;
            Settings.Width = Settings.Height;
            Settings.Width = temp;

            // Hook up to events.
            Context.Core.Window.OrientationChanged += Window_SizeChanged;
#endif
            return true;
        }

        /// <summary>
        /// Applies the screen settings, and regenerates a new camera and screen adapter.
        /// </summary>
        public void UpdateWindow()
        {
#if !ANDROID
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
#else
            // If Android we just go full screen.
            Context.GraphicsManager.IsFullScreen = true;
            Context.GraphicsManager.ApplyChanges();

#endif
        }

        #region Event Wrappers
        /// <summary>
        /// Triggered when the size of the window changes. Happens if Settings.ResizableWindow is true.
        /// </summary>
        private void Window_SizeChanged(object sender, EventArgs e)
        {
            // Update the screen adapter.
            Screen?.Update();

            // Invoke the size changed event.
            OnSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Wrapper for the display changed event.
        /// </summary>
        public void triggerDisplayChanged()
        {
            // Update the window.
            UpdateWindow();

            // Trigger actual event.
            OnDisplayModeChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

    }
}
