#region Using

using System;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    ///     <para>
    ///     An opaque handle to a cache node object. Each cache node is reference-counted. A node with a count of 0 might
    ///     be flushed out of a full cache whenever a lookup request is performed.
    ///     </para>
    ///     <para>
    ///     If you look up nodes, you have the ability to ‘acquire’ them, i.e., to increment their reference count. This
    ///     will prevent the node from being flushed out of the cache until you explicitly ‘release’ it.
    ///     </para>
    /// </summary>
    /// <see cref="Node.Unref" />
    /// <seealso cref="SBitCache.Lookup" />
    /// <seealso cref="ImageCache.Lookup" />
    public class Node
    {
        #region Fields

        #endregion

        #region Constructors

        internal Node(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        internal IntPtr Reference { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Decrement a cache node's internal reference count. When the count reaches 0, it is not destroyed but
        /// becomes eligible for subsequent cache flushes.
        /// </summary>
        /// <param name="manager">The cache manager handle.</param>
        public void Unref(Manager manager)
        {
            FT.FTC_Node_Unref(Reference, manager.Reference);
        }

        #endregion
    }
}