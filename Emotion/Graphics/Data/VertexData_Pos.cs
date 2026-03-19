#nullable enable

using System.Runtime.InteropServices;

namespace Emotion.Graphics.Data;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData_Pos
{
    public readonly static VertexDataFormat Format = new VertexDataFormat()
       .AddVertexPosition()
       .Build();

    public Vector3 Position;
}