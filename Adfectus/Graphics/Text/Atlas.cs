#region Using

using System;
using System.Numerics;

#endregion

namespace Adfectus.Graphics.Text
{
    /// <summary>
    /// A font atlas is a texture which contains all the letters from a font in a certain size.
    /// </summary>
    public abstract class Atlas
    {
        #region Properties

        /// <summary>
        /// The glyphs loaded in the atlas. The char code corresponds to the index in the array.
        /// </summary>
        public Glyph[] Glyphs { get; protected set; }

        /// <summary>
        /// The atlas texture.
        /// </summary>
        public Texture Texture { get; protected set; }

        /// <summary>
        /// The spacing between lines for this atlas.
        /// </summary>
        public float LineSpacing { get; set; }

        /// <summary>
        /// The highest a letter can be in this atlas.
        /// </summary>
        public float Ascent { get; set; }

        #endregion

        /// <summary>
        /// Destroy the atlas. Should only be called by the font managing this object.
        /// </summary>
        public abstract void Dispose();

        #region Public API

        /// <summary>
        /// Returns the size of the specified string.
        /// </summary>
        /// <param name="input">The string to measure.</param>
        /// <returns>The size of the string.</returns>
        public virtual Vector2 MeasureString(string input)
        {
            // Split text into lines.
            string[] lines = input.Split('\n');

            Vector2 totalCalc = new Vector2(0, 0);

            // Calculate each line.
            foreach (string line in lines)
            {
                float minX = 0;
                float maxX = 0;
                float x = 0;
                float lineWidth = 0;

                foreach (char c in line)
                {
                    Glyph glyph = Glyphs[c];

                    float z = x + glyph.MinX;
                    if (minX > z) minX = z;
                    if (glyph.Advance > glyph.MaxX)
                        z = x + glyph.Advance;
                    else
                        z = x + glyph.MaxX;
                    if (maxX < z) maxX = z;
                    x += glyph.Advance;

                    lineWidth += glyph.MinX + glyph.Advance;
                }

                lineWidth = maxX - minX;

                // Determine whether to override total calc.
                if (lineWidth > totalCalc.X)
                    totalCalc.X = lineWidth;

                // Add another line.
                totalCalc.Y += LineSpacing;
            }

            return totalCalc;
        }

        /// <summary>
        /// Returns the horizontal kerning of two letters.
        /// </summary>
        /// <param name="previousChar">The previous character.</param>
        /// <param name="currentChar">The current character.</param>
        /// <returns>The kerning between the previous and current characters.</returns>
        public abstract float GetKerning(char previousChar, char currentChar);

        #endregion

        /// <summary>
        /// Converts a pixel font size to a point character size.
        /// </summary>
        /// <param name="pixelSize">The pixel size to convert.</param>
        /// <returns>The provided pixel size in points.</returns>
        public static float PixelFontSizeToCharSize(uint pixelSize)
        {
            return (float) Math.Pow(96 / 72 / pixelSize, -1);
        }
    }
}