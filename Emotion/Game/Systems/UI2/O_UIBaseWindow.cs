#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Game.Systems.UI2;

public partial class O_UIBaseWindow
{
    [DontSerialize]
    public O_UIBaseWindow? Parent;

    public string Name = string.Empty;

    [SerializeNonPublicGetSet]
    public O_UIWindowLayoutMetrics Layout { get; private set; } = new O_UIWindowLayoutMetrics();

    [SerializeNonPublicGetSet]
    public O_UIWindowVisuals Visuals { get; private set; } = new O_UIWindowVisuals();

    [SerializeNonPublicGetSet]
    [DontSerializeButShowInEditor]
    public O_UIWindowCalculatedMetrics CalculatedMetrics { get; private set; } = new O_UIWindowCalculatedMetrics();

    public List<O_UIBaseWindow> Children { get => ChildrenSerialized; init => ChildrenSerialized = value; }

    [SerializeNonPublicGetSet]
    public List<O_UIBaseWindow> ChildrenSerialized { get; private set; } = new List<O_UIBaseWindow>();

    public class O_UIBaseWindowSystemAdapter // Friend class workaround.
    {
        private O_UIBaseWindow _this;

        public O_UIBaseWindowSystemAdapter(O_UIBaseWindow win)
        {
            _this = win;
        }
    }

    [DontSerialize]
    public O_UIBaseWindowSystemAdapter UISystemAdapter { get; private set; }

    public O_UIBaseWindow()
    {
        UISystemAdapter = new O_UIBaseWindowSystemAdapter(this);
    }

    public virtual void Update()
    {
        //SystemDoLayout(UILayoutPass.Measure);
        //SystemDoLayout(UILayoutPass.Grow);
        //SystemDoLayout(UILayoutPass.Layout);
    }

    public void Render(Renderer c)
    {
        var calc = CalculatedMetrics;
        if (Visuals.Color.A != 0)
        {
            c.RenderSprite(calc.Position, calc.Size, Visuals.Color);
        }

        foreach (O_UIBaseWindow win in Children)
        {
            win.Render(c);
        }
    }

    #region Hierarchy

    public void AddChild(O_UIBaseWindow window)
    {
        //Assert(window.Parent == null, "UI window already present in the hierarchy, or wasn't removed properly!");
        //if (window.Parent != null) return;

        Children.Add(window);
        window.Parent = this;
        InvalidateLayout();
    }

    public void RemoveChild(O_UIBaseWindow window)
    {
        Assert(Children.Contains(window), "Tried to remove child that isn't mine!");
        Assert(window.Parent == this);

        Children.Remove(window);
        window.Parent = null;
        InvalidateLayout();
    }

    public void ClearChildren()
    {
        for (var i = Children.Count - 1; i >= 0; i--)
        {
            O_UIBaseWindow child = Children[i];
            RemoveChild(child);
        }
    }

    #endregion

    #region Invalidation

    protected bool _layoutDirty = true;

    public virtual void InvalidateLayout()
    {
        _layoutDirty = true;
        Parent?.InvalidateLayout(); // todo: not always the case
    }

    #endregion

    #region Layout

    public enum UILayoutPass
    {
        None,

        Measure,
        Grow,
        Layout
    }

    protected Vector2 MeasureWindow()
    {
        Vector2 mySize = InternalGetWindowMinSize();
        mySize += Layout.Padding.TopLeft;
        mySize += Layout.Padding.BottomRight;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.Free:
                foreach (O_UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;
                }
                break;

            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                int inverseListMask = listMask == 1 ? 0 : 1;

                Vector2 pen = new Vector2();
                foreach (O_UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;

                    pen[listMask] += windowMinSize[listMask];
                    pen[inverseListMask] = MathF.Max(pen[inverseListMask], windowMinSize[inverseListMask]);
                }

                float totalSpacing = Layout.ListSpacing[listMask] * (Children.Count - 1);

                bool fitAlongList = listMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                if (fitAlongList)
                    mySize[listMask] += pen[listMask] + totalSpacing;

                bool fitAcrossList = inverseListMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                if (fitAcrossList)
                    mySize[inverseListMask] += pen[inverseListMask];

                break;
        }

