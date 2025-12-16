#nullable enable

#region Using

using Emotion.Core.Utility;
using Emotion.Core.Utility.Profiling;
using Emotion.Graphics.Data;
using Emotion.Graphics.Shading;
using OpenGL;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics.Batches;

public static class TextureAtlasHelper
{
    private static Dictionary<PixelFormat, Vector2> _pixelFormatToSize = [];

    public static Vector2 GetMaxAtlasTextureSize(PixelFormat format)
    {
        if (_pixelFormatToSize.TryGetValue(format, out Vector2 atlasSize))
            return atlasSize;

        int componentCount = Gl.PixelFormatToComponentCount(format);
        atlasSize = new Vector2(Gl.CurrentLimits.MaxTextureSize / componentCount);
        _pixelFormatToSize.Add(format, atlasSize);
        return atlasSize;
    }
}

public class TextureAtlasSimple<T> : Packing.PackingResumableState
    where T : notnull
{
    public struct TextureAtlasItem
    {
        public int Staleness;
        public Rectangle UV;
    }

    public Texture Texture => _atlasTexture;
    public Vector2 Spacing = new Vector2(2);

    private readonly Texture _atlasTexture;
    private Dictionary<T, int> _items = new();
    private List<TextureAtlasItem> _itemBacking = new();

    public TextureAtlasSimple(PixelFormat pixelFormat, InternalFormat internalFormat, PixelType pixelType)
        : base(TextureAtlasHelper.GetMaxAtlasTextureSize(pixelFormat))
    {
        _atlasTexture = new Texture(Size, pixelFormat, false, internalFormat, pixelType);
    }

    public bool CheckHas(T hash, out Rectangle uv, bool markActivity = false)
    {
        if (_items.TryGetValue(hash, out int itemIdx))
        {
            TextureAtlasItem item = _itemBacking[itemIdx];
            item.Staleness = 0;
            _itemBacking[itemIdx] = item;

            uv = item.UV;
            return true;
        }

        uv = Rectangle.Empty;
        return false;
    }

    public Rectangle? TryAdd(T hash, IntVector2 size, Span<byte> data)
    {
        Vector2? offset = Packing.FitRectanglesResumable(size.ToVec2() + Spacing * 2, this);
        if (offset == null) return null;

        Vector2 renderOffset = offset.Value + Spacing;

        var uv = new Primitives.Rectangle(renderOffset, size.ToVec2());
        var newItem = new TextureAtlasItem()
        {
            Staleness = 0,
            UV = uv
        };
        _items.TryAdd(hash, _itemBacking.Count);
        _itemBacking.Add(newItem);

        _atlasTexture.UploadPartial(renderOffset.ToIVec2Floor(), size, data);
        return uv;
    }

    public bool Update(T hash, IntVector2 size, Span<byte> data)
    {
        if (!_items.TryGetValue(hash, out int itemIdx)) return false;

        TextureAtlasItem item = _itemBacking[itemIdx];

        Rectangle uv = item.UV;
        IntVector2 renderOffset = uv.Position.ToIVec2Floor();
        IntVector2 uvSize = uv.Size.ToIVec2Floor();
        if (uvSize != size) return false;

        _atlasTexture.UploadPartial(renderOffset, size, data);

        return true;
    }

    public (int staleItems, int totalItems, float usage) TickAllItems(int activityThreshold)
    {
        int itemsThatHaventBeenUsedRecently = 0;

        Span<TextureAtlasItem> blocksSpan = CollectionsMarshal.AsSpan(_itemBacking);
        for (int i = 0; i < blocksSpan.Length; i++)
        {
            ref TextureAtlasItem item = ref blocksSpan[i];
            if (item.Staleness > activityThreshold)
                itemsThatHaventBeenUsedRecently++;

            item.Staleness++;
        }

        return (itemsThatHaventBeenUsedRecently, blocksSpan.Length, CanvasPos.Y / Size.Y);
    }

    public void Clear()
    {
        _items.Clear();
        _itemBacking.Clear();

        CanvasPos = Vector2.Zero;
        PackingSpaces.Clear();
    }
}

