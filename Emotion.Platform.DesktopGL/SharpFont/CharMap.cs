#region Using

using System;
using SharpFont.Internal;
using SharpFont.TrueType;

#endregion

namespace SharpFont
{
    /// <summary>
    /// The base charmap structure.
    /// </summary>
    public sealed class CharMap
    {
        #region Fields

        private IntPtr reference;
        private CharMapRec rec;

        #endregion

        #region Constructors

        internal CharMap(IntPtr reference, Face parent)
        {
            Reference = reference;
            Face = parent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a handle to the parent face object.
        /// </summary>
        public Face Face { get; }

        /// <summary>
        /// Gets an <see cref="Encoding" /> tag identifying the charmap. Use this with
        /// <see cref="SharpFont.Face.SelectCharmap" />.
        /// </summary>
        public Encoding Encoding
        {
            get => rec.encoding;
        }

        /// <summary>
        /// Gets an ID number describing the platform for the following encoding ID. This comes directly from the
        /// TrueType specification and should be emulated for other formats.
        /// </summary>

        public PlatformId PlatformId
        {
            get => rec.platform_id;
        }

        /// <summary>
        /// Gets a platform specific encoding number. This also comes from the TrueType specification and should be
        /// emulated similarly.
        /// </summary>

        public ushort EncodingId
        {
            get => rec.encoding_id;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<CharMapRec>(reference);
            }
        }

        #endregion

        #region Methods

        #region Base Interface

        /// <summary>
        /// Retrieve index of a given charmap.
        /// </summary>
        /// <returns>The index into the array of character maps within the face to which ‘charmap’ belongs.</returns>
        public int GetCharmapIndex()
        {
            return FT.FT_Get_Charmap_Index(Reference);
        }

        #endregion

        #region TrueType Tables

        /// <summary>
        /// Return TrueType/sfnt specific cmap language ID. Definitions of language ID values are in
        /// ‘freetype/ttnameid.h’.
        /// </summary>
        /// <returns>
        /// The language ID of ‘charmap’. If ‘charmap’ doesn't belong to a TrueType/sfnt face, just return 0 as the
        /// default value.
        /// </returns>
        public uint GetCMapLanguageId()
        {
            return FT.FT_Get_CMap_Language_ID(Reference);
        }

        /// <summary>
        /// Return TrueType/sfnt specific cmap format.
        /// </summary>
        /// <returns>The format of ‘charmap’. If ‘charmap’ doesn't belong to a TrueType/sfnt face, return -1.</returns>
        public int GetCMapFormat()
        {
            return FT.FT_Get_CMap_Format(Reference);
        }

        #endregion

        #endregion
    }
}