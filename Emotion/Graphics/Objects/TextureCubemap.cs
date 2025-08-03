#nullable enable

using Emotion.Core.Utility.Threading;
using OpenGL;

namespace Emotion.Graphics.Objects;

public class TextureCubemap : TextureObjectBase
{
    /// <summary>
    /// The size of the texture in pixels.
    /// </summary>
    public virtual Vector2 Size { get; protected set; }

    /// <summary>
    /// Whether the image was uploaded upside down.
    /// Note: Upside down in this case means not-upside down due to the way images are stored.
    /// </summary>
    public bool FlipY = false;

    protected override TextureTarget _textureTarget => TextureTarget.TextureCubeMap;

    public TextureCubemap() : this(0)
    {
        Assert(GLThread.IsGLThread());
        Pointer = Gl.GenTexture();
    }

    protected TextureCubemap(uint pointer)
    {
        Pointer = pointer;
    }

    protected TextureCubemap(float size)
    {
        Size = new Vector2(size);

#if DEBUG
        AllTextures.Add(this);
        CreationStack = Environment.StackTrace;

        int lastCtorCall = CreationStack.LastIndexOf("Emotion.Graphics.Objects.TextureCubemap.", StringComparison.Ordinal);
        int newLineAfterThat = CreationStack.IndexOf("\n", lastCtorCall, StringComparison.Ordinal);

        int fbCtorCall = CreationStack.LastIndexOf("Emotion.Graphics.Objects.FrameBuffer.", StringComparison.Ordinal);
        if (fbCtorCall != -1) newLineAfterThat = CreationStack.IndexOf("\n", fbCtorCall, StringComparison.Ordinal);

        CreationStack = CreationStack.Substring(newLineAfterThat + 1);
#endif
    }

    public static TextureCubemap NonGLThreadInitialize(float size)
    {
        return new TextureCubemap(size);
    }

    public static void NonGLThreadInitializedCreatePointer(TextureCubemap t)
    {
        t.Pointer = Gl.GenTexture();
    }

    public void SetFormat(PixelFormat pixelFormat, InternalFormat internalFormat, PixelType pixelType)
    {
        PixelFormat = pixelFormat;
        InternalFormat = internalFormat;
        PixelType = pixelType;
    }

    public void Upload(CubeFace side, byte[] data)
    {
        TextureTarget textureTarget = TextureTarget.Invalid;
        switch (side)
        {
            case CubeFace.PositiveX:
                textureTarget = TextureTarget.TextureCubeMapPositiveX;
                break;
            case CubeFace.NegativeX:
                textureTarget = TextureTarget.TextureCubeMapNegativeX;
                break;
            case CubeFace.PositiveY:
                textureTarget = TextureTarget.TextureCubeMapPositiveY;
                break;
            case CubeFace.NegativeY:
                textureTarget = TextureTarget.TextureCubeMapNegativeY;
                break;
            case CubeFace.PositiveZ:
                textureTarget = TextureTarget.TextureCubeMapPositiveZ;
                break;
            case CubeFace.NegativeZ:
                textureTarget = TextureTarget.TextureCubeMapNegativeZ;
                break;
        }
        if (textureTarget == TextureTarget.Invalid) return;

        var pixelFormat = PixelFormat;
        var internalFormat = InternalFormat;
        PrepareForUpload(data, ref pixelFormat, ref internalFormat);

        Gl.TexImage2D(textureTarget, 0, internalFormat, (int)Size.X, (int)Size.Y, 0, pixelFormat, PixelType, data);

        PostUpload();
    }

    public virtual void Upload(Vector2 size, byte[] data, PixelFormat? pixelFormatSet = null, InternalFormat? internalFormatSet = null, PixelType? pixelTypeSet = null)
    {
        PixelFormat pixelFormat = pixelFormatSet ?? PixelFormat;
        PixelFormat = pixelFormat;

        InternalFormat internalFormat = internalFormatSet ?? InternalFormat;
        InternalFormat = internalFormat;

        PixelType pixelType = pixelTypeSet ?? PixelType;
        PixelType = pixelType;

        Size = size;

        PrepareForUpload(data, ref pixelFormat, ref internalFormat);
        if (data == null)
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, (int)size.X, (int)size.Y, 0, pixelFormat,
                pixelType, IntPtr.Zero);
        else
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, (int)size.X, (int)size.Y, 0, pixelFormat,
                pixelType, data);

        PostUpload();
    }

    public static void EnsureBound(uint pointer, uint slot = 0)
    {
        TextureObjectBase.EnsureBound(TextureTarget.TextureCubeMap, pointer, slot);
    }
}
