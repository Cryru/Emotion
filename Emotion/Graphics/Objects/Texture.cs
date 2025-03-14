﻿#region Using

using Emotion.Common.Serialization;
using Emotion.Common.Threading;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// An uploaded texture.
    /// </summary>
    [DontSerialize]
    public class Texture : IDisposable
    {
        #region DEBUG

        public static List<Texture> AllTextures = new List<Texture>();

        public string CreationStack;

        #endregion

        /// <summary>
        /// The texture's version is incremented every time new data is uploaded to it.
        /// It's used to keep track of changes.
        /// </summary>
        public int Version = 0;

        /// <summary>
        /// The bound textures.
        /// </summary>
        public static uint[] Bound = new uint[Engine.Renderer.TextureArrayLimit];
        private static int _activeSlot = -1;

        /// <summary>
        /// The OpenGL pointer to this Texture.
        /// </summary>
        public uint Pointer { get; protected set; }

        /// <summary>
        /// The size of the texture in pixels.
        /// </summary>
        public virtual Vector2 Size { get; protected set; }

        /// <summary>
        /// Whether the image was uploaded upside down. Upside down in this case means not-upside down
        /// due to the way images are stored.
        /// </summary>
        public bool FlipY = false;

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

                GLThread.ExecuteGLThreadAsync(() =>
                {
                    int smoothOption = _hasMipmap ? Gl.LINEAR_MIPMAP_LINEAR : Gl.LINEAR;

                    if (Engine.Renderer.Dsa)
                    {
                        Gl.TextureParameter(Pointer, TextureParameterName.TextureMinFilter, _smooth ? smoothOption : Gl.NEAREST);
                        Gl.TextureParameter(Pointer, TextureParameterName.TextureMagFilter, _smooth ? Gl.LINEAR : Gl.NEAREST);
                    }
                    else
                    {
                        EnsureBound(Pointer);
                        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, _smooth ? smoothOption : Gl.NEAREST);
                        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, _smooth ? Gl.LINEAR : Gl.NEAREST);
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
                        EnsureBound(Pointer);
                        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                        Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapT, _tile ? Gl.REPEAT : Gl.CLAMP_TO_EDGE);
                    }
                });
            }
        }

        /// <summary>
        /// The format in which the texture is stored internally - this is the format shader should expect to work with.
        /// </summary>
        public InternalFormat InternalFormat { get; protected set; }

        /// <summary>
        /// The source format in which the texture's pixels were uploaded.
        /// </summary>
        public PixelFormat PixelFormat { get; protected set; }

        /// <summary>
        /// The source pixel type of the texture's pixels.
        /// </summary>
        public PixelType PixelType { get; protected set; } = PixelType.UnsignedByte;

        private bool _tile;
        protected bool _smooth;
        private bool _smoothSet;

        public bool Mipmap
        {
            get => _hasMipmap;
            set
            {
                if (!value) return;

                _hasMipmap = true;
                _smoothSet = false;
                Smooth = _smooth;

                GLThread.ExecuteGLThreadAsync(() =>
                {
                    if (Engine.Renderer.Dsa)
                    {
                        Gl.GenerateTextureMipmap(Pointer);
                    }
                    else
                    {
                        EnsureBound(Pointer);
                        Gl.GenerateMipmap(TextureTarget.Texture2d);
                    }
                });
            }
        }

        private bool _hasMipmap;

        /// <summary>
        /// Create a new uninitialized texture.
        /// </summary>
        public Texture() : this(Vector2.Zero)
        {
            Pointer = Gl.GenTexture();
        }

        protected Texture(uint pointer)
        {
            Pointer = pointer;
        }

        protected Texture(Vector2 size)
        {
            Size = size;

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
        /// <param name="size">The size of the texture.</param>
        /// <param name="pixelFormat">The pixel format of the texture.</param>
        /// <param name="smooth">Whether to apply linear interpolation to the surface's texture.</param>
        /// <param name="internalFormat">The internal format of the texture.</param>
        /// <param name="pixelType">The data type of individual pixel components.</param>
        public Texture(Vector2 size, PixelFormat pixelFormat, bool? smooth = null, InternalFormat internalFormat = InternalFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte) : this()
        {
            _smooth = smooth ?? Engine.Configuration.TextureDefaultSmooth;
            Upload(size, null, pixelFormat, internalFormat, pixelType);
        }

        /// <summary>
        /// Create a texture from data.
        /// </summary>
        /// <param name="size">The size of the texture.</param>
        /// <param name="data">The data to upload.</param>
        /// <param name="pixelFormat">The pixel format of the texture.</param>
        /// <param name="smooth">Whether to apply linear interpolation to the surface's texture.</param>
        /// <param name="internalFormat">The internal format of the texture.</param>
        public Texture(Vector2 size, byte[] data, PixelFormat pixelFormat, bool? smooth = false, InternalFormat internalFormat = InternalFormat.Rgba) : this()
        {
            _smooth = smooth ?? Engine.Configuration.TextureDefaultSmooth;
            Upload(size, data, pixelFormat, internalFormat);
        }

        /// <summary>
        /// Uploads data to the texture. If no data is specified the texture is just resized.
        /// This will reset the texture matrix.
        /// </summary>
        /// <param name="size">The width and height of the texture data.</param>
        /// <param name="data">The data to upload.</param>
        /// <param name="pixelFormat">The pixel format of the texture. If null the format which was last used is taken.</param>
        /// <param name="internalFormat">The internal format of the texture. If null the format which was last used is taken.</param>
        /// <param name="pixelType">The data type of individual pixel components.</param>
        public virtual void Upload(Vector2 size, byte[] data, PixelFormat? pixelFormat = null, InternalFormat? internalFormat = null, PixelType? pixelType = null)
        {
            Size = size;

            pixelFormat ??= PixelFormat;
            PixelFormat = (PixelFormat)pixelFormat;

            internalFormat ??= InternalFormat;
            InternalFormat = (InternalFormat)internalFormat;

            pixelType ??= PixelType;
            PixelType = (PixelType)pixelType;

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

            EnsureBound(Pointer);
            if (data == null)
                Gl.TexImage2D(TextureTarget.Texture2d, 0, (InternalFormat)internalFormat, (int)Size.X, (int)Size.Y, 0, (PixelFormat)pixelFormat,
                    (PixelType)pixelType, IntPtr.Zero);
            else
                Gl.TexImage2D(TextureTarget.Texture2d, 0, (InternalFormat)internalFormat, (int)Size.X, (int)Size.Y, 0, (PixelFormat)pixelFormat,
                    (PixelType)pixelType, data);

            if (_hasMipmap)
                Gl.GenerateMipmap(TextureTarget.Texture2d);

            _smoothSet = false;
            Smooth = _smooth;
            Tile = _tile;
            Version++;
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

        /// <summary>
        /// Ensures the provided pointer is the currently bound texture in the provided slot.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        /// <param name="slot">Ensure the texture is bound in this slot.</param>
        public static void EnsureBound(uint pointer, uint slot = 0)
        {
            // Check if it is already bound.
            if (Bound[slot] == pointer && pointer != 0)
            {
                // If in debug mode, verify this with OpenGL.
                if (!Engine.Configuration.GlDebugMode) return;

                if (slot != _activeSlot)
                {
                    _activeSlot = (int) slot;
                    Gl.ActiveTexture(TextureUnit.Texture0 + _activeSlot);
                }

                Gl.Get(GetPName.TextureBinding2d, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed texture bound to slot {slot} was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            if (slot != _activeSlot)
            {
                _activeSlot = (int)slot;
                Gl.ActiveTexture(TextureUnit.Texture0 + _activeSlot);
            }
            Gl.BindTexture(TextureTarget.Texture2d, pointer);
            Bound[slot] = pointer;
        }

        /// <summary>
        /// Destroy the texture freeing up memory.
        /// The graphics object's destruction is async.
        /// </summary>
        public virtual void Dispose()
        {
            if (Pointer == 0) return;

#if DEBUG
            AllTextures.Remove(this);
#endif

            uint ptr = Pointer;
            Pointer = 0;

            // Unbind texture from all binding tracker slots.
            for (var i = 0; i < Bound.Length; i++)
            {
                if (Bound[i] == ptr) Bound[i] = 0;
            }

            GLThread.ExecuteGLThreadAsync(() => { Gl.DeleteTextures(ptr); });
        }

        // Systemic procedurally created textures.

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
}