namespace SharpFont
{
    /// <summary>
    /// These values determine how the end of opened sub-paths are rendered in a stroke.
    /// </summary>
    public enum StrokerLineCap
    {
        /// <summary>
        /// The end of lines is rendered as a full stop on the last point itself.
        /// </summary>
        Butt = 0,

        /// <summary>
        /// The end of lines is rendered as a half-circle around the last point.
        /// </summary>
        Round,

        /// <summary>
        /// The end of lines is rendered as a square around the last point.
        /// </summary>
        Square
    }
}