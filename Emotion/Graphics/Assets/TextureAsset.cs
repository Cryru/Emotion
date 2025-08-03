#nullable enable

#region Using

using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Profiling;
using Emotion.Core.Utility.Threading;
using OpenGL;
using System.Buffers;

#endregion

namespace Emotion.Graphics.Assets;

/// <summary>
/// Represents an image. Supports all formats supported by Emotion.Standard.Image
/// </summary>
public class TextureAsset : TextureAssetBase<Texture>
{
    static TextureAsset()
    {
        RegisterFileExtensionSupport<TextureAsset>([ "png", "bmp", "imgbin" ]);
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        if (!DecodeImage(data, out byte[] pixels, out Vector2 size, out bool flipped, out PixelFormat format, out bool rentedMemory))
        {
            Engine.Log.Warning($"Couldn't load texture - {Name}.", MessageSource.AssetLoader);
            yield break;
        }

        ByteSize = pixels.Length;
        UploadTexture(size, pixels, flipped, format, rentedMemory);
    }

    protected virtual void UploadTexture(Vector2 size, byte[] pixels, bool flipped, PixelFormat pixelFormat, bool rentedMemory)
    {
        Texture ??= Texture.NonGLThreadInitialize(size);

        Texture.FlipY = flipped;
        GLThread.ExecuteGLThreadAsync(() =>
        {
            PerfProfiler.ProfilerEventStart($"Uploading Image {Name}", "Loading");
            Texture.NonGLThreadInitializedCreatePointer(Texture);
            Texture.Upload(size, pixels, pixelFormat, pixelFormat == PixelFormat.Red ? InternalFormat.Red : null);
            PerfProfiler.ProfilerEventEnd($"Uploading Image {Name}", "Loading");

            if (rentedMemory)
                ArrayPool<byte>.Shared.Return(pixels);

#if DEBUG
            Texture.CreationStack = new string(' ', 3) + Name + new string(' ', 100) + "\n" + Texture.CreationStack;
#endif
        });
    }
}