namespace FreeImageAPI
{
    /// <summary>
    /// Dithering algorithms.
    /// Constants used in FreeImage_Dither.
    /// </summary>
    public enum FREE_IMAGE_DITHER
    {
        /// <summary>
        /// Floyd and Steinberg error diffusion
        /// </summary>
        FID_FS = 0,

        /// <summary>
        /// Bayer ordered dispersed dot dithering (order 2 dithering matrix)
        /// </summary>
        FID_BAYER4x4 = 1,

        /// <summary>
        /// Bayer ordered dispersed dot dithering (order 3 dithering matrix)
        /// </summary>
        FID_BAYER8x8 = 2,

        /// <summary>
        /// Ordered clustered dot dithering (order 3 - 6x6 matrix)
        /// </summary>
        FID_CLUSTER6x6 = 3,

        /// <summary>
        /// Ordered clustered dot dithering (order 4 - 8x8 matrix)
        /// </summary>
        FID_CLUSTER8x8 = 4,

        /// <summary>
        /// Ordered clustered dot dithering (order 8 - 16x16 matrix)
        /// </summary>
        FID_CLUSTER16x16 = 5,

        /// <summary>
        /// Bayer ordered dispersed dot dithering (order 4 dithering matrix)
        /// </summary>
        FID_BAYER16x16 = 6
    }
}