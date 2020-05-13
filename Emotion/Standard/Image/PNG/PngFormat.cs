#region Using

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Standard.Logging;
using Emotion.Standard.Utility.Zlib;
using Emotion.Utility;

#endregion

namespace Emotion.Standard.Image.PNG
{
    public static class PngFormat
    {
        /// <summary>
        /// Verify whether the bytes are a png file, by checking the magic.
        /// </summary>
        /// <param name="data">The data to check.</param>
        /// <returns>Whether the data fits the png magic.</returns>
        public static bool IsPng(byte[] data)
        {
            return data.Length >= 8 &&
                   data[0] == 0x89 &&
                   data[1] == 0x50 && // P
                   data[2] == 0x4E && // N
                   data[3] == 0x47 && // G
                   data[4] == 0x0D && // CR
                   data[5] == 0x0A && // LF
                   data[6] == 0x1A && // EOF
                   data[7] == 0x0A; // LF
        }

        /// <summary>
        /// Encodes the provided BGRA, Y flipped, pixel data to a PNG image.
        /// </summary>
        /// <param name="pixels">The pixel date to encode.</param>
        /// <param name="width">The width of the image in the data.</param>
        /// <param name="height">The height of the image in the data.</param>
        /// <returns>A PNG image as bytes.</returns>
        public static byte[] Encode(byte[] pixels, int width, int height)
        {
            var pngHeader = new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A};
            const int maxBlockSize = 0xFFFF;

            using var stream = new MemoryStream();

            // Write the png header.
            stream.Write(pngHeader, 0, 8);

            var header = new PngFileHeader
            {
                Width = width,
                Height = height,
                ColorType = 6,
                BitDepth = 8,
                FilterMethod = 0,
                CompressionMethod = 0,
                InterlaceMethod = 0
            };

            // Write header chunk.
            var chunkData = new byte[13];

            WriteInteger(chunkData, 0, header.Width);
            WriteInteger(chunkData, 4, header.Height);

            chunkData[8] = header.BitDepth;
            chunkData[9] = header.ColorType;
            chunkData[10] = header.CompressionMethod;
            chunkData[11] = header.FilterMethod;
            chunkData[12] = header.InterlaceMethod;

            WriteChunk(stream, PngChunkTypes.HEADER, chunkData);

            // Write data chunks.
            var data = new byte[width * height * 4 + height];
            int rowLength = width * 4 + 1; // One byte for the filter
            for (var y = 0; y < height; y++)
            {
                data[y * rowLength] = 0;

                for (var x = 0; x < width; x++)
                {
                    // Calculate the offset for the new array.
                    int dataOffset = y * rowLength + x * 4 + 1;

                    // Calculate the offset for the original pixel array.
                    int pixelOffset = (y * width + x) * 4;

                    data[dataOffset + 0] = pixels[pixelOffset + 2];
                    data[dataOffset + 1] = pixels[pixelOffset + 1];
                    data[dataOffset + 2] = pixels[pixelOffset + 0];
                    data[dataOffset + 3] = pixels[pixelOffset + 3];
                }
            }

            byte[] buffer = ZlibStreamUtility.Compress(data, 6);
            int numChunks = buffer.Length / maxBlockSize;
            if (buffer.Length % maxBlockSize != 0) numChunks++;
            for (var i = 0; i < numChunks; i++)
            {
                int length = buffer.Length - i * maxBlockSize;

                if (length > maxBlockSize) length = maxBlockSize;
                WriteChunk(stream, PngChunkTypes.DATA, buffer, i * maxBlockSize, length);
            }

            // Write end chunk.
            WriteChunk(stream, PngChunkTypes.END, null);
            stream.Flush();
            return stream.ToArray();
        }

