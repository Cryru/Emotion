#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of bit-field constants used with <see cref="Face.TrueTypeGXValidate" /> to indicate which TrueTypeGX/AAT
    /// Type tables should be validated.
    /// </summary>
    [Flags]
    public enum TrueTypeValidationFlags : uint
    {
        /// <summary>Validate ‘feat’ table.</summary>
        Feat = 0x4000 << 0,

        /// <summary>Validate ‘mort’ table.</summary>
        Mort = 0x4000 << 1,

        /// <summary>Validate ‘morx’ table.</summary>
        Morx = 0x4000 << 2,

        /// <summary>Validate ‘bsln’ table.</summary>
        Bsln = 0x4000 << 3,

        /// <summary>Validate ‘just’ table.</summary>
        Just = 0x4000 << 4,

        /// <summary>Validate ‘kern’ table.</summary>
        Kern = 0x4000 << 5,

        /// <summary>Validate ‘opbd’ table.</summary>
        Opbd = 0x4000 << 6,

        /// <summary>Validate ‘trak’ table.</summary>
        Trak = 0x4000 << 7,

        /// <summary>Validate ‘prop’ table.</summary>
        Prop = 0x4000 << 8,

        /// <summary>Validate ‘lcar’ table.</summary>
        Lcar = 0x4000 << 9,

        /// <summary>Validate all TrueTypeGX tables.</summary>
        All = Feat | Mort | Morx | Bsln | Just | Kern | Opbd | Trak | Prop | Lcar
    }
}