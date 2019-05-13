#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A handle (pointer) to a raster object. Each object can be used independently to convert an outline into a
    /// bitmap or pixmap.
    /// </summary>
    public class Raster : NativeObject
    {
        #region Constructors

        internal Raster(IntPtr reference) : base(reference)
        {
        }

        #endregion
    }
}