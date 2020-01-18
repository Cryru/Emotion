#region Using

using System;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Image.PNG.Readers
{
    /// <summary>
    /// Color reader for reading truecolors from a PNG file. Only colors
    /// with 24 or 32 bit (3 or 4 bytes) per pixel are supported at the moment.
    /// </summary>
    internal sealed class TrueColorReader : IColorReader
    {
        #region Fields

        private bool _useAlpha;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TrueColorReader" /> class.
        /// </summary>
        /// <param name="useAlpha">
        /// if set to <c>true</c> the color reader will also read the
        /// alpha channel from the scanline.
        /// </param>
        public TrueColorReader(bool useAlpha)
        {
            _useAlpha = useAlpha;
        }

        #endregion

        #region IPngFormatHandler Members

        /// <inheritdoc />
        public void ReadScanline(Span<byte> scanline, byte[] pixels, PngFileHeader header, int row)
        {
            Span<byte> convertedLine = ImageUtil.ToArrayByBitsLength(scanline, header.BitDepth);

            int offset;
            if (_useAlpha)
            {
                for (var x = 0; x < convertedLine.Length; x += 4)
                {
                    offset = (row * header.Width + (x >> 2)) * 4;

                    pixels[offset + 0] = convertedLine[x + 2];
                    pixels[offset + 1] = convertedLine[x + 1];
                    pixels[offset + 2] = convertedLine[x + 0];
                    pixels[offset + 3] = convertedLine[x + 3];
                }
            }
            else
            {
                if (header.BitDepth == 8)
                    for (var x = 0; x < convertedLine.Length / 3; x++)
                    {
                        offset = (row * header.Width + x) * 4;

                        pixels[offset + 0] = convertedLine[x * 3 + 2];
                        pixels[offset + 1] = convertedLine[x * 3 + 1];
                        pixels[offset + 2] = convertedLine[x * 3 + 0];
                        pixels[offset + 3] = 255;
                    }
                else
                    for (var x = 0; x < convertedLine.Length / 6; x++)
                    {
                        offset = (row * header.Width + x) * 4;

                        pixels[offset + 0] = convertedLine[x * 6 + 4];
                        pixels[offset + 1] = convertedLine[x * 6 + 2];
                        pixels[offset + 2] = convertedLine[x * 6 + 0];
                        pixels[offset + 3] = 255;
                    }
            }
        }

        /// <inheritdoc />
        public void ReadScanlineInterlaced(Span<byte> scanline, byte[] pixels, PngFileHeader header, int row, int pass)
        {
            Span<byte> convertedLine = ImageUtil.ToArrayByBitsLength(scanline, header.BitDepth);

            if (!_useAlpha) return;

            var pixel = 0;
            for (var x = 0; x < convertedLine.Length; x += 4)
            {
                int offset = row * 4 * header.Width + (Adam7.FirstColumn[pass] + pixel * Adam7.ColumnIncrement[pass]) * 4;

                pixels[offset + 0] = convertedLine[x + 2];
                pixels[offset + 1] = convertedLine[x + 1];
                pixels[offset + 2] = convertedLine[x + 0];
                pixels[offset + 3] = convertedLine[x + 3];
                pixel++;
            }
        }

        #endregion
    }
}