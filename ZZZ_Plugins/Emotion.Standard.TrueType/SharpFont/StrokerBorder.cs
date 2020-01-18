namespace SharpFont
{
    /// <summary>
    /// These values are used to select a given stroke border in <see cref="Stroker.GetBorderCounts" /> and
    /// <see cref="Stroker.ExportBorder" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Applications are generally interested in the ‘inside’ and ‘outside’ borders. However, there is no direct
    ///     mapping between these and the ‘left’ and ‘right’ ones, since this really depends on the glyph's drawing
    ///     orientation, which varies between font formats.
    ///     </para>
    ///     <para>
    ///     You can however use <see cref="Outline.GetInsideBorder" /> and <see cref="Outline.GetOutsideBorder" /> to get
    ///     these.
    ///     </para>
    /// </remarks>
    public enum StrokerBorder
    {
        /// <summary>
        /// Select the left border, relative to the drawing direction.
        /// </summary>
        Left = 0,

        /// <summary>
        /// Select the right border, relative to the drawing direction.
        /// </summary>
        Right
    }
}