// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Drawing;
using Breath.Systems;
using OpenTK;
using OpenTK.Input;

#endregion

namespace Soul.Engine.Modules
{
    public static class Input
    {
        #region Keyboard Declarations

        /// <summary>
        /// The state of the keyboard in this frame.
        /// </summary>
        private static KeyboardState _keys;

        /// <summary>
        /// The state of the keyboard from the last frame.
        /// </summary>
        private static KeyboardState _keysLastFrame;

        #endregion

        #region Mouse Declarations

        /// <summary>
        /// The state of the mouse in this frame.
        /// </summary>
        private static MouseState _mouse;

        /// <summary>
        /// The state of the mouse from the last frame.
        /// </summary>
        private static MouseState _mouseLastFrame;

        #endregion

        #region Module API

        /// <summary>
        /// Initializes the module.
        /// </summary>
        internal static void Setup()
        {
            _keys = Keyboard.GetState();
            _mouse = Mouse.GetState();
            _keysLastFrame = _keys;
            _mouseLastFrame = _mouse;
        }

        /// <summary>
        /// Updates the module.
        /// </summary>
        internal static void Update()
        {
            _keys = Keyboard.GetState();
            _mouse = Mouse.GetState();

            // Alt + Enter to toggle between Fullscreen and Windowed.
            if (KeyHeld(Key.AltLeft) && KeyPressed(Key.Enter))
            {
                Core.BreathWin.ChangeWindowMode(Core.BreathWin.WindowMode == Breath.Enums.WindowMode.Windowed
                    ? Breath.Enums.WindowMode.Borderless
                    : Breath.Enums.WindowMode.Windowed);
            }

            // Escape closes everything.
            if (KeyPressed(Key.Escape)) Core.Stop();
        }

        internal static void UpdateEnd()
        {
            _keysLastFrame = _keys;
            _mouseLastFrame = _mouse;
        }

        #endregion

        #region Keyboard Functions

        /// <summary>
        /// Returns true when a keyboard key is pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True when the key is pressed.</returns>
        public static bool KeyPressed(Key key)
        {
            // If the key was not pressed last frame and now is, then it was pressed.
            return !_keysLastFrame[key] && _keys[key];
        }

        /// <summary>
        /// Returns true when the key is being held.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is being held.</returns>
        public static bool KeyHeld(Key key)
        {
            return _keysLastFrame[key] && _keys[key];
        }

        #endregion

        #region Mouse Functions

        /// <summary>
        /// Returns true when the mouse button is pressed.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True when the button is pressed.</returns>
        public static bool MouseButtonPressed(MouseButton button)
        {
            // If the key was not pressed last frame and now is, then it was pressed.
            return !_mouseLastFrame[button] && _mouse[button];
        }

        /// <summary>
        /// Returns whether the mouse button is being held.
        /// </summary>
        /// <param name="button">The button to check.</param>
        /// <returns>True if the button is being held.</returns>
        public static bool MouseButtonHeld(MouseButton button)
        {
            return _mouseLastFrame[button] && _mouse[button];
        }

        /// <summary>
        /// Returns 1 if scrolled up, and -1 if scrolled down.
        /// </summary>
        /// <returns>A value representing the direction of any scrolling done.</returns>
        public static int MouseWheelScroll()
        {
            return _mouse.ScrollWheelValue - _mouseLastFrame.ScrollWheelValue;
        }

        /// <summary>
        /// Returns the location of the mouse within the window.
        /// </summary>
        /// <returns>The location of the mouse as Vector2 in window coordinates.</returns>
        public static Vector2 MouseLocation()
        {
            Vector2 renderSize = Core.BreathWin.RenderSize;
            Size windowSize = Core.BreathWin.ClientSize;
            Vector2 mouseLocation = new Vector2(Core.BreathWin.Mouse.X, Core.BreathWin.Mouse.Y);

            int smallerValueRender = (int) Math.Min(renderSize.X, renderSize.Y);
            int smallerValueWindow = Math.Min(windowSize.Width, windowSize.Height);

            mouseLocation = (mouseLocation * smallerValueRender) / smallerValueWindow;

            return mouseLocation;
        }

        #endregion
    }
}