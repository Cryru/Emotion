#region Using

using System.Linq;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Platform.Debugger;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
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

            public Texture Texture;
        }

        public uint AtlasPointer
        {
            get => _fbo.ColorAttachment.Pointer;
        }

        private class TextureAtlasMetaData
        {
            public int Activity;

            public Vector2 Offset;
            public Vector2 SizeBatched;

            public bool Batched = false;

            public int DrawnVersion = -1;
            public int BatchedVersion = -1;
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
        protected TextureMapping _lastTextureMapping;
        protected static ObjectPool<TextureMapping> _textureMappingPool = new ObjectPool<TextureMapping>();

        private static int _repackRate = 60 * 60; // How often to repack the atlas. (In frames) If two repacks pass without a texture being used, it will be ejected.
        private static int _usagesToPack = 20; // How many texture usages are needed to add the texture to the pack. (In texture usages over _repackRate frames)

        protected static Vector2 _maxTextureBatchSize;
        protected static Vector2 _atlasTextureSize;

        protected int _maxTexturesToDrawToAtlasPerFrame = 2;

        static TextureAtlas()
        {
            _atlasTextureSize = new Vector2(Gl.CurrentLimits.MaxTextureSize / 4f);
            _maxTextureBatchSize = _atlasTextureSize / 2f;
            Engine.Log.Trace($"Texture atlas textures will be of size {_atlasTextureSize}", MessageSource.Renderer);
        }

        public TextureAtlas(bool smooth = false) : base(_atlasTextureSize)
        {
            Size = _atlasTextureSize;

            _vbo = new VertexBuffer((uint)(VertexData.SizeInBytes * 4), BufferUsage.StaticDraw);
            _vboLocal = new VertexData[4];
            var ibo = new IndexBuffer(12 * sizeof(ushort));
            var quadIndices = new ushort[12];
            IndexBuffer.FillQuadIndices(quadIndices, 0);
            ibo.Upload(quadIndices);
            _vao = new VertexArrayObject<VertexData>(_vbo, ibo);

            _fbo = new FrameBuffer(_atlasTextureSize).WithColor();
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

            // Don't batch "no" texture. Those UVs are used for effects.
            if (texture == Texture.NoTexture || texture == Texture.EmptyWhiteTexture) return false;

            // Don't batch tiled or smoothed textures (unless the atlas is smooth).
            if (texture.Tile || texture.Smooth != _fbo.Texture.Smooth) return false;

            // Don't cache if not default shader, because getTextureSize will not work correctly.
            if (Engine.Renderer.CurrentState.Shader != ShaderFactory.DefaultProgram && !Engine.Renderer.CurrentState.Shader.AllowTextureBatch) return false;

            // If the texture is in the batch, return it as such only if it was drawn to the internal texture (which means it's usable).
            TextureAtlasMetaData? meta;
            if (_textureToMeta.TryGetValue(texture, out meta) && meta.Batched)
            {
                bool isLatestVersion = meta.DrawnVersion == texture.Version;
                if (isLatestVersion) return true;

                bool latestVersionBatched = meta.BatchedVersion == texture.Version;
                if (latestVersionBatched) return false;

                Engine.Log.Trace($"Texture {texture.Pointer} was updated!", MessageSource.Renderer);

                // Texture needs an update.
                if (meta.SizeBatched.X < texture.Size.X || meta.SizeBatched.Y < texture.Size.Y) // Texture needs to be repacked.
                {
                    Engine.Log.Trace($"Texture {texture.Pointer} had its size changed!", MessageSource.Renderer);

                    // Try to reuse this space
                    PackingSpaces.Add(new Packing.PackingSpace(new Rectangle(meta.Offset, meta.SizeBatched + _texturesMarginVec2)));

                    Vector2? newOffset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
                    if (newOffset == null)
                    {
                        meta.Batched = false;
                        return false;
                    }
                    else
                    {
                        meta.Offset = newOffset.Value;
                        meta.SizeBatched = texture.Size;
                        meta.BatchedVersion = texture.Version;
                        _haveDirtyTextures = true;
                        return false;
                    }
                }

                // Texture just needs to be redrawn.
                meta.BatchedVersion = texture.Version;
                _haveDirtyTextures = true;
                return false;
            }

            // The texture is eligible for this atlas!
            // Add meta so we can track usages, if it is used enough it will be packed.
            if (meta == null)
            {
                meta = new TextureAtlasMetaData();
                _textureToMeta.Add(texture, meta);
            }

            meta.Activity++;
            if (meta.Activity < _usagesToPack) return false;

            // Check if there is space in the internal texture.
            Vector2? offset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
            if (offset == null) return false;
            meta.Offset = offset.Value;
            meta.SizeBatched = texture.Size;
            meta.BatchedVersion = texture.Version;
            meta.Batched = true;
            _haveDirtyTextures = true;

            return false;
        }

        /// <summary>
        /// Records a specified struct range as using the specified texture.
        /// This texture is expected to have been batched.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="toStruct"></param>
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
        /// Remaps UVs of a generic struct to the texture UVs within the atlas.
        /// </summary>
        /// <param name="dataPointer">A pointer to the struct data.</param>
        /// <param name="lengthBytes">The length of the pointer in bytes.</param>
        /// <param name="structByteSize">The size of one struct in bytes.</param>
        /// <param name="uvOffsetIntoStruct">The byte offset within the struct to the Vec2(at least) member which holds the UVs.</param>
        public unsafe void RemapBatchUVs(IntPtr dataPointer, uint lengthBytes, uint structByteSize, int uvOffsetIntoStruct)
        {
            PerfProfiler.FrameEventStart("Remapping UVs to Atlas");
            Assert(_atlasTextureRange.Count > 0);
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

        /// <summary>
        /// Update the atlas, making sure all textures within are drawn to the internal texture, and
        /// that unused textures are vacated.
        /// </summary>
        /// <param name="c"></param>
        public void Update(RenderComposer c)
        {
            UpdateTextureUsage();
            DrawTextureAtlas(c);
        }

        /// <summary>
        /// Reset the texture-struct mapping tracking.
        /// </summary>
        public void ResetMapping()
        {
            _atlasTextureRange.Clear();
            _lastTextureMapping = null;
        }

        #region Update Methods

        private List<Texture> _deleteFromDictionary = new();

        private List<Texture> _packedTextures = new();

        protected virtual int TextureSortHeight(Texture x, Texture y)
        {
            return MathF.Sign(x.Size.Y - y.Size.Y);
        }

        protected void UpdateTextureUsage()
        {
            _textureActivityFrames++;
            if (_textureActivityFrames <= _repackRate) return;
            _textureActivityFrames = 0;

            List<Texture> deleteFromDictionary = _deleteFromDictionary;
            deleteFromDictionary.Clear();

            var repack = false;
            foreach ((Texture texture, TextureAtlasMetaData meta) in _textureToMeta)
            {
                bool deletedTexture = texture.Pointer == 0;
                bool existsInThePack = meta.Batched;
                int timesUsed = meta.Activity;

                // Wasn't used since the last repack tick, delete from the pack only if
                // the version currently in the pack is the latest one. Otherwise
                // it means the texture needs to be updated, and the reason it hasn't been
                // used is because the newer version is used.
                if (existsInThePack && timesUsed == 0)
                    deletedTexture = texture.Version == meta.DrawnVersion;

                // If the texture wasn't used in the last X frames, or it was disposed,
                // eject it from the atlas.
                if (deletedTexture)
                {
                    repack = true;
                    deleteFromDictionary.Add(texture);
                    continue;
                }

                meta.Activity = 0;
            }

            // Cleanup activity. Cannot do that in the foreach as the enumerator will complain.
            for (var i = 0; i < deleteFromDictionary.Count; i++)
            {
                Texture texture = deleteFromDictionary[i];
                Engine.Log.Trace($"Texture {texture.Pointer} dropped from atlas.", MessageSource.Renderer);
                _textureToMeta.Remove(texture);
            }

            if (!repack) return;

            Engine.Log.Trace($"Texture Atlas repack triggered!", MessageSource.Renderer);

            CanvasPos = Vector2.Zero;
            PackingSpaces.Clear();

            _packedTextures.Clear();
            foreach ((Texture texture, TextureAtlasMetaData meta) in _textureToMeta)
            {
                if (!meta.Batched) continue;
                _packedTextures.Add(texture);
            }
            _packedTextures.Sort(TextureSortHeight);

            for (int i = 0; i < _packedTextures.Count; i++)
            {
                Texture texture = _packedTextures[i];
                TextureAtlasMetaData meta = _textureToMeta[texture];

                Vector2? offset = Packing.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
                if (offset.HasValue)
                {
                    meta.Offset = offset.Value;
                    _haveDirtyTextures = true;
                }
                else
                {
                    Engine.Log.Trace($"Texture {texture.Pointer} previously fit in the atlas, but no longer does.", MessageSource.Renderer);
                    meta.Batched = false;
                    meta.Activity = 0;
                }
                meta.DrawnVersion = -1; // Force redraw
            }
        }

        protected void DrawTextureAtlas(RenderComposer c)
        {
            if (!_haveDirtyTextures) return;

            int drawnThisFrame = 0;
            bool hasMoreToDraw = false;

            // Draw all textures that need to be drawn to the atlas.
            c.SetState(c.BlitState);
            c.RenderTo(_fbo);
            if (_firstDraw) Gl.Clear(ClearBufferMask.ColorBufferBit);
            VertexArrayObject.EnsureBound(_vao);

            Engine.Renderer.SyncShaderIfDirty();

            Span<VertexData> vboLocalSpan = _vboLocal;
            foreach ((Texture textureKey, TextureAtlasMetaData meta) in _textureToMeta)
            {
                Texture texture = textureKey;
                if (meta.DrawnVersion == texture.Version) continue;
                if (!meta.Batched) continue;    
                if (texture.Pointer == 0) continue;

                if (drawnThisFrame >= _maxTexturesToDrawToAtlasPerFrame)
                {
                    hasMoreToDraw = true;
                    break;
                }
                drawnThisFrame++;

                //Engine.Log.Info($"Drawing texture {texture.Pointer} to atlas", "");

                Vector2 offset = meta.Offset;
                VirtualTextureAtlasTexture? virtualTexture = texture as VirtualTextureAtlasTexture;

                if (virtualTexture != null)
                {
                    virtualTexture.StartVirtualTextureRender(c, texture.Size);
                    virtualTexture.VirtualTextureRenderToBatch(c);
                    texture = virtualTexture.EndVirtualTextureRender(c);

                    // Restore state
                    c.SetState(c.BlitStatePremult);
                    VertexArrayObject.EnsureBound(_vao);

                    VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset + _texturesMarginVec, 0), textureKey.Size, Color.White, texture);

                    _vbo.Upload(_vboLocal);
                    Texture.EnsureBound(texture.Pointer);
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
                    vboLocalSpan[0].UV += _texturesMarginVec * (pixelInUv * new Vector2(-1, -1));
                    vboLocalSpan[1].UV += _texturesMarginVec * (pixelInUv * new Vector2(1, -1));
                    vboLocalSpan[2].UV += _texturesMarginVec * (pixelInUv * new Vector2(1, 1));
                    vboLocalSpan[3].UV += _texturesMarginVec * (pixelInUv * new Vector2(-1, 1));

                    _vbo.Upload(_vboLocal);

                    Texture.EnsureBound(texture.Pointer);
                    Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
                }

                meta.DrawnVersion = textureKey.Version;
            }

            c.RenderTo(null);
            _haveDirtyTextures = false;
            _firstDraw = false;

            // We hit the draw limit
            if (hasMoreToDraw) _haveDirtyTextures = true;
        }

#endregion

        #region Helpers

        protected Rectangle GetTextureUVMinMax(Texture texture)
        {
            _textureToMeta.TryGetValue(texture, out TextureAtlasMetaData? meta);
            AssertNotNull(meta);
            Assert(meta.DrawnVersion == texture.Version);

            meta.Activity++;
            Vector2 atlasOffset = meta.Offset + _texturesMarginVec;
            var minMaxUV = new Rectangle(atlasOffset, atlasOffset + texture.Size);
            return new Rectangle(minMaxUV.Position / Size, minMaxUV.Size / Size);
        }

        #endregion
    }
}