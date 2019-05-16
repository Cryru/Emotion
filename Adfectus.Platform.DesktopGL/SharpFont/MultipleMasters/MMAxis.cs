#region Using

using System;
using SharpFont.MultipleMasters.Internal;

#endregion

namespace SharpFont.MultipleMasters
{
    /// <summary>
    ///     <para>
    ///     A simple structure used to model a given axis in design space for Multiple Masters fonts.
    ///     </para>
    ///     <para>
    ///     This structure can't be used for GX var fonts.
    ///     </para>
    /// </summary>
    public class MMAxis
    {
        #region Fields

        private IntPtr reference;
        private MMAxisRec rec;

        #endregion

        #region Constructors

        internal MMAxis(IntPtr reference)
        {
            Reference = reference;
        }

        internal MMAxis(MMAxisRec axisInternal)
        {
            rec = axisInternal;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the axis's name.
        /// </summary>
        public string Name
        {
            get => rec.name;
        }

        /// <summary>
        /// Gets the axis's minimum design coordinate.
        /// </summary>
        public int Minimum
        {
            get => (int) rec.minimum;
        }

        /// <summary>
        /// Gets the axis's maximum design coordinate.
        /// </summary>
        public int Maximum
        {
            get => (int) rec.maximum;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<MMAxisRec>(reference);
            }
        }

        #endregion
    }
}