#region Using

using System;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont.TrueType
{
    /// <summary>
    ///     <para>
    ///     A structure used to model a TrueType OS/2 table. This is the long table version. All fields comply to the
    ///     TrueType specification.
    ///     </para>
    ///     <para>
    ///     Note that we now support old Mac fonts which do not include an OS/2 table. In this case, the ‘version’ field is
    ///     always set to 0xFFFF.
    ///     </para>
    /// </summary>
    public class OS2
    {
        #region Fields

        private IntPtr reference;
        private OS2Rec rec;

        #endregion

        #region Constructors

        internal OS2(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The version of this table.
        /// </summary>

        public ushort Version
        {
            get => rec.version;
        }

        /// <summary>
        /// The average glyph width, computed by averaging ALL non-zero width glyphs in the font, in pels/em.
        /// </summary>
        public short AverageCharWidth
        {
            get => rec.xAvgCharWidth;
        }

        /// <summary>
        /// The visual weight of the font.
        /// </summary>

        public ushort WeightClass
        {
            get => rec.usWeightClass;
        }

        /// <summary>
        /// The relative change in width from the normal aspect ratio.
        /// </summary>

        public ushort WidthClass
        {
            get => rec.usWidthClass;
        }

        /// <summary>
        /// Font embedding and subsetting licensing rights as determined by the font author.
        /// </summary>

        public EmbeddingTypes EmbeddingType
        {
            get => rec.fsType;
        }

        /// <summary>
        /// The font author's recommendation for sizing glyphs (em square) to create subscripts when a glyph doesn't exist for a
        /// subscript.
        /// </summary>
        public short SubscriptSizeX
        {
            get => rec.ySubscriptXSize;
        }

        /// <summary>
        /// The font author's recommendation for sizing glyphs (em height) to create subscripts when a glyph doesn't exist for a
        /// subscript.
        /// </summary>
        public short SubscriptSizeY
        {
            get => rec.ySubscriptYSize;
        }

        /// <summary>
        /// The font author's recommendation for vertically positioning subscripts that are created when a glyph doesn't exist for
        /// a subscript.
        /// </summary>
        public short SubscriptOffsetX
        {
            get => rec.ySubscriptXOffset;
        }

        /// <summary>
        /// The font author's recommendation for horizontally positioning subscripts that are created when a glyph doesn't exist
        /// for a subscript.
        /// </summary>
        public short SubscriptOffsetY
        {
            get => rec.ySubscriptYOffset;
        }

        /// <summary>
        /// The font author's recommendation for sizing glyphs (em square) to create superscripts when a glyph doesn't exist for a
        /// subscript.
        /// </summary>
        public short SuperscriptSizeX
        {
            get => rec.ySuperscriptXSize;
        }

        /// <summary>
        /// The font author's recommendation for sizing glyphs (em height) to create superscripts when a glyph doesn't exist for a
        /// subscript.
        /// </summary>
        public short SuperscriptSizeY
        {
            get => rec.ySuperscriptYSize;
        }

        /// <summary>
        /// The font author's recommendation for vertically positioning superscripts that are created when a glyph doesn't exist
        /// for a subscript.
        /// </summary>
        public short SuperscriptOffsetX
        {
            get => rec.ySuperscriptXOffset;
        }

        /// <summary>
        /// The font author's recommendation for horizontally positioning superscripts that are created when a glyph doesn't exist
        /// for a subscript.
        /// </summary>
        public short SuperscriptOffsetY
        {
            get => rec.ySuperscriptYOffset;
        }

        /// <summary>
        /// The thickness of the strikeout stroke.
        /// </summary>
        public short StrikeoutSize
        {
            get => rec.yStrikeoutSize;
        }

        /// <summary>
        /// The position of the top of the strikeout line relative to the baseline.
        /// </summary>
        public short StrikeoutPosition
        {
            get => rec.yStrikeoutPosition;
        }

        /// <summary>
        /// The IBM font family class and subclass, useful for choosing visually similar fonts.
        /// </summary>
        /// <remarks>Refer to https://www.microsoft.com/typography/otspec160/ibmfc.htm. </remarks>
        public short FamilyClass
        {
            get => rec.sFamilyClass;
        }

        //TODO write a PANOSE class from TrueType spec?
        /// <summary>
        /// The Panose values describe visual characteristics of the font.
        /// Similar fonts can then be selected based on their Panose values.
        /// </summary>
        public byte[] Panose
        {
            get => rec.panose;
        }

        /// <summary>
        /// Unicode character range, bits 0-31.
        /// </summary>

        public uint UnicodeRange1
        {
            get => (uint) rec.ulUnicodeRange1;
        }

        /// <summary>
        /// Unicode character range, bits 32-63.
        /// </summary>

        public uint UnicodeRange2
        {
            get => (uint) rec.ulUnicodeRange2;
        }

        /// <summary>
        /// Unicode character range, bits 64-95.
        /// </summary>

        public uint UnicodeRange3
        {
            get => (uint) rec.ulUnicodeRange3;
        }

        /// <summary>
        /// Unicode character range, bits 96-127.
        /// </summary>

        public uint UnicodeRange4
        {
            get => (uint) rec.ulUnicodeRange4;
        }

        /// <summary>
        /// The vendor's identifier.
        /// </summary>
        public byte[] VendorId
        {
            get => rec.achVendID;
        }

        /// <summary>
        /// Describes variations in the font.
        /// </summary>

        public ushort SelectionFlags
        {
            get => rec.fsSelection;
        }

        /// <summary>
        /// The minimum Unicode index (character code) in this font.
        /// Since this value is limited to 0xFFFF, applications should not use this field.
        /// </summary>

        public ushort FirstCharIndex
        {
            get => rec.usFirstCharIndex;
        }

        /// <summary>
        /// The maximum Unicode index (character code) in this font.
        /// Since this value is limited to 0xFFFF, applications should not use this field.
        /// </summary>

        public ushort LastCharIndex
        {
            get => rec.usLastCharIndex;
        }

        /// <summary>
        /// The ascender value, useful for computing a default line spacing in conjunction with unitsPerEm.
        /// </summary>
        public short TypographicAscender
        {
            get => rec.sTypoAscender;
        }

        /// <summary>
        /// The descender value, useful for computing a default line spacing in conjunction with unitsPerEm.
        /// </summary>
        public short TypographicDescender
        {
            get => rec.sTypoDescender;
        }

        /// <summary>
        /// The line gap value, useful for computing a default line spacing in conjunction with unitsPerEm.
        /// </summary>
        public short TypographicLineGap
        {
            get => rec.sTypoLineGap;
        }

        /// <summary>
        /// The ascender metric for Windows, usually set to yMax. Windows will clip glyphs that go above this value.
        /// </summary>

        public ushort WindowsAscent
        {
            get => rec.usWinAscent;
        }

        /// <summary>
        /// The descender metric for Windows, usually set to yMin. Windows will clip glyphs that go below this value.
        /// </summary>

        public ushort WindowsDescent
        {
            get => rec.usWinDescent;
        }

        /// <summary>
        /// Specifies the code pages encompassed by this font.
        /// </summary>

        public uint CodePageRange1
        {
            get => (uint) rec.ulCodePageRange1;
        }

        /// <summary>
        /// Specifies the code pages encompassed by this font.
        /// </summary>

        public uint CodePageRange2
        {
            get => (uint) rec.ulUnicodeRange1;
        }

        /// <summary>
        /// The approximate height of non-ascending lowercase letters relative to the baseline.
        /// </summary>
        public short Height
        {
            get => rec.sxHeight;
        }

        /// <summary>
        /// The approximate height of uppercase letters relative to the baseline.
        /// </summary>
        public short CapHeight
        {
            get => rec.sCapHeight;
        }

        /// <summary>
        /// The Unicode index (character code)  of the glyph to use when a glyph doesn't exist for the requested character.
        /// Since this value is limited to 0xFFFF, applications should not use this field.
        /// </summary>

        public ushort DefaultChar
        {
            get => rec.usDefaultChar;
        }

        /// <summary>
        /// The Unicode index (character code)  of the glyph to use as the break character.
        /// The 'space' character is normally the break character.
        /// Since this value is limited to 0xFFFF, applications should not use this field.
        /// </summary>

        public ushort BreakChar
        {
            get => rec.usBreakChar;
        }

        /// <summary>
        /// The maximum number of characters needed to determine glyph context when applying features such as ligatures.
        /// </summary>

        public ushort MaxContext
        {
            get => rec.usMaxContext;
        }

        /// <summary>
        /// The lowest point size at which the font starts to be used, in twips.
        /// </summary>

        public ushort LowerOpticalPointSize
        {
            get => rec.usLowerOpticalPointSize;
        }

        /// <summary>
        /// The highest point size at which the font is no longer used, in twips.
        /// </summary>

        public ushort UpperOpticalPointSize
        {
            get => rec.usUpperOpticalPointSize;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<OS2Rec>(reference);
            }
        }

        #endregion
    }
}