namespace FreeImageAPI
{
    /// <summary>
    /// Image color types used in FreeImage.
    /// </summary>
    public enum FREE_IMAGE_COLOR_TYPE
    {
        /// <summary>
        /// min value is white
        /// </summary>
        FIC_MINISWHITE = 0,

        /// <summary>
        /// min value is black
        /// </summary>
        FIC_MINISBLACK = 1,

        /// <summary>
        /// RGB color model
        /// </summary>
        FIC_RGB = 2,

        /// <summary>
        /// color map indexed
        /// </summary>
        FIC_PALETTE = 3,

        /// <summary>
        /// RGB color model with alpha channel
        /// </summary>
        FIC_RGBALPHA = 4,

        /// <summary>
        /// CMYK color model
        /// </summary>
        FIC_CMYK = 5
    }
}