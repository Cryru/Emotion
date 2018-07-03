// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using Emotion.Primitives;
using SharpFont;
using Glyph = Emotion.GLES.Text.Glyph;

#endregion

namespace Emotion.IO
{
    public class Font : Asset
    {
        /// <summary>
        /// An instance of the Freetype library.
        /// </summary>
        internal static Library FreeTypeLib = new Library();

        /// <summary>
        /// The font face.
        /// </summary>
        private Face _face;

        /// <summary>
        /// A collection of glyph maps for each character size.
        /// </summary>
        private Dictionary<uint, Dictionary<uint, Glyph>> _glyphMap;

        /// <summary>
        /// The size of the character last rendered.
        /// </summary>
        private uint _characterSizeLast = 10;

        internal override void Process(byte[] data)
        {
            _glyphMap = new Dictionary<uint, Dictionary<uint, Glyph>>();

            _face = new Face(FreeTypeLib, data, 0);
            _face.SelectCharmap(Encoding.Unicode);

            // Set default size.
            _face.SetPixelSizes(0, _characterSizeLast);

            base.Process(data);
        }

        /// <summary>
        /// Returns the requested glyph, if not loaded loads it.
        /// </summary>
        /// <param name="charCode">The character code of the glyph.</param>
        /// <param name="characterSize">The size of the character to render IN PIXELS.</param>
        /// <returns>A glyph object containing a rendered glyph and metadata.</returns>
        internal Glyph GetGlyph(char charCode, uint characterSize)
        {
            EnsureSize(characterSize);

            // Check if a glyph map exists for the requested size.
            if (!_glyphMap.ContainsKey(characterSize)) _glyphMap.Add(characterSize, new Dictionary<uint, Glyph>());

            // Check if the glyph map has the requested glyph.
            if (_glyphMap[characterSize].ContainsKey(charCode)) return _glyphMap[characterSize][charCode];

            // If not loaded, load it and cache it.
            Glyph g = LoadGlyph(charCode, characterSize);
            _glyphMap[characterSize].Add(charCode, g);
            return g;
        }

        #region Public API

        /// <summary>
        /// Get the kerning between two glyphs.
        /// </summary>
        /// <param name="charOne">The first glyph.</param>
        /// <param name="charTwo">The second glyph.</param>
        /// <param name="size">The font size to get kerning at.</param>
        /// <returns>The kerning between the two characters.</returns>
        public float GetKerning(char charOne, char charTwo, uint size)
        {
            // Null characters have no kerning.
            if (charOne == 0 || charTwo == 0) return 0;

            // No kerning, return 0.
            if (_face == null || !_face.HasKerning) return 0;

            // Ensure size.
            EnsureSize(size);

            // Convert the characters to glyph index.
            uint indexOne = _face.GetCharIndex(charOne);
            uint indexTwo = _face.GetCharIndex(charTwo);

            // Get the kerning.
            FTVector26Dot6 kerning = _face.GetKerning(indexOne, indexTwo, KerningMode.Default);

            // Convert to pixels.
            return (float) kerning.X;
        }

