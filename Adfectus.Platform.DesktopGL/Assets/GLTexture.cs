#region Using

using System.IO;
using System.Numerics;
using Adfectus.Common;
using Adfectus.Graphics;
using FreeImageAPI;

#endregion

namespace Adfectus.Platform.DesktopGL.Assets
{
    /// <inheritdoc />
    public class GLTexture : Texture
    {
        /// <summary>
        /// The OpenGL pointer of the texture.
        /// </summary>
        public uint Pointer;

        /// <inheritdoc />
        public GLTexture()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Wrap an already created texture in a GLTexture object.
        /// </summary>
        /// <param name="pointer">Pointer to the created OpenGL texture.</param>
        /// <param name="size">The size of the texture.</param>
        /// <param name="mulMatrix">An additional matrix to be multiplied to the texture matrix.</param>
        /// <param name="name">The name of the texture.</param>
        public GLTexture(uint pointer, Vector2 size, Matrix4x4? mulMatrix = null, string name = null)
        {
            Pointer = pointer;
            Size = size;
            TextureMatrix = Matrix4x4.CreateOrthographicOffCenter(0, Size.X * 2, Size.Y * 2, 0, 0, 1);
            Name = name ?? $"OpenGL Texture - {Pointer}";
            Created = true;

            if (mulMatrix != null) TextureMatrix *= (Matrix4x4) mulMatrix;
        }

        protected override void CreateInternal(byte[] data)
        {
            // Create a GL pointer to the new texture.
            Pointer = Engine.GraphicsManager.CreateTexture();

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
                    TexturePixelFormat.Rgba);
            });

            // Cleanup FreeImage object.
            FreeImage.Unload(freeImageBitmap);
        }

        protected override void DisposeInternal()
        {
            Engine.GraphicsManager.DeleteTexture(Pointer);
        }

        /// <inheritdoc />
        public override Texture FlipHorizontal()
        {
            GLTexture newTe = new GLTexture
            {
                Pointer = Pointer,
                Size = Size,
                TextureMatrix = TextureMatrix * Matrix4x4.CreateScale(-1, 1, 1),
                Name = $"{Name} - Flipped Horizontally"
            };
            return newTe;
        }

        /// <inheritdoc />
        public override Texture FlipVertical()
        {
            GLTexture newTe = new GLTexture
            {
                Pointer = Pointer,
                Size = Size,
                TextureMatrix = TextureMatrix * Matrix4x4.CreateScale(1, -1, 1),
                Name = $"{Name} - Flipped Vertically"
            };
            return newTe;
        }

        /// <inheritdoc />
        public override Texture ModifyMatrix(Matrix4x4 mat, bool replace = false)
        {
            GLTexture newTe = new GLTexture
            {
                Pointer = Pointer,
                Size = Size,
                TextureMatrix = replace ? mat : TextureMatrix * mat,
                Name = $"{Name} - Modified by Matrix" + (replace ? " and Replaced" : "")
            };
            return newTe;
        }
    }
}