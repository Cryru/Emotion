﻿#region Using

using System.Numerics;
using OpenGL;

#endregion

namespace Emotion.Standard.Image.PNG
{
    public class PngFileHeader
    {
        /// <summary>
        /// The bitmap width in pixels (signed integer).
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// Bit depth is a single-byte integer giving the number of bits per sample
        /// or per palette index (not per pixel). Valid values are 1, 2, 4, 8, and 16,
        /// although not all values are allowed for all color types.
        /// </summary>
        public byte BitDepth;

        /// <summary>
        /// Color type is a integer that describes the interpretation of the
        /// image data. Color type codes represent sums of the following values:
        /// 1 (palette used), 2 (color used), and 4 (alpha channel used).
        /// </summary>
        public byte ColorType;

        /// <summary>
        /// Indicates the method  used to compress the image data. At present,
        /// only compression method 0 (deflate/inflate compression with a sliding
        /// window of at most 32768 bytes) is defined.
        /// </summary>
        public byte CompressionMethod;

        /// <summary>
        /// Indicates the preprocessing method applied to the image
        /// data before compression. At present, only filter method 0
        /// (adaptive filtering with five basic filter types) is defined.
        /// </summary>
        public byte FilterMethod;

        /// <summary>
        /// Indicates the transmission order of the image data.
        /// Two values are currently defined: 0 (no interlace) or 1 (Adam7 interlace).
        /// </summary>
        public byte InterlaceMethod;

        /// <summary>
        /// The byte format of the pixels.
        /// </summary>
        public PixelFormat PixelFormat;
    }
}