#region Using

using System.Collections.Generic;
using System.Numerics;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Standard.OpenType.Helpers
{
    /// <summary>
    /// This is a helper class to gather the relevant part of parsing cff glyphs in one place.
    /// </summary>
    public sealed class CffGlyphFactory
    {
        public FontGlyph Glyph;

        public bool Started;

        public float Scale;

        private List<GlyphDrawCommand> _commandsInProgress = new List<GlyphDrawCommand>();
        private Vector2 _pen;

        public CffGlyphFactory(float scale)
        {
            Scale = scale;
            Glyph = new FontGlyph();
        }

        public void Done()
        {
            Glyph.Min *= Scale;
            Glyph.Max *= Scale;
            Glyph.Commands = _commandsInProgress.ToArray();
        }

        public void MoveTo(float dx, float dy)
        {
            CloseShape();
            _pen += new Vector2(dx, dy);

            PushCommand(GlyphDrawCommandType.Move, _pen, Vector2.Zero);
            EnsureBounds(_pen);
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
            _pen += new Vector2(dx, dy);
            PushCommand(GlyphDrawCommandType.Line, _pen, Vector2.Zero);
            EnsureBounds(_pen);
        }

        public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2, float dx3, float dy3)
        {
            Vector2 prevPos = _pen;

            float cx1 = prevPos.X + dx1;
            float cy1 = prevPos.Y + dy1;
            float cx2 = cx1 + dx2;
            float cy2 = cy1 + dy2;
            _pen = new Vector2(cx2 + dx3, cy2 + dy3);

            // Convert cubic curves to quad curves for easier rendering.
            List<Vector2>? quadCurves = CubicCurveConverter.CubicToQuad(prevPos, new Vector2(cx1, cy1), new Vector2(cx2, cy2), _pen);
            for (var i = 1; i < quadCurves.Count; i += 2)
            {
                Vector2 endPoint = quadCurves[i + 1];
                Vector2 controlPoint = quadCurves[i];
                PushCommand(GlyphDrawCommandType.Curve, endPoint, controlPoint);
            }

            EnsureBounds(_pen);
            EnsureBounds(new Vector2(cx1, cy1));
            EnsureBounds(new Vector2(cx2, cy2));
        }

        private void PushCommand(GlyphDrawCommandType type, Vector2 pos, Vector2 cp)
        {
            _commandsInProgress.Add(new GlyphDrawCommand
            {
                Type = type,
                P0 = pos * Scale,
                P1 = cp * Scale,
            });
        }

        private void EnsureBounds(Vector2 p)
        {
            float dx = p.X;
            float dy = p.Y;

            if (dx > Glyph.Max.X || !Started)
                Glyph.Max.X = dx;
            if (dy > Glyph.Max.Y || !Started)
                Glyph.Max.Y = dy;
            if (dx < Glyph.Min.X || !Started)
                Glyph.Min.X = dx;
            if (dy < Glyph.Min.Y || !Started)
                Glyph.Min.Y = dy;
            Started = true;
        }
    }
}