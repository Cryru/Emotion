#region Using

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// A batch which directly maps to GPU buffers for fast unsynchronized rendering of any size.
    /// </summary>
    /// <typeparam name="T">The batch's struct type.</typeparam>
    public class UnsynchronizedBatch<T> : RenderBatch<T>
    {
        protected FencedBufferSource _memory;
        private int _bufferCount;

        /// <summary>
        /// Create a new batch.
        /// </summary>
        /// <param name="size">The size of the buffer.</param>
        /// <param name="bufferCount">The number of full sized buffers to employ in the ring buffer.</param>
        public UnsynchronizedBatch(uint size = 0, int bufferCount = 3) : base(true, size)
        {
            _bufferCount = bufferCount;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override unsafe IntPtr GetMemoryPointer()
        {
            // Initialize memory lazily.
            if (_memory == null)
            {
                _memory = new FencedBufferSource(_bufferSize, _spriteByteSize, _bufferCount, s =>
                {
                    var vbo = new VertexBuffer(s);
                    var vao = new VertexArrayObject<T>(vbo);
                    return new FencedBufferObjects(vbo, vao);
                });
                CacheBufferSizes();
            }

            return (IntPtr) _memory.CurrentBuffer.VBO.CreateUnsafeMapper((int) _memory.CurrentBufferOffset, _memory.CurrentBufferSize,
                BufferAccessMask.MapWriteBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit
            );
        }

        /// <inheritdoc />
        protected override void Resize(uint structsNeeded, uint indicesNeeded)
        {
            _memory.CurrentBuffer.VBO.FinishMapping();
            SwapInternalBuffer();
            _memoryPtr = GetMemoryPointer();
        }

        /// <inheritdoc />
        public override void SetBatchMode(BatchMode mode)
        {
            base.SetBatchMode(mode);
            if (_memory != null)
            {
                // Assert that the buffer was flushed.
                Debug.Assert(_mappedTo == 0);
                Debug.Assert(_indicesUsed == 0);
                Debug.Assert(_memoryPtr == IntPtr.Zero);

                uint indexOffset = _memory.CurrentIndexOffset;
                var structOffset = (uint) (_memory.CurrentBufferOffset / _structByteSize);
                uint verticesBytePadding = 0;
                uint indicesPadding = 0;
                if (indexOffset > _indexCapacity) // It's possible for the capacity to have changed.
                {
                    _memory.SwapBuffer();
                }
                else if (indexOffset != 0 && mode == BatchMode.Quad)
                {
                    // When switching from another mode to quads we need to make sure to align the indices.
                    // To do this we pretend that all vertices mapped until this join have been quads and
                    // continue from there.
                    uint verticesShouldBeUsed = (uint) MathF.Ceiling(structOffset / 4.0f) * 4;
                    uint indicesShouldBeUsed = verticesShouldBeUsed / 4 * 6;
                    indicesPadding = indicesShouldBeUsed - indexOffset;

                    uint verticesComp = verticesShouldBeUsed - structOffset;
                    verticesBytePadding = (uint) (verticesComp * _structByteSize);
                }
                else if (indexOffset != 0)
                {
                    // When switching from quads to another mode we need to align the vertices as the quad
                    // IBO is 6 times larger. To do this we pretend that all indices up to now were used for
                    // drawing arbitrary triangles and continue from there.
                    uint verticesShouldBeUsed = indexOffset;
                    uint verticesComp = verticesShouldBeUsed - structOffset;
                    verticesBytePadding = (uint) (verticesComp * _structByteSize);
                }

                // Check if the current buffer can take the corrected offset padding.
                if (_memory.CurrentBufferOffset + verticesBytePadding > _memory.Size || _memory.CurrentIndexOffset + indicesPadding > _indexCapacity)
                    _memory.SwapBuffer();
                else
                    _memory.SetUsed(verticesBytePadding, indicesPadding);

                CacheBufferSizes();
            }
        }

        /// <summary>
        /// Cache the index and vertex sizes of the current buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CacheBufferSizes()
        {
            Debug.Assert(_memory.CurrentIndexOffset < _indexCapacity);
            _indices = _indexCapacity - _memory.CurrentIndexOffset;
            _bufferSize = _memory.CurrentBufferSize;
        }

        /// <inheritdoc />
        public override void Render(RenderComposer composer, uint startIndex = 0, int length = -1)
        {
            // If nothing mapped - or no memory (which shouldn't happen), do nothing.
            if (_mappedTo == 0 || _memoryPtr == IntPtr.Zero)
            {
                Debug.Assert(false);
                return;
            }

            // Upload the data.
            var mappedBytes = (uint) (_mappedTo * _structByteSize);
            _memory.CurrentBuffer.VBO.FinishMappingRange(0, mappedBytes); // This range is relative to the mapped range, not the whole buffer.
            _memory.CurrentBuffer.VBO.FinishMapping();

            // Correct ranges with internal use count.
            startIndex = _memory.CurrentIndexOffset + startIndex;
            if (length == -1) length = (int) _indicesUsed;

            if (_batchableLengthUtilization > 0)
            {
                var vertOffset = (int) (_memory.CurrentBufferOffset / _structByteSize);
                for (var i = 0; i < _batchableLengthUtilization; i++)
                {
                    _batchableLengths[0][i] += vertOffset;
                }
            }

            // Render using the internal method.
            Render(_memory.CurrentBuffer.VAO, startIndex, length);

            // Mark memory as used after the draw. You can't draw between buffer boundaries.
            _memory.SetUsed(mappedBytes, _indicesUsed);
            CacheBufferSizes();
            if (_indices >= _minimumIndices) return;
            SwapInternalBuffer();
        }

        private void SwapInternalBuffer()
        {
            _memory.SwapBuffer();
            CacheBufferSizes();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Reset();
            _memory = null;
        }
    }
}