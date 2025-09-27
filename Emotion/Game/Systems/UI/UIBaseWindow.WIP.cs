#nullable enable

namespace Emotion.Game.Systems.UI;

// On Rounding In UI Layout:
// Sizes should always be rounded up.
// Positions should always be rounded down.
// Offsets (spacings) should always be rounded to the closest.

public partial class UIBaseWindow : IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
{
    // Legacy attributes
#if NEW_UI
    /// <summary>
    /// The size returned by the window measure.
    /// This is the defacto minimum size the child would occupy.
    /// </summary>
    [DontSerialize] protected Vector2 _measuredSize;

    public bool ChildrenAllSameWidth; // todo: delete
#endif

    public bool ExpandParent
    {
        get => _expandParent;
        set
        {
            if (_expandParent == value) return;
            _expandParent = value;
            InvalidateLayout();
        }
    }

    private bool _expandParent = true;

    /// <summary>
    /// The amount of space used by the children of this window during measurement.
    /// </summary>
    [DontSerialize] protected Vector2 _measureChildrenUsedSpace;

    /// <summary>
    /// Whether the window should fill the available space of its parent.
    /// On by default. Doesn't apply when the parent is of a list layout.
    /// </summary>
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

    [DontSerialize]
    public UIAnchor AnchorAndParentAnchor
    {
        get => ParentAnchor == Anchor ? Anchor : UIAnchor.TopLeft;
        set
        {
            ParentAnchor = value;
            Anchor = value;
        }
    }
}