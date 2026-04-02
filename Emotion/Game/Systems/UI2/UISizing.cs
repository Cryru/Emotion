namespace Emotion.Game.Systems.UI2;

public record struct UISizing
{
    public enum UISizingMode
    {
        Invalid,
        Fit,
        Grow,
        Fixed,
        ShrinkOnly
    }

    public int Size;
    public UISizingMode Mode;

    public static UISizing Fixed(int size)
    {
        return new UISizing()
        {
            Size = size,
            Mode = UISizingMode.Fixed
        };
    }

    public static UISizing Fit()
    {
        return new UISizing()
        {
            Mode = UISizingMode.Fit
        };
    }

    public static UISizing Grow()
    {
        return new UISizing()
        {
            Mode = UISizingMode.Grow
        };
    }

    public static UISizing ShrinkOnly()
    {
        return new UISizing()
        {
            Mode = UISizingMode.ShrinkOnly
        };
    }

    public readonly bool CanGrowOrShrink()
    {
        return Mode == UISizingMode.Grow || Mode == UISizingMode.ShrinkOnly;
    }

    public readonly bool CanGrow()
    {
        return Mode == UISizingMode.Grow;
    }

    public override readonly string ToString()
    {
        if (Mode == UISizingMode.Fixed)
            return $"Fixed {Size}";

        return $"{Mode}";
    }
}
