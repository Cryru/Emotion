#region Using

using System;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType.Helpers;
using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/cff
    ///
    /// This format is objectively horrible to parse, use, and yeah.
    /// IT'S AN ENTIRE LANGUAGE RUNTIME
    /// Sorry, I needed to vent
    /// </summary>
    public partial class CffTable
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
                FdSelect = Array.Empty<byte>();
                // todo
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
                    Subr = Array.Empty<byte[]>();
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
                    CustomEncoding = ParseCustomCffEncoding(reader.Branch(encoding, true));
                    break;
            }
        }

        public FontGlyph ParseCffGlyph(int index, float scale)
        {
            var glyphData = new CffGlyphFactory(scale);
            bool successful = RunCharstring(index, glyphData);
            if (!successful) Engine.Log.Warning($"Couldn't read CFF glyff - {index}", MessageSource.FontParser);
            return glyphData.Glyph;
        }

        /// <summary>
        /// Horrible
        /// https://www.adobe.com/devnet/font.html
        /// </summary>
        private bool RunCharstring(int glyphIndex, CffGlyphFactory c)
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