        #region Writing Utility

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            WriteChunk(stream, type, data, 0, data?.Length ?? 0);
        }

        private static void WriteChunk(Stream stream, string type, byte[] data, int offset, int length)
        {
            WriteInteger(stream, length);

            var typeArray = new byte[4];
            typeArray[0] = (byte) type[0];
            typeArray[1] = (byte) type[1];
            typeArray[2] = (byte) type[2];
            typeArray[3] = (byte) type[3];

            stream.Write(typeArray, 0, 4);

            if (data != null) stream.Write(data, offset, length);

            var crc32 = new Crc32();
            crc32.Update(typeArray);

            if (data != null) crc32.Update(data, offset, length);

            WriteInteger(stream, (uint) crc32.Value);
        }

        private static void WriteInteger(byte[] data, int offset, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
            Array.Copy(buffer, 0, data, offset, 4);
        }

        private static void WriteInteger(Stream stream, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
            stream.Write(buffer, 0, 4);
        }

        private static void WriteInteger(Stream stream, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian) Array.Reverse(buffer);
            stream.Write(buffer, 0, 4);
        }

        #endregion

        /// <summary>
        /// Decode the provided png file to a BGRA pixel array, Y flipped.
        /// </summary>
        /// <param name="pngData">The png file as bytes.</param>
        /// <param name="fileHeader">The png file's header.</param>
        /// <returns>The RGBA pixel data from the png.</returns>
        public static byte[] Decode(byte[] pngData, out PngFileHeader fileHeader)
        {
            fileHeader = new PngFileHeader();

            if (!IsPng(pngData))
            {
                Engine.Log.Warning("Tried to decode a non-png image!", MessageSource.ImagePng);
                return null;
            }

            using var stream = new MemoryStream(pngData);
            stream.Seek(8, SeekOrigin.Current);

            var endChunkReached = false;
            byte[] palette = null;
            byte[] paletteAlpha = null;

            using var dataStream = new MemoryStream();

            // Read chunks while there are valid chunks.
            PngChunk currentChunk;
            while ((currentChunk = new PngChunk(stream)).Valid)
            {
                if (endChunkReached)
                {
                    Engine.Log.Warning("Image did not end with an end chunk...", MessageSource.ImagePng);
                    continue;
                }

                switch (currentChunk.Type)
                {
                    case PngChunkTypes.HEADER:
                    {
                        Array.Reverse(currentChunk.Data, 0, 4);
                        Array.Reverse(currentChunk.Data, 4, 4);

                        fileHeader.Width = BitConverter.ToInt32(currentChunk.Data, 0);
                        fileHeader.Height = BitConverter.ToInt32(currentChunk.Data, 4);

                        fileHeader.BitDepth = currentChunk.Data[8];
                        fileHeader.ColorType = currentChunk.Data[9];
                        fileHeader.FilterMethod = currentChunk.Data[11];
                        fileHeader.InterlaceMethod = currentChunk.Data[12];
                        fileHeader.CompressionMethod = currentChunk.Data[10];
                        break;
                    }
                    case PngChunkTypes.DATA:
                        dataStream.Write(currentChunk.Data, 0, currentChunk.Data.Length);
                        break;
                    case PngChunkTypes.PALETTE:
                        palette = currentChunk.Data;
                        break;
                    case PngChunkTypes.PALETTE_ALPHA:
                        paletteAlpha = currentChunk.Data;
                        break;
                    case PngChunkTypes.END:
                        endChunkReached = true;
                        break;
                }
            }

            stream.Dispose();

            // Decompress data.
            dataStream.Position = 0;
            PerfProfiler.ProfilerEventStart("PNG Decompression", "Loading");
            byte[] data = ZlibStreamUtility.Decompress(dataStream);
            PerfProfiler.ProfilerEventEnd("PNG Decompression", "Loading");

            var channelsPerColor = 0;
            ColorReader reader;
            switch (fileHeader.ColorType)
            {
                case 0:
                    // Grayscale - No Alpha
                    channelsPerColor = 1;
                    reader = Grayscale;
                    break;
                case 2:
                    // RGB
                    channelsPerColor = 3;
                    reader = Rgb;
                    break;
                case 3:
                    // Palette
                    channelsPerColor = 1;
                    reader = (a, b, c, d) => { Palette(palette, paletteAlpha, a, b, c, d); };
                    break;
                case 4:
                    // Grayscale - Alpha
                    channelsPerColor = 2;
                    reader = GrayscaleAlpha;
                    break;
                case 6:
                    // RGBA
                    channelsPerColor = 4;
                    reader = Rgba;
                    break;
                default:
                    reader = null;
                    break;
            }

            // Unknown color mode.
            if (reader == null)
            {
                Engine.Log.Warning($"Unsupported color type - {fileHeader.ColorType}", MessageSource.ImagePng);
                return new byte[fileHeader.Width * fileHeader.Height * 4];
            }

            // Calculate the bytes per pixel.
            var bytesPerPixel = 1;
            if (fileHeader.BitDepth >= 8) bytesPerPixel = channelsPerColor * fileHeader.BitDepth / 8;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (fileHeader.InterlaceMethod == 1)
                return ParseInterlaced(data, fileHeader, bytesPerPixel, channelsPerColor, reader);

            int scanlineLength = GetScanlineLength(fileHeader, channelsPerColor) + 1;
            int scanLineCount = data.Length / scanlineLength;
            return Parse(scanlineLength, scanLineCount, data, bytesPerPixel, fileHeader, reader);
        }

        #region Readers

        private delegate void ColorReader(int rowPixels, Span<byte> row, byte[] imageDest, int destRow);

        private static void Grayscale(int pixelCount, Span<byte> data, byte[] pixels, int y)
        {
            int totalOffset = y * pixelCount;
            for (var x = 0; x < pixelCount; x++)
            {
                int offset = (totalOffset + x) * 4;
                pixels[offset + 0] = data[x];
                pixels[offset + 1] = data[x];
                pixels[offset + 2] = data[x];
                pixels[offset + 3] = 255;
            }
        }

        private static void GrayscaleAlpha(int pixelCount, Span<byte> data, byte[] pixels, int y)
        {
            int totalOffset = y * pixelCount;
            for (var x = 0; x < pixelCount; x++)
            {
                int offsetSrc = x * 2;
                int offset = (totalOffset + x) * 4;

                pixels[offset + 0] = data[offsetSrc];
                pixels[offset + 1] = data[offsetSrc];
                pixels[offset + 2] = data[offsetSrc];
                pixels[offset + 3] = data[offsetSrc + 1];
            }
        }

        private static void Rgb(int pixelCount, Span<byte> data, byte[] pixels, int y)
        {
            int totalOffset = y * pixelCount;
            for (var x = 0; x < pixelCount; x++)
            {
                int offsetSrc = x * 3;
                int offset = (totalOffset + x) * 4;

                pixels[offset + 0] = data[offsetSrc + 2];
                pixels[offset + 1] = data[offsetSrc + 1];
                pixels[offset + 2] = data[offsetSrc];
                pixels[offset + 3] = 255;
            }
        }

        private static void Rgba(int pixelCount, Span<byte> data, byte[] pixels, int y)
        {
            int totalOffset = y * pixelCount;
            for (var x = 0; x < pixelCount; x++)
            {
                int offsetSrc = x * 4;
                int offset = (totalOffset + x) * 4;

                pixels[offset + 0] = data[offsetSrc + 2];
                pixels[offset + 1] = data[offsetSrc + 1];
                pixels[offset + 2] = data[offsetSrc];
                pixels[offset + 3] = data[offsetSrc + 3];
            }
        }

        private static void Palette(byte[] palette, byte[] paletteAlpha, int pixelCount, Span<byte> data, byte[] pixels, int y)
        {
            if (paletteAlpha != null && paletteAlpha.Length > 0)
            {
                // If the alpha palette is not null and does one or
                // more entries, this means, that the image contains and alpha
                // channel and we should try to read it.
                for (var i = 0; i < pixelCount; i++)
                {
                    int index = data[i];
                    if (index * 3 + 2 >= palette.Length) continue;

                    int offset = (y * pixelCount + i) * 4;
                    pixels[offset + 0] = palette[index * 3 + 2];
                    pixels[offset + 1] = palette[index * 3 + 1];
                    pixels[offset + 2] = palette[index * 3 + 0];
                    pixels[offset + 3] = paletteAlpha.Length > index ? paletteAlpha[index] : (byte) 255;
                }

                return;
            }

            for (var i = 0; i < pixelCount; i++)
            {
                int index = data[i];
                if (index * 3 + 2 >= palette.Length) continue;

                int offset = (y * pixelCount + i) * 4;
                pixels[offset + 0] = palette[index * 3 + 2];
                pixels[offset + 1] = palette[index * 3 + 1];
                pixels[offset + 2] = palette[index * 3 + 0];
                pixels[offset + 3] = 255;
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] Parse(int scanlineLength, int scanlineCount, byte[] pureData, int bytesPerPixel, PngFileHeader header, ColorReader reader)
        {
            var pixels = new byte[header.Width * header.Height * 4];
            int length = scanlineLength - 1;
            var data = new Span<byte>(pureData);

            // Analyze if the scanlines can be read in parallel.
            var filterMode = byte.MaxValue;
            for (var i = 0; i < scanlineCount; i++)
            {
                byte f = data[scanlineLength * i];
                if (f == filterMode) continue;
                filterMode = byte.MaxValue;
                break;
            }

            // Multiple filters or a dependency filter are in affect.
            if (filterMode == byte.MaxValue || filterMode != 0 && filterMode != 1)
            {
                if (scanlineCount >= 1500) Engine.Log.Trace("Loaded a big PNG with scanlines which require filtering. If you re-export it without that, it will load faster.", MessageSource.ImagePng);

                PerfProfiler.ProfilerEventStart("PNG Parse Sequential", "Loading");
                var readOffset = 0;
                var previousScanline = Span<byte>.Empty;
                for (var i = 0; i < scanlineCount; i++)
                {
                    // Early out for invalid data.
                    if (data.Length - readOffset < scanlineLength) break;

                    Span<byte> scanline = data.Slice(readOffset + 1, length);
                    int filter = data[readOffset];
                    ApplyFilter(scanline, previousScanline, filter, bytesPerPixel);

                    reader(header.Width, ConvertBitArray(scanline, header), pixels, i);
                    previousScanline = scanline;
                    readOffset += scanlineLength;
                }

                PerfProfiler.ProfilerEventEnd("PNG Parse Sequential", "Loading");
                return pixels;
            }

            // Single line filter
            if (filterMode == 1)
            {
                PerfProfiler.ProfilerEventStart("PNG Parse Threaded", "Loading");
                ParallelWork.FastLoops(scanlineCount, (start, end) =>
                {
                    int readOffset = start * scanlineLength;
                    for (int i = start; i < end; i++)
                    {
                        // Early out for invalid data.
                        if (pureData.Length - readOffset < scanlineLength) break;
                        Span<byte> scanline = new Span<byte>(pureData).Slice(readOffset + 1, length);
                        for (int j = bytesPerPixel; j < scanline.Length; j++)
                        {
                            scanline[j] = (byte) (scanline[j] + scanline[j - bytesPerPixel]);
                        }

                        reader(header.Width, ConvertBitArray(scanline, header), pixels, i);
                        readOffset += scanlineLength;
                    }
                }).Wait();
                PerfProfiler.ProfilerEventEnd("PNG Parse Threaded", "Loading");
            }

            // No filter!
            // ReSharper disable once InvertIf
            if (filterMode == 0)
            {
                PerfProfiler.ProfilerEventStart("PNG Parse Threaded", "Loading");
                ParallelWork.FastLoops(scanlineCount, (start, end) =>
                {
                    int readOffset = start * scanlineLength;
                    for (int i = start; i < end; i++)
                    {
                        // Early out for invalid data.
                        if (pureData.Length - readOffset < scanlineLength) break;
                        Span<byte> row = ConvertBitArray(new Span<byte>(pureData).Slice(readOffset + 1, length), header);
                        reader(header.Width, row, pixels, i);
                        readOffset += scanlineLength;
                    }
                }).Wait();
                PerfProfiler.ProfilerEventEnd("PNG Parse Threaded", "Loading");
            }

            return pixels;
        }

        /// <summary>
        /// Adam7 interlaced images.
        /// No optimizations are done here.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] ParseInterlaced(byte[] pureData, PngFileHeader fileHeader, int bytesPerPixel, int channelsPerColor, ColorReader reader)
        {
            PerfProfiler.ProfilerEventStart("PNG Parse Interlaced", "Loading");

            // Combine interlaced pixels into one image here.
            var combination = new byte[fileHeader.Width * fileHeader.Height * channelsPerColor];
            var pixels = new byte[fileHeader.Width * fileHeader.Height * 4];

            const int passes = 7;
            var readOffset = 0;
            var done = false;
            var data = new Span<byte>(pureData);
            for (var i = 0; i < passes; i++)
            {
                int columns = Adam7.ComputeColumns(fileHeader.Width, i);
                if (columns == 0) continue;
                int scanlineLength = GetScanlineLengthInterlaced(columns, fileHeader, channelsPerColor) + 1;
                int length = scanlineLength - 1;

                int pixelsInLine = Adam7.ComputeBlockWidth(fileHeader.Width, i);

                // Read scanlines in this pass.
                var previousScanline = Span<byte>.Empty;
                for (int row = Adam7.FirstRow[i]; row < fileHeader.Height; row += Adam7.RowIncrement[i])
                {
                    // Early out if invalid pass.
                    if (data.Length - readOffset < scanlineLength)
                    {
                        done = true;
                        break;
                    }

                    Span<byte> scanLine = data.Slice(readOffset + 1, length);
                    int filter = data[readOffset];
                    readOffset += scanlineLength;
                    ApplyFilter(scanLine, previousScanline, filter, bytesPerPixel);

                    // Dump the row into the combined image.
                    Span<byte> convertedLine = ConvertBitArray(scanLine, fileHeader);
                    for (var pixel = 0; pixel < pixelsInLine; pixel++)
                    {
                        int offset = row * channelsPerColor * fileHeader.Width + (Adam7.FirstColumn[i] + pixel * Adam7.ColumnIncrement[i]) * channelsPerColor;
                        for (var p = 0; p < channelsPerColor; p++)
                        {
                            if (offset + p >= combination.Length) continue;
                            combination[offset + p] = convertedLine[pixel + p];
                        }
                    }

                    previousScanline = scanLine;
                }

                if (done) break;
            }

            // Read the combined image.
            int stride = fileHeader.Width * channelsPerColor;
            int scanlineCount = combination.Length / stride;
            for (var i = 0; i < scanlineCount; i++)
            {
                Span<byte> row = new Span<byte>(combination).Slice(stride * i, stride);
                reader(fileHeader.Width, row, pixels, i);
            }

            PerfProfiler.ProfilerEventEnd("PNG Parse Interlaced", "Loading");
            return pixels;
        }

        #region Reading Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Span<byte> ConvertBitArray(Span<byte> bytes, PngFileHeader header)
        {
            int bits = header.BitDepth;

            if (bits == 8) return bytes;
            if (bits < 8) return ImageUtil.ToArrayByBitsLength(bytes, bits, header.ColorType != 3);
            if (bits > 16) return bytes;

            Span<ushort> conv = MemoryMarshal.Cast<byte, ushort>(bytes);
            Span<byte> byteArray = new byte[conv.Length];
            for (var i = 0; i < byteArray.Length; i++)
            {
                // Simulate a 8bit display by clipping everything above 255.
                byteArray[i] = (byte) conv[i];
            }

            return byteArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetScanlineLength(PngFileHeader fileHeader, int channelsPerColor)
        {
            int scanlineLength = fileHeader.Width * fileHeader.BitDepth * channelsPerColor;
            int amount = scanlineLength % 8;
            if (amount != 0) scanlineLength += 8 - amount;
            scanlineLength /= 8;
            return scanlineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetScanlineLengthInterlaced(int columns, PngFileHeader fileHeader, int channelsPerColor)
        {
            int scanlineLength = columns * fileHeader.BitDepth * channelsPerColor;
            int amount = scanlineLength % 8;
            if (amount != 0) scanlineLength += 8 - amount;
            scanlineLength /= 8;
            return scanlineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyFilter(Span<byte> current, Span<byte> previous, int filter, int bytesPerPixel)
        {
            byte previousPixel = 0;
            byte upperLeft = 0;
            byte pixelAbove = 0;
            // Process each pixel in the scanline.
            for (var column = 0; column < current.Length; column++)
            {
                // Get surrounding pixels for the filter.
                if (column >= bytesPerPixel)
                {
                    previousPixel = current[column - bytesPerPixel];
                    if (previous != Span<byte>.Empty) upperLeft = previous[column - bytesPerPixel];
                }

                if (previous != Span<byte>.Empty) pixelAbove = previous[column];

                byte pixel = current[column];
                // ReSharper disable InvalidXmlDocComment
                current[column] = filter switch
                {
                    /// <summary>
                    /// The Sub filter transmits the difference between each byte and the value of the corresponding
                    /// byte of the prior pixel.
                    /// </summary>
                    1 => (byte) (pixel + previousPixel),

                    /// <summary>
                    /// The Up filter is just like the Sub filter except that the pixel immediately above the current
                    /// pixel, rather than just to its left, is used as the predictor.
                    /// </summary>
                    2 => (byte) (pixel + pixelAbove),

                    /// <summary>
                    /// The Average filter uses the average of the two neighboring pixels (left and above) to
                    /// predict the value of a pixel.
                    /// </summary>
                    3 => (byte) (pixel + (byte) ((previousPixel + pixelAbove) / 2)),

                    /// <summary>
                    /// The Paeth filter computes a simple linear function of the three neighboring pixels (left, above, upper left),
                    /// then chooses as predictor the neighboring pixel closest to the computed value.
                    /// This technique is named after Alan W. Paeth
                    /// </summary>
                    4 => (byte) (pixel + PaethPredicator(previousPixel, pixelAbove, upperLeft)),

                    // No filter, or unknown.
                    _ => pixel
                };
                // ReSharper enable InvalidXmlDocComment
            }
        }

        /// <summary>
        /// Used for the Paeth filter.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte PaethPredicator(byte a, byte b, byte c)
        {
            byte predicator;

            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc)
                predicator = a;
            else if (pb <= pc)
                predicator = b;
            else
                predicator = c;

            return predicator;
        }

        #endregion
    }
}