#region Using

using System;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used to hold a single list element.
    /// </summary>
    public class ListNode : NativeObject
    {
        #region Fields

        private ListNodeRec rec;

        #endregion

        #region Constructors

        internal ListNode(IntPtr reference) : base(reference)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the previous element in the list. NULL if first.
        /// </summary>
        public ListNode Previous
        {
            get
            {
                if (rec.prev == IntPtr.Zero)
                    return null;

                return new ListNode(rec.prev);
            }
        }

        /// <summary>
        /// Gets the next element in the list. NULL if last.
        /// </summary>
        public ListNode Next
        {
            get
            {
                if (rec.next == IntPtr.Zero)
                    return null;

                return new ListNode(rec.next);
            }
        }

        /// <summary>
        /// Gets a typeless pointer to the listed object.
        /// </summary>
        public IntPtr Data
        {
            get => rec.data;
        }

        internal override IntPtr Reference
        {
            get => base.Reference;

            set
            {
                base.Reference = value;
                rec = PInvokeHelper.PtrToStructure<ListNodeRec>(value);
            }
        }

        #endregion
    }
}