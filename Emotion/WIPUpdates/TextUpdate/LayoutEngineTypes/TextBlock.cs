#region Using
using Emotion.Graphics.Text;

#endregion

namespace Emotion.WIPUpdates.TextUpdate.LayoutEngineTypes;

public struct TextBlock
{
    public int StartIndex;
    public int Length;
    public bool Skip;

    public Color Color;
    public bool UseDefaultColor;

    public FontEffect TextEffect;
    public Color EffectColor;
    public int EffectParam;

    public bool Newline;
    public SpecialLayoutFlag SpecialLayout;

    public TextBlock(int startIndex)
    {
        StartIndex = startIndex;
        UseDefaultColor = true;
    }

    public TextBlock(int startIndex, int length)
    {
        StartIndex = startIndex;
        Length = length;
        UseDefaultColor = true;
    }

    public ReadOnlySpan<char> GetBlockString(string totalText)
    {
        return totalText.AsSpan().Slice(StartIndex, Length);
    }
}