/// <summary>
/// A texture which contains other textures. Used by the render stream batch to
/// batch draw calls using different textures together.
/// This class is also responsible for remapping the UVs and keeping track of texture-struct(vertex) mapping.
/// </summary>
public class TextureAtlas : Packing.PackingResumableState
{
    public class TextureMapping
    {
        public int UpToStruct;

        public Texture Texture = Texture.EmptyWhiteTexture;
    }

    public uint AtlasPointer
    {
        get => _fbo.ColorAttachment.Pointer;
    }

    public enum AtlasTextureMetaState
    {
        None,
        TrackingActivity,
        NeedsUpdate, // Set for a moment only by the try-batch
        NeedsToBeDrawn, // Set by the try-batch
        CanBeUsed // Set by the render update
    }

    private class TextureAtlasMetaData
    {
        public AtlasTextureMetaState State;
        public int Activity;

        public Vector2 BatchOffset; // Offset in the packing
        public Vector2 BatchSize; // Size in the packing (doesn't include margins)

        public int DrawnVersion = -1;
        public Vector2 DrawnSize; // Size rendered in the texture
    }

    private Dictionary<Texture, TextureAtlasMetaData> _textureToMeta = new();

    private readonly FrameBuffer _fbo;
    private readonly VertexBuffer _vbo;
    private readonly VertexArrayObject<VertexData> _vao;
    private readonly VertexData[] _vboLocal;

    protected bool _haveDirtyTextures;
    protected bool _firstDraw; // Used to track fbo clearing.
    protected int _textureActivityFrames;

    protected int _texturesMargin;
    protected int _texturesMargin2;
    protected Vector2 _texturesMarginVec;
    protected Vector2 _texturesMarginVec2;

    protected Queue<TextureMapping> _atlasTextureRange = new Queue<TextureMapping>();
    protected TextureMapping? _lastTextureMapping;
    protected static ObjectPool<TextureMapping> _textureMappingPool = new ObjectPool<TextureMapping>();

    private static int _repackRate = 60 * 60; // How often to repack the atlas. (In frames) If two repacks pass without a texture being used, it will be ejected.
    private static int _activityBeforePack = 20; // How many texture usages are needed to add the texture to the pack. (In texture usages between _repackRate ticks)

    protected Vector2 _maxTextureBatchSize;
    //protected static Vector2 _atlasTextureSize;

    protected int _maxTexturesToDrawToAtlasPerFrame = 2;

    public TextureAtlas(bool smooth = false) : base(TextureAtlasHelper.GetMaxAtlasTextureSize(PixelFormat.Rgba))
    {
        _maxTextureBatchSize = Size / 2f;

        _vbo = new VertexBuffer((uint)(VertexData.SizeInBytes * 4), BufferUsage.StaticDraw);
        _vboLocal = new VertexData[4];
        var ibo = new IndexBuffer(12 * sizeof(ushort));
        var quadIndices = new ushort[12];
        IndexBuffer.FillQuadIndices(quadIndices, 0);
        ibo.Upload(quadIndices);
        _vao = new VertexArrayObject<VertexData>(_vbo, ibo);

        _fbo = new FrameBuffer(Size).WithColor();
        _fbo.CheckErrors();
        _firstDraw = true;

        if (smooth)
        {
            _fbo.Texture.Smooth = true;
            _texturesMargin = 2;
            _texturesMargin2 = _texturesMargin * 2;
            _texturesMarginVec = new Vector2(_texturesMargin);
            _texturesMarginVec2 = new Vector2(_texturesMargin * 2);
        }
    }

