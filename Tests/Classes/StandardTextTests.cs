#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Primitives;
using Emotion.Standard.OpenType;
using Emotion.Test;
using Emotion.Utility;
using StbTrueTypeSharp;
using Tests.Results;

#endregion

namespace Tests.Classes
{
    [Test("StandardText", true)]
    public class StandardTextTests
    {
        [Test]
        public void VerifyFontRendering()
        {
            var fonts = new[]
            {
                "Junction-Bold.otf", // Cff
                "CaslonOS.otf", // Cff 2 (covers other cases)
                "1980XX.ttf", // Ttf
                "LatoWeb-Regular.ttf", // Composite
                "Junction-Bold.otf", // 14 font size
                "Junction-Bold.otf" // 11 font size
            };

            var names = new[]
            {
                "Junction-Bold",
                "CaslonOS-Regular",
                "1980XX",
                "Lato Regular",
                "Junction-Bold",
                "Junction-Bold"
            };

            var unitsPerEm = new[]
            {
                1000,
                1000,
                1024,
                2000,
                1000,
                1000
            };

            int[] descender =
            {
                -250,
                -360,
                -128,
                -426,
                -250,
                -250
            };

            var ascender = new[]
            {
                750,
                840,
                682,
                1974,
                750,
                750
            };

            var glyphs = new[]
            {
                270,
                279,
                141,
                2165, // 2164, but Emotion adds a fake-space char.
                270,
                270
            };

            string[] cachedRender =
            {
                ResultDb.EmotionCffAtlas,
                "",
                ResultDb.EmotionTtAtlas,
                ResultDb.EmotionCompositeAtlas,
                "",
                ""
            };

            int[] fontSizes =
            {
                17,
                17,
                17,
                17,
                14,
                11
            };

            for (var i = 0; i < fonts.Length; i++)
            {
                byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", fonts[i]));
                var f = new Font(data);

                // Verify basic font data.
                Assert.True(f.Valid);
                Assert.True(f.FullName == names[i]);
                Assert.True(f.UnitsPerEm == unitsPerEm[i]);
                Assert.True(f.Descender == descender[i]);
                Assert.True(f.Ascender == ascender[i]);
                Assert.True(f.Glyphs.Length == glyphs[i]);

                // Get atlases.
                int fontSize = fontSizes[i];
                FontAtlas emotionAtlas = f.GetAtlas(fontSize);
                FontAtlas packedStbAtlas = RenderFontStbPacked(data, fontSize, emotionAtlas.Size * 2, (int) f.LastCharIndex + 1, f, out StbTrueType.stbtt_fontinfo stbFont);

                // Compare glyph parsing.
                CompareMetricsWithStb(f, stbFont, emotionAtlas.Scale);

                // Compare render metrics.
                foreach (KeyValuePair<char, AtlasGlyph> g in emotionAtlas.Glyphs)
                {
                    AtlasGlyph glyph = packedStbAtlas.Glyphs[g.Key];
                    Assert.Equal(glyph.Advance, g.Value.Advance);
                    Assert.Equal(glyph.XMin, g.Value.XMin);
                    Assert.Equal(glyph.Size, g.Value.Size);
                    Assert.Equal(glyph.YBearing, g.Value.YBearing);
                }

                // Check if there's a verified render.
                if (string.IsNullOrEmpty(cachedRender[i])) continue;

                // Compare with cached render.
                byte[] emotionAtlasRgba = ImageUtil.AToRgba(emotionAtlas.Pixels);
                ImageUtil.FlipImageY(emotionAtlasRgba, (int) emotionAtlas.Size.Y);
                Runner.VerifyCachedRender(cachedRender[i], emotionAtlasRgba, emotionAtlas.Size);
            }
        }

