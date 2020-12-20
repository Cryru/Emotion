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
    }
}