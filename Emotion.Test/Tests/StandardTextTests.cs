#region Using

using System.IO;
using System.Numerics;
using Emotion.Standard.Image;
using Emotion.Standard.Text;
using Emotion.Test.Results;
using StbTrueTypeSharp;

#endregion

namespace Emotion.Test.Tests
{
    [Test("StandardText")]
    public class StandardTextTests
    {
        [Test]
        public void ParseTrueTypeFont()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "1980XX.ttf"));
            var f = new Font(data);
            StbTrueType.stbtt_fontinfo stbFont = LoadFontStb(data);
            CompareMetricsWithStb(f, stbFont);

            Assert.True(f.Valid);
            Assert.True(f.FullName == "1980XX");
            Assert.True(f.UnitsPerEm == 1024);
            Assert.True(f.Descender == -128);
            Assert.True(f.Ascender == 682);
            Assert.True(f.Glyphs.Length == 144);
        }

        [Test]
        public void CompareEmotionAtlasWithStbAtlasTrueType()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "1980XX.ttf"));

            var f = new Font(data);
            FontAtlas emotionAtlas = f.GetAtlas(17);
            FontAtlas stbAtlas = f.GetAtlas(17, rasterizer: Font.GlyphRasterizer.StbTrueType);

            Assert.True(emotionAtlas != null);
            if (emotionAtlas == null) return;
            Assert.True(stbAtlas != null);

            byte[] emotionAtlasRgba = ImageUtil.AToRgba(emotionAtlas.Pixels);

            Runner.VerifyImages("CompareEmotionAtlasWithStbAtlasTrueType",
                emotionAtlasRgba,
                ImageUtil.AToRgba(stbAtlas?.Pixels), stbAtlas?.Size ?? Vector2.Zero);

            ImageUtil.FlipImageY(emotionAtlasRgba, (int) emotionAtlas.Size.X, (int) emotionAtlas.Size.Y);
            Runner.VerifyCachedRender(ResultDb.EmotionTTAtlas, emotionAtlasRgba, emotionAtlas.Size);
        }

        [Test]
        public void ParseCffFont()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "Junction-Bold.otf"));
            var f = new Font(data);
            StbTrueType.stbtt_fontinfo stbFont = LoadFontStb(data);
            CompareMetricsWithStb(f, stbFont);

            Assert.True(f.Valid);
            Assert.True(f.FullName == "Junction-Bold");
            Assert.True(f.UnitsPerEm == 1000);
            Assert.True(f.Descender == -250);
            Assert.True(f.Ascender == 750);
            Assert.True(f.Glyphs.Length == 270);
        }

        [Test]
        public void CompareEmotionAtlasWithStbAtlasCff()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "Junction-Bold.otf"));

            var f = new Font(data);
            FontAtlas emotionAtlas = f.GetAtlas(17);
            FontAtlas stbAtlas = f.GetAtlas(17, rasterizer: Font.GlyphRasterizer.StbTrueType);

            Assert.True(emotionAtlas != null);
            if (emotionAtlas == null) return;
            Assert.True(stbAtlas != null);

            byte[] emotionAtlasRgba = ImageUtil.AToRgba(emotionAtlas.Pixels);

            Runner.VerifyImages("CompareEmotionAtlasWithStbAtlasCff",
                emotionAtlasRgba,
                ImageUtil.AToRgba(stbAtlas?.Pixels), stbAtlas?.Size ?? Vector2.Zero);

            ImageUtil.FlipImageY(emotionAtlasRgba, (int) emotionAtlas.Size.X, (int) emotionAtlas.Size.Y);
            Runner.VerifyCachedRender(ResultDb.EmotionCffAtlas, emotionAtlasRgba, emotionAtlas.Size);
        }

        [Test]
        public void ParseCompositeTtFont()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "LatoWeb-Regular.ttf"));
            var f = new Font(data);

            StbTrueType.stbtt_fontinfo stbFont = LoadFontStb(data);
            CompareMetricsWithStb(f, stbFont);

            Assert.True(f.Valid);
            Assert.True(f.FullName == "Lato Regular");
            Assert.True(f.UnitsPerEm == 2000);
            Assert.True(f.Descender == -426);
            Assert.True(f.Ascender == 1974);
            Assert.True(f.Glyphs.Length == 3023);
        }

        [Test]
        public void CompareEmotionAtlasWithStbAtlasComposite()
        {
            byte[] data = File.ReadAllBytes(Path.Combine("Assets", "Fonts", "LatoWeb-Regular.ttf"));

            var f = new Font(data);
            FontAtlas emotionAtlas = f.GetAtlas(17, 0, 1000);
            FontAtlas stbAtlas = f.GetAtlas(17, 0, 1000, Font.GlyphRasterizer.StbTrueType);

            Assert.True(emotionAtlas != null);
            if (emotionAtlas == null) return;
            Assert.True(stbAtlas != null);

            byte[] emotionAtlasRgba = ImageUtil.AToRgba(emotionAtlas.Pixels);

            Runner.VerifyImages("CompareEmotionAtlasWithStbAtlasComposite",
                emotionAtlasRgba,
                ImageUtil.AToRgba(stbAtlas?.Pixels), stbAtlas?.Size ?? Vector2.Zero);

            ImageUtil.FlipImageY(emotionAtlasRgba, (int) emotionAtlas.Size.X, (int) emotionAtlas.Size.Y);
            Runner.VerifyCachedRender(ResultDb.EmotionCompositeAtlas, emotionAtlasRgba, emotionAtlas.Size);
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

        public static unsafe void CompareMetricsWithStb(Font f, StbTrueType.stbtt_fontinfo stbFont)
        {
            foreach (Glyph glyph in f.Glyphs)
            {
                if (glyph.CharIndex == (char) 0) continue;
                var advance = 0;
                var bearing = 0;
                StbTrueType.stbtt_GetCodepointHMetrics(stbFont, (int) glyph.CharIndex, &advance, &bearing);
                Assert.True(advance == glyph.AdvanceWidth);
                Assert.True(bearing == glyph.LeftSideBearing);

                var minX = 0;
                var maxX = 0;
                var minY = 0;
                var maxY = 0;
                StbTrueType.stbtt_GetCodepointBitmapBoxSubpixel(stbFont, (int) glyph.CharIndex, 1, 1, 0, 0, &minX, &minY, &maxX, &maxY);

                // The Y is inverted in STB so we invert the ymin and ymax when verifying.
                Assert.True(minX == glyph.XMin);
                Assert.True(maxX == glyph.XMax);
                Assert.True(minY == -glyph.YMax);
                Assert.True(maxY == -glyph.YMin);
            }
        }

        #endregion
    }
}