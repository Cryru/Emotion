#nullable enable

#region Using

using Emotion.Core.Systems.IO;
using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.UI2;

#endregion

namespace Emotion.Game.Systems.UI;

public enum UIWindowState
{
    Uninitialized,
    Open,
    Closed
}

public partial class UIBaseWindow
{
    public string Name = string.Empty;

    [DontSerialize]
    public UIWindowState State = UIWindowState.Uninitialized;

    [SerializeNonPublicGetSet]
    public O_UIWindowLayoutMetrics Layout { get; private set; } = new O_UIWindowLayoutMetrics();

    [SerializeNonPublicGetSet]
    public O_UIWindowVisuals Visuals { get; private set; } = new O_UIWindowVisuals();

    public O_UIWindowCalculatedMetrics CalculatedMetrics { get; private set; } = new O_UIWindowCalculatedMetrics();

    #region Lifecycle

    public void RestoreFromSerialized()
    {
        EnsureParentLinks();
    }

    private void SetStateOpened()
    {
        Assert(State != UIWindowState.Open);
        if (State == UIWindowState.Open) return;

        State = UIWindowState.Open;
        OnOpen();
        foreach (UIBaseWindow child in Children)
        {
            child.SetStateOpened();
        }
    }

    protected virtual void OnOpen()
    {

    }

    private void SetStateClosed()
    {
        if (State != UIWindowState.Open) return;

        Assert(State != UIWindowState.Closed);
        State = UIWindowState.Closed;
        OnClose();
        foreach (UIBaseWindow child in Children)
        {
            child.SetStateClosed();
        }
    }
    protected virtual void OnClose()
    {

    }

    #endregion

    #region DeleteMe

    public virtual void AttachedToController(UIController controller)
    {

    }

    public virtual void DetachedFromController(UIController controller)
    {

    }

    protected virtual bool RenderInternal(Renderer c)
    {
        return false;
    }

    protected virtual void AfterRenderChildren(Renderer c)
    {
    }

    #endregion

    #region Hierarchy

    /// <summary>
    /// This window's parent.
    /// </summary>
    [DontSerialize]
    public UIBaseWindow Parent = null!; // note: only UISystem doesn't have a parent.

    /// <summary>
    /// Children of this window.
    /// </summary>
    [SerializeNonPublicGetSet]
    public List<UIBaseWindow> Children { get; set; } = new List<UIBaseWindow>(0);

    public virtual void AddChild(UIBaseWindow? child)
    {
        Assert(child != null || child == this);
        if (child == this || child == null) return;

        Assert(child.State != UIWindowState.Open, "Adding a child that is already attached to the UI system.");
        if (child.State == UIWindowState.Open) return;

        // Check for duplicate ids
        if (Engine.Configuration.DebugMode && !string.IsNullOrEmpty(child.Id))
        {
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow c = Children[i];
                if (c.Id == child.Id)
                {
                    Engine.Log.Warning($"Child with duplicate id was added - {child.Id}", "UI");
                    break;
                }
            }
        }

        child.EnsureParentLinks();
        child.Parent = this;
        Children.Add(child);

        // Custom insertion sort as Array.Sort is unstable
        // Isn't too problematic performance wise since adding children shouldn't happen often.
        for (var i = 1; i < Children.Count; i++)
        {
            UIBaseWindow thisC = Children[i];
            for (int j = i - 1; j >= 0;)
            {
                UIBaseWindow otherC = Children[j];
                if (thisC.CompareTo(otherC) < 0)
                {
                    Children[j + 1] = otherC;
                    Children[j] = thisC;
                    j--;
                }
                else
                {
                    break;
                }
            }
        }

        if (State == UIWindowState.Open)
            child.SetStateOpened();

