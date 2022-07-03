#region Using

using System;
using System.Numerics;

#endregion

namespace Emotion.Graphics.Text.NewRenderer
{
    public class Parabola
    {
        public Matrix3x2 Matrix;
        public float Scale;
        public float XStart;
        public float XEnd;

        public static Parabola FromQuadratic(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            Parabola res = new Parabola();

            Vector2 pc = Vector2.Lerp(p0, p2, 0.5f);
            Vector2 yaxis = Vector2.Normalize(pc - p1);
            Vector2 xaxis = perp_right(yaxis);

            Vector2 p01 = Vector2.Normalize(p1 - p0);
            Vector2 p12 = Vector2.Normalize(p2 - p1);
            float cx0 = Vector2.Dot(xaxis, p01);
            float sx0 = Vector2.Dot(yaxis, p01);
            float cx2 = Vector2.Dot(xaxis, p12);
            float sx2 = Vector2.Dot(yaxis, p12);

            float x0 = sx0 / cx0 * 0.5f;
            float x2 = sx2 / cx2 * 0.5f;
            float y0 = x0 * x0;

            float p02x = Vector2.Dot(p2 - p0, xaxis);
            float scale = p02x / (x2 - x0);

            Vector2 vertex = p0 - new Vector2(y0 * scale) * yaxis - new Vector2(x0 * scale) * xaxis;

            res.Scale = scale;
            res.Matrix = new Matrix3x2(xaxis.X, xaxis.Y, yaxis.X, yaxis.Y, vertex.X, vertex.Y);

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
            float px0 = 100.0f;
            float px1 = 100.0f + 1e-3f;

            float py0 = px0 * px0;
            float py1 = px1 * px1;
            float plen = (new Vector2(px0, py0) - new Vector2(px1, py1)).Length();
            float pslope = 2.0f * px0 / MathF.Sqrt(1.0f * 4.0f * px0 * px0);
            float len = (p1 - p0).Length();
            Vector2 ldir = (p1 - p0) / len;
            Vector2 lndir = perp_right(ldir);
            float scale = len / plen;
            Vector2 rc = p0 + (ldir * (px1 - px0) + lndir * (py1 - py0)) * pslope * scale;
            Vector2 px = Vector2.Normalize(rc - p0);
            Vector2 py = perp_left(px);
            Vector2 pvertex = p0 - px * scale * px0 - py * scale * py0;

            Parabola res = new Parabola();

            res.Matrix = new Matrix3x2(px.X, px.Y, py.X, py.Y, pvertex.X, pvertex.Y);
            res.Scale = scale;
            res.XStart = px0;
            res.XEnd = px1;
            return res;
        }

        public Vector2 world_to_par(Vector2 pos)
        {
            float iss = 1.0f / Scale;
            Vector2 dpos = pos - new Vector2(Matrix.M31, Matrix.M32);
            Vector2 r0 = dpos * new Vector2(Matrix.M11, Matrix.M12);
            Vector2 r1 = dpos * new Vector2(Matrix.M21, Matrix.M22);
            float v0 = iss * (r0.X + r0.Y);
            float v1 = iss * (r1.X + r1.Y);
            return new Vector2(v0, v1);
        }

        public static Vector2 perp_right(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        public static Vector2 perp_left(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }
    }
}