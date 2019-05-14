namespace SharpFont
{
    /// <summary>
    ///     <para>
    ///     A list of values used to describe an outline's contour orientation.
    ///     </para>
    ///     <para>
    ///     The TrueType and PostScript specifications use different conventions to determine whether outline contours
    ///     should be filled or unfilled.
    ///     </para>
    /// </summary>
    public enum Orientation
    {
        /// <summary>
        /// According to the TrueType specification, clockwise contours must be filled, and counter-clockwise ones must
        /// be unfilled.
        /// </summary>
        TrueType = 0,

        /// <summary>
        /// According to the PostScript specification, counter-clockwise contours must be filled, and clockwise ones
        /// must be unfilled.
        /// </summary>
        PostScript = 1,

        /// <summary>
        /// This is identical to <see cref="TrueType" />, but is used to remember that in TrueType, everything that is
        /// to the right of the drawing direction of a contour must be filled.
        /// </summary>
        FillRight = TrueType,

        /// <summary>
        /// This is identical to <see cref="PostScript" />, but is used to remember that in PostScript, everything that
        /// is to the left of the drawing direction of a contour must be filled.
        /// </summary>
        FillLeft = PostScript,

        /// <summary>
        /// The orientation cannot be determined. That is, different parts of the glyph have different orientation.
        /// </summary>
        None
    }
}