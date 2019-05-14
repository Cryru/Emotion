namespace SharpFont.PostScript
{
    /// <summary>
    /// An enumeration describing the ‘Encoding’ entry in a Type 1 dictionary.
    /// </summary>
    public enum EncodingType
    {
        /// <summary>
        /// Not encoded.
        /// </summary>
        None = 0,

        /// <summary>
        /// Array encoding.
        /// </summary>
        Array,

        /// <summary>
        /// Standard encoding.
        /// </summary>
        Standard,

        /// <summary>
        /// ISO Latin 1 encoding.
        /// </summary>
        IsoLatin1,

        /// <summary>
        /// Expert encoding.
        /// </summary>
        Expert
    }
}