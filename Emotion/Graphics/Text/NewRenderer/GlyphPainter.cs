#region Using

using System.Numerics;
using Emotion.Standard.OpenType;

#endregion

#nullable enable

namespace Emotion.Graphics.Text.NewRenderer
{
    public class GlyphPainter
    {
        public LinePainter LinePainter = new ();
        public FillPainter FillPainter = new ();

        public void RasterizeGlyph(FontGlyph glyph, Vector2 pos, float scale, float sdfDist)
        {
            GlyphDrawCommand[]? fontCommands = glyph.Commands;
            if (fontCommands == null || fontCommands.Length == 0) return;

            for (var i = 0; i < fontCommands.Length; i++)
            {
                GlyphDrawCommand cmd = fontCommands[i];
                switch (cmd.Type)
                {
                    case GlyphDrawCommandType.Move:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        FillPainter.MoveTo(p0);
                        LinePainter.MoveTo(p0);
                        break;
                    }
                    case GlyphDrawCommandType.Line:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        FillPainter.LineTo(p0);
                        LinePainter.LineTo(p0, sdfDist);
                        break;
                    }
                    case GlyphDrawCommandType.Curve:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        Vector2 controlPoint = cmd.P1 * scale + pos;
                        FillPainter.QuadraticTo(controlPoint, p0);
                        LinePainter.QuadraticTo(controlPoint, p0, sdfDist);
                        break;
                    }
                    case GlyphDrawCommandType.Close:
                    {
                        FillPainter.Close();
                        LinePainter.Close(sdfDist);
                        break;
                    }
                }
            }
        }
    }
}