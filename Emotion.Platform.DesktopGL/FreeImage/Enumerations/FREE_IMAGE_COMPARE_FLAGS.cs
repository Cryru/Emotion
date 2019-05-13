#region Using

using System;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// List of combinable compare modes.
    /// </summary>
    [Flags]
    public enum FREE_IMAGE_COMPARE_FLAGS
    {
        /// <summary>
        /// Compare headers.
        /// </summary>
        HEADER = 0x1,

        /// <summary>
        /// Compare palettes.
        /// </summary>
        PALETTE = 0x2,

        /// <summary>
        /// Compare pixel data.
        /// </summary>
        DATA = 0x4,

        /// <summary>
        /// Compare meta data.
        /// </summary>
        METADATA = 0x8,

        /// <summary>
        /// Compare everything.
        /// </summary>
        COMPLETE = HEADER | PALETTE | DATA | METADATA
    }
}