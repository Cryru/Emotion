#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A handle to a given FreeType renderer. A renderer is a special module in charge of converting a glyph image to
    /// a bitmap, when necessary. Each renderer supports a given glyph image format, and one or more target surface
    /// depths.
    /// </summary>
    public class Renderer
    {
        #region Fields

        #endregion

        #region Constructors

        internal Renderer(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        internal IntPtr Reference { get; set; }

        #endregion
    }
}