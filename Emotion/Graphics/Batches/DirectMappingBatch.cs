#region Using

using System;
using System.Diagnostics;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// A batch which directly maps to a GPU buffer for caching render data.
    /// </summary>
    /// <typeparam name="T">The batch's struct type.</typeparam>
    public class DirectMappingBatch<T> : RenderBatch<T>
    {
        protected VertexBuffer _memory;
        protected VertexArrayObject<T> _vao;

        /// <inheritdoc />
        public DirectMappingBatch(uint size = 0) : base(false, size)
        {
        }

        /// <inheritdoc />
        protected override unsafe IntPtr GetMemoryPointer()
        {
            if (_memory == null)
            {
                _memory = new VertexBuffer(_bufferSize, BufferUsage.StaticDraw);
                _vao = new VertexArrayObject<T>(_memory);
            }

            return (IntPtr) _memory.CreateUnsafeMapper(0, _memory.Size,
                BufferAccessMask.MapWriteBit | BufferAccessMask.MapFlushExplicitBit
            );
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
            _memory.FinishMappingRange(0, mappedBytes);
            _memory.FinishMapping();

            // Render using the internal method.
            Render(_vao, startIndex, length);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Reset();
            _memory = null;
        }
    }
}