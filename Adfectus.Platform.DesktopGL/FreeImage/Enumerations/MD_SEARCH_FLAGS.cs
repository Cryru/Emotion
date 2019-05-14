#region Using

using System;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// List different search modes.
    /// </summary>
    [Flags]
    public enum MD_SEARCH_FLAGS
    {
        /// <summary>
        /// The key of the metadata.
        /// </summary>
        KEY = 0x1,

        /// <summary>
        /// The description of the metadata
        /// </summary>
        DESCRIPTION = 0x2,

        /// <summary>
        /// The ToString value of the metadata
        /// </summary>
        TOSTRING = 0x4
    }
}