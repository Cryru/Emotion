#region Using

using Emotion.Common.Serialization;

#endregion

#nullable enable

namespace Emotion.UI;

// On Rounding In UI Layout:
// Sizes should always be rounded up.
// Positions should always be rounded down.
// Offsets (spacings) should always be rounded to the closest.

public partial class UIBaseWindow : IRenderable, IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
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
    public bool FillX
    {
        get => _fillX;
        set
        {
            if (_fillX == value) return;
            _fillX = value;
            InvalidateLayout();
        }
    }

    private bool _fillX = true;

    /// <inheritdoc cref="FillX" />
    public bool FillY
    {
        get => _fillY;
        set
        {
            if (_fillY == value) return;
            _fillY = value;
            InvalidateLayout();
        }
    }

    private bool _fillY = true;

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

    public bool ChildrenCanExpandParent = true;

    // ReSharper disable once InconsistentNaming
    protected static List<UIBaseWindow> EMPTY_CHILDREN_LIST = new(0);

#if NEW_UI
    protected virtual Vector2 InternalMeasure(Vector2 space)
#else
    protected virtual Vector2 NEW_InternalMeasure(Vector2 space)
#endif
    {
        return Vector2.Zero;
    }

    protected virtual Rectangle GetChildrenMeasureSpace(Vector2 pos, Vector2 space)
    {
        return new Rectangle(pos, space);
    }

    /// <summary>
    /// Given the max space by the parent, return the minimum size this window needs.
    /// </summary>
    protected virtual Vector2 Measure(Vector2 space)
    {
        UIController.DebugShouldBreakpointMeasure(this);

        if (!Visible && DontTakeSpaceWhenHidden)
        {
            _measuredSize = Vector2.Zero;
            Size = Vector2.Zero;
            return _measuredSize;
        }

        float scale = GetScale();

        // Now find out the minimum size of children of this window.
        bool amInsideParent = AnchorsInsideParent(ParentAnchor, Anchor);
        if (!amInsideParent) space = Controller!.Size;

        _layoutEngine.Reset();
        _layoutEngine.SetLayoutDimensions(GetChildrenMeasureSpace(Vector2.Zero, space), Margins * scale, MaxSize * scale, Paddings * scale);

        Vector2 scaledListSpacing = (ListSpacing * scale).RoundAwayFromZero();
        _layoutEngine.SetLayoutMode(UIPass.Measure, LayoutMode, scaledListSpacing);

        // Append children to the layout.
        List<UIBaseWindow> children = GetWindowChildren();
        for (var i = 0; i < children.Count; i++)
        {
            UIBaseWindow child = children[i];
            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            Vector2 freeSpace = _layoutEngine.GetFreeSpace();
            Vector2 childNeedsSpace = child.Measure(freeSpace);
            _layoutEngine.AppendChild(child, childNeedsSpace, child.Margins * child.GetScale());
        }

        UIController.DebugShouldBreakpointMeasureAfterChildren(this);

        Vector2 childrenUsed = _layoutEngine.ApplyMeasure();
        _measureChildrenUsedSpace = childrenUsed;

        // Find out the minimum size of this window.
#if NEW_UI
        Vector2 minWindowSize = InternalMeasure(space);
#else
        Vector2 minWindowSize = NEW_InternalMeasure(space);
#endif
        minWindowSize = Vector2.Clamp(minWindowSize, MinSize * scale, MaxSize * scale).Ceiling();

        // Now that we know how big this window wants to be at minimum,
        // and the minimum of all children, we can determine the actual minimum of this window.
        Vector2 size = minWindowSize;

        // Parents are expanded by their children.
        // todo: disable some windows from having children? maybe just editor hint?
        if (true)
        {
            childrenUsed.X = MathF.Min(childrenUsed.X, space.X); // Dont allow expansion higher than space.
            size.X = MathF.Max(childrenUsed.X, size.X);
        }

        if (true)
        {
            childrenUsed.Y = MathF.Min(childrenUsed.Y, space.Y);
            size.Y = MathF.Max(childrenUsed.Y, size.Y);
        }

        if (size.X < 0 || size.Y < 0)
        {
            Assert(false, $"UIWindow of id {Id} measured with a size smaller than 0.");
            size.X = MathF.Max(size.X, 0);
            size.Y = MathF.Max(size.Y, 0);
        }

        _measuredSize = size;
        Size = size;

        return size;
    }

    protected virtual void Layout(Vector2 pos, Vector2 size)
    {
        if (size.X < 0 || size.Y < 0)
        {
            Assert(false, $"UIWindow of id {Id} layouted with a size smaller than 0.");
            size.X = MathF.Max(size.X, 0);
            size.Y = MathF.Max(size.Y, 0);
        }

        float scale = GetScale();
        pos += (Offset * scale).Round();
        pos = pos.Floor();
        Position = pos.ToVec3(Z);
        Size = size.Ceiling();

        _layoutEngine.Reset();

        // The size being passed to us in Layout already respects our limit and
        // has the margins applied - meaning we don't have to do it.
        // Since the paddings are on the inside of our content though, we have to apply them.
        _layoutEngine.SetLayoutDimensions(new Rectangle(Position2, Size), Rectangle.Empty, DefaultMaxSize, Paddings * scale);

        _layoutEngine.SetLayoutMode(UIPass.Layout, LayoutMode, (ListSpacing * scale).RoundAwayFromZero());

        // Append children to the layout.
        List<UIBaseWindow> children = GetWindowChildren();
        for (var i = 0; i < children.Count; i++)
        {
            UIBaseWindow child = children[i];
            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            Vector2 childSize = child._measuredSize;
            float childScale = child.GetScale();
            _layoutEngine.AppendChild(child, childSize, child.Margins * childScale);
        }

        UIController.DebugShouldBreakpointLayoutAfterChildren(this);

        _layoutEngine.ApplyLayout();

        // Invalidate transformations.
        if (_transformationStackBacking != null) _transformationStackBacking.MatrixDirty = true;

        // Construct input detecting boundary that includes this window's children.
        _inputBoundsWithChildren = Bounds;
        for (var i = 0; i < children.Count; i++)
        {
            UIBaseWindow child = children[i];
            _inputBoundsWithChildren = Rectangle.Union(child._inputBoundsWithChildren, _inputBoundsWithChildren);
        }

        AfterLayout();
    }
}