#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of constants used to describe subglyphs. Please refer to the TrueType specification for the meaning of
    /// the various flags.
    /// </summary>
    [Flags]
    public enum SubGlyphFlags
    {
        /// <summary>
        /// Set this to indicate arguments are word size; otherwise, they are byte size.
        /// </summary>
        ArgsAreWords = 0x0001,

        /// <summary>
        /// Set this to indicate arguments are X and Y values; otherwise, X and Y indicate point coordinates.
        /// </summary>
        ArgsAreXYValues = 0x0002,

        /// <summary>
        /// Set this to round XY values to the grid.
        /// </summary>
        RoundXYToGrid = 0x0004,

        /// <summary>
        /// Set this to indicate the component has a simple scale; otherwise, the scale is 1.0.
        /// </summary>
        Scale = 0x0008,

        /// <summary>
        /// Set this to indicate that X and Y are scaled independently.
        /// </summary>
        XYScale = 0x0040,

        /// <summary>
        /// Set this to indicate there is a 2 by 2 transformation used to scale the component.
        /// </summary>
        TwoByTwo = 0x0080,

        /// <summary>
        /// Set this to forse aw, lsb and rsb for the composite to be equal to those from the original glyph.
        /// </summary>
        UseMyMetrics = 0x0200
    }
}