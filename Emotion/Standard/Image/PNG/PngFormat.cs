#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Standard.Image.PNG.Readers;
using Emotion.Standard.Logging;
using Emotion.Standard.Utility.Zlib;

#endregion

namespace Emotion.Standard.Image.PNG
{
    public static class PngFormat
    {
        /// <summary>
        /// Lookup table for supported PNG color types - bit depths and their readers.
        /// </summary>
        private static readonly PngColorTypeInformation[] ColorTypes =
        {
            new PngColorTypeInformation(1, new[] {1, 2, 4, 8},
                (p, a) => new GrayscaleReader(false)),
            null,
            new PngColorTypeInformation(3, new[] {8, 16},
                (p, a) => new TrueColorReader(false)),
            new PngColorTypeInformation(1, new[] {1, 2, 4, 8},
                (p, a) => new PaletteIndexReader(p, a)),
            new PngColorTypeInformation(2, new[] {8},
                (p, a) => new GrayscaleReader(true)),
            null,
            new PngColorTypeInformation(4, new[] {8},
                (p, a) => new TrueColorReader(true))
        };


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

            int rowLength = width * 4 + 1;

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

                        // Validate header.
                        if (fileHeader.ColorType >= ColorTypes.Length || ColorTypes[fileHeader.ColorType] == null)
                        {
                            Engine.Log.Warning($"Color type '{fileHeader.ColorType}' is not supported or not valid.", MessageSource.ImagePng);
                            return null;
                        }

                        if (!ColorTypes[fileHeader.ColorType].SupportedBitDepths.Contains(fileHeader.BitDepth))
                            Engine.Log.Warning($"Bit depth '{fileHeader.BitDepth}' is not supported or not valid for color type {fileHeader.ColorType}.", MessageSource.ImagePng);

                        if (fileHeader.FilterMethod != 0) Engine.Log.Warning("The png specification only defines 0 as filter method.", MessageSource.ImagePng);
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

            // Determine color reader to use.
            PngColorTypeInformation colorTypeInformation = ColorTypes[fileHeader.ColorType];
            if (colorTypeInformation == null) return null;
            IColorReader colorReader = colorTypeInformation.CreateColorReader(palette, paletteAlpha);

            // Calculate the bytes per pixel.
            var bytesPerPixel = 1;
            if (fileHeader.BitDepth >= 8) bytesPerPixel = colorTypeInformation.ChannelsPerColor * fileHeader.BitDepth / 8;

            // Decompress data.
            dataStream.Position = 0;
            PerfProfiler.ProfilerEventStart("PNG Decompression", "Loading");
            byte[] data = ZlibStreamUtility.Decompress(dataStream);
            PerfProfiler.ProfilerEventEnd("PNG Decompression", "Loading");

            // Parse into pixels.
            var pixels = new byte[fileHeader.Width * fileHeader.Height * 4];
            if (fileHeader.InterlaceMethod == 1)
                ParseInterlaced(data, fileHeader, bytesPerPixel, colorReader, pixels);
            else
                Parse(data, fileHeader, bytesPerPixel, colorReader, pixels);

