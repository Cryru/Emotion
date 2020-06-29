#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Graphics.Objects;
using OpenGL;

#endregion

namespace Emotion.Graphics.Batches
{
    /// <summary>
    /// A batch which maps to CPU memory and then copies it to GPU memory.
    /// Much slower, but allows for CPU vertex modification.
    /// </summary>
    /// <typeparam name="T">The batch's struct type.</typeparam>
    public class BackedBatch<T> : DirectMappingBatch<T>
    {
        protected IntPtr _totalMemory;

        /// <inheritdoc />
        public BackedBatch(uint size = 0) : base(size)
        {
        }

        /// <inheritdoc />
        protected override IntPtr GetMemoryPointer()
        {
            if (_memory == null)
            {
                _memory = new VertexBuffer(_bufferSize, BufferUsage.StaticDraw);
                _vao = new VertexArrayObject<T>(_memory);
                _totalMemory = Marshal.AllocHGlobal((int) _bufferSize);
            }

            return _totalMemory;
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
            _memory.Upload(_totalMemory, mappedBytes, BufferUsage.StaticDraw);

            // Render using the internal method.
            Render(_vao, startIndex, length);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Reset();
            _memory = null;
            Marshal.FreeHGlobal(_totalMemory);
        }
    }
}