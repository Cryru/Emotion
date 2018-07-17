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
        private bool _noFocus;
        internal Vector2 MouseLocation;

        internal Input(Context context) : base(context)
        {
            Context.Window.MouseDown += WindowMouseDown;
            Context.Window.MouseUp += WindowMouseUp;
            // Moves an internal mouse position based on the window which is more accurate than directly polling the mouse.
            Context.Window.MouseMove += (sender, e) => { MouseLocation = new Vector2(e.X, e.Y); };
            // Sets the unfocused tag.
            Context.Window.FocusedChanged += (sender, e) =>
            {
                if (!Context.Window.Focused) _noFocus = true;
            };
        }

        /// <summary>
        /// Handles the window mouse down event in order to determine a button has been pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_noFocus) return;
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

        /// <summary>
        /// Handles the window mouse down event in order to determine a button is no longer held.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_noFocus) return;
            switch (e.Button)
            {
                case MouseButton.Left:
                    MouseHeld[0] = false;
                    break;
                case MouseButton.Right:
                    MouseHeld[1] = false;
                    break;
                case MouseButton.Middle:
                    MouseHeld[2] = false;
                    break;
            }
        }

        internal void Update()
        {
            // Transfer current to last, and clear current.
            _keyboardLast = _keyboard;

            // Reset mouse states and transfer pressed to held.
            for (int i = 0; i < MousePressed.Length; i++)
            {
                if (MousePressed[i]) MouseHeld[i] = MousePressed[i];
                MousePressed[i] = false;
            }

            // Check if focus has returned and skip input loop if no focus.
            if (Context.Window.Focused && _noFocus) _noFocus = false;
            if (_noFocus)
            {
                for (int i = 0; i < MousePressed.Length; i++)
                {
                    MousePressed[i] = false;
                    MouseHeld[i] = false;
                }

                return;
            }

            // Get current keyboard state.
            _keyboard = Keyboard.GetState();

            // Check for fullscreen toggling key combo.
            if (IsKeyHeld("LAlt") && IsKeyDown("Enter"))
            {
                Context.Settings.WindowMode = Context.Settings.WindowMode == WindowMode.Borderless ? WindowMode.Windowed : WindowMode.Borderless;
                //todo Context.ApplySettings();
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
        /// Returns whether the key is being held down, using its name.
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
        /// Returns whether the key was pressed down, using its name.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        public bool IsKeyDown(string key)
        {
            if (Enum.TryParse(key, out Key otKey)) return _keyboard.IsKeyDown(otKey) && !_keyboardLast.IsKeyDown(otKey);
#if DEBUG
            Debugger.Log(MessageType.Error, MessageSource.Input, "Couldn't find key: " + key);
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
            Debugger.Log(MessageType.Error, MessageSource.Input, "Couldn't find key: " + key);
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