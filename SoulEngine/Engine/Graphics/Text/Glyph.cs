// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Drawing;
using Breath.Objects;

#endregion

namespace Soul.Engine.Graphics.Text
{
    public class Glyph
    {
        /// <summary>
        /// An OpenGL texture holding the glyph.
        /// </summary>
        public Texture GlyphTexture;

        #region Metrics

        /// <summary>
        /// The size of the glyph bitmap.
        /// </summary>
        public int Top { get; }

        /// <summary>
        /// The offset from the top so the glyph lays on the baseline.
        /// </summary>
        public int TopOffset { get; }

        /// <summary>
        /// The glyph advance AKA where the next glyph should begin.
        /// </summary>
        public int Advance { get; }

        /// <summary>
        /// The maximum size a glyph can be within the font.
        /// </summary>
        public int FontMax { get; }

        /// <summary>
        /// The width of the glyph.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the glyph.
        /// </summary>
        public int Height { get; }

        #endregion

        /// <summary>
        /// Load a new glyph.
        /// </summary>
        public Glyph(int top, int topOffset, int advance, int fontMax, int width, int height)
        {
            Top = top;
            TopOffset = topOffset;
            Advance = advance;
            FontMax = fontMax;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Sets the glyph's texture.
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetTexture(Bitmap bitmap)
        {
            GlyphTexture = new Texture();
            GlyphTexture.Upload(bitmap);
            bitmap.Dispose();
        }

        /// <summary>
        /// Destroy the glyph.
        /// </summary>
        public void Dispose()
        {
            GlyphTexture.Destroy();
            GlyphTexture = null;
        }
    }
}