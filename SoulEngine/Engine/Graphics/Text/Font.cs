// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System;
using SharpFont;
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

        #region Private Data
        
        /// <summary>
        /// The font face.
        /// </summary>
        private Face _face;

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
        /// If the glyph is loaded, returns it from the cache, otherwise loads it.
        /// </summary>
        /// <param name="charCode">The character the glyph should be of.</param>
        /// <param name="characterSize">The size of the character to render IN PIXELS.</param>
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

            // No kerning, return 0.
            if (_face == null || !_face.HasKerning) return 0;

            // Convert the characters to glyph index.
            uint indexOne = _face.GetCharIndex(charOne);
            uint indexTwo = _face.GetCharIndex(charTwo);

            // Get the kerning.
            FTVector26Dot6 kerning = _face.GetKerning(indexOne, indexTwo, KerningMode.Unfitted);

            // X advance is already in pixels for bitmap fonts.
            if (_face.IsScalable)
            {
                return (float) kerning.X;
            }

            // Convert to pixels.
            return (float) kerning.X / 64;
        }

        #region Private Functions

        /// <summary>
        /// Loads a new glyph. Used by GetGlyph to cache glyphs.
        /// </summary>
        /// <param name="charCode">The character of the glyph to load.</param>
        /// <param name="characterSize">The size to load.</param>
        /// <returns>A loaded glyph.</returns>
        private Glyph LoadGlyph(char charCode, uint characterSize)
        {
            // Check if we have to set pixel size. This is a slow operation so we need to avoid calling it too often.
            if (_characterSizeLast != characterSize)
            {
                _face.SetPixelSizes(0, characterSize);
                _characterSizeLast = characterSize;
            }

            // Load the glyph for the character.
            _face.LoadChar(charCode, LoadFlags.Default, LoadTarget.Normal);

            // Get the FreeType glyph and render it.
            _face.Glyph.RenderGlyph(RenderMode.Normal);
            GlyphSlot freeTypeGlyph = _face.Glyph;

            // Create the SoulEngine glyph.
            GlyphMetrics glyphMetrics = freeTypeGlyph.Metrics;

            Glyph seGlyph = new Glyph(
                _face.Size.Metrics.Ascender.ToInt32() - Math.Abs(freeTypeGlyph.BitmapTop),
                glyphMetrics.HorizontalBearingX.ToInt32(),
                (int) (_face.Size.Metrics.Height.ToInt32()),
                glyphMetrics.Width.ToInt32(),
                glyphMetrics.Height.ToInt32(),
                glyphMetrics.HorizontalAdvance.ToInt32()
                );

            // Convert the rendered data to a bitmap.
            FTBitmap freeTypeBitmap = _face.Glyph.Bitmap;

            // Check if it isn't empty.
            if (freeTypeBitmap.Width != 0 && freeTypeBitmap.Rows != 0)
            {
                seGlyph.SetTexture(freeTypeBitmap.ToGdipBitmap());
            }

            return seGlyph;
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