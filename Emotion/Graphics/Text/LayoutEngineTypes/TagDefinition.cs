#nullable enable

using Emotion;

namespace Emotion.Graphics.Text.LayoutEngineTypes;

public struct TagDefinition
{
    public int NameStartIdx;
    public int NameLength;

    public TextTagType TagType;
    public Color ExtraData; // todo: read/write as color functions so it can be used
    public int ExtraData2; // todo: read/write as type functions

    public TagDefinition(int nameStartIdx, int nameLength)
    {
        NameStartIdx = nameStartIdx;
        NameLength = nameLength;
    }

    public ReadOnlySpan<char> GetTagName(ReadOnlySpan<char> totalText)
    {
        return totalText.Slice(NameStartIdx, NameLength);
    }

    public int GetTagStartIndexInText()
    {
        return NameStartIdx + NameLength + 1; // "name>" + 1 because of the bracket
    }
}
