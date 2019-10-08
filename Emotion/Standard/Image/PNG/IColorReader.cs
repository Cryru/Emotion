#region Using

using System;

#endregion

namespace Emotion.Standard.Image.PNG
{
    /// <summary>
    /// Interface for color readers, which are responsible for reading
    /// different color formats from a png file.
    /// </summary>
    internal interface IColorReader
    {
        /// <summary>
        /// Reads the specified scanline.
        /// </summary>
        /// <param name="scanline">The scanline.</param>
        /// <param name="pixels">The pixels, where the colors should be stored in RGBA format.</param>
        /// <param name="header">
        /// The header, which contains information about the png file, like
        /// the width of the image and the height.
        /// </param>
        /// <param name="row">The row in the pixels where the provided scanline should be read to.</param>
        void ReadScanline(Span<byte> scanline, byte[] pixels, PngFileHeader header, int row);

        /// <summary>
        /// Reads the specified interlaced scanline.
        /// </summary>
        /// <param name="scanline">The defiltered interlaced scanline.</param>
        /// <param name="rowPixels">The pixels of the row, where the colors should be stored in RGBA format.</param>
        /// <param name="header">
        /// The header, which contains information about the png file, like
        /// the width of the image and the height.
        /// </param>
        /// <param name="row">The row in the pixels where the provided scanline should be read to.</param>
        /// <param name="pass">The Adam7 pass index (0-6).</param>
        void ReadScanlineInterlaced(Span<byte> scanline, byte[] rowPixels, PngFileHeader header, int row, int pass);
    }
}