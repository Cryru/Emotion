#nullable enable

using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Threading;
using OpenGL;

namespace Emotion.Graphics.Objects;

[DontSerialize]
public abstract class TextureObjectBase : IDisposable
{
    #region DEBUG

    public static List<TextureObjectBase> AllTextures = new();

    public string CreationStack = string.Empty;

    #endregion

    /// <summary>
    /// The texture's version is incremented every time new data is uploaded to it.
    /// It's used to keep track of changes.
    /// </summary>
    public int Version = 0;

    /// <summary>
    /// The OpenGL pointer to this Texture.
    /// </summary>
    public uint Pointer { get; protected set; }

    /// <summary>
    /// Whether to apply linear interpolation to the texture. Off by default.
    /// </summary>
    public bool Smooth
    {
        get => _smooth;
        set
        {
            if (value == _smooth && _smoothSet) return;
            _smooth = value;
            _smoothSet = true;

            // Texture was deleted or not uploaded yet, if it will be uploaded the smooth value set will be applied then.
            if (Pointer == 0) return;

            GLThread.ExecuteGLThreadAsync(() =>
            {
                int smoothOption = HasMipmaps ? Gl.LINEAR_MIPMAP_LINEAR : Gl.LINEAR;

                if (Engine.Renderer.Dsa)
                {
                    Gl.TextureParameter(Pointer, TextureParameterName.TextureMinFilter, _smooth ? smoothOption : Gl.NEAREST);
                    Gl.TextureParameter(Pointer, TextureParameterName.TextureMagFilter, _smooth ? Gl.LINEAR : Gl.NEAREST);
                }
                else
                {
                    EnsureBound(_textureTarget, Pointer);
                    Gl.TexParameter(_textureTarget, TextureParameterName.TextureMinFilter, _smooth ? smoothOption : Gl.NEAREST);
                    Gl.TexParameter(_textureTarget, TextureParameterName.TextureMagFilter, _smooth ? Gl.LINEAR : Gl.NEAREST);
                }
            });
        }
    }

