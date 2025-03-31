namespace Emotion.Graphics.Data;

public struct VertexData_Pos_UV_Normal
{
    public readonly static VertexDataDescription Descriptor = new VertexDataDescription()
        .AddVertexPosition()
        .AddUV(1)
        .AddNormal()
        .Build();

    public Vector3 Position;
    public Vector2 UV;
    public Vector3 Normal;
}
