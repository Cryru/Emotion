#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Image.BMP;
using Emotion.Standard.Image.PNG;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Represents an image. Supports all formats supported by Emotion.Standard.Image
    /// </summary>
    public class TextureAsset : Asset
    {
        /// <summary>
        /// The asset's uploaded graphics texture.
        /// </summary>
        public Texture Texture { get; set; }

        public TextureAsset()
        {
        }

        /// <summary>
        /// Create a fake texture asset from a texture.
        /// </summary>
        /// <param name="texture"></param>
        public TextureAsset(Texture texture)
        {
            Texture = texture;
            Name = $"Synthesized TextureAsset - {texture.Pointer}";
        }

        protected override void CreateInternal(ReadOnlyMemory<byte> data)
        {
            byte[] pixels = null;
            Vector2 size = Vector2.Zero;
            var flipped = false; // Whether the image was uploaded flipped - top to bottom.
            var format = PixelFormat.Unknown;

            PerfProfiler.ProfilerEventStart("Decoding Image", "Loading");

            // Check if PNG.
            if (PngFormat.IsPng(data))
            {
                pixels = PngFormat.Decode(data, out PngFileHeader header);
                size = header.Size;
                format = header.PixelFormat;
            }
            // Check if BMP.
            else if (BmpFormat.IsBmp(data))
            {
                pixels = BmpFormat.Decode(data, out BmpFileHeader header);
                size = new Vector2(header.Width, header.HeaderSize);
                flipped = true;
                format = PixelFormat.Bgra;
            }

            if (pixels == null || size.X == 0 || size.Y == 0)
            {
                Engine.Log.Warning($"Couldn't load texture - {Name}.", MessageSource.AssetLoader);
                return;
            }

            Size = pixels.Length;
            PerfProfiler.ProfilerEventEnd("Decoding Image", "Loading");
            UploadTexture(size, pixels, flipped, format);
        }

        protected virtual void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat pixelFormat)
        {
            GLThread.ExecuteGLThread(() =>
            {
                PerfProfiler.ProfilerEventStart($"Uploading Image {Name}", "Loading");
                Texture = new Texture(size, pixels, pixelFormat)
                {
                    FlipY = flipped
                };
                PerfProfiler.ProfilerEventEnd($"Uploading Image {Name}", "Loading");
            });
        }

        protected override void DisposeInternal()
        {
            Texture.Dispose();
            Texture = null;
        }
    }
}