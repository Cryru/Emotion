#region Using

using System.Numerics;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics.Data
{
    /// <summary>
    /// Represents the data of a single vertex within a model with skeletal animation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexDataWithBones
    {
        /// <summary>
        /// The size of vertex data in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new VertexDataWithBones());

        /// <summary>
        /// The vertex itself.
        /// </summary>
        [VertexAttribute(3, false)] public Vector3 Vertex;

        /// <summary>
        /// The UV of the vertex's texture.
        /// </summary>
        [VertexAttribute(2, false)] public Vector2 UV;

        /// <summary>
        /// Indices to the entity's bones indicating which bones affect this vertex.
        /// </summary>
        [VertexAttribute(4, false)]
        public Vector4 BoneIds;

        /// <summary>
        /// Weights that specify how much each bone affecting the vertex affects it.
        /// All bone weights must sum up to 1.
        /// </summary>
        [VertexAttribute(4, false)]
        public Vector4 BoneWeights;
    }
}