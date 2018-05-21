// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Drawing;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.GLES;
using Emotion.Primitives;
using OpenTK.Input;

#endregion

namespace Emotion.Input
{
    /// <summary>
    /// Handles input from the mouse, keyboard, and other devices.
    /// </summary>
    public class Input : ContextObject
    {
        internal bool[] MouseHeld = new bool[Enum.GetValues(typeof(MouseKeys)).Length];
        internal bool[] MousePressed = new bool[Enum.GetValues(typeof(MouseKeys)).Length];
        private KeyboardState _keyboardLast;
        private KeyboardState _keyboard;
        private bool _wasNotFocused = true;
        internal Vector2 MouseLocation;

        internal Input(Context context) : base(context)
        {
            Context.Window.MouseDown += WindowMouseDown;
            Context.Window.MouseMove += (sender, e) => { MouseLocation = new Vector2(e.X, e.Y); };
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_wasNotFocused) return;
            switch (e.Button)
            {
                case MouseButton.Left:
                    MousePressed[0] = true;
                    break;
                case MouseButton.Right:
                    MousePressed[1] = true;
                    break;
                case MouseButton.Middle:
                    MousePressed[2] = true;
                    break;
            }
        }

        internal void Update()
        {
            // Transfer current to last, and clear current.
            _keyboardLast = _keyboard;

            // Reset mouse states.
            for (int i = 0; i < MouseHeld.Length; i++)
            {
                MouseHeld[i] = false;
            }
            for (int i = 0; i < MousePressed.Length; i++)
            {
                MousePressed[i] = false;
            }

            // Get current mouse states.
            MouseState state = Mouse.GetState();
            MouseHeld[0] = state.IsButtonDown(MouseButton.Left);
            MouseHeld[1] = state.IsButtonDown(MouseButton.Right);
            MouseHeld[2] = state.IsButtonDown(MouseButton.Middle);

            // Get current keyboard state.
            _keyboard = Keyboard.GetState();

            // Reset mouse states if the window is not focused.
            if (!Context.Window.Focused)
            {
                MouseHeld[0] = false;
                MouseHeld[1] = false;
                MouseHeld[2] = false;
                MousePressed[0] = false;
                MousePressed[1] = false;
                MousePressed[2] = false;
                _wasNotFocused = true;
            }
            else
            {
                _wasNotFocused = false;
            }

            // Check for fullscreen toggling key combo.
            if (IsKeyHeld("LAlt") && IsKeyDown("Enter"))
            {
                Context.Settings.WindowMode = Context.Settings.WindowMode == WindowMode.Borderless ? WindowMode.Windowed : WindowMode.Borderless;
                Context.ApplySettings();
            }

            // Check for closing combo.
            if (IsKeyDown("Escape")) Context.Quit();
        }

        #region Mouse

        /// <summary>
        /// Returns the position of the mouse cursor within the window.
        /// </summary>
        /// <param name="camera">The camera to convert to world units through, or null to return window units.</param>
        /// <returns>The position of the mouse cursor within the window.</returns>
        public Vector2 GetMousePosition(CameraBase camera = null)
        {
            Size windowSize = Context.Window.ClientSize;
            Vector2 mouseLocation = new Vector2(MouseLocation.X, MouseLocation.Y);

            int smallerValueRender = Math.Min(Context.Settings.RenderWidth, Context.Settings.RenderHeight);
            int smallerValueWindow = Math.Min(windowSize.Width, windowSize.Height);

            mouseLocation = mouseLocation * smallerValueRender / smallerValueWindow;

            if (camera == null) return mouseLocation;

            mouseLocation.X += camera.Bounds.X;
            mouseLocation.Y += camera.Bounds.Y;

            return mouseLocation;
        }

        /// <summary>
        /// Returns whether the mouse key was pressed down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key was pressed down.</returns>
        public bool IsMouseKeyDown(MouseKeys key)
        {
            return MousePressed[(int) key];
        }

        /// <summary>
        /// Returns whether the mouse key is being held down.
        /// </summary>
        /// <param name="key">The mouse key to check.</param>
        /// <returns>Whether the mouse key is being held down.</returns>
        public bool IsMouseKeyHeld(MouseKeys key)
        {
            return MouseHeld[(int) key];
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// Returns whether the key is being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        public bool IsKeyHeld(string key)
        {
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyDown(otKey) && _keyboardLast.IsKeyDown(otKey);
#if DEBUG
            Debugger.Log(MessageType.Error, MessageSource.Input, "Couldn't find key: " + key);
#endif
            return false;
        }

        /// <summary>
        /// Returns whether the key was pressed down.
        /// </summary>
        /// <param name="key">The key ot check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(string key)
        {
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyDown(otKey) && !_keyboardLast.IsKeyDown(otKey);
#if DEBUG
            Debugger.Log(MessageType.Error, MessageSource.Input, "Couldn't find key: " + key);
#endif
            return false;
        }

        #endregion
    }
}