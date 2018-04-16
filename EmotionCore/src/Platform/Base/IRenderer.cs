// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base.Assets;
using Emotion.Primitives;

#endregion

namespace Emotion.Platform.Base
{
    /// <summary>
    /// Handles rendering.
    /// </summary>
    public interface IRenderer
    {
        #region Primary Functions

        /// <summary>
        /// Clear everything drawn on the screen.
        /// </summary>
        void Clear(Color color);

        /// <summary>
        /// Swaps window buffers, displaying everything rendered to the window.
        /// </summary>
        void Present();

        #endregion

        #region Texture Drawing

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true);

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawTexture(Texture texture, Rectangle location, bool camera = true);

        #endregion

        #region Primitive Drawing

        /// <summary>
        /// Draws a rectangle outline on the current render target, which by default is the window.
        /// </summary>
        /// <param name="rect">The rectangle to outline.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true);

        /// <summary>
        /// Draws a filled rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawRectangle(Rectangle rect, Color color, bool camera = true);

        /// <summary>
        /// Draws a line on the current render target, which by default is the window.
        /// </summary>
        /// <param name="start">The line's starting point.</param>
        /// <param name="end">The line's ending point.</param>
        /// <param name="color">The line's color.</param>
        /// <param name="camera">
        /// Whether the line's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true);

        #endregion

        #region Text Drawing

        /// <summary>
        /// Draws text on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="camera">
        /// Whether the text's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawText(Font font, string text, Color color, Vector2 location, int size, bool camera = true);

        /// <summary>
        /// Draws a text array with line spacing, on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="text">The text array to render, each item will be on a separate line.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="camera">
        /// Whether the text's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        void DrawText(Font font, string[] text, Color color, Vector2 location, int size, bool camera = true);

        #endregion

        #region Camera

        /// <summary>
        /// Set the renderer's camera.
        /// </summary>
        /// <param name="camera">The camera the renderer should use.</param>
        void SetCamera(CameraBase camera);

        /// <summary>
        /// Returns the renderer's camera.
        /// </summary>
        /// <returns>The camera the renderer is currently using.</returns>
        CameraBase GetCamera();

        #endregion
    }
}