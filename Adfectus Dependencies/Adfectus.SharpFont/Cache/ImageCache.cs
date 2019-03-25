#region Using

using System;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A handle to an glyph image cache object. They are designed to hold many distinct glyph images while not
    /// exceeding a certain memory threshold.
    /// </summary>
    public class ImageCache
    {
        #region Fields

        private IntPtr reference;
        private Manager parentManager;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCache" /> class.
        /// </summary>
        /// <param name="manager">The parent manager for the image cache.</param>
        public ImageCache(Manager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            IntPtr cacheRef;
            Error err = FT.FTC_ImageCache_New(manager.Reference, out cacheRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            parentManager = manager;
            Reference = cacheRef;
        }

        #endregion

        #region Properties

        internal IntPtr Reference
        {
            get
            {
                if (parentManager.IsDisposed)
                    throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

                return reference;
            }

            set
            {
                if (parentManager.IsDisposed)
                    throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

                reference = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieve a given glyph image from a glyph image cache.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     The returned glyph is owned and managed by the glyph image cache. Never try to transform or discard it
        ///     manually! You can however create a copy with <see cref="Glyph.Copy" /> and modify the new one.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is not NULL, it receives the address of the cache node containing the glyph image,
        ///     after increasing its reference count. This ensures that the node (as well as the <see cref="Glyph" />) will
        ///     always be kept in the cache until you call <see cref="Node.Unref" /> to ‘release’ it.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is NULL, the cache node is left unchanged, which means that the <see cref="Glyph" />
        ///     could be flushed out of the cache on the next call to one of the caching sub-system APIs. Don't assume that
        ///     it is persistent!
        ///     </para>
        /// </remarks>
        /// <param name="type">A pointer to a glyph image type descriptor.</param>
        /// <param name="gIndex">The glyph index to retrieve.</param>
        /// <param name="node">
        /// Used to return the address of of the corresponding cache node after incrementing its reference count (see
        /// note below).
        /// </param>
        /// <returns>The corresponding <see cref="Glyph" /> object. 0 in case of failure.</returns>
        public Glyph Lookup(ImageType type, uint gIndex, out Node node)
        {
            if (parentManager.IsDisposed)
                throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

            IntPtr glyphRef, nodeRef;
            Error err = FT.FTC_ImageCache_Lookup(Reference, type.Reference, gIndex, out glyphRef, out nodeRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            node = new Node(nodeRef);
            return new Glyph(glyphRef, null);
        }

        /// <summary>
        /// A variant of <see cref="ImageCache.Lookup" /> that uses a <see cref="Scaler" /> to specify the face ID and its
        /// size.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     The returned glyph is owned and managed by the glyph image cache. Never try to transform or discard it
        ///     manually! You can however create a copy with <see cref="Glyph.Copy" /> and modify the new one.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is not NULL, it receives the address of the cache node containing the glyph image,
        ///     after increasing its reference count. This ensures that the node (as well as the <see cref="Glyph" />) will
        ///     always be kept in the cache until you call <see cref="Node.Unref" /> to ‘release’ it.
        ///     </para>
        ///     <para>
        ///     If ‘node’ is NULL, the cache node is left unchanged, which means that the <see cref="Glyph" />
        ///     could be flushed out of the cache on the next call to one of the caching sub-system APIs. Don't assume that
        ///     it is persistent!
        ///     </para>
        ///     <para>
        ///     Calls to <see cref="Face.SetCharSize" /> and friends have no effect on cached glyphs; you should always use
        ///     the FreeType cache API instead.
        ///     </para>
        /// </remarks>
        /// <param name="scaler">A pointer to a scaler descriptor.</param>
        /// <param name="loadFlags">The corresponding load flags.</param>
        /// <param name="gIndex">The glyph index to retrieve.</param>
        /// <param name="node">
        /// Used to return the address of of the corresponding cache node after incrementing its reference count (see
        /// note below).
        /// </param>
        /// <returns>The corresponding <see cref="Glyph" /> object. 0 in case of failure.</returns>
        public Glyph LookupScaler(Scaler scaler, LoadFlags loadFlags, uint gIndex, out Node node)
        {
            if (parentManager.IsDisposed)
                throw new ObjectDisposedException("Reference", "Cannot access a disposed object.");

            IntPtr glyphRef, nodeRef;
            Error err = FT.FTC_ImageCache_LookupScaler(Reference, scaler.Reference, loadFlags, gIndex, out glyphRef, out nodeRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            node = new Node(nodeRef);
            return new Glyph(glyphRef, null);
        }

        #endregion
    }
}