        /// <summary>
        /// Returns the size in pixels the string would use at the specified font size.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <param name="size">The font size to use to measure.</param>
        /// <returns>The size in pixels the string would use at the specified font size.</returns>
        public Vector2 MeasureString(string text, uint size)
        {
            // Split text into lines.
            string[] lines = text.Split('\n');
            Vector2 totalCalc = new Vector2(1, 1);

            // Calculate each line.
            foreach (string line in lines)
            {
                float minX = 0;
                float maxX = 0;
                float minY = 0;
                float maxY = 0;
                float x = 0;
                Vector2 lineCalc = new Vector2();

                EnsureSize(size);
                foreach (char c in line)
                {
                    Glyph glyph = GetGlyph(c, size);

                    float z = x + glyph.MinX;
                    if (minX > z) minX = z;
                    if (glyph.Advance > glyph.MaxX)
                        z = x + glyph.Advance;
                    else
                        z = x + glyph.MaxX;
                    if (maxX < z) maxX = z;
                    x += glyph.Advance;

                    if (glyph.MinY < minY) minY = glyph.MinY;

                    if (glyph.MaxY > maxY) maxY = glyph.MaxY;

                    lineCalc.X += glyph.MinX + glyph.Advance;
                }

                lineCalc.X = maxX - minX;
                lineCalc.Y = (float) _face.Size.Metrics.Ascender.ToDouble() - minY;

                // Determine whether to override total calc.
                if (lineCalc.X > totalCalc.X) 
                    totalCalc.X = lineCalc.X;

                // Determine whether this is the first line, and if not add line spacing.
                if (totalCalc.Y == 1) totalCalc.Y = lineCalc.Y;
                else
                    totalCalc.Y += GetLineSpacing(size) + lineCalc.Y;
            }

            return totalCalc;
        }

        /// <summary>
        /// Returns the distance between text lines.
        /// </summary>
        /// <param name="size">The font size to use to measure.</param>
        /// <returns>The distance between text lines.</returns>
        public int GetLineSpacing(uint size)
        {
            EnsureSize(size);
            return _face.Size.Metrics.Height.ToInt32();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Load all glyphs for a certain size. Experimental. May cause screen flickering if on the wrong thread.
        /// </summary>
        public void Preload(uint size)
        {
            for (short i = 0; i < 32767; i++)
            {
                GetGlyph((char) i, size);
            }
        }

        /// <summary>
        /// Load a glyph from the font.
        /// </summary>
        /// <param name="charCode">The character code of the glyph..</param>
        /// <param name="characterSize">The font size to load the glyph at.</param>
        /// <returns>A loaded glyph.</returns>
        private Glyph LoadGlyph(char charCode, uint characterSize)
        {
            EnsureSize(characterSize);

            // Load the glyph for the character.
            _face.LoadChar(charCode, LoadFlags.Render, LoadTarget.Normal);

            // Get the FreeType glyph.
            GlyphSlot freeTypeGlyph = _face.Glyph;
            // Get the scaled metrics.
            GlyphMetrics glyphMetrics = freeTypeGlyph.Metrics;

            // Create the SoulEngine glyph.
            Glyph result = new Glyph
            {
                MinX = (float) glyphMetrics.HorizontalBearingX.ToDouble(),
                MaxX = (float) (glyphMetrics.HorizontalBearingX.ToDouble() + glyphMetrics.Width.ToDouble()),
                MaxY = (float) glyphMetrics.HorizontalBearingY.ToDouble(),
                MinY = (float) (glyphMetrics.HorizontalBearingY.ToDouble() - Math.Ceiling(glyphMetrics.Height.ToDouble())),
                TopOffset = (float) (_face.Size.Metrics.Ascender.ToDouble() - glyphMetrics.HorizontalBearingY.ToDouble()),
                Advance = (float) glyphMetrics.HorizontalAdvance.ToDouble()
            };

            // Convert the rendered data to a bitmap.
            FTBitmap freeTypeBitmap = freeTypeGlyph.Bitmap;
            freeTypeBitmap = freeTypeBitmap.Convert(FreeTypeLib, 4);

            // Check if it isn't empty.
            if (freeTypeBitmap.Width != 0 && freeTypeBitmap.Rows != 0) result.Upload(freeTypeBitmap);

            return result;
        }

        /// <summary>
        /// Ensure the face is set to the specified size.
        /// </summary>
        /// <param name="size">The size expected.</param>
        private void EnsureSize(uint size)
        {
            if (_characterSizeLast == size) return;
            _characterSizeLast = size;
            _face.SetPixelSizes(0, _characterSizeLast);
        }

        #endregion

        internal override void Destroy()
        {
            _face.Dispose();
            base.Destroy();
        }
    }
}