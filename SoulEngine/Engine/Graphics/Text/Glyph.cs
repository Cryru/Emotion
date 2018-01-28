// SoulEngine - https://github.com/Cryru/SoulEngine

using System;
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

        public int Top;
        public int Left;

        /// <summary>
        /// Load a new glyph from bitmap data and a freetype glyph.
        /// </summary>
        /// <param name="data">The bitmap data containing the character.</param>
        /// <param name="freeTypeGlyph">The freetype object representing the glyph.</param>
        public Glyph(Bitmap data, GlyphSlot freeTypeGlyph, int top, int left)
        {
            FreeTypeGlyph = freeTypeGlyph;
            Top = top;
            Left = left;

            if (data != null)
            {
                data.Save("test.png");
                GlyphTexture = new Texture();
                GlyphTexture.Upload(data);
                data.Dispose();
            }
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