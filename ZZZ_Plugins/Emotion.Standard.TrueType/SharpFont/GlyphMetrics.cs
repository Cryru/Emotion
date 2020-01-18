#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used to model the metrics of a single glyph. The values are expressed in 26.6 fractional pixel
    /// format; if the flag <see cref="LoadFlags.NoScale" /> has been used while loading the glyph, values are expressed
    /// in font units instead.
    /// </summary>
    /// <remarks>
    /// If not disabled with <see cref="LoadFlags.NoHinting" />, the values represent dimensions of the hinted glyph (in
    /// case hinting is applicable).
    /// </remarks>
    public sealed class GlyphMetrics
    {
        #region Fields

        private IntPtr reference;
        private GlyphMetricsRec rec;

        #endregion

        #region Constructors

        internal GlyphMetrics(IntPtr reference)
        {
            Reference = reference;
        }

        internal GlyphMetrics(GlyphMetricsRec glyphMetInt)
        {
            rec = glyphMetInt;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the glyph's width. If getting metrics from a face loaded with <see cref="LoadFlags.NoScale" />, call
        /// <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 Width
        {
            get => Fixed26Dot6.FromRawValue((int) rec.width);
        }

        /// <summary>
        /// Gets the glyph's height. If getting metrics from a face loaded with <see cref="LoadFlags.NoScale" />, call
        /// <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 Height
        {
            get => Fixed26Dot6.FromRawValue((int) rec.height);
        }

        /// <summary>
        /// Gets the left side bearing for horizontal layout. If getting metrics from a face loaded with
        /// <see cref="LoadFlags.NoScale" />, call <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 HorizontalBearingX
        {
            get => Fixed26Dot6.FromRawValue((int) rec.horiBearingX);
        }

        /// <summary>
        /// Gets the top side bearing for horizontal layout. If getting metrics from a face loaded with
        /// <see cref="LoadFlags.NoScale" />, call <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 HorizontalBearingY
        {
            get => Fixed26Dot6.FromRawValue((int) rec.horiBearingY);
        }

        /// <summary>
        /// Gets the advance width for horizontal layout. If getting metrics from a face loaded with
        /// <see cref="LoadFlags.NoScale" />, call <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 HorizontalAdvance
        {
            get => Fixed26Dot6.FromRawValue((int) rec.horiAdvance);
        }

        /// <summary>
        /// Gets the left side bearing for vertical layout. If getting metrics from a face loaded with
        /// <see cref="LoadFlags.NoScale" />, call <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 VerticalBearingX
        {
            get => Fixed26Dot6.FromRawValue((int) rec.vertBearingX);
        }

        /// <summary>
        /// Gets the top side bearing for vertical layout. Larger positive values mean further below the vertical glyph
        /// origin. If getting metrics from a face loaded with <see cref="LoadFlags.NoScale" />, call
        /// <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 VerticalBearingY
        {
            get => Fixed26Dot6.FromRawValue((int) rec.vertBearingY);
        }

        /// <summary>
        /// Gets the advance height for vertical layout. Positive values mean the glyph has a positive advance
        /// downward. If getting metrics from a face loaded with <see cref="LoadFlags.NoScale" />, call
        /// <see cref="Fixed26Dot6.Value" /> to get the unscaled value.
        /// </summary>
        public Fixed26Dot6 VerticalAdvance
        {
            get => Fixed26Dot6.FromRawValue((int) rec.vertAdvance);
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<GlyphMetricsRec>(reference);
            }
        }

        #endregion
    }
}