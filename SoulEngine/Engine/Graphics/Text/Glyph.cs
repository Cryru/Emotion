// SoulEngine - https://github.com/Cryru/SoulEngine

using System.Drawing;
using Breath.Objects;
using SharpFont;

namespace Soul.Engine.Graphics.Text
{
    public class Glyph
    {
        /// <summary>
        /// An OpenGL texture holding the glyph.
        /// </summary>
        public Texture GlyphTexture;
        /// <summary>
        /// A Freetype object with glyph data.
        /// </summary>
        public GlyphSlot FreeTypeGlyph;

        /// <summary>
        /// Load a new glyph from bitmap data and a freetype glyph.
        /// </summary>
        /// <param name="data">The bitmap data containing the character.</param>
        /// <param name="freeTypeGlyph">The freetype object representing the glyph.</param>
        public Glyph(Bitmap data, GlyphSlot freeTypeGlyph)
        {
            FreeTypeGlyph = freeTypeGlyph;

            GlyphTexture = new Texture(null);
            GlyphTexture.Upload(data);
        }

        /// <summary>
        /// Destroy the glyph.
        /// </summary>
        public void Dispose()
        {
            FreeTypeGlyph = null;
            GlyphTexture.Destroy();
            GlyphTexture = null;
        }
    }
}