#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Emotion.Common;
using Emotion.Game;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// A texture which contains other textures. Used by the render stream batch to
    /// batch draw calls using different textures together.
    /// This class is also responsible for remapping the UVs and keeping track of texture-struct mapping.
    /// </summary>
    public class TextureAtlas : Binning.BinningResumableState
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

        private Dictionary<Texture, Vector2> _textureToOffset = new Dictionary<Texture, Vector2>();
        private Dictionary<Texture, bool> _textureNeedDraw = new Dictionary<Texture, bool>();
        private Dictionary<Texture, int> _textureActivity = new Dictionary<Texture, int>();

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

        protected static Vector2 _maxTextureBatchSize = new Vector2(1024); // Maybe % based of atlas size?
        protected static Vector2 _atlasTextureSize = new Vector2(2048); // Proxy texture check for this size and disable atlasing in some cases or something.
        protected Queue<TextureMapping> _atlasTextureRange = new Queue<TextureMapping>();
        protected TextureMapping _lastTextureMapping;

        protected static ObjectPool<TextureMapping> _textureMappingPool = new ObjectPool<TextureMapping>();

        private static int _repackRate = 60 * 60; // How often to repack the atlas. (In frames) If two repacks pass without a texture being used, it will be ejected.
        private static int _usagesToPack = 20; // How many texture usages are needed to add the texture to the pack. (In texture usages)

        public TextureAtlas(bool smooth = false) : base(_atlasTextureSize)
        {
            _vbo = new VertexBuffer((uint)(VertexData.SizeInBytes * 8), BufferUsage.StaticDraw);
            _vboLocal = new VertexData[8];
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
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public bool TryBatchTexture(Texture texture)
        {
            // Don't store frame buffer textures.
            if (texture is FrameBufferTexture) return false;

            // Texture too large to atlas.
            if (texture.Size.X + _texturesMargin2 > _maxTextureBatchSize.X || texture.Size.Y + _texturesMargin2 > _maxTextureBatchSize.Y) return false;

            // Don't batch "no" texture. Those UVs are used for effects.
            if (texture == Texture.NoTexture) return false;

            // Don't batch tiled or smoothed textures (unless the atlas is smooth).
            if (texture.Tile || texture.Smooth != _fbo.Texture.Smooth) return false;

            // If the texture is in the batch, return it as such only if it was drawn to the internal texture (which means it's usable).
            if (_textureToOffset.ContainsKey(texture)) return !_textureNeedDraw[texture];

            // The texture is eligible for this atlas. Start tracking its usage, if it is used enough it will be packed.
            if (_textureActivity.ContainsKey(texture))
                _textureActivity[texture]++;
            else
                _textureActivity.Add(texture, 1);
            if (_textureActivity[texture] < _usagesToPack) return false;

            // Check if there is space in the internal texture.
            Vector2? offset = Binning.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
            if (offset == null) return false;
            _textureToOffset.Add(texture, offset.Value);
            _textureNeedDraw.Add(texture, true);
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

            Debug.Assert(_atlasTextureRange.Count > 0);
#if DEBUG

            var totalStructs = 0;
            int count = _atlasTextureRange.Count;
            var currentIdx = 0;
            foreach (TextureMapping mappingRange in _atlasTextureRange)
            {
                currentIdx++;
                if (currentIdx == count) totalStructs = mappingRange.UpToStruct;
            }

            Debug.Assert(totalStructs == lengthBytes / structByteSize);

#endif

            var dataPtr = (byte*)dataPointer;
            var reader = 0;
            var structIdx = 0;
            TextureMapping textureMapping = _atlasTextureRange.Dequeue();
            Rectangle textureMinMax = GetTextureUVMinMax(textureMapping.Texture);

            Debug.Assert(textureMapping.UpToStruct <= lengthBytes / structByteSize);

            while (reader < lengthBytes)
            {
                var targetPtr = (Vector2*)(dataPtr + reader + uvOffsetIntoStruct);

                if (structIdx >= textureMapping.UpToStruct)
                {
                    Debug.Assert(_atlasTextureRange.Count > 0);
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

        protected void UpdateTextureUsage()
        {
            _textureActivityFrames++;
            if (_textureActivityFrames <= _repackRate) return;
            _textureActivityFrames = 0;

            List<Texture> deleteFromDictionary = null;
            var repack = false;
            foreach ((Texture texture, int timesUsed) in _textureActivity)
            {
                bool deletedTexture = texture.Pointer == 0;
                bool existsInThePack = _textureNeedDraw.ContainsKey(texture);

                // Wasn't used since the last repack tick.
                if (existsInThePack && timesUsed == 0) deletedTexture = !_textureNeedDraw[texture];

                // If the texture wasn't used in the last X frames, or it was disposed,
                // eject it from the atlas.
                if (deletedTexture)
                {
                    repack = true;
                    _textureToOffset.Remove(texture);
                    _textureNeedDraw.Remove(texture);

                    deleteFromDictionary ??= new List<Texture>();
                    deleteFromDictionary.Add(texture);
                    continue;
                }

                _textureActivity[texture] = 0;
            }

            // Cleanup activity. Cannot do that in the foreach as the enumerator will complain.
            if (deleteFromDictionary != null)
                for (var i = 0; i < deleteFromDictionary.Count; i++)
                {
                    _textureActivity.Remove(deleteFromDictionary[i]);
                }

            if (!repack) return;

            CanvasPos = Vector2.Zero;
            PackingSpaces.Clear();
            IOrderedEnumerable<Texture> allTextures = _textureToOffset.Keys.ToArray().OrderBy(x => x.Size.Y);
            foreach (Texture texture in allTextures)
            {
                Vector2? offset = Binning.FitRectanglesResumable(texture.Size + _texturesMarginVec2, this);
                if (offset.HasValue)
                {
                    _textureToOffset[texture] = offset.Value;
                    _textureNeedDraw[texture] = true;
                    _haveDirtyTextures = true;
                }
                else
                {
                    Engine.Log.Trace($"Texture {texture.Pointer} previously fit in the atlas, but no longer does.", MessageSource.Renderer);
                    _textureToOffset.Remove(texture);
                    _textureNeedDraw.Remove(texture);
                    _textureActivity[texture] = 0;
                }
            }
        }

        protected void DrawTextureAtlas(RenderComposer c)
        {
            if (!_haveDirtyTextures) return;

            // Draw all textures that need to be drawn to the atlas.
            c.SetState(c.BlitState);
            c.RenderTo(_fbo);
            if (_firstDraw) Gl.Clear(ClearBufferMask.ColorBufferBit);
            VertexArrayObject.EnsureBound(_vao);

            Span<VertexData> vboLocalSpan = _vboLocal;
            foreach ((Texture texture, bool needDraw) in _textureNeedDraw)
            {
                if (!needDraw || texture.Pointer == 0) continue;

                Vector2 offset = _textureToOffset[texture];
                if (_texturesMarginVec == Vector2.Zero)
                {
                    offset += _texturesMarginVec;
                    VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset, 0), texture.Size, Color.White, texture);
                    _vbo.Upload(_vboLocal);

                    Texture.EnsureBound(texture.Pointer);
                    Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
                }
                else
                {
                    // Make sure the area in the margins is clean.
                    VertexData.SpriteToVertexData(vboLocalSpan, new Vector3(offset, 0), texture.Size + _texturesMarginVec2, Color.Transparent);

                    offset += _texturesMarginVec;
                    VertexData.SpriteToVertexData(vboLocalSpan.Slice(4), new Vector3(offset, 0), texture.Size, Color.White, texture);
                    _vbo.Upload(_vboLocal);

                    Texture.EnsureBound(Texture.EmptyWhiteTexture.Pointer);
                    Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);

                    Texture.EnsureBound(texture.Pointer);
                    Gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedShort, (IntPtr)(6 * sizeof(ushort)));
                }

                _textureNeedDraw[texture] = false;
            }

            c.RenderTo(null);
            _haveDirtyTextures = false;
            _firstDraw = false;
        }

        #endregion

        #region Helpers

        protected Rectangle GetTextureUVMinMax(Texture texture)
        {
            Debug.Assert(_textureToOffset.ContainsKey(texture));
            Debug.Assert(!_textureNeedDraw[texture]);

            _textureActivity[texture]++;
            Vector2 atlasOffset = _textureToOffset[texture] + _texturesMarginVec;
            var minMaxUV = new Rectangle(atlasOffset, atlasOffset + texture.Size);
            return new Rectangle(minMaxUV.Position / Size, minMaxUV.Size / Size);
        }

        #endregion
    }
}