    /// <summary>
    /// Tries to batch the provided texture within the atlas.
    /// Returns true if the texture is batched.
    /// </summary>
    public bool TryBatchTexture(Texture texture)
    {
        // Don't store frame buffer textures.
        if (texture is FrameBufferTexture) return false;

        // Texture is invalid?
        if (texture.Size.X == 0 || texture.Size.Y == 0) return false;

        // Texture too large to atlas.
        if (texture.Size.X + _texturesMargin2 > _maxTextureBatchSize.X || texture.Size.Y + _texturesMargin2 > _maxTextureBatchSize.Y) return false;

        // Don't batch "no" texture, that might cause problems.
        if (texture == Texture.NoTexture) return false;

        // Don't batch tiled or smoothed textures (unless the atlas is smooth).
        if (texture.Tile || texture.Smooth != _fbo.Texture.Smooth) return false;

        // Don't cache if not default shader, because getTextureSize will not work correctly.
        if (Engine.Renderer.CurrentShader != ShaderFactory.DefaultProgram && !Engine.Renderer.CurrentShader.AllowTextureBatch) return false;

        // Check if we have a record of this texture since it seems eligible.
        // If not create meta data for it.
        if (!_textureToMeta.TryGetValue(texture, out TextureAtlasMetaData? meta))
        {
            if (meta == null)
            {
                meta = new TextureAtlasMetaData();
                _textureToMeta.Add(texture, meta);
            }
        }

        // Does the texture think it's usable?
        if (meta.State == AtlasTextureMetaState.CanBeUsed)
        {
            // Verify this assertion.
            bool isLatestVersion = meta.DrawnVersion == texture.Version;
            bool isCorrectSize = meta.DrawnSize == texture.Size;
            if (isLatestVersion && isCorrectSize) return true;

            // Otherwise it needs to be updated.
            meta.State = AtlasTextureMetaState.NeedsUpdate;
        }

        // The texture's size/version has changed.
        if (meta.State == AtlasTextureMetaState.NeedsUpdate)
        {
            const bool drawStableTexturesOnly = true;
            if (drawStableTexturesOnly)
            {
                // Reset activity as we only want to batch textures that don't change too much.
                PackingSpaces.Add(new Packing.PackingSpace(new Rectangle(meta.BatchOffset, meta.BatchSize + _texturesMarginVec2)));
                meta.State = AtlasTextureMetaState.TrackingActivity;
                meta.Activity = 0;
            }
            else
            {
                // We just need to redraw without rebatching as the size just shrank,
                // however it can retain it's current size.
                if (meta.BatchSize.X > texture.Size.X && meta.BatchSize.Y > texture.Size.Y)
                {
                    // nop, we just gotta redraw
                }
                // The texture needs more space.
                else
                {
                    Vector2? offset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
                    if (offset == null) return false;

                    // Return the space previously occupied to be reused
                    PackingSpaces.Add(new Packing.PackingSpace(new Rectangle(meta.BatchOffset, meta.BatchSize + _texturesMarginVec2)));
                    meta.BatchOffset = offset.Value;
                    meta.BatchSize = texture.Size;
                }

                meta.State = AtlasTextureMetaState.NeedsToBeDrawn;
                _haveDirtyTextures = true;
            }
        }

        // Wait for texture to be drawn
        if (meta.State == AtlasTextureMetaState.NeedsToBeDrawn)
        {
            return false;
        }

        // Start tracking activity of the texture
        if (meta.State == AtlasTextureMetaState.None)
        {
            meta.State = AtlasTextureMetaState.TrackingActivity;
        }

        // Track how many times the texture has been used to decide whether to pack it.
        if (meta.State == AtlasTextureMetaState.TrackingActivity)
        {
            meta.Activity++;
            if (meta.Activity < _activityBeforePack) return false;

            // If the texture has been used enough, try to pack it.
            Vector2? offset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
            if (offset == null) return false;
            meta.BatchOffset = offset.Value;
            meta.BatchSize = texture.Size;
            meta.State = AtlasTextureMetaState.NeedsToBeDrawn;
            _haveDirtyTextures = true;
            return false;
        }

        return false;
    }

