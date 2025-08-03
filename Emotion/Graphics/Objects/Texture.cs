#nullable enable

#region Using

using Emotion.Core.Utility.Threading;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects;

/// <summary>
/// An OpenGL Texture2D
/// </summary>
[DontSerialize]
public class Texture : TextureObjectBase
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

    protected override TextureTarget _textureTarget => TextureTarget.Texture2d;

    /// <summary>
    /// Create a new uninitialized texture which will have a GL pointer allocated.
    /// </summary>
    public Texture() : this(Vector2.Zero)
    {
        Assert(GLThread.IsGLThread());
        Pointer = Gl.GenTexture();
    }

    protected Texture(uint pointer)
    {
        Pointer = pointer;
    }

    protected Texture(Vector2 size)
    {
#if DEBUG
        AllTextures.Add(this);
        CreationStack = Environment.StackTrace;

        int lastCtorCall = CreationStack.LastIndexOf("Emotion.Graphics.Objects.Texture.", StringComparison.Ordinal);
        int newLineAfterThat = CreationStack.IndexOf("\n", lastCtorCall, StringComparison.Ordinal);

        int fbCtorCall = CreationStack.LastIndexOf("Emotion.Graphics.Objects.FrameBuffer.", StringComparison.Ordinal);
        if (fbCtorCall != -1) newLineAfterThat = CreationStack.IndexOf("\n", fbCtorCall, StringComparison.Ordinal);

        CreationStack = CreationStack.Substring(newLineAfterThat + 1);
#endif
    }

    public static Texture NonGLThreadInitialize(Vector2 size)
    {
        var t = new Texture(size)
        {
            Pointer = EmptyWhiteTexture.Pointer,
            InternalFormat = InternalFormat.Rgba,
            PixelType = PixelType.UnsignedByte
        };
        return t;
    }

    public static void NonGLThreadInitializedCreatePointer(Texture t)
    {
        t.Pointer = Gl.GenTexture();
    }

    /// <summary>
    /// Create a new empty texture.
    /// </summary>
    public Texture(Vector2 size, PixelFormat pixelFormat, bool? smooth = null, InternalFormat internalFormat = InternalFormat.Rgba,
        PixelType pixelType = PixelType.UnsignedByte) : this()
    {
        _smooth = smooth ?? Engine.Configuration.TextureDefaultSmooth;
        Upload(size, null, pixelFormat, internalFormat, pixelType);
    }

    /// <summary>
    /// Create a texture from data.
    /// </summary>
    public Texture(Vector2 size, byte[] data, PixelFormat pixelFormat, bool? smooth = false, InternalFormat internalFormat = InternalFormat.Rgba) : this()
    {
        _smooth = smooth ?? Engine.Configuration.TextureDefaultSmooth;
        Upload(size, data, pixelFormat, internalFormat);
    }

    /// <summary>
    /// Uploads data to the texture. If no data is specified the texture is just resized.
    /// This will reset the texture matrix.
    /// </summary>
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

    /// <summary>
    /// Download the texture from the GPU.
    /// Do not use this in game code as it blocks the render thread.
    /// </summary>
    public byte[] Download()
    {
        var data = new byte[(int)(Size.X * Size.Y * Gl.PixelTypeToByteCount(PixelType) * Gl.PixelFormatToComponentCount(PixelFormat))];
        EnsureBound(Pointer);
        Gl.GetTexImage(TextureTarget.Texture2d, 0, PixelFormat, PixelType, data);
        return data;
    }

    public static void EnsureBound(uint pointer, uint slot = 0)
    {
        TextureObjectBase.EnsureBound(TextureTarget.Texture2d, pointer, slot);
    }

    // Procedurally created default textures

    public static Texture NoTexture = new(0);
    public static Texture EmptyWhiteTexture;
    public static Texture Smooth_EmptyWhiteTexture;

    public static void InitializeEmptyTexture()
    {
        EmptyWhiteTexture = new Texture(new Vector2(1, 1), new byte[] { 255, 255, 255, 255 }, PixelFormat.Rgba);
        Smooth_EmptyWhiteTexture = new Texture(new Vector2(2, 2), new byte[] {
                255, 255, 255, 255,
                255, 255, 255, 255,
                255, 255, 255, 255,
                255, 255, 255, 255
        }, PixelFormat.Rgba, true);
    }
}