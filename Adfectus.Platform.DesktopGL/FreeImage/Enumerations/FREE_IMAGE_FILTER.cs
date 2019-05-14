namespace FreeImageAPI
{
    /// <summary>
    /// Upsampling / downsampling filters. Constants used in FreeImage_Rescale.
    /// </summary>
    public enum FREE_IMAGE_FILTER
    {
        /// <summary>
        /// Box, pulse, Fourier window, 1st order (constant) b-spline
        /// </summary>
        FILTER_BOX = 0,

        /// <summary>
        /// Mitchell and Netravali's two-param cubic filter
        /// </summary>
        FILTER_BICUBIC = 1,

        /// <summary>
        /// Bilinear filter
        /// </summary>
        FILTER_BILINEAR = 2,

        /// <summary>
        /// 4th order (cubic) b-spline
        /// </summary>
        FILTER_BSPLINE = 3,

        /// <summary>
        /// Catmull-Rom spline, Overhauser spline
        /// </summary>
        FILTER_CATMULLROM = 4,

        /// <summary>
        /// Lanczos3 filter
        /// </summary>
        FILTER_LANCZOS3 = 5
    }
}