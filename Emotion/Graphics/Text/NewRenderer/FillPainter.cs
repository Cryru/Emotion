#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

namespace Emotion.Graphics.Text.NewRenderer
{
    public class FillPainter
    {
        private Vector2 fan_pos;
        private Vector2 prev_pos;
        public List<SdfVertex> vertices = new List<SdfVertex>();

        public void move_to(Vector2 p0)
        {
            fan_pos = p0;
            prev_pos = p0;
        }

        public void line_to(Vector2 p1)
        {
            FillTriangle(fan_pos, prev_pos, p1, vertices);
            prev_pos = p1;
        }

        public void qbez_to(Vector2 p1, Vector2 p2)
        {
            FillTriangle(fan_pos, prev_pos, p2, vertices);

            var v0 = new SdfVertex(prev_pos, new Vector2(-1.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            var v1 = new SdfVertex(p1, new Vector2(0.0f, -1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            var v2 = new SdfVertex(p2, new Vector2(1.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            prev_pos = p2;
        }

        public void close()
        {
            if ((fan_pos - prev_pos).LengthSquared() < 1e-7) return;
            line_to(fan_pos);
        }

        public static void FillTriangle(Vector2 p0, Vector2 p1, Vector2 p2, List<SdfVertex> vertices)
        {
            SdfVertex v0, v1, v2;
            v0 = new SdfVertex(p0, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            v1 = new SdfVertex(p1, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            v2 = new SdfVertex(p2, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
        }
    }
}