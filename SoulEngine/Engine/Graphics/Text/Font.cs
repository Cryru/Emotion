// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using SharpFont;
using Soul.Engine.Enums;
using Soul.Engine.Modules;

#endregion

namespace Soul.Engine.Graphics.Text
{
    /// <summary>
    /// A font object, loaded by FreeType.
    /// </summary>
    public class Font
    {
        #region Public Data

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name;
        #endregion

        #region Data
        
        /// <summary>
        /// The font face.
        /// </summary>
        public Face _face;

        /// <summary>
        /// A cache of glyphs.
        /// </summary>
        private GlyphTable _cachedGlyphs;

        #endregion

        #region Cache

        /// <summary>
        /// The size of the character last rendered.
        /// </summary>
        private uint _characterSizeLast = 10;

        #endregion

        /// <summary>
        /// Create a font from a byte array representing a .ttf font.
        /// </summary>
        /// <param name="data">A byte array to create the font from</param>
        public Font(byte[] data)
        {
            _face = new Face(AssetLoader.FreeTypeLib, data, 0);
            _face.SelectCharmap(Encoding.Unicode);

            Name = _face.FamilyName;

            _cachedGlyphs = new GlyphTable(Name);

            // Set default size.
            _face.SetPixelSizes(0, _characterSizeLast);
        }

        /// <summary>
        /// Returns a glyph.
        /// </summary>
        /// <param name="charCode">The charcode of the glyph to return.</param>
        /// <param name="characterSize">The size of the character to render.</param>
        /// <returns>A glyph object containing a rendered glyph and metadata.</returns>
        public Glyph GetGlyph(char charCode, uint characterSize)
        {
            // Check if we have the glyph cached already.
            if (_cachedGlyphs.HasGlyph(charCode, characterSize)) return _cachedGlyphs.GetGlyph(charCode, characterSize);

            // If not loaded, load it and cache it.
            Glyph g = LoadGlyph(charCode, characterSize);
            _cachedGlyphs.AddGlyph(charCode, characterSize, g);
            return g;
        }

        /// <summary>
        /// Get the kerning between two glyphs.
        /// </summary>
        /// <param name="charOne">The first glyph.</param>
        /// <param name="charTwo">The second glyph.</param>
        /// <returns>The kerning between the two characters.</returns>
        public float GetKerning(char charOne, char charTwo)
        {
            // Null characters have no kerning.
            if (charOne == 0 || charTwo == 0) return 0;

            if (_face != null && _face.HasKerning)
            {
                // Convert the characters to glyph index.
                uint indexOne = _face.GetCharIndex(charOne);
                uint indexTwo = _face.GetCharIndex(charTwo);

                // Get the kerning.
                FTVector26Dot6 kerning = _face.GetKerning(indexOne, indexTwo, KerningMode.Default);

                // X advance is already in pixels for bitmap fonts.
                if (_face.IsScalable)
                {
                    return (float) kerning.X;
                }

                // Convert to pixels.
                return (float) kerning.X / (1 << 6);
            }

            // No kerning, or error.
            return 0;
        }

        /// <summary>
        /// Get the line spacing of the font.
        /// </summary>
        /// <returns>The spacing between the lines.</returns>
        public float GetFaceSpacing()
        {
            if (_face != null)
            {
                return (float) _face.Size.Metrics.Height / (1 << 6);
            }

            return 0;
        }

        #region Local

        private Glyph LoadGlyph(char charCode, uint characterSize)
        {
            // Check if we have to set pixel size. This is a slow operation so we need to avoid calling it too often.
            if (_characterSizeLast != characterSize)
            {
                _face.SetPixelSizes(0, characterSize);
                _characterSizeLast = characterSize;
            }

            _face.LoadChar(charCode, LoadFlags.Default, LoadTarget.Normal);

            GlyphSlot freeTypeGlyph = _face.Glyph;
            freeTypeGlyph.RenderGlyph(RenderMode.Normal);

            // Convert the character to bitmap.
            FTBitmap freeTypeBitmap = _face.Glyph.Bitmap;

            if (freeTypeBitmap.Width == 0 && freeTypeBitmap.Rows == 0)
            {
                return new Glyph(null, freeTypeGlyph, _face.Glyph.BitmapTop, _face.Glyph.BitmapLeft);
            }

            Glyph g =  new Glyph(freeTypeBitmap.ToGdipBitmap(), freeTypeGlyph, _face.Glyph.BitmapTop, _face.Glyph.BitmapLeft);
            freeTypeBitmap.Dispose();
            return g;
        }

        #endregion

        /// <summary>
        /// Destroy the font.
        /// </summary>
        public void Dispose()
        {
            // Clear the cached glyphs.
            _cachedGlyphs.Dispose();

            // Clear the free type font.
            _face.Dispose();
        }
    }
}