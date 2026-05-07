#nullable enable

namespace Emotion.Game.Systems.UI2;

public struct GridLayoutVisualConfig
{
    public Color LineColor = Color.White;
    public bool LineAfterFirstRow;

    public GridLayoutVisualConfig()
    {

    }
}

public struct UIWindowVisualConfig
{
    public Color BackgroundColor = Color.White.SetAlpha(0);

    public int RoundRadius = 0;

    public int Border = 0;
    public Color BorderColor = Color.Black;

    public GridLayoutVisualConfig GridVisual;

    /// <summary>
    /// Whether the window is visible.
    /// If not, the RenderInternal function will early out
    /// and children's Renders will not be called
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (value == _visible) return;
            _visible = value;

            Engine.UI.InvalidateInputFocus();
            if (_dontTakeSpaceWhenHidden)
                InvalidateLayout();
        }
    }

    private bool _visible = true;

    /// <summary>
    /// Whether to consider this window as part of the layout when invisible.
    /// Matters only within lists.
    /// </summary>
    public bool DontTakeSpaceWhenHidden
    {
        get => _dontTakeSpaceWhenHidden;
        set
        {
            if (value == _dontTakeSpaceWhenHidden) return;
            _dontTakeSpaceWhenHidden = value;
            InvalidateLayout();
        }
    }

    private bool _dontTakeSpaceWhenHidden;

    public UIWindowVisualConfig()
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

    public override string ToString()
    {
        return "Visuals";
    }
}