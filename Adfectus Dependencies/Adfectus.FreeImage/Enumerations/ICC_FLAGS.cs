#region Using

using System;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// Flags for ICC profiles.
    /// </summary>
    [Flags]
    public enum ICC_FLAGS : ushort
    {
        /// <summary>
        /// Default value.
        /// </summary>
        FIICC_DEFAULT = 0x00,

        /// <summary>
        /// The color is CMYK.
        /// </summary>
        FIICC_COLOR_IS_CMYK = 0x01
    }
}