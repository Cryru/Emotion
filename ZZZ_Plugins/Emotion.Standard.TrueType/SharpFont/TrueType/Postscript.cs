#region Using

using System;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont.TrueType
{
    /// <summary>
    /// A structure used to model a TrueType PostScript table. All fields comply to the TrueType specification. This
    /// structure does not reference the PostScript glyph names, which can be nevertheless accessed with the ‘ttpost’
    /// module.
    /// </summary>
    public class Postscript
    {
        #region Fields

        private IntPtr reference;
        private PostscriptRec rec;

        #endregion

        #region Constructors

        internal Postscript(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the version of the table information.
        /// </summary>
        public int FormatType
        {
            get => (int) rec.FormatType;
        }

        /// <summary>
        /// Gets the angle of italics, in degrees, counter-clockwise from vertical.
        /// </summary>
        public int ItalicAngle
        {
            get => (int) rec.italicAngle;
        }

        /// <summary>
        /// Gets the recommended position of the underline.
        /// </summary>
        public short UnderlinePosition
        {
            get => rec.underlinePosition;
        }

        /// <summary>
        /// Gets the recommended thickness of the underline.
        /// </summary>
        public short UnderlineThickness
        {
            get => rec.underlineThickness;
        }

        /// <summary>
        /// </summary>

        public uint IsFixedPitch
        {
            get => (uint) rec.isFixedPitch;
        }

        /// <summary>
        /// Gets the minimum amount of memory used by the font when an OpenType font is loaded.
        /// </summary>

        public uint MinimumMemoryType42
        {
            get => (uint) rec.minMemType42;
        }

        /// <summary>
        /// Gets the maximum amount of memory used by the font when an OpenType font is loaded.
        /// </summary>

        public uint MaximumMemoryType42
        {
            get => (uint) rec.maxMemType42;
        }

        /// <summary>
        /// Gets the minimum amount of memory used by the font when an OpenType font is loaded as
        /// a Type 1 font.
        /// </summary>

        public uint MinimumMemoryType1
        {
            get => (uint) rec.minMemType1;
        }

        /// <summary>
        /// Gets the maximum amount of memory used by the font when an OpenType font is loaded as
        /// a Type 1 font.
        /// </summary>

        public uint MaximumMemoryType1
        {
            get => (uint) rec.maxMemType1;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<PostscriptRec>(reference);
            }
        }

        #endregion
    }
}