#nullable enable

using Emotion.Core.Systems.Logging;

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Standard.Parsers.OpenType.FontTables;

/// <summary>
/// https://docs.microsoft.com/en-us/typography/opentype/spec/cmap
/// This table stores the mappings from characters to glyphs.
/// </summary>
public class CMapTable
{
    public Dictionary<uint, uint> GlyphIndexMap { get; init; }

    private CMapTable(Dictionary<uint, uint> indexMap)
    {
        GlyphIndexMap = indexMap;
    }

    public static CMapTable? ParseCmap(ByteReader reader)
    {
        ushort version = reader.ReadUShortBE();
        if (version != 0) Engine.Log.Warning("CMap table version should be 0.", MessageSource.FontParser);

        // The CMap table can contain many sub-tables, each with their own format.
        // We're only interested in a "platform 0" (Unicode format) and "platform 3" (Windows format) table.
        ushort numTables = reader.ReadUShortBE();
        int tableOffset = -1;
        for (var i = 0; i < numTables; i += 1)
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
            return null;
        }

        Dictionary<uint, uint> glyphIndexMap;
        ByteReader subReader = reader.Branch(tableOffset, true);
        ushort format = subReader.ReadUShortBE();
        if (format == 12)
        {
            glyphIndexMap = ReadFormat12(subReader);
        }
        else if (format == 4)
        {
            glyphIndexMap = ReadFormat4(subReader);
        }
        else
        {
            Engine.Log.Warning($"Unsupported CMap format - {format}", MessageSource.FontParser);
            return null;
        }

        return new CMapTable(glyphIndexMap);
    }

    /// <summary>
    /// Segmented coverage
    /// </summary>
    private static Dictionary<uint, uint> ReadFormat12(ByteReader reader)
    {
        //Skip reserved.
        reader.ReadUShortBE();

        // Length in bytes of the sub-tables.
        reader.ReadULongBE(); // length
        reader.ReadULongBE(); // language

        uint groupCount = reader.ReadULongBE();
        var glyphIndexMap = new Dictionary<uint, uint>();

        for (ulong i = 0; i < groupCount; i += 1)
        {
            uint startCharCode = reader.ReadULongBE();
            uint endCharCode = reader.ReadULongBE();
            uint startGlyphId = reader.ReadULongBE();

            for (uint c = startCharCode; c <= endCharCode; c += 1)
            {
                glyphIndexMap[c] = startGlyphId;
                startGlyphId++;
            }
        }

        return glyphIndexMap;
    }

    /// <summary>
    /// Segment mapping to delta values
    /// </summary>
    private static Dictionary<uint, uint> ReadFormat4(ByteReader reader)
    {
        reader.ReadUShortBE(); // length
        reader.ReadUShortBE(); // language

        // segCount is stored x 2.
        int segCount = reader.ReadUShortBE() >> 1;

        // Skip searchRange, entrySelector, rangeShift.
        reader.ReadUShortBE();
        reader.ReadUShortBE();
        reader.ReadUShortBE();

        // The "unrolled" mapping from character codes to glyph indices.
        var glyphIndexMap = new Dictionary<uint, uint>();
        ByteReader endCountReader = reader.Branch(14, true);
        ByteReader startCountReader = reader.Branch(16 + segCount * 2, true);
        ByteReader idDeltaReader = reader.Branch(16 + segCount * 4, true);
        ByteReader idRangeOffsetReader = reader.Branch(16 + segCount * 6, true);
        int idRangeRelativeOffset = 16 + segCount * 6;

        for (uint i = 0; i < segCount - 1; i += 1)
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
                    if (glyphIndex != 0) glyphIndex = glyphIndex + (uint) idDelta & 0xFFFF;
                }
                else
                {
                    glyphIndex = (uint) (c + idDelta) & 0xFFFF;
                }

                glyphIndexMap[c] = glyphIndex;
            }
        }

        return glyphIndexMap;
    }
}