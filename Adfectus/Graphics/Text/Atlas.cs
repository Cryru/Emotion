#region Using

using System;
using System.Numerics;
using Adfectus.Common;
using SharpFont;

#endregion

namespace Adfectus.Graphics.Text
{
    /// <summary>
    /// A font atlas is a texture which contains all the letters from a font in a certain size.
    /// </summary>
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
        public float LineSpacing { get; set; }

        /// <summary>
        /// The highest a letter can be in this atlas.
        /// </summary>
        public float Ascent { get; set; }

        #endregion

        static Atlas()
        {
            FT.Init(Bootstrapper.LoadedLibraries["freetype"]);
        }

        /// <summary>
        /// Create a new font atlas.
        /// </summary>
        /// <param name="fontBytes">The bytes of the font to create an atlas from.</param>
        /// <param name="fontSize">The font size in points to create an atlas at.</param>
        /// <param name="glyphCount">the number of glyphs to cache.</param>
        public Atlas(byte[] fontBytes, uint fontSize, int glyphCount = 128)
        {
            Glyphs = new Glyph[glyphCount];

            Library library = new Library();
            Face face = new Face(library, fontBytes, 0);
            face.SetCharSize(0, (int) fontSize, 96, 96);

            // Get line spacing.
            LineSpacing = (float) face.Size.Metrics.Height;
            Ascent = (float) face.Size.Metrics.Ascender.ToDouble();
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
                    MaxY = (float) face.Glyph.Metrics.HorizontalBearingY.ToDouble()
                };

                // Increment pen. Leave one pixel space.
                penX += bitmap.Width + 1;
                bitmap.Dispose();
            }

            // Assign the face.
            Face = face;

            // Create texture.
            GLThread.ExecuteGLThread(() =>
            {
                uint texturePointer = Engine.GraphicsManager.CreateTexture();
                Engine.GraphicsManager.BindTexture(texturePointer);
                // Set the red channel to the alpha channel and set all others to 1.
                Engine.GraphicsManager.SetTextureMask(0xffffffff, 0xffffffff, 0xffffffff, 0xff000000);
                Engine.GraphicsManager.UploadToTexture(pixels, new Vector2(texWidth, texWidth), TextureInternalFormat.R8, TexturePixelFormat.Red);
                Texture = new Texture(texturePointer, new Vector2(texWidth, texWidth), $"{face.FamilyName} {fontSize}", Matrix4x4.CreateScale(1, -1, 1));
            });
        }

        /// <summary>
        /// Destroy the atlas. Should only be called by the font managing this object.
        /// </summary>
        public void Destroy()
        {
            Texture.DestroyAsset();
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
        public float GetKerning(char previousChar, char currentChar)
        {
            return (float) Face.GetKerning(previousChar, currentChar, KerningMode.Unfitted).X;
        }

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