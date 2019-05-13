#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of values and/or bit-flags returned by the FT_Get_Gasp function.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The bit-flags <see cref="Gasp.DoGridfit" /> and <see cref="Gasp.DoGray" /> are to be used for standard font
    ///     rasterization only. Independently of that, <see cref="Gasp.SymmetricSmoothing" /> and
    ///     <see cref="Gasp.SymmetricGridfit" /> are to be used if ClearType is enabled (and <see cref="Gasp.DoGridfit" />
    ///     and <see cref="Gasp.DoGray" /> are consequently ignored).
    ///     </para>
    ///     <para>
    ///     ‘ClearType’ is Microsoft's implementation of LCD rendering, partly protected by patents.
    ///     </para>
    /// </remarks>
    [Flags]
    public enum Gasp
    {
        /// <summary>
        /// This special value means that there is no GASP table in this face. It is up to the client to decide what to
        /// do.
        /// </summary>
        NoTable = -1,

        /// <summary>
        /// Grid-fitting and hinting should be performed at the specified ppem. This really means TrueType bytecode
        /// interpretation. If this bit is not set, no hinting gets applied.
        /// </summary>
        DoGridfit = 0x01,

        /// <summary>
        /// Anti-aliased rendering should be performed at the specified ppem. If not set, do monochrome rendering.
        /// </summary>
        DoGray = 0x02,

        /// <summary>
        /// If set, smoothing along multiple axes must be used with ClearType.
        /// </summary>
        SymmetricSmoothing = 0x08,

        /// <summary>
        /// Grid-fitting must be used with ClearType's symmetric smoothing.
        /// </summary>
        SymmetricGridfit = 0x10
    }
}