#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of bit-field constants used with <see cref="Face.ClassicKernValidate" /> to indicate the classic kern
    /// dialect or dialects. If the selected type doesn't fit, <see cref="Face.ClassicKernValidate" /> regards the table
    /// as invalid.
    /// </summary>
    [Flags]
    public enum ClassicKernValidationFlags : uint
    {
        /// <summary>Handle the ‘kern’ table as a classic Microsoft kern table.</summary>
        Microsoft = 0x4000 << 0,

        /// <summary>Handle the ‘kern’ table as a classic Apple kern table.</summary>
        Apple = 0x4000 << 1,

        /// <summary>Handle the ‘kern’ as either classic Apple or Microsoft kern table.</summary>
        All = Microsoft | Apple
    }
}