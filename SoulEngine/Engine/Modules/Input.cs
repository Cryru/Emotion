// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

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
            //            _keysLastFrame = new Dictionary<Keyboard.Key, bool>();
            //            _mouseButtons = new Dictionary<Mouse.Button, bool>();
            //            _mouseButtonsLastFrame = new Dictionary<Mouse.Button, bool>();

            //            // Get a list of keys.
            //            Keyboard.Key[] keyList = Enum.GetValues(typeof(Keyboard.Key)).Cast<Keyboard.Key>().ToArray();
            //            Mouse.Button[] mouseButtonList = Enum.GetValues(typeof(Mouse.Button)).Cast<Mouse.Button>().ToArray();

            //            // Generate the key dictionary.
            //            foreach (Keyboard.Key k in keyList)
            //            {
            //                _keys.Add(k, false);
            //                _keysLastFrame.Add(k, false);
            //            }

            //            // Generate the mouse dictionary.
            //            foreach (Mouse.Button b in mouseButtonList)
            //            {
            //                _mouseButtons.Add(b, false);
            //                _mouseButtonsLastFrame.Add(b, false);
            //            }

            //            // Hook up to the mouse wheel event.
            //            Core.NativeContext.MouseWheelScrolled += NativeContext_MouseWheelScrolled;
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
            return (int) _mouse.ScrollWheelValue - _mouseLastFrame.ScrollWheelValue;
        }

        /// <summary>
        /// Returns the location of the mouse within the window.
        /// </summary>
        /// <returns>The location of the mouse as Vector2 in window coordinates.</returns>
        public static Vector2 MouseLocation()
        {
            return new Vector2(Core.BreathWin.Mouse.X, Core.BreathWin.Mouse.Y);
        }

        #endregion
    }
}