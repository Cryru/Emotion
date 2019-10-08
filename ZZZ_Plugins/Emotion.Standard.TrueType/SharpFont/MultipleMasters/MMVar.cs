#region Using

using System;
using SharpFont.MultipleMasters.Internal;

#endregion

namespace SharpFont.MultipleMasters
{
    /// <summary>
    ///     <para>
    ///     A structure used to model the axes and space of a Multiple Masters or GX var distortable font.
    ///     </para>
    ///     <para>
    ///     Some fields are specific to one format and not to the other.
    ///     </para>
    /// </summary>
    public class MMVar
    {
        #region Fields

        private IntPtr reference;
        private MMVarRec rec;

        #endregion

        #region Constructors

        internal MMVar(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of axes. The maximum value is 4 for MM; no limit in GX.
        /// </summary>

        public uint AxisCount
        {
            get => rec.num_axis;
        }

        /// <summary>
        /// Gets the number of designs; should be normally 2^num_axis for MM fonts. Not meaningful for GX (where every
        /// glyph could have a different number of designs).
        /// </summary>

        public uint DesignsCount
        {
            get => rec.num_designs;
        }

        /// <summary>
        /// Gets the number of named styles; only meaningful for GX which allows certain design coordinates to have a
        /// string ID (in the ‘name’ table) associated with them. The font can tell the user that, for example,
        /// Weight=1.5 is ‘Bold’.
        /// </summary>

        public uint NamedStylesCount
        {
            get => rec.num_namedstyles;
        }

        /// <summary>
        /// Gets a table of axis descriptors. GX fonts contain slightly more data than MM.
        /// </summary>
        public VarAxis Axis
        {
            get => new VarAxis(rec.axis);
        }

        /// <summary>
        /// Gets a table of named styles. Only meaningful with GX.
        /// </summary>
        public VarNamedStyle NamedStyle
        {
            get => new VarNamedStyle(rec.namedstyle);
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<MMVarRec>(reference);
            }
        }

        #endregion
    }
}