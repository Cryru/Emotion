#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/cff
    /// </summary>
    public class CffTable
    {
        public byte FormatMajor;
        public byte FormatMinor;
        public byte Size;
        public byte OffsetSize;
        public byte StartOffset;
        public byte EndOffset;

        public string[] NameIndex;
        public byte[][] TopDictIndex;
        public string[] StringIndex;

        public byte[][] GlobalSubrIndex;
        public int GlobalBias;

        public Dictionary<string, object> TopDict;
        public int DefaultWidthX;
        public int NominalWidthX;

        public bool IsCidFont;
        public byte[] FdSelect;

        public byte[][] Subr;
        public int SubrBias;

        public int NumberOfGlyphs;

        public byte[][] CharStringIndex;
        public string[] Encoding;
        public string[] Charset;
        public Dictionary<byte, byte> CustomEncoding;

        /// <summary>
        /// Parse the `Cff` table. This table stores glyphs in Cff format fonts.
        /// </summary>
        public CffTable(ByteReader reader)
        {
            // Cff header
            FormatMajor = reader.ReadByte();
            FormatMinor = reader.ReadByte();
            Size = reader.ReadByte();
            OffsetSize = reader.ReadByte();

            ByteReader b = reader.Branch(0, false);
            NameIndex = ReadIndexArray<string>(b);
            TopDictIndex = ReadIndexArray<byte[]>(b);
            StringIndex = ReadIndexArray<string>(b);

            GlobalSubrIndex = ReadIndexArray<byte[]>(b);
            GlobalBias = CalculateSubroutineBias(GlobalSubrIndex);

            Dictionary<string, object>[] topDicts = GatherCffTopDicts(reader.Branch(0, true), TopDictIndex);

            if (topDicts.Length > 1)
            {
                Engine.Log.Warning($"CFF table has many fonts in 'FontSet' - {topDicts.Length} Only the first will be parsed.", MessageSource.FontParser);
            }
            else if (topDicts.Length == 0)
            {
                Engine.Log.Warning("CFF table has no fonts in 'FontSet' - broken font?", MessageSource.FontParser);
                return;
            }

            TopDict = topDicts[0];
            if (TopDict.ContainsKey("_defaultWidthX")) DefaultWidthX = Convert.ToInt32(TopDict["_defaultWidthX"]);
            if (TopDict.ContainsKey("_nominalWidthX")) NominalWidthX = Convert.ToInt32(TopDict["_nominalWidthX"]);

            var ros = (object[]) TopDict["ros"];
            if (ros[0] != null && ros[1] != null) IsCidFont = true;

            if (IsCidFont)
            {
                var fdArrayOffset = Convert.ToInt32(TopDict["fdArray"]);
                var fdSelectOffset = Convert.ToInt32(TopDict["fdSelect"]);

                if (fdArrayOffset == 0 || fdSelectOffset == 0)
                {
                    Engine.Log.Warning("Font is marked as a CID font, but FDArray and/or FDSelect is missing.", MessageSource.FontParser);
                    return;
                }

                // todo
                // ByteReader r = reader.Branch(fdArrayOffset, true);
                // byte[][] fdArrayIndex = ReadIndexArray<byte[]>(r);
                // Dictionary<string, object>[] fd = GatherCffTopDicts(reader.Branch(0, true), fdArrayIndex);

                Debugger.Break();
            }
            else
            {
                FdSelect = new byte[0];
            }

            var offsets = (object[]) TopDict["private"];
            if (offsets.Length > 0)
            {
                var privateDictOffset = Convert.ToInt32(offsets[1]);
                ByteReader privateDictReader = reader.Branch(privateDictOffset, true, Convert.ToInt32(offsets[0]));
                Dictionary<string, object> privateDict = ParseCffTopDict(privateDictReader, _privateDictMeta);

                DefaultWidthX = Convert.ToInt32(privateDict["defaultWidthX"]);
                NominalWidthX = Convert.ToInt32(privateDict["nominalWidthX"]);

                var subrs = Convert.ToInt32(privateDict["subrs"]);

                if (subrs != 0)
                {
                    int subrOffset = privateDictOffset + subrs;
                    byte[][] subrIndex = ReadIndexArray<byte[]>(reader.Branch(subrOffset, true));
                    Subr = subrIndex;
                    SubrBias = CalculateSubroutineBias(subrIndex);
                }
                else
                {
                    Subr = new byte[0][];
                    SubrBias = 0;
                }
            }
            else
            {
                Engine.Log.Warning("Missing second private dictionary.", MessageSource.FontParser);
                return;
            }

            var charStringsOffset = Convert.ToInt32(TopDict["charStrings"]);
            CharStringIndex = ReadIndexArray<byte[]>(reader.Branch(charStringsOffset, true));
            NumberOfGlyphs = CharStringIndex.Length;

            var charSetOffset = Convert.ToInt32(TopDict["charset"]);
            Charset = ParseCffCharset(reader.Branch(charSetOffset, true));

            var encoding = Convert.ToInt32(TopDict["encoding"]);

            switch (encoding)
            {
                // standard
                case 0:
                    Encoding = CffStandardEncoding;
                    break;
                // expert
                case 1:
                    Encoding = CffExpertEncoding;
                    break;
                // wtf
                default:
                    CustomEncoding = ParseCffEncoding(reader.Branch(encoding, true));
                    break;
            }
        }

        // Parse the CFF encoding data. Only one encoding can be specified per font.
        // See Adobe TN #5176 chapter 12, "Encodings".
        private static Dictionary<byte, byte> ParseCffEncoding(ByteReader reader)
        {
            var encoding = new Dictionary<byte, byte>();
            byte format = reader.ReadByte();
            if (format == 0)
            {
                byte nCodes = reader.ReadByte();
                for (byte i = 0; i < nCodes; i++)
                {
                    byte code = reader.ReadByte();
                    encoding[code] = i;
                }
            }
            else if (format == 1)
            {
                byte nRanges = reader.ReadByte();
                byte code = 1;

                for (byte i = 0; i < nRanges; i++)
                {
                    byte first = reader.ReadByte();
                    byte left = reader.ReadByte();

                    for (byte j = 0; j <= first + left; j++)
                    {
                        encoding[j] = code;
                        code++;
                    }
                }
            }
            else
            {
                Engine.Log.Warning($"Unknown font encoding {format}", MessageSource.FontParser);
            }

            return encoding;
        }

        // Parse the CFF charset table, which contains internal names for all the glyphs.
        // This function will return a list of glyph names.
        // See Adobe TN #5176 chapter 13, "Charsets".
        private string[] ParseCffCharset(ByteReader reader)
        {
            // The .notdef glyph is not included, so subtract 1.
            int numGlyphs = NumberOfGlyphs - 1;
            var charSet = new List<string> {".notdef"};

            byte format = reader.ReadByte();
            ushort sid;
            ushort count;
            switch (format)
            {
                case 0:

                    for (var i = 0; i < numGlyphs; i++)
                    {
                        sid = reader.ReadUShortBE();
                        charSet.Add(GetCffString(StringIndex, sid));
                    }

                    break;
                case 1:

                    while (charSet.Count <= numGlyphs)
                    {
                        sid = reader.ReadUShortBE();
                        count = reader.ReadByte();

                        for (var i = 0; i <= count; i++)
                        {
                            charSet.Add(GetCffString(StringIndex, sid));
                            sid += 1;
                        }
                    }

                    break;

                case 2:

                    while (charSet.Count <= numGlyphs)
                    {
                        sid = reader.ReadUShortBE();
                        count = reader.ReadUShortBE();

                        for (var i = 0; i <= count; i++)
                        {
                            charSet.Add(GetCffString(StringIndex, sid));
                            sid += 1;
                        }
                    }

                    break;
                default:
                    Engine.Log.Warning($"Unknown charset format{format}", MessageSource.FontParser);
                    break;
            }

            return charSet.ToArray();
        }

        // Subroutines are encoded using the negative half of the number space.
        // See type 2 chapter 4.7 "Subroutine operators".
        private static int CalculateSubroutineBias<T>(T[] arr)
        {
            if (arr.Length < 1240)
                return 107;
            return arr.Length < 33900 ? 1131 : 32768;
        }

        private T[] ReadIndexArray<T>(ByteReader reader)
        {
            int count = reader.ReadUShortBE();
            var offsets = new List<uint>();
            var objectOffset = 0;
            var objects = new List<T>();

            if (count != 0)
            {
                byte offsetSize = reader.ReadByte();
                objectOffset = reader.Position - 1 + (count + 1) * offsetSize;
                for (var i = 0; i <= count; i++)
                {
                    switch (offsetSize)
                    {
                        case 1:
                            offsets.Add(reader.ReadByte());
                            break;
                        case 2:
                            offsets.Add(reader.ReadUShortBE());
                            break;
                        case 3:
                            offsets.Add(reader.ReadUInt24BE());
                            break;
                        case 4:
                            offsets.Add(reader.ReadUIntBE());
                            break;
                    }
                }

                reader.Position = objectOffset + (int) offsets[^1];
            }

            ByteReader objectReader = reader.Branch(objectOffset, true);
            for (var i = 0; i < offsets.Count - 1; i++)
            {
                objectReader.Position = 0;

                objectReader.ReadBytes((int) offsets[i]);
                ReadOnlySpan<byte> bytes = objectReader.ReadBytes((int) (offsets[i + 1] - offsets[i]));

                if (typeof(T) == typeof(string))
                    objects.Add((T) Convert.ChangeType(System.Text.Encoding.ASCII.GetString(bytes), typeof(T)));
                else
                    objects.Add((T) Convert.ChangeType(bytes.ToArray(), typeof(T)));
            }

            return objects.ToArray();
        }

        // Returns a list of "Top DICT"s found using an INDEX list.
        // Used to read both the usual high-level Top DICTs and also the FDArray
        // discovered inside CID-keyed fonts.  When a Top DICT has a reference to
        // a Private DICT that is read and saved into the Top DICT.
        //
        // In addition to the expected/optional values as outlined in TOP_DICT_META
        // the following values might be saved into the Top DICT.
        //
        //    _subrs []        array of local CFF subroutines from Private DICT
        //    _subrsBias       bias value computed from number of subroutines
        //                      (see calcCFFSubroutineBias() and parseCFFCharstring())
        //    _defaultWidthX   default widths for CFF characters
        //    _nominalWidthX   bias added to width embedded within glyph description
        //
        //    _privateDict     saved copy of parsed Private DICT from Top DICT
        private Dictionary<string, object>[] GatherCffTopDicts(ByteReader reader, byte[][] index)
        {
            var topDicts = new Dictionary<string, object>[index.Length];

            for (var i = 0; i < index.Length; i++)
            {
                Dictionary<string, object> topDict = ParseCffTopDict(new ByteReader(index[i]), _topDictMeta);

                var privateData = (object[]) topDict["private"];

                var privateSize = Convert.ToInt32(privateData[0]);
                var privateOffset = Convert.ToInt32(privateData[1]);

                if (privateSize != 0 && privateOffset != 0)
                {
                    Dictionary<string, object> privateDict = ParseCffTopDict(reader.Branch(privateOffset, true, privateSize), _privateDictMeta);
                    topDict["_defaultWidthX"] = privateDict["defaultWidthX"];
                    topDict["_nominalWidthX"] = privateDict["nominalWidthX"];

                    var subrs = Convert.ToInt32(privateDict["subrs"]);
                    if (subrs != 0)
                    {
                        int subROffset = privateOffset + subrs;
                        byte[][] subrIndex = ReadIndexArray<byte[]>(reader.Branch(subROffset, true));
                        topDict["_subrs"] = subrIndex;
                        topDict["_subrsBias"] = CalculateSubroutineBias(subrIndex);
                    }
                    else
                    {
                        topDict["_subrs"] = new object[0];
                        topDict["_subrsBias"] = 0;
                    }

                    topDict["_privateDict"] = privateDict;
                }

                topDicts[i] = topDict;
            }

            return topDicts;
        }

        // Parse the CFF top dictionary. A CFF table can contain multiple fonts, each with their own top dictionary.
        // The top dictionary contains the essential metadata for the font, together with the private dictionary.
        private Dictionary<string, object> ParseCffTopDict(ByteReader reader, DictMeta[] meta)
        {
            var entries = new Dictionary<int, float[]>();
            var operands = new List<float>();

            while (reader.Position != reader.Data.Length)
            {
                int op = reader.ReadByte();

                // The first byte for each dict item distinguishes between operator (key) and operand (value).
                // Values <= 21 are operators.
                if (op <= 21)
                {
                    // Two-byte operators have an initial escape byte of 12.
                    if (op == 12) op = 1200 + reader.ReadByte();

                    entries.Add(op, operands.ToArray());
                    operands.Clear();
                }
                else
                {
                    // Since the operands (values) come before the operators (keys), we store all operands in a list
                    // until we encounter an operator.
                    operands.Add(ParseOperand(reader, op));
                }
            }

            // Interpret a dictionary and return it with readable keys and values for missing entries.
            var newDict = new Dictionary<string, object>();
            foreach (DictMeta m in meta)
            {
                string[] types = m.Type.Split(",");
                var values = new object[types.Length];

                for (var j = 0; j < types.Length; j++)
                {
                    if (entries.ContainsKey(m.Op))
                        if (entries[m.Op].Length - 1 >= j)
                            values[j] = entries[m.Op][j];

                    if (values[j] == null) values[j] = m.Values != null && m.Values.Length - 1 >= j ? (int?) m.Values[j] : m.Value;
                    if (types[j] == "SID" && values[j] != null) values[j] = GetCffString(StringIndex, Convert.ToInt32(values[j]));
                }

                if (values.Length == 1)
                    newDict[m.Name] = values[0];
                else
                    newDict[m.Name] = values;
            }

            return newDict;
        }

        // Parse a `CFF` DICT operand.
        private static float ParseOperand(ByteReader reader, int b0)
        {
            int b1;
            int b2;
            switch (b0)
            {
                case 28:
                    b1 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    return (b1 << 8) | b2;
                case 29:
                    b1 = reader.ReadByte();
                    b2 = reader.ReadByte();
                    int b3 = reader.ReadByte();
                    int b4 = reader.ReadByte();
                    return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
                case 30:
                    return ParseFloatOperand(reader);
            }

            if (b0 >= 32 && b0 <= 246) return b0 - 139;

            if (b0 >= 247 && b0 <= 250)
            {
                b1 = reader.ReadByte();
                return (b0 - 247) * 256 + b1 + 108;
            }

            if (b0 < 251 || b0 > 254) throw new Exception("Font: Invalid b0 " + b0);
            b1 = reader.ReadByte();
            return -(b0 - 251) * 256 - b1 - 108;
        }

        // Parse a `CFF` DICT real value.
        private static float ParseFloatOperand(ByteReader reader)
        {
            var s = "";
            const int eof = 15;
            string[] lookup = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "E", "E-", null, "-"};
            while (true)
            {
                int b = reader.ReadByte();
                int n1 = b >> 4;
                int n2 = b & 15;

                if (n1 == eof) break;

                s += lookup[n1];

                if (n2 == eof) break;

                s += lookup[n2];
            }

            return float.Parse(s);
        }

        private class DictMeta
        {
            public string Name;
            public int Op;
            public string Type;
            public int? Value;
            public float[] Values;
        }

        private static DictMeta[] _topDictMeta =
        {
            new DictMeta {Name = "version", Op = 0, Type = "SID"},
            new DictMeta {Name = "notice", Op = 1, Type = "SID"},
            new DictMeta {Name = "copyright", Op = 1200, Type = "SID"},
            new DictMeta {Name = "fullName", Op = 2, Type = "SID"},
            new DictMeta {Name = "familyName", Op = 3, Type = "SID"},
            new DictMeta {Name = "weight", Op = 4, Type = "SID"},
            new DictMeta {Name = "isFixedPitch", Op = 1201, Type = "number", Value = 0},
            new DictMeta {Name = "italicAngle", Op = 1202, Type = "number", Value = 0},
            new DictMeta {Name = "underlinePosition", Op = 1203, Type = "number", Value = -100},
            new DictMeta {Name = "underlineThickness", Op = 1204, Type = "number", Value = 50},
            new DictMeta {Name = "paintType", Op = 1205, Type = "number", Value = 0},
            new DictMeta {Name = "charstringType", Op = 1206, Type = "number", Value = 2},
            new DictMeta
            {
                Name = "fontMatrix",
                Op = 1207,
                Type = "real,real,real,real,real,real",
                Values = new[] {0.001f, 0, 0, 0.001f, 0, 0}
            },
            new DictMeta {Name = "uniqueId", Op = 13, Type = "number"},
            new DictMeta {Name = "fontBBox", Op = 5, Type = "number,number,number,number", Values = new[] {0f, 0, 0, 0}},
            new DictMeta {Name = "strokeWidth", Op = 1208, Type = "number", Value = 0},
            new DictMeta {Name = "xuid", Op = 14, Type = "", Value = null},
            new DictMeta {Name = "charset", Op = 15, Type = "offset", Value = 0},
            new DictMeta {Name = "encoding", Op = 16, Type = "offset", Value = 0},
            new DictMeta {Name = "charStrings", Op = 17, Type = "offset", Value = 0},
            new DictMeta {Name = "private", Op = 18, Type = "number,offset", Values = new[] {0f, 0}},
            new DictMeta {Name = "ros", Op = 1230, Type = "SID,SID,number"},
            new DictMeta {Name = "cidFontVersion", Op = 1231, Type = "number", Value = 0},
            new DictMeta {Name = "cidFontRevision", Op = 1232, Type = "number", Value = 0},
            new DictMeta {Name = "cidFontType", Op = 1233, Type = "number", Value = 0},
            new DictMeta {Name = "cidCount", Op = 1234, Type = "number", Value = 8720},
            new DictMeta {Name = "uidBase", Op = 1235, Type = "number"},
            new DictMeta {Name = "fdArray", Op = 1236, Type = "offset"},
            new DictMeta {Name = "fdSelect", Op = 1237, Type = "offset"},
            new DictMeta {Name = "fontName", Op = 1238, Type = "SID"}
        };

        private static DictMeta[] _privateDictMeta =
        {
            new DictMeta {Name = "subrs", Op = 19, Type = "offset", Value = 0},
            new DictMeta {Name = "defaultWidthX", Op = 20, Type = "number", Value = 0},
            new DictMeta {Name = "nominalWidthX", Op = 21, Type = "number", Value = 0}
        };

        // ReSharper disable StringLiteralTypo
        private static string[] _cffStandardStrings =
        {
            ".notdef", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent", "ampersand", "quoteright",
            "parenleft", "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash", "zero", "one", "two",
            "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less", "equal", "greater",
            "question", "at", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S",
            "T", "U", "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright", "asciicircum", "underscore",
            "quoteleft", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
            "u", "v", "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde", "exclamdown", "cent", "sterling",
            "fraction", "yen", "florin", "section", "currency", "quotesingle", "quotedblleft", "guillemotleft",
            "guilsinglleft", "guilsinglright", "fi", "fl", "endash", "dagger", "daggerdbl", "periodcentered", "paragraph",
            "bullet", "quotesinglbase", "quotedblbase", "quotedblright", "guillemotright", "ellipsis", "perthousand",
            "questiondown", "grave", "acute", "circumflex", "tilde", "macron", "breve", "dotaccent", "dieresis", "ring",
            "cedilla", "hungarumlaut", "ogonek", "caron", "emdash", "AE", "ordfeminine", "Lslash", "Oslash", "OE",
            "ordmasculine", "ae", "dotlessi", "lslash", "oslash", "oe", "germandbls", "onesuperior", "logicalnot", "mu",
            "trademark", "Eth", "onehalf", "plusminus", "Thorn", "onequarter", "divide", "brokenbar", "degree", "thorn",
            "threequarters", "twosuperior", "registered", "minus", "eth", "multiply", "threesuperior", "copyright",
            "Aacute", "Acircumflex", "Adieresis", "Agrave", "Aring", "Atilde", "Ccedilla", "Eacute", "Ecircumflex",
            "Edieresis", "Egrave", "Iacute", "Icircumflex", "Idieresis", "Igrave", "Ntilde", "Oacute", "Ocircumflex",
            "Odieresis", "Ograve", "Otilde", "Scaron", "Uacute", "Ucircumflex", "Udieresis", "Ugrave", "Yacute",
            "Ydieresis", "Zcaron", "aacute", "acircumflex", "adieresis", "agrave", "aring", "atilde", "ccedilla", "eacute",
            "ecircumflex", "edieresis", "egrave", "iacute", "icircumflex", "idieresis", "igrave", "ntilde", "oacute",
            "ocircumflex", "odieresis", "ograve", "otilde", "scaron", "uacute", "ucircumflex", "udieresis", "ugrave",
            "yacute", "ydieresis", "zcaron", "exclamsmall", "Hungarumlautsmall", "dollaroldstyle", "dollarsuperior",
            "ampersandsmall", "Acutesmall", "parenleftsuperior", "parenrightsuperior", "266 ff", "onedotenleader",
            "zerooldstyle", "oneoldstyle", "twooldstyle", "threeoldstyle", "fouroldstyle", "fiveoldstyle", "sixoldstyle",
            "sevenoldstyle", "eightoldstyle", "nineoldstyle", "commasuperior", "threequartersemdash", "periodsuperior",
            "questionsmall", "asuperior", "bsuperior", "centsuperior", "dsuperior", "esuperior", "isuperior", "lsuperior",
            "msuperior", "nsuperior", "osuperior", "rsuperior", "ssuperior", "tsuperior", "ff", "ffi", "ffl",
            "parenleftinferior", "parenrightinferior", "Circumflexsmall", "hyphensuperior", "Gravesmall", "Asmall",
            "Bsmall", "Csmall", "Dsmall", "Esmall", "Fsmall", "Gsmall", "Hsmall", "Ismall", "Jsmall", "Ksmall", "Lsmall",
            "Msmall", "Nsmall", "Osmall", "Psmall", "Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall",
            "Xsmall", "Ysmall", "Zsmall", "colonmonetary", "onefitted", "rupiah", "Tildesmall", "exclamdownsmall",
            "centoldstyle", "Lslashsmall", "Scaronsmall", "Zcaronsmall", "Dieresissmall", "Brevesmall", "Caronsmall",
            "Dotaccentsmall", "Macronsmall", "figuredash", "hypheninferior", "Ogoneksmall", "Ringsmall", "Cedillasmall",
            "questiondownsmall", "oneeighth", "threeeighths", "fiveeighths", "seveneighths", "onethird", "twothirds",
            "zerosuperior", "foursuperior", "fivesuperior", "sixsuperior", "sevensuperior", "eightsuperior", "ninesuperior",
            "zeroinferior", "oneinferior", "twoinferior", "threeinferior", "fourinferior", "fiveinferior", "sixinferior",
            "seveninferior", "eightinferior", "nineinferior", "centinferior", "dollarinferior", "periodinferior",
            "commainferior", "Agravesmall", "Aacutesmall", "Acircumflexsmall", "Atildesmall", "Adieresissmall",
            "Aringsmall", "AEsmall", "Ccedillasmall", "Egravesmall", "Eacutesmall", "Ecircumflexsmall", "Edieresissmall",
            "Igravesmall", "Iacutesmall", "Icircumflexsmall", "Idieresissmall", "Ethsmall", "Ntildesmall", "Ogravesmall",
            "Oacutesmall", "Ocircumflexsmall", "Otildesmall", "Odieresissmall", "OEsmall", "Oslashsmall", "Ugravesmall",
            "Uacutesmall", "Ucircumflexsmall", "Udieresissmall", "Yacutesmall", "Thornsmall", "Ydieresissmall", "001.000",
            "001.001", "001.002", "001.003", "Black", "Bold", "Book", "Light", "Medium", "Regular", "Roman", "Semibold"
        };

        public static string[] CffStandardEncoding =
        {
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "space", "exclam", "quotedbl", "numbersign", "dollar", "percent", "ampersand", "quoteright",
            "parenleft", "parenright", "asterisk", "plus", "comma", "hyphen", "period", "slash", "zero", "one", "two",
            "three", "four", "five", "six", "seven", "eight", "nine", "colon", "semicolon", "less", "equal", "greater",
            "question", "at", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S",
            "T", "U", "V", "W", "X", "Y", "Z", "bracketleft", "backslash", "bracketright", "asciicircum", "underscore",
            "quoteleft", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
            "u", "v", "w", "x", "y", "z", "braceleft", "bar", "braceright", "asciitilde", "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "exclamdown", "cent", "sterling", "fraction", "yen", "florin", "section", "currency", "quotesingle",
            "quotedblleft", "guillemotleft", "guilsinglleft", "guilsinglright", "fi", "fl", "", "endash", "dagger",
            "daggerdbl", "periodcentered", "", "paragraph", "bullet", "quotesinglbase", "quotedblbase", "quotedblright",
            "guillemotright", "ellipsis", "perthousand", "", "questiondown", "", "grave", "acute", "circumflex", "tilde",
            "macron", "breve", "dotaccent", "dieresis", "", "ring", "cedilla", "", "hungarumlaut", "ogonek", "caron",
            "emdash", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "AE", "", "ordfeminine", "", "", "",
            "", "Lslash", "Oslash", "OE", "ordmasculine", "", "", "", "", "", "ae", "", "", "", "dotlessi", "", "",
            "lslash", "oslash", "oe", "germandbls"
        };

        public static string[] CffExpertEncoding =
        {
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "", "", "", "", "space", "exclamsmall", "Hungarumlautsmall", "", "dollaroldstyle", "dollarsuperior",
            "ampersandsmall", "Acutesmall", "parenleftsuperior", "parenrightsuperior", "twodotenleader", "onedotenleader",
            "comma", "hyphen", "period", "fraction", "zerooldstyle", "oneoldstyle", "twooldstyle", "threeoldstyle",
            "fouroldstyle", "fiveoldstyle", "sixoldstyle", "sevenoldstyle", "eightoldstyle", "nineoldstyle", "colon",
            "semicolon", "commasuperior", "threequartersemdash", "periodsuperior", "questionsmall", "", "asuperior",
            "bsuperior", "centsuperior", "dsuperior", "esuperior", "", "", "isuperior", "", "", "lsuperior", "msuperior",
            "nsuperior", "osuperior", "", "", "rsuperior", "ssuperior", "tsuperior", "", "ff", "fi", "fl", "ffi", "ffl",
            "parenleftinferior", "", "parenrightinferior", "Circumflexsmall", "hyphensuperior", "Gravesmall", "Asmall",
            "Bsmall", "Csmall", "Dsmall", "Esmall", "Fsmall", "Gsmall", "Hsmall", "Ismall", "Jsmall", "Ksmall", "Lsmall",
            "Msmall", "Nsmall", "Osmall", "Psmall", "Qsmall", "Rsmall", "Ssmall", "Tsmall", "Usmall", "Vsmall", "Wsmall",
            "Xsmall", "Ysmall", "Zsmall", "colonmonetary", "onefitted", "rupiah", "Tildesmall", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
            "exclamdownsmall", "centoldstyle", "Lslashsmall", "", "", "Scaronsmall", "Zcaronsmall", "Dieresissmall",
            "Brevesmall", "Caronsmall", "", "Dotaccentsmall", "", "", "Macronsmall", "", "", "figuredash", "hypheninferior",
            "", "", "Ogoneksmall", "Ringsmall", "Cedillasmall", "", "", "", "onequarter", "onehalf", "threequarters",
            "questiondownsmall", "oneeighth", "threeeighths", "fiveeighths", "seveneighths", "onethird", "twothirds", "",
            "", "zerosuperior", "onesuperior", "twosuperior", "threesuperior", "foursuperior", "fivesuperior",
            "sixsuperior", "sevensuperior", "eightsuperior", "ninesuperior", "zeroinferior", "oneinferior", "twoinferior",
            "threeinferior", "fourinferior", "fiveinferior", "sixinferior", "seveninferior", "eightinferior",
            "nineinferior", "centinferior", "dollarinferior", "periodinferior", "commainferior", "Agravesmall",
            "Aacutesmall", "Acircumflexsmall", "Atildesmall", "Adieresissmall", "Aringsmall", "AEsmall", "Ccedillasmall",
            "Egravesmall", "Eacutesmall", "Ecircumflexsmall", "Edieresissmall", "Igravesmall", "Iacutesmall",
            "Icircumflexsmall", "Idieresissmall", "Ethsmall", "Ntildesmall", "Ogravesmall", "Oacutesmall",
            "Ocircumflexsmall", "Otildesmall", "Odieresissmall", "OEsmall", "Oslashsmall", "Ugravesmall", "Uacutesmall",
            "Ucircumflexsmall", "Udieresissmall", "Yacutesmall", "Thornsmall", "Ydieresissmall"
        };
        // ReSharper enable StringLiteralTypo

        // Given a String Index (SID), return the value of the string.
        // Strings below index 392 are standard CFF strings and are not encoded in the font.
        private static string GetCffString(string[] strings, int index)
        {
            return index <= 390 ? _cffStandardStrings[index] : strings[index - 391];
        }

        public Glyph CffGlyphLoad(int index)
        {
            var glyphData = new CffGlyphFactory();
            bool successful = RunCharstring(index, glyphData);
            if (!successful) Engine.Log.Warning($"Couldn't read CFF glyff - {index}", MessageSource.FontParser);
            return glyphData.Glyph;
        }

        public class CffGlyphFactory
        {
            public Glyph Glyph;

            public bool Started;
            public float FirstX;
            public float FirstY;
            public float X;
            public float Y;
            public List<GlyphVertex> VerticesInProgress = new List<GlyphVertex>();

            public CffGlyphFactory()
            {
                Glyph = new Glyph();
            }

            public void Done()
            {
                Glyph.Vertices = VerticesInProgress.ToArray();
            }

            public void MoveTo(float dx, float dy)
            {
                CloseShape();
                X += dx;
                FirstX = X;
                Y += dy;
                FirstY = Y;

                Vertex(VertexTypeFlag.Move, (int) X, (int) Y, 0, 0, 0, 0);
            }

            public void CloseShape()
            {
                if (FirstX != X || FirstY != Y)
                    Vertex(VertexTypeFlag.Line, (int) FirstX, (int) FirstY, 0,
                        0, 0, 0);
            }

            public void Vertex(VertexTypeFlag type, int x, int y, int cx, int cy, int cx1, int cy1)
            {
                TrackVertex(x, y);
                if (type == VertexTypeFlag.Cubic)
                {
                    TrackVertex(cx, cy);
                    TrackVertex(cx1, cy1);
                }

                VerticesInProgress.Add(new GlyphVertex
                {
                    TypeFlag = type,
                    X = (short) x,
                    Y = (short) y,
                    Cx = (short) cx,
                    Cy = (short) cy,
                    Cx1 = (short) cx1,
                    Cy1 = (short) cy1
                });
            }

            public void TrackVertex(int dx, int dy)
            {
                if (dx > Glyph.XMax || !Started)
                    Glyph.XMax = (short) dx;
                if (dy > Glyph.YMax || !Started)
                    Glyph.YMax = (short) dy;
                if (dx < Glyph.XMin || !Started)
                    Glyph.XMin = (short) dx;
                if (dy < Glyph.YMin || !Started)
                    Glyph.YMin = (short) dy;
                Started = true;
            }

            public void LineTo(float dx, float dy)
            {
                X += dx;
                Y += dy;
                Vertex(VertexTypeFlag.Line, (int) X, (int) Y, 0, 0, 0,
                    0);
            }

            public void CubicCurveTo(float dx1, float dy1, float dx2, float dy2,
                float dx3, float dy3)
            {
                float cx1 = X + dx1;
                float cy1 = Y + dy1;
                float cx2 = cx1 + dx2;
                float cy2 = cy1 + dy2;
                X = cx2 + dx3;
                Y = cy2 + dy3;
                Vertex(VertexTypeFlag.Cubic, (int) X, (int) Y, (int) cx1, (int) cy1,
                    (int) cx2, (int) cy2);
            }
        }

        /// <summary>
        /// Horrible
        /// https://www.adobe.com/devnet/font.html
        /// </summary>
        public bool RunCharstring(int glyphIndex, CffGlyphFactory c)
        {
            var inHeader = true;
            var maskBits = 0;
            var subrStackHeight = 0;
            var sp = 0;
            var hasSubrs = false;

            // Argument stack.
            var s = new float[48];

            // Subr nesting stack.
            var subrStack = new ByteReader[10];
            using var defaultB = new ByteReader(CharStringIndex[glyphIndex]);
            ByteReader b = defaultB;

            while (b.Position < b.Data.Length)
            {
                var i = 0;
                var clearStack = true;
                int b0 = b.ReadByte();
                float f;
                switch (b0)
                {
                    case 0x13:
                    case 0x14:
                        if (inHeader)
                            maskBits += sp / 2;
                        inHeader = false;
                        b.ReadBytes((maskBits + 7) / 8);
                        break;
                    case 0x01:
                    case 0x03:
                    case 0x12:
                    case 0x17:
                        maskBits += sp / 2;
                        break;
                    case 0x15:
                        inHeader = false;
                        if (sp < 2)
                            return false;
                        c.MoveTo(s[sp - 2], s[sp - 1]);
                        break;
                    case 0x04:
                        inHeader = false;
                        if (sp < 1)
                            return false;
                        c.MoveTo(0, s[sp - 1]);
                        break;
                    case 0x16:
                        inHeader = false;
                        if (sp < 1)
                            return false;
                        c.MoveTo(s[sp - 1], 0);
                        break;
                    case 0x05:
                        if (sp < 2)
                            return false;
                        for (; i + 1 < sp; i += 2)
                        {
                            c.LineTo(s[i], s[i + 1]);
                        }

                        break;
                    case 0x07:
                    case 0x06:
                        if (sp < 1)
                            return false;
                        bool gotoVLineTo = b0 == 0x07;
                        for (;;)
                        {
                            if (!gotoVLineTo)
                            {
                                if (i >= sp)
                                    break;
                                c.LineTo(s[i], 0);
                                i++;
                            }

                            gotoVLineTo = false;
                            if (i >= sp)
                                break;
                            c.LineTo(0, s[i]);
                            i++;
                        }

                        break;
                    case 0x1F:
                    case 0x1E:
                        if (sp < 4)
                            return false;
                        bool gotoHvCurveTo = b0 == 0x1F;
                        for (;;)
                        {
                            if (!gotoHvCurveTo)
                            {
                                if (i + 3 >= sp)
                                    break;
                                c.CubicCurveTo(0, s[i], s[i + 1],
                                    s[i + 2], s[i + 3],
                                    sp - i == 5 ? s[i + 4] : 0.0f);
                                i += 4;
                            }

                            gotoHvCurveTo = false;
                            if (i + 3 >= sp)
                                break;
                            c.CubicCurveTo(s[i], 0, s[i + 1],
                                s[i + 2], sp - i == 5 ? s[i + 4] : 0.0f, s[i + 3]);
                            i += 4;
                        }

                        break;
                    case 0x08:
                        if (sp < 6)
                            return false;
                        for (; i + 5 < sp; i += 6)
                        {
                            c.CubicCurveTo(s[i], s[i + 1], s[i + 2],
                                s[i + 3], s[i + 4], s[i + 5]);
                        }

                        break;
                    case 0x18:
                        if (sp < 8)
                            return false;
                        for (; i + 5 < sp - 2; i += 6)
                        {
                            c.CubicCurveTo(s[i], s[i + 1], s[i + 2],
                                s[i + 3], s[i + 4], s[i + 5]);
                        }

                        if (i + 1 >= sp)
                            return false;
                        c.LineTo(s[i], s[i + 1]);
                        break;
                    case 0x19:
                        if (sp < 8)
                            return false;
                        for (; i + 1 < sp - 6; i += 2)
                        {
                            c.LineTo(s[i], s[i + 1]);
                        }

                        if (i + 5 >= sp)
                            return false;
                        c.CubicCurveTo(s[i], s[i + 1], s[i + 2],
                            s[i + 3], s[i + 4], s[i + 5]);
                        break;
                    case 0x1A:
                    case 0x1B:
                        if (sp < 4)
                            return false;
                        f = 0.0f;
                        if ((sp & 1) != 0)
                        {
                            f = s[i];
                            i++;
                        }

                        for (; i + 3 < sp; i += 4)
                        {
                            if (b0 == 0x1B)
                                c.CubicCurveTo(s[i], f, s[i + 1],
                                    s[i + 2], s[i + 3], (float) 0.0);
                            else
                                c.CubicCurveTo(f, s[i], s[i + 1],
                                    s[i + 2], (float) 0.0, s[i + 3]);
                            f = 0.0f;
                        }

                        break;
                    case 0x0A:
                    case 0x1D:
                        if (b0 == 0x0A)
                            if (!hasSubrs)
                                // todo
                                //if (FdSelect.Length != 0)
                                //    subrs = new ByteReader(Subr[glyphIndex]);
                                hasSubrs = true;

                        if (sp < 1)
                            return false;
                        var v = (int) s[--sp];
                        if (subrStackHeight >= 10)
                            return false;

                        subrStack[subrStackHeight++] = b;
                        b = b0 == 0x0A ? new ByteReader(Subr[v + SubrBias]) : new ByteReader(GlobalSubrIndex[v + GlobalBias]);

                        if (b.Data.Length == 0)
                            return false;
                        b.Position = 0;
                        clearStack = false;
                        break;
                    case 0x0B:
                        if (subrStackHeight <= 0)
                            return false;
                        b = subrStack[--subrStackHeight];
                        clearStack = false;
                        break;
                    case 0x0E:
                        c.CloseShape();
                        c.Done();
                        return true;
                    case 0x0C:
                    {
                        float dx1;
                        float dx2;
                        float dx3;
                        float dx4;
                        float dx5;
                        float dx6;
                        float dy1;
                        float dy2;
                        float dy3;
                        float dy4;
                        float dy5;
                        float dy6;
                        int b1 = b.ReadByte();
                        switch (b1)
                        {
                            case 0x22:
                                if (sp < 7)
                                    return false;
                                dx1 = s[0];
                                dx2 = s[1];
                                dy2 = s[2];
                                dx3 = s[3];
                                dx4 = s[4];
                                dx5 = s[5];
                                dx6 = s[6];
                                c.CubicCurveTo(dx1, 0, dx2, dy2,
                                    dx3, 0);
                                c.CubicCurveTo(dx4, 0, dx5, -dy2,
                                    dx6, 0);
                                break;
                            case 0x23:
                                if (sp < 13)
                                    return false;
                                dx1 = s[0];
                                dy1 = s[1];
                                dx2 = s[2];
                                dy2 = s[3];
                                dx3 = s[4];
                                dy3 = s[5];
                                dx4 = s[6];
                                dy4 = s[7];
                                dx5 = s[8];
                                dy5 = s[9];
                                dx6 = s[10];
                                dy6 = s[11];
                                c.CubicCurveTo(dx1, dy1, dx2, dy2,
                                    dx3, dy3);
                                c.CubicCurveTo(dx4, dy4, dx5, dy5,
                                    dx6, dy6);
                                break;
                            case 0x24:
                                if (sp < 9)
                                    return false;
                                dx1 = s[0];
                                dy1 = s[1];
                                dx2 = s[2];
                                dy2 = s[3];
                                dx3 = s[4];
                                dx4 = s[5];
                                dx5 = s[6];
                                dy5 = s[7];
                                dx6 = s[8];
                                c.CubicCurveTo(dx1, dy1, dx2, dy2,
                                    dx3, 0);
                                c.CubicCurveTo(dx4, 0, dx5, dy5,
                                    dx6, -(dy1 + dy2 + dy5));
                                break;
                            case 0x25:
                                if (sp < 11)
                                    return false;
                                dx1 = s[0];
                                dy1 = s[1];
                                dx2 = s[2];
                                dy2 = s[3];
                                dx3 = s[4];
                                dy3 = s[5];
                                dx4 = s[6];
                                dy4 = s[7];
                                dx5 = s[8];
                                dy5 = s[9];
                                dx6 = dy6 = s[10];
                                float dx = dx1 + dx2 + dx3 + dx4 + dx5;
                                float dy = dy1 + dy2 + dy3 + dy4 + dy5;
                                if (MathF.Abs(dx) > MathF.Abs(dy))
                                    dy6 = -dy;
                                else
                                    dx6 = -dx;
                                c.CubicCurveTo(dx1, dy1, dx2, dy2,
                                    dx3, dy3);
                                c.CubicCurveTo(dx4, dy4, dx5, dy5,
                                    dx6, dy6);
                                break;
                            default:
                                return false;
                        }
                    }
                        break;
                    default:
                        if (b0 != 255 && b0 != 28 && (b0 < 32 || b0 > 254))
                            return false;
                        if (b0 == 255)
                        {
                            f = (float) b.ReadUIntBE() / 0x10000;
                        }
                        else
                        {
                            b.Position -= 1;
                            f = (short) ReadCffInt(b);
                        }

                        if (sp >= 48)
                            return false;
                        s[sp++] = f;
                        clearStack = false;
                        break;
                }

                if (clearStack)
                    sp = 0;
            }

            return false;
        }

        public static uint ReadCffInt(ByteReader r)
        {
            int b0 = r.ReadByte();
            if (b0 >= 32 && b0 <= 246)
                return (uint) (b0 - 139);
            if (b0 >= 247 && b0 <= 250)
                return (uint) ((b0 - 247) * 256 + r.ReadByte() + 108);
            if (b0 >= 251 && b0 <= 254)
                return (uint) (-(b0 - 251) * 256 - r.ReadByte() - 108);
            if (b0 == 28)
                return r.ReadUShortBE();
            if (b0 == 29)
                return r.ReadUIntBE();
            return 0;
        }
    }
}