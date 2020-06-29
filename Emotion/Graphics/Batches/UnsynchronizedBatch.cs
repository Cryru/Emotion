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
        public UnsynchronizedBatch(uint size = 0, int bufferCount = 3) : base(size)
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
        public override void SetBatchMode(BatchMode mode)
        {
            base.SetBatchMode(mode);
            if (_memory != null)
            {
                CacheBufferSizes();
            }
        }

        /// <summary>
        /// Cache the index and vertex sizes of the current buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CacheBufferSizes()
        {
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

            // Render using the internal method.
            Render(_memory.CurrentBuffer.VAO, startIndex, length);

            // Mark memory as used after the draw. You can't draw between buffer boundaries.
            _memory.SetUsed(mappedBytes, _indicesUsed);
            CacheBufferSizes();
            if (_indices >= RenderComposer.MINIMUM_INDICES) return;
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