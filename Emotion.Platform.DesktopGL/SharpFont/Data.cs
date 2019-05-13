#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace SharpFont
{
    /// <summary>
    /// Read-only binary data represented as a pointer and a length.
    /// </summary>
    public sealed class Data
    {
        #region Fields

        #endregion

        #region Constructors

        internal Data(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data.
        /// </summary>
        public IntPtr Pointer
        {
            get => Marshal.ReadIntPtr(Reference, 0);
        }

        /// <summary>
        /// Gets the length of the data in bytes.
        /// </summary>
        public int Length
        {
            get => Marshal.ReadInt32(Reference, IntPtr.Size);
        }

        internal IntPtr Reference { get; set; }

        #endregion
    }
}