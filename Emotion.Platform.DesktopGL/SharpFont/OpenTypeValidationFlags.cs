#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of bit-field constants used with <see cref="Face.OpenTypeValidate" /> to indicate which OpenType tables
    /// should be validated.
    /// </summary>
    [Flags]
    public enum OpenTypeValidationFlags : uint
    {
        /// <summary>Validate BASE table.</summary>
        Base = 0x0100,

        /// <summary>Validate GDEF table.</summary>
        Gdef = 0x0200,

        /// <summary>Validate GPOS table.</summary>
        Gpos = 0x0400,

        /// <summary>Validate GSUB table.</summary>
        Gsub = 0x0800,

        /// <summary>Validate JSTF table.</summary>
        Jstf = 0x1000,

        /// <summary>Validate MATH table.</summary>
        Math = 0x2000,

        /// <summary>Validate all OpenType tables.</summary>
        All = Base | Gdef | Gpos | Gsub | Jstf | Math
    }
}