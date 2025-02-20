using System.Runtime.InteropServices;

namespace Emotion.Graphics.Data;

[StructLayout(LayoutKind.Sequential)]
public struct VertexDataWithNormal
{
    public static readonly int SizeInBytes = Marshal.SizeOf(new VertexDataWithNormal());

    [VertexAttribute(3, false)] public Vector3 Vertex;
    [VertexAttribute(2, false)] public Vector2 UV;
    [VertexAttribute(4, true, typeof(byte))] public uint Color;
    [VertexAttribute(3, false)] public Vector3 Normal;
}