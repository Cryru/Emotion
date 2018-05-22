// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Engine;
using Emotion.Utils;
using OpenTK;
using OpenTK.Graphics.ES30;
using SharpFont;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.GLES.Text
{
    /// <summary>
    /// An image loaded into memory which can be drawn to the screen.
    /// </summary>
    public sealed class Glyph : ITexture
    {
        #region Properties

        public Matrix4 TextureMatrix { get; private set; }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool HasTexture { get; private set; }

        #endregion

        #region Metrics

        public float MaxX { get; internal set; }
        public float MinX { get; internal set; }
        public float MaxY { get; internal set; }
        public float MinY { get; internal set; }
        public float Advance { get; internal set; }
        public float TopOffset { get; internal set; }

        #endregion

        private int _pointer;

        public void Upload(FTBitmap glyphBitmap)
        {
            byte[] source = glyphBitmap.BufferData;

            // Check if the bitmap was exported as a two level grey, in which case we need to overwrite the 1 bit value with the maximum byte value.
            if (glyphBitmap.GrayLevels == 2 && glyphBitmap.PixelMode == PixelMode.Gray)
                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i] == 1) source[i] = 255;
                }

            ThreadManager.ExecuteGLThread(() =>
            {
                _pointer = GL.GenTexture();
                HasTexture = true;
                Width = glyphBitmap.Width;
                Height = glyphBitmap.Rows;
                TextureMatrix = Matrix4.CreateScale(1, -1, 1) * Matrix4.CreateOrthographicOffCenter(0, Width * 2, Height * 2, 0, 0, 1);

                // Bind the texture.
                Use();

                // Create a swizzle mask to convert R to AAAA.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int) All.One);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int) All.One);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int) All.One);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int) All.Red);

                // Set scaling to pixel perfect.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

                // Upload the texture.
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba, Width, Height, 0, PixelFormat.Red, PixelType.UnsignedByte, source);

                Helpers.CheckError("uploading glyph texture");
            });
        }

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, _pointer);
        }

        public void Cleanup()
        {
            GL.DeleteTexture(_pointer);
        }
    }
}