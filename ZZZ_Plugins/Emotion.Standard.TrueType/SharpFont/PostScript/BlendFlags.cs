namespace SharpFont.PostScript
{
    /// <summary>
    /// A set of flags used to indicate which fields are present in a given blend dictionary (font info or private).
    /// Used to support Multiple Masters fonts.
    /// </summary>
    public enum BlendFlags
    {
        /// <summary>
        /// The position of the underline stroke.
        /// </summary>
        UnderlinePosition = 0,

        /// <summary>
        /// The thickness of the underline stroke.
        /// </summary>
        UnderlineThickness,

        /// <summary>
        /// The angle of italics.
        /// </summary>
        ItalicAngle,

        /// <summary>
        /// Set if the font contains BlueValues.
        /// </summary>
        BlueValues,

        /// <summary>
        /// Set if the font contains OtherBlues.
        /// </summary>
        OtherBlues,

        /// <summary>
        /// Set if the font contains StandardWidth values.
        /// </summary>
        StandardWidth,

        /// <summary>
        /// Set if the font contains StandardHeight values.
        /// </summary>
        StandardHeight,

        /// <summary>
        /// Set if the font contains StemSnapWidths.
        /// </summary>
        StemSnapWidths,

        /// <summary>
        /// Set if the font contains StemSnapHeights.
        /// </summary>
        StemSnapHeights,

        /// <summary>
        /// Set if the font contains BlueScale values.
        /// </summary>
        BlueScale,

        /// <summary>
        /// Set if the font contains BlueShift values.
        /// </summary>
        BlueShift,

        /// <summary>
        /// Set if the font contains FamilyBlues values.
        /// </summary>
        FamilyBlues,

        /// <summary>
        /// Set if the font contains FamilyOtherBlues values.
        /// </summary>
        FamilyOtherBlues,

        /// <summary>
        /// Force bold blending.
        /// </summary>
        ForceBold
    }
}