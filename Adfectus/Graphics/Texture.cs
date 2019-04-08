#region Using

using System.IO;
using System.Numerics;
using Adfectus.Common;
using Adfectus.IO;
using FreeImageAPI;

#endregion

namespace Adfectus.Graphics
{
    /// <summary>
    /// A asset of a texture object loaded from an image.
    /// </summary>
    public class Texture : Asset
    {
        #region Properties

        /// <summary>
        /// The size of the texture.
        /// </summary>
        public Vector2 Size { get; protected set; }

        /// <summary>
        /// The texture matrix used to convert UVs.
        /// </summary>
        public Matrix4x4 TextureMatrix { get; protected set; }

        /// <summary>
        /// The OpenGL pointer of this texture.
        /// </summary>
        public uint Pointer { get; protected set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates an empty texture.
        /// </summary>
        public Texture()
        {
            Pointer = Engine.GraphicsManager.CreateTexture();
            Size = new Vector2();
        }

        /// <summary>
        /// Create a texture object from an already created texture pointer.
        /// </summary>
        /// <param name="pointer">Pointer to the texture.</param>
        /// <param name="size">The texture size.</param>
        /// <param name="name">A custom name for the texture.</param>
        /// <param name="mulMatrix">An additional matrix to be multiplied to the texture matrix.</param>
        public Texture(uint pointer, Vector2 size, string name = null, Matrix4x4? mulMatrix = null)
        {
            Name = name ?? $"OpenGL Texture {pointer}";
            Pointer = pointer;
            Size = size;
            TextureMatrix = Matrix4x4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1);

            if (mulMatrix != null) TextureMatrix *= (Matrix4x4) mulMatrix;
        }

        /// <summary>
        /// Creates a texture from an image read as bytes using FreeImage.
        /// </summary>
        /// <param name="data">The image read as bytes.</param>
        public Texture(byte[] data)
        {
            CreateAsset(data);
        }

        #endregion

        #region Asset API

        /// <summary>
        /// Uploads an array of bytes as a texture.
        /// </summary>
        /// <param name="data">The bytes to upload.</param>
        internal override void CreateAsset(byte[] data)
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

            FIBITMAP preRotation = freeImageBitmap;
            freeImageBitmap = FreeImage.Rotate(preRotation, 180);
            FreeImage.Unload(preRotation);
           
            TextureMatrix = Matrix4x4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1) * Matrix4x4.CreateScale(-1, -1, 1);

            // Even though all of the calls in the graphics manager call for execution on the GL Thread, wrapping them together like this ensures they'll be called within one loop.
            GLThread.ExecuteGLThread(() =>
            {
                Engine.GraphicsManager.BindTexture(Pointer);
                Engine.GraphicsManager.SetTextureMask(0x0000ff00, 0x00ff0000, 0xff000000, 0x000000ff);
                Engine.GraphicsManager.UploadToTexture(FreeImage.GetBits(freeImageBitmap), Size, TextureInternalFormat.Rgba8,
                    Engine.Flags.RenderFlags.TextureLoadStandard ? TexturePixelFormat.Rgba : TexturePixelFormat.Bgra);
            });

            // Cleanup FreeImage object.
            FreeImage.Unload(freeImageBitmap);
        }

        /// <summary>
        /// Destroy the texture, freeing memory.
        /// </summary>
        internal override void DestroyAsset()
        {
            Engine.GraphicsManager.DeleteTexture(Pointer);
            Pointer = 0;
        }

        #endregion

        #region Other

        /// <summary>
        /// Create a new texture with a modified matrix.
        /// </summary>
        /// <param name="matrix">The matrix to modify the texture with.</param>
        /// <returns>A new texture which is a copy of this texture but has a modified matrix.</returns>
        public Texture ModifyMatrix(Matrix4x4 matrix)
        {
            Texture newTe = new Texture
            {
                Pointer = Pointer,
                Size = Size,
                TextureMatrix = TextureMatrix * matrix,
                Name = $"{Name} Modified by {matrix}"
            };
            return newTe;
        }

        #endregion
    }
}