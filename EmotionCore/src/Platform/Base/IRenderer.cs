// Emotion - https://github.com/Cryru/Emotion

using Emotion.Game.Objects.Camera;
using Emotion.Primitives;

namespace Emotion.Platform.Base
{
    /// <summary>
    /// Handles rendering.
    /// </summary>
    public interface IRenderer
    {
        #region Rectangle Drawing

        /// <summary>
        /// Draws a rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">Whether to draw through the current camera, or on the screen.</param>
        void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true);

        #endregion

        #region Camera

        /// <summary>
        /// Sets the renderer's camera.
        /// </summary>
        /// <param name="camera">The camera to bind to the renderer.</param>
        void SetCamera(CameraBase camera);

        /// <summary>
        /// Returns the renderer's camera.
        /// </summary>
        /// <returns>The currently bound to the renderer camera.</returns>
        CameraBase GetCamera();

        #endregion
    }
}