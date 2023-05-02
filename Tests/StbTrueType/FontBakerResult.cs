#region Using

using System;
using System.Collections.Generic;

#endregion

namespace Tests.StbTrueType
{
#if !STBSHARP_INTERNAL
    public
#else
	internal
#endif
        class FontBakerResult
    {
        public FontBakerResult(Dictionary<int, GlyphInfo> glyphs, byte[] bitmap, int width, int height)
        {
            if (glyphs == null)
                throw new ArgumentNullException(nameof(glyphs));

            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            if (bitmap.Length < width * height)
                throw new ArgumentException("pixels.Length should be higher than width * height");

            Glyphs = glyphs;
            Bitmap = bitmap;
            Width = width;
            Height = height;
        }

        public Dictionary<int, GlyphInfo> Glyphs { get; }

        public byte[] Bitmap { get; }

        public int Width { get; }

        public int Height { get; }
    }
}