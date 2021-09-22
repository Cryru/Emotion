#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Standard.OpenType.FontTables;
using Emotion.Utility;

#nullable enable

#endregion

namespace Emotion.Standard.OpenType
{
    /// <summary>
    /// Represents an OpenType font.
    /// </summary>
    public class Font
    {
        /// <summary>
        /// Whether the font was successfully parsed.
        /// </summary>
        public bool Valid;

        #region Meta

        /// <summary>
        /// The font's magic tag from which the format was inferred.
        /// Generally this is useless to users.
        /// </summary>
        public string? Tag { get; protected set; }

        /// <summary>
        /// The font's format.
        /// This will be either "truetype" or "cff".
        /// </summary>
        public string? Format { get; protected set; }

        /// <summary>
        /// The font's family name.
        /// </summary>
        public string? FontFamily { get; protected set; }

        /// <summary>
        /// The font's sub family name.
        /// </summary>
        public string? FontSubFamily { get; protected set; }

        /// <summary>
        /// The full name of the font.
        /// </summary>
        public string? FullName { get; protected set; }

        /// <summary>
        /// The font's version.
        /// </summary>
        public string? Version { get; protected set; }

        /// <summary>
        /// The font's copyright.
        /// </summary>
        public string? Copyright { get; protected set; }

        /// <summary>
        /// The font's uniqueId.
        /// </summary>
        public string? UniqueId { get; protected set; }

        #endregion

        /// <summary>
        /// Glyphs found in the font.
        /// </summary>
        public Dictionary<char, Glyph> Glyphs = new();

        /// <summary>
        /// The character index of the first glyph.
        /// </summary>
        public uint FirstCharIndex { get; protected set; }

        /// <summary>
        /// The largest character index found in the font.
        /// </summary>
        public uint LastCharIndex { get; protected set; }

        /// <summary>
        /// The font's ascender minus its descender. Is used as the distance between lines and is regarded as the safe space.
        /// </summary>
        public int Height
        {
            get => Ascender - Descender;
        }

        /// <summary>
        /// The highest a glyph can reach.
        /// </summary>
        public short Ascender { get; protected set; }

        /// <summary>
        /// The lowest a glyph can reach.
        /// </summary>
        public short Descender { get; protected set; }

        /// <summary>
        /// The font's defined resolution. Is used for scaling glyphs.
        /// </summary>
        public ushort UnitsPerEm { get; protected set; }

        #region Parsing

        /// <summary>
        /// Font tables found within the font.
        /// After the initial parsing these aren't used.
        /// </summary>
        public List<FontTable> Tables = new List<FontTable>();

        /// <summary>
        /// The number of horizontal metrics found in the font.
        /// </summary>
        public ushort NumberOfHMetrics { get; protected set; }

        #endregion

