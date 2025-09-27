#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public struct UIWindowLayoutConfig
{
    public const int DEFAULT_MAX_SIZE = 99999;

    public Vector2 Scale = new Vector2(1f);
    public bool ScaleWithResolution = true;

    public IntVector2 Offset
    {
        readonly get => _offset;
        set
        {
            if (value == _offset) return;
            _offset = value;
            InvalidateLayout();
        }
    }

    private IntVector2 _offset = IntVector2.Zero;

    /// <summary>
    /// The minimum size the window can be.
    /// </summary>
    public IntVector2 MinSize = IntVector2.Zero;

    public int MinSizeX
    {
        readonly get => MinSize.X;
        set => MinSize.X = value;
    }

    public int MinSizeY
    {
        readonly get => MinSize.Y;
        set => MinSize.Y = value;
    }

    public IntVector2 MaxSize = new IntVector2(DEFAULT_MAX_SIZE);

    public int MaxSizeX
    {
        readonly get => MaxSize.X;
        set => MaxSize.X = value;
    }

    public int MaxSizeY
    {
        readonly get => MaxSize.Y;
        set => MaxSize.Y = value;
    }

    public UISpacing Padding;
    public UISpacing Margins;

    public UILayoutMethod LayoutMethod = UILayoutMethod.Free(UIAnchor.TopLeft, UIAnchor.TopLeft);
    public UISizing SizingX = UISizing.Grow();
    public UISizing SizingY = UISizing.Grow();

    /// <summary>
    /// The point in the parent to anchor the window to, if the parent window is of the "Free" layout type.
    /// </summary>
    public UIAnchor ParentAnchor
    {
        readonly get => _parentAnchor;
        set
        {
            if (value == _parentAnchor) return;
            _parentAnchor = value;
            InvalidateLayout();
        }
    }

    private UIAnchor _parentAnchor { get; set; } = UIAnchor.TopLeft;

    /// <summary>
    /// Where the window should anchor to relative to the alignment in its parent, if the parent window is of the "Free" layout type.
    /// </summary>
    public UIAnchor Anchor
    {
        readonly get => _anchor;
        set
        {
            if (value == _anchor) return;
            _anchor = value;
            InvalidateLayout();
        }
    }

    private UIAnchor _anchor { get; set; } = UIAnchor.TopLeft;

    [DontSerialize]
    public UIAnchor AnchorAndParentAnchor
    {
        set
        {
            ParentAnchor = value;
            Anchor = value;
        }
    }

    public UIWindowLayoutConfig()
    {
    }

    private UIBaseWindow? _owner;

    internal void SetWindowOwner(UIBaseWindow owner)
    {
        _owner = owner;
    }

    private readonly void InvalidateLayout()
    {
        if (_owner == null) return;
        _owner.InvalidateLayout();
    }

    public override readonly string ToString()
    {
        return "Metrics";
    }
}
