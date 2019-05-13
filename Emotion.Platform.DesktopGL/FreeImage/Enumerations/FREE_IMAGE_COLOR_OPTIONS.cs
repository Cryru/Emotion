namespace FreeImageAPI
{
    /// <summary>
    /// Constants used in color filling routines.
    /// </summary>
    public enum FREE_IMAGE_COLOR_OPTIONS
    {
        /// <summary>
        /// Default value.
        /// </summary>
        FICO_DEFAULT = 0x0,

        /// <summary>
        /// <see cref="RGBQUAD" /> color is RGB color (contains no valid alpha channel).
        /// </summary>
        FICO_RGB = 0x0,

        /// <summary>
        /// <see cref="RGBQUAD" /> color is RGBA color (contains a valid alpha channel).
        /// </summary>
        FICO_RGBA = 0x1,

        /// <summary>
        /// Lookup nearest RGB color from palette.
        /// </summary>
        FICO_NEAREST_COLOR = 0x0,

        /// <summary>
        /// Lookup equal RGB color from palette.
        /// </summary>
        FICO_EQUAL_COLOR = 0x2,

        /// <summary>
        /// <see cref="RGBQUAD.rgbReserved" /> contains the palette index to be used.
        /// </summary>
        FICO_ALPHA_IS_INDEX = 0x4
    }
}