namespace SharpFont.TrueType
{
    /// <summary>
    /// An enumeration used to specify the index of an SFNT table. Used in the <see cref="Face.GetSfntTable" /> API
    /// function.
    /// </summary>
    public enum SfntTag
    {
        /// <summary>
        /// The 'head' (header) table.
        /// </summary>
        Header = 0,

        /// <summary>
        /// The 'maxp' (maximum profile) table.
        /// </summary>
        MaxProfile = 1,

        /// <summary>
        /// The 'os/2' (OS/2 and Windows) table.
        /// </summary>
        OS2 = 2,

        /// <summary>
        /// The 'hhea' (horizontal metrics header) table.
        /// </summary>
        HorizontalHeader = 3,

        /// <summary>
        /// The 'vhea' (vertical metrics header) table.
        /// </summary>
        VertHeader = 4,

        /// <summary>
        /// The 'post' (PostScript) table.
        /// </summary>
        Postscript = 5,

        /// <summary>
        /// The 'pclt' (PCL5 data) table.
        /// </summary>
        Pclt = 6
    }
}