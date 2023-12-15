#nullable enable

#region Using

using Emotion.Standard.OpenType;
using StbTrueTypeSharp;

#endregion

namespace Emotion.Graphics.Text.RasterizationNew;

// Based on stbtruetype by nothings
// https://github.com/nothings/stb/blob/master/stb_truetype.h
public static class SoftwareGlyphRasterizer
{
    public const float FLATNESS_IN_PIXELS = 0.35f;

    public static void RenderGlyph(StbTrueType.stbtt_fontinfo info, byte[] output, int width, int height, int stride, float scaleX, float scaleY, float shiftX, float shiftY, int glyph)
    {
        float scale = scaleX > scaleY ? scaleY : scaleX;
        float flatness = FLATNESS_IN_PIXELS / scale;

        GlyphVertex[] glyphVertices = GetGlyphVertices(info, glyph);
        VerticesToContours(glyphVertices, flatness, out Vector2[]? points, out int[]? contourLengths);
        GetGlyphBitmapBoxSize(info, glyph, scaleX, scaleY, shiftX, shiftY, out int glyphWidth, out int glyphHeight);
    }

    public static void GetGlyphBitmapBoxSize(StbTrueType.stbtt_fontinfo info, int glyph, float scaleX, float scaleY, float shiftX, float shiftY, out int width, out int height)
    {
        // clean
        // Glyph box
        int w;
        int h;
        unsafe
        {
            StbTrueType.stbtt_GetGlyphBitmapBoxSubpixel(info, glyph, scaleX, scaleY, shiftX, shiftY, &w, &h, (int*) 0, (int*) 0);
        }

        width = w;
        height = h;
    }

    public static unsafe GlyphVertex[] GetGlyphVertices(StbTrueType.stbtt_fontinfo info, int glyph)
    {
        // clean
        // Convert from STB verts
        StbTrueType.stbtt_vertex* vertices;
        int numVerts = StbTrueType.stbtt_GetGlyphShape(info, glyph, &vertices);

        var glyphVertices = new GlyphVertex[numVerts];
        for (var i = 0; i < numVerts; i++)
        {
            StbTrueType.stbtt_vertex stbVert = vertices[i];
            ref GlyphVertex emVert = ref glyphVertices[i];
            emVert.X = stbVert.x;
            emVert.Y = stbVert.y;
            emVert.Cx = stbVert.cx;
            emVert.Cy = stbVert.cy;
            emVert.Cx1 = stbVert.cx1;
            emVert.Cy1 = stbVert.cy1;
            emVert.Flags = stbVert.type;
        }

        return glyphVertices;
    }

    public static void VerticesToContours(GlyphVertex[] vertices, float objFlatness, out Vector2[]? points, out int[]? windingLengths)
    {
        float flatness2 = objFlatness * objFlatness;
        points = null;
        windingLengths = null;

        var length = 0;
        for (var i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].TypeFlag == VertexTypeFlag.Move) length++;
        }

        if (length == 0) return;

        windingLengths = new int[length];

        var contourStart = 0;
        for (var pass = 0; pass < 2; pass++)
        {
            float x = 0;
            float y = 0;

            var pointsLength = 0;
            int contourCount = -1;

            for (var i = 0; i < vertices.Length; i++)
            {
                ref GlyphVertex vertex = ref vertices[i];

                switch (vertices[i].TypeFlag)
                {
                    case VertexTypeFlag.Move:
                    {
                        if (contourCount != -1) windingLengths[contourCount] = pointsLength - contourStart;
                        contourCount++;
                        contourStart = pointsLength;
                        if (points != null) points[pointsLength] = new Vector2(vertex.X, vertex.Y);
                        pointsLength++;
                        break;
                    }
                    case VertexTypeFlag.Line:
                    {
                        if (points != null) points[pointsLength] = new Vector2(vertex.X, vertex.Y);
                        pointsLength++;
                        break;
                    }
                    case VertexTypeFlag.Curve:
                    {
                        GeneratePointsForQuadCurve(points, ref pointsLength,
                            new Vector2(x, y),
                            new Vector2(vertex.Cx, vertex.Cy),
                            new Vector2(vertex.X, vertex.Y),
                            flatness2
                        );
                        break;
                    }
                    // We dont need to handle Cubic curves as the Emotion font parser will convert them to Quadratic
                }

                x = vertex.X;
                y = vertex.Y;
            }

            windingLengths[contourCount] = pointsLength - contourStart;
            if (pass == 0) points = new Vector2[pointsLength];
        }
    }

    private static void GeneratePointsForQuadCurve(Vector2[]? points, ref int offset, Vector2 p1, Vector2 pc, Vector2 p2, float flatness2, int count = 0)
    {
        // 65536 segments on one curve better be enough!
        if (count > 16)
            return;

        Vector2 midPoint = (p1 + 2 * pc + p2) / 4;
        Vector2 directDrawnLine = (p1 + p2) / 2 - midPoint;

        if (directDrawnLine.X * directDrawnLine.X + directDrawnLine.Y * directDrawnLine.Y > flatness2)
        {
            // half-pixel error allowed... need to be smaller if AA
            GeneratePointsForQuadCurve(points, ref offset, p1, (p1 + pc) / 2f, midPoint, flatness2, count + 1);
            GeneratePointsForQuadCurve(points, ref offset, midPoint, (p1 + p2) / 2f, p2, flatness2, count + 1);
        }
        else
        {
            if (points != null) points[offset] = p2;
            offset++;
        }
    }
}