// Emotion - https://github.com/Cryru/Emotion

#region Using

using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// A map buffer optimized for drawing quads.
    /// </summary>
    public sealed class QuadMapBuffer : MapBuffer
    {
        /// <inheritdoc />
        public QuadMapBuffer(int size) : base(size, 4, null, 6, PrimitiveType.Triangles)
        {
        }
    }
}