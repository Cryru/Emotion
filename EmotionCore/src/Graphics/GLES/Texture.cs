// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using Emotion.Engine;
using Emotion.IO;
using Emotion.Primitives;
using FreeImageAPI;
using OpenTK.Graphics.ES30;
using Matrix4 = OpenTK.Matrix4;

#endregion

namespace Emotion.Graphics.GLES
{
    public sealed class Texture : Asset, IGLObject
    {
        public Vector2 Size { get; private set; }
        public Matrix4 TextureMatrix { get; private set; }

        private int _pointer;

        public Texture()
        {
            _pointer = GL.GenTexture();
            Size = new Vector2();
        }

        #region IGLObject API

        /// <summary>
        /// Use this texture for any following operations.
        /// </summary>
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, _pointer);
        }

        /// <summary>
        /// Stop using any texture.
        /// </summary>
        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        #endregion

        #region Asset API

        /// <summary>
        /// Uploads an array of bytes as a texture.
        /// </summary>
        /// <param name="data">The bytes to upload.</param>
        internal override void Create(byte[] data)
        {
            FIBITMAP freeImageBitmap;
            uint bbp;

            // Put the bytes into a stream.
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Get the image format and load the image as a FreeImageBitmap.
                FREE_IMAGE_FORMAT fileType = FreeImage.GetFileTypeFromStream(stream);
                freeImageBitmap = FreeImage.LoadFromStream(stream, ref fileType);

                // Get the bits per pixel.
                bbp = FreeImage.GetBPP(freeImageBitmap);

                // Assign size.
                Size = new Vector2(FreeImage.GetWidth(freeImageBitmap), FreeImage.GetHeight(freeImageBitmap));
            }

            // Upload the texture on the GL thread.
            ThreadManager.ExecuteGLThread(() =>
            {
                _pointer = GL.GenTexture();
                TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1);

                // Bind the texture.
                Bind();

                // Set scaling to pixel perfect.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

                // Create a swizzle mask to convert RGBA to BGRA which for some reason is the format FreeImage spits out above.
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int) All.Blue);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int) All.Green);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int) All.Red);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int) All.Alpha);

                // Upload the texture.
                GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba8, (int) Size.X, (int) Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, FreeImage.GetBits(freeImageBitmap));

                Utils.Helpers.CheckError("uploading texture");

                // Cleanup FreeImage object.
                FreeImage.Unload(freeImageBitmap);
            });
        }

        /// <summary>
        /// Destroy the texture, freeing memory.
        /// </summary>
        internal override void Destroy()
        {
            GL.DeleteTexture(_pointer);
            _pointer = -1;
        }

        #endregion
    }
}