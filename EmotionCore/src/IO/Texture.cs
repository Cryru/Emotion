// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using Emotion.Engine;
using Emotion.GLES;
using Emotion.Utils;
using FreeImageAPI;
using OpenTK;
using OpenTK.Graphics.ES30;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// An image loaded into memory which can be drawn to the screen.
    /// </summary>
    /// <inheritdoc cref="ITexture" />
    public sealed class Texture : Asset, ITexture
    {
        #region Properties

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Matrix4 TextureMatrix { get; private set; }

        #endregion

        private int _pointer { get; set; }

        #region Asset API

        internal override void Process(byte[] data)
        {
            FIBITMAP processedBytes;

            // Put the bytes into a stream.
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Load a free image bitmap from memory.
                FIBITMAP freeImageBitmap = FreeImage.ConvertTo32Bits(FreeImage.LoadFromStream(stream));

                // Assign size.
                Width = (int) FreeImage.GetWidth(freeImageBitmap);
                Height = (int) FreeImage.GetHeight(freeImageBitmap);

                processedBytes = freeImageBitmap;
            }

            // Upload the texture on the GL thread.
            ThreadManager.ExecuteGLThread(() =>
            {
                _pointer = GL.GenTexture();
                TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Width * 2, Height * 2, 0, 0, 1);

                // Bind the texture.
                Use();

                // Set scaling to pixel perfect.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

                // Create a swizzle mask to convert RGBA to BGRA which for some reason is the format FreeImage spits out above.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int) All.Blue);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int) All.Green);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int) All.Red);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int) All.Alpha);

                // Upload the texture.
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, FreeImage.GetBits(processedBytes));

                Helpers.CheckError("uploading texture");

                // Cleanup processed data.
                processedBytes.SetNull();
            });

            base.Process(data);
        }

        internal override void Destroy()
        {
            Cleanup();
            base.Destroy();
        }

        #endregion

        #region Texture API

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, _pointer);
        }

        public void Cleanup()
        {
            ThreadManager.ExecuteGLThread(() => { GL.DeleteTexture(_pointer); });
        }

        #endregion
    }
}