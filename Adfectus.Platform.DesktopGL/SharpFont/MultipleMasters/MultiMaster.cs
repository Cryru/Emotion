#region Using

using System;
using SharpFont.MultipleMasters.Internal;

#endregion

namespace SharpFont.MultipleMasters
{
    /// <summary>
    ///     <para>
    ///     A structure used to model the axes and space of a Multiple Masters font.
    ///     </para>
    ///     <para>
    ///     This structure can't be used for GX var fonts.
    ///     </para>
    /// </summary>
    public class MultiMaster
    {
        #region Fields

        private IntPtr reference;
        private MultiMasterRec rec;

        #endregion

        #region Constructors

        internal MultiMaster(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of axes. Cannot exceed 4.
        /// </summary>

        public uint AxisCount
        {
            get => rec.num_axis;
        }

        /// <summary>
        /// Gets the number of designs; should be normally 2^num_axis even though the Type 1 specification strangely
        /// allows for intermediate designs to be present. This number cannot exceed 16.
        /// </summary>

        public uint DesignsCount
        {
            get => rec.num_designs;
        }

        /// <summary>
        /// Gets a table of axis descriptors.
        /// </summary>
        public MMAxis[] Axis
        {
            get
            {
                MMAxis[] axis = new MMAxis[rec.num_axis];

                for (int i = 0; i < rec.num_axis; i++)
                {
                    axis[i] = new MMAxis(rec.axis[i]);
                }

                return axis;
            }
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<MultiMasterRec>(reference);
            }
        }

        #endregion
    }
}