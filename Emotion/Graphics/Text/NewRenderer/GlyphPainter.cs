#region Using

using System.Numerics;
using Emotion.Standard.OpenType;

#endregion

namespace Emotion.Graphics.Text.NewRenderer
{
    public class GlyphPainter
    {
        public LinePainter LinePainter = new LinePainter();
        public FillPainter FillPainter = new FillPainter();

        public void RasterizeGlyph(TestGlyphRenderer.NewRendererGlyph glyph, Vector2 pos, float scale, float sdfDist)
        {
            FontGlyph fontGlyph = glyph.FontGlyph;
            GlyphDrawCommand[] fontCommands = fontGlyph.Commands;
            if (fontCommands == null || fontCommands.Length == 0) return;

            for (var i = 0; i < fontCommands.Length; i++)
            {
                GlyphDrawCommand cmd = fontCommands[i];
                switch (cmd.Type)
                {
                    case GlyphDrawCommandType.Move:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        FillPainter.move_to(p0);
                        LinePainter.move_to(p0);
                        break;
                    }
                    case GlyphDrawCommandType.Line:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        FillPainter.line_to(p0);
                        LinePainter.line_to(p0, sdfDist);
                        break;
                    }
                    case GlyphDrawCommandType.Curve:
                    {
                        Vector2 p0 = cmd.P0 * scale + pos;
                        Vector2 p1 = cmd.P1 * scale + pos;
                        FillPainter.qbez_to(p0, p1);
                        LinePainter.qbez_to(p0, p1, sdfDist);
                        break;
                    }
                    case GlyphDrawCommandType.Close:
                    {
                        FillPainter.close();
                        LinePainter.close(sdfDist);
                        break;
                    }
                }
            }
        }
    }
}