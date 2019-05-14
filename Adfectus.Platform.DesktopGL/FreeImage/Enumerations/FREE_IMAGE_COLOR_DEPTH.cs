#region Using

using System;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// Enumeration used for color conversions.
    /// FREE_IMAGE_COLOR_DEPTH contains several colors to convert to.
    /// The default value 'FICD_AUTO'.
    /// </summary>
    [Flags]
    public enum FREE_IMAGE_COLOR_DEPTH
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        FICD_UNKNOWN = 0,

        /// <summary>
        /// Auto selected by the used algorithm.
        /// </summary>
        FICD_AUTO = FICD_UNKNOWN,

        /// <summary>
        /// 1-bit.
        /// </summary>
        FICD_01_BPP = 1,

        /// <summary>
        /// 1-bit using dithering.
        /// </summary>
        FICD_01_BPP_DITHER = FICD_01_BPP,

        /// <summary>
        /// 1-bit using threshold.
        /// </summary>
        FICD_01_BPP_THRESHOLD = FICD_01_BPP | 2,

        /// <summary>
        /// 4-bit.
        /// </summary>
        FICD_04_BPP = 4,

        /// <summary>
        /// 8-bit.
        /// </summary>
        FICD_08_BPP = 8,

        /// <summary>
        /// 16-bit 555 (1 bit remains unused).
        /// </summary>
        FICD_16_BPP_555 = FICD_16_BPP | 2,

        /// <summary>
        /// 16-bit 565 (all bits are used).
        /// </summary>
        FICD_16_BPP = 16,

        /// <summary>
        /// 24-bit.
        /// </summary>
        FICD_24_BPP = 24,

        /// <summary>
        /// 32-bit.
        /// </summary>
        FICD_32_BPP = 32,

        /// <summary>
        /// Reorder palette (make it linear). Only affects 1-, 4- and 8-bit images.
        /// <para>
        /// The palette is only reordered in case the image is greyscale
        /// (all palette entries have the same red, green and blue value).
        /// </para>
        /// </summary>
        FICD_REORDER_PALETTE = 1024,

        /// <summary>
        /// Converts the image to greyscale.
        /// </summary>
        FICD_FORCE_GREYSCALE = 2048,

        /// <summary>
        /// Flag to mask out all non color depth flags.
        /// </summary>
        FICD_COLOR_MASK = FICD_01_BPP | FICD_04_BPP | FICD_08_BPP | FICD_16_BPP | FICD_24_BPP | FICD_32_BPP
    }
}