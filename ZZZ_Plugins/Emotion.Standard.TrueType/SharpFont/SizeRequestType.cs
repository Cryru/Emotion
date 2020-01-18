namespace SharpFont
{
    /// <summary>
    /// An enumeration type that lists the supported size request types.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The above descriptions only apply to scalable formats. For bitmap formats, the behaviour is up to the driver.
    ///     </para>
    ///     <para>
    ///     See the note section of <see cref="SizeMetrics" /> if you wonder how size
    ///     requesting relates to scaling values.
    ///     </para>
    /// </remarks>
    public enum SizeRequestType
    {
        /// <summary>
        /// The nominal size. The ‘units_per_EM’ field of <see cref="Face" /> is used to determine both scaling values.
        /// </summary>
        Normal,

        /// <summary>
        /// The real dimension. The sum of the the ‘ascender’ and (minus of) the ‘descender’ fields of
        /// <see cref="Face" /> are used to determine both scaling values.
        /// </summary>
        RealDimensions,

        /// <summary>
        /// The font bounding box. The width and height of the ‘bbox’ field of <see cref="Face" /> are used to determine
        /// the horizontal and vertical scaling value, respectively.
        /// </summary>
        BoundingBox,

        /// <summary>
        /// The ‘max_advance_width’ field of <see cref="Face" /> is used to determine the horizontal scaling value; the
        /// vertical scaling value is determined the same way as <see cref="SizeRequestType.RealDimensions" /> does.
        /// Finally, both scaling values are set to the smaller one. This type is useful if you want to specify the
        /// font size for, say, a window of a given dimension and 80x24 cells.
        /// </summary>
        Cell,

        /// <summary>
        /// Specify the scaling values directly.
        /// </summary>
        Scales
    }
}