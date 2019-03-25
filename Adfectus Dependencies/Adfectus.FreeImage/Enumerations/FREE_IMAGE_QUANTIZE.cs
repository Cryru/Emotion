namespace FreeImageAPI
{
    /// <summary>
    /// Color quantization algorithms.
    /// Constants used in FreeImage_ColorQuantize.
    /// </summary>
    public enum FREE_IMAGE_QUANTIZE
    {
        /// <summary>
        /// Xiaolin Wu color quantization algorithm
        /// </summary>
        FIQ_WUQUANT = 0,

        /// <summary>
        /// NeuQuant neural-net quantization algorithm by Anthony Dekker
        /// </summary>
        FIQ_NNQUANT = 1
    }
}