            return pixels;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Parse(byte[] data, PngFileHeader fileHeader, int bytesPerPixel, IColorReader reader, byte[] pixels)
        {
            // Find the scan line length.
            int scanlineLength = GetScanlineLength(fileHeader.Width, fileHeader) + 1;
            int scanLineCount = data.Length / scanlineLength;

            // Run through all scanlines.
            var cannotParallel = new List<int>();
            ParallelWork.FastLoops(scanLineCount, (start, end) =>
            {
                int readOffset = start * scanlineLength;
                for (int i = start; i < end; i++)
                {
                    // Early out for invalid data.
                    if (data.Length - readOffset < scanlineLength) break;

                    // Get the current scanline.
                    var rowData = new Span<byte>(data, readOffset + 1, scanlineLength - 1);
                    int filter = data[readOffset];
                    readOffset += scanlineLength;

                    // Check if it has a filter.
                    // PNG filters require the previous row.
                    // We can't do those in parallel.
                    if (filter != 0)
                    {
                        lock (cannotParallel)
                        {
                            cannotParallel.Add(i);
                        }

                        continue;
                    }

                    reader.ReadScanline(rowData, pixels, fileHeader, i);
                }
            }).Wait();

            if (cannotParallel.Count == 0) return;

            PerfProfiler.ProfilerEventStart("PNG Parse Sequential", "Loading");

            // Run scanlines which couldn't be parallel processed.
            if (scanLineCount >= 2000) Engine.Log.Trace("Loaded a big PNG with scanlines which require filtering. If you re-export it without that, it will load faster.", MessageSource.ImagePng);
            cannotParallel.Sort();
            for (var i = 0; i < cannotParallel.Count; i++)
            {
                int idx = cannotParallel[i];

                int rowStart = idx * scanlineLength;
                Span<byte> prevRowData = idx == 0 ? null : new Span<byte>(data, (idx - 1) * scanlineLength + 1, scanlineLength - 1);
                var rowData = new Span<byte>(data, rowStart + 1, scanlineLength - 1);

                // Apply filter to the whole row.
                int filter = data[rowStart];
                for (var column = 0; column < rowData.Length; column++)
                {
                    rowData[column] = ApplyFilter(rowData, prevRowData, filter, column, bytesPerPixel);
                }

                reader.ReadScanline(rowData, pixels, fileHeader, idx);
            }

            PerfProfiler.ProfilerEventEnd("PNG Parse Sequential", "Loading");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ParseInterlaced(byte[] data, PngFileHeader fileHeader, int bytesPerPixel, IColorReader reader, byte[] pixels)
        {
            const int passes = 7;
            var readOffset = 0;
            var done = false;
            var r = new Span<byte>(data);
            for (var i = 0; i < passes; i++)
            {
                int columns = Adam7.ComputeColumns(fileHeader.Width, i);
                if (columns == 0) continue;
                int rowSize = GetScanlineLength(columns, fileHeader) + 1;

                // Read rows.
                Span<byte> prevRowData = null;
                for (int row = Adam7.FirstRow[i]; row < fileHeader.Height; row += Adam7.RowIncrement[i])
                {
                    // Early out if invalid pass.
                    if (r.Length - readOffset < rowSize)
                    {
                        done = true;
                        break;
                    }

                    var rowData = new Span<byte>(new byte[rowSize]);
                    r.Slice(readOffset, rowSize).CopyTo(rowData);
                    int filter = rowData[0];
                    rowData = rowData.Slice(1);
                    readOffset += rowSize;

                    // Apply filter to the whole row.
                    for (var column = 0; column < rowData.Length; column++)
                    {
                        rowData[column] = ApplyFilter(rowData, prevRowData, filter, column, bytesPerPixel);
                    }

                    reader.ReadScanlineInterlaced(rowData, pixels, fileHeader, row, i);
                    prevRowData = rowData;
                }

                if (done) break;
            }
        }

        #region Helpers

        private static int GetScanlineLength(int width, PngFileHeader fileHeader)
        {
            int scanlineLength = width * fileHeader.BitDepth * ColorTypes[fileHeader.ColorType].ChannelsPerColor;
            int amount = scanlineLength % 8;
            if (amount != 0) scanlineLength += 8 - amount;
            scanlineLength /= 8;
            return scanlineLength;
        }

        private static byte ApplyFilter(Span<byte> current, Span<byte> previous, int filterType, int column, int bytesPerPixel)
        {
            bool firstScanline = previous.Length == 0;

            byte previousPixel;
            byte upperLeft;
            // Check if first on the column.
            if (column >= bytesPerPixel)
            {
                previousPixel = current[column - bytesPerPixel];
                upperLeft = firstScanline ? (byte) 0 : previous[column - bytesPerPixel];
            }
            else
            {
                previousPixel = 0;
                upperLeft = 0;
            }

            byte pixel = current[column];
            byte pixelAbove = firstScanline ? (byte) 0 : previous[column];

            // ReSharper disable InvalidXmlDocComment
            return filterType switch
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

        /// <summary>
        /// Used for the Paeth filter.
        /// </summary>
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