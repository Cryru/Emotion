// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Numerics;
using Emotion.Debug;
using Emotion.Input;
using OpenTK.Input;

#endregion

namespace Emotion.Engine.Hosting.Desktop
{
    /// <summary>
    /// Handles input from the mouse, keyboard, and other devices.
    /// </summary>
    public class OtkInputManager : IInputManager
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
        /// The status of the keyboard in the last frame.
        /// </summary>
        private KeyboardState _keyboardLast;

        /// <summary>
        /// The status of the keyboard this frame.
        /// </summary>
        private KeyboardState _keyboard;

        /// <summary>
        /// An internal mouse position based on the window which is more accurate than directly polling the mouse.
        /// </summary>
        private Vector2 _mouseLocation;

        /// <summary>
        /// The scroll of the mouse. Stored for relative calculations.
        /// </summary>
        private float _mouseWheelScroll;

        /// <summary>
        /// The Otk host.
        /// </summary>
        private OtkWindow _host;

        #endregion

        #region Input Focus Tracker

        private bool _inputFocus = true;

        #endregion

        internal OtkInputManager(OtkWindow host)
        {
            _host = host;

            host.MouseDown += WindowMouseDown;
            host.MouseUp += WindowMouseUp;
            host.MouseMove += (sender, e) => { _mouseLocation = new Vector2(e.X, e.Y); };
        }

        internal void Update()
        {
            // Set input focus to false if host focus is lost.
            if (!_host.Focused)
            {
                _inputFocus = false;
            }

            // Check if focus has returned and skip input loop if no focus.
            if (!_inputFocus)
            {
                for (int i = 0; i < _mouse.Length; i++)
                {
                    _mouse[i] = false;
                    _mouseLast[i] = false;
                }

                return;
            }

            // Transfer current to last.
            for (int i = 0; i < _mouse.Length; i++)
            {
                _mouseLast[i] = _mouse[i];
            }
            // Transfer holder to current.
            for (int i = 0; i < _mouse.Length; i++)
            {
                _mouse[i] = _mouseHolder[i];
            }

            // Transfer current to last, and clear current.
            _keyboardLast = _keyboard;
            // Get current keyboard state.
            _keyboard = Keyboard.GetState();

            // Check for fullscreen toggling key combo.
            if (IsKeyHeld("LAlt") && IsKeyDown("Enter"))
            {
                Context.Settings.HostSettings.WindowMode = Context.Settings.HostSettings.WindowMode == WindowMode.Borderless ? WindowMode.Windowed : WindowMode.Borderless;
                Context.Host.ApplySettings(Context.Settings.HostSettings);
            }

            // Check for closing combo.
            if (IsKeyDown("Escape")) Context.Quit();
        }

        #region Events

        /// <summary>
        /// Handles the window mouse down event in order to determine a button has been pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_host.Focused && !_inputFocus)
            {
                _inputFocus = true;
                Context.Log.Warning("Regained input focus.", MessageSource.Input);
                return;
            }

            if (!_inputFocus) return;
            switch (e.Button)
            {
                case MouseButton.Left:
                    _mouseHolder[0] = true;
                    break;
                case MouseButton.Right:
                    _mouseHolder[1] = true;
                    break;
                case MouseButton.Middle:
                    _mouseHolder[2] = true;
                    break;
            }
        }

        /// <summary>
        /// Handles the window mouse down event in order to determine a button is no longer held.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_inputFocus) return;
            switch (e.Button)
            {
                case MouseButton.Left:
                    _mouseHolder[0] = false;
                    break;
                case MouseButton.Right:
                    _mouseHolder[1] = false;
                    break;
                case MouseButton.Middle:
                    _mouseHolder[2] = false;
                    break;
            }
        }

        #endregion

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
            float scaleX = Context.Settings.RenderSettings.Width / Context.Host.Size.X;
            float scaleY = Context.Settings.RenderSettings.Height / Context.Host.Size.Y;

            Vector2 mouseLocation = new Vector2(_mouseLocation.X * scaleX, _mouseLocation.Y * scaleY);
            return mouseLocation;
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
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyDown(otKey) && _keyboardLast.IsKeyDown(otKey);
#if DEBUG
            Context.Log.Error($"Couldn't find key [{key}] for held check.", MessageSource.Input);
#endif
            return false;
        }

        /// <summary>
        /// Returns whether the key was pressed down, using its name.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(string key)
        {
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyDown(otKey) && !_keyboardLast.IsKeyDown(otKey);
#if DEBUG
            Context.Log.Error($"Couldn't find key [{key}] for down check.", MessageSource.Input);
#endif
            return false;
        }

        /// <summary>
        /// Returns whether the key was let go, using its name.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was let go.</returns>
        public bool IsKeyUp(string key)
        {
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyUp(otKey) && !_keyboardLast.IsKeyUp(otKey);
#if DEBUG
            Context.Log.Error($"Couldn't find key [{key}] for up check.", MessageSource.Input);
#endif
            return false;
        }

        /// <summary>
        /// Returns whether the key is being held down, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        public bool IsKeyHeld(short key)
        {
            return _keyboard.IsKeyDown(key) && _keyboardLast.IsKeyDown(key);
        }

        /// <summary>
        /// Returns whether the key was pressed down, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(short key)
        {
            return _keyboard.IsKeyDown(key) && !_keyboardLast.IsKeyDown(key);
        }

        /// <summary>
        /// Returns whether the key was let go, using its key code.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        /// <returns>Whether the key was let go.</returns>
        public bool IsKeyUp(short key)
        {
            return _keyboard.IsKeyUp(key) && !_keyboardLast.IsKeyUp(key);
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