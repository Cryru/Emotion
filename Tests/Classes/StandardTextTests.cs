#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.Graphics.Text.StbRenderer;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.OpenType;
using Emotion.Test;
using Emotion.Test.Helpers;
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
                2164,
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

            FrameBuffer b = null;
            for (var i = 0; i < fonts.Length; i++)
            {
                Engine.Log.Info($"Running font {fonts[i]} ({i})...", TestRunnerLogger.TestRunnerSrc);
                ReadOnlyMemory<byte> data = Engine.AssetLoader.Get<OtherAsset>($"Fonts/{fonts[i]}")?.Content ?? ReadOnlyMemory<byte>.Empty;
                var f = new Font(data);

                // Verify basic font data.
                Assert.True(f.Valid);
                Assert.True(f.FullName == names[i]);
                Assert.True(f.UnitsPerEm == unitsPerEm[i]);
                Assert.True(f.Descender == descender[i]);
                Assert.True(f.Ascender == ascender[i]);
                Assert.True(f.CharToGlyph.Count == glyphs[i]);

                // Get atlases.
                int fontSize = fontSizes[i];
                var emotionAtlas = new StbDrawableFontAtlas(f, fontSize);
                Runner.ExecuteAsLoop(_ =>
                {
                    var str = "";
                    for (uint j = emotionAtlas.Font.FirstCharIndex; j < emotionAtlas.Font.LastCharIndex; j++)
                    {
                        str += (char) j;
                    }

                    emotionAtlas.CacheGlyphs(str);
                }).WaitOne();
                DrawableFontAtlas packedStbAtlas = RenderFontStbPacked(data.ToArray(), fontSize, emotionAtlas.Texture.Size * 3, (int) f.LastCharIndex + 1, f,
                    out StbTrueType.StbTrueType.stbtt_fontinfo stbFont);

                // Compare glyph parsing.
                CompareMetricsWithStb(f, emotionAtlas, stbFont);

                // Compare render metrics.
                foreach (KeyValuePair<char, DrawableGlyph> g in emotionAtlas.Glyphs)
                {
                    DrawableGlyph glyph = packedStbAtlas.Glyphs[g.Key];
                    Assert.Equal(glyph.XAdvance, g.Value.XAdvance);

                    var fontGlyph = g.Value.FontGlyph;
                    int width = (int) (MathF.Ceiling(fontGlyph.Max.X * emotionAtlas.RenderScale) - MathF.Floor(fontGlyph.Min.X * emotionAtlas.RenderScale));
                    int height = (int) (MathF.Ceiling(-fontGlyph.Min.Y * emotionAtlas.RenderScale) - MathF.Floor(-fontGlyph.Max.Y * emotionAtlas.RenderScale));

                    Assert.Equal(glyph.Width, width);
                    Assert.Equal(glyph.Height, height);
                }

                // Check if there's a verified render.
                if (string.IsNullOrEmpty(cachedRender[i])) continue;

                // Compare with cached render.
                // ReSharper disable AccessToModifiedClosure

                Runner.ExecuteAsLoop(_ =>
                {
                    if (b == null)
                        b = new FrameBuffer(emotionAtlas.Texture.Size).WithColor();
                    else
                        b.Resize(emotionAtlas.Texture.Size, true);

                    RenderComposer composer = Engine.Renderer.StartFrame();
                    composer.RenderToAndClear(b);
                    composer.RenderSprite(Vector3.Zero, emotionAtlas.Texture.Size, Color.White, emotionAtlas.Texture);
                    composer.RenderTo(null);
                    Engine.Renderer.EndFrame();
                    Runner.VerifyScreenshot(cachedRender[i], b);
                }).WaitOne();
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

        public static unsafe DrawableFontAtlas RenderFontStbPacked(byte[] ttf, float fontSize, Vector2 atlasSize, int numChars, Font f, out StbTrueType.StbTrueType.stbtt_fontinfo fontInfo)
        {
            var atlasObj = new DrawableFontAtlas(f, fontSize);
            fontSize = atlasObj.FontSize;

            fontInfo = new StbTrueType.StbTrueType.stbtt_fontinfo();
            fixed (byte* ttPtr = &ttf[0])
            {
                StbTrueType.StbTrueType.stbtt_InitFont(fontInfo, ttPtr, 0);

                float scaleFactor = StbTrueType.StbTrueType.stbtt_ScaleForMappingEmToPixels(fontInfo, fontSize);
                int ascent, descent, lineGap;
                StbTrueType.StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

                atlasSize *= 3; // Needs to be big as the packing sucks, and glyphs getting cut out messes with the tests.
                var pixels = new byte[(int) atlasSize.X * (int) atlasSize.Y];

                var pc = new StbTrueType.StbTrueType.stbtt_pack_context();
                fixed (byte* pixelsPtr = pixels)
                {
                    StbTrueType.StbTrueType.stbtt_PackBegin(pc, pixelsPtr, (int) atlasSize.X, (int) atlasSize.Y, (int) atlasSize.X, 1, null);
                }

                var cd = new StbTrueType.StbTrueType.stbtt_packedchar[numChars];
                fixed (StbTrueType.StbTrueType.stbtt_packedchar* charPtr = cd)
                {
                    StbTrueType.StbTrueType.stbtt_PackFontRange(pc, ttPtr, 0, -fontSize, 0, numChars, charPtr);
                }

                StbTrueType.StbTrueType.stbtt_PackEnd(pc);
                for (var i = 0; i < cd.Length; ++i)
                {
                    var atlasGlyph = DrawableGlyph.CreateForTest(cd[i].xadvance, cd[i].xoff, cd[i].x1 - cd[i].x0, cd[i].y1 - cd[i].y0);
                    atlasGlyph.GlyphUV = new Rectangle(cd[i].x0, cd[i].y0, atlasGlyph.Width, atlasGlyph.Height);
                    atlasObj.Glyphs[(char) i] = atlasGlyph;
                }
            }

            return atlasObj;
        }

        public static StbTrueType.StbTrueType.stbtt_fontinfo LoadFontStb(byte[] bytes)
        {
            var font = new StbTrueType.StbTrueType.stbtt_fontinfo();

            unsafe
            {
                fixed (byte* fontDataPtr = &bytes[0])
                {
                    StbTrueType.StbTrueType.stbtt_InitFont(font, fontDataPtr, 0);
                }
            }

            return font;
        }

        public static unsafe void CompareMetricsWithStb(Font f, DrawableFontAtlas atlas, StbTrueType.StbTrueType.stbtt_fontinfo stbFont)
        {
            var pc = new StbTrueType.StbTrueType.stbtt_pack_context();
            StbTrueType.StbTrueType.stbtt_PackBegin(pc, (byte*) 0, 512, 512, 512, 1, null);

            var cd = new StbTrueType.StbTrueType.stbtt_packedchar[f.LastCharIndex + 1];
            StbTrueType.StbTrueType.stbrp_rect[] rects;
            fixed (StbTrueType.StbTrueType.stbtt_packedchar* charDataPtr = &cd[0])
            {
                var range = new StbTrueType.StbTrueType.stbtt_pack_range
                {
                    first_unicode_codepoint_in_range = 0,
                    array_of_unicode_codepoints = null,
                    num_chars = (int) f.LastCharIndex + 1,
                    chardata_for_range = charDataPtr,
                    font_size = -atlas.FontSize
                };

                rects = new StbTrueType.StbTrueType.stbrp_rect[f.LastCharIndex + 1];
                fixed (StbTrueType.StbTrueType.stbrp_rect* rectPtr = &rects[0])
                {
                    int n = StbTrueType.StbTrueType.stbtt_PackFontRangesGatherRects(pc, stbFont, &range, 1, rectPtr);
                    StbTrueType.StbTrueType.stbtt_PackFontRangesPackRects(pc, rectPtr, n);
                }
            }

            foreach ((char charIndex, DrawableGlyph atlasGlyph) in atlas.Glyphs)
            {
                FontGlyph glyph = atlasGlyph.FontGlyph;

                var advance = 0;
                var bearing = 0;
                StbTrueType.StbTrueType.stbtt_GetCodepointHMetrics(stbFont, charIndex, &advance, &bearing);
                Assert.True(advance == glyph.AdvanceWidth);
                Assert.True(bearing == glyph.LeftSideBearing || glyph.LeftSideBearing == 0); // stb has junk data beyond valid

                var minX = 0;
                var maxX = 0;
                var minY = 0;
                var maxY = 0;
                StbTrueType.StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(stbFont, charIndex, atlas.RenderScale, atlas.RenderScale, 0, 0, &minX, &minY, &maxX, &maxY);

                Rectangle bbox = glyph.GetBBox(atlas.RenderScale);

                Assert.Equal(minX, bbox.X);
                Assert.Equal(minY, bbox.Y);
                Assert.Equal(maxX, bbox.Width);
                Assert.Equal(maxY, bbox.Height);

                Rectangle drawBox = new Rectangle(0, 0, bbox.Width - bbox.X, bbox.Height - bbox.Y);
                drawBox.Size += Vector2.One; // Add padding from stb
                StbTrueType.StbTrueType.stbrp_rect rect = rects[charIndex];
                Assert.Equal(rect.w, drawBox.Width);
                Assert.Equal(rect.h, drawBox.Height);
            }
        }

        #endregion
    }
}