        [Test]
        public void ParseCompositeWoff()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "open-iconic.woff"));
            var f = new Font(data);

            // Not implemented yet.
            Assert.False(f.Valid);
        }

        #region Helpers

        public static unsafe FontAtlas RenderFontStbPacked(byte[] ttf, float fontSize, Vector2 atlasSize, int numChars, Font f, out StbTrueType.stbtt_fontinfo fontInfo)
        {
            FontAtlas atlasObj;

            fontInfo = new StbTrueType.stbtt_fontinfo();
            fixed (byte* ttPtr = &ttf[0])
            {
                StbTrueType.stbtt_InitFont(fontInfo, ttPtr, 0);

                float scaleFactor = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, fontSize);
                int ascent, descent, lineGap;
                StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

                atlasSize *= 3; // Needs to be big as the packing sucks, and glyphs getting cut out messes with the tests.
                var pixels = new byte[(int) atlasSize.X * (int) atlasSize.Y];

                var pc = new StbTrueType.stbtt_pack_context();
                fixed (byte* pixelsPtr = pixels)
                {
                    StbTrueType.stbtt_PackBegin(pc, pixelsPtr, (int) atlasSize.X, (int) atlasSize.Y, (int) atlasSize.X, 1, null);
                }

                var cd = new StbTrueType.stbtt_packedchar[numChars];
                fixed (StbTrueType.stbtt_packedchar* charPtr = cd)
                {
                    StbTrueType.stbtt_PackFontRange(pc, ttPtr, 0, fontSize, 0, numChars, charPtr);
                }

                StbTrueType.stbtt_PackEnd(pc);
                atlasObj = new FontAtlas(atlasSize, pixels, "test", fontSize, f);
                for (var i = 0; i < cd.Length; ++i)
                {
                    float yOff = cd[i].yoff;
                    yOff += MathF.Ceiling(ascent * scaleFactor);
                    var atlasGlyph = new AtlasGlyph((char) i, (int) MathF.Round(cd[i].xadvance), (int) cd[i].xoff, 10)
                    {
                        Location = new Vector2(cd[i].x0, cd[i].y0),
                        Size = new Vector2(cd[i].x1 - cd[i].x0, cd[i].y1 - cd[i].y0),
                        YBearing = (int) MathF.Round(yOff) // overwrite
                    };
                    atlasObj.Glyphs[(char) i] = atlasGlyph;
                }
            }

            return atlasObj;
        }

        public static StbTrueType.stbtt_fontinfo LoadFontStb(byte[] bytes)
        {
            var font = new StbTrueType.stbtt_fontinfo();

            unsafe
            {
                fixed (byte* fontDataPtr = &bytes[0])
                {
                    StbTrueType.stbtt_InitFont(font, fontDataPtr, 0);
                }
            }

            return font;
        }

        public static unsafe void CompareMetricsWithStb(Font f, StbTrueType.stbtt_fontinfo stbFont, float scale)
        {
            var pc = new StbTrueType.stbtt_pack_context();
            StbTrueType.stbtt_PackBegin(pc, (byte*) 0, 512, 512, 512, 1, null);

            var cd = new StbTrueType.stbtt_packedchar[f.LastCharIndex + 1];
            StbTrueType.stbrp_rect[] rects;
            fixed (StbTrueType.stbtt_packedchar* charDataPtr = &cd[0])
            {
                var range = new StbTrueType.stbtt_pack_range
                {
                    first_unicode_codepoint_in_range = 0,
                    array_of_unicode_codepoints = null,
                    num_chars = (int) f.LastCharIndex + 1,
                    chardata_for_range = charDataPtr,
                    font_size = scale * f.Height
                };

                rects = new StbTrueType.stbrp_rect[f.LastCharIndex + 1];
                fixed (StbTrueType.stbrp_rect* rectPtr = &rects[0])
                {
                    int n = StbTrueType.stbtt_PackFontRangesGatherRects(pc, stbFont, &range, 1, rectPtr);
                    StbTrueType.stbtt_PackFontRangesPackRects(pc, rectPtr, n);
                }
            }

            foreach (Glyph glyph in f.Glyphs)
            {
                if (glyph.CharIndex == (char) 0) continue;
                var advance = 0;
                var bearing = 0;
                StbTrueType.stbtt_GetCodepointHMetrics(stbFont, (int) glyph.CharIndex, &advance, &bearing);
                Assert.True(advance == glyph.AdvanceWidth);
                Assert.True(bearing == glyph.LeftSideBearing || glyph.LeftSideBearing == 0); // stb has junk data beyond valid

                var minX = 0;
                var maxX = 0;
                var minY = 0;
                var maxY = 0;
                StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(stbFont, (int) glyph.CharIndex, scale, scale, 0, 0, &minX, &minY, &maxX, &maxY);

                Rectangle bbox = glyph.GetBBox(scale);

                Assert.Equal(minX, bbox.X);
                Assert.Equal(minY, bbox.Y);
                Assert.Equal(maxX, bbox.Width);
                Assert.Equal(maxY, bbox.Height);

                Rectangle drawBox = glyph.GetDrawBox(scale);
                drawBox.Size += Vector2.One; // Add padding from stb
                StbTrueType.stbrp_rect rect = rects[glyph.CharIndex];
                Assert.Equal(rect.w, drawBox.Width);
                Assert.Equal(rect.h, drawBox.Height);

                StbTrueType.stbtt_vertex* vertices;
                int verticesNum = StbTrueType.stbtt_GetGlyphShape(stbFont, (int) glyph.MapIndex, &vertices);

                Assert.Equal(verticesNum, glyph.Vertices.Length);
                for (var i = 0; i < glyph.Vertices.Length; i++)
                {
                    GlyphVertex vertex = glyph.Vertices[i];
                    StbTrueType.stbtt_vertex stbVertex = vertices[i];
                    Assert.Equal(stbVertex.x, vertex.X);
                    Assert.Equal(stbVertex.y, vertex.Y);
                    Assert.Equal(stbVertex.cx, vertex.Cx);
                    Assert.Equal(stbVertex.cy, vertex.Cy);
                    Assert.Equal(stbVertex.type, vertex.Flags);
                }
            }
        }

        #endregion
    }
}