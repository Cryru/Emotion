namespace Emotion.Graphics.Data;

public struct VertexData_Pos_UV_Normal_Color
{
    public readonly static VertexDataFormat Format = new VertexDataFormat()
        .AddVertexPosition()
        .AddUV(1)
        .AddNormal()
        .AddVertexColor()
        .Build();

    public Vector3 Position;

    public Vector2 UV;

    public Vector3 Normal;

    public uint Color;
}