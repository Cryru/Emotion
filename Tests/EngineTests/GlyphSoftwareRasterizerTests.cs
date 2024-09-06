#nullable enable

#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Text.RasterizationNew;
using Emotion.IO;
using Emotion.Standard.OpenType;
using Emotion.Testing;
using Emotion.Utility;
using static StbTrueTypeSharp.StbTrueType;

#endregion

namespace Tests.EngineTests;

public class GlyphSoftwareRasterizerTests : TestingScene
{
    private List<IntPtr> _pinnedFonts = new();

    public override IEnumerator UnloadSceneRoutineAsync()
    {
        for (var i = 0; i < _pinnedFonts.Count; i++)
        {
            UnmanagedMemoryAllocator.Free(_pinnedFonts[i]);
        }

        _pinnedFonts.Clear();
        return base.UnloadSceneRoutineAsync();
    }

    protected override void TestUpdate()
    {
    }

    protected override void TestDraw(RenderComposer c)
    {
    }

    public override Func<IEnumerator>[] GetTestCoroutines()
    {
        return new[]
        {
            CompareMetricsOfBuiltInFont
        };
    }

    private unsafe stbtt_fontinfo GetStbFont(FontAsset emotionFont)
    {
        var byteAsset = Engine.AssetLoader.Get<OtherAsset>(emotionFont.Name, false);
        ReadOnlyMemory<byte> fontBytes = byteAsset!.Content;
        IntPtr ptr = UnmanagedMemoryAllocator.MemAlloc(fontBytes.Length);
        fontBytes.ToArray().CopyTo(new Span<byte>((void*) ptr, fontBytes.Length));
        _pinnedFonts.Add(ptr);

        var fontInfo = new stbtt_fontinfo();
        stbtt_InitFont(fontInfo, (byte*) ptr, 0);

        return fontInfo;
    }

    private IEnumerator CompareMetricsOfBuiltInFont()
    {
        FontAsset? builtIn = FontAsset.GetDefaultBuiltIn();

        stbtt_fontinfo fontInfo = GetStbFont(builtIn);

        int glyphIndex = stbtt_FindGlyphIndex(fontInfo, 'T');
        VerifyGlyphVertices(fontInfo, glyphIndex);
        VerifyCurves(fontInfo, glyphIndex, 28);

        yield break;
    }

    private unsafe void VerifyGlyphVertices(stbtt_fontinfo info, int glyph)
    {
        // Get Emotion metrics
        GlyphVertex[] glyphVertices = SoftwareGlyphRasterizer.GetGlyphVertices(info, glyph);

        // TEMP
        for (var i = 0; i < glyphVertices.Length; i++)
        {
            GlyphVertex vertex = glyphVertices[i];
            if (vertex.TypeFlag == VertexTypeFlag.Cubic) return;
        }

        // Get STB metrics
        stbtt_vertex* vertices;
        int numVerts = stbtt_GetGlyphShape(info, glyph, &vertices);

        // Remove cubic curves from stb shapes. Emotion converts them when parsing the font.

        // Compare
        Assert.Equal(numVerts, glyphVertices.Length);

        for (var i = 0; i < numVerts; i++)
        {
            GlyphVertex vertexEmotion = glyphVertices[i];
            stbtt_vertex vertexStb = vertices[i];

            Assert.Equal(vertexStb.x, vertexEmotion.X);
            Assert.Equal(vertexStb.y, vertexEmotion.Y);
            Assert.Equal(vertexStb.cx, vertexEmotion.Cx);
            Assert.Equal(vertexStb.cy, vertexEmotion.Cy);
            Assert.Equal(vertexStb.cx1, vertexEmotion.Cx1);
            Assert.Equal(vertexStb.cy1, vertexEmotion.Cy1);
            Assert.Equal(vertexStb.type, vertexEmotion.Flags);
        }
    }

    private unsafe void VerifyCurves(stbtt_fontinfo info, int glyph, int fontSize)
    {
        // ReSharper disable once InconsistentNaming
        const float FLATNESS_IN_PIXELS = SoftwareGlyphRasterizer.FLATNESS_IN_PIXELS;

        int ascent, descent, lineGap;
        stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        float scaleX = fontSize / (float) ascent;
        float scaleY = scaleX;

        float scale = scaleX > scaleY ? scaleY : scaleX;

        // Get Emotion metrics
        float flatness = FLATNESS_IN_PIXELS / scale;
        GlyphVertex[] glyphVertices = SoftwareGlyphRasterizer.GetGlyphVertices(info, glyph);
        SoftwareGlyphRasterizer.VerticesToContours(glyphVertices, flatness, out Vector2[]? points, out int[]? contourLengths);

        Assert.NotNull(points);
        Assert.NotNull(contourLengths);

        // Get STB metrics
        stbtt_vertex* vertices;
        int numVerts = stbtt_GetGlyphShape(info, glyph, &vertices);

        var windingCount = 0;
        int* windingLengths = null;
        stbtt__point* windings = stbtt_FlattenCurves(vertices, numVerts, FLATNESS_IN_PIXELS / scale, &windingLengths, &windingCount, (void*) 0);

        Assert.True(windingLengths != null);

        // Compare
        var totalWindings = 0;
        for (var i = 0; i < windingCount; i++)
        {
            totalWindings += windingLengths![i];
        }

        Assert.Equal(totalWindings, points!.Length);
        Assert.Equal(windingCount, contourLengths!.Length);

        for (var i = 0; i < points.Length; i++)
        {
            Vector2 point = points[i];
            stbtt__point winding = windings[i];

            Assert.Equal(winding.x, point.X);
            Assert.Equal(winding.y, point.Y);
        }
    }
}