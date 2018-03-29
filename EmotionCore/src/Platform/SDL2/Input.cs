// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Enums;
using Emotion.Primitives;
using SDL2;

#endregion

namespace Emotion.Platform.SDL2
{
    /// <inheritdoc />
    public sealed class Input : IInput
    {
        #region State Trackers

        private byte[] _keyStateArray = new byte[512];

        internal Vector2 MousePosition = new Vector2(0, 0);
        private bool[] _mouseStateHeld = new bool[Enum.GetValues(typeof(MouseKeys)).Length];
        internal bool[] MouseStatePressed = new bool[Enum.GetValues(typeof(MouseKeys)).Length];

        #endregion

        internal void UpdateInputs()
        {
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
        }

        #region Keyboard

        /// <summary>
        /// Returns whether the key is pressed down or not.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key is pressed or not.</returns>
        public bool IsKeyDown(string key)
        {
            // Get the scan code of the key.
            int scanCode = (int) SDL.SDL_GetScancodeFromName(key);
            return _keyStateArray[scanCode] != 0;
        }

        #endregion

        #region Mouse

        public Vector2 GetMousePosition(CameraBase camera = null)
        {
            return camera == null ? MousePosition : new Vector2(MousePosition.X + camera.Bounds.X, MousePosition.Y + camera.Bounds.Y);
        }

        /// <summary>
        /// Triggered when the specified mouse key is pressed.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is pressed.</returns>
        public bool IsMouseKeyDown(MouseKeys key)
        {
            return MouseStatePressed[(int) key];
        }

        /// <summary>
        /// Triggered when the specified mouse key is held.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is being held.</returns>
        public bool IsMouseKeyHeld(MouseKeys key)
        {
            return _mouseStateHeld[(int) key];
        }

        #endregion
    }
}

#endif