#nullable enable


namespace Emotion.Game.Systems.UI;

// On Rounding In UI Layout:
// Sizes should always be rounded up.
// Positions should always be rounded down.
// Offsets (spacings) should always be rounded to the closest.

public partial class UIBaseWindow : IComparable<UIBaseWindow>
{
    /// <summary>
    /// Whether the window should fill the available space of its parent.
    /// On by default. Doesn't apply when the parent is of a list layout.
    /// </summary>
    [Obsolete("Use Layout.SizingX")]
    public bool GrowX
    {
        get => _growX;
        set
        {
            if (_growX == value) return;
            _growX = value;
            InvalidateLayout();
        }
    }

    private bool _growX = true;

    /// <inheritdoc cref="GrowX" />
    [Obsolete("Use Layout.SizingY")]
    public bool GrowY
    {
        get => _growY;
        set
        {
            if (_growY == value) return;
            _growY = value;
            InvalidateLayout();
        }
    }

    private bool _growY = true;
}