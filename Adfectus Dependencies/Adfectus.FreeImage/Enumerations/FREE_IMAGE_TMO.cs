namespace FreeImageAPI
{
    /// <summary>
    /// Tone mapping operators. Constants used in FreeImage_ToneMapping.
    /// </summary>
    public enum FREE_IMAGE_TMO
    {
        /// <summary>
        /// Adaptive logarithmic mapping (F. Drago, 2003)
        /// </summary>
        FITMO_DRAGO03 = 0,

        /// <summary>
        /// Dynamic range reduction inspired by photoreceptor physiology (E. Reinhard, 2005)
        /// </summary>
        FITMO_REINHARD05 = 1,

        /// <summary>
        /// Gradient domain high dynamic range compression (R. Fattal, 2002)
        /// </summary>
        FITMO_FATTAL02
    }
}