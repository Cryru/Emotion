using Emotion.Primitives;
using Emotion.Standard.Parsers.OpenType;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Emotion.ExecTest.Experiment;

public static class GPURenderTextExperiment
{
    private static Rectangle MeasureText(string text, Font font, float size)
    {
        float x = 0;
        float y = 0;
        float unitsPerEm = font.UnitsPerEm;

        // If hinting is enabled, worldSize must be an integer and defines the font size in pixels used for hinting.
        // Otherwise, worldSize can be an arbitrary floating-point value.
        float emSize = unitsPerEm;

        float fontHeight = font.Height;
        float worldSize = size / unitsPerEm;

        bool hinting = false;

        Vector2 min = new Vector2();
        Vector2 max = new Vector2();

        float originalX = x;

        char previous = '\0';
        foreach (char charCode in text)
        {
            if (charCode == '\r') continue;

            if (charCode == '\n')
            {
                x = originalX;
                y -= (float)fontHeight / (float)unitsPerEm * worldSize;
                if (hinting) y = MathF.Round(y);
                continue;
            }

            if (!font.CharToGlyph.TryGetValue(charCode, out FontGlyph glyph))
            {
                glyph = font.Glyphs[0];
            }

            // Note: Do not apply dilation here, we want to calculate exact bounds.
            float u0 = glyph.LeftSideBearing / emSize;
            float v0 = (glyph.TopSideBearing - glyph.ONE_Height) / emSize;
            float u1 = (glyph.LeftSideBearing + glyph.ONE_Width) / emSize;
            float v1 = glyph.TopSideBearing / emSize;

            float x0 = x + u0 * worldSize;
            float y0 = y + v0 * worldSize;
            float x1 = x + u1 * worldSize;
            float y1 = y + v1 * worldSize;

            if (min.X == 0 || x0 < min.X) min.X = x0;
            if (min.Y == 0 || y0 < min.Y) min.Y = y0;
            if (max.X == 0 || x1 > max.X) max.X = x1;
            if (max.Y == 0 || y1 > max.Y) max.Y = y1;

            x += glyph.AdvanceWidth / emSize * worldSize;
            previous = charCode;
        }

        return Rectangle.FromMinMaxPoints(min, max);
    }

    private const int MAX_CURVES_PER_GLYPH = 200;
    private const int CURVE_ELEMENTS_SIZE = 2 + 2 + 2;

    struct BezierQuadCurve
    {
        public Vector2 P1;
        public Vector2 P2; // Control Point
        public Vector2 P3;
    }

    private static List<BezierQuadCurve> GetGlyphCurves(FontGlyph glyph, Vector2 glyphOffset)
    {
        var curves = new List<BezierQuadCurve>();

        Vector2 contourStart = Vector2.Zero;
        Vector2 pen = Vector2.Zero;
        for (int e = 0; e < glyph.Commands.Length; e++)
        {
            GlyphDrawCommand command = glyph.Commands[e];
            Vector2 nextPoint = command.P0 + glyphOffset;
            Vector2 controlPoint = command.P1 + glyphOffset;

            if (command.Type == GlyphDrawCommandType.Close)
            {
                nextPoint = contourStart;
                controlPoint = (pen + contourStart) / 2f;
            }
            else if (command.Type == GlyphDrawCommandType.Line)
            {
                controlPoint = (pen + nextPoint) / 2f;
            }

            if (command.Type != GlyphDrawCommandType.Move && pen != nextPoint)
            {
                curves.Add(new BezierQuadCurve
                {
                    P1 = pen,
                    P2 = controlPoint,
                    P3 = nextPoint,
                });
            }

            if (command.Type == GlyphDrawCommandType.Close)
            {
                pen = contourStart;
                contourStart = Vector2.Zero;
                continue;
            }

            pen = nextPoint;
            if (contourStart == Vector2.Zero) contourStart = nextPoint;
        }

        return curves;
    }

    public static void RenderFont(Vector2 position, string text, Font font, float size, Color col)
    {
        Rectangle bb = MeasureText(text, font, size);
        bb.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        float cx = -(0.5f * (min.X + max.X));
        float cy = -(0.5f * (min.Y + max.Y));

        int visibleGlyphCount = 0;
        foreach (char charCode in text)
        {
            if (charCode == '\r') continue;
            if (charCode == '\n') continue;

            if (font.CharToGlyph.TryGetValue(charCode, out FontGlyph glyph) && glyph.Commands != null && glyph.Commands.Length > 0)
            {
                visibleGlyphCount++;
            }
        }

        int PER_GLYPH_ELEMENTS = MAX_CURVES_PER_GLYPH * CURVE_ELEMENTS_SIZE;
        ushort[] curveTexture = new ushort[visibleGlyphCount * PER_GLYPH_ELEMENTS];

        foreach (char charCode in text)
        {
            if (charCode == '\r') continue;
            if (charCode == '\n') continue;

            if (font.CharToGlyph.TryGetValue(charCode, out FontGlyph glyph) && glyph.Commands != null && glyph.Commands.Length > 0)
            {
                List<BezierQuadCurve> curves = GetGlyphCurves(glyph, Vector2.Zero);


            }
        }

        //Vector2 curveTextureSize = new Vector2();

        //bool a = true;
    }
}
