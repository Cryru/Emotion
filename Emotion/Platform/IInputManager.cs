#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Platform.Input;
using Emotion.Utility;

#pragma warning disable 618

#endregion

namespace Emotion.Platform
{
    /// <summary>
    /// Manages input from a mouse and keyboard, regardless of whether they are physical or virtual.
    /// On a device with a touchscreen for instance the mouse would be the touches.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Called when a key is pressed, let go, or a held event is triggered.
        /// </summary>
        EmotionEvent<Key, KeyStatus> OnKey { get; }

        /// <summary>
        /// Called when a mouse key is pressed or let go.
        /// </summary>
        EmotionEvent<MouseKey, KeyStatus> OnMouseKey { get; }

        /// <summary>
        /// Called when the mouse scrolls.
        /// </summary>
        EmotionEvent<float> OnMouseScroll { get; }

        /// <summary>
        /// Called when text input is detected. Most of the time this is identical to OnKey, but without the state.
        /// </summary>
        EmotionEvent<char> OnTextInput { get; }

        /// <summary>
        /// Returns the current mouse position. Is preprocessed by the Renderer to scale to the window if possible.
        /// Therefore it is in screen coordinates which change with the size of the Engine.Renderer.ScreenBuffer.
        /// </summary>
        Vector2 MousePosition { get; }

        /// <summary>
        /// The amount the scroll wheel is scrolled.
        /// </summary>
        /// <returns>The amount the scroll wheel is scrolled.</returns>
        float GetMouseScroll();

        /// <summary>
        /// The amount the mouse has scrolled since the last tick.
        /// </summary>
        /// <returns>The amount the mouse has scrolled since the last tick.</returns>
        float GetMouseScrollRelative();

        /// <summary>
        /// Returns whether the specified mouse key was pressed down this tick.
        /// </summary>
        /// <param name="key">To mouse key to check.</param>
        bool IsMouseKeyDown(MouseKey key);

        /// <summary>
        /// Returns whether the key was let go this tick.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        bool IsMouseKeyUp(MouseKey key);

        /// <summary>
        /// Returns whether the key is being held down this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        bool IsMouseKeyHeld(MouseKey key);

        /// <summary>
        /// Get a list of all keys which are held down this tick.
        /// </summary>
        IEnumerable<Key> GetAllKeysHeld();

        /// <summary>
        /// Get a list of all keys which were pressed down this tick.
        /// </summary>
        IEnumerable<Key> GetAllKeysDown();

        /// <summary>
        /// Returns whether the specified key was pressed down this tick.
        /// </summary>
        /// <param name="key">To key to check.</param>
        bool IsKeyDown(Key key);

        /// <summary>
        /// Returns whether the key is currently being pressed down, regardless of the state of the last tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        bool KeyState(Key key);

        /// <summary>
        /// Returns whether the key is being held down this tick.
        /// </summary>
        /// <param name="key">The key to check.</param>
        bool IsKeyHeld(Key key);

        /// <summary>
        /// Returns whether the key was let go this tick.
        /// </summary>
        /// <param name="key">The key code to check.</param>
        bool IsKeyUp(Key key);

        /// <summary>
        /// Returns the input value of the specified axis or axes.
        /// The keys are considered only if held.
        /// </summary>
        /// <param name="axis">The axis to get the value of.</param>
        Vector2 GetAxisHeld(Key axis);

        /// <summary>
        /// Returns the input value of the specified axis or axes.
        /// The keys are considered only if they were just pressed down.
        /// </summary>
        /// <param name="axis">The axis to get the value of.</param>
        Vector2 GetAxisDown(Key axis);
    }
}