        Invalidate();
    }

    /// <summary>
    /// Parents aren't serialized so the links need to be reestablished once the UI is loaded.
    /// </summary>
    private void EnsureParentLinks()
    {
        foreach (UIBaseWindow child in Children)
        {
            child.Parent = this;
            child.EnsureParentLinks();
        }
    }

    public virtual void RemoveChild(UIBaseWindow child)
    {
        Assert(child.Parent == this);

        Children.Remove(child);
        child.SetStateClosed();

        Invalidate();
    }

    public bool IsWithin(UIBaseWindow? within)
    {
        if (within == null) return false;
        if (within == this) return true;

        if (RelativeTo != null)
        {
            UIBaseWindow? relativeToWin = Controller?.GetWindowById(RelativeTo);
            if (relativeToWin == null) return false;
            return relativeToWin.IsWithin(within);
        }

        UIBaseWindow? parent = Parent;
        while (parent != null)
        {
            if (parent == within) return true;
            parent = parent.Parent;
        }

        return false;
    }

    // Helpers
    // ------

    protected T? FindParentOfType<T>() where T : UIBaseWindow
    {
        UIBaseWindow? parent = Parent;
        while (parent != null && parent is not T)
        {
            parent = parent.Parent;
        }
        return (T?)parent;
    }

    public virtual void ClearChildren()
    {
        foreach (UIBaseWindow child in Children)
        {
            Assert(child.Parent == this);
            child.SetStateClosed();
        }
        Children.Clear();

        Invalidate();
    }

    public void Close()
    {
        AssertNotNull(Parent);
        if (Parent == null) return;
        Parent.RemoveChild(this);
    }

    #endregion

    #region Updates

    public void Invalidate()
    {
        InvalidateLayout();
    }

    private void InvalidateAssets()
    {
        _needsLoading = true;
    }

    protected bool _needsLayout = true;

    public virtual void InvalidateLayout()
    {
        _needsLayout = true;

        // Note: since this will reach all the way to the UISystem we need to check nullability of parent.
        Parent?.InvalidateLayout(); // todo: not always the case
    }

    #endregion

    #region Loading

    private bool _needsLoading = true;
    private Coroutine _loadingRoutine = Coroutine.CompletedRoutine;

    public bool IsLoading()
    {
        return _needsLoading || !_loadingRoutine.Finished;
    }

    #endregion

    #region Per-Frame Methods

    public void Render(Renderer c)
    {
        Assert(State == UIWindowState.Open);

        // Draw background
        if (Visuals.Color.A != 0)
            c.RenderSprite(CalculatedMetrics.Position, CalculatedMetrics.Size, Visuals.Color);

        // todo: Draw border

        InternalRender(c);
        RenderChildren(c);
    }

    protected virtual void InternalRender(Renderer r)
    {

    }

    protected virtual void RenderChildren(Renderer r)
    {
        foreach (UIBaseWindow child in Children)
        {
            if (!child.Visuals.Visible) continue;
            if (child.IsLoading()) continue;

            //if (child.OverlayWindow) continue;
            child.Render(r);
        }
    }

    public void Update()
    {
        Assert(State == UIWindowState.Open);

        if (_needsLayout)
        {
            CalculatedMetrics.Size = MeasureWindow();
            GrowWindow();
            LayoutWindow(CalculatedMetrics.Position);
        }

        if (_needsLoading)
        {
            _needsLoading = false;
        }

        foreach (UIBaseWindow child in Children)
        {
            child.Update();
        }
    }

    protected virtual bool UpdateInternal()
    {
        // nop
        return true;
    }

    #endregion

    #region Layout

    protected virtual Vector2 InternalGetWindowMinSize()
    {
        return Vector2.Zero;
    }

    protected Vector2 MeasureWindow()
    {
        Vector2 mySize = InternalGetWindowMinSize();
        mySize += Layout.Padding.TopLeft;
        mySize += Layout.Padding.BottomRight;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.Free:
                foreach (UIBaseWindow child in Children)
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
                foreach (UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Fixed)
                        windowMinSize.X = MathF.Max(windowMinSize.X, child.Layout.SizingX.Size);
                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Fixed)
                        windowMinSize.Y = MathF.Max(windowMinSize.Y, child.Layout.SizingY.Size);
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
                foreach (UIBaseWindow child in Children)
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
                foreach (UIBaseWindow child in Children)
                {
                    float listSize = child.CalculatedMetrics.Size[listMask];
                    listRemainingSize -= listSize;
                }
                listRemainingSize -= Layout.ListSpacing[listMask] * (Children.Count - 1);

                // Grow across list
                foreach (UIBaseWindow child in Children)
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
                    foreach (UIBaseWindow child in Children)
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
                    foreach (UIBaseWindow child in Children)
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
                foreach (UIBaseWindow child in Children)
                {
                    child.GrowWindow();
                }

                break;
        }
    }

    protected void LayoutWindow(Vector2 pos)
    {
        Vector2 myPos = Layout.Offset + pos;
        CalculatedMetrics.Position = myPos;

        Vector2 pen = myPos;
        pen += Layout.Padding.TopLeft;

        switch (Layout.LayoutMode)
        {
            case LayoutMode.Free:
                foreach (UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                }
                break;

            case LayoutMode.HorizontalList:
            case LayoutMode.VerticalList:
                int listMask = Layout.LayoutMode == LayoutMode.HorizontalList ? 0 : 1;
                foreach (UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                    pen[listMask] += child.CalculatedMetrics.Size[listMask] + Layout.ListSpacing[listMask];
                }
                break;
        }

        _needsLayout = false;
    }

    #endregion
}