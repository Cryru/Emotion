#region Using


#endregion

#nullable enable

namespace Emotion.Graphics.Text.EmotionSDF
{
    public class FillPainter
    {
        private Vector2 _fanPos;
        private Vector2 _prevPos;
        public List<SdfVertex> Vertices = new();

        public void MoveTo(Vector2 p0)
        {
            _fanPos = p0;
            _prevPos = p0;
        }

        public void LineTo(Vector2 p0)
        {
            FillTriangle(_fanPos, _prevPos, p0);
            _prevPos = p0;
        }

        public void QuadraticTo(Vector2 controlPoint, Vector2 p0)
        {
            FillTriangle(_fanPos, _prevPos, p0);

            var v0 = new SdfVertex(_prevPos, new Vector2(-1.0f, 1.0f), Vector2.Zero, 0.0f, 0.0f);
            var v1 = new SdfVertex(controlPoint, new Vector2(0.0f, -1.0f), Vector2.Zero, 0.0f, 0.0f);
            var v2 = new SdfVertex(p0, new Vector2(1.0f, 1.0f), Vector2.Zero, 0.0f, 0.0f);

            Vertices.Add(v0);
            Vertices.Add(v1);
            Vertices.Add(v2);

            _prevPos = p0;
        }

        public void Close()
        {
            if ((_fanPos - _prevPos).LengthSquared() < Maths.EPSILON) return;
            LineTo(_fanPos);
        }

        public void FillTriangle(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var v0 = new SdfVertex(p0, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            var v1 = new SdfVertex(p1, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);
            var v2 = new SdfVertex(p2, new Vector2(0.0f, 1.0f), new Vector2(0.0f), 0.0f, 0.0f);

            Vertices.Add(v0);
            Vertices.Add(v1);
            Vertices.Add(v2);
        }
    }
}