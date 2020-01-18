#region Using

using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// A buffer holding vertex data.
    /// </summary>
    public sealed class VertexBuffer : DataBuffer
    {
        /// <summary>
        /// The bound index buffer.
        /// </summary>
        public new static uint Bound
        {
            get => DataBuffer.Bound[BufferTarget.ArrayBuffer];
            set => DataBuffer.Bound[BufferTarget.ArrayBuffer] = value;
        }

        public VertexBuffer(uint byteSize = 0, BufferUsage usage = BufferUsage.DynamicDraw) : base(BufferTarget.ArrayBuffer, byteSize, usage)
        {
        }

        /// <summary>
        /// Ensures the provided pointer is the currently bound vertex buffer.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        public static void EnsureBound(uint pointer)
        {
            EnsureBound(pointer, BufferTarget.ArrayBuffer);
        }
    }
}