#region Using

using System;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Image.PNG.Readers
{
    /// <summary>
    /// Color reader for reading grayscale colors from a PNG file.
    /// </summary>
    internal sealed class GrayscaleReader : IColorReader
    {
        #region Fields

        private bool _useAlpha;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GrayscaleReader" /> class.
        /// </summary>
        /// <param name="useAlpha">
        /// if set to <c>true</c> the color reader will also read the
        /// alpha channel from the scanline.
        /// </param>
        public GrayscaleReader(bool useAlpha)
        {
            _useAlpha = useAlpha;
        }

        #endregion

        #region IPngFormatHandler Members

        /// <inheritdoc />
        public void ReadScanline(Span<byte> scanline, byte[] pixels, PngFileHeader header, int row)
        {
            Span<byte> newScanline = ImageUtil.ToArrayByBitsLength(scanline, header.BitDepth);

            int offset;
            if (_useAlpha)
                for (var x = 0; x < header.Width / 2; x++)
                {
                    offset = (row * header.Width + x) * 4;

                    pixels[offset + 0] = newScanline[x * 2];
                    pixels[offset + 1] = newScanline[x * 2];
                    pixels[offset + 2] = newScanline[x * 2];
                    pixels[offset + 3] = newScanline[x * 2 + 1];
                }
            else
                for (var x = 0; x < header.Width; x++)
                {
                    offset = (row * header.Width + x) * 4;

                    pixels[offset + 0] = newScanline[x];
                    pixels[offset + 1] = newScanline[x];
                    pixels[offset + 2] = newScanline[x];
                    pixels[offset + 3] = 255;
                }

            row++;
        }

        /// <inheritdoc />
        public void ReadScanlineInterlaced(Span<byte> scanline, byte[] rowPixels, PngFileHeader header, int row, int pass)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}