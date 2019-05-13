#region Using

using System;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A handle to a small bitmap cache. These are special cache objects used to store small glyph bitmaps (and
    /// anti-aliased pixmaps) in a much more efficient way than the traditional glyph image cache implemented by
    /// <see cref="ImageCache" />.
    /// </summary>
    public class SBitCache
    {
        #region Fields

        private Manager parentManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SBitCache" /> class.
        /// </summary>
        /// <param name="manager">A handle to the source cache manager.</param>
        public SBitCache(Manager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            IntPtr cacheRef;
            Error err = FT.FTC_SBitCache_New(manager.Reference, out cacheRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            Reference = cacheRef;
            parentManager = manager;
        }

        #endregion

        #region Properties

        internal IntPtr Reference { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Look up a given small glyph bitmap in a given sbit cache and ‘lock’ it to prevent its flushing from the
        /// cache until needed.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     The small bitmap descriptor and its bit buffer are owned by the cache and should never be freed by the
        ///     application. They might as well disappear from memory on the next cache lookup, so don't treat them as
        ///     persistent data.
        ///     </para>
        ///     <para>
        ///     The descriptor's ‘buffer’ field is set to 0 to indicate a missing glyph bitmap.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is not NULL, it receives the address of the cache node containing the bitmap, after
        ///     increasing its reference count. This ensures that the node (as well as the image) will always be kept in
        ///     the cache until you call <see cref="Node.Unref" /> to ‘release’ it.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is NULL, the cache node is left unchanged, which means that the bitmap could be
        ///     flushed out of the cache on the next call to one of the caching sub-system APIs. Don't assume that it is
        ///     persistent!
        ///     </para>
        /// </remarks>
        /// <param name="type">A pointer to the glyph image type descriptor.</param>
        /// <param name="gIndex">The glyph index.</param>
        /// <param name="node">
        /// Used to return the address of of the corresponding cache node after incrementing its reference count (see
        /// note below).
        /// </param>
        /// <returns>A handle to a small bitmap descriptor.</returns>
        public SBit Lookup(ImageType type, uint gIndex, out Node node)
        {
            if (parentManager.IsDisposed)
                throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

            IntPtr sbitRef, nodeRef;
            Error err = FT.FTC_SBitCache_Lookup(Reference, type.Reference, gIndex, out sbitRef, out nodeRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            node = new Node(nodeRef);
            return new SBit(sbitRef);
        }

        /// <summary>
        /// A variant of <see cref="SBitCache.Lookup" /> that uses a <see cref="Scaler" /> to specify the face ID and its
        /// size.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     The small bitmap descriptor and its bit buffer are owned by the cache and should never be freed by the
        ///     application. They might as well disappear from memory on the next cache lookup, so don't treat them as
        ///     persistent data.
        ///     </para>
        ///     <para>
        ///     The descriptor's ‘buffer’ field is set to 0 to indicate a missing glyph bitmap.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is not NULL, it receives the address of the cache node containing the bitmap, after
        ///     increasing its reference count. This ensures that the node (as well as the image) will always be kept in
        ///     the cache until you call <see cref="Node.Unref" /> to ‘release’ it.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is NULL, the cache node is left unchanged, which means that the bitmap could be
        ///     flushed out of the cache on the next call to one of the caching sub-system APIs. Don't assume that it is
        ///     persistent!
        ///     </para>
        /// </remarks>
        /// <param name="scaler">A pointer to the scaler descriptor.</param>
        /// <param name="loadFlags">The corresponding load flags.</param>
        /// <param name="gIndex">The glyph index.</param>
        /// <param name="node">
        /// Used to return the address of of the corresponding cache node after incrementing its reference count (see
        /// note below).
        /// </param>
        /// <returns>A handle to a small bitmap descriptor.</returns>
        public SBit LookupScaler(Scaler scaler, LoadFlags loadFlags, uint gIndex, out Node node)
        {
            if (parentManager.IsDisposed)
                throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

            IntPtr sbitRef, nodeRef;
            Error err = FT.FTC_SBitCache_LookupScaler(Reference, scaler.Reference, loadFlags, gIndex, out sbitRef, out nodeRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            node = new Node(nodeRef);
            return new SBit(sbitRef);
        }

        #endregion
    }
}