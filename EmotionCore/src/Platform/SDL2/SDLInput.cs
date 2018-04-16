// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Engine.Enums;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Enums;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2
{
    /// <inheritdoc />
    public sealed class SDLInput : IInput
    {
        #region State Trackers

        private byte[] _keyStateArray = new byte[512];
        private byte[] _keyStateArrayLastFrame = new byte[512];

        internal Vector2 MousePosition = new Vector2(0, 0);
        private bool[] _mouseStateHeld = new bool[Enum.GetValues(typeof(MouseKeys)).Length];
        internal bool[] MouseStatePressed = new bool[Enum.GetValues(typeof(MouseKeys)).Length];

        #endregion

        private SDLContext _context;

        internal SDLInput(SDLContext context)
        {
            _context = context;
        }

        internal void UpdateInputs()
        {
            // Copy the state from the last frame.
            Array.Copy(_keyStateArray, _keyStateArrayLastFrame, 512);

            // Get the keyboard state for the current frame.
            IntPtr keyState = SDL.SDL_GetKeyboardState(out int keyLength);
            Marshal.Copy(keyState, _keyStateArray, 0, keyLength);

            // Get mouse states for the current frame.
            _mouseStateHeld[0] = (SDL.SDL_GetMouseState(IntPtr.Zero, IntPtr.Zero) & SDL.SDL_BUTTON_LEFT) == 1;
            _mouseStateHeld[1] = (SDL.SDL_GetMouseState(IntPtr.Zero, IntPtr.Zero) & SDL.SDL_BUTTON_RIGHT) == 1;
            _mouseStateHeld[2] = (SDL.SDL_GetMouseState(IntPtr.Zero, IntPtr.Zero) & SDL.SDL_BUTTON_MIDDLE) == 1;

            // Clear press states from the previous frame.
            for (int i = 0; i < MouseStatePressed.Length; i++)
            {
                MouseStatePressed[i] = false;
            }

            // Check for fullscreen toggling key combo.
            if (IsKeyHeld("Left Alt") && IsKeyDown("Return"))
            {
                _context.Settings.WindowMode = _context.Settings.WindowMode == WindowMode.Borderless ? WindowMode.Windowed : WindowMode.Borderless;
                _context.ApplySettings();
            }

            // Check for closing combo.
            if (IsKeyDown("Escape")) _context.Quit();
        }

        #region Keyboard

        public bool IsKeyHeld(string key)
        {
            // Get the scan code of the key.
            int scanCode = (int) SDL.SDL_GetScancodeFromName(key);
            return _keyStateArray[scanCode] != 0;
        }

        public bool IsKeyDown(string key)
        {
            // Get the scan code of the key.
            int scanCode = (int) SDL.SDL_GetScancodeFromName(key);
            return _keyStateArray[scanCode] == 1 && _keyStateArrayLastFrame[scanCode] == 0;
        }

        #endregion

        #region Mouse

        public Vector2 GetMousePosition(CameraBase camera = null)
        {
            return camera == null ? MousePosition : new Vector2(MousePosition.X + camera.Bounds.X, MousePosition.Y + camera.Bounds.Y);
        }

        /// <summary>
        /// Returns whether the mouse key was pressed down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was pressed down.</returns>
        public bool IsMouseKeyDown(MouseKeys key)
        {
            return MouseStatePressed[(int) key];
        }

        /// <summary>
        /// Returns whether the mouse key is being held down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is being held down.</returns>
        public bool IsMouseKeyHeld(MouseKeys key)
        {
            return _mouseStateHeld[(int) key];
        }

        #endregion
    }
}

#endif