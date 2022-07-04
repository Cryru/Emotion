#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

#nullable enable

namespace Emotion.Standard.OpenType.Helpers
{
    public interface ICffGlyphFactory
    {
        public void Done();
        public void MoveTo(float dx, float dy);
        public void CloseShape();
        public void LineTo(float dx, float dy);
        public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2, float dx3, float dy3);
    }

    /// <summary>
    /// This is a helper class to gather the relevant part of parsing cff glyphs in one place.
    /// </summary>
    public sealed class CffGlyphFactory : ICffGlyphFactory
    {
        public FontGlyph Glyph;

        public bool Started;

        public float Scale;
        public float X;
        public float Y;
        public List<GlyphDrawCommand> VerticesInProgress = new List<GlyphDrawCommand>();

        public CffGlyphFactory(float scale)
        {
            Scale = scale;
            Glyph = new FontGlyph();
        }

        public void Done()
        {
            Glyph.Commands = VerticesInProgress.ToArray();
        }

        public void MoveTo(float dx, float dy)
        {
            CloseShape();
            X += dx;
            Y += dy;

            Vertex(GlyphDrawCommandType.Move, (int) X, (int) Y, 0, 0, 0, 0);
        }

        public void CloseShape()
        {
            if (VerticesInProgress.Count == 0) return;

            VerticesInProgress.Add(new GlyphDrawCommand
            {
                Type = GlyphDrawCommandType.Close
            });
        }

        public void LineTo(float dx, float dy)
        {
            X += dx;
            Y += dy;
            Vertex(GlyphDrawCommandType.Line, (int) X, (int) Y, 0, 0, 0, 0);
        }

        public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2, float dx3, float dy3)
        {
            float cx1 = X + dx1;
            float cy1 = Y + dy1;
            float cx2 = cx1 + dx2;
            float cy2 = cy1 + dy2;
            X = cx2 + dx3;
            Y = cy2 + dy3;
            Vertex(GlyphDrawCommandType.Line, (int) X, (int) Y, (int) cx1, (int) cy1, (int) cx2, (int) cy2);
        }

        private void Vertex(GlyphDrawCommandType type, int x, int y, int cx, int cy, int cx1, int cy1)
        {
            TrackVertex(x, y);
            if (type == GlyphDrawCommandType.Curve)
            {
                TrackVertex(cx, cy);
                TrackVertex(cx1, cy1);
            }

            VerticesInProgress.Add(new GlyphDrawCommand
            {
                Type = type,
                P0 = new Vector2(x, y),
                P1 = new Vector2(cx, cy),

                //X = (short) x,
                //Y = (short) y,
                //Cx = (short) cx,
                //Cy = (short) cy,
                //Cx1 = (short) cx1,
                //Cy1 = (short) cy1
            });
        }

        private void TrackVertex(int dx, int dy)
        {
            if (dx > Glyph.Max.X || !Started)
                Glyph.Max.X = (short) dx;
            if (dy > Glyph.Max.Y || !Started)
                Glyph.Max.Y = (short) dy;
            if (dx < Glyph.Min.X || !Started)
                Glyph.Min.X = (short) dx;
            if (dy < Glyph.Min.Y || !Started)
                Glyph.Min.Y = (short) dy;
            Started = true;
        }
    }
}