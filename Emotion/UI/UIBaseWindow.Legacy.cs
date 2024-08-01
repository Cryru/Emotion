#nullable enable

namespace Emotion.UI;

#if !NEW_UI

public partial class UIBaseWindow : IRenderable, IComparable<UIBaseWindow>, IEnumerable<UIBaseWindow>
{
    [Obsolete("Deprecated for HandleInput/ChildrenHandleInput")]
    public bool InputTransparent
    {
        get => !ChildrenHandleInput;
        set => ChildrenHandleInput = !value;
    }

    /// <summary>
    /// All child windows will be of the width of the widest child window.
    /// Only works on vertical list layout mode.
    /// </summary>
    public bool ChildrenAllSameWidth { get; set; } = false;

    protected Vector2 _measuredSize;

    protected virtual Vector2 InternalMeasure(Vector2 space)
    {
        return space;
    }

    public bool StretchX { get; set; }

    public bool StretchY { get; set; }

    public virtual Vector2 CalculateContentPos(Vector2 parentPos, Vector2 parentSize, Rectangle parentScaledPadding)
    {
        float scale = GetScale();
        var parentSpaceForChild = new Rectangle(0, 0, parentSize);
        Rectangle childScaledMargins = Margins * scale;
        if (AnchorsInsideParent(ParentAnchor, Anchor))
        {
            parentSpaceForChild.X += childScaledMargins.X;
            parentSpaceForChild.Y += childScaledMargins.Y;
            parentSpaceForChild.Width -= childScaledMargins.Width;
            parentSpaceForChild.Height -= childScaledMargins.Height;

            parentSpaceForChild.X += parentScaledPadding.X;
            parentSpaceForChild.Y += parentScaledPadding.Y;
            parentSpaceForChild.Width -= parentScaledPadding.Width;
            parentSpaceForChild.Height -= parentScaledPadding.Height;
        }
        else
        {
            bool applyYMargin = ParentAnchor is UIAnchor.TopCenter;
            if (ParentAnchor is UIAnchor.TopLeft or UIAnchor.TopRight && Anchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight) applyYMargin = true;

            bool applyXMargin = ParentAnchor is UIAnchor.CenterLeft;
            if (ParentAnchor is UIAnchor.TopLeft or UIAnchor.BottomLeft && Anchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight) applyXMargin = true;

            if (applyYMargin)
                parentSpaceForChild.Y -= childScaledMargins.Height;
            else if (applyXMargin)
                parentSpaceForChild.X -= childScaledMargins.Width;

            parentSpaceForChild.Width += childScaledMargins.X;
            parentSpaceForChild.Height += childScaledMargins.Y;

            // Quirk: Parent padding will be applied to out of parent children.
            // This might be a bad idea, but allows for some interesting layouts.
            // Leaving it in for now.
            parentSpaceForChild.X += parentScaledPadding.X;
            parentSpaceForChild.Y += parentScaledPadding.Y;
            parentSpaceForChild.Width -= parentScaledPadding.Width;
            parentSpaceForChild.Height -= parentScaledPadding.Height;
        }

        //Debugger?.RecordMetric(this, "Layout_ParentContentRect", parentSpaceForChild);
        return parentPos + GetUIAnchorPosition(ParentAnchor, parentSize, parentSpaceForChild, Anchor, _measuredSize);
    }

    protected virtual Vector2 GetChildrenLayoutSize(Vector2 space, Vector2 measuredSize, Vector2 paddingSize)
    {
        Vector2 freeSpace = StretchX || StretchY ? space : measuredSize;
        freeSpace.X -= paddingSize.X;
        freeSpace.Y -= paddingSize.Y;
        return freeSpace;
    }

    private List<UIBaseWindow> _simulateNewLayoutList = new List<UIBaseWindow>(1);

