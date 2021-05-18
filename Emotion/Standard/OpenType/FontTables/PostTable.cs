#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/post
    /// </summary>
    public class PostTable
    {
        // ReSharper disable StringLiteralTypo
        public static string[] StandardNames =
        {
            ".notdef", ".null", "nonmarkingreturn", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent",
            "ampersand", "quotesingle", "parenleft", "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash",
            "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less",
            "equal", "greater", "question", "at", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
            "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright",
            "asciicircum", "underscore", "grave", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o",
            "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde",
            "Adieresis", "Aring", "Ccedilla", "Eacute", "Ntilde", "Odieresis", "Udieresis", "aacute", "agrave",
            "acircumflex", "adieresis", "atilde", "aring", "ccedilla", "eacute", "egrave", "ecircumflex", "edieresis",
            "iacute", "igrave", "icircumflex", "idieresis", "ntilde", "oacute", "ograve", "ocircumflex", "odieresis",
            "otilde", "uacute", "ugrave", "ucircumflex", "udieresis", "dagger", "degree", "cent", "sterling", "section",
            "bullet", "paragraph", "germandbls", "registered", "copyright", "trademark", "acute", "dieresis", "notequal",
            "AE", "Oslash", "infinity", "plusminus", "lessequal", "greaterequal", "yen", "mu", "partialdiff", "summation",
            "product", "pi", "integral", "ordfeminine", "ordmasculine", "Omega", "ae", "oslash", "questiondown",
            "exclamdown", "logicalnot", "radical", "florin", "approxequal", "Delta", "guillemotleft", "guillemotright",
            "ellipsis", "nonbreakingspace", "Agrave", "Atilde", "Otilde", "OE", "oe", "endash", "emdash", "quotedblleft",
            "quotedblright", "quoteleft", "quoteright", "divide", "lozenge", "ydieresis", "Ydieresis", "fraction",
            "currency", "guilsinglleft", "guilsinglright", "fi", "fl", "daggerdbl", "periodcentered", "quotesinglbase",
            "quotedblbase", "perthousand", "Acircumflex", "Ecircumflex", "Aacute", "Edieresis", "Egrave", "Iacute",
            "Icircumflex", "Idieresis", "Igrave", "Oacute", "Ocircumflex", "apple", "Ograve", "Uacute", "Ucircumflex",
            "Ugrave", "dotlessi", "circumflex", "tilde", "macron", "breve", "dotaccent", "ring", "cedilla", "hungarumlaut",
            "ogonek", "caron", "Lslash", "lslash", "Scaron", "scaron", "Zcaron", "zcaron", "brokenbar", "Eth", "eth",
            "Yacute", "yacute", "Thorn", "thorn", "minus", "multiply", "onesuperior", "twosuperior", "threesuperior",
            "onehalf", "onequarter", "threequarters", "franc", "Gbreve", "gbreve", "Idotaccent", "Scedilla", "scedilla",
            "Cacute", "cacute", "Ccaron", "ccaron", "dcroat"
        };
        // ReSharper restore StringLiteralTypo

        public float Version;
        public float ItalicAngle;
        public short UnderlinePosition;
        public short UnderlineThickness;
        public uint FixedPitch;
        public uint MinMemType42;
        public uint MaxMemType42;
        public uint MinMemType1;
        public uint MaxMemType1;

        public string[] Names;

        public ushort NumberOfGlyphs;
        public ushort[] GlyphNameIndex;
        public char[] Offset;

        /// <summary>
        /// Additional information like names.
        /// </summary>
        public PostTable(ByteReader reader)
        {
            Version = reader.ReadOpenTypeVersionBE();
            ItalicAngle = reader.ReadFloatBE();
            UnderlinePosition = reader.ReadShortBE();
            UnderlineThickness = reader.ReadShortBE();
            FixedPitch = reader.ReadULongBE();
            MinMemType42 = reader.ReadULongBE();
            MaxMemType42 = reader.ReadULongBE();
            MinMemType1 = reader.ReadULongBE();
            MaxMemType1 = reader.ReadULongBE();

            switch (Version)
            {
                case 1:
                    Names = StandardNames;
                    break;
                case 2:
                    NumberOfGlyphs = reader.ReadUShortBE();
                    GlyphNameIndex = new ushort[NumberOfGlyphs];
                    for (var i = 0; i < NumberOfGlyphs; i++)
                    {
                        GlyphNameIndex[i] = reader.ReadUShortBE();
                    }

                    Names = new string[NumberOfGlyphs];
                    for (var i = 0; i < NumberOfGlyphs; i++)
                    {
                        if (GlyphNameIndex[i] < StandardNames.Length)
                        {
                            Names[i] = StandardNames[GlyphNameIndex[i]];
                        }
                        else
                        {
                            char nameLength = reader.ReadChar();
                            Names[i] = new string(reader.ReadChars(nameLength));
                        }
                    }

                    break;
                case 2.5f:
                    NumberOfGlyphs = reader.ReadUShortBE();
                    Offset = new char[NumberOfGlyphs];
                    for (var i = 0; i < NumberOfGlyphs; i++)
                    {
                        Offset[i] = reader.ReadChar();
                    }

                    break;
                case 3:
                    // todo
                    break;
            }
        }
    }
}