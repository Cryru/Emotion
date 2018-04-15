// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Objects.Camera;
using Emotion.Primitives;

#endregion

namespace Emotion.Platform.Base
{
    /// <summary>
    /// Handles input from the mouse, keyboard, and other devices.
    /// </summary>
    public interface IInput
    {
        #region Mouse

        /// <summary>
        /// Returns the position of the mouse cursor within the window.
        /// </summary>
        /// <param name="camera">The camera to convert to world units through, or null to return window units.</param>
        /// <returns>The position of the mouse cursor within the window.</returns>
        Vector2 GetMousePosition(CameraBase camera = null);

        #endregion

        #region Keyboard

        /// <summary>
        /// Returns whether the key is being held down.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>Whether the key is being held down.</returns>
        bool IsKeyHeld(string key);

        /// <summary>
        /// Returns whether the key was pressed down.
        /// </summary>
        /// <param name="key">The key ot check.</param>
        /// <returns>Whether the key was pressed down.</returns>
        bool IsKeyDown(string key);

        #endregion
    }
}