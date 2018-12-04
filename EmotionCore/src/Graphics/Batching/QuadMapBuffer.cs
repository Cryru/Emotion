// Emotion - https://github.com/Cryru/Emotion

#region Using

using Emotion.Graphics.Objects;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// A map buffer optimized for drawing quads.
    /// </summary>
    public sealed class QuadMapBuffer : MapBuffer
    {
        #region Properties

        /// <summary>
        /// The ibo to be used by all QuadMapBuffers.
        /// </summary>
        private static readonly IndexBuffer IBO;

        #endregion

        static QuadMapBuffer()
        {
            // Generate indices.
            ushort[] indices = new ushort[Renderer.MaxRenderable * 6];
            uint offset = 0;
            for (int i = 0; i < indices.Length; i += 6)
            {
                indices[i] = (ushort) (offset + 0);
                indices[i + 1] = (ushort) (offset + 1);
                indices[i + 2] = (ushort) (offset + 2);
                indices[i + 3] = (ushort) (offset + 2);
                indices[i + 4] = (ushort) (offset + 3);
                indices[i + 5] = (ushort) (offset + 0);

                offset += 4;
            }

            IBO = new IndexBuffer(indices);

            GLThread.CheckError("map buffer - creating ibo");
        }

        /// <inheritdoc />
        public QuadMapBuffer(int size) : base(size, 4, IBO, 6, PrimitiveType.Triangles)
        {
        }
    }
}