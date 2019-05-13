#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A union type used to store either a long or a pointer. This is used to store a file descriptor or a ‘FILE*’ in
    /// an input stream.
    /// </summary>
    public class StreamDesc
    {
        #region Fields

        private IntPtr reference;
        private StreamDescRec rec;

        #endregion

        #region Constructors

        internal StreamDesc(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="StreamDesc" /> as a file descriptor.
        /// </summary>
        public int Value
        {
            get => (int) rec.value;
        }

        /// <summary>
        /// Gets the <see cref="StreamDesc" /> as an input stream (FILE*).
        /// </summary>
        public IntPtr Pointer
        {
            get => rec.pointer;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<StreamDescRec>(reference);
            }
        }

        #endregion
    }
}