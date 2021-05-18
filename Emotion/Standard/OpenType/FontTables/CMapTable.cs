#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.OpenType.FontTables
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
    /// </summary>
    public class CMapTable
    {
        public ushort Version;
        public ushort NumTables;
        public ushort Format;
        public uint Length;
        public uint Language;
        public uint GroupCount;

        public int SegCount;

        public Dictionary<uint, uint> GlyphIndexMap;

        /// <summary>
        /// Parse the `CMap` table. This table stores the mappings from characters to glyphs.
        /// </summary>
        public CMapTable(ByteReader reader)
        {
            Version = reader.ReadUShortBE();

            if (Version != 0) Engine.Log.Warning("CMap table version should be 0.", MessageSource.FontParser);

            // The CMap table can contain many sub-tables, each with their own format.
            // We're only interested in a "platform 0" (Unicode format) and "platform 3" (Windows format) table.
            NumTables = reader.ReadUShortBE();
            int tableOffset = -1;
            for (var i = 0; i < NumTables; i += 1)
            {
                ushort platformId = reader.ReadUShortBE();
                ushort encodingId = reader.ReadUShortBE();
                var offset = (int) reader.ReadULongBE();
                if ((platformId != 3 || encodingId != 0 && encodingId != 1 && encodingId != 10) &&
                    (platformId != 0 || encodingId != 0 && encodingId != 1 && encodingId != 2 && encodingId != 3 && encodingId != 4)) continue;
                tableOffset = offset;
                break;
            }

            if (tableOffset == -1)
            {
                Engine.Log.Warning("No valid CMap sub-tables found.", MessageSource.FontParser);
                return;
            }

            ByteReader subReader = reader.Branch(tableOffset, true);
            Format = subReader.ReadUShortBE();

            if (Format == 12)
                ReadFormat12(subReader);
            else if (Format == 4)
                ReadFormat4(subReader);
            else
                Engine.Log.Warning($"Unsupported CMap format - {Format}", MessageSource.FontParser);
        }

        /// <summary>
        /// Segmented coverage
        /// </summary>
        private void ReadFormat12(ByteReader reader)
        {
            //Skip reserved.
            reader.ReadUShortBE();

            // Length in bytes of the sub-tables.
            Length = reader.ReadULongBE();
            Language = reader.ReadULongBE();

            GroupCount = reader.ReadULongBE();
            GlyphIndexMap = new Dictionary<uint, uint>();

            for (ulong i = 0; i < GroupCount; i += 1)
            {
                uint startCharCode = reader.ReadULongBE();
                uint endCharCode = reader.ReadULongBE();
                uint startGlyphId = reader.ReadULongBE();

                for (uint c = startCharCode; c <= endCharCode; c += 1)
                {
                    GlyphIndexMap[c] = startGlyphId;
                    startGlyphId++;
                }
            }
        }

        /// <summary>
        /// Segment mapping to delta values
        /// </summary>
        private void ReadFormat4(ByteReader reader)
        {
            Length = reader.ReadUShortBE();
            Language = reader.ReadUShortBE();

            // segCount is stored x 2.
            SegCount = reader.ReadUShortBE() >> 1;

            // Skip searchRange, entrySelector, rangeShift.
            reader.ReadUShortBE();
            reader.ReadUShortBE();
            reader.ReadUShortBE();

            // The "unrolled" mapping from character codes to glyph indices.
            GlyphIndexMap = new Dictionary<uint, uint>();
            using ByteReader endCountReader = reader.Branch(14, true);
            using ByteReader startCountReader = reader.Branch(16 + SegCount * 2, true);
            using ByteReader idDeltaReader = reader.Branch(16 + SegCount * 4, true);
            using ByteReader idRangeOffsetReader = reader.Branch(16 + SegCount * 6, true);
            int idRangeRelativeOffset = 16 + SegCount * 6;

            for (uint i = 0; i < SegCount - 1; i += 1)
            {
                ushort endCount = endCountReader.ReadUShortBE();
                ushort startCount = startCountReader.ReadUShortBE();
                short idDelta = idDeltaReader.ReadShortBE();
                ushort idRangeOffset = idRangeOffsetReader.ReadUShortBE();

                for (ushort c = startCount; c <= endCount; c += 1)
                {
                    uint glyphIndex;
                    if (idRangeOffset != 0)
                    {
                        // The idRangeOffset is relative to the current position in the idRangeOffset array.
                        // Take the current offset in the idRangeOffset array.
                        int glyphIndexOffset = idRangeOffsetReader.Position + idRangeRelativeOffset - 2;

                        // Add the value of the idRangeOffset, which will move us into the glyphIndex array.
                        glyphIndexOffset += idRangeOffset;

                        // Then add the character index of the current segment, multiplied by 2 for USHORTs.
                        glyphIndexOffset += (c - startCount) * 2;
                        reader.Position = glyphIndexOffset;
                        glyphIndex = reader.ReadUShortBE();
                        if (glyphIndex != 0) glyphIndex = (glyphIndex + (uint) idDelta) & 0xFFFF;
                    }
                    else
                    {
                        glyphIndex = (uint) (c + idDelta) & 0xFFFF;
                    }

                    GlyphIndexMap[c] = glyphIndex;
                }
            }
        }
    }
}