#region Using

using System;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Emotion.Utility
{
    public static class ImageUtil
    {
        /// <summary>
        /// Convert an image from a single component (assumed to be alpha) to a four component image.
        /// </summary>
        /// <param name="pixels">The single component pixel data.</param>
        /// <returns>The four component pixel data.</returns>
        public static byte[] AToRgba(byte[] pixels)
        {
            if (pixels == null) return null;

            var output = new byte[pixels.Length * 4];
            for (var p = 0; p < pixels.Length * 4; p += 4)
            {
                byte alpha = pixels[p / 4];
                if (alpha <= 0) continue;
                output[p] = 255;
                output[p + 1] = 255;
                output[p + 2] = 255;
                output[p + 3] = alpha;
            }

            return output;
        }

        public static byte[] RgbaToA(byte[] pixels)
        {
            if (pixels == null) return null;

            var output = new byte[pixels.Length / 4];
            for (var p = 0; p < pixels.Length; p += 4)
            {
                byte alpha = pixels[p + 3];
                output[p / 4] = alpha;
            }

            return output;
        }

        /// <summary>
        /// Convert an image of Bgra pixels to Rgba pixels.
        /// </summary>
        /// <param name="pixels">The pixels to convert.</param>
        /// <param name="inPlace">Whether to perform the conversion in place, or create a new byte array.</param>
        /// <returns></returns>
        public static byte[] BgraToRgba(byte[] pixels, bool inPlace = true)
        {
            if (pixels == null) return null;

            byte[] output = inPlace ? pixels : new byte[pixels.Length];
            for (var p = 0; p < pixels.Length; p += 4)
            {
                byte r = pixels[p + 2];
                byte b = pixels[p];

                output[p] = r;
                output[p + 1] = pixels[p + 1];
                output[p + 2] = b;
                output[p + 3] = pixels[p + 3];
            }

            return output;
        }

        /// <summary>
        /// Convert an image of Bgr pixels to Rgb pixels.
        /// </summary>
        /// <param name="pixels">The pixels to convert.</param>
        /// <param name="inPlace">Whether to perform the conversion in place, or create a new byte array.</param>
        /// <returns></returns>
        public static byte[] BgrToRgb(byte[] pixels, bool inPlace = true)
        {
            if (pixels == null) return null;

            byte[] output = inPlace ? pixels : new byte[pixels.Length];
            for (var p = 0; p < pixels.Length; p += 3)
            {
                byte r = pixels[p + 2];
                byte b = pixels[p];

                output[p] = r;
                output[p + 1] = pixels[p + 1];
                output[p + 2] = b;
            }

            return output;
        }

        /// <summary>
        /// Convert an image of Rgba pixels to Bgra pixels.
        /// </summary>
        /// <param name="pixels">The pixels to convert.</param>
        /// <param name="inPlace">Whether to perform the conversion in place, or create a new byte array.</param>
        public static byte[] RgbaToBgra(byte[] pixels, bool inPlace = true)
        {
            byte[] output = inPlace ? pixels : new byte[pixels.Length];
            for (var p = 0; p < pixels.Length; p += 4)
            {
                byte r = pixels[p + 2];
                byte b = pixels[p];

                output[p] = r;
                output[p + 1] = pixels[p + 1];
                output[p + 2] = b;
                output[p + 3] = pixels[p + 3];
            }

            return output;
        }

        /// <summary>
        /// Flips the image on the Y axis.
        /// </summary>
        /// <param name="imageData">The image pixels.</param>
        /// <param name="height">The height of the image.</param>
        public static unsafe void FlipImageY(byte[] imageData, int height)
        {
            int bytesPerRow = imageData.Length / height;

            fixed (void* dataPtr = &imageData[0])
            {
                // Used to temporary hold a row.
                var copyRow = new Span<byte>(new byte[bytesPerRow]);

                var buffer = new Span<byte>(dataPtr, imageData.Length);
                for (var y = 0; y < height / 2; y++)
                {
                    Span<byte> row = buffer.Slice(y * bytesPerRow, bytesPerRow);
                    int invertedY = height - 1 - y;
                    Span<byte> reverseRow = buffer.Slice(invertedY * bytesPerRow, bytesPerRow);

                    row.CopyTo(copyRow);
                    reverseRow.CopyTo(row);
                    copyRow.CopyTo(reverseRow);
                }
            }
        }

        /// <summary>
        /// Flips the image on the Y axis - but doesn't mutate the input pixel data.
        /// </summary>
        /// <param name="imageData">The image pixels.</param>
        /// <param name="height">The height of the image.</param>
        public static unsafe byte[] FlipImageYNoMutate(byte[] imageData, int height)
        {
            var output = new byte[imageData.Length];

            int bytesPerRow = imageData.Length / height;
            fixed (void* outDataPtr = &output[0])
            {
                fixed (void* dataPtr = &imageData[0])
                {
                    var buffer = new Span<byte>(dataPtr, imageData.Length);
                    var outBuffer = new Span<byte>(outDataPtr, imageData.Length);
                    for (var y = 0; y < height / 2; y++)
                    {
                        int rowIdx = y * bytesPerRow;
                        int reverseRowIdx = (height - 1 - y) * bytesPerRow;

                        Span<byte> row = buffer.Slice(rowIdx, bytesPerRow);
                        Span<byte> reverseRow = buffer.Slice(reverseRowIdx, bytesPerRow);

                        Span<byte> outputRow = outBuffer.Slice(rowIdx, bytesPerRow);
                        Span<byte> outputReverseRow = outBuffer.Slice(reverseRowIdx, bytesPerRow);

                        row.CopyTo(outputReverseRow);
                        reverseRow.CopyTo(outputRow);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Invert the endianess of pixels who are made up of four bytes.
        /// This can be used to convert RGBA to BGRA or in reverse.
        /// </summary>
        /// <param name="imageData">The pixel data to invert.</param>
        public static unsafe void InvertEndian32Bpp(byte[] imageData)
        {
            const int components = 4;

            Debug.Assert(imageData.Length % 4 == 0);

            fixed (byte* dataPtr = &imageData[0])
            {
                for (var pixelOffset = 0; pixelOffset < imageData.Length; pixelOffset += components)
                {
                    byte b = dataPtr[pixelOffset];
                    byte g = dataPtr[pixelOffset + 1];
                    byte r = dataPtr[pixelOffset + 2];
                    byte a = dataPtr[pixelOffset + 3];

                    dataPtr[pixelOffset] = r;
                    dataPtr[pixelOffset + 1] = g;
                    dataPtr[pixelOffset + 2] = b;
                    dataPtr[pixelOffset + 3] = a;
                }
            }
        }

        /// <summary>
        /// Converts byte array to a new array where each value in the original array is represented
        /// by a specified number of bits.
        /// </summary>
        /// <param name="bytes">The bytes to convert from. Cannot be null.</param>
        /// <param name="bits">The number of bits per value.</param>
        /// <param name="relativeValue">
        /// If true then the value in the bits will be scaled to the byte's range, if false it will be
        /// clipped.
        /// </param>
        /// <returns>The resulting byte array. Is never null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="bits" /> is greater or equals than zero.</exception>
        public static Span<byte> ToArrayByBitsLength(Span<byte> bytes, int bits, bool relativeValue = true)
        {
            if (bits >= 8) return bytes;

            Span<byte> result = new byte[bytes.Length * 8 / bits];
            int factor = (int) Math.Pow(2, bits) - 1;
            int f = 255 / factor;
            int mask = 0xFF >> (8 - bits);
            var resultOffset = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];

                for (var shift = 0; shift < 8; shift += bits)
                {
                    int value = (b >> (8 - bits - shift)) & mask;
                    if (relativeValue) value *= f;
                    result[resultOffset] = (byte) value;
                    resultOffset++;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies a 3x3 box filter to an image.
        /// </summary>
        /// <param name="pixels">The image pixels.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="kernel">The box filter kernel. Only 3x3 kernels are supported.</param>
        /// <returns></returns>
        public static byte[] BoxFilter3X3(byte[] pixels, int width, int height, float[] kernel)
        {
            var result = new byte[width * height];
            int[] indices =
            {
                -(width + 1), -width, -(width - 1),
                -1, 0, +1,
                width - 1, width, width + 1
            };

            float denominator = kernel.Sum();
            if (denominator == 0.0f) denominator = 1.0f;

            for (var i = 1; i < height - 1; i++)
            {
                for (var j = 1; j < width - 1; j++)
                {
                    var value = 0f;
                    int indexOffset = i * width + j;
                    for (var k = 0; k < kernel.Length; k++)
                    {
                        byte valueA = pixels[indexOffset + indices[k]];
                        value += valueA * kernel[k];
                    }

                    result[indexOffset] = (byte) (value / denominator);
                }
            }

            return result;
        }
    }
}