#region Using

using System;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics;
using Adfectus.Graphics.Text;
using SharpFont;
using Glyph = Adfectus.Graphics.Text.Glyph;

#endregion

namespace Emotion.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public sealed class AtlasGL : Atlas
    {
        /// <summary>
        /// The FreeType Face corresponding to this atlas.
        /// </summary>
        public Face Face { get; private set; }

        public AtlasGL(byte[] fontBytes, uint fontSize, int glyphCount = 128)
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
                Texture = new TextureGL(texturePointer, new Vector2(texWidth, texWidth), Matrix4x4.CreateScale(1, -1, 1), $"{face.FamilyName} {fontSize}");
            });
        }

        public override void Dispose()
        {
            Texture.Dispose();
            Face.Dispose();
        }

        public override float GetKerning(char previousChar, char currentChar)
        {
            return (float) Face.GetKerning(previousChar, currentChar, KerningMode.Unfitted).X;
        }
    }
}