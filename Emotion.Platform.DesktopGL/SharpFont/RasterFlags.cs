#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of bit flag constants as used in the ‘flags’ field of a <see cref="RasterParams" /> structure.
    /// </summary>
    [Flags]
    public enum RasterFlags
    {
        /// <summary>
        /// This value is 0.
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// This flag is set to indicate that an anti-aliased glyph image should be generated. Otherwise, it will be
        /// monochrome (1-bit).
        /// </summary>
        AntiAlias = 0x1,

        /// <summary>
        ///     <para>
        ///     This flag is set to indicate direct rendering. In this mode, client applications must provide their own
        ///     span callback. This lets them directly draw or compose over an existing bitmap. If this bit is not set, the
        ///     target pixmap's buffer must be zeroed before rendering.
        ///     </para>
        ///     <para>
        ///     Note that for now, direct rendering is only possible with anti-aliased glyphs.
        ///     </para>
        /// </summary>
        Direct = 0x2,

        /// <summary>
        ///     <para>
        ///     This flag is only used in direct rendering mode. If set, the output will be clipped to a box specified in
        ///     the ‘clip_box’ field of the <see cref="RasterParams" /> structure.
        ///     </para>
        ///     <para>
        ///     Note that by default, the glyph bitmap is clipped to the target pixmap, except in direct rendering mode
        ///     where all spans are generated if no clipping box is set.
        ///     </para>
        /// </summary>
        Clip = 0x4
    }
}