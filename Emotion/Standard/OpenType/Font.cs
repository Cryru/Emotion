#region Using

using System;
using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType.FontTables;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// Represents a parsed OpenType font file.
    /// </summary>
    public class Font
    {
        #region Meta

        /// <summary>
        /// Whether the font file data passed was valid.
        /// </summary>
        public bool Valid
        {
            get => Glyphs != null;
        }

        /// <summary>
        /// The font's format.
        /// This will be either "truetype" or "cff" or "unsupported".
        /// </summary>
        public string Format { get; protected set; } = "unsupported";

        /// <summary>
        /// The font's family name.
        /// </summary>
        public string FontFamily { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's sub family name.
        /// </summary>
        public string FontSubFamily { get; protected set; } = "Unknown";

        /// <summary>
        /// The full name of the font.
        /// </summary>
        public string FullName { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's version.
        /// </summary>
        public string Version { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's copyright.
        /// </summary>
        public string Copyright { get; protected set; } = "Unknown";

        /// <summary>
        /// The font's uniqueId.
        /// </summary>
        public string UniqueId { get; protected set; } = "Unknown";

        #endregion

        /// <summary>
        /// The ratio of glyph units per em.
        /// Used to scale uniformly across fonts.
        /// </summary>
        public int UnitsPerEm { get; protected set; }

        /// <summary>
        /// The font's ascender minus its descender.
        /// </summary>
        public float Height
        {
            get => Ascender - Descender;
        }

        /// <summary>
        /// The highest a glyph can reach.
        /// </summary>
        public float Ascender { get; protected set; }

        /// <summary>
        /// The lowest a glyph can reach.
        /// </summary>
        public float Descender { get; protected set; }

        /// <summary>
        /// Distance between lines.
        /// </summary>
        public float LineGap { get; protected set; }

        /// <summary>
        /// Font is scaled to the ascender.
        /// </summary>
        public float ScaleApplied { get; protected set; }

        // This should probably be private
        private List<FontTable>? _tables;

        /// <summary>
        /// All of the glyphs and their data from this font.
        /// </summary>
        public FontGlyph[]? Glyphs;

        /// <summary>
        /// Glyphs found in the font related to the chars they represent.
        /// </summary>
        // ReSharper disable once CollectionNeverQueried.Global
        public Dictionary<char, FontGlyph> CharToGlyph = new();

        /// <summary>
        /// The character index of the first glyph.
        /// </summary>
        public uint FirstCharIndex { get; protected set; }

        /// <summary>
        /// The largest character index found in the font.
        /// </summary>
        public uint LastCharIndex { get; protected set; }

        public Font(ReadOnlyMemory<byte> fileData)
        {
            // Note: OpenType fonts use big endian byte ordering.
            using var r = new ByteReader(fileData);
            var tag = new string(r.ReadChars(4));

            switch (tag)
            {
                case "\0\u0001\0\0":
                case "true":
                case "typ1":
                {
                    Format = "truetype";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    _tables = GetTables(r, numTables);
                    break;
                }

                case "OTTO":
                {
                    Format = "cff";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    _tables = GetTables(r, numTables);
                    break;
                }

                default:
                {
                    Engine.Log.Error($"Unsupported font format - {tag}", MessageSource.FontParser);
                    return;
                }
            }

            // Parse optional tables.
            FontTable? nameTable = GetTable("name");
            if (nameTable != null)
            {
                // todo: ltag parsing
                Dictionary<string, Dictionary<string, string>> names = NameTable.ParseName(r.Branch(nameTable.Offset, true, nameTable.Length), null);

                FontFamily = NameTable.GetDefaultValue(names, "fontFamily");
                FontSubFamily = NameTable.GetDefaultValue(names, "fontSubfamily");
                FullName = NameTable.GetDefaultValue(names, "fullName");
                Version = NameTable.GetDefaultValue(names, "version");
                Copyright = NameTable.GetDefaultValue(names, "copyright");
                UniqueId = NameTable.GetDefaultValue(names, "uniqueID");
            }

            // Parse required tables
            FontTable? headTable = GetTable("head");
            if (headTable == null)
            {
                Engine.Log.Error("Font is missing 'head' table.", MessageSource.FontParser);
                return;
            }

            FontTable? maxpTable = GetTable("maxp");
            if (maxpTable == null)
            {
                Engine.Log.Error("Font is missing 'maxp' table.", MessageSource.FontParser);
                return;
            }

            FontTable? hheaTable = GetTable("hhea");
            if (hheaTable == null)
            {
                Engine.Log.Error("Font is missing 'hhea' table.", MessageSource.FontParser);
                return;
            }

            var headTableParsed = new HeadTable(r.Branch(headTable.Offset, true, headTable.Length));
            var maxpTableParsed = new MaxpTable(r.Branch(maxpTable.Offset, true, maxpTable.Length));
            var hheaTableParsed = new HheaTable(r.Branch(hheaTable.Offset, true, hheaTable.Length));

            int numberOfHMetrics = hheaTableParsed.NumberOfHMetrics;
            int glyphCount = maxpTableParsed.NumGlyphs;

            var scale = 1f;
            UnitsPerEm = headTableParsed.UnitsPerEm;
            Ascender = hheaTableParsed.Ascender * scale;
            Descender = hheaTableParsed.Descender * scale;
            LineGap = hheaTableParsed.LineGap * scale;
            ScaleApplied = scale;

            // glyf fonts
            FontTable? glyfTable = GetTable("glyf");
            if (glyfTable != null)
            {
                FontTable? locaTable = GetTable("loca");
                if (locaTable == null) return;

                var glyphs = new FontGlyph[glyphCount];
                for (var i = 0; i < glyphs.Length; i++)
                {
                    var glyph = new FontGlyph();
                    glyph.MapIndex = i;
                    glyphs[i] = glyph;
                }

                short indexToLocFormat = headTableParsed.IndexToLocFormat;
                LocaTable locaTableParsed = LocaTable.ParseLoca(r.Branch(locaTable.Offset, true, locaTable.Length), glyphCount, indexToLocFormat == 0);
                GlyfTable.ParseGlyf(r.Branch(glyfTable.Offset, true, glyfTable.Length), locaTableParsed, glyphs, scale);
                Glyphs = glyphs;
            }
            // cff fonts
            else
            {
                FontTable? cffTable = GetTable("CFF ");
                if (cffTable == null)
                {
                    Engine.Log.Warning("Font - neither glyf nor cff table found.", MessageSource.FontParser);
                    return;
                }

                var cff = new CffTable(r.Branch(cffTable.Offset, true, cffTable.Length));

                var glyphs = new FontGlyph[cff.NumberOfGlyphs];
                for (var i = 0; i < glyphs.Length; i++)
                {
                    FontGlyph? glyph = cff.ParseCffGlyph(i, scale);
                    glyph.MapIndex = i;
                    glyphs[i] = glyph;
                }

                // todo: parsing

                Glyphs = glyphs;
            }

            FontTable? hmtxTable = GetTable("hmtx");
            if (hmtxTable != null)
            {
                HmtxTable hmtxTableParsed = HmtxTable.ParseHmtx(r.Branch(hmtxTable.Offset, true, hmtxTable.Length));
                hmtxTableParsed.ApplyToGlyphs(numberOfHMetrics, Glyphs);
                for (var i = 0; i < Glyphs.Length; i++)
                {
                    FontGlyph glyph = Glyphs[i];
                    glyph.AdvanceWidth *= scale;
                    glyph.LeftSideBearing *= scale;
                }
            }

            // Assign glyph mapping
            FontTable? cmapTable = GetTable("cmap");
            CMapTable? cmapTableParsed = cmapTable != null ? CMapTable.ParseCmap(r.Branch(cmapTable.Offset, true, cmapTable.Length)) : null;
            if (cmapTableParsed != null)
            {
                // Add glyph names if present.
                string[]? names = null;
                FontTable? postTable = GetTable("post");
                if (postTable != null)
                {
                    var post = new PostTable(r.Branch(postTable.Offset, true, postTable.Length));
                    names = post.Names;
                }

                var smallestCharIdx = uint.MaxValue;
                uint highestCharIdx = 0;

                foreach ((uint key, uint value) in cmapTableParsed.GlyphIndexMap)
                {
                    var valInt = (int) value;
                    if (valInt >= Glyphs.Length) continue; // Should never happen, but it's outside data, soo...

                    FontGlyph glyph = Glyphs[valInt];
                    glyph.Name = names != null ? names[valInt] : ((char) key).ToString();

                    smallestCharIdx = Math.Min(smallestCharIdx, key);
                    highestCharIdx = Math.Max(highestCharIdx, key);

                    CharToGlyph.Add((char) key, glyph);
                }

                FirstCharIndex = smallestCharIdx;
                LastCharIndex = highestCharIdx;
            }
            else
            {
                for (var i = 0; i < Glyphs.Length; i++)
                {
                    FontGlyph glyph = Glyphs[i];
                    glyph.Name = ((char) i).ToString();
                    CharToGlyph.Add((char) i, glyph);
                }

                FirstCharIndex = 0;
                LastCharIndex = (uint) (Glyphs.Length - 1);
            }

            // os/2 parsed, but unused
            // cvt parsed, but unused
            // todo: kern
            // todo: gsub
            // todo: gpos
            // todo: fvar
            // todo: meta
        }

        #region Parse Helpers

        /// <summary>
        /// Finds all tables in the font and their data.
        /// </summary>
        private static List<FontTable> GetTables(ByteReaderBase reader, ushort count)
        {
            var tables = new List<FontTable>();

            for (var i = 0; i < count; i++)
            {
                var t = new FontTable
                {
                    Tag = new string(reader.ReadChars(4)),
                    Checksum = (int) reader.ReadULongBE(),
                    Offset = (int) reader.ReadULongBE(),
                    Length = (int) reader.ReadULongBE()
                };
                tables.Add(t);
            }

            return tables;
        }

        /// <summary>
        /// Get the font table under the specified tag.
        /// </summary>
        /// <param name="tag">The tag of the table to get.</param>
        /// <returns>The table with the specified tag, or null if not found.</returns>
        private FontTable? GetTable(string tag)
        {
            for (var i = 0; i < _tables?.Count; i++)
            {
                FontTable table = _tables[i];
                if (table.Tag == tag) // should probably be name and not tag
                    return table;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// https://docs.microsoft.com/en-us/typography/opentype/spec/cvt
        /// Instructions for control over certain glyphs.
        /// </summary>
        private static short[] ParseCvt(ByteReader reader, uint length)
        {
            var cvt = new short[length / 2];

            for (uint i = 0; i < length / 2; i++)
            {
                cvt[i] = reader.ReadShortBE();
            }

            return cvt;
        }
    }
}