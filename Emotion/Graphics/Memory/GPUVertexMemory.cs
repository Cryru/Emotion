#nullable enable

using Emotion.Graphics.Data;
using OpenGL;

namespace Emotion.Graphics.Memory;

public class GPUVertexMemory
{
    public VertexDataFormat Format { get => VAO.Format; }

    public VertexArrayObjectFromFormat VAO;
    public VertexBuffer VBO;

    public GPUVertexMemory(VertexDataFormat format)
    {
        VBO = new VertexBuffer(0, BufferUsage.StaticDraw);
        VAO = new VertexArrayObjectFromFormat(format, VBO);
    }
}
