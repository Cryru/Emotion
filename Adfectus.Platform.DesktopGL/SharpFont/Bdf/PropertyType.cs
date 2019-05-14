namespace SharpFont.Bdf
{
    /// <summary>
    /// A list of BDF property types.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>Value 0 is used to indicate a missing property.</summary>
        None = 0,

        /// <summary>Property is a string atom.</summary>
        Atom = 1,

        /// <summary>Property is a 32-bit signed integer.</summary>
        Integer = 2,

        /// <summary>Property is a 32-bit unsigned integer.</summary>
        Cardinal = 3
    }
}