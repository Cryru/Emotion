// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.IO;
using Emotion.Primitives;

#endregion

namespace Emotion.GLES.Text
{
    /// <summary>
    /// A text drawing session. Used as an object holder by the Renderer when rendering text.
    /// </summary>
    public class TextDrawingSession
    {
        public Vector2 Size { get; internal set; }
        public Vector2 MasterOffset { get; set; }

        internal Font Font;
        internal Renderer Renderer;

        /// <summary>
        /// The size of the font.
        /// </summary>
        internal uint FontSize;

        /// <summary>
        /// The x location of the glyph currently being rendered.
        /// </summary>
        private float _locY;

        /// <summary>
        /// The y location of the glyph currently being rendered.
        /// </summary>
        private float _locX;

        /// <summary>
        /// The previous character to the one currently being rendered.
        /// </summary>
        private char _prevChar;

        /// <summary>
        /// The cache to render to.
        /// </summary>
        private RenderTarget _renderCache;

        /// <summary>
        /// Whether to use caching.
        /// </summary>
        internal bool UseCache = true;

        /// <summary>
        /// Add a glyph to a text rendering session. The glyph is automatically positioned to be after the previous one, but can be
        /// additionally offset.
        /// </summary>
        /// <param name="glyphChar">The glyph to add.</param>
        /// <param name="color">The glyph color.</param>
        /// <param name="xOffset">The x offset to render the glyph at.</param>
        /// <param name="yOffset">The y offset to render the glyph at.</param>
        public void AddGlyph(char glyphChar, Color color, int xOffset = 0, int yOffset = 0)
        {
            // Switch to drawing to the cache.
            if (UseCache)
            {
                Renderer.DrawOn(_renderCache);
            }

            // Get the glyph requested under the specified size.
            Glyph glyph = Font.GetGlyph(glyphChar, FontSize);

            // Find its render bounds.
            Rectangle des = BoundsOfNextGlyph(glyphChar, xOffset, yOffset);
            _locX = des.X;
            _locY = des.Y;

            // If it posses a texture draw it.
            if (glyph.HasTexture)
            {
                des.Y += glyph.TopOffset;
                des.Width = glyph.Width;
                des.Height = glyph.Height;
                Renderer.DrawTexture(glyph, des, color, false);
            }

            // Add advance.
            _locX += glyph.Advance;// - glyph.MinX;

            _locX -= (int) MasterOffset.X;
            _locY -= (int) MasterOffset.Y;

            // Restore drawing.
            if (UseCache) Renderer.DrawOnScreen();
        }

        /// <summary>
        /// Returns the bounds of the next glyph.
        /// </summary>
        /// <param name="glyphChar">The character to check.</param>
        /// <param name="xOffset">The x offset.</param>
        /// <param name="yOffset">The y offset.</param>
        /// <returns></returns>
        public Rectangle BoundsOfNextGlyph(char glyphChar, int xOffset = 0, int yOffset = 0)
        {
            // Add master offset.
            xOffset += (int) MasterOffset.X;
            yOffset += (int) MasterOffset.Y;

            // Get the requested glyph.
            Glyph glyph = Font.GetGlyph(glyphChar, FontSize);
            Rectangle glyphBounds = new Rectangle();

            // Get kerning.
            float kerning = 0;
            if (_prevChar != default(char)) kerning = Font.GetKerning(_prevChar, glyphChar, FontSize);
            _prevChar = glyphChar;

            // Push glyph to the right.
            glyphBounds.X = _locX + glyph.MinX + kerning;

            // Add user offsets.
            glyphBounds.X += xOffset;
            glyphBounds.Y = yOffset;

            // Offset to top left render origin.
            glyphBounds.Y += _locY;

            // Set size.
            glyphBounds.Size = Font.MeasureString(glyphChar.ToString(), FontSize);
            
            return glyphBounds;
        }

        /// <summary>
        /// Move the X and Y offsets of the next glyphs without drawing anything.
        /// </summary>
        /// <param name="xOffset">The amount to move on the X offset.</param>
        /// <param name="yOffset">The amount to move on the Y offset.</param>
        public void Push(int xOffset = 0, int yOffset = 0)
        {
            _locX += xOffset;
            _locY += yOffset;
        }

        /// <summary>
        /// Add a new line to the text rendering session.
        /// </summary>
        public void NewLine()
        {
            _locX = 0;
            _locY += Font.GetLineSpacing(FontSize);
        }

        /// <summary>
        /// Return the state of the session as a texture. Any old states returned are automatically freed from memory.
        /// </summary>
        /// <returns>A texture state of the text drawing session.</returns>
        public ITexture GetTexture()
        {
            return _renderCache;
        }

        /// <summary>
        /// Reset the text drawing session, clearing any exported results, and the current state.
        /// </summary>
        public void Reset()
        {
            // Check if using cache.
            if (UseCache)
            {
                // Create a cache if missing.
                if (_renderCache == null) _renderCache = new RenderTarget((int) Size.X, (int) Size.Y);
                // Clear the cache.
                Renderer.DrawOn(_renderCache);
                Renderer.ClearTarget();
                Renderer.DrawOnScreen();
            }

            _locX = 0;
            _locY = 0;
            _prevChar = '\0';
        }

        /// <summary>
        /// Destroy the session freeing up resources.
        /// </summary>
        /// <param name="destroyCache">Whether to destroy the render cache as well, clearing any obtained textures.</param>
        public void Destroy(bool destroyCache = false)
        {
            if (destroyCache) _renderCache?.Cleanup();
        }
    }
}