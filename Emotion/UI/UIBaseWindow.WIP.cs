#region Using

using Emotion.Common.Serialization;
using static Emotion.UI.UIBaseWindow;

#endregion

#nullable enable

namespace Emotion.UI;

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
        get => _fillY; // && Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;
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

    public enum UIDockDirection
    {
        None, Left, Top, Bottom, Right
    }

    /// <summary>
    /// If set to a 
    /// Works with the "Free" layout only.
    /// </summary>
    public UIDockDirection Dock
    {
        get => _dock;
        set
        {
            if (value == _dock) return;
            _dock = value;
            InvalidateLayout();
        }
    }
    private UIDockDirection _dock = UIDockDirection.None;

    private Vector2 GetDockMask()
    {
        switch (_dock)
        {
            case UIDockDirection.Top:
            case UIDockDirection.Bottom:
                return new Vector2(0, 1);
            case UIDockDirection.Left:
            case UIDockDirection.Right:
                return new Vector2(1, 0);
            default:
                return Vector2.Zero;
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

    // On Rounding In UI Layout:
    // Sizes should always be rounded up.
    // Positions should always be rounded down.
    // Offsets (spacings) should always be rounded to the closest.

    /// <summary>
    /// Given the max space by the parent, return the minimum size this window needs.
    /// </summary>
#if NEW_UI
    protected virtual Vector2 Measure(Vector2 space)
#else
    protected virtual Vector2 NEW_Measure(Vector2 space)
#endif
    {
        UIController.DebugShouldBreakpointMeasure(this);
        SortChildren();
        UseNewLayoutSystem = true;
        // ==========

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
        _layoutEngine.SetDimensions(new Rectangle(Vector2.Zero, space));
        _layoutEngine.SetLayoutMode(UIPass.Measure, LayoutMode, (ListSpacing * scale).RoundAwayFromZero());

        // The parent's margins and paddings is space children cannot take, so we take them out.
        _layoutEngine.DeflateDimensions(Margins * scale);
        _layoutEngine.SetBoundLimit(MaxSize * scale);
        _layoutEngine.AddPadding(Paddings * scale);

        // Append children to the layout.
        List<UIBaseWindow> children = Children ?? EMPTY_CHILDREN_LIST;
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;
        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            Vector2 layoutSpaceNow = _layoutEngine.GetFreeSpace();
            Vector2 childSpaceTaken = child.NEW_Measure(layoutSpaceNow);
            float childScale = child.GetScale();
            _layoutEngine.AppendChild(child, childSpaceTaken, child.Margins * childScale);
        }

        Vector2 childrenUsed = _layoutEngine.ApplyLayout();
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

    private bool IsOutsideParentListLayout(int axisMask, UIAnchor childAnchor)
    {
        if (axisMask == 0 && childAnchor is UIAnchor.BottomRight or UIAnchor.CenterRight or UIAnchor.TopRight) return true;
        return false;
    }

    protected virtual void Layout(Vector2 pos, Vector2 size)
    {
        UIController.DebugShouldBreakpointLayout(this);

        if (size.X < 0 || size.Y < 0)
        {
            Assert(false, $"UIWindow of id {Id} layouted with a size smaller than 0.");
            size.X = MathF.Max(size.X, 0);
            size.Y = MathF.Max(size.Y, 0);
        }

        float scale = GetScale();
        pos += (Offset * scale).RoundClosest();
        pos = pos.Floor();
        Position = pos.ToVec3(Z);
        Size = size.Ceiling();

        _layoutEngine.Reset();
        _layoutEngine.SetDimensions(Bounds);
        _layoutEngine.SetLayoutMode(UIPass.Layout, LayoutMode, (ListSpacing * scale).RoundAwayFromZero());
        _layoutEngine.AddPadding(Paddings * scale);

        // Append children to the layout.
        List<UIBaseWindow> children = Children ?? EMPTY_CHILDREN_LIST;
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;
        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            Vector2 childSize = child._measuredSize;
            float childScale = child.GetScale();
            _layoutEngine.AppendChild(child, childSize, child.Margins * childScale);
        }

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

    protected static Vector2 GetChildPositionAnchorWise(UIAnchor parentAnchor, UIAnchor anchor, Rectangle parentContentRect, Vector2 contentSize)
    {
        Vector2 offset = Vector2.Zero;

        switch (parentAnchor)
        {
            case UIAnchor.TopLeft:
            case UIAnchor.CenterLeft:
            case UIAnchor.BottomLeft:
                offset.X += parentContentRect.X;
                break;
            case UIAnchor.TopCenter:
            case UIAnchor.CenterCenter:
            case UIAnchor.BottomCenter:
                offset.X += parentContentRect.Center.X;
                break;
            case UIAnchor.TopRight:
            case UIAnchor.CenterRight:
            case UIAnchor.BottomRight:
                offset.X += parentContentRect.Right;
                break;
        }

        switch (parentAnchor)
        {
            case UIAnchor.TopLeft:
            case UIAnchor.TopCenter:
            case UIAnchor.TopRight:
                offset.Y += parentContentRect.Y;
                break;
            case UIAnchor.CenterLeft:
            case UIAnchor.CenterCenter:
            case UIAnchor.CenterRight:
                offset.Y += parentContentRect.Center.Y;
                break;
            case UIAnchor.BottomLeft:
            case UIAnchor.BottomCenter:
            case UIAnchor.BottomRight:
                offset.Y += parentContentRect.Bottom;
                break;
        }

        switch (anchor)
        {
            case UIAnchor.TopCenter:
            case UIAnchor.CenterCenter:
            case UIAnchor.BottomCenter:
                offset.X -= contentSize.X / 2;
                break;
            case UIAnchor.TopRight:
            case UIAnchor.CenterRight:
            case UIAnchor.BottomRight:
                offset.X -= contentSize.X;
                break;
        }

        switch (anchor)
        {
            case UIAnchor.CenterLeft:
            case UIAnchor.CenterCenter:
            case UIAnchor.CenterRight:
                offset.Y -= contentSize.Y / 2;
                break;
            case UIAnchor.BottomLeft:
            case UIAnchor.BottomCenter:
            case UIAnchor.BottomRight:
                offset.Y -= contentSize.Y;
                break;
        }

        return offset;
    }

    #region Trash

    private Vector2 LayoutMode_FreeMeasure(List<UIBaseWindow> children, Vector2 spaceForChildren)
    {
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;

        Vector2 consumedSpace = Vector2.Zero;
        Vector2 usedSpace = Vector2.Zero;
        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

#if NEW_UI
            Vector2 childSize = child.Measure(spaceForChildren);
#else
            Vector2 childSize = child.NEW_Measure(spaceForChildren);
#endif

            if (insideParent)
            {
                // The biggest window decides how big the free layout parent should be.
                usedSpace = Vector2.Max(usedSpace, childSize);

                // Children that reduce parent size (docked) need to be added additionally.
                consumedSpace += childSize * child.GetDockMask();
            }
        }

        // Special case when there is only one child and its the consuming one.
        if (usedSpace == consumedSpace) consumedSpace = Vector2.Zero;

        return usedSpace + consumedSpace;
    }

    private void LayoutMode_FreeLayout(List<UIBaseWindow> children, Rectangle childSpaceRect)
    {
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;

        void InnerLayoutChild(UIBaseWindow child, bool insideParent)
        {
            Vector2 childSize = child._measuredSize;
            Rectangle thisChildSpaceRect = childSpaceRect;

            float childScale = child.GetScale();
            Rectangle childScaledMargins = child.Margins * childScale;

            float marginsX = childScaledMargins.Width + childScaledMargins.X;
            float marginsY = childScaledMargins.Height + childScaledMargins.Y;

            thisChildSpaceRect.X += childScaledMargins.X;
            thisChildSpaceRect.Y += childScaledMargins.Y;
            thisChildSpaceRect.Width -= marginsX;
            thisChildSpaceRect.Height -= marginsY;

            // todo: wtf?
            // Size is part of the child size during measurement,
            // so we need to subtract it here.
            if (insideParent)
            {
                childSize.X -= marginsX;
                childSize.Y -= marginsY;
            }

            bool childFillX = child.FillX && child.Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter;
            bool childFillY = child.FillY && child.Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;

            Vector2 childSizeFilled = childSize;
            if (childFillX) childSizeFilled.X = thisChildSpaceRect.Width;
            if (childFillY) childSizeFilled.Y = thisChildSpaceRect.Height;
            childSizeFilled = Vector2.Clamp(childSizeFilled, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();

            Vector2 childPos = GetChildPositionAnchorWise(child.ParentAnchor, child.Anchor, thisChildSpaceRect, childSizeFilled);

            if (insideParent)
            {
                // Subtract only right and bottom margins as the childPos is the top left.
                if (childFillX) childSize.X = childSpaceRect.Width - childScaledMargins.Width - (childPos.X - childSpaceRect.X);
                if (childFillY) childSize.Y = childSpaceRect.Height - childScaledMargins.Height - (childPos.Y - childSpaceRect.Y);
                childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
            }

            child.Layout(childPos, childSize);
        }

        // Apply docking children first.
        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

            var childDock = child._dock;
            if (!insideParent || childDock == UIDockDirection.None) continue;

            InnerLayoutChild(child, insideParent);

            var childRect = child.Bounds;

            float childScale = child.GetScale();
            Rectangle childScaledMargins = child.Margins * childScale;

            // Add margins manually pepega wtf
            // todo: maybe windows should hold a bounds and bounds with margins rect?
            childRect.X -= childScaledMargins.X;
            childRect.Y -= childScaledMargins.Y;
            childRect.Width += childScaledMargins.X + childScaledMargins.Width;
            childRect.Height += childScaledMargins.Y + childScaledMargins.Height;

            if (childDock == UIDockDirection.Right)
            {
                childSpaceRect.Width -= childRect.Width;
            }
            else if (childDock == UIDockDirection.Bottom)
            {
                childSpaceRect.Height -= childRect.Height;
            }
            else if (childDock == UIDockDirection.Left)
            {
                childSpaceRect.X += childRect.X;
                childSpaceRect.Width -= childRect.Width;
            }
            else if (childDock == UIDockDirection.Top)
            {
                childSpaceRect.Y += childRect.Height;
                childSpaceRect.Height -= childRect.Height;
            }
        }

        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

            var childDock = child._dock;
            if (insideParent && childDock != UIDockDirection.None) continue;

            InnerLayoutChild(child, insideParent);
        }
    }

    private Vector2 LayoutMode_ListMeasure(List<UIBaseWindow> children, Vector2 freeSpace, bool wrap, int axisMask)
    {
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;

        Vector2 pen = Vector2.Zero;

        Vector2 usedSpace = Vector2.Zero;
        float highestOtherAxis = 0;

        int invertedMask = 1 - axisMask;

        float scale = GetScale();
        Vector2 spacing = (ListSpacing * scale).RoundClosest();

        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

#if NEW_UI
            Vector2 childSize = child.Measure(freeSpace - pen);
#else
            Vector2 childSize = child.NEW_Measure(freeSpace);
#endif

            // Dont count space taken by windows outside parent.
            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor) && !IsOutsideParentListLayout(axisMask, child.Anchor);
            if (insideParent)
            {
                pen[axisMask] += childSize[axisMask];
                highestOtherAxis = MathF.Max(highestOtherAxis, childSize[invertedMask]);
                usedSpace[axisMask] = MathF.Max(usedSpace[axisMask], pen[axisMask]);

                bool addSpacing = pen[axisMask] != 0; // Skip spacing at start.
                if (addSpacing) pen[axisMask] += spacing[axisMask];
            }
        }

        usedSpace[invertedMask] = pen[invertedMask] + highestOtherAxis;
        return usedSpace;
    }

    private void LayoutMode_ListLayout(List<UIBaseWindow> children, Rectangle childSpaceRect, bool wrap, int axisMask)
    {
        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;

        Vector2 pen = Vector2.Zero;
        Vector2 prevChildSize = Vector2.Zero;

        int invertedMask = 1 - axisMask;

        float scale = GetScale();
        Vector2 spacing = (ListSpacing * scale).RoundClosest();

        for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        {
            UIBaseWindow child;
            if (i < children.Count)
            {
                child = children[i];
                if (child.RelativeTo != null) continue;
            }
            else
            {
                child = relativeToMe[i - children.Count];
            }

            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);

            Vector2 childSize = child._measuredSize;
            Rectangle thisChildSpaceRect = childSpaceRect;

            // Don't add spacing before 0 size windows.
            bool addSpacing = insideParent && childSize[axisMask] != 0 && prevChildSize[axisMask] != 0;
            if (addSpacing) pen[axisMask] += spacing[axisMask];

            thisChildSpaceRect.Position += pen;
            thisChildSpaceRect.Size -= pen;

            float childScale = child.GetScale();
            Rectangle childScaledMargins = child.Margins * childScale;

            float marginsX = childScaledMargins.Width + childScaledMargins.X;
            float marginsY = childScaledMargins.Height + childScaledMargins.Y;

            thisChildSpaceRect.X += childScaledMargins.X;
            thisChildSpaceRect.Y += childScaledMargins.Y;
            thisChildSpaceRect.Width -= marginsX;
            thisChildSpaceRect.Height -= marginsY;

            // todo: wtf?
            // Size is part of the child size during measurement,
            // so we need to subtract it here.
            if (insideParent)
            {
                childSize.X -= marginsX;
                childSize.Y -= marginsY;
            }

            bool childFillX = child.FillX && child.Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter;
            bool childFillY = child.FillY && child.Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;

            if (axisMask == 1)
            {
                bool a = true;
            }

            if (axisMask == 0 && i != children.Count - 1) childFillX = false;
            if (axisMask == 1 && i != children.Count - 1) childFillY = false;

            Vector2 childSizeFilled = childSize;
            if (childFillX) childSizeFilled.X = thisChildSpaceRect.Width;
            if (childFillY) childSizeFilled.Y = thisChildSpaceRect.Height;
            childSizeFilled = Vector2.Clamp(childSizeFilled, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();

            Vector2 childPos = GetChildPositionAnchorWise(child.ParentAnchor, child.Anchor, thisChildSpaceRect, childSizeFilled);

            if (insideParent)
            {
                // Subtract only right and bottom margins as the childPos is the top left.
                if (childFillX) childSize.X = childSpaceRect.Width - childScaledMargins.Width - (childPos.X - childSpaceRect.X);
                if (childFillY) childSize.Y = childSpaceRect.Height - childScaledMargins.Height - (childPos.Y - childSpaceRect.Y);
                childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
            }

            child.Layout(childPos, childSize);

            pen[axisMask] += childSize[axisMask];
            prevChildSize = childSize;
        }
    }

    private Vector2 LayoutMode_ListLayoutAAA(List<UIBaseWindow> children, Rectangle childSpaceRect, bool wrap, int axisMask)
    {
        if (Id == "PanelItself")
        {
            bool a = true;
        }

        List<UIBaseWindow> relativeToMe = Controller?.GetWindowsRelativeToWindow(this) ?? EMPTY_CHILDREN_LIST;

        Vector2 pen = Vector2.Zero;

        Vector2 usedSpace = Vector2.Zero;
        float highestOtherAxis = 0;

        int invertedMask = 1 - axisMask;

        float scale = GetScale();
        Vector2 spacing = (ListSpacing * scale).RoundClosest();

        //// Calculate the size for filling windows.
        //// All leftover size will be equally distributed.
        //// For this we basically need to precalculate the layout.
        //Vector2 fillingPen = Vector2.Zero;
        //Vector2 fillingWindowPerAxis = Vector2.Zero;
        //for (var i = 0; i < children.Count + relativeToMe.Count; i++)
        //{
        //    UIBaseWindow child;
        //    if (i < children.Count)
        //    {
        //        child = children[i];
        //        if (child.RelativeTo != null) continue;
        //    }
        //    else
        //    {
        //        child = relativeToMe[i - children.Count];
        //    }

        //    // Need to check to prevent spacing from being added.
        //    // todo: however there is a check for size 0 already, so is this needed?
        //    if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

        //    Vector2 childSize = child._measuredSize;

        //    bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor) && !IsOutsideParentListLayout(axisMask, child.Anchor);

        //    // Don't add spacing before 0 size windows.
        //    bool addSpacing = insideParent && childSize[axisMask] != 0;
        //    if (addSpacing) fillingPen[axisMask] += spacing[axisMask];

        //    // Dont count space taken by windows outside parent.
        //    if (insideParent)
        //    {
        //        fillingPen[axisMask] += childSize[axisMask];

        //        // Count up filling windows per axis.
        //        if (child.FillXInList && axisMask == 0)
        //            fillingWindowPerAxis.X++;
        //        if (child.FillYInList && axisMask == 1)
        //            fillingWindowPerAxis.Y++;
        //    }
        //}

        //fillingWindowPerAxis = Vector2.Max(fillingWindowPerAxis, Vector2.One);
        //Vector2 fillWindowsSize = (childSpaceRect.Size - fillingPen) / fillingWindowPerAxis;

        // Layout the children.
        List<UIBaseWindow>? windowsOutsideParent = null;
        Vector2 prevChildSize = Vector2.Zero;
        for (var i = 0; i < children.Count; i++)
        {
            UIBaseWindow child = children[i];
            if (child.RelativeTo != null) continue;
            if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

            Vector2 childSize = child._measuredSize;
            //if (child.FillXInList || child.FillYInList) // todo: delete?
            //{
            //    if (child.FillXInList) childSize.X = fillWindowsSize.X;
            //    if (child.FillYInList) childSize.Y = fillWindowsSize.Y;

            //    // todo: different max/min sizes in a list would mean that not all filling windows would
            //    // be the same size. This is currently unhandled.
            //    float childScale = child.GetScale();
            //    childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
            //}

            bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor) && !IsOutsideParentListLayout(axisMask, child.Anchor);

            // Don't add spacing before 0 size windows.
            bool addSpacing = insideParent && childSize[axisMask] != 0 && prevChildSize[axisMask] != 0;
            if (addSpacing) pen[axisMask] += spacing[axisMask];

            // Don't count space taken by windows outside parent.
            if (insideParent)
            {
                Rectangle thisChildSpaceRect = childSpaceRect;
                float childScale = child.GetScale();
                Rectangle childScaledMargins = child.Margins * childScale;
                float marginsX = childScaledMargins.Width + childScaledMargins.X;
                float marginsY = childScaledMargins.Height + childScaledMargins.Y;
                thisChildSpaceRect.X += childScaledMargins.X;
                thisChildSpaceRect.Y += childScaledMargins.Y;
                thisChildSpaceRect.Width -= marginsX;
                thisChildSpaceRect.Height -= marginsY;

                // todo: wtf?
                // Size is part of the child size during measurement,
                // so we need to subtract it here.
                if (insideParent)
                {
                    childSize.X -= marginsX;
                    childSize.Y -= marginsY;
                }

                bool childFillX = child.FillX;// && child.Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter;
                bool childFillY = child.FillY;// && child.Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;

                if (axisMask == 0 && i != children.Count - 1) childFillX = false;
                if (axisMask == 1 && i != children.Count - 1) childFillY = false;

                Vector2 childSizeFilled = childSize;
                if (childFillX) childSizeFilled.X = thisChildSpaceRect.Width;
                if (childFillY) childSizeFilled.Y = thisChildSpaceRect.Height;
                childSizeFilled = Vector2.Clamp(childSizeFilled, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();

                Vector2 childPos = GetChildPositionAnchorWise(child.ParentAnchor, child.Anchor, thisChildSpaceRect, childSizeFilled);

                if (insideParent)
                {
                    // Subtract only right and bottom margins as the childPos is the top left.
                    if (childFillX) childSize.X = childSpaceRect.Width - childScaledMargins.Width - (childPos.X - childSpaceRect.X);
                    if (childFillY) childSize.Y = childSpaceRect.Height - childScaledMargins.Height - (childPos.Y - childSpaceRect.Y);
                    childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
                }

                child.Layout(pen + thisChildSpaceRect.Position, childSize);

                pen[axisMask] += childSize[axisMask];
                prevChildSize = childSize;

                highestOtherAxis = MathF.Max(highestOtherAxis, childSize[invertedMask]);
                usedSpace[axisMask] = MathF.Max(usedSpace[axisMask], pen[axisMask]);
            }
            else
            {
                windowsOutsideParent ??= new List<UIBaseWindow>();
                windowsOutsideParent.Add(child);
            }
        }

        if (windowsOutsideParent != null)
            for (var i = 0; i < windowsOutsideParent.Count; i++)
            {
                UIBaseWindow child = windowsOutsideParent[i];

                Vector2 childSize = child._measuredSize;
                float childScale = child.GetScale();

                Rectangle thisChildSpaceRect = childSpaceRect;

                bool childFillX = child.FillX && child.Anchor is not UIAnchor.TopCenter and not UIAnchor.CenterCenter and not UIAnchor.BottomCenter;
                bool childFillY = child.FillY && child.Anchor is not UIAnchor.CenterLeft and not UIAnchor.CenterCenter and not UIAnchor.CenterRight;

                Vector2 childSizeFilled = childSize;
                if (childFillX) childSizeFilled.X = thisChildSpaceRect.Width;
                if (childFillY) childSizeFilled.Y = thisChildSpaceRect.Height;
                childSizeFilled = Vector2.Clamp(childSizeFilled, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();

                Vector2 childPos = GetChildPositionAnchorWise(child.ParentAnchor, child.Anchor, thisChildSpaceRect, childSizeFilled);

                //// Subtract only right and bottom margins as the childPos is the top left.
                //if (childFillX) childSize.X = childSpaceRect.Width - childScaledMargins.Width - (childPos.X - childSpaceRect.X);
                //if (childFillY) childSize.Y = childSpaceRect.Height - childScaledMargins.Height - (childPos.Y - childSpaceRect.Y);
                //childSize = Vector2.Clamp(childSize, child.MinSize * childScale, child.MaxSize * childScale).Ceiling();
                child.Layout(childPos, childSize);
            }

        usedSpace[invertedMask] = pen[invertedMask] + highestOtherAxis;
        return usedSpace;
    }

    #endregion
}