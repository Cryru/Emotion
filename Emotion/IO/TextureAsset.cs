#region Using

using Emotion.Common.Threading;
using Emotion.Graphics.Objects;
using Emotion.Standard.Image.BMP;
using Emotion.Standard.Image.ImgBin;
using Emotion.Standard.Image.PNG;
using OpenGL;
using System.Buffers;
#if MORE_IMAGE_TYPES
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#endif

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Represents an image. Supports all formats supported by Emotion.Standard.Image
    /// </summary>
    public class TextureAsset : Asset, IHotReloadableAsset
    {
        /// <summary>
        /// The asset's uploaded graphics texture.
        /// </summary>
        public Texture Texture { get; set; }

        private bool _pixelsMemoryIsRented;

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

            // Magic number check to find out format.
            if (PngFormat.IsPng(data))
            {
                pixels = PngFormat.Decode(data, out PngFileHeader header);
                size = header.Size;
                format = header.PixelFormat;
            }
            else if (BmpFormat.IsBmp(data))
            {
                pixels = BmpFormat.Decode(data, out BmpFileHeader header);
                size = new Vector2(header.Width, header.HeaderSize);
                flipped = true;
                format = PixelFormat.Bgra;
            }
            else if (ImgBinFormat.IsImgBin(data))
            {
                pixels = ImgBinFormat.Decode(data, out ImgBinFileHeader header);
                size = header.Size;
                format = header.Format;
                _pixelsMemoryIsRented = true;
            }
#if MORE_IMAGE_TYPES
            else
            {
                ReadOnlySpan<byte> span = data.Span;
                Image<Bgra32> image = Image.Load<Bgra32>(span);
                pixels = new byte[image.Width * image.Height * 4];
                image.CopyPixelDataTo(pixels);
                size = new Vector2(image.Width, image.Height);
                format = PixelFormat.Bgra;
                image.Dispose();
            }
#endif

            if (pixels == null || size.X == 0 || size.Y == 0)
            {
                Texture = Texture.EmptyWhiteTexture;
                Engine.Log.Warning($"Couldn't load texture - {Name}.", MessageSource.AssetLoader);
                return;
            }

            ByteSize = pixels.Length;
            PerfProfiler.ProfilerEventEnd("Decoding Image", "Loading");
            UploadTexture(size, pixels, flipped, format);
        }

        protected virtual void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat pixelFormat)
        {
            Texture ??= Texture.NonGLThreadInitialize(size);

            Texture.FlipY = flipped;
            GLThread.ExecuteGLThreadAsync(() =>
            {
                PerfProfiler.ProfilerEventStart($"Uploading Image {Name}", "Loading");
                Texture.NonGLThreadInitializedCreatePointer(Texture);
                Texture.Upload(size, pixels, pixelFormat, pixelFormat == PixelFormat.Red ? InternalFormat.Red : null);
                PerfProfiler.ProfilerEventEnd($"Uploading Image {Name}", "Loading");

                if (_pixelsMemoryIsRented)
                    ArrayPool<byte>.Shared.Return(pixels);

#if DEBUG
                Texture.CreationStack = new string(' ', 3) + Name + new string(' ', 100) + "\n" + Texture.CreationStack;
#endif
            });
        }

        protected override void DisposeInternal()
        {
            Texture.Dispose();
            Texture = null;
        }

        public void Reload(ReadOnlyMemory<byte> data)
        {
            CreateInternal(data);
        }
    }
}