    /// <summary>
    /// Update the atlas, making sure all textures within are drawn to the internal texture, and
    /// that unused textures are vacated. For the atlases used by the RenderStream this happens at the end of each frame.
    /// </summary>
    public void Update(Renderer c)
    {
        UpdateTextureUsage();
        DrawTextureAtlas(c);
    }

    #region Update Methods

    // Allocation savers
    private List<Texture> _allocationPreventReusableList = new(32);

    protected virtual int TextureSortHeight(Texture x, Texture y)
    {
        return MathF.Sign(x.Size.Y - y.Size.Y);
    }

    protected void UpdateTextureUsage()
    {
        _textureActivityFrames++;
        if (_textureActivityFrames <= _repackRate) return;
        _textureActivityFrames = 0;

        List<Texture> deleteFromDictionary = _allocationPreventReusableList;
        deleteFromDictionary.Clear();

        var repack = false;
        foreach ((Texture texture, TextureAtlasMetaData meta) in _textureToMeta)
        {
            int timesUsed = meta.Activity;
            meta.Activity = 0; // Reset activity tracking.

            bool deletedTexture = texture.Pointer == 0;

            if (meta.State == AtlasTextureMetaState.CanBeUsed && timesUsed == 0)
                deletedTexture = true;

            // If the texture wasn't used in the last <_repackRate> frames, or it was disposed,
            // eject it from the atlas.
            if (deletedTexture)
            {
                repack = true;
                deleteFromDictionary.Add(texture);
            }
        }

        // Cleanup activity. Cannot do that in the foreach as the enumerator will complain.
        for (var i = 0; i < deleteFromDictionary.Count; i++)
        {
            Texture texture = deleteFromDictionary[i];
            //Engine.Log.Trace($"Texture {texture.Pointer} dropped from atlas.", MessageSource.Renderer);
            _textureToMeta.Remove(texture);
        }

        if (!repack) return;

        Engine.Log.Trace($"Texture Atlas repack triggered!", MessageSource.Renderer);

        CanvasPos = Vector2.Zero;
        PackingSpaces.Clear();

        // We sort the textures by height for more optimal packing.
        // If we're gonna repack everything - might as well.
        // todo: repacking can be done on another thread hmm...
        List<Texture> repackTextures = _allocationPreventReusableList;
        repackTextures.Clear();
        foreach ((Texture texture, TextureAtlasMetaData meta) in _textureToMeta)
        {
            if (meta.State == AtlasTextureMetaState.CanBeUsed || meta.State == AtlasTextureMetaState.NeedsToBeDrawn)
                repackTextures.Add(texture);
        }
        repackTextures.Sort(TextureSortHeight);

        for (int i = 0; i < repackTextures.Count; i++)
        {
            Texture texture = repackTextures[i];
            TextureAtlasMetaData meta = _textureToMeta[texture];

            Vector2? offset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
            if (offset.HasValue)
            {
                meta.BatchOffset = offset.Value;
                meta.BatchSize = texture.Size;
                meta.State = AtlasTextureMetaState.NeedsToBeDrawn;
                _haveDirtyTextures = true;
            }
            else
            {
                // Couldn't find space for the texture - this could technically happen.
                // In this case just boot it out.
                Engine.Log.Trace($"Texture {texture.Pointer} previously fit in the atlas, but no longer does.", MessageSource.Renderer);
                meta.State = AtlasTextureMetaState.None;
                meta.Activity = 0;
            }
        }
    }

