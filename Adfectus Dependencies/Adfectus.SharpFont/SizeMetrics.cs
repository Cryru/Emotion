#region Using

using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The size metrics structure gives the metrics of a size object.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The scaling values, if relevant, are determined first during a size changing operation. The remaining fields
    ///     are then set by the driver. For scalable formats, they are usually set to scaled values of the corresponding
    ///     fields in <see cref="Face" />.
    ///     </para>
    ///     <para>
    ///     Note that due to glyph hinting, these values might not be exact for certain fonts. Thus they must be treated as
    ///     unreliable with an error margin of at least one pixel!
    ///     </para>
    ///     <para>
    ///     Indeed, the only way to get the exact metrics is to render all glyphs. As this would be a definite performance
    ///     hit, it is up to client applications to perform such computations.
    ///     </para>
    ///     <para>
    ///     The <see cref="SizeMetrics" /> structure is valid for bitmap fonts also.
    ///     </para>
    /// </remarks>
    public sealed class SizeMetrics
    {
        #region Fields

        private SizeMetricsRec rec;

        #endregion

        #region Constructors

        internal SizeMetrics(SizeMetricsRec metricsInternal)
        {
            rec = metricsInternal;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the width of the scaled EM square in pixels, hence the term ‘ppem’ (pixels per EM). It is also referred to
        /// as ‘nominal width’.
        /// </summary>

        public ushort NominalWidth
        {
            get => rec.x_ppem;
        }

        /// <summary>
        /// Gets the height of the scaled EM square in pixels, hence the term ‘ppem’ (pixels per EM). It is also referred to
        /// as ‘nominal height’.
        /// </summary>

        public ushort NominalHeight
        {
            get => rec.y_ppem;
        }

        /// <summary>
        /// Gets a 16.16 fractional scaling value used to convert horizontal metrics from font units to 26.6 fractional
        /// pixels. Only relevant for scalable font formats.
        /// </summary>
        public Fixed16Dot16 ScaleX
        {
            get => Fixed16Dot16.FromRawValue((int) rec.x_scale);
        }

        /// <summary>
        /// Gets a 16.16 fractional scaling value used to convert vertical metrics from font units to 26.6 fractional
        /// pixels. Only relevant for scalable font formats.
        /// </summary>
        public Fixed16Dot16 ScaleY
        {
            get => Fixed16Dot16.FromRawValue((int) rec.y_scale);
        }

        /// <summary>
        /// Gets the ascender in 26.6 fractional pixels.
        /// </summary>
        /// <see cref="Face" />
        public Fixed26Dot6 Ascender
        {
            get => Fixed26Dot6.FromRawValue((int) rec.ascender);
        }

        /// <summary>
        /// Gets the descender in 26.6 fractional pixels.
        /// </summary>
        /// <see cref="Face" />
        public Fixed26Dot6 Descender
        {
            get => Fixed26Dot6.FromRawValue((int) rec.descender);
        }

        /// <summary>
        /// Gets the height in 26.6 fractional pixels.
        /// </summary>
        /// <see cref="Face" />
        public Fixed26Dot6 Height
        {
            get => Fixed26Dot6.FromRawValue((int) rec.height);
        }

        /// <summary>
        /// Gets the maximal advance width in 26.6 fractional pixels.
        /// </summary>
        /// <see cref="Face" />
        public Fixed26Dot6 MaxAdvance
        {
            get => Fixed26Dot6.FromRawValue((int) rec.max_advance);
        }

        #endregion
    }
}