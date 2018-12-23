// Emotion - https://github.com/Cryru/Emotion

#region Using

using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics.Batching
{
    /// <summary>
    /// A map buffer optimized for drawing quads.
    /// </summary>
    public sealed class TriMapBuffer : MapBuffer
    {
        /// <summary>
        /// Create a triangle map buffer of the specified size.
        /// </summary>
        /// <param name="size">The size of the map buffer in objects.</param>
        public TriMapBuffer(int size) : base(size, 3, null, 3, PrimitiveType.Triangles)
        {
        }
    }
}