    protected void DrawTextureAtlas(Renderer c)
    {
        if (!_haveDirtyTextures) return;

        int drawnThisFrame = 0;
        bool hasMoreToDraw = false;

        // Draw all textures that need to be drawn to the atlas.
        c.SetState(c.BlitState);
        c.RenderTo(_fbo);
        if (_firstDraw) Gl.Clear(ClearBufferMask.ColorBufferBit);
        VertexArrayObject.EnsureBound(_vao);

        Span<VertexData> vboLocalSpan = _vboLocal;
        foreach ((Texture textureKey, TextureAtlasMetaData meta) in _textureToMeta)
        {
            Texture texture = textureKey;
            if (meta.State != AtlasTextureMetaState.NeedsToBeDrawn) continue;

            // Don't draw deleted textures!
            if (texture.Pointer == 0) continue;

            // Recheck if size has changed, we don't want to draw over other batched textures :/
            // If a texture is changing every frame (after the batch) this would be hit every time,
            // and that is kind of a nightmare scenario as it will never batch.
            if (texture.Size.X > meta.BatchSize.X || texture.Size.Y > meta.BatchSize.Y)
            {
                meta.State = AtlasTextureMetaState.NeedsUpdate;
                continue;
            }

            // Don't draw too many in one frame (is this needed?)
            if (drawnThisFrame >= _maxTexturesToDrawToAtlasPerFrame)
            {
                hasMoreToDraw = true;
                break;
            }
            drawnThisFrame++;

            //Engine.Log.Info($"Drawing texture {texture.Pointer} to atlas", "");

            Vector2 offset = meta.BatchOffset;
            var virtualTexture = texture as VirtualTextureAtlasTexture;

            if (virtualTexture != null)
            {
                virtualTexture.StartVirtualTextureRender(c, texture.Size + _texturesMarginVec2);
                virtualTexture.VirtualTextureRenderToBatch(c, _texturesMarginVec);
                var backingTexture = virtualTexture.EndVirtualTextureRender(c);

                // Restore state
                c.SetState(c.BlitStatePremult);
                VertexArrayObject.EnsureBound(_vao); // VirtualTextureRenderToBatch has changed it for sure :D

                VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset, 0), texture.Size + _texturesMarginVec2, Color.White, backingTexture, new Rectangle(0, 0, texture.Size + _texturesMarginVec2));

                _vbo.Upload(_vboLocal);
                Texture.EnsureBound(backingTexture.Pointer);
                Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);

