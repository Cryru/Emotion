// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using SharpFont;

#endregion

namespace Emotion.Graphics.Text
{
    public sealed class Atlas
    {
        public Glyph[] Glyphs { get; private set; }
        public Texture Texture { get; private set; }
        public Face Face { get; private set; }

        /// <summary>
        /// Create a new font atlas.
        /// </summary>
        /// <param name="fontBytes">The bytes of the font to create an atlas from.</param>
        /// <param name="fontSize">The font size to create an atlas at.</param>
        /// <param name="glyphCount">the number of glyphs to cache.</param>
        public Atlas(byte[] fontBytes, int fontSize, int glyphCount = 128)
        {
            Glyphs = new Glyph[glyphCount];

            Library library = new Library();
            Face face = new Face(library, fontBytes, 0);
            face.SetCharSize(0, fontSize, 96, 96);

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
                    YBearing = (float) (face.Size.Metrics.Ascender.ToDouble() - face.Glyph.Metrics.HorizontalBearingY.ToDouble())
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

        #region Text API

        public Vector2 MeasureString(string input)
        {
            return Vector2.Zero;
        }

        public float GetNewLineSpacing()
        {
            return 0f;
        }

        #endregion
    }
}