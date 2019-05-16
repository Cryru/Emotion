#region Using

using System;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// An opaque handle used to model a charmap cache. This cache is to hold character codes -> glyph indices
    /// mappings.
    /// </summary>
    public class CMapCache
    {
        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CMapCache" /> class.
        /// </summary>
        /// <remarks>
        /// Like all other caches, this one will be destroyed with the cache manager.
        /// </remarks>
        /// <param name="manager">A handle to the cache manager.</param>
        public CMapCache(Manager manager)
        {
            IntPtr cacheRef;
            Error err = FT.FTC_CMapCache_New(manager.Reference, out cacheRef);

            if (err != Error.Ok)
                throw new FreeTypeException(err);

            Reference = cacheRef;
        }

        #endregion

        #region Properties

        internal IntPtr Reference { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Translate a character code into a glyph index, using the charmap cache.
        /// </summary>
        /// <param name="faceId">The source face ID.</param>
        /// <param name="cmapIndex">
        /// The index of the charmap in the source face. Any negative value means to use the cache <see cref="Face" />'s
        /// default charmap.
        /// </param>
        /// <param name="charCode">The character code (in the corresponding charmap).</param>
        /// <returns>Glyph index. 0 means ‘no glyph’.</returns>
        public uint Lookup(IntPtr faceId, int cmapIndex, uint charCode)
        {
            return FT.FTC_CMapCache_Lookup(Reference, faceId, cmapIndex, charCode);
        }

        #endregion
    }
}