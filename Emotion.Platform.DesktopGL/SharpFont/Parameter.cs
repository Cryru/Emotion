#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A simple structure used to pass more or less generic parameters to <see cref="Library.OpenFace" />.
    /// </summary>
    /// <remarks>
    /// The ID and function of parameters are driver-specific. See the various <see cref="ParamTag" /> flags for more
    /// information.
    /// </remarks>
    public sealed class Parameter
    {
        #region Fields

        private IntPtr reference;

        #endregion

        #region Constructors

        internal Parameter(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a four-byte identification tag.
        /// </summary>

        public ParamTag Tag
        {
            get => (ParamTag) Record.tag;
        }

        /// <summary>
        /// Gets a pointer to the parameter data.
        /// </summary>
        public IntPtr Data
        {
            get => Record.data;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                Record = PInvokeHelper.PtrToStructure<ParameterRec>(reference);
            }
        }

        internal ParameterRec Record { get; private set; }

        #endregion
    }
}