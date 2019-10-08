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

        /// <summary>
        /// An index buffer suitable for drawing quads using two triangles.
        /// </summary>
        public static IndexBuffer QuadIbo;

        /// <summary>
        /// An index buffer suitable for drawing any vertices.
        /// </summary>
        public static IndexBuffer SequentialIbo;

        /// <summary>
        /// Creates the default index buffers.
        /// </summary>
        public static void CreateDefaultIndexBuffers()
        {
            if (QuadIbo != null && SequentialIbo != null) return;

            // Create default quad ibo.
            QuadIbo = new IndexBuffer(ushort.MaxValue * 6 * sizeof(ushort));
            Span<ushort> mapper = QuadIbo.CreateMapper<ushort>();

            uint offset = 0;
            for (var i = 0; i < mapper.Length; i += 6)
            {
                mapper[i] = (ushort) (offset + 0);
                mapper[i + 1] = (ushort) (offset + 1);
                mapper[i + 2] = (ushort) (offset + 2);
                mapper[i + 3] = (ushort) (offset + 2);
                mapper[i + 4] = (ushort) (offset + 3);
                mapper[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }

            QuadIbo.FinishMapping();

            // Create sequential index buffer.
            SequentialIbo = new IndexBuffer(ushort.MaxValue * sizeof(ushort));
            mapper = SequentialIbo.CreateMapper<ushort>();

            for (ushort i = 0; i < mapper.Length; i++)
            {
                mapper[i] = i;
            }

            SequentialIbo.FinishMapping();
        }
    }
}