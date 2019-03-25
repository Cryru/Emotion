#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A handle to a given FreeType module object. Each module can be a font driver, a renderer, or anything else that
    /// provides services to the formers.
    /// </summary>
    public sealed class Module : NativeObject
    {
        #region Constructors

        internal Module(IntPtr reference) : base(reference)
        {
        }

        #endregion
    }
}