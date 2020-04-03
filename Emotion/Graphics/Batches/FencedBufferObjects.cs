#region Using

using Emotion.Graphics.Objects;

#endregion

namespace Emotion.Graphics.Batches
{
    public class FencedBufferObjects
    {
        public VertexBuffer VBO { get; private set; }
        public VertexArrayObject VAO { get; private set; }

        public FencedBufferObjects(VertexBuffer vbo, VertexArrayObject vao)
        {
            VBO = vbo;
            VAO = vao;
        }
    }
}