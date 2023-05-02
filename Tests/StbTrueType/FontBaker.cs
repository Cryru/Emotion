#region Using

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Tests.StbTrueType
{
#if !STBSHARP_INTERNAL
    public
#else
	internal
#endif
        unsafe class FontBaker
    {
        private byte[] _bitmap;
        private StbTrueType.stbtt_pack_context _context;
        private Dictionary<int, GlyphInfo> _glyphs;
        private int bitmapWidth, bitmapHeight;

        public void Begin(int width, int height)
        {
            bitmapWidth = width;
            bitmapHeight = height;
            _bitmap = new byte[width * height];
            _context = new StbTrueType.stbtt_pack_context();

            fixed (byte* pixelsPtr = _bitmap)
            {
                StbTrueType.stbtt_PackBegin(_context, pixelsPtr, width, height, width, 1, null);
            }

            _glyphs = new Dictionary<int, GlyphInfo>();
        }

        public void Add(byte[] ttf, float fontPixelHeight,
            IEnumerable<CharacterRange> characterRanges)
        {
            if (ttf == null || ttf.Length == 0)
                throw new ArgumentNullException(nameof(ttf));

            if (fontPixelHeight <= 0)
                throw new ArgumentOutOfRangeException(nameof(fontPixelHeight));

            if (characterRanges == null)
                throw new ArgumentNullException(nameof(characterRanges));

            if (!characterRanges.Any())
                throw new ArgumentException("characterRanges must have a least one value.");

            fixed (byte* ttfPtr = ttf)
            {
                var fontInfo = new StbTrueType.stbtt_fontinfo();
                if (StbTrueType.stbtt_InitFont(fontInfo, ttfPtr, 0) == 0)
                    throw new Exception("Failed to init font.");

                var scaleFactor = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, fontPixelHeight);

                int ascent, descent, lineGap;
                StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

                foreach (var range in characterRanges)
                {
                    if (range.Start > range.End)
                        continue;

                    var cd = new StbTrueType.stbtt_packedchar[range.End - range.Start + 1];
                    fixed (StbTrueType.stbtt_packedchar* chardataPtr = cd)
                    {
                        StbTrueType.stbtt_PackFontRange(_context, ttfPtr, 0, fontPixelHeight,
                            range.Start,
                            range.End - range.Start + 1,
                            chardataPtr);
                    }

                    for (var i = 0; i < cd.Length; ++i)
                    {
                        var yOff = cd[i].yoff;
                        yOff += ascent * scaleFactor;

                        var glyphInfo = new GlyphInfo
                        {
                            X = cd[i].x0,
                            Y = cd[i].y0,
                            Width = cd[i].x1 - cd[i].x0,
                            Height = cd[i].y1 - cd[i].y0,
                            XOffset = (int) cd[i].xoff,
                            YOffset = (int) Math.Round(yOff),
                            XAdvance = (int) Math.Round(cd[i].xadvance)
                        };

                        _glyphs[i + range.Start] = glyphInfo;
                    }
                }
            }
        }

        public FontBakerResult End()
        {
            return new FontBakerResult(_glyphs, _bitmap, bitmapWidth, bitmapHeight);
        }
    }
}