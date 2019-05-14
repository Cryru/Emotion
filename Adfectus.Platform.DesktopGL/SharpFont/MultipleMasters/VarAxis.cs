#region Using

using System;
using SharpFont.MultipleMasters.Internal;

#endregion

namespace SharpFont.MultipleMasters
{
    /// <summary>
    /// A simple structure used to model a given axis in design space for Multiple Masters and GX var fonts.
    /// </summary>
    public class VarAxis
    {
        #region Fields

        private IntPtr reference;
        private VarAxisRec rec;

        #endregion

        #region Constructors

        internal VarAxis(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the axis's name. Not always meaningful for GX.
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
        /// Gets the axis's default design coordinate. FreeType computes meaningful default values for MM; it is then
        /// an integer value, not in 16.16 format.
        /// </summary>
        public int Default
        {
            get => (int) rec.def;
        }

        /// <summary>
        /// Gets the axis's maximum design coordinate.
        /// </summary>
        public int Maximum
        {
            get => (int) rec.maximum;
        }

        /// <summary>
        /// Gets the axis's tag (the GX equivalent to ‘name’). FreeType provides default values for MM if possible.
        /// </summary>

        public uint Tag
        {
            get => (uint) rec.tag;
        }

        /// <summary>
        /// Gets the entry in ‘name’ table (another GX version of ‘name’). Not meaningful for MM.
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
                rec = PInvokeHelper.PtrToStructure<VarAxisRec>(reference);
            }
        }

        #endregion
    }
}