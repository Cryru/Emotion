#region Using

using System;
using System.IO;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Image.BMP
{
    public static class BmpFormat
    {
        /// <summary>
        /// The size of the magic for recognizing this file.
        /// </summary>
        public const int FILE_MAGIC_SIZE = 2;

        /// <summary>
        /// Verify whether the bytes are a bmp file, by checking the magic.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns>Whether the data fits the bmp magic.</returns>
        public static bool IsBmp(ReadOnlyMemory<byte> data)
        {
            ReadOnlySpan<byte> span = data.Span;
            var bmp = false;
            if (data.Length >= 2)
                bmp =
                    span[0] == 0x42 && // B
                    span[1] == 0x4D; // M

            return bmp;
        }

        /// <summary>
        /// Encodes the provided BGRA pixel data to a 24bit BMP image.
        /// </summary>
        /// <param name="pixelData">The date to encode.</param>
        /// <param name="width">The width of the image in the data.</param>
        /// <param name="height">The height of the image in the data.</param>
        /// <returns>A 24bit BMP image as bytes.</returns>
        public static byte[] Encode(byte[] pixelData, int width, int height)
        {
            int rowWidth = width;
            int amount = width * 3 % 4;
            if (amount != 0) rowWidth += 4 - amount;

            int fileSize = BmpFileHeader.DEFAULT_OFFSET + height * rowWidth * 3;
            var output = new byte[fileSize];

            using (var writer = new BinaryWriter(new MemoryStream(output)))
            {
                var fileHeader = new BmpFileHeader
                {
                    FileSize = fileSize,
                    Height = height,
                    Width = width,
                    BitsPerPixel = 24,
                    Planes = 1,
                    ImageSize = fileSize - BmpFileHeader.DEFAULT_OFFSET,
                    ClrUsed = 0,
                    ClrImportant = 0
                };
                writer.Write(fileHeader.Type);
                writer.Write(fileHeader.FileSize);
                writer.Write(fileHeader.Reserved);
                writer.Write(fileHeader.Offset);
                writer.Write(fileHeader.HeaderSize);
                writer.Write(fileHeader.Width);
                writer.Write(fileHeader.Height);
                writer.Write(fileHeader.Planes);
                writer.Write(fileHeader.BitsPerPixel);
                writer.Write(fileHeader.Compression);
                writer.Write(fileHeader.ImageSize);
                writer.Write(fileHeader.XPelsPerMeter);
                writer.Write(fileHeader.YPelsPerMeter);
                writer.Write(fileHeader.ClrUsed);
                writer.Write(fileHeader.ClrImportant);

                if (amount != 0) amount = 4 - amount;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (var x = 0; x < width; x++)
                    {
                        int offset = (y * width + x) * 4;

                        writer.Write(pixelData[offset + 0]);
                        writer.Write(pixelData[offset + 1]);
                        writer.Write(pixelData[offset + 2]);
                    }

                    for (var i = 0; i < amount; i++)
                    {
                        writer.Write((byte) 0);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Decode the provided bmp to a BGRA pixel array.
        /// Supported formats are:
        /// - 1, 4, and 8 bit BMP, tested with up to 256 colors.
        /// - 16bit BMP (untested)
        /// - 24bit BMP
        /// - 32bit BMP (untested)
        /// </summary>
        /// <param name="bmpData">The bmp file as bytes to decode.</param>
        /// <param name="fileHeader">The header of the file. Contains information such as the size of the image.</param>
        /// <returns>An RGBA pixel array extracted from the BMP.</returns>
        public static byte[] Decode(ReadOnlyMemory<byte> bmpData, out BmpFileHeader fileHeader)
        {
            using var reader = new ByteReader(bmpData);

            fileHeader = new BmpFileHeader
            {
                Type = reader.ReadInt16(),
                FileSize = reader.ReadInt32(),
                Reserved = reader.ReadInt32(),
                Offset = reader.ReadInt32(),

                // Second header
                HeaderSize = reader.ReadInt32(),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                Planes = reader.ReadInt16(),
                BitsPerPixel = reader.ReadInt16(),
                Compression = reader.ReadInt32(),
                ImageSize = reader.ReadInt32(),
                XPelsPerMeter = reader.ReadInt32(),
                YPelsPerMeter = reader.ReadInt32(),
                ClrUsed = reader.ReadInt32(),
                ClrImportant = reader.ReadInt32()
            };

            if (fileHeader.Compression != 0) throw new Exception($"Unsupported BMP compression - {fileHeader.Compression}");

            int colorMapSize = -1;

            if (fileHeader.ClrUsed == 0)
            {
                if (fileHeader.BitsPerPixel == 1 ||
                    fileHeader.BitsPerPixel == 4 ||
                    fileHeader.BitsPerPixel == 8)
                    colorMapSize = (int) Math.Pow(2, fileHeader.BitsPerPixel) * 4;
            }
            else
            {
                colorMapSize = fileHeader.ClrUsed * 4;
            }

            ReadOnlySpan<byte> palette = null;

            if (colorMapSize > 0) palette = reader.ReadBytes(colorMapSize);

            ReadOnlySpan<byte> pixelIn = reader.ReadBytes(fileHeader.ImageSize);
            var pixelOut = new byte[fileHeader.Width * fileHeader.Height * 4];

            if (fileHeader.HeaderSize != 40)
                throw new Exception(
                    $"Header Size value '{fileHeader.HeaderSize}' is not valid.");

            if (fileHeader.BitsPerPixel == 32)
                ReadRgb32(pixelIn, pixelOut, fileHeader.Width, fileHeader.Height);
            else if (fileHeader.BitsPerPixel == 24)
                ReadRgb24(pixelIn, pixelOut, fileHeader.Width, fileHeader.Height);
            else if (fileHeader.BitsPerPixel == 16)
                ReadRgb16(pixelIn, pixelOut, fileHeader.Width, fileHeader.Height);
            else if (fileHeader.BitsPerPixel <= 8)
                ReadRgbPalette(
                    pixelIn,
                    pixelOut,
                    palette,
                    fileHeader.Width,
                    fileHeader.Height,
                    fileHeader.BitsPerPixel);

            return pixelOut;
        }

        #region Decoder Helpers

        private static void ReadRgbPalette(ReadOnlySpan<byte> inputData, byte[] outputData, ReadOnlySpan<byte> colors, int width, int height, int bits)
        {
            // Pixels per byte (bits per pixel)
            int ppb = 8 / bits;

            int arrayWidth = (width + ppb - 1) / ppb;

            // Bit mask
            int mask = 0xFF >> (8 - bits);

            // Rows are aligned on 4 byte boundaries
            int alignment = arrayWidth % 4;
            if (alignment != 0) alignment = 4 - alignment;

            for (var y = 0; y < height; y++)
            {
                int rowOffset = y * (arrayWidth + alignment);

                for (var x = 0; x < arrayWidth; x++)
                {
                    int offset = rowOffset + x;

                    int colOffset = x * ppb;

                    for (var shift = 0; shift < ppb && colOffset + shift < width; shift++)
                    {
                        int colorIndex = (inputData[offset] >> (8 - bits - shift * bits)) & mask;

                        int arrayOffset = (y * width + colOffset + shift) * 4;
                        outputData[arrayOffset + 0] = colors[colorIndex * 4 + 0];
                        outputData[arrayOffset + 1] = colors[colorIndex * 4 + 1];
                        outputData[arrayOffset + 2] = colors[colorIndex * 4 + 2];

                        outputData[arrayOffset + 3] = 255;
                    }
                }
            }
        }

        private static void ReadRgb16(ReadOnlySpan<byte> inputData, byte[] outputData, int width, int height)
        {
            // Masks for the colors of the 16 bit BMP.
            const int rgb16RMask = 0x00007C00;
            const int rgb16GMask = 0x000003E0;
            const int rgb16BMask = 0x0000001F;

            const int scaleR = 256 / 32;
            const int scaleG = 256 / 64;

            int alignment = width * 2 % 4;
            if (alignment != 0) alignment = 4 - alignment;

            for (var y = 0; y < height; y++)
            {
                int rowOffset = y * (width * 2 + alignment);

                for (var x = 0; x < width; x++)
                {
                    int offset = rowOffset + x * 2;

                    var temp = BitConverter.ToInt16(inputData[offset..]);

                    var r = (byte) (((temp & rgb16RMask) >> 11) * scaleR);
                    var g = (byte) (((temp & rgb16GMask) >> 5) * scaleG);
                    var b = (byte) ((temp & rgb16BMask) * scaleR);

                    int arrayOffset = (y * width + x) * 4;
                    outputData[arrayOffset + 0] = b;
                    outputData[arrayOffset + 1] = g;
                    outputData[arrayOffset + 2] = r;

                    outputData[arrayOffset + 3] = 255;
                }
            }
        }

        private static void ReadRgb24(ReadOnlySpan<byte> inputData, byte[] outputData, int width, int height)
        {
            int alignment = width * 3 % 4;
            if (alignment != 0) alignment = 4 - alignment;

            for (var y = 0; y < height; y++)
            {
                int rowOffset = y * (width * 3 + alignment);

                for (var x = 0; x < width; x++)
                {
                    int offset = rowOffset + x * 3;

                    int arrayOffset = (y * width + x) * 4;
                    outputData[arrayOffset + 0] = inputData[offset + 0];
                    outputData[arrayOffset + 1] = inputData[offset + 1];
                    outputData[arrayOffset + 2] = inputData[offset + 2];

                    outputData[arrayOffset + 3] = 255;
                }
            }
        }

        private static void ReadRgb32(ReadOnlySpan<byte> inputData, byte[] outputData, int width, int height)
        {
            int alignment = width * 4 % 4;
            if (alignment == 0) alignment = 4 - alignment;

            for (var y = 0; y < height; y++)
            {
                int rowOffset = y * (width * 4 + alignment);

                for (var x = 0; x < width; x++)
                {
                    int offset = rowOffset + x * 4;

                    int arrayOffset = (y * width + x) * 4;
                    outputData[arrayOffset + 0] = inputData[offset + 0];
                    outputData[arrayOffset + 1] = inputData[offset + 1];
                    outputData[arrayOffset + 2] = inputData[offset + 2];

                    outputData[arrayOffset + 3] = 255;
                }
            }
        }

        #endregion
    }
}