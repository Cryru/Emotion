#region Using

using System.Runtime.InteropServices;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics.Text.EmotionSDF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SdfVertex
    {
        [VertexAttribute(2, false)] public Vector2 Pos;

        // Vertex pos in parabola space
        [VertexAttribute(2, false)] public Vector2 Parabola;

        // Parabolic segment
        [VertexAttribute(2, false)] public Vector2 Limit;

        [VertexAttribute(1, false)] public float Scale;

        [VertexAttribute(1, false)] public float LineWidth;

        public SdfVertex(Vector2 pos, float lineWidth)
        {
            Pos = pos;
            Parabola = Vector2.Zero;
            Limit = Vector2.Zero;
            Scale = 0f;
            LineWidth = lineWidth;
        }

        public SdfVertex(Vector2 pos, Vector2 parabola, Vector2 limit, float scale, float lineWidth)
        {
            Pos = pos;
            Parabola = parabola;
            Limit = limit;
            Scale = scale;
            LineWidth = lineWidth;
        }
    }
}