namespace SharpFont
{
    /// <summary>
    /// Constants used as the tag of <see cref="Parameter" /> structures.
    /// </summary>
    public enum ParamTag : uint
    {
        /// <summary>
        /// A constant used as the tag of <see cref="Parameter" /> structures to make <see cref="Library.OpenFace" />
        /// ignore preferred family subfamily names in ‘name’ table since OpenType version 1.4. For backwards
        /// compatibility with legacy systems which has 4-face-per-family restriction.
        /// </summary>
        IgnorePreferredFamily = ('i' << 24) | ('g' << 16) | ('p' << 8) | 'f',

        /// <summary>
        /// A constant used as the tag of <see cref="Parameter" /> structures to make <see cref="Library.OpenFace" />
        /// ignore preferred subfamily names in ‘name’ table since OpenType version 1.4. For backwards compatibility
        /// with legacy systems which has 4-face-per-family restriction.
        /// </summary>
        IgnorePreferredSubfamily = ('i' << 24) | ('g' << 16) | ('p' << 8) | 's',

        /// <summary>
        /// A constant used as the tag of <see cref="Parameter" /> structures to indicate an incremental loading object
        /// to be used by FreeType.
        /// </summary>
        Incremental = ('i' << 24) | ('n' << 16) | ('c' << 8) | 'r',

        /// <summary>
        /// A constant used as the tag of an <see cref="Parameter" /> structure to indicate that unpatented methods only
        /// should be used by the TrueType bytecode interpreter for a typeface opened by
        /// <see cref="Library.OpenFace" />.
        /// </summary>
        UnpatentedHinting = ('u' << 24) | ('n' << 16) | ('p' << 8) | 'a'
    }
}