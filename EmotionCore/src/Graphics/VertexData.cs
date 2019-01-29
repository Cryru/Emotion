// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Numerics;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// Represents the data of a single vertex.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        /// <summary>
        /// The size of vertex data in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(new VertexData());

        /// <summary>
        /// The vertex itself.
        /// </summary>
        public Vector3 Vertex;

        /// <summary>
        /// The UV of the vertex's texture.
        /// </summary>
        public Vector2 UV;

        /// <summary>
        /// The texture's id within the loaded textures.
        /// </summary>
        public float Tid;
        // todo: this should be an int.

        /// <summary>
        /// The packed color of the vertex.
        /// </summary>
        public uint Color;
    }
}