        /// <summary>
        /// Create a new OpenType font from a font file.
        /// </summary>
        /// <param name="fontData">The bytes that make up the font file.</param>
        public Font(ReadOnlyMemory<byte> fontData)
        {
            // Note: OpenType fonts use big endian byte ordering.
            using var r = new ByteReader(fontData);
            Tag = new string(r.ReadChars(4));

            switch (Tag)
            {
                case "\0\u0001\0\0":
                case "true":
                case "typ1":
                {
                    Format = "truetype";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    Tables = GetTables(r, numTables);
                    break;
                }

                case "OTTO":
                {
                    Format = "cff";
                    ushort numTables = r.ReadUShortBE();
                    r.ReadBytes(6);
                    Tables = GetTables(r, numTables);
                    break;
                }
                case "wOFF":
                {
                    Format = new string(r.ReadChars(4)) == "OTTO" ? "cff" : "truetype";
                    r.ReadBytes(4);
                    r.ReadUShortBE();
                    // todo: Read wOFF tables.
                    break;
                }
            }

            // Header
            FontTable? table = GetTable("head");
            short indexToLocFormat;
            if (table != null)
            {
                var head = new HeadTable(r.Branch(table.Offset, true, table.Length));
                UnitsPerEm = head.UnitsPerEm;
                indexToLocFormat = head.IndexToLocFormat;
            }
            else
            {
                Engine.Log.Warning("Font head table not found.", MessageSource.FontParser);
                return;
            }

            // Horizontal header - information about horizontal layout of glyphs.
            table = GetTable("hhea");
            if (table != null)
            {
                var hhea = new HheaTable(r.Branch(table.Offset, true, table.Length));

                Ascender = hhea.Ascender;
                Descender = hhea.Descender;
                NumberOfHMetrics = hhea.NumberOfHMetrics;
            }
            else
            {
                Engine.Log.Warning("Font hhea table not found.", MessageSource.FontParser);
                return;
            }

            // Name table - contains meta information about the font.
            table = GetTable("name");
            if (table != null)
            {
                // todo: ltag parsing
                Dictionary<string, Dictionary<string, string>> names = NameTable.ParseName(r.Branch(table.Offset, true, table.Length), null);

                FontFamily = NameTable.GetDefaultValue(names, "fontFamily");
                FontSubFamily = NameTable.GetDefaultValue(names, "fontSubfamily");
                FullName = NameTable.GetDefaultValue(names, "fullName");
                Version = NameTable.GetDefaultValue(names, "version");
                Copyright = NameTable.GetDefaultValue(names, "copyright");
                UniqueId = NameTable.GetDefaultValue(names, "uniqueID");
            }
            else
            {
                Engine.Log.Warning("Font name table not found.", MessageSource.FontParser);
                return;
            }

            // MaxP - contains information about the font's memory footprint.
            table = GetTable("maxp");
            ushort numGlyphs;
            if (table != null)
            {
                var maxp = new MaxpTable(r.Branch(table.Offset, true, table.Length));
                numGlyphs = maxp.NumGlyphs;
            }
            else
            {
                Engine.Log.Warning("Font maxp table not found.", MessageSource.FontParser);
                return;
            }

            // Get the cmap table which defines glyph indices.
            table = GetTable("cmap");
            CMapTable? cMap = table != null ? new CMapTable(r.Branch(table.Offset, true, table.Length)) : null;

            // Glyf - glyph data.
            // Also reads loca for locations, and post for glyph names.
            Glyph[] glyphs;
            table = GetTable("glyf");
            if (table != null)
            {
                FontTable? locaTable = GetTable("loca");
                if (locaTable == null) return;
                int[] locaOffsets = LocaTable.ParseLoca(r.Branch(locaTable.Offset, true, locaTable.Length), numGlyphs, indexToLocFormat == 0);
                glyphs = GlyfTable.ParseGlyf(r.Branch(table.Offset, true, table.Length), locaOffsets);
            }
            else
            {
                table = GetTable("CFF ");
                if (table == null)
                {
                    Engine.Log.Warning("Font - neither glyf nor cff table found.", MessageSource.FontParser);
                    return;
                }

                var cff = new CffTable(r.Branch(table.Offset, true, table.Length));
                glyphs = new Glyph[cff.NumberOfGlyphs];
                for (var i = 0; i < glyphs.Length; i++)
                {
                    glyphs[i] = cff.CffGlyphLoad(i);
                }
            }

            // Apply the character map (glyph to char index).

            if (cMap?.GlyphIndexMap != null)
            {
                var smallestCharIdx = uint.MaxValue;
                uint highestCharIdx = 0;

                // Add glyph names if present.
                string[]? names = null;
                FontTable? postTable = GetTable("post");
                if (postTable != null)
                {
                    var post = new PostTable(r.Branch(postTable.Offset, true, postTable.Length));
                    names = post.Names;
                }

                foreach ((uint key, uint value) in cMap.GlyphIndexMap)
                {
                    var valInt = (int)value;
                    if (valInt >= glyphs.Length) continue; // Should never happen, but it's outside data, soo...

                    Glyph glyph = glyphs[valInt];
                    glyph.Name = names != null ? names[valInt] : ((char)key).ToString();

                    smallestCharIdx = Math.Min(smallestCharIdx, key);
                    highestCharIdx = Math.Max(highestCharIdx, key);

                    Glyphs.Add((char)key, glyph);
                    glyph.MapIndex = value;
                }

                FirstCharIndex = smallestCharIdx;
                LastCharIndex = highestCharIdx;
            }
            else
            {
                for (var i = 0; i < glyphs.Length; i++)
                {
                    glyphs[i].Name = ((char)i).ToString();
                    Glyphs.Add((char)i, glyphs[i]);
                }
            }

            // Add metrics. This requires the glyph's to have MapIndices
            table = GetTable("hmtx");
            if (table != null) HmtxTable.ParseHmtx(r.Branch(table.Offset, true, table.Length), NumberOfHMetrics, glyphs);

            // os/2 parsed, but unused
            // cvt parsed, but unused
            // todo: kern
            // todo: gsub
            // todo: gpos
            // todo: fvar
            // todo: meta

            Valid = true;
        }

        protected Font()
        {
        }

        #region Parse Helpers

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

        /// <summary>
        /// Finds all tables in the font and their data.
        /// </summary>
        private static List<FontTable> GetTables(ByteReader reader, ushort count)
        {
            var tables = new List<FontTable>();

            for (var i = 0; i < count; i++)
            {
                var t = new FontTable
                {
                    Tag = new string(reader.ReadChars(4)),
                    Checksum = (int)reader.ReadULongBE(),
                    Offset = (int)reader.ReadULongBE(),
                    Length = (int)reader.ReadULongBE()
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
        public FontTable? GetTable(string tag)
        {
            return Tables.FirstOrDefault(x => x.Tag == tag);
        }

        #endregion
    }
}