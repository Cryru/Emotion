#region Using

using System;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont.TrueType
{
    /// <summary>
    /// A structure used to model a TrueType PCLT table. All fields comply to the TrueType specification.
    /// </summary>
    public class Pclt
    {
        #region Fields

        private IntPtr reference;
        private PCLTRec rec;

        #endregion

        #region Constructors

        internal Pclt(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The version number of this table. Version 1.0 is represented as 0x00010000.
        /// </summary>
        public int Version
        {
            get => (int) rec.Version;
        }

        /// <summary>
        /// A unique identifier for the font. Refer to the specification for the meaning of various bits.
        /// </summary>

        public uint FontNumber
        {
            get => (uint) rec.FontNumber;
        }

        /// <summary>
        /// The width of the space character, in FUnits (see UnitsPerEm in the head table).
        /// </summary>

        public ushort Pitch
        {
            get => rec.Pitch;
        }

        /// <summary>
        /// The height of the optical height of the lowercase 'x' in FUnits.
        /// This is a separate value from its height measurement.
        /// </summary>

        public ushort Height
        {
            get => rec.xHeight;
        }

        /// <summary>
        /// Describes structural appearance and effects of letterforms.
        /// </summary>

        public ushort Style
        {
            get => rec.Style;
        }

        /// <summary>
        /// Encodes the font vendor code and font family code into 16 bits.
        /// Refer to the spec for details.
        /// </summary>

        public ushort TypeFamily
        {
            get => rec.TypeFamily;
        }

        /// <summary>
        /// The height of the optical height of the uppercase 'H' in FUnits.
        /// This is a separate value from its height measurement.
        /// </summary>

        public ushort CapHeight
        {
            get => rec.CapHeight;
        }

        /// <summary>
        /// Encodes the symbol set's number field and ID field.
        /// Refer to the spec for details.
        /// </summary>

        public ushort SymbolSet
        {
            get => rec.SymbolSet;
        }

        /// <summary>
        /// The name and style of the font. The names of fonts within a family should be identical and the
        /// style identifiers should be standardized: e.g., Bd, It, BdIt. Length is 16 bytes.
        /// </summary>
        public string Typeface
        {
            get => rec.TypeFace;
        }

        /// <summary>
        /// Identifies the symbol collections provided by the font. Length is 8 bytes.
        /// Refer to the spec for details.
        /// </summary>
        public byte[] CharacterComplement
        {
            get => rec.CharacterComplement;
        }

        /// <summary>
        /// A standardized filename of the font. Length is 6 bytes.
        /// Refer to the spec for details.
        /// </summary>
        public byte[] FileName
        {
            get => rec.FileName;
        }

        /// <summary>
        /// Indicates the stroke weight. Valid values are in the range -7 to 7. Length is 1 byte.
        /// </summary>
        public byte StrokeWeight
        {
            get => rec.StrokeWeight;
        }

        /// <summary>
        /// Indicates the stroke weight. Valid values are in the range -5 to 5. Length is 1 byte.
        /// </summary>
        public byte WidthType
        {
            get => rec.WidthType;
        }

        /// <summary>
        /// Encodes the serif style. The top two bits indicate sans serif/monoline or serif/contrasting.
        /// Valid values for the lower 6 bits are in the range 0 to 12. Length is 1 byte.
        /// </summary>
        public byte SerifStyle
        {
            get => rec.SerifStyle;
        }

        /// <summary>
        /// Reserved. Set to 0.
        /// </summary>
        public byte Reserved
        {
            get => rec.Reserved;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<PCLTRec>(reference);
            }
        }

        #endregion
    }
}