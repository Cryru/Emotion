#region Using

using System;
using SharpFont.PostScript.Internal;

#endregion

namespace SharpFont.PostScript
{
    /// <summary>
    /// A structure used to represent CID Face information.
    /// </summary>
    public class FaceInfo
    {
        #region Fields

        private IntPtr reference;
        private FaceInfoRec rec;

        #endregion

        #region Constructors

        internal FaceInfo(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the font, usually condensed from FullName.
        /// </summary>
        public string CidFontName
        {
            get => rec.cid_font_name;
        }

        /// <summary>
        /// The version number of the font.
        /// </summary>
        public int CidVersion
        {
            get => (int) rec.cid_version;
        }

        /// <summary>
        /// Gets the string identifying the font's manufacturer.
        /// </summary>
        public string Registry
        {
            get => rec.registry;
        }

        /// <summary>
        /// Gets the unique identifier for the character collection.
        /// </summary>
        public string Ordering
        {
            get => rec.ordering;
        }

        /// <summary>
        /// Gets the identifier (supplement number) of the character collection.
        /// </summary>
        public int Supplement
        {
            get => rec.supplement;
        }

        /// <summary>
        /// Gets the dictionary of font info that is not used by the PostScript interpreter.
        /// </summary>
        public FontInfo FontInfo
        {
            get => new FontInfo(rec.font_info);
        }

        /// <summary>
        /// Gets the coordinates of the corners of the bounding box.
        /// </summary>
        public BBox FontBBox
        {
            get => rec.font_bbox;
        }

        /// <summary>
        /// Gets the value to form UniqueID entries for base fonts within a composite font.
        /// </summary>

        public uint UidBase
        {
            get => (uint) rec.uid_base;
        }

        /// <summary>
        /// Gets the number of entries in the XUID array.
        /// </summary>
        public int XuidCount
        {
            get => rec.num_xuid;
        }

        /// <summary>
        /// Gets the extended unique IDS that identify the form, which allows
        /// the PostScript interpreter to cache the output for reuse.
        /// </summary>

        public uint[] Xuid
        {
            get
            {
                uint[] xuid = new uint[rec.xuid.Length];
                for (int i = 0; i < xuid.Length; i++)
                {
                    xuid[i] = (uint) rec.xuid[i];
                }

                return xuid;
            }
        }

        /// <summary>
        /// Gets the offset in bytes to the charstring offset table.
        /// </summary>

        public uint CidMapOffset
        {
            get => (uint) rec.cidmap_offset;
        }

        /// <summary>
        /// Gets the length in bytes of the FDArray index.
        /// A value of 0 indicates that the charstring offset table doesn't contain
        /// any FDArray indexes.
        /// </summary>
        public int FDBytes
        {
            get => rec.fd_bytes;
        }

        /// <summary>
        /// Gets the length of the offset to the charstring for each CID in the CID font.
        /// </summary>
        public int GDBytes
        {
            get => rec.gd_bytes;
        }

        /// <summary>
        /// Gets the number of valid CIDs in the CIDFont.
        /// </summary>

        public uint CidCount
        {
            get => (uint) rec.cid_count;
        }

        /// <summary>
        /// Gets the number of entries in the FontDicts array.
        /// </summary>
        public int DictsCount
        {
            get => rec.num_dicts;
        }

        /// <summary>
        /// Gets the set of font dictionaries for this font.
        /// </summary>
        public FaceDict FontDicts
        {
            get => new FaceDict(PInvokeHelper.AbsoluteOffsetOf<FaceInfoRec>(Reference, "font_dicts"));
        }

        /// <summary>
        /// The offset of the data.
        /// </summary>

        public uint DataOffset
        {
            get => (uint) rec.data_offset;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<FaceInfoRec>(reference);
            }
        }

        #endregion
    }
}