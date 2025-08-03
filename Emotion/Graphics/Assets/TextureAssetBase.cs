#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Standard.Parsers.Image.BMP;
using Emotion.Standard.Parsers.Image.ImgBin;
using Emotion.Standard.Parsers.Image.PNG;
using OpenGL;

#if MORE_IMAGE_TYPES
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

#endif

namespace Emotion.Graphics.Assets;

public abstract class TextureAssetBase<TTexture> : Asset where TTexture : IDisposable
{
    /// <summary>
    /// The asset's uploaded graphics texture.
    /// </summary>
    public TTexture? Texture { get; set; }

    protected TextureAssetBase()
    {
        _useNewLoading = true;
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        // nop - new loading
        Coroutine.RunInline(Internal_LoadAssetRoutine(data));
    }

    protected abstract override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data);

    protected override void DisposeInternal()
    {
        Texture?.Dispose();
        Texture = default;
    }

    // flipped - Whether the image was uploaded flipped - top to bottom.
    protected bool DecodeImage(
        ReadOnlyMemory<byte> data,
        out byte[] pixels,
        out Vector2 size,
        out bool flipped,
        out PixelFormat format,
        out bool rentedMemory
    )
    {
        pixels = Array.Empty<byte>();
        size = Vector2.Zero;
        flipped = false;
        format = PixelFormat.Unknown;
        rentedMemory = false;

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
            rentedMemory = true;
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

        return pixels != null || size.X != 0 && size.Y != 0 && format != PixelFormat.Unknown;
    }
}