        return Vector2.Clamp(mySize, Layout.MinSize, Layout.MaxSize);
    }

    protected void GrowWindow()
    {
        // Early out
        if (Children.Count == 0)
            return;

        Vector2 myMeasuredSize = CalculatedMetrics.Size;
        myMeasuredSize -= Layout.Padding.TopLeft;
        myMeasuredSize -= Layout.Padding.BottomRight;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.Free:
                foreach (O_UIBaseWindow child in Children)
                {
                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.X = myMeasuredSize.X;

                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.Y = myMeasuredSize.Y;

                    child.GrowWindow();
                }
                break;

            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                int inverseListMask = listMask == 1 ? 0 : 1;

                float listRemainingSize = myMeasuredSize[listMask];
                foreach (O_UIBaseWindow child in Children)
                {
                    float listSize = child.CalculatedMetrics.Size[listMask];
                    listRemainingSize -= listSize;
                }
                listRemainingSize -= Layout.ListSpacing[listMask] * (Children.Count - 1);

                // Grow across list
                foreach (O_UIBaseWindow child in Children)
                {
                    bool growAcrossList = inverseListMask == 0 ? child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow : child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow;
                    if (growAcrossList)
                        child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask];
                }

                // Grow along list
                while (listRemainingSize > 0)
                {
                    float smallest = float.PositiveInfinity;
                    float secondSmallest = float.PositiveInfinity;
                    int growingCount = 0;
                    foreach (O_UIBaseWindow child in Children)
                    {
                        bool growAlongList = listMask == 0 ? child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow : child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow;
                        if (!growAlongList) continue;

                        growingCount++;

                        float sizeListDirection = child.CalculatedMetrics.Size[listMask];
                        // Initialize smallest
                        if (smallest == float.PositiveInfinity)
                        {
                            smallest = sizeListDirection;
                            continue;
                        }
                        // Smaller than smallest
                        else if (sizeListDirection < smallest)
                        {
                            secondSmallest = smallest;
                            smallest = sizeListDirection;
                        }
                        // Bigger than smallest but smaller than second smallest
                        else if (sizeListDirection > smallest && sizeListDirection < secondSmallest)
                        {
                            secondSmallest = sizeListDirection;
                        }
                    }

                    // Nothing to do.
                    if (growingCount == 0)
                        break;

                    float widthToAdd = MathF.Min(secondSmallest - smallest, listRemainingSize / growingCount);
                    foreach (O_UIBaseWindow child in Children)
                    {
                        bool growAlongList = listMask == 0 ? child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow : child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow;
                        if (!growAlongList) continue;

                        float sizeListDirection = child.CalculatedMetrics.Size[listMask];
                        if (sizeListDirection == smallest)
                        {
                            child.CalculatedMetrics.Size[listMask] += widthToAdd;
                            listRemainingSize -= widthToAdd;
                        }
                    }
                }

                // Now the children can grow their children
                foreach (O_UIBaseWindow child in Children)
                {
                    child.GrowWindow();
                }

                break;
        }
    }

    protected void LayoutWindow(Vector2 pos)
    {
        Vector2 myPos = Layout.Offset + pos;
        CalculatedMetrics.Position = myPos.ToVec3();

        Vector2 pen = myPos;
        pen += Layout.Padding.TopLeft;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.Free:
                foreach (O_UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                }
                break;

            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                foreach (O_UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                    pen[listMask] += child.CalculatedMetrics.Size[listMask] + Layout.ListSpacing[listMask];
                }
                break;
        }

        _layoutDirty = false;
    }

    #endregion

    #region API

    protected virtual Vector2 InternalGetWindowMinSize()
    {
        return Vector2.Zero;
    }

    #endregion

    public override string ToString()
    {
        return $"{(string.IsNullOrEmpty(Name) ? "Window" : Name)}: {Layout.LayoutMode}";
    }
}