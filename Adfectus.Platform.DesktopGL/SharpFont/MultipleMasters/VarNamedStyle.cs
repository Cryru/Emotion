#region Using

using System;
using SharpFont.MultipleMasters.Internal;

#endregion

namespace SharpFont.MultipleMasters
{
    /// <summary>
    ///     <para>
    ///     A simple structure used to model a named style in a GX var font.
    ///     </para>
    ///     <para>
    ///     This structure can't be used for MM fonts.
    ///     </para>
    /// </summary>
    public class VarNamedStyle
    {
        #region Fields

        private IntPtr reference;
        private VarNamedStyleRec rec;

        #endregion

        #region Constructors

        internal VarNamedStyle(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the design coordinates for this style. This is an array with one entry for each axis.
        /// </summary>
        public IntPtr Coordinates
        {
            get => rec.coords;
        }

        /// <summary>
        /// Gets the entry in ‘name’ table identifying this style.
        /// </summary>

        public uint StrId
        {
            get => rec.strid;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<VarNamedStyleRec>(reference);
            }
        }

        #endregion
    }
}