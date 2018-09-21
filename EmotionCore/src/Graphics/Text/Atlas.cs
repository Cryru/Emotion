// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using SharpFont;

#endregion

namespace Emotion.Graphics.Text
{
    public sealed class Atlas
    {
        #region Properties

        /// <summary>
        /// The glyphs loaded in the atlas. The char code corresponds to the index in the array.
        /// </summary>
        public Glyph[] Glyphs { get; private set; }

        /// <summary>
        /// The atlas texture.
        /// </summary>
        public Texture Texture { get; private set; }

        /// <summary>
        /// The FreeType Face corresponding to this atlas.
        /// </summary>
        public Face Face { get; private set; }

        /// <summary>
        /// The spacing between lines for this atlas.
        /// </summary>
        public float LineSpacing;

        #endregion

        /// <summary>
        /// Create a new font atlas.
        /// </summary>
        /// <param name="fontBytes">The bytes of the font to create an atlas from.</param>
        /// <param name="fontSize">The font size to create an atlas at.</param>
        /// <param name="glyphCount">the number of glyphs to cache.</param>
        public Atlas(byte[] fontBytes, uint fontSize, int glyphCount = 128)
        {
            Glyphs = new Glyph[glyphCount];

            Library library = new Library();
            Face face = new Face(library, fontBytes, 0);
            face.SetCharSize(0, (int) fontSize, 96, 96);

            // Get line spacing.
            LineSpacing = (float) face.Size.Metrics.Height;

            // Get max size of the atlas texture.
            int maxDimension = (int) ((1 + face.Size.Metrics.Height) * Math.Ceiling(Math.Sqrt(glyphCount)));
            int texWidth = 1;
            while (texWidth < maxDimension) texWidth <<= 1;

            byte[] pixels = new byte[texWidth * texWidth];
            int penX = 0;
            int penY = 0;

            // Cache all glyphs.
            for (int i = 0; i < glyphCount; i++)
            {
                face.LoadChar((uint) i, LoadFlags.Render | LoadFlags.ForceAutohint, LoadTarget.Light);
                FTBitmap bitmap = face.Glyph.Bitmap;

                if (penX + bitmap.Width >= texWidth)
                {
                    penX = 0;
                    penY += (int) (face.Size.Metrics.Height + 1);
                }

                // Copy pixels.
                for (int row = 0; row < bitmap.Rows; row++)
                {
                    for (int col = 0; col < bitmap.Width; col++)
                    {
                        int x = penX + col;
                        int y = penY + row;
                        pixels[y * texWidth + x] = bitmap.BufferData[row * bitmap.Pitch + col];
                    }
                }

                // Create glyph data.
                Glyphs[i] = new Glyph
                {
                    X = penX,
                    Y = penY,
                    Width = bitmap.Width,
                    Height = bitmap.Rows,

                    XOffset = face.Glyph.BitmapLeft,
                    YOffset = face.Glyph.BitmapTop,
                    MinX = (float) face.Glyph.Metrics.HorizontalBearingX.ToDouble(),
                    Advance = (float) face.Glyph.Metrics.HorizontalAdvance.ToDouble(),
                    YBearing = (float) (face.Size.Metrics.Ascender.ToDouble() - face.Glyph.Metrics.HorizontalBearingY.ToDouble()),

                    MaxX = (float) (face.Glyph.Metrics.HorizontalBearingX.ToDouble() + face.Glyph.Metrics.Width.ToDouble()),
                    MinY = (float) (face.Glyph.Metrics.HorizontalBearingY.ToDouble() - Math.Ceiling(face.Glyph.Metrics.Height.ToDouble())),
                    MaxY = (float) face.Glyph.Metrics.HorizontalBearingY.ToDouble(),
                    Ascender = (float) face.Size.Metrics.Ascender.ToDouble()
                };

                // Increment pen. Leave one pixel space.
                penX += bitmap.Width + 1;
                bitmap.Dispose();
            }

            // Assign the face.
            Face = face;

            // Create texture.
            ThreadManager.ExecuteGLThread(() => { Texture = new Texture(pixels, texWidth, texWidth, true); });
        }

        /// <summary>
        /// Destroy the atlas. Should only be called by the font managing this object.
        /// </summary>
        public void Destroy()
        {
            Texture.Destroy();
            Face.Dispose();
        }

        #region Public API

        /// <summary>
        /// Returns the size of the specified string.
        /// </summary>
        /// <param name="input">The string to measure.</param>
        /// <returns>The size of the string.</returns>
        public Vector2 MeasureString(string input)
        {
            // Check for single lined text. #28
            if (input.IndexOf('\n') == -1) input += "\n ";

            // Split text into lines.
            string[] lines = input.Split('\n');

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

                    if (glyph.MinY < minY) minY = glyph.MinY;

                    if (glyph.MaxY > maxY) maxY = glyph.MaxY;

                    lineCalc.X += glyph.MinX + glyph.Advance;
                }

                lineCalc.X = maxX - minX;
                lineCalc.Y = -minY;

                // Determine whether to override total calc.
                if (lineCalc.X > totalCalc.X)
                    totalCalc.X = lineCalc.X;

                // Determine whether this is the first line, and if not add line spacing.
                if (totalCalc.Y == 1) totalCalc.Y = lineCalc.Y;
                else
                    totalCalc.Y += LineSpacing + lineCalc.Y;
            }

            return totalCalc;
        }

        /// <summary>
        /// Returns the horizontal kerning of two letters.
        /// </summary>
        /// <param name="previousChar">The previous character.</param>
        /// <param name="currentChar">The current character.</param>
        /// <returns>The kerning between the previous and current characters.</returns>
        public float GetKerning(char previousChar, char currentChar)
        {
            return (float) Face.GetKerning(previousChar, currentChar, KerningMode.Unfitted).X;
        }

        #endregion
    }
}