                // Restore state, once again
                c.SetState(c.BlitState);
            }
            else if (_texturesMarginVec == Vector2.Zero)
            {
                VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset, 0), texture.Size, Color.White, texture);
                _vbo.Upload(_vboLocal);

                Texture.EnsureBound(texture.Pointer);
                Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }
            else
            {
                Vector2 pixelInUv = Vector2.One / texture.Size;
                VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset, 0),
                    texture.Size + _texturesMarginVec2, Color.White, texture);
                vboLocalSpan[0].UV += _texturesMarginVec * (pixelInUv * new Vector2(-1, 1));
                vboLocalSpan[1].UV += _texturesMarginVec * (pixelInUv * new Vector2(1, 1));
                vboLocalSpan[2].UV += _texturesMarginVec * (pixelInUv * new Vector2(1, -1));
                vboLocalSpan[3].UV += _texturesMarginVec * (pixelInUv * new Vector2(-1, -1));

                _vbo.Upload(_vboLocal);

                Texture.EnsureBound(texture.Pointer);
                Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
            }

            meta.DrawnVersion = textureKey.Version;
            meta.DrawnSize = textureKey.Size;
            meta.State = AtlasTextureMetaState.CanBeUsed;
        }

        c.RenderTo(null);
        _haveDirtyTextures = false;
        _firstDraw = false;

        // We hit the draw limit
        if (hasMoreToDraw) _haveDirtyTextures = true;
    }

    #endregion

    #region UV Remapping

    /// <summary>
    /// Records a specified struct range in the vertex buffer as using the specified texture.
    /// This texture is expected to have been batched.
    /// </summary>
    public void RecordTextureMapping(Texture texture, int toStruct)
    {
        // Increase the up to struct of the last mapping instead of adding a new one, if possible.
        if (_lastTextureMapping != null && _lastTextureMapping.Texture == texture)
        {
            _lastTextureMapping.UpToStruct = toStruct;
        }
        else
        {
            TextureMapping textureMapping = _textureMappingPool.Get();
            textureMapping.UpToStruct = toStruct;
            textureMapping.Texture = texture;

            _atlasTextureRange.Enqueue(textureMapping);
            _lastTextureMapping = textureMapping;
        }
    }

    /// <summary>
    /// Reset the texture usage mapping tracking.
    /// </summary>
    public void ResetMapping()
    {
        _atlasTextureRange.Clear();
        _lastTextureMapping = null;
    }

    /// <summary>
    /// Remaps UVs of a generic struct to the texture UVs within the atlas.
    /// </summary>
    /// <param name="dataPointer">A pointer to the vertex buffer struct data.</param>
    /// <param name="lengthBytes">The length of the vertex data in bytes.</param>
    /// <param name="structByteSize">The size of one struct in bytes.</param>
    /// <param name="uvOffsetIntoStruct">The byte offset within the struct to the Vec2(at least) member which holds the UVs.</param>
    public unsafe void RemapBatchUVs(IntPtr dataPointer, uint lengthBytes, uint structByteSize, int uvOffsetIntoStruct)
    {
        if (_atlasTextureRange.Count == 0) return; // No batched textures this draw call
        PerfProfiler.FrameEventStart("Remapping UVs to Atlas");

#if DEBUG
        var totalStructs = 0;
        int count = _atlasTextureRange.Count;
        var currentIdx = 0;
        foreach (TextureMapping mappingRange in _atlasTextureRange)
        {
            currentIdx++;
            if (currentIdx == count) totalStructs = mappingRange.UpToStruct;
        }

        Assert(totalStructs == lengthBytes / structByteSize);
#endif

        var dataPtr = (byte*)dataPointer;
        var reader = 0;
        var structIdx = 0;
        TextureMapping textureMapping = _atlasTextureRange.Dequeue();
        Rectangle textureMinMax = GetTextureUVMinMax(textureMapping.Texture);

        Assert(textureMapping.UpToStruct <= lengthBytes / structByteSize);

        while (reader < lengthBytes)
        {
            var targetPtr = (Vector2*)(dataPtr + reader + uvOffsetIntoStruct);

            if (structIdx >= textureMapping.UpToStruct)
            {
                Assert(_atlasTextureRange.Count > 0);
                _textureMappingPool.Return(textureMapping);
                textureMapping = _atlasTextureRange.Dequeue();
                textureMinMax = GetTextureUVMinMax(textureMapping.Texture);
            }

            targetPtr->X = Maths.Lerp(textureMinMax.X, textureMinMax.Width, targetPtr->X);
            targetPtr->Y = 1.0f - Maths.Lerp(textureMinMax.Y, textureMinMax.Height, targetPtr->Y); // Since the atlas is flipped, we need to flip the Y UV.

            reader += (int)structByteSize;
            structIdx++;
        }

        _textureMappingPool.Return(textureMapping);
        PerfProfiler.FrameEventEnd("Remapping UVs to Atlas");
    }

    protected Rectangle GetTextureUVMinMax(Texture texture)
    {
        _textureToMeta.TryGetValue(texture, out TextureAtlasMetaData? meta);
        AssertNotNull(meta);
        Assert(meta.DrawnVersion == texture.Version);

        meta.Activity++;
        Vector2 atlasOffset = meta.BatchOffset + _texturesMarginVec;
        var minMaxUV = new Rectangle(atlasOffset, atlasOffset + texture.Size);
        return new Rectangle(minMaxUV.Position / Size, minMaxUV.Size / Size);
    }

    #endregion
}