    /// <summary>
    /// Whether to tile the texture if sampled outside its bounds. On by default.
    /// </summary>
    public bool Tile
    {
        get => _tile;
        set
        {
            _tile = value;

            GLThread.ExecuteGLThreadAsync(() =>
            {
                if (Engine.Renderer.Dsa)
                {
                    Gl.TextureParameter(Pointer, TextureParameterName.TextureWrapS, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                    Gl.TextureParameter(Pointer, TextureParameterName.TextureWrapT, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                }
                else
                {
                    EnsureBound(_textureTarget, Pointer);
                    Gl.TexParameter(_textureTarget, TextureParameterName.TextureWrapS, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                    Gl.TexParameter(_textureTarget, TextureParameterName.TextureWrapT, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                }
            });
        }
    }

    /// <summary>
    /// The format in which the texture is stored internally - this is the format shader should expect to work with.
    /// </summary>
    public InternalFormat InternalFormat { get; protected set; } = InternalFormat.Rgba;

    /// <summary>
    /// The source format in which the texture's pixels were uploaded.
    /// </summary>
    public PixelFormat PixelFormat { get; protected set; } = PixelFormat.Rgba;

    /// <summary>
    /// The source pixel type of the texture's pixels.
    /// </summary>
    public PixelType PixelType { get; protected set; } = PixelType.UnsignedByte;

    protected bool _tile;
    protected bool _smooth;
    protected bool _smoothSet;

    public bool HasMipmaps { get; protected set; }

    public void CreateMipMaps()
    {
        HasMipmaps = true;
        _smoothSet = false;
        Smooth = _smooth;

        if (Engine.Renderer.Dsa)
        {
            GLThread.ExecuteOnGLThreadAsync(Gl.GenerateTextureMipmap, Pointer);
        }
        else
        {
            GLThread.ExecuteOnGLThreadAsync(static (TextureTarget target, uint pointer) =>
            {
                EnsureBound(target, pointer);
                Gl.GenerateMipmap(target);
            }, _textureTarget, Pointer);
        }
    }

    protected abstract TextureTarget _textureTarget { get; }

    #region Uploading

    protected void PrepareForUpload(Span<byte> data, ref PixelFormat pixelFormat, ref InternalFormat internalFormat)
    {
        if (Gl.CurrentVersion.GLES)
        {
            // ES doesn't support BGRA so convert it to RGBA on the CPU
            switch (pixelFormat)
            {
                case PixelFormat.Bgra:
                    ImageUtil.BgraToRgba(data);
                    pixelFormat = PixelFormat.Rgba;
                    break;
                case PixelFormat.Bgr:
                    ImageUtil.BgrToRgb(data);
                    pixelFormat = PixelFormat.Rgb;
                    break;
            }

            // ES has different constants for some platforms.
            switch (internalFormat)
            {
                case InternalFormat.Red:
                    internalFormat = InternalFormat.R8;
                    break;
            }
        }

        EnsureBound(_textureTarget, Pointer);
    }

    protected void PostUpload()
    {
        if (HasMipmaps)
            Gl.GenerateMipmap(_textureTarget);

        _smoothSet = false;
        Smooth = _smooth;
        Tile = _tile;
        Version++;
    }

    #endregion

    /// <summary>
    /// The bound buffers of each type.
    /// </summary>
    public static Dictionary<TextureTarget, uint[]> Bound = new Dictionary<TextureTarget, uint[]>();
    private static int _activeSlot = -1;

    /// <summary>
    /// Ensures the provided pointer is the currently bound texture in the provided slot.
    /// </summary>
    /// <param name="pointer">The pointer to ensure is bound.</param>
    /// <param name="slot">Ensure the texture is bound in this slot.</param>
    public static void EnsureBound(TextureTarget textureTarget, uint pointer, uint slot = 0)
    {
        uint[] listForType = Bound[textureTarget];

        // Check if it is already bound.
        if (listForType[slot] == pointer && pointer != 0)
        {
            // If in debug mode, verify this with OpenGL.
            if (!Engine.Configuration.GlDebugMode) return;

            if (slot != _activeSlot)
            {
                _activeSlot = (int)slot;
                Gl.ActiveTexture(TextureUnit.Texture0 + _activeSlot);
            }

            GetPName bindingName = GetBindingNameFromTargetName(textureTarget);
            if (bindingName != GetPName.Invalid)
            {
                Gl.Get(GetPName.TextureBinding2d, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed texture bound to slot {slot} was {pointer} but it was {actualBound}.", MessageSource.GL);
            }

            return;
        }

        if (slot != _activeSlot)
        {
            _activeSlot = (int)slot;
            Gl.ActiveTexture(TextureUnit.Texture0 + _activeSlot);
        }
        Gl.BindTexture(textureTarget, pointer);
        listForType[slot] = pointer;
    }

    protected static GetPName GetBindingNameFromTargetName(TextureTarget textureTarget)
    {
        switch(textureTarget)
        {
            case TextureTarget.Texture2d:
                return GetPName.TextureBinding2d;
        }
        return GetPName.Invalid;
    }

    /// <summary>
    /// Destroy the texture - freeing up GPU memory.
    /// </summary>
    public virtual void Dispose()
    {
        if (Pointer == 0) return;

#if DEBUG
        AllTextures.Remove(this);
#endif

        uint oldPtr = Pointer;
        Pointer = 0;

        // Unbind texture from all slots its was bound in.
        uint[] listForType = Bound[_textureTarget];
        for (var i = 0; i < listForType.Length; i++)
        {
            if (listForType[i] == oldPtr) listForType[i] = 0;
        }

        GLThread.ExecuteOnGLThreadAsync(Gl.DeleteTexture, oldPtr);
    }
}
