#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Graphics.Objects;
using Emotion.Primitives;
using Emotion.Utility;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    public unsafe class RenderStreamBatch<T> where T : new()
    {
        public ref struct StreamData
        {
            public Span<T> VerticesData;
            public Span<ushort> IndicesData;
            public ushort StructIndex;
        }

        protected Type _structType;
        protected uint _structByteSize;
        protected uint _structCapacity;
        protected int _bufferCount;
        protected uint _indexByteSize;

        /// <summary>
        /// The current batch mode of the stream. This determines the primitives drawing mode.
        /// </summary>
        public BatchMode BatchMode { get; protected set; } = BatchMode.Quad;

        /// <summary>
        /// Whether anything is mapped to the stream.
        /// </summary>
        public bool AnythingMapped
        {
            get => _dataPointer != IntPtr.Zero;
        }

        #region Memory

        protected FencedBufferSource _memory;
        protected FencedBufferSource _memoryIndices;

        #endregion

        // The pointers are mapped to the buffers started from the offset at which the mapping started, indicated by the offset variables.

        #region Mapping

        protected uint _mapOffsetStart;
        protected IntPtr _dataPointer;
        protected uint _indexMapOffsetStart;
        protected IntPtr _indexPointer;
        protected ushort _vertexIndex;

        #endregion

        #region MultiDraw

        protected int[][] _batchableLengths = new int[2][];
        protected int _batchableLengthUtilization;

        #endregion

        #region Texturing

        protected static bool _textureAtlas = true;
        protected static Vector2 _maxTextureBatchSize = new Vector2(1000);
        protected static Vector2 _atlasTextureSize = new Vector2(2048); // Proxy texture check for this size and disable atlasing in some cases or something.
        protected uint _currentTexture;
        protected TextureAtlasBinningState _atlasState;
        protected Queue<TextureMapping> _atlasTextureRange = new Queue<TextureMapping>();
        protected ObjectPool<TextureMapping> _textureMappingPool = new ObjectPool<TextureMapping>();
        protected TextureMapping _lastTextureMapping;

        #endregion

        public RenderStreamBatch(uint sizeStructs = 0, int bufferCount = 3)
        {
            _structType = typeof(T);
            _structByteSize = (uint) Marshal.SizeOf<T>();
            _indexByteSize = sizeof(ushort);

            _structCapacity = sizeStructs == 0 ? ushort.MaxValue : sizeStructs;
            _bufferCount = bufferCount;

            Debug.Assert(_structCapacity <= ushort.MaxValue);
            uint bufferSizeBytes = _structByteSize * _structCapacity;

            _memory = new FencedBufferSource(bufferSizeBytes, _bufferCount, s =>
            {
                var vbo = new VertexBuffer(s, BufferUsage.StreamDraw);
                var vao = new VertexArrayObject<T>(vbo);
                return new FencedBufferObjects(vbo, vao);
            });
            _memoryIndices = new FencedBufferSource(_indexByteSize * _structCapacity, _bufferCount, s =>
            {
                var ibo = new IndexBuffer(s, BufferUsage.StreamDraw);
                return new FencedBufferObjects(ibo, null);
            });

            if (!_textureAtlas) return;
            _atlasState = new TextureAtlasBinningState(_atlasTextureSize);
            _currentTexture = _atlasState.AtlasPointer;
        }

        public StreamData GetStreamMemory(uint structCount, uint indexCount, BatchMode batchMode, Texture texture = null)
        {
            if (batchMode != BatchMode && AnythingMapped) FlushRender();
            BatchMode = batchMode;

            texture ??= Texture.EmptyWhiteTexture;
            uint texturePointer = texture.Pointer;

            // Texture atlas logic
            bool batchableTexture = _textureAtlas;
            {
                // ReSharper disable once ReplaceWithSingleAssignment.True

                // Don't store frame buffer textures.
                if (texture is FrameBufferTexture) batchableTexture = false;

                // Texture too large to atlas.
                if (texture.Size.X > _maxTextureBatchSize.X || texture.Size.Y > _maxTextureBatchSize.Y) batchableTexture = false;

                // Don't batch "no" texture. Those UVs are used for effects.
                if (texture == Texture.NoTexture) batchableTexture = false;

                // Don't batch tiled or smoothed textures.
                if (texture.Tile) batchableTexture = false;

                // Check if the texture can be stored in the atlas.
                batchableTexture = batchableTexture && _atlasState.StoreTexture(texture);

                // If batching set the current texture to the atlas.
                if (batchableTexture) texturePointer = _atlasState.AtlasPointer;
            }

            // If the texture is changing, flush old data.
            if (texturePointer != _currentTexture)
            {
                if (AnythingMapped) FlushRender();
                _currentTexture = texturePointer;
            }

            uint vBytesNeeded = structCount * _structByteSize;
            uint iBytesNeeded = indexCount * _indexByteSize;

            // This request can never be serviced, as it itself is larger that an empty buffer.
            if (vBytesNeeded > _memory.Size || iBytesNeeded > _memoryIndices.Size) return default;

            // Check if the request can be served, if not - flush the buffers.
            bool gotStructs = _memory.CurrentBufferSize >= vBytesNeeded;
            bool gotIndices = _memoryIndices.CurrentBufferSize >= iBytesNeeded;
            if (!gotStructs || !gotIndices)
            {
                if (AnythingMapped) FlushRender();

                if (!gotStructs)
                {
                    Debug.Assert(!_memory.CurrentBuffer.DataBuffer.Mapping);
                    _memory.SwapBuffer();
                    _vertexIndex = 0;
                }

                if (!gotIndices)
                {
                    Debug.Assert(!_memoryIndices.CurrentBuffer.DataBuffer.Mapping);
                    _memoryIndices.SwapBuffer();
                }

                // Should have enough size now.
                Debug.Assert(_memory.CurrentBufferSize >= vBytesNeeded);
                Debug.Assert(_memoryIndices.CurrentBufferSize >= iBytesNeeded);
            }

            // This will start mapping. It needs to finish with a flush in all cases.
            EnsureMemoryMapped();
            Debug.Assert(_indexPointer != IntPtr.Zero);
            Debug.Assert(_dataPointer != IntPtr.Zero);

            // Mark memory area as used.
            uint vOffset = _memory.CurrentBufferOffset - _mapOffsetStart;
            uint iOffset = _memoryIndices.CurrentBufferOffset - _indexMapOffsetStart;
            _memory.SetUsed(vBytesNeeded);
            _memoryIndices.SetUsed(iBytesNeeded);

            // ReSharper disable PossibleNullReferenceException
            var verticesData = new Span<T>(&((byte*) _dataPointer)[vOffset], (int) structCount);
            var indicesData = new Span<ushort>(&((byte*) _indexPointer)[iOffset], (int) indexCount);
            // ReSharper enable PossibleNullReferenceException

            ushort index = _vertexIndex;
            _vertexIndex = (ushort) (_vertexIndex + structCount);

            // Some batch modes cannot be used with draw elements.
            // For them record where the memory request started and its length.
            // 1 request = 1 mesh
            if (BatchMode == BatchMode.TriangleFan)
            {
                // Not enough size.
                if (_batchableLengthUtilization + 1 >= (_batchableLengths[0]?.Length ?? 0))
                    for (var i = 0; i < _batchableLengths.Length; i++)
                    {
                        _batchableLengths[i] ??= new int[1];
                        Array.Resize(ref _batchableLengths[i], _batchableLengths[i].Length * 2);
                    }

                _batchableLengths[0][_batchableLengthUtilization] = index;
                _batchableLengths[1][_batchableLengthUtilization] = (int) indexCount;
                _batchableLengthUtilization++;
            }

            // If using the texture atlas, record the mapping.
            // This needs to happen after any potential flushes.
            if (batchableTexture)
            {
                var structsMappedInCurrentMapping = (int) (vOffset / _structByteSize);
                var upToStructNow = (int) (structsMappedInCurrentMapping + structCount);

                // Increase the up to struct of the last mapping instead of adding a new one, if possible.
                if (_lastTextureMapping != null && _lastTextureMapping.Texture == texture)
                {
                    _lastTextureMapping.UpToStruct = upToStructNow;
                }
                else
                {
                    TextureMapping textureMapping = _textureMappingPool.Get();
                    textureMapping.UpToStruct = upToStructNow;
                    textureMapping.Texture = texture;

                    _atlasTextureRange.Enqueue(textureMapping);
                    _lastTextureMapping = textureMapping;
                }
            }

            return new StreamData
            {
                VerticesData = verticesData,
                IndicesData = indicesData,
                StructIndex = index
            };
        }

        public void FlushRender()
        {
            Debug.Assert(AnythingMapped);
            Debug.Assert(_dataPointer != IntPtr.Zero);
            Debug.Assert(_indexPointer != IntPtr.Zero);

            uint mappedBytes = _memory.CurrentBufferOffset - _mapOffsetStart;
            uint mappedBytesIndices = _memoryIndices.CurrentBufferOffset - _indexMapOffsetStart;

            PerfProfiler.FrameEventStart($"Stream Render {mappedBytes / _structByteSize} Vertices with {mappedBytesIndices / _indexByteSize} Indices");

            // Remap UVs to be within the atlas, if using the atlas.
            if (_currentTexture == _atlasState?.AtlasPointer)
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

                Debug.Assert(totalStructs == mappedBytes / _structByteSize);

#endif

                int uvOffsetIntoStruct = _memory.CurrentBuffer.VAO.UVByteOffset;
                var dataPtr = (byte*) _dataPointer;
                var reader = 0;
                var structIdx = 0;
                TextureMapping textureMapping = _atlasTextureRange.Dequeue();
                Rectangle textureMinMax = _atlasState.GetTextureUVMinMax(textureMapping.Texture);

                Debug.Assert(textureMapping.UpToStruct <= mappedBytes / _structByteSize);

                while (reader < mappedBytes)
                {
                    var targetPtr = (Vector2*) (dataPtr + reader + uvOffsetIntoStruct);

                    if (structIdx >= textureMapping.UpToStruct)
                    {
                        Debug.Assert(_atlasTextureRange.Count > 0);
                        _textureMappingPool.Return(textureMapping);
                        textureMapping = _atlasTextureRange.Dequeue();
                        textureMinMax = _atlasState.GetTextureUVMinMax(textureMapping.Texture);
                    }

                    targetPtr->X = Maths.Lerp(textureMinMax.X, textureMinMax.Width, targetPtr->X);
                    targetPtr->Y = 1.0f - Maths.Lerp(textureMinMax.Y, textureMinMax.Height, targetPtr->Y); // Since the atlas is flipped, we need to flip the Y UV.

                    reader += (int) _structByteSize;
                    structIdx++;
                }

                _textureMappingPool.Return(textureMapping);
                PerfProfiler.FrameEventEnd("Remapping UVs to Atlas");
            }

            // Range is relative to the mapped range, not the whole buffer.
            _memory.CurrentBuffer.DataBuffer.FinishMappingRange(0, mappedBytes);
            _memory.CurrentBuffer.DataBuffer.FinishMapping();
            _memoryIndices.CurrentBuffer.DataBuffer.FinishMappingRange(0, mappedBytesIndices);
            _memoryIndices.CurrentBuffer.DataBuffer.FinishMapping();

            // Bind GL state.
            Texture.EnsureBound(_currentTexture);
            VertexArrayObject.EnsureBound(_memory.CurrentBuffer.VAO);
            IndexBuffer.EnsureBound(_memoryIndices.CurrentBuffer.DataBuffer.Pointer);

            // Draw with the appropriate state.
            PrimitiveType primitiveType = BatchMode switch
            {
                BatchMode.Quad => PrimitiveType.Triangles,
                BatchMode.SequentialTriangles => PrimitiveType.Triangles,
                BatchMode.TriangleFan => PrimitiveType.TriangleFan,
                _ => PrimitiveType.Triangles
            };
            if (BatchMode == BatchMode.TriangleFan)
            {
                if (Gl.CurrentVersion.GLES)
                    for (var i = 0; i < _batchableLengthUtilization; i++)
                    {
                        Gl.DrawArrays(primitiveType, _batchableLengths[0][i], _batchableLengths[1][i]);
                    }
                else
                    Gl.MultiDrawArrays(primitiveType, _batchableLengths[0], _batchableLengths[1], _batchableLengthUtilization);

                _batchableLengthUtilization = 0;
            }
            else
            {
                var startIndexInt = (IntPtr) _indexMapOffsetStart;
                var count = (int) (mappedBytesIndices / _indexByteSize);
                Gl.DrawElements(primitiveType, count, DrawElementsType.UnsignedShort, startIndexInt);
            }

            PerfProfiler.FrameEventEnd($"Stream Render {mappedBytes / _structByteSize} Vertices with {mappedBytesIndices / _indexByteSize} Indices");

            // Reset mapping.
            _dataPointer = IntPtr.Zero;
            _indexPointer = IntPtr.Zero;
            _mapOffsetStart = 0;
            _indexMapOffsetStart = 0;
            _atlasTextureRange.Clear();
            _lastTextureMapping = null;
        }

        public void DoTasks(RenderComposer c)
        {
            if (_atlasState == null) return;
            _atlasState.UpdateTextureUsage();
            _atlasState.UpdateTextureAtlas(c);
        }

        #region Helpers and Overloads

        /// <summary>
        /// Ensures the memory pointers are mapped and lazily initializes memory.
        /// </summary>
        protected void EnsureMemoryMapped()
        {
            if (_dataPointer == IntPtr.Zero)
            {
                _mapOffsetStart = _memory.CurrentBufferOffset;
                _dataPointer = StartMappingFencedBuffer(_memory);
            }

            if (_indexPointer == IntPtr.Zero)
            {
                _indexMapOffsetStart = _memoryIndices.CurrentBufferOffset;
                _indexPointer = StartMappingFencedBuffer(_memoryIndices);
            }
        }

        protected IntPtr StartMappingFencedBuffer(FencedBufferSource fencedBuffer)
        {
            return (IntPtr) fencedBuffer.CurrentBuffer.DataBuffer.CreateUnsafeMapper((int) fencedBuffer.CurrentBufferOffset, fencedBuffer.CurrentBufferSize,
                BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit
            );
        }

        /// <summary>
        /// Get stream memory and automatically map indices depending on the batch mode.
        /// </summary>
        public Span<T> GetStreamMemory(uint structCount, BatchMode batchMode, Texture texture = null)
        {
            uint indexCount = 0;
            switch (batchMode)
            {
                case BatchMode.Quad:
                    indexCount = structCount / 4 * 6;
                    break;
                case BatchMode.TriangleFan:
                case BatchMode.SequentialTriangles:
                    indexCount = structCount;
                    break;
            }

            StreamData streamData = GetStreamMemory(structCount, indexCount, batchMode, texture);
            Span<ushort> indicesSpan = streamData.IndicesData;
            ushort offset = streamData.StructIndex;
            switch (batchMode)
            {
                case BatchMode.Quad:
                    IndexBuffer.FillQuadIndices(indicesSpan, offset);
                    break;
                case BatchMode.TriangleFan:
                case BatchMode.SequentialTriangles:
                    for (ushort i = 0; i < indicesSpan.Length; i++)
                    {
                        indicesSpan[i] = (ushort) (offset + i);
                    }

                    break;
            }

            return streamData.VerticesData;
        }

        #endregion
    }
}