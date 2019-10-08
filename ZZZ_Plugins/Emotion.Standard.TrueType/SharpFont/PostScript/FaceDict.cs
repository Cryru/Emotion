#region Using

using System;
using SharpFont.PostScript.Internal;

#endregion

namespace SharpFont.PostScript
{
    /// <summary>
    /// A structure used to represent data in a CID top-level dictionary.
    /// </summary>
    public class FaceDict
    {
        #region Fields

        private IntPtr reference;
        private FaceDictRec rec;

        #endregion

        #region Constructors

        internal FaceDict(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Private structure containing more information.
        /// </summary>
        public Private PrivateDictionary
        {
            get => new Private(rec.private_dict);
        }

        /// <summary>
        /// Gets the length of the BuildChar entry.
        /// </summary>

        public uint BuildCharLength
        {
            get => rec.len_buildchar;
        }

        /// <summary>
        /// Gets whether to force bold characters when a regular character has
        /// strokes drawn 1-pixel wide.
        /// </summary>
        public int ForceBoldThreshold
        {
            get => (int) rec.forcebold_threshold;
        }

        /// <summary>
        /// Gets the width of stroke.
        /// </summary>
        public int StrokeWidth
        {
            get => (int) rec.stroke_width;
        }

        /// <summary>
        /// Gets hinting useful for rendering glyphs such as barcodes and logos that
        /// have many counters.
        /// </summary>
        public int ExpansionFactor
        {
            get => (int) rec.expansion_factor;
        }

        /// <summary>
        /// Gets the method for painting strokes (fill or outline).
        /// </summary>
        public byte PaintType
        {
            get => rec.paint_type;
        }

        /// <summary>
        /// Gets the type of font. Must be set to 1 for all Type 1 fonts.
        /// </summary>
        public byte FontType
        {
            get => rec.font_type;
        }

        /// <summary>
        /// Gets the matrix that indicates scaling of space units.
        /// </summary>
        public FTMatrix FontMatrix
        {
            get => rec.font_matrix;
        }

        /// <summary>
        /// Gets the offset of the font.
        /// </summary>
        public FTVector FontOffset
        {
            get => rec.font_offset;
        }

        /// <summary>
        /// Gets the number of subroutines.
        /// </summary>

        public uint SubrsCount
        {
            get => rec.num_subrs;
        }

        /// <summary>
        /// Gets the offset in bytes, from the start of the
        /// data section of the CIDFont to the beginning of the SubrMap.
        /// </summary>

        public uint SubrmapOffset
        {
            get => (uint) rec.subrmap_offset;
        }

        /// <summary>
        /// Gets the number of bytes needed to store the SD value.
        /// </summary>
        public int SDBytes
        {
            get => rec.sd_bytes;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<FaceDictRec>(reference);
            }
        }

        #endregion
    }
}