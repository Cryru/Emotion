#nullable enable

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Threading;
using OpenGL;
using System.Buffers;

namespace Emotion.Graphics.Assets;

public class TextureCubemapAsset : TextureAssetBase<TextureCubemap>
{
    private OtherAsset?[] _sides = new OtherAsset[5];

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        string name = Name;

        _sides[0] = LoadAssetDependency<OtherAsset>(name.Replace("_0", "_1"), false);
        _sides[1] = LoadAssetDependency<OtherAsset>(name.Replace("_0", "_2"), false);
        _sides[2] = LoadAssetDependency<OtherAsset>(name.Replace("_0", "_3"), false);
        _sides[3] = LoadAssetDependency<OtherAsset>(name.Replace("_0", "_4"), false);
        _sides[4] = LoadAssetDependency<OtherAsset>(name.Replace("_0", "_5"), false);

        DecodeImage(data, out byte[] pixels, out Vector2 size, out bool flipped, out PixelFormat format, out bool rentedMemory);
        if (size.X != size.Y)
        {
            Engine.Log.Warning($"Cubemap image must be a square!", "CubeMapTextureAsset");
            yield break;
        }

        Texture = TextureCubemap.NonGLThreadInitialize(size.X);
        Texture.SetFormat(format, InternalFormat.Rgba, PixelType.UnsignedByte);

        // todo: not sure why these rotations are needed, is something wrong with out cube?
        ImageUtil.RotateSquareInPlace(pixels, Gl.PixelFormatToComponentCount(format) * 1, (int)size.X, 3);
        GLThread.ExecuteOnGLThreadAsync(UploadToCubeMap, Texture, (CubeFace.PositiveX, pixels, rentedMemory));
        
        yield return WaitAllDependenciesToLoad();

        for (int i = 0; i < _sides.Length; i++)
        {
            OtherAsset? side = _sides[i];
            if (side == null) continue;

            DecodeImage(side.Content, out byte[] sidePixels, out Vector2 sideSize, out bool sideFlipped, out PixelFormat sideFormat, out bool sideRentedMemory);
            Assert(sideSize == size);
            Assert(sideFormat == format);

            if (i == 0)
                ImageUtil.RotateSquareInPlace(sidePixels, Gl.PixelFormatToComponentCount(format) * 1, (int)size.X, 1);
            else if (i == 1)
                ImageUtil.RotateSquareInPlace(sidePixels, Gl.PixelFormatToComponentCount(format) * 1, (int)size.X, 2);
            else if (i == 3 || i == 4)
                ImageUtil.RotateSquareInPlace(sidePixels, Gl.PixelFormatToComponentCount(format) * 1, (int)size.X, 3);

            GLThread.ExecuteOnGLThreadAsync(UploadToCubeMap, Texture, (CubeFace.NegativeX + i, sidePixels, sideRentedMemory));
        }

        yield break;
    }

    public static void UploadToCubeMap(TextureCubemap texture, (CubeFace side, byte[] pixels, bool rented) parameters)
    {
        if (texture.Pointer == 0)
            TextureCubemap.NonGLThreadInitializedCreatePointer(texture);

        texture.Upload(parameters.side, parameters.pixels);

        if (parameters.rented)
            ArrayPool<byte>.Shared.Return(parameters.pixels);
    }
}
