// Emotion - https://github.com/Cryru/Emotion

#if SDL2

#region Using

using System;
using System.Runtime.InteropServices;
using SDL2;

#endregion

namespace Emotion.Engine
{
    public class Input
    {
        private byte[] _keyStateArray = new byte[512];

        internal void UpdateInputs()
        {
            // Get the keyboard state for the current frame.
            IntPtr keyState = SDL.SDL_GetKeyboardState(out int keyLength);
            Marshal.Copy(keyState, _keyStateArray, 0, keyLength);
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
    }
}

#endif