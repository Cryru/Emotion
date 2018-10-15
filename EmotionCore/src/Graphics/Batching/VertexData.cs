// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Runtime.InteropServices;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics.Batching
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public static readonly int SizeInBytes = Marshal.SizeOf(new VertexData());

        // Location 0
        public Vector3 Vertex;

        // Location 1
        public Vector2 UV;

        // Location 2
        public float Tid;
        // todo: this should be an int.

        // Location 3
        public uint Color;
    }
}