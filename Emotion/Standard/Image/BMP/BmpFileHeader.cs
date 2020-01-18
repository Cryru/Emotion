namespace Emotion.Standard.Image.BMP
{
    /// <summary>
    /// Stores general information about the Bitmap file.
    /// </summary>
    /// <remarks>
    /// The first two bytes of the Bitmap file format
    /// (thus the Bitmap header) are stored in big-endian order.
    /// All of the other integer values are stored in little-endian format
    /// (i.e. least-significant byte first).
    /// </remarks>
    public class BmpFileHeader
    {
        /// <summary>
        /// The default size of the bmp file header.
        /// </summary>
        public const int SIZE = 14;

        /// <summary>
        /// The size of the info header.
        /// </summary>
        public const int HEADER_SIZE = 40;

        /// <summary>
        /// The default offset of the two combined headers.
        /// </summary>
        public const int DEFAULT_OFFSET = 54;

        /// <summary>
        /// The magic number used to identify the bitmap file: 0x42 0x4D
        /// (Hex code points for B and M)
        /// </summary>
        public short Type = 19778;

        /// <summary>
        /// The size of the bitmap file in bytes.
        /// </summary>
        public int FileSize;

        /// <summary>
        /// Reserved; actual value depends on the application
        /// that creates the image.
        /// </summary>
        public int Reserved;

        /// This block of bytes tells the application detailed information
        /// about the image, which will be used to display the image on
        /// the screen.
        ///
        /// The bmp is implied to be compressed in the RGB format.
        /// Each image row has a multiple of four elements. If the
        /// row has less elements, zeros will be added at the right side.
        /// The format depends on the number of bits, stored in the info header.
        /// If the number of bits are one, four or eight each pixel data is
        /// a index to the palette. If the number of bits are sixteen,
        /// twenty-four or thirty two each pixel contains a color.
        /// <summary>
        /// The size of the BMP header, in bytes.
        /// </summary>
        public int HeaderSize = HEADER_SIZE;

        /// <summary>
        /// The offset, i.e. starting address, of the byte where
        /// the bitmap data can be found.
        /// </summary>
        public int Offset = DEFAULT_OFFSET;

        /// <summary>
        /// The bitmap width in pixels (signed integer).
        /// </summary>
        public int Width;

        /// <summary>
        /// The bitmap height in pixels (signed integer).
        /// </summary>
        public int Height;

        /// <summary>
        /// The number of color planes being used. Must be set to 1.
        /// </summary>
        public short Planes;

        /// <summary>
        /// The number of bits per pixel, which is the color depth of the image.
        /// Typical values are 1, 4, 8, 16, 24 and 32.
        /// </summary>
        public short BitsPerPixel;

        /// <summary>
        /// The image size. This is the size of the raw bitmap data (see below),
        /// and should not be confused with the file size.
        /// </summary>
        public int ImageSize;

        /// <summary>
        /// The horizontal resolution of the image.
        /// (pixel per meter, signed integer)
        /// </summary>
        public int XPelsPerMeter;

        /// <summary>
        /// The vertical resolution of the image.
        /// (pixel per meter, signed integer)
        /// </summary>
        public int YPelsPerMeter;

        /// <summary>
        /// The number of colors in the color palette,
        /// or 0 to default to 2^n.
        /// </summary>
        public int ClrUsed;

        /// <summary>
        /// The number of important colors used,
        /// or 0 when every color is important; generally ignored.
        /// </summary>
        public int ClrImportant;

        /// <summary>
        /// The compression of the image - only RGB is supported.
        /// </summary>
        public int Compression;
    }
}