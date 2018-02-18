// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Soul.Engine.Enums;
using DisplayMode = Soul.Engine.Enums.DisplayMode;

#endregion

namespace Soul.Engine.Modules
{
    internal static class Input
    {
        /// <summary>
        /// The state of the keyboard within this frame.
        /// </summary>
        public static KeyboardState KeyboardState;

        /// <summary>
        /// The state of the keyboard from the last frame.
        /// </summary>
        public static KeyboardState LastKeyboardState;

        /// <summary>
        /// The state of the mouse within this frame.
        /// </summary>
        public static MouseState MouseState;

        /// <summary>
        /// The state of the mouse within the last frame.
        /// </summary>
        public static MouseState LastMouseState;

        /// <summary>
        /// Gets input data for the current frame.
        /// </summary>
        public static void Update()
        {
            // Carry over last frame's current as this frame's last.
            LastKeyboardState = KeyboardState;
            LastMouseState = MouseState;

            //Record the frame's keyboard and mouse states for the current frame.
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            // Check if closing.
            if (KeyHeld(Keys.Escape)) Core.Stop();

            // Check if toggling fullscreen.
            if (!KeyPressed(Keys.Enter) || !KeyHeld(Keys.LeftAlt)) return;
            Settings.DisplayMode = Settings.DisplayMode == DisplayMode.Windowed ? DisplayMode.BorderlessFullscreen : DisplayMode.Windowed;
        }

        #region Keyboard


        /// <summary>
        /// Returns whether the specified key is being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if held, false if not.</returns>
        public static bool KeyHeld(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Returns whether the specified button was pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if pressed this frame, false if not.</returns>
        public static bool KeyPressed(Keys key)
        {
            return KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyDown(key) == false;
        }

        /// <summary>
        /// Returns whether the specified button was let go.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if pressed, false if not.</returns>
        public static bool KeyLetGo(Keys key)
        {
            return KeyboardState.IsKeyUp(key) && LastKeyboardState.IsKeyUp(key) == false;
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Returns the location of the mouse.
        /// </summary>
        /// <returns>A vector2 representing the location of the mouse cursor.</returns>
        public static Vector2 MouseLocation()
        {
            // Get the viewport and warp the current mouse position through it.
            Viewport viewport = Core.Context.GraphicsDevice.Viewport;
            return Vector2.Transform(MouseState.Position.ToVector2() - new Vector2(viewport.X, viewport.Y), Matrix.Invert(WindowManager.GetScreenMatrix()));
        }

        /// <summary>
        /// Returns whether a mouse button is down.
        /// </summary>
        /// <param name="button">The button to check.</param>
        public static bool MouseButtonHeld(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return MouseState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return MouseState.RightButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return MouseState.MiddleButton == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns whether a mouse button was pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        public static bool MouseButtonPressed(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return MouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
                case MouseButton.Right:
                    return MouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released;
                case MouseButton.Middle:
                    return MouseState.MiddleButton == ButtonState.Pressed && LastMouseState.MiddleButton == ButtonState.Released;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns whether a mouse button was let go.
        /// </summary>
        /// <param name="button">The button to check.</param>
        public static bool MouseButtonLetGo(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return LastMouseState.LeftButton == ButtonState.Pressed && MouseState.LeftButton == ButtonState.Released;
                case MouseButton.Right:
                    return LastMouseState.RightButton == ButtonState.Pressed && MouseState.RightButton == ButtonState.Released;
                case MouseButton.Middle:
                    return LastMouseState.MiddleButton == ButtonState.Pressed && MouseState.MiddleButton == ButtonState.Released;
                default:
                    return false;
            }
        }


        #endregion
    }
}