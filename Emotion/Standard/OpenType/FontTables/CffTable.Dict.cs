#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// This file contains all the helpers functions needed to parse the cff table,
    /// while the main file contains the runtime code.
    /// </summary>
    public partial class CffTable
    {
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

        private static T[] ReadIndexArray<T>(ByteReader reader)
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

        // Given a String Index (SID), return the value of the string.
        // Strings below index 392 are standard CFF strings and are not encoded in the font.
        private static string GetCffString(string[] strings, int index)
        {
            return index <= 390 ? _cffStandardStrings[index] : strings[index - 391];
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

        // Parse the CFF encoding data. Only one encoding can be specified per font.
        // See Adobe TN #5176 chapter 12, "Encodings".
        private static Dictionary<byte, byte> ParseCustomCffEncoding(ByteReader reader)
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

        private static string[] CffStandardEncoding =
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

        private static string[] CffExpertEncoding =
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
    }
}