// SoulEngine - https://github.com/Cryru/SoulEngine

#region Using

using System.Drawing;
using Breath.Objects;
using OpenTK;
using OpenTK.Graphics.OpenGL;

#endregion

namespace Soul.Engine.Graphics.Text
{
    public class Glyph
    {
        /// <summary>
        /// An OpenGL texture holding the glyph.
        /// </summary>
        public Texture GlyphTexture;

        #region Metrics

        /// <summary>
        /// The offset from the top so the glyph lays on the baseline within the SE rendering.
        /// </summary>
        public int TopOffset { get; }

        /// <summary>
        /// The distance between the previous pen position and the next one. Subtract the BearingX to obtain the distance to next.
        /// </summary>
        public int Advance { get; }

        /// <summary>
        /// The space between neew lines.
        /// </summary>
        public int NewLineSpace { get; }

        /// <summary>
        /// The width of the glyph.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the glyph.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The distance between the previous pen position and the start of the glyph.
        /// </summary>
        public int BearingX;

        #endregion

        /// <summary>
        /// Load a new glyph.
        /// </summary>
        public Glyph(int topOffset, int bearingX, int newLineSpace, int width, int height, int advance)
        {
            TopOffset = topOffset;
            BearingX = bearingX;
            NewLineSpace = newLineSpace;
            Width = width;
            Height = height;
            Advance = advance;
        }

        /// <summary>
        /// Sets the glyph's texture.
        /// </summary>
        /// <param name="bitmap"></param>
        public void SetTexture(Bitmap bitmap)
        {
            
            GlyphTexture = new Texture();
            GlyphTexture.Use();
            // Apply a swizzle mask so the letters can be colored.
            // Red - 0x1903
            // Green - 0x1904
            // Blue - 0x1905
            // Alpha - 0x1906
            //int[] swizzleMask = { 0x1906, 0x1906, 0x1906, 0x1906 };
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, swizzleMask);
            GlyphTexture.StopUsing();
            GlyphTexture.Upload(bitmap);
            // Overwrite the texture model matrix.
            GlyphTexture.TextureModelMatrix = Matrix4.Identity;
            bitmap.Dispose();
        }

        /// <summary>
        /// Destroy the glyph.
        /// </summary>
        public void Dispose()
        {
            GlyphTexture.Destroy();
            GlyphTexture = null;
        }
    }
}