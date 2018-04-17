// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.Base.Objects;
using Emotion.Primitives;

#endregion

namespace Emotion.Platform.Base
{
    /// <summary>
    /// Handles rendering.
    /// </summary>
    public abstract class Renderer
    {
        #region Properties

        /// <summary>
        /// The resolution to render at.
        /// </summary>
        public abstract Vector2 RenderSize { get; protected set; }

        /// <summary>
        /// The renderer's camera.
        /// </summary>
        public abstract CameraBase Camera { get; set; }

        #endregion

        #region Primary Functions

        /// <summary>
        /// Clear everything drawn on the screen.
        /// </summary>
        public abstract void Clear(Color color);

        /// <summary>
        /// Swaps window buffers, displaying everything rendered to the window.
        /// </summary>
        public abstract void Present();

        #endregion

        #region Texture Drawing

        
        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="opacity">How opaque the texture should be. 0 is invisible, 1 is transparent.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public abstract void DrawTexture(Texture texture, Rectangle location, Rectangle source, float opacity, bool camera = true);

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
        public abstract void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true);

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public abstract void DrawTexture(Texture texture, Rectangle location, bool camera = true);

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
        public abstract void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true);

        /// <summary>
        /// Draws a filled rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public abstract void DrawRectangle(Rectangle rect, Color color, bool camera = true);

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
        public abstract void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true);

        #endregion

        #region Text Drawing

        /// <summary>
        /// Begin a text rendering session.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="fontSize">The font size to use.</param>
        /// <param name="width">The width of the final resulting texture.</param>
        /// <param name="height">The height of the final resulting texture.</param>
        /// <returns>A text drawing session.</returns>
        public abstract TextDrawingSession StartTextSession(Font font, int fontSize, int width, int height);

        /// <summary>
        /// Draws text on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        /// <param name="camera">
        /// Whether the text's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public abstract void DrawText(Font font, int size, string text, Color color, Vector2 location, bool camera = true);

        /// <summary>
        /// Draws a text array with line spacing, on the current render target, which by default is the window.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size to use.</param>
        /// <param name="text">The text array to render, each item will be on a separate line.</param>
        /// <param name="color">The text color.</param>
        /// <param name="location">The point to start rendering from.</param>
        /// <param name="camera">
        /// Whether the text's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        public abstract void DrawText(Font font, int size, string[] text, Color color, Vector2 location, bool camera = true);

        #endregion
    }
}