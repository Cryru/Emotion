#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct UIWindowLayoutConfig
{
    public const int DEFAULT_MAX_SIZE = 99999;

    /// <summary>
    /// Scale of the window. Note this includes the children of the window as well.
    /// </summary>
    public Vector2 Scale
    {
        readonly get => _scale;
        set
        {
            if (value == _scale) return;
            _scale = value;
            InvalidateLayout();
        }
    }
    private Vector2 _scale = Vector2.One;

    /// <summary>
    /// Whether this window scales with the resolution of the UISystem (meaning the Host's screen)
    /// </summary>
    public bool ScaleWithResolution
    {
        readonly get => _scaleWithResolution;
        set
        {
            if (value == _scaleWithResolution) return;
            _scaleWithResolution = value;
            InvalidateLayout();
        }
    }

    private bool _scaleWithResolution = true;

    /// <summary>
    /// Final offset to the position of the window, applied at the final layout step.
    /// </summary>
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
    public IntVector2 MinSize
    {
        readonly get => _minSize;
        set
        {
            if (_minSize == value) return;
            _minSize = value;
            InvalidateLayout();
        }
    }

    private IntVector2 _minSize = IntVector2.Zero;

    public int MinSizeX
    {
        readonly get => MinSize.X;
        set => MinSize = MinSize with { X = value };
    }

    public int MinSizeY
    {
        readonly get => MinSize.Y;
        set => MinSize = MinSize with { Y = value };
    }

    /// <summary>
    /// The maximum size this window can be.
    /// </summary>
    public IntVector2 MaxSize
    {
        readonly get => _maxSize;
        set
        {
            if (_maxSize == value) return;
            _maxSize = value;
            InvalidateLayout();
        }
    }

    private IntVector2 _maxSize = new IntVector2(DEFAULT_MAX_SIZE);

    public int MaxSizeX
    {
        readonly get => MaxSize.X;
        set => MaxSize = MaxSize with { X = value };
    }

    public int MaxSizeY
    {
        readonly get => MaxSize.Y;
        set => MaxSize = MaxSize with { Y = value };
    }

    /// <summary>
    /// Space between the window's box and its children
    /// </summary>
    public UISpacing Padding
    {
        readonly get => _padding;
        set
        {
            if (_padding == value) return;
            _padding = value;
            InvalidateLayout();
        }
    }

    private UISpacing _padding;

    /// <summary>
    /// Space outside the window's content
    /// </summary>
    public UISpacing Margins
    {
        readonly get => _margins;
        set
        {
            if (_margins == value) return;
            _margins = value;
            InvalidateLayout();
        }
    }

    private UISpacing _margins;

    /// <summary>
    /// How to layout children of this window.
    /// </summary>
    public UILayoutMethod LayoutMethod
    {
        readonly get => _layoutMethod;
        set
        {
            if (_layoutMethod == value) return;
            _layoutMethod = value;
            InvalidateLayout();
        }
    }

    private UILayoutMethod _layoutMethod = UILayoutMethod.Free();

    public UISizing SizingX
    {
        readonly get => _sizingX;
        set
        {
            if (_sizingX == value) return;
            _sizingX = value;
            InvalidateLayout();
        }
    }

    private UISizing _sizingX = UISizing.Grow();

    public UISizing SizingY
    {
        readonly get => _sizingY;
        set
        {
            if (_sizingY == value) return;
            _sizingY = value;
            InvalidateLayout();
        }
    }

    private UISizing _sizingY = UISizing.Grow();

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

    public bool ChildrenCanExpand = true;

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
