#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Standard.MathLib;

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

        private List<GlyphDrawCommand> _commandsInProgress = new List<GlyphDrawCommand>();

        public CffGlyphFactory(float scale)
        {
            Scale = scale;
            Glyph = new FontGlyph();
        }

        public void Done()
        {
            Glyph.Commands = _commandsInProgress.ToArray();
        }

        public void MoveTo(float dx, float dy)
        {
            CloseShape();
            X += dx;
            Y += dy;

            PushCommand(GlyphDrawCommandType.Move, X, Y, 0, 0);
        }

        public void CloseShape()
        {
            if (_commandsInProgress.Count == 0) return;

            _commandsInProgress.Add(new GlyphDrawCommand
            {
                Type = GlyphDrawCommandType.Close
            });
        }

        public void LineTo(float dx, float dy)
        {
            X += dx;
            Y += dy;
            PushCommand(GlyphDrawCommandType.Line, X, Y, 0, 0);
        }

        public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2, float dx3, float dy3)
        {
            var prevPos = new Vector2(X, Y);

            float cx1 = X + dx1;
            float cy1 = Y + dy1;
            float cx2 = cx1 + dx2;
            float cy2 = cy1 + dy2;
            X = cx2 + dx3;
            Y = cy2 + dy3;

            // Convert cubic curves to quad curves for easier rendering.
            List<Vector2>? quadCurves = CubicCurveConverter.CubicToQuad(prevPos, new Vector2(cx1, cy1), new Vector2(cx2, cy2), new Vector2(X, Y));
            for (var i = 1; i < quadCurves.Count; i += 2)
            {
                Vector2 endPoint = quadCurves[i + 1];
                Vector2 controlPoint = quadCurves[i];
                PushCommand(GlyphDrawCommandType.Curve, endPoint.X, endPoint.Y, controlPoint.X, controlPoint.Y);
            }
        }

        private void PushCommand(GlyphDrawCommandType type, float x, float y, float cx, float cy)
        {
            EnsureBounds(x, y);
            if (type == GlyphDrawCommandType.Curve) EnsureBounds(cx, cy);

            _commandsInProgress.Add(new GlyphDrawCommand
            {
                Type = type,
                P0 = new Vector2(x, y),
                P1 = new Vector2(cx, cy),
            });
        }

        private void EnsureBounds(float dx, float dy)
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