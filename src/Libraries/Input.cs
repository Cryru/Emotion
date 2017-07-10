using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Events;
using SoulEngine.Modules;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Location, Size, and Rotation.
    /// </summary>
    class Input
    {
        #region "States"
        public static KeyboardState currentFrameKeyState; //The keyboard state of the current frame. Used for button events.
        public static KeyboardState lastFrameKeyState; //The keyboard state of the last frame. Used for button events.
        public static MouseState currentFrameMouseState; //The keyboard state of the current frame. Used for mouse events.
        public static MouseState lastFrameMouseState; //The keyboard state of the last frame. Used for mouse events.
#if ANDROID
        public static TouchCollection currentTouchState; //The touch screen state of the current frame.
#endif
        #endregion

        #region "Events"
        /// <summary>
        /// Triggered when some sort of text input is detected.
        /// </summary>
        public static event EventHandler<TextInputEventArgs> OnTextInput;
        /// <summary>
        /// Triggered when a button is pressed.
        /// </summary>
        public static event EventHandler<KeyEventArgs> OnKeyDown;
        /// <summary>
        /// Triggered when a button is let go.
        /// </summary>
        public static event EventHandler<KeyEventArgs> OnKeyUp;
        /// <summary>
        /// Triggered when the mouse moves.
        /// </summary>
        public static event EventHandler<MouseMoveEventArgs> OnMouseMove;
        /// <summary>
        /// Triggered when the mouse wheel is scrolled.
        /// </summary>
        public static event EventHandler<MouseScrollEventArgs> OnMouseScroll;
        /// <summary>
        /// Triggered when a mouse button is pressed.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> OnMouseButtonDown;
        /// <summary>
        /// Triggered when a mouse button is let go.
        /// </summary>
        public static event EventHandler<MouseButtonEventArgs> OnMouseButtonUp;
        #endregion

        /// <summary>
        /// Gets input data for the current frame.
        /// </summary>
        public static void UpdateInput()
        {
            //Record the frame's keyboard and mouse states for the current frame.
            currentFrameKeyState = Keyboard.GetState();
            currentFrameMouseState = Mouse.GetState();

            List<Keys> keysDownNow = currentFrameKeyState.GetPressedKeys().ToList();
            List<Keys> keysDownBefore = lastFrameKeyState.GetPressedKeys().ToList();

            //Check for keys being pressed and let go events.
            for (int i = 0; i < keysDownNow.Count; i++)
            {
                if(keysDownBefore.IndexOf(keysDownNow[i]) == -1)
                {
                    OnKeyDown?.Invoke(null, new KeyEventArgs(keysDownNow[i]));
                }
            }

            for (int i = 0; i < keysDownBefore.Count; i++)
            {
                if (keysDownNow.IndexOf(keysDownBefore[i]) == -1)
                {
                    OnKeyUp?.Invoke(null, new KeyEventArgs(keysDownBefore[i]));
                }
            }

            //Check for mouse move event.
            if(currentFrameMouseState.Position != lastFrameMouseState.Position)
            {
                Vector2 StartPosition = lastFrameMouseState.Position.ToVector2();
                Vector2 EndPosition = currentFrameMouseState.Position.ToVector2();

                // Check if a window manager is loaded, in which case we want to warp the mouse coordintes through its screen matrix.
                if (Context.Core.isModuleLoaded<WindowManager>())
                {
                    StartPosition = Context.Core.Module<WindowManager>().Screen.ScreenToView(StartPosition);
                    EndPosition = Context.Core.Module<WindowManager>().Screen.ScreenToView(EndPosition);
                }

                OnMouseMove?.Invoke(null, new MouseMoveEventArgs(StartPosition, EndPosition));
            }

            //Check for scrolling events.
            if(currentFrameMouseState.ScrollWheelValue != lastFrameMouseState.ScrollWheelValue)
            {
                int scrollDif = lastFrameMouseState.ScrollWheelValue - currentFrameMouseState.ScrollWheelValue;

                OnMouseScroll?.Invoke(null, new MouseScrollEventArgs(scrollDif));
            }

            //Check for mouse key events.
            // Left Button
            if (lastFrameMouseState.LeftButton == ButtonState.Released &&
                 currentFrameMouseState.LeftButton == ButtonState.Pressed)
            {
                OnMouseButtonDown?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Left));
            }
            if (lastFrameMouseState.LeftButton == ButtonState.Pressed &&
                currentFrameMouseState.LeftButton == ButtonState.Released)
            {
                OnMouseButtonUp?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Left));
            }
            // Right Button
            if (lastFrameMouseState.RightButton == ButtonState.Released &&
                 currentFrameMouseState.RightButton == ButtonState.Pressed)
            {
                OnMouseButtonDown?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Right));
            }
            if (lastFrameMouseState.RightButton == ButtonState.Pressed &&
                currentFrameMouseState.RightButton == ButtonState.Released)
            {
                OnMouseButtonUp?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Right));
            }
            // Middle
            if (lastFrameMouseState.MiddleButton == ButtonState.Released &&
                currentFrameMouseState.MiddleButton == ButtonState.Pressed)
            {
                OnMouseButtonDown?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Middle));
            }
            if (lastFrameMouseState.MiddleButton == ButtonState.Pressed &&
                currentFrameMouseState.MiddleButton == ButtonState.Released)
            {
                OnMouseButtonUp?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Middle));
            }
            // Forth
            if (lastFrameMouseState.XButton1 == ButtonState.Released &&
                currentFrameMouseState.XButton1 == ButtonState.Pressed)
            {
                OnMouseButtonDown?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Forth));
            }
            if (lastFrameMouseState.XButton1 == ButtonState.Pressed &&
                currentFrameMouseState.XButton1 == ButtonState.Released)
            {
                OnMouseButtonUp?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Forth));
            }
            // Fifth
            if (lastFrameMouseState.XButton2 == ButtonState.Released &&
                currentFrameMouseState.XButton2 == ButtonState.Pressed)
            {
                OnMouseButtonDown?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Fifth));
            }
            if (lastFrameMouseState.XButton2 == ButtonState.Pressed &&
                currentFrameMouseState.XButton2 == ButtonState.Released)
            {
                OnMouseButtonUp?.Invoke(null, new MouseButtonEventArgs(Enums.MouseButton.Fifth));
            }

            //Check if closing.
            if (isKeyDown(Keys.Escape)) Context.Core.Exit();

            //Check if toggling fullscreen.
            if (KeyDownTrigger(Keys.Enter) && isKeyDown(Keys.LeftAlt))
            {
                if (Settings.DisplayMode == Enums.DisplayMode.Windowed) Settings.DisplayMode = Enums.DisplayMode.BorderlessFullscreen;
                else
                    Settings.DisplayMode = Enums.DisplayMode.Windowed;
            }
        }
        /// <summary>
        /// At the end of the frame moves the current frame variables to the last frame input variables.
        /// </summary>
        public static void UpdateInput_End()
        {
            //Assign this frame's code to be used as the last frame's code.
            lastFrameKeyState = currentFrameKeyState;
            lastFrameMouseState = currentFrameMouseState;
        }

        #region "Functions"
        #region "Keyboard"
        /// <summary>
        /// Returns a bool based on whether the specified key is held down or not.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>bool</returns>
        public static bool isKeyDown(Keys key)
        {
            return currentFrameKeyState.IsKeyDown(key);
        }
        /// <summary>
        /// Inverse of the isKeyDown function.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>bool</returns>
        public static bool isKeyUp(Keys key)
        {
            return !isKeyDown(key);
        }
        /// <summary>
        /// Returns true only on the frame that the button is pressed and not when held.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns></returns>
        public static bool KeyDownTrigger(Keys key)
        {
            if (currentFrameKeyState.IsKeyDown(key) == true && lastFrameKeyState.IsKeyDown(key) == false)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true only on the frame that the button is let go after being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns></returns>
        public static bool KeyUpTrigger(Keys key)
        {
            if (currentFrameKeyState.IsKeyUp(key) == true && lastFrameKeyState.IsKeyUp(key) == false)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region "Keyboard MultiKey"
        /// <summary>
        /// Returns a bool based on whether any of the specified keys is held down or not.
        /// </summary>
        /// <param name="key">The keys to check.</param>
        /// <returns>bool</returns>
        public static bool isKeyDown(Keys[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if(currentFrameKeyState.IsKeyDown(key[i]) == true) return true;
            }
            return false;
        }
        /// <summary>
        /// Inverse of the isKeyDown function.
        /// </summary>
        /// <param name="key">The keys to check.</param>
        /// <returns>bool</returns>
        public static bool isKeyUp(Keys[] key)
        {
            return !isKeyDown(key);
        }
        /// <summary>
        /// Returns true only on the frame that one of the buttons is pressed and not when held.
        /// </summary>
        /// <param name="key">The keys to check.</param>
        /// <returns></returns>
        public static bool KeyDownTrigger(Keys[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if(currentFrameKeyState.IsKeyDown(key[i]) == true && lastFrameKeyState.IsKeyDown(key[i]) == false)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns true only on the frame that one of the buttons is let go after being held down.
        /// </summary>
        /// <param name="key">The keys to check.</param>
        /// <returns></returns>
        public static bool KeyUpTrigger(Keys[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (currentFrameKeyState.IsKeyUp(key[i]) == true && lastFrameKeyState.IsKeyUp(key[i]) == false)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region "Mouse"
        /// <summary>
        /// Returns a Vector2 of the location of the mouse pointer on the game's window.
        /// </summary>
        /// <returns></returns>
        public static Vector2 getMousePos()
        {
            // Check if a window manager is loaded, in which case we want to warp the mouse through the screen matrix.
            if(Context.Core.isModuleLoaded<WindowManager>())
            {
                return Context.Core.Module<WindowManager>().Screen.ScreenToView(currentFrameMouseState.Position.ToVector2());
            }
            else
            {
                return currentFrameMouseState.Position.ToVector2();
            }
        }
        #region "Left Button"
        /// <summary>
        /// Returns a bool based on whether the left mouse button is held down or not.
        /// </summary>
        /// <returns>bool</returns>
        public static bool isLeftClickDown()
        {
            return currentFrameMouseState.LeftButton == ButtonState.Pressed;
        }
        /// <summary>
        /// Inverse of the isLeftClickDown function.
        /// </summary>
        /// <returns>bool</returns>
        public static bool isLeftClickUp()
        {
            return currentFrameMouseState.LeftButton == ButtonState.Released;
        }
        /// <summary>
        /// Returns true only on the frame that the left mouse button is pressed and not when held.
        /// </summary>
        public static bool LeftClickDownTrigger()
        {
            if (currentFrameMouseState.LeftButton == ButtonState.Pressed && lastFrameMouseState.LeftButton == ButtonState.Released)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true only on the frame that the left mouse button is let go after being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns></returns>
        public static bool LeftClickUpTrigger()
        {
            if (currentFrameMouseState.LeftButton == ButtonState.Released && lastFrameMouseState.LeftButton == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region "Right Button"
        /// <summary>
        /// Returns a bool based on whether the right mouse button is held down or not.
        /// </summary>
        /// <returns>bool</returns>
        public static bool isRightClickDown()
        {
            return currentFrameMouseState.RightButton == ButtonState.Pressed;
        }
        /// <summary>
        /// Inverse of the isRightClickDown function.
        /// </summary>
        /// <returns>bool</returns>
        public static bool isRightClickUp()
        {
            return currentFrameMouseState.RightButton == ButtonState.Released;
        }
        /// <summary>
        /// Returns true only on the frame that the right mouse button is pressed and not when held.
        /// </summary>
        public static bool RightClickDownTrigger()
        {
            if (currentFrameMouseState.RightButton == ButtonState.Pressed && lastFrameMouseState.RightButton == ButtonState.Released)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Returns true only on the frame that the right mouse button is let go after being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns></returns>
        public static bool RightClickUpTrigger()
        {
            if (currentFrameMouseState.RightButton == ButtonState.Released && lastFrameMouseState.RightButton == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }
        #endregion
        #endregion
        #endregion

        #region "Event Wrapper"
        public static void triggerTextInput(object sender, TextInputEventArgs e)
        {
            OnTextInput?.Invoke(sender, e);
        }
#endregion
    }
}
