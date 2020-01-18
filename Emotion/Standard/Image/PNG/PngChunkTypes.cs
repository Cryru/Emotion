namespace Emotion.Standard.Image.PNG
{
    /// <summary>
    /// Contains a list of possible chunk type identifier.
    /// </summary>
    internal class PngChunkTypes
    {
        /// <summary>
        /// The first chunk in a png file. Can only exists once. Contains
        /// common information like the width and the height of the image or
        /// the used compression method.
        /// </summary>
        public const string HEADER = "IHDR";

        /// <summary>
        /// The PLTE chunk contains from 1 to 256 palette entries, each a three byte
        /// series in the RGB format.
        /// </summary>
        public const string PALETTE = "PLTE";

        /// <summary>
        /// The IDAT chunk contains the actual image data. The image can contains more
        /// than one chunk of this type. All chunks together are the whole image.
        /// </summary>
        public const string DATA = "IDAT";

        /// <summary>
        /// This chunk must appear last. It marks the end of the PNG datastream.
        /// The chunk's data field is empty.
        /// </summary>
        public const string END = "IEND";

        /// <summary>
        /// This chunk specifies that the image uses simple transparency:
        /// either alpha values associated with palette entries (for indexed-color images)
        /// or a single transparent color (for grayscale and truecolor images).
        /// </summary>
        public const string PALETTE_ALPHA = "tRNS";

        /// <summary>
        /// Textual information that the encoder wishes to record with the image can be stored in
        /// tEXt chunks. Each tEXt chunk contains a keyword and a text string.
        /// </summary>
        public const string TEXT = "tEXt";

        /// <summary>
        /// This chunk specifies the relationship between the image samples and the desired
        /// display output intensity.
        /// </summary>
        public const string GAMMA = "gAMA";

        /// <summary>
        /// The pHYs chunk specifies the intended pixel size or aspect ratio for display of the image.
        /// </summary>
        public const string PHYSICAL = "pHYs";
    }
}