#nullable enable

namespace Emotion.Standard.Parsers.OpenType;

/// <summary>
/// Represents metadata about a data table in the font file.
/// </summary>
public class FontTable
{
    /// <summary>
    /// The identifier of the table.
    /// </summary>
    public string Tag;

    /// <summary>
    /// Validation checksum for the table contents.
    /// </summary>
    public int Checksum;

    /// <summary>
    /// Offset of the table from the start of the file, in bytes.
    /// </summary>
    public int Offset;

    /// <summary>
    /// The length of the table's data, in bytes.
    /// </summary>
    public int Length;
}