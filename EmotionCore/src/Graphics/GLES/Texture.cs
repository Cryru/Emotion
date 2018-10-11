// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;
using FreeImageAPI;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.GLES
{
    public class Texture : Asset, IGLObject
    {
        #region Properties

        /// <summary>
        /// The size of the texture.
        /// </summary>
        public Vector2 Size { get; protected set; }

        /// <summary>
        /// The texture matrix used to convert UVs.
        /// </summary>
        public Matrix4 TextureMatrix { get; protected set; }

        /// <summary>
        /// The OpenGL pointer of this texture.
        /// </summary>
        public int Pointer { get; protected set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates an empty texture.
        /// </summary>
        public Texture()
        {
            ThreadManager.ExecuteGLThread(() => { Pointer = GL.GenTexture(); });
            Size = new Vector2();
        }

        /// <summary>
        /// Creates a texture from an image read as bytes using FreeImage.
        /// </summary>
        /// <param name="data">The image read as bytes.</param>
        public Texture(byte[] data)
        {
            Create(data);
        }

        /// <summary>
        /// Creates a custom texture from bytes and other data.
        /// </summary>
        /// <param name="data">The bytes to upload to OpenGL.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="componentCount">The texture's component count.</param>
        /// <param name="format">The format of the bytes.</param>
        /// <param name="textureMatrix">An additional matrix to multiply the texture matrix by.</param>
        public Texture(byte[] data, int width, int height, TextureComponentCount componentCount, PixelFormat format, Matrix4? textureMatrix = null)
        {
            Name = "Custom Texture";
            Size = new Vector2(width, height);
            TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1);
            if (textureMatrix != null) TextureMatrix *= (Matrix4) textureMatrix;
            CreateFromBytes(data, componentCount, format);
        }

        /// <summary>
        /// Create a texture for an Atlas object. Uploads everything to the red channel and shifts it to the alpha channel. The
        /// texture matrix also has a reverse Y coordinate.
        /// </summary>
        /// <param name="data">The atlas bytes.</param>
        /// <param name="width">The width of the atlas.</param>
        /// <param name="height">The height of the atlas.</param>
        /// <param name="atlasTextureName">The name of the font atlas this texture belongs to.</param>
        public Texture(byte[] data, int width, int height, string atlasTextureName)
        {
            Name = "Atlas Texture " + atlasTextureName;
            Size = new Vector2(width, height);
            TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1) * Matrix4.CreateScale(1, -1, 1);
            CreateForGlyph(data);
        }

        #endregion

        #region IGLObject API

        /// <summary>
        /// Use this texture for any following operations on slot 0.
        /// </summary>
        public void Bind()
        {
            Bind(0);
        }

        /// <summary>
        /// Use this texture for any following operations.
        /// </summary>
        /// <param name="slot">Which slot to bind in. 0-16 (32 on some systems)</param>
        public void Bind(int slot)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, Pointer);
        }

        /// <summary>
        /// Stop using any texture.
        /// </summary>
        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Delete the texture freeing memory.
        /// </summary>
        public virtual void Delete()
        {
            ThreadManager.ExecuteGLThread(() => { GL.DeleteTexture(Pointer); });
            Pointer = -1;
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

            // Put the bytes into a stream.
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Get the image format and load the image as a FreeImageBitmap.
                FREE_IMAGE_FORMAT fileType = FreeImage.GetFileTypeFromStream(stream);
                freeImageBitmap = FreeImage.ConvertTo32Bits(FreeImage.LoadFromStream(stream, ref fileType));

                // Assign size.
                Size = new Vector2(FreeImage.GetWidth(freeImageBitmap), FreeImage.GetHeight(freeImageBitmap));
            }

            // Upload the texture on the GL thread.
            ThreadManager.ExecuteGLThread(() =>
            {
                Pointer = GL.GenTexture();
                TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1);

                // Bind the texture.
                Bind(0);

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

                Helpers.CheckError("uploading texture");

                // Cleanup FreeImage object.
                FreeImage.Unload(freeImageBitmap);
            });
        }

        /// <summary>
        /// Destroy the texture, freeing memory.
        /// </summary>
        internal override void Destroy()
        {
            Delete();
        }

        #endregion

        #region Other

        /// <summary>
        /// Create a texture from bytes.
        /// </summary>
        /// <param name="data">The texture data.</param>
        /// <param name="componentCount">The component count.</param>
        /// <param name="format">The texture format.</param>
        private void CreateFromBytes(byte[] data, TextureComponentCount componentCount, PixelFormat format)
        {
            Pointer = GL.GenTexture();

            // Bind the texture.
            Bind(0);

            // Set scaling to pixel perfect.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

            // Upload the texture.
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, componentCount, (int) Size.X, (int) Size.Y, 0, format, PixelType.UnsignedByte, data);

            Helpers.CheckError("uploading texture");
        }

        /// <summary>
        /// Creates a texture for a glyph.
        /// </summary>
        /// <param name="data"></param>
        private void CreateForGlyph(byte[] data)
        {
            Pointer = GL.GenTexture();

            // Bind the texture.
            Bind(0);

            // Set scaling to pixel perfect.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

            // Set the red channel to the alpha channel and set all others to 1.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleR, (int) All.One);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleG, (int) All.One);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleB, (int) All.One);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleA, (int) All.Red);

            // Upload the texture.
            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.R8, (int) Size.X, (int) Size.Y, 0, PixelFormat.Red, PixelType.UnsignedByte, data);

            Helpers.CheckError("uploading texture");
        }

        #endregion
    }
}