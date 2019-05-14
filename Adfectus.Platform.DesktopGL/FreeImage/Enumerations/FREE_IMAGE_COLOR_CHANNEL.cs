namespace FreeImageAPI
{
    /// <summary>
    /// Color channels. Constants used in color manipulation routines.
    /// </summary>
    public enum FREE_IMAGE_COLOR_CHANNEL
    {
        /// <summary>
        /// Use red, green and blue channels
        /// </summary>
        FICC_RGB = 0,

        /// <summary>
        /// Use red channel
        /// </summary>
        FICC_RED = 1,

        /// <summary>
        /// Use green channel
        /// </summary>
        FICC_GREEN = 2,

        /// <summary>
        /// Use blue channel
        /// </summary>
        FICC_BLUE = 3,

        /// <summary>
        /// Use alpha channel
        /// </summary>
        FICC_ALPHA = 4,

        /// <summary>
        /// Use black channel
        /// </summary>
        FICC_BLACK = 5,

        /// <summary>
        /// Complex images: use real part
        /// </summary>
        FICC_REAL = 6,

        /// <summary>
        /// Complex images: use imaginary part
        /// </summary>
        FICC_IMAG = 7,

        /// <summary>
        /// Complex images: use magnitude
        /// </summary>
        FICC_MAG = 8,

        /// <summary>
        /// Complex images: use phase
        /// </summary>
        FICC_PHASE = 9
    }
}