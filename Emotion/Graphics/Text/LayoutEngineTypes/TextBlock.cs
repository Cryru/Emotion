#nullable enable

namespace Emotion.Graphics.Text.LayoutEngineTypes;

public struct TextBlock
{
    public int StartIndex;
    public int Length;
    public bool Skip;

    // Bounds
    public int X;
    public int Y;
    public int Width;

    // Render data
    public Color Color;
    public bool UseDefaultColor;

    public TextEffectType TextEffect;
    public Color EffectColor;
    public int EffectParam;

    // Layout data
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

    public ReadOnlySpan<char> GetBlockString(ReadOnlySpan<char> totalText)
    {
        return totalText.Slice(StartIndex, Length);
    }
}
