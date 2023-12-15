#region Using

#endregion

namespace Emotion.Graphics.Text.EmotionSDF
{
    public class Parabola
    {
        public Matrix3x2 Matrix;
        public float Scale;
        public float XStart;
        public float XEnd;

        public static Parabola FromQuadratic(Vector2 p0, Vector2 cp, Vector2 p1)
        {
            var res = new Parabola();

            Vector2 pc = Vector2.Lerp(p0, p1, 0.5f);
            Vector2 yAxis = Vector2.Normalize(pc - cp);
            Vector2 xAxis = PerpendicularRight(yAxis);

            Vector2 p01 = Vector2.Normalize(cp - p0);
            Vector2 p12 = Vector2.Normalize(p1 - cp);
            float cx0 = Vector2.Dot(xAxis, p01);
            float sx0 = Vector2.Dot(yAxis, p01);
            float cx2 = Vector2.Dot(xAxis, p12);
            float sx2 = Vector2.Dot(yAxis, p12);

            float x0 = sx0 / cx0 * 0.5f;
            float x2 = sx2 / cx2 * 0.5f;
            float y0 = x0 * x0;

            float pointDot = Vector2.Dot(p1 - p0, xAxis);
            float scale = pointDot / (x2 - x0);

            Vector2 vertex = p0 - new Vector2(y0 * scale) * yAxis - new Vector2(x0 * scale) * xAxis;

            res.Scale = scale;
            res.Matrix = new Matrix3x2(xAxis.X, xAxis.Y, yAxis.X, yAxis.Y, vertex.X, vertex.Y);

            if (x0 < x2)
            {
                res.XStart = x0;
                res.XEnd = x2;
            }
            else
            {
                res.XStart = x2;
                res.XEnd = x0;
            }

            return res;
        }

        public static Parabola FromLine(Vector2 p0, Vector2 p1)
        {
            const float px0 = 100.0f;
            const float px1 = 100.0f + 1e-3f;

            const float py0 = px0 * px0;
            const float py1 = px1 * px1;

            float pLength = (new Vector2(px0, py0) - new Vector2(px1, py1)).Length();
            float pSlope = 2.0f * px0 / MathF.Sqrt(1.0f * 4.0f * px0 * px0);

            float len = (p1 - p0).Length();
            Vector2 lineDir = (p1 - p0) / len;
            Vector2 lineDirNormal = PerpendicularRight(lineDir);
            float scale = len / pLength;
            Vector2 rc = p0 + (lineDir * (px1 - px0) + lineDirNormal * (py1 - py0)) * pSlope * scale;
            Vector2 px = Vector2.Normalize(rc - p0);
            Vector2 py = PerpendicularLeft(px);
            Vector2 pVertex = p0 - px * scale * px0 - py * scale * py0;

            var res = new Parabola
            {
                Matrix = new Matrix3x2(px.X, px.Y, py.X, py.Y, pVertex.X, pVertex.Y),
                Scale = scale,
                XStart = px0,
                XEnd = px1
            };

            return res;
        }

        /// <summary>
        /// Converts a vec2 from world space to parabola space.
        /// </summary>
        public Vector2 WorldToParabola(Vector2 pos)
        {
            float iss = 1.0f / Scale;
            Vector2 dPos = pos - new Vector2(Matrix.M31, Matrix.M32);
            Vector2 r0 = dPos * new Vector2(Matrix.M11, Matrix.M12);
            Vector2 r1 = dPos * new Vector2(Matrix.M21, Matrix.M22);
            float v0 = iss * (r0.X + r0.Y);
            float v1 = iss * (r1.X + r1.Y);
            return new Vector2(v0, v1);
        }

        private static Vector2 PerpendicularRight(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        private static Vector2 PerpendicularLeft(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }
    }
}