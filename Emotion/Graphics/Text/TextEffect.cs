#nullable enable

namespace Emotion.Graphics.Text;

public enum TextEffectType
{
    None,
    Outline,
    Shadow,
    Glow
}

public struct TextEffect
{
    public TextEffectType Type;
    public Color EffectColor;
    public int EffectValue;

    public readonly IntVector2 GrowAtlas()
    {
        return new IntVector2(EffectValue * 2);
    }

    public readonly IntVector2 GetRenderOffset()
    {
        if (Type == TextEffectType.Shadow)
            return IntVector2.Zero;
        return new IntVector2(EffectValue);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Type, EffectColor, EffectValue);
    }

    public static TextEffect Outline(Color outlineColor, int outlineSize)
    {
        return new TextEffect()
        {
            Type = TextEffectType.Outline,
            EffectColor = outlineColor,
            EffectValue = outlineSize
        };
    }

    public static TextEffect Shadow(Color shadowColor, int shadowOffset)
    {
        return new TextEffect()
        {
            Type = TextEffectType.Shadow,
            EffectColor = shadowColor,
            EffectValue = shadowOffset
        };
    }
}