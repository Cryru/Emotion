// Emotion - https://github.com/Cryru/Emotion

#region Using

using System.Runtime.InteropServices;
using Emotion.Primitives;

#endregion

namespace Emotion.Graphics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public static readonly int SizeInBytes = Marshal.SizeOf(new VertexData());

        // Location 0
        public Vector3 Vertex;

        // Location 1
        public uint Color;
    }
}