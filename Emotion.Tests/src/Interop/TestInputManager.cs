// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Input;
using OpenTK.Input;

#endregion

namespace Emotion.Tests.Interop
{
    /// <summary>
    /// Handles input from the mouse, keyboard, and other devices.
    /// </summary>
    public class TestInputManager : IInputManager
    {
        #region Properties

        /// <summary>
        /// The state of the mouse captured from the host.
        /// </summary>
        private bool[] _mouseHolder = new bool[Enum.GetValues(typeof(MouseKeys)).Length];

        /// <summary>
        /// The state of the mouse for the last update.
        /// </summary>
        private bool[] _mouseLast = new bool[Enum.GetValues(typeof(MouseKeys)).Length];

        /// <summary>
        /// The state of the mouse for the current update.
        /// </summary>
        private bool[] _mouse = new bool[Enum.GetValues(typeof(MouseKeys)).Length];

        /// <summary>
        /// The scroll of the mouse. Stored for relative calculations.
        /// </summary>
        private float _mouseWheelScroll;

        /// <summary>
        /// The Otk host.
        /// </summary>
        private TestHost _host;

        #endregion

        internal TestInputManager(TestHost host)
        {
            _host = host;
        }

        internal void Update()
        {
        }

        #region Mouse

        /// <summary>
        /// The amount the scroll wheel is scrolled.
        /// </summary>
        /// <returns>The amount the scroll wheel is scrolled.</returns>
        public float GetMouseScroll()
        {
            _mouseWheelScroll = Mouse.GetState().WheelPrecise;
            return _mouseWheelScroll;
        }

        /// <summary>
        /// The amount the mouse has scrolled since the last check.
        /// </summary>
        /// <returns>The amount the mouse has scrolled since the last check.</returns>
        public float GetMouseScrollRelative()
        {
            float position = Mouse.GetState().WheelPrecise;
            float relativePos = _mouseWheelScroll - position;
            _mouseWheelScroll = position;
            return relativePos;
        }

        /// <summary>
        /// Returns the position of the mouse cursor within the window.
        /// </summary>
        /// <returns>The position of the mouse cursor within the window.</returns>
        public Vector2 GetMousePosition()
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Returns whether the mouse key was pressed down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was pressed down.</returns>
        public bool IsMouseKeyDown(MouseKeys key)
        {
            return _mouse[(int) key] && !_mouseLast[(int) key];
        }

        /// <summary>
        /// Returns whether the mouse key was let go.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was let go.</returns>
        public bool IsMouseKeyUp(MouseKeys key)
        {
            return _mouseLast[(int) key] && !_mouse[(int) key];
        }

        /// <summary>
        /// Returns whether the mouse key is being held down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is being held down.</returns>
        public bool IsMouseKeyHeld(MouseKeys key)
        {
            return _mouse[(int) key] && _mouseLast[(int) key];
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// Returns whether the key is being held down, using its name.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        public bool IsKeyHeld(string key)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the key was pressed down, using its name.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(string key)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the key was let go, using its name.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was let go.</returns>
        public bool IsKeyUp(string key)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the key is being held down, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        public bool IsKeyHeld(short key)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the key was pressed down, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(short key)
        {
            return false;
        }

        /// <summary>
        /// Returns whether the key was let go, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was let go.</returns>
        public bool IsKeyUp(short key)
        {
            return false;
        }

        /// <summary>
        /// Returns the name of a key from its key code.
        /// </summary>
        /// <param name="key">The key code whose name to return.</param>
        /// <returns>The name of the key under the provided key code.</returns>
        public string GetKeyNameFromCode(short key)
        {
            return Enum.GetName(typeof(Key), key);
        }

        #endregion
    }
}