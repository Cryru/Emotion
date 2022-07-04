#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.NewRenderer
{
    public class LinePainter
    {
        private Vector2 _startPos;
        private Vector2 _prevPos;
        public List<SdfVertex> Vertices = new();

        public void MoveTo(Vector2 p0)
        {
            _startPos = p0;
            _prevPos = p0;
        }

        public void LineTo(Vector2 nextPos, float lineWidth)
        {
            Vector2 vMin = Vector2.Min(_prevPos, nextPos);
            Vector2 vMax = Vector2.Max(_prevPos, nextPos);

            vMin -= new Vector2(lineWidth);
            vMax += new Vector2(lineWidth);

            Parabola par = Parabola.FromLine(_prevPos, nextPos);
            LineRect(par, vMin, vMax, lineWidth, Vertices);

            _prevPos = nextPos;
        }

        public void QuadraticTo(Vector2 controlPoint, Vector2 nextPos, float lineWidth)
        {
            Vector2 prevPos = _prevPos;

            Vector2 mid01 = new Vector2(0.5f) * (prevPos + controlPoint);
            Vector2 mid12 = new Vector2(0.5f) * (controlPoint + nextPos);

            Vector2 vMin = Vector2.Min(prevPos, mid01);
            vMin = Vector2.Min(vMin, mid12);
            vMin = Vector2.Min(vMin, nextPos);

            Vector2 vMax = Vector2.Max(prevPos, mid01);
            vMax = Vector2.Max(vMax, mid12);
            vMax = Vector2.Max(vMax, nextPos);

            vMin -= new Vector2(lineWidth);
            vMax += new Vector2(lineWidth);

            Vector2 v10 = prevPos - controlPoint;
            Vector2 v12 = nextPos - controlPoint;
            Vector2 np10 = Vector2.Normalize(v10);
            Vector2 np12 = Vector2.Normalize(v12);

            QuadraticCurveType qtype = GetQuadraticCurveType(np10, np12);

            switch (qtype)
            {
                case QuadraticCurveType.Parabola:
                {
                    Parabola par = Parabola.FromQuadratic(prevPos, controlPoint, nextPos);
                    LineRect(par, vMin, vMax, lineWidth, Vertices);
                    break;
                }
                case QuadraticCurveType.Line:
                {
                    Parabola par = Parabola.FromLine(prevPos, nextPos);
                    LineRect(par, vMin, vMax, lineWidth, Vertices);
                    break;
                }
                case QuadraticCurveType.TwoLines:
                {
                    float l10 = v10.Length();
                    float l12 = v12.Length();
                    float qt = l10 / (l10 + l12);
                    float nqt = 1.0f - qt;
                    Vector2 qtop = prevPos * (nqt * nqt) + controlPoint * (2.0f * nqt * qt) + nextPos * (qt * qt);
                    Parabola par0 = Parabola.FromLine(prevPos, qtop);
                    LineRect(par0, vMin, vMax, lineWidth, Vertices);
                    Parabola par1 = Parabola.FromLine(qtop, controlPoint);
                    LineRect(par1, vMin, vMax, lineWidth, Vertices);
                    break;
                }
            }

            _prevPos = nextPos;
        }

        public static void LineRect(Parabola par, Vector2 vmin, Vector2 vmax, float lineWidth, List<SdfVertex> vertices)
        {
            var v0 = new SdfVertex(new Vector2(vmin.X, vmin.Y), lineWidth);
            var v1 = new SdfVertex(new Vector2(vmax.X, vmin.Y), lineWidth);
            var v2 = new SdfVertex(new Vector2(vmax.X, vmax.Y), lineWidth);
            var v3 = new SdfVertex(new Vector2(vmin.X, vmax.Y), lineWidth);

            SetParabolaProperties(ref v0, par);
            SetParabolaProperties(ref v1, par);
            SetParabolaProperties(ref v2, par);
            SetParabolaProperties(ref v3, par);

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            vertices.Add(v0);
            vertices.Add(v2);
            vertices.Add(v3);
        }

        public void Close(float lineWidth)
        {
            if ((_startPos - _prevPos).LengthSquared() < Maths.EPSILON) return;
            LineTo(_startPos, lineWidth);
        }

        public enum QuadraticCurveType
        {
            Parabola,
            Line,
            TwoLines
        }

        public static QuadraticCurveType GetQuadraticCurveType(Vector2 np10, Vector2 np12)
        {
            float d = Vector2.Dot(np10, np12);
            float dMax = 1.0f - 1e-6f;
            if (d >= dMax) return QuadraticCurveType.TwoLines;
            if (d <= -dMax) return QuadraticCurveType.Line;
            return QuadraticCurveType.Parabola;
        }

        public static void SetParabolaProperties(ref SdfVertex v, Parabola par)
        {
            Vector2 parPos = par.world_to_par(v.Pos);
            v.Parabola = parPos;
            v.Limit = new Vector2(par.XStart, par.XEnd);
            v.Scale = par.Scale;
        }
    }
}