    // On Rounding In UI Layout:
    // Sizes should always be rounded up.
    // Positions should always be rounded down.
    // Offsets (spacings) should always be rounded to the closest.
    protected virtual Vector2 Measure(Vector2 space)
    {
        if (UseNewLayoutSystem)
        {
            if (Parent == null) // controller
                return NEW_Measure(space);

            // Simulate that this child was layouted by a "new layout" parent.
            _simulateNewLayoutList.Clear();
            _simulateNewLayoutList.Add(this);
            return Parent!.LayoutMode_FreeMeasure(_simulateNewLayoutList, space);
        }

        float scale = GetScale();
        if (AnchorsInsideParent(ParentAnchor, Anchor))
        {
            Rectangle scaledMargins = Margins * scale;
            space.X -= scaledMargins.X + scaledMargins.Width;
            space.Y -= scaledMargins.Y + scaledMargins.Height;
        }
        else
        {
            space = Controller!.Size;
        }

        Vector2 contentSize = InternalMeasure(space);
        contentSize = Vector2.Clamp(contentSize, MinSize * scale, MaxSize * scale).Ceiling();
        AfterMeasure(contentSize);
        Vector2 usedSpace = Vector2.Zero;
        Rectangle scaledPadding = Paddings * scale;
        var paddingSize = new Vector2(scaledPadding.X + scaledPadding.Width, scaledPadding.Y + scaledPadding.Height);
        if (Children != null)
        {
            bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
            Vector2 scaledSpacing = (ListSpacing * scale).RoundClosest();
            Vector2 pen = Vector2.Zero;
            Vector2 spaceClampedToConstraints = Vector2.Clamp(space, MinSize * scale, MaxSize * scale).Ceiling();
            Vector2 spaceForChildren = GetChildrenLayoutSize(spaceClampedToConstraints, contentSize, paddingSize);
            float highestOnRow = 0;
            float widestInColumn = 0;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                float childScale = child.GetScale();
                bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
                LayoutMode layoutMode = LayoutMode;
                if (child.RelativeTo != null) layoutMode = LayoutMode.Free;
                switch (layoutMode)
                {
                    case LayoutMode.Free:
                    {
                        if (child.RelativeTo != null)
                        {
                            if (child.UseNewLayoutSystem) continue;

                            UIBaseWindow? win = GetWindowById(child.RelativeTo) ?? Controller?.GetWindowById(child.RelativeTo);
                            if (win != null)
                            {
                                // All windows are measured in one pass. For the "relative to" measure to work, windows attached to other windows need
                                // to be lower in the hierarchy than/following their attached parent.
                                child.Measure(win.Size);
                                continue;
                            }

                            Engine.Log.Warning($"{this} tried to layout relative to {child.RelativeTo} but it couldn't find it.", "UI");
                        }

                        Vector2 childSize = child.Measure(spaceForChildren);
                        if (insideParent)
                        {
                            Rectangle childScaledMargins = child.Margins * childScale;
                            usedSpace = Vector2.Max(usedSpace, childSize + new Vector2(childScaledMargins.X + childScaledMargins.Width, childScaledMargins.Y + childScaledMargins.Height));
                        }

                        break;
                    }
                    case LayoutMode.HorizontalListWrap:
                    case LayoutMode.HorizontalList:
                    {
                        bool addSpacing = insideParent && pen.X != 0; // Skip spacing at start of row.
                        Vector2 childSpace = wrap ? spaceForChildren : spaceForChildren - pen; // Give full space as available space if wrapping.
                        if (addSpacing)
                            childSpace.X -= scaledSpacing.X;
                        Vector2 childSize = child.Measure(childSpace);
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
                        if (!insideParent) continue;
                        Vector2 childScaledOffset = child.Offset * childScale;
                        Rectangle childScaledMargins = child.Margins * childScale;
                        float spaceTaken = childSize.X + childScaledOffset.X + childScaledMargins.X + childScaledMargins.Width;
                        if (wrap && pen.X + spaceTaken > spaceForChildren.X)
                        {
                            pen.X = 0;
                            pen.Y += highestOnRow + scaledSpacing.Y;
                            highestOnRow = 0;
                            addSpacing = false;
                        }

                        if (addSpacing) pen.X += scaledSpacing.X;
                        pen.X += spaceTaken;
                        highestOnRow = MathF.Max(highestOnRow, childSize.Y + childScaledMargins.Y + childScaledMargins.Height);
                        usedSpace.X = MathF.Max(usedSpace.X, pen.X);
                        usedSpace.Y = pen.Y + highestOnRow;
                        break;
                    }
                    case LayoutMode.VerticalListWrap:
                    case LayoutMode.VerticalList:
                    {
                        bool addSpacing = insideParent && pen.Y != 0;
                        Vector2 childSpace = wrap ? spaceForChildren : spaceForChildren - pen;
                        if (addSpacing)
                            childSpace.Y -= scaledSpacing.Y;
                        Vector2 childSize = child.Measure(childSpace);
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
                        if (!insideParent) continue;
                        Vector2 childScaledOffset = child.Offset * childScale;
                        Rectangle childScaledMargins = child.Margins * childScale;
                        float spaceTaken = childSize.Y + childScaledOffset.Y + childScaledMargins.Y + childScaledMargins.Height;
                        if (wrap && pen.Y + spaceTaken > spaceForChildren.Y)
                        {
                            pen.Y = 0;
                            pen.X += widestInColumn + scaledSpacing.X;
                            widestInColumn = 0;
                            addSpacing = false;
                        }

                        if (addSpacing) pen.Y += scaledSpacing.Y;
                        pen.Y += spaceTaken;
                        widestInColumn = MathF.Max(widestInColumn, childSize.X + childScaledMargins.X + childScaledMargins.Width);
                        usedSpace.X = pen.X + widestInColumn;
                        usedSpace.Y = MathF.Max(usedSpace.Y, pen.Y);
                        break;
                    }
                }
            }
        }

