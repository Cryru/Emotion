#region Using

using System;

#endregion

namespace Emotion.Standard.Image.PNG.Readers
{
    /// <summary>
    /// A color reader for reading palette indices from the PNG file.
    /// </summary>
    internal sealed class PaletteIndexReader : IColorReader
    {
        #region Fields

        private byte[] _palette;
        private byte[] _paletteAlpha;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteIndexReader" /> class.
        /// </summary>
        /// <param name="palette">
        /// The palette as simple byte array. It will contains 3 values for each
        /// color, which represents the red-, the green- and the blue channel.
        /// </param>
        /// <param name="paletteAlpha">
        /// The alpha palette. Can be null, if the image does not have an
        /// alpha channel and can contain less entries than the number of colors in the palette.
        /// </param>
        public PaletteIndexReader(byte[] palette, byte[] paletteAlpha)
        {
            _palette = palette;

            _paletteAlpha = paletteAlpha;
        }

        #endregion

        #region IColorReader Members

        /// <inheritdoc />
        public void ReadScanline(Span<byte> scanline, byte[] pixels, PngFileHeader header, int row)
        {
            Span<byte> newScanline = ImageUtil.ToArrayByBitsLength(scanline, header.BitDepth);

            int index;
            int offset;
            if (_paletteAlpha != null && _paletteAlpha.Length > 0)
                // If the alpha palette is not null and does one or
                // more entries, this means, that the image contains and alpha
                // channel and we should try to read it.
                for (var i = 0; i < header.Width; i++)
                {
                    index = newScanline[i];

                    offset = (row * header.Width + i) * 4;

                    pixels[offset + 0] = _palette[index * 3 + 2];
                    pixels[offset + 1] = _palette[index * 3 + 1];
                    pixels[offset + 2] = _palette[index * 3 + 0];
                    pixels[offset + 3] = _paletteAlpha.Length > index ? _paletteAlpha[index] : (byte) 255;
                }
            else
                for (var i = 0; i < header.Width; i++)
                {
                    index = newScanline[i];

                    offset = (row * header.Width + i) * 4;

                    pixels[offset + 0] = _palette[index * 3 + 2];
                    pixels[offset + 1] = _palette[index * 3 + 1];
                    pixels[offset + 2] = _palette[index * 3 + 0];
                    pixels[offset + 3] = 255;
                }
        }

        /// <inheritdoc />
        public void ReadScanlineInterlaced(Span<byte> scanline, byte[] rowPixels, PngFileHeader header, int row, int pass)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}