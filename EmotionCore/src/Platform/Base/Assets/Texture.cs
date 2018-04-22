// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.IO;
using Emotion.Platform.Base.Interfaces;
using FreeImageAPI;

#endregion

namespace Emotion.Platform.Base.Assets
{
    /// <summary>
    /// An image loaded into memory which can be drawn to the screen. Managed by the platform's asset loader.
    /// </summary>
    public abstract class Texture : Destroyable
    {
        #region Properties

        /// <summary>
        /// The width of the texture in pixels.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The height of the texture in pixels.
        /// </summary>
        public int Height { get; protected set; }

        #endregion

        /// <summary>
        /// Create an empty texture.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        protected Texture(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create a texture from a file. This is used by the asset loader.
        /// </summary>
        /// <param name="context">The context which is loading the texture.</param>
        /// <param name="data">The bytes representing the file the texture is loaded from.</param>
        protected Texture(Context context, byte[] data)
        {
            // Put the bytes into a stream.
            using (MemoryStream stream = new MemoryStream(data))
            {
                // Load a free image bitmap from memory.
                FIBITMAP freeImageBitmap = FreeImage.LoadFromStream(stream);

                // Assign size.
                Width = (int) FreeImage.GetWidth(freeImageBitmap);
                Height = (int) FreeImage.GetHeight(freeImageBitmap);

                using (MemoryStream bmpConverted = new MemoryStream())
                {
                    // Convert it to a BMP format.
                    FreeImage.SaveToStream(freeImageBitmap, bmpConverted, FREE_IMAGE_FORMAT.FIF_BMP);

                    // Get the converted bytes and convert to a pointer.
                    byte[] memoryToBytes = bmpConverted.ToArray();

                    // Parse on the platform.
                    ParseData(context, memoryToBytes);
                }

                freeImageBitmap.SetNull();
            }
        }

        protected abstract void ParseData(Context context, byte[] data);
    }
}