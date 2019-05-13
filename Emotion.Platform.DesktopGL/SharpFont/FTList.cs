#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// An <see cref="FTList" /> iterator function which is called during a list parse by <see cref="FTList.Iterate" />.
    /// </summary>
    /// <param name="node">The current iteration list node.</param>
    /// <param name="user">
    /// A typeless pointer passed to <see cref="ListIterator" />. Can be used to point to the iteration's state.
    /// </param>
    /// <returns>Error code.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate Error ListIterator(NativeReference<ListNode> node, IntPtr user);

    /// <summary>
    /// An <see cref="FTList" /> iterator function which is called during a list finalization by
    /// <see cref="FTList.Finalize" /> to destroy all elements in a given list.
    /// </summary>
    /// <param name="memory">The current system object.</param>
    /// <param name="data">The current object to destroy.</param>
    /// <param name="user">
    /// A typeless pointer passed to <see cref="FTList.Iterate" />. It can be used to point to the iteration's state.
    /// </param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ListDestructor(NativeReference<Memory> memory, IntPtr data, IntPtr user);

    /// <summary>
    /// A structure used to hold a simple doubly-linked list. These are used in many parts of FreeType.
    /// </summary>
    public sealed class FTList
    {
        #region Fields

        private IntPtr reference;
        private ListRec rec;

        #endregion

        #region Constructors

        internal FTList(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the head (first element) of doubly-linked list.
        /// </summary>
        public ListNode Head
        {
            get => new ListNode(rec.head);
        }

        /// <summary>
        /// Gets the tail (last element) of doubly-linked list.
        /// </summary>
        public ListNode Tail
        {
            get => new ListNode(rec.tail);
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<ListRec>(reference);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find the list node for a given listed object.
        /// </summary>
        /// <param name="data">The address of the listed object.</param>
        /// <returns>List node. NULL if it wasn't found.</returns>
        public ListNode Find(IntPtr data)
        {
            return new ListNode(FT.FT_List_Find(Reference, data));
        }

        /// <summary>
        /// Append an element to the end of a list.
        /// </summary>
        /// <param name="node">The node to append.</param>
        public void Add(ListNode node)
        {
            FT.FT_List_Add(Reference, node.Reference);
        }

        /// <summary>
        /// Insert an element at the head of a list.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        public void Insert(ListNode node)
        {
            FT.FT_List_Insert(Reference, node.Reference);
        }

        /// <summary>
        /// Remove a node from a list. This function doesn't check whether the node is in the list!
        /// </summary>
        /// <param name="node">The node to remove.</param>
        public void Remove(ListNode node)
        {
            FT.FT_List_Remove(Reference, node.Reference);
        }

        /// <summary>
        /// Move a node to the head/top of a list. Used to maintain LRU lists.
        /// </summary>
        /// <param name="node">The node to move.</param>
        public void Up(ListNode node)
        {
            FT.FT_List_Up(Reference, node.Reference);
        }

        /// <summary>
        /// Parse a list and calls a given iterator function on each element. Note that parsing is stopped as soon as
        /// one of the iterator calls returns a non-zero value.
        /// </summary>
        /// <param name="iterator">An iterator function, called on each node of the list.</param>
        /// <param name="user">A user-supplied field which is passed as the second argument to the iterator.</param>
        public void Iterate(ListIterator iterator, IntPtr user)
        {
            Error err = FT.FT_List_Iterate(Reference, iterator, user);

            if (err != Error.Ok)
                throw new FreeTypeException(err);
        }

        /// <summary>
        /// Destroy all elements in the list as well as the list itself.
        /// </summary>
        /// <remarks>
        /// This function expects that all nodes added by <see cref="Add" /> or <see cref="Insert" /> have been
        /// dynamically allocated.
        /// </remarks>
        /// <param name="destroy">A list destructor that will be applied to each element of the list.</param>
        /// <param name="memory">The current memory object which handles deallocation.</param>
        /// <param name="user">A user-supplied field which is passed as the last argument to the destructor.</param>
        public void Finalize(ListDestructor destroy, Memory memory, IntPtr user)
        {
            FT.FT_List_Finalize(Reference, destroy, memory.Reference, user);
        }

        #endregion
    }
}