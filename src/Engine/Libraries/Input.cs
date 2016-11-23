using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // Soul Engine - A game engine based on the MonoGame Framework.             //
    //                                                                          //
    // Copyright © 2016 Vlad Abadzhiev                                          //
    //                                                                          //
    // Records and allows access to user input.                                 //
    //                                                                          //
    // Refer to the documentation for any questions, or                         //
    // to TheCryru@gmail.com                                                    //
    //////////////////////////////////////////////////////////////////////////////
    class Input
    {
        #region "Declarations"
        //States
        public static KeyboardState currentFrameKeyState; //The keyboard state of the current frame. Used for button events.
        public static KeyboardState lastFrameKeyState; //The keyboard state of the last frame. Used for button events.
        public static MouseState currentFrameMouseState; //The keyboard state of the current frame. Used for mouse events.
        public static MouseState lastFrameMouseState; //The keyboard state of the last frame. Used for mouse events.
#if ANDROID //Touch is Android only.
        public static TouchCollection currentTouchState; //The touch screen state of the current frame.
#endif
        #endregion

        public static void UpdateInput()
        {
#if !ANDROID //Android doesn't have a mouse and doesn't use a keyboard in the traditional sense.

            //Record the frame's keyboard and mouse states for the current frame.
            currentFrameKeyState = Keyboard.GetState();
            currentFrameMouseState = Mouse.GetState();

            //Check if closing.
            if (isKeyDown(Keys.Escape)) Core.host.Exit();

#else //On Android we get the touchstate instead.
            currentTouchState = TouchPanel.GetState();
#endif
        }
        public static void UpdateInput_End()
        {
#if !ANDROID //For the same reason as in the other method we skip this on Android.

            //Assign this frame's code to be used as the last frame's code.
            lastFrameKeyState = currentFrameKeyState;
            lastFrameMouseState = currentFrameMouseState;

#endif
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
        /// <param name="key">The key to check..</param>
        /// <returns>bool</returns>
        public static bool isKeyUp(Keys key)
        {
            return !currentFrameKeyState.IsKeyDown(key);
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
        #region "Mouse"
        /// <summary>
        /// Returns a Vector2 of the location of the mouse pointer on the game's window.
        /// </summary>
        /// <returns></returns>
        public static Vector2 getMousePos()
        {
            return Core.maincam.ScreenToWorld(currentFrameMouseState.Position.ToVector2());
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
        public static bool isLeftClickUp(Keys key)
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
        public static bool isRightClickUp(Keys key)
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
    }
}