        Vector2 minSizeScaled = MinSize * scale;
        float measuredX = StretchX ? MathF.Max(usedSpace.X + paddingSize.X, minSizeScaled.X) : contentSize.X;
        float measuredY = StretchY ? MathF.Max(usedSpace.Y + paddingSize.Y, minSizeScaled.Y) : contentSize.Y;
        _measuredSize = new Vector2(measuredX, measuredY);
        _measuredSize = _measuredSize.Ceiling();
        AfterMeasureChildren(usedSpace);

        if (_measuredSize.X < 0 || _measuredSize.Y < 0)
        {
            Engine.Log.Warning($"UIWindow of id {Id} measured with a size smaller than 0.", MessageSource.UI, true);
            _measuredSize.X = MathF.Max(_measuredSize.X, 0);
            _measuredSize.Y = MathF.Max(_measuredSize.Y, 0);
        }

        Size = _measuredSize;
        return Size;
    }

    protected void ChildrenAreAllAsWideAsWidest()
    {
        Assert(LayoutMode == LayoutMode.VerticalList);
        if (Children != null)
        {
            float width = 0;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
                width = MathF.Max(width, child.Size.X);
            }

            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.Size = new Vector2(width, child.Height);
                child._measuredSize = new Vector2(width, child.Height);
            }
        }
    }

    protected void Layout(Vector2 contentPos)
    {
        if (UseNewLayoutSystem)
        {
            if (Parent == null) // controller
            {
                Layout(contentPos, _measuredSize);
                return;
            }

            // Simulate that this child was layouted by a "new layout" parent.
            _simulateNewLayoutList.Clear();
            _simulateNewLayoutList.Add(this);
            Parent!.LayoutMode_FreeLayout(_simulateNewLayoutList, Parent!.Bounds);
            return;
        }

        float scale = GetScale();
        Assert(Size == _measuredSize);
        Size = _measuredSize;
        if (ChildrenAllSameWidth) ChildrenAreAllAsWideAsWidest();
        contentPos += Offset * scale;
        contentPos = BeforeLayout(contentPos);
        Position = contentPos.RoundClosest().ToVec3(Z);
        // Invalidate transformations.
        if (_transformationStackBacking != null) _transformationStackBacking.MatrixDirty = true;
        if (Children != null)
        {
            bool wrap = LayoutMode is LayoutMode.HorizontalListWrap or LayoutMode.VerticalListWrap;
            Vector2 scaledSpacing = (ListSpacing * scale).RoundClosest();
            Rectangle parentPadding = Paddings * scale;
            Vector2 pen = Vector2.Zero;
            Vector2 freeSpace = _measuredSize;
            float highestOnRow = 0;
            float widestInColumn = 0;
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                float childScale = child.GetScale();
                bool insideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
                LayoutMode layoutMode = LayoutMode;
                if (child.RelativeTo != null || !insideParent) layoutMode = LayoutMode.Free;
                switch (layoutMode)
                {
                    case LayoutMode.Free:
                    {
                        UIBaseWindow parent = this;
                        Vector2 parentPos = parent.Position2;
                        Vector2 parentSize = parent.Size;
                        if (child.RelativeTo != null)
                        {
                            if (child.UseNewLayoutSystem) continue;

                            UIBaseWindow? win = GetWindowById(child.RelativeTo) ?? Controller?.GetWindowById(child.RelativeTo);
                            if (win != null)
                            {
                                parent = win;
                                parentPos = win.Position2;
                                parentSize = win.Size;
                            }
                            else
                            {
                                Engine.Log.Warning($"{this} tried to layout relative to {child.RelativeTo} but it couldn't find it.", "UI");
                            }
                        }

                        Vector2 childPos = child.CalculateContentPos(parentPos, parentSize, parent.Paddings * parent.GetScale());
                        child.Layout(childPos);
                        break;
                    }
                    case LayoutMode.HorizontalListWrap:
                    case LayoutMode.HorizontalList:
                    {
                        bool addSpacing = insideParent && pen.X != 0;
                        Vector2 childOffsetScaled = child.Offset * childScale;
                        Rectangle childMarginsScaled = child.Margins * childScale;
                        float spaceTaken = child.Size.X + childOffsetScaled.X + childMarginsScaled.X + childMarginsScaled.Width;
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
                        if (wrap && pen.X + spaceTaken > freeSpace.X)
                        {
                            pen.X = 0;
                            pen.Y += highestOnRow + scaledSpacing.Y;
                            highestOnRow = 0;
                            addSpacing = false;
                        }

                        if (addSpacing)
                            pen.X += scaledSpacing.X;
                        // Dont count space taken by windows outside parent.
                        bool windowTakesSpace = insideParent && (child.Visible || !child.DontTakeSpaceWhenHidden);
                        if (windowTakesSpace) highestOnRow = MathF.Max(highestOnRow, child.Size.Y + childMarginsScaled.Y + childMarginsScaled.Height);
                        bool anchorInList = insideParent && child.Anchor != UIAnchor.CenterRight && child.Anchor != UIAnchor.TopRight && child.Anchor != UIAnchor.BottomRight;
                        // Child space is constrained to allow some anchors to work as expected within lists.
                        Vector2 childSpace = anchorInList ? new Vector2(child.Size.X, freeSpace.Y) : freeSpace - pen;
                        Vector2 pos = child.CalculateContentPos(pen + contentPos, childSpace, parentPadding);
                        child.Layout(pos);
                        if (!windowTakesSpace) continue;
                        pen.X += spaceTaken;
                        break;
                    }
                    case LayoutMode.VerticalListWrap:
                    case LayoutMode.VerticalList:
                    {
                        bool addSpacing = insideParent && pen.Y != 0;
                        Vector2 childOffsetScaled = child.Offset * childScale;
                        Rectangle childMarginsScaled = child.Margins * childScale;
                        float spaceTaken = child.Size.Y + childOffsetScaled.Y + childMarginsScaled.Y + childMarginsScaled.Height;
                        if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;
                        if (wrap && pen.Y + spaceTaken > freeSpace.Y)
                        {
                            pen.Y = 0;
                            pen.X += widestInColumn + scaledSpacing.X;
                            widestInColumn = 0;
                            addSpacing = false;
                        }

                        if (addSpacing)
                            pen.Y += scaledSpacing.Y;
                        // Dont count space taken by windows outside parent.
                        bool windowTakesSpace = insideParent && (child.Visible || !child.DontTakeSpaceWhenHidden);
                        if (windowTakesSpace) widestInColumn = MathF.Max(widestInColumn, child.Size.X + childMarginsScaled.X + childMarginsScaled.Width);
                        Vector2 childSpace = insideParent ? new Vector2(freeSpace.X, child.Size.Y) : freeSpace - pen;
                        Vector2 pos = child.CalculateContentPos(pen + contentPos, childSpace, parentPadding);
                        child.Layout(pos);
                        if (!windowTakesSpace) continue;
                        pen.Y += spaceTaken;
                        break;
                    }
                }
            }
        }

        // Construct input detecting boundary.
        _inputBoundsWithChildren = Bounds;
        if (Children != null)
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                _inputBoundsWithChildren = Rectangle.Union(child._inputBoundsWithChildren, _inputBoundsWithChildren);
            }

        AfterLayout();
    }

    protected virtual void AfterMeasureChildren(Vector2 usedSpace)
    {
    }
}
#else
public partial class UIBaseWindow
{
	// todo: delete, here for compilation reasons
	public bool StretchX { get; set; }
	public bool StretchY { get; set; }

	protected virtual Vector2 GetChildrenLayoutSize(Vector2 space, Vector2 measuredSize, Vector2 paddingSize)
	{
		Vector2 freeSpace = StretchX || StretchY ? space : measuredSize;
		freeSpace.X -= paddingSize.X;
		freeSpace.Y -= paddingSize.Y;
		return freeSpace;
	}

	public virtual Vector2 CalculateContentPos(Vector2 parentPos, Vector2 parentSize, Rectangle parentScaledPadding)
	{
		return Vector2.Zero;
	}

	protected virtual void AfterMeasureChildren(Vector2 usedSpace)
	{
	}
}
#endif