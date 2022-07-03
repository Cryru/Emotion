#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

namespace Emotion.Graphics.Text.NewRenderer
{
    public class LinePainter
    {
        private Vector2 start_pos;
        private Vector2 prev_pos;
        public List<SdfVertex> vertices = new List<SdfVertex>();

        public void move_to(Vector2 p0)
        {
            start_pos = p0;
            prev_pos = p0;
        }

        public void line_to(Vector2 p1, float lineWidth)
        {
            Vector2 vmin = Vector2.Min(prev_pos, p1);
            Vector2 vmax = Vector2.Max(prev_pos, p1);

            vmin -= new Vector2(lineWidth);
            vmax += new Vector2(lineWidth);

            Parabola par = Parabola.FromLine(prev_pos, p1);
            LineRect(par, vmin, vmax, lineWidth, vertices);

            prev_pos = p1;
        }

        public void qbez_to(Vector2 p1, Vector2 p2, float line_width)
        {
            Vector2 p0 = prev_pos;

            Vector2 mid01 = new Vector2(0.5f) * (p0 + p1);
            Vector2 mid12 = new Vector2(0.5f) * (p1 + p2);

            Vector2 vmin = Vector2.Min(p0, mid01);
            vmin = Vector2.Min(vmin, mid12);
            vmin = Vector2.Min(vmin, p2);

            Vector2 vmax = Vector2.Max(p0, mid01);
            vmax = Vector2.Max(vmax, mid12);
            vmax = Vector2.Max(vmax, p2);

            vmin -= new Vector2(line_width);
            vmax += new Vector2(line_width);

            Vector2 v10 = p0 - p1;
            Vector2 v12 = p2 - p1;
            Vector2 np10 = Vector2.Normalize(v10);
            Vector2 np12 = Vector2.Normalize(v12);

            QbezType qtype = qbez_type(np10, np12);

            switch (qtype)
            {
                case QbezType.Parabola:
                {
                    var par = Parabola.FromQuadratic(p0, p1, p2);
                    LineRect(par, vmin, vmax, line_width, vertices);
                    break;
                }
                case QbezType.Line:
                {
                    var par = Parabola.FromLine(p0, p2);
                    LineRect(par, vmin, vmax, line_width, vertices);
                    break;
                }
                case QbezType.TwoLines:
                {
                    float l10 = v10.Length();
                    float l12 = v12.Length();
                    float qt = l10 / (l10 + l12);
                    float nqt = 1.0f - qt;
                    Vector2 qtop = p0 * (nqt * nqt) + p1 * (2.0f * nqt * qt) + p2 * (qt * qt);
                    Parabola par0 = Parabola.FromLine(p0, qtop);
                    LineRect(par0, vmin, vmax, line_width, vertices);
                    Parabola par1 = Parabola.FromLine(qtop, p1);
                    LineRect(par1, vmin, vmax, line_width, vertices);
                    break;
                }
            }

            prev_pos = p2;
        }

        public static void LineRect(Parabola par, Vector2 vmin, Vector2 vmax, float lineWidth, List<SdfVertex> vertices)
        {
            SdfVertex v0, v1, v2, v3;
            v0 = new SdfVertex(new Vector2(vmin.X, vmin.Y), lineWidth);
            v1 = new SdfVertex(new Vector2(vmax.X, vmin.Y), lineWidth);
            v2 = new SdfVertex(new Vector2(vmax.X, vmax.Y), lineWidth);
            v3 = new SdfVertex(new Vector2(vmin.X, vmax.Y), lineWidth);

            set_par_vertex(ref v0, par);
            set_par_vertex(ref v1, par);
            set_par_vertex(ref v2, par);
            set_par_vertex(ref v3, par);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            vertices.Add(v0);
            vertices.Add(v2);
            vertices.Add(v3);
        }

        public void close(float lineWidth)
        {
            if ((start_pos - prev_pos).LengthSquared() < 1e-7) return;
            line_to(start_pos, lineWidth);
        }

        public enum QbezType
        {
            Parabola,
            Line,
            TwoLines
        }

        public static QbezType qbez_type(Vector2 np10, Vector2 np12)
        {
            float d = Vector2.Dot(np10, np12);
            float dmax = 1.0f - 1e-6f;
            if (d >= dmax) return QbezType.TwoLines;
            if (d <= -dmax) return QbezType.Line;
            return QbezType.Parabola;
        }


        public static void set_par_vertex(ref SdfVertex v, Parabola par)
        {
            Vector2 par_pos = par.world_to_par(v.Pos);
            v.Parabola = par_pos;
            v.Limit = new Vector2(par.XStart, par.XEnd);
            v.Scale = par.Scale;
        }
    }
}