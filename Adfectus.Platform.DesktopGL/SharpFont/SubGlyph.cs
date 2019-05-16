#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The subglyph structure is an internal object used to describe subglyphs (for example, in the case of
    /// composites).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The subglyph implementation is not part of the high-level API, hence the forward structure declaration.
    ///     </para>
    ///     <para>
    ///     You can however retrieve subglyph information with <see cref="GlyphSlot.GetSubGlyphInfo" />.
    ///     </para>
    /// </remarks>
    public sealed class SubGlyph
    {
        #region Fields

        #endregion

        #region Constructors

        internal SubGlyph(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        internal IntPtr Reference { get; set; }

        #endregion
    }
}