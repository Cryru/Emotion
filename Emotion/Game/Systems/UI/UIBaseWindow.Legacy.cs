#nullable enable

namespace Emotion.Game.Systems.UI;

#if !NEW_UI

public partial class UIBaseWindow
{
    /// <summary>
    /// Whether to layout the window using the new layout system.
    /// Off by default due to legacy compatibility, will be on by default once the new system is stable.
    /// </summary>
    public bool UseNewLayoutSystem = true;

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