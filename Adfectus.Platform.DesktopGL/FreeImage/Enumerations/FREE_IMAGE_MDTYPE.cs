namespace FreeImageAPI
{
    /// <summary>
    /// Tag data type information (based on TIFF specifications)
    /// Note: RATIONALs are the ratio of two 32-bit integer values.
    /// </summary>
    public enum FREE_IMAGE_MDTYPE
    {
        /// <summary>
        /// placeholder
        /// </summary>
        FIDT_NOTYPE = 0,

        /// <summary>
        /// 8-bit unsigned integer
        /// </summary>
        FIDT_BYTE = 1,

        /// <summary>
        /// 8-bit bytes w/ last byte null
        /// </summary>
        FIDT_ASCII = 2,

        /// <summary>
        /// 16-bit unsigned integer
        /// </summary>
        FIDT_SHORT = 3,

        /// <summary>
        /// 32-bit unsigned integer
        /// </summary>
        FIDT_LONG = 4,

        /// <summary>
        /// 64-bit unsigned fraction
        /// </summary>
        FIDT_RATIONAL = 5,

        /// <summary>
        /// 8-bit signed integer
        /// </summary>
        FIDT_SBYTE = 6,

        /// <summary>
        /// 8-bit untyped data
        /// </summary>
        FIDT_UNDEFINED = 7,

        /// <summary>
        /// 16-bit signed integer
        /// </summary>
        FIDT_SSHORT = 8,

        /// <summary>
        /// 32-bit signed integer
        /// </summary>
        FIDT_SLONG = 9,

        /// <summary>
        /// 64-bit signed fraction
        /// </summary>
        FIDT_SRATIONAL = 10,

        /// <summary>
        /// 32-bit IEEE floating point
        /// </summary>
        FIDT_FLOAT = 11,

        /// <summary>
        /// 64-bit IEEE floating point
        /// </summary>
        FIDT_DOUBLE = 12,

        /// <summary>
        /// 32-bit unsigned integer (offset)
        /// </summary>
        FIDT_IFD = 13,

        /// <summary>
        /// 32-bit RGBQUAD
        /// </summary>
        FIDT_PALETTE = 14
    }
}