using System.Runtime.InteropServices;

namespace Emotion.Graphics.Data;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData_Pos_UV_Normal_Color
{
    public readonly static VertexDataDescription Format = new VertexDataDescription()
        .AddVertexPosition()
        .AddUV(1)
        .AddNormal()
        .AddVertexColor()
        .Build();

    [VertexAttribute(3, false)]
    public Vector3 Position;

    [VertexAttribute(2, false)]
    public Vector2 UV;

    [VertexAttribute(3, false)]
    public Vector3 Normal;

    [VertexAttribute(4, true, typeof(byte))]
    public uint Color;
}
