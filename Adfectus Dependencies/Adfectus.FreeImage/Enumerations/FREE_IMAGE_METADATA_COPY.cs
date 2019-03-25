namespace FreeImageAPI
{
    /// <summary>
    /// Flags for copying data from a bitmap to another.
    /// </summary>
    public enum FREE_IMAGE_METADATA_COPY
    {
        /// <summary>
        /// Exisiting metadata will remain unchanged.
        /// </summary>
        KEEP_EXISITNG = 0x0,

        /// <summary>
        /// Existing metadata will be cleared.
        /// </summary>
        CLEAR_EXISTING = 0x1,

        /// <summary>
        /// Existing metadata will be overwritten.
        /// </summary>
        REPLACE_EXISTING = 0x2
    }
}