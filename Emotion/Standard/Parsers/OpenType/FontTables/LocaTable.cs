#nullable enable

namespace Emotion.Standard.Parsers.OpenType.FontTables;

/// <summary>
/// https://docs.microsoft.com/en-us/typography/opentype/spec/loca
/// </summary>
public class LocaTable
{
    public int[] GlyphOffsets;

    private LocaTable(int[] glyphOffsets)
    {
        GlyphOffsets = glyphOffsets;
    }

    // Parse the `loca` table. This table stores the offsets to the locations of the glyphs in the font,
    // relative to the beginning of the glyphData table.
    // The number of glyphs stored in the `loca` table is specified in the `maxp` table (under numGlyphs)
    // The loca table has two versions: a short version where offsets are stored as UShorts, and a long
    // version where offsets are stored as ULongs.
    // The `head` table specifies which version to use (under indexToLocFormat).
    public static LocaTable ParseLoca(ByteReader reader, int numGlyphs, bool shortVersion)
    {
        var glyphOffsets = new int[numGlyphs + 1];

        // There is an extra entry after the last index element to compute the length of the last glyph.
        // That's why we use numGlyphs + 1.
        for (var i = 0; i < numGlyphs; i++)
        {
            if (shortVersion)
            {
                glyphOffsets[i] = reader.ReadUShortBE();

                // The short table version stores the actual offset divided by 2.
                glyphOffsets[i] *= 2;
            }
            else
            {
                glyphOffsets[i] = (int) reader.ReadULongBE();
            }
        }

        return new LocaTable(glyphOffsets);
    }
}