// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Raya.Enums;
using Raya.Events;
using Raya.Input;
using Raya.Primitives;

#endregion

namespace Soul.Engine.Modules
{
    public static class Input
    {
        #region Keyboard

        /// <summary>
        /// List of keys and their status for the current frame.
        /// </summary>
        private static Dictionary<Keyboard.Key, bool> _keys;

        /// <summary>
        /// List of keys and their status for the last frame.
        /// </summary>
        private static Dictionary<Keyboard.Key, bool> _keysLastFrame;

        #endregion

        #region Mouse

        /// <summary>
        /// List of mouse buttons and their status for the current frame.
        /// </summary>
        private static Dictionary<Mouse.Button, bool> _mouseButtons;

        /// <summary>
        /// List of mouse buttons and their status for the last frame.
        /// </summary>
        private static Dictionary<Mouse.Button, bool> _mouseButtonsLastFrame;

        /// <summary>
        /// The position of the mouse cursor.
        /// </summary>
        public static Vector2 MousePosition
        {
            get
            {
                // Check if focused.
                if (!Core.NativeContext.Window.Focused) return _mousePosition;

                Vector2 pos = Mouse.GetPosition(Core.NativeContext.Window);
                pos = (Vector2) Core.NativeContext.Window.MapPixelToCoords(pos);

                // Check if within window.
                if (pos.X >= 0 && pos.Y >= 0 && pos.X <= Core.NativeContext.Window.Size.X &&
                    pos.Y <= Core.NativeContext.Window.Size.Y)
                {
                    _mousePosition = pos;
                    return pos;
                }

                // If it wasn't within the window return 0,0.
                return _mousePosition;
            }
            set
            {
                // Check if focused.
                if (!Core.NativeContext.Window.Focused) return;

                Mouse.SetPosition(value, Core.NativeContext.Window);
            }
        }

        private static Vector2 _mousePosition;

        /// <summary>
        /// The amount of mouse wheel scrolling done this frame.
        /// </summary>
        private static int _mouseScroll;

        #endregion

        #region Trackers

        /// <summary>
        /// Whether window was focused in the last frame. Used to track the first mouse click within the window.
        /// </summary>
        private static bool _lastFrameFocused;

        #endregion

        /// <summary>
        /// Setup the module.
        /// </summary>
        static Input()
        {
            // Define dictionaries.
            _keys = new Dictionary<Keyboard.Key, bool>();
            _keysLastFrame = new Dictionary<Keyboard.Key, bool>();
            _mouseButtons = new Dictionary<Mouse.Button, bool>();
            _mouseButtonsLastFrame = new Dictionary<Mouse.Button, bool>();

            // Get a list of keys.
            Keyboard.Key[] keyList = Enum.GetValues(typeof(Keyboard.Key)).Cast<Keyboard.Key>().ToArray();
            Mouse.Button[] mouseButtonList = Enum.GetValues(typeof(Mouse.Button)).Cast<Mouse.Button>().ToArray();

            // Generate the key dictionary.
            foreach (Keyboard.Key k in keyList)
            {
                _keys.Add(k, false);
                _keysLastFrame.Add(k, false);
            }

            // Generate the mouse dictionary.
            foreach (Mouse.Button b in mouseButtonList)
            {
                _mouseButtons.Add(b, false);
                _mouseButtonsLastFrame.Add(b, false);
            }

            // Hook up to the mouse wheel event.
            Core.NativeContext.MouseWheelScrolled += NativeContext_MouseWheelScrolled;
        }

        /// <summary>
        /// Update the module.
        /// </summary>
        public static void Update()
        {
            if (!Core.Paused || !Core.NativeContext.Window.Focused)
            {
                // Update key data.
                foreach (Keyboard.Key key in _keys.Keys.ToList())
                {
                    // Get the new and old values.
                    bool oldValue = _keys[key];
                    bool newValue = Keyboard.IsKeyPressed(key);

                    // Update the last frame data.
                    _keysLastFrame[key] = oldValue;

                    // Update the current frame data.
                    _keys[key] = newValue;
                }

                // Update mouse data.
                foreach (Mouse.Button button in _mouseButtons.Keys.ToList())
                {
                    // Get the new and old values.
                    bool oldValue = _mouseButtons[button];
                    bool newValue = Mouse.IsButtonPressed(button);

                    // Update the last frame data.
                    _mouseButtonsLastFrame[button] = oldValue;

                    // Update the current frame data.
                    _mouseButtons[button] = newValue;
                }

                // Reset scroll.
                _mouseScroll = 0;

                // Check if alt+enter are pressed for fullscreen.
                if (KeyHeld(Keyboard.Key.LAlt) && KeyPressed(Keyboard.Key.Return))
                    Core.NativeContext.Window.Mode = Core.NativeContext.Window.Mode == WindowMode.Window
                        ? WindowMode.Borderless
                        : WindowMode.Window;

                // Check if escape is pressed to exit.
                if (KeyPressed(Keyboard.Key.Escape))
                    Core.NativeContext.Quit();
            }


            // Track focus.
            _lastFrameFocused = Core.NativeContext.Window.Focused;
        }

        #region Keyboard

        /// <summary>
        /// Returns true when the key is pressed.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True when the key is pressed.</returns>
        public static bool KeyPressed(Keyboard.Key key)
        {
            // Check if focused.
            if (!Core.NativeContext.Window.Focused) return false;

            // If the key was not pressed last frame and now is, then it was pressed.
            return !_keysLastFrame[key] && _keys[key];
        }

        /// <summary>
        /// Returns whether the key is being held.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is being held, false otherwise.</returns>
        public static bool KeyHeld(Keyboard.Key key)
        {
            // Check if focused.
            if (!Core.NativeContext.Window.Focused) return false;

            return _keysLastFrame[key] && _keys[key];
        }

        #endregion

        #region Mouse

        /// <summary>
        /// Returns true when the button is pressed.
        /// </summary>
        /// <param name="button">The key to check.</param>
        /// <returns>True when the button is pressed.</returns>
        public static bool MouseButtonPressed(Mouse.Button button)
        {
            // Check if the window was focused in the last frame.
            if (!_lastFrameFocused) return false;

            // If the key was not pressed last frame and now is, then it was pressed.
            return !_mouseButtonsLastFrame[button] && _mouseButtons[button];
        }

        /// <summary>
        /// Returns whether the button is being held.
        /// </summary>
        /// <param name="button">The key to check.</param>
        /// <returns>True if the button is being held, false otherwise.</returns>
        public static bool MouseButtonHeld(Mouse.Button button)
        {
            return _mouseButtonsLastFrame[button] && _mouseButtons[button];
        }

        /// <summary>
        /// Returns the amount of mouse wheel scrolling done this frame.
        /// </summary>
        /// <returns></returns>
        public static int MouseWheelScroll()
        {
            return _mouseScroll;
        }

        /// <summary>
        /// The Raya mouse wheel event we are hooking up to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void NativeContext_MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            _mouseScroll = (int) e.Delta;
        }

        #endregion
    }
}