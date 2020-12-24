#region Using

using System;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// A buffer holding vertex indices.
    /// </summary>
    public sealed class IndexBuffer : DataBuffer
    {
        /// <summary>
        /// The bound index buffer.
        /// </summary>
        public new static uint Bound
        {
            get => DataBuffer.Bound[BufferTarget.ElementArrayBuffer];
            set => DataBuffer.Bound[BufferTarget.ElementArrayBuffer] = value;
        }

        public IndexBuffer(uint byteSize = 0, BufferUsage usage = BufferUsage.StaticDraw) : base(BufferTarget.ElementArrayBuffer, byteSize, usage)
        {
        }

        /// <summary>
        /// Ensures the provided pointer is the currently bound index buffer.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        public static void EnsureBound(uint pointer)
        {
            EnsureBound(pointer, BufferTarget.ElementArrayBuffer);
        }

        public static void FillQuadIndices(Span<ushort> indices, int offset)
        {
            for (var i = 0; i < indices.Length; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }
        }
    }
}