#region Using

using System.Collections.Generic;

#endregion

namespace Emotion.Standard.OpenType.Helpers
{
    public class CffGlyphFactoryOld : ICffGlyphFactory
    {
        public Glyph Glyph;

        public bool Started;
        public float FirstX;
        public float FirstY;
        public float X;
        public float Y;
        public List<GlyphVertex> VerticesInProgress = new List<GlyphVertex>();

        public CffGlyphFactoryOld()
        {
            Glyph = new Glyph();
        }

        public void Done()
        {
            Glyph.Vertices = VerticesInProgress.ToArray();
        }

        public void MoveTo(float dx, float dy)
        {
            CloseShape();
            X += dx;
            FirstX = X;
            Y += dy;
            FirstY = Y;

            Vertex(VertexTypeFlag.Move, (int) X, (int) Y, 0, 0, 0, 0);
        }

        public void CloseShape()
        {
            if (FirstX != X || FirstY != Y)
                Vertex(VertexTypeFlag.Line, (int) FirstX, (int) FirstY, 0,
                    0, 0, 0);
        }

        public void Vertex(VertexTypeFlag type, int x, int y, int cx, int cy, int cx1, int cy1)
        {
            TrackVertex(x, y);
            if (type == VertexTypeFlag.Cubic)
            {
                TrackVertex(cx, cy);
                TrackVertex(cx1, cy1);
            }

            VerticesInProgress.Add(new GlyphVertex
            {
                TypeFlag = type,
                X = (short) x,
                Y = (short) y,
                Cx = (short) cx,
                Cy = (short) cy,
                Cx1 = (short) cx1,
                Cy1 = (short) cy1
            });
        }

        public void TrackVertex(int dx, int dy)
        {
            if (dx > Glyph.XMax || !Started)
                Glyph.XMax = (short) dx;
            if (dy > Glyph.YMax || !Started)
                Glyph.YMax = (short) dy;
            if (dx < Glyph.XMin || !Started)
                Glyph.XMin = (short) dx;
            if (dy < Glyph.YMin || !Started)
                Glyph.YMin = (short) dy;
            Started = true;
        }

        public void LineTo(float dx, float dy)
        {
            X += dx;
            Y += dy;
            Vertex(VertexTypeFlag.Line, (int) X, (int) Y, 0, 0, 0,
                0);
        }

        public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2,
            float dx3, float dy3)
        {
            float cx1 = X + dx1;
            float cy1 = Y + dy1;
            float cx2 = cx1 + dx2;
            float cy2 = cy1 + dy2;
            X = cx2 + dx3;
            Y = cy2 + dy3;
            Vertex(VertexTypeFlag.Cubic, (int) X, (int) Y, (int) cx1, (int) cy1,
                (int) cx2, (int) cy2);
        }
    }
}