#nullable enable

#region Using

using Emotion.Core.Utility.Coroutines;
using Emotion.Game.Systems.UI2;
using static Emotion.Game.Systems.UI2.UILayoutMethod;

#endregion

namespace Emotion.Game.Systems.UI;

public enum UIWindowState
{
    Uninitialized,
    Open,
    Closed
}

public class UIContainer : UIBaseWindow
{
    public UIContainer()
    {
        Layout.SizingX = UISizing.Fit();
        Layout.SizingY = UISizing.Fit();
    }
}

public partial class UIBaseWindow
{
    /// <summary>
    /// Unique identifier for this window within its parent, to be used with GetWindowById.
    /// If two windows share a name in the hierarchy the one closer to the parent GetWindowById is called from will be returned.
    /// </summary>
    public string Name = string.Empty;

    [DontSerialize]
    public UIWindowState State = UIWindowState.Uninitialized;

    [SerializeNonPublicGetSet]
    public O_UIWindowLayoutMetrics Layout = new O_UIWindowLayoutMetrics();

    [SerializeNonPublicGetSet]
    public O_UIWindowVisuals Visuals = new O_UIWindowVisuals();

    public O_UIWindowCalculatedMetrics CalculatedMetrics = new O_UIWindowCalculatedMetrics();

    #region Lifecycle

    private void SetStateOpened()
    {
        Assert(State != UIWindowState.Open);
        if (State == UIWindowState.Open) return;

        State = UIWindowState.Open;
        foreach (UIBaseWindow child in Children)
        {
            child.SetStateOpened();
        }
        OnOpen();
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

    public virtual UIBaseWindow? FindMouseInput(Vector2 pos)
    {
        return null;
    }


    protected virtual bool RenderInternal(Renderer c)
    {
        return false;
    }

    protected virtual void AfterRenderChildren(Renderer c)
    {
    }

    protected Rectangle _renderBoundsCalculatedFrom; // .Bounds at time of caching.
    private Matrix4x4? _renderBoundsCachedMatrix; // The matrix _renderBounds was generated from.
    protected Rectangle _renderBounds; // Bounds but with any displacements active on the window applied 
    protected Rectangle _renderBoundsWithChildren; // _inputBoundsWithChildren but with any displacements active on the window applied
    private Rectangle _inputBoundsWithChildren; // Bounds unioned with all children bounds.

    public Rectangle RenderBounds
    {
        get => _renderBoundsWithChildren;
    }

    public void EnsureRenderBoundsCached(Renderer c)
    {
        if (c.ModelMatrix == _renderBoundsCachedMatrix && _renderBoundsCalculatedFrom == _inputBoundsWithChildren) return;
        _renderBoundsWithChildren = Rectangle.Transform(_inputBoundsWithChildren, c.ModelMatrix);
        _renderBoundsWithChildren.Position = _renderBoundsWithChildren.Position.Floor();
        _renderBoundsWithChildren.Size = _renderBoundsWithChildren.Size.Ceiling();

        _renderBounds = Rectangle.Transform(Bounds, c.ModelMatrix);
        _renderBounds.Position = _renderBounds.Position.Floor();
        _renderBounds.Size = _renderBounds.Size.Ceiling();

        _renderBoundsCachedMatrix = c.ModelMatrix;
        _renderBoundsCalculatedFrom = _inputBoundsWithChildren;
    }

    public virtual bool IsPointInside(Vector2 pt)
    {
        return _renderBoundsCalculatedFrom != Rectangle.Empty ? _renderBoundsWithChildren.Contains(pt) : _inputBoundsWithChildren.Contains(pt);
    }

    public virtual bool IsInsideRect(Rectangle rect)
    {
        return _renderBoundsCalculatedFrom != Rectangle.Empty ? rect.ContainsInclusive(_renderBoundsWithChildren) : rect.ContainsInclusive(_inputBoundsWithChildren);
    }

    public virtual bool IsInsideOrIntersectRect(Rectangle rect, out bool inside)
    {
        Rectangle checkAgainst = _renderBoundsCalculatedFrom != Rectangle.Empty ? _renderBoundsWithChildren : _inputBoundsWithChildren;
        if (rect.ContainsInclusive(checkAgainst))
        {
            inside = true;
            return true;
        }

        if (rect.IntersectsInclusive(checkAgainst))
        {
            inside = false;
            return true;
        }

        inside = false;
        return false;
    }

    public virtual bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        return true;
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
        if (Engine.Configuration.DebugMode && !string.IsNullOrEmpty(child.Name))
        {
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow c = Children[i];
                if (c.Name == child.Name)
                {
                    Engine.Log.Warning($"Child with duplicate id was added - {child.Name}", "UI");
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

        InvalidateLayout();
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

        InvalidateLayout();
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

        InvalidateLayout();
    }

    public void Close()
    {
        AssertNotNull(Parent);
        if (Parent == null) return;
        Parent.RemoveChild(this);
    }

    #endregion

    #region Updates

    private bool _needsLoading = true;

    protected void InvalidateAssets()
    {
        _needsLoading = true;
    }

    protected bool _needsLayout = true;

    public virtual void InvalidateLayout()
    {
        _needsLayout = true;

        // Note: since this will reach all the way to the UISystem we need to check nullability of parent.
        Parent?.InvalidateLayout(); // todo: if the parent is free layout and not fit, we can skip this
    }

    #endregion

    #region Loading

    private Coroutine _loadingRoutine = Coroutine.CompletedRoutine;

    public bool IsLoading()
    {
        return _needsLoading || !_loadingRoutine.Finished;
    }

    private void AttemptLoad()
    {
        // Already loading
        if (!_loadingRoutine.Finished) return;

        // Try to get the loading routine.
        _loadingRoutine = InternalLoad() ?? Coroutine.CompletedRoutine;
        _needsLoading = false;
    }

    protected virtual Coroutine? InternalLoad()
    {
        return null;
    }

    #endregion

    #region Per-Frame Methods

    public void Render(Renderer r)
    {
        Assert(State == UIWindowState.Open);

        // Draw background
        if (Visuals.Color.A != 0)
            r.RenderSprite(CalculatedMetrics.Position, CalculatedMetrics.Size, Visuals.Color);

        // todo: Draw border
        r.RenderRectOutline(CalculatedMetrics.Position.ToVec3(), CalculatedMetrics.Size, Color.Red);

        InternalRender(r);
        RenderChildren(r);
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

        // Check if needs to load...If loading we don't want to update or draw the UI
        if (_needsLoading) AttemptLoad();
        if (IsLoading()) return;

        if (_needsLayout)
        {
            // A layout can be started at any part of the tree,
            // and all UI below it will be handled.
            CalculatedMetrics.Size = MeasureWindow();
            GrowWindow();
            LayoutWindow(CalculatedMetrics.Position);
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

    #region Input

    public bool ChildrenHandleInput
    {
        get => _childrenHandleInput;
        set
        {
            if (value == _childrenHandleInput) return;
            _childrenHandleInput = value;
            Engine.UI.InvalidateInputFocus();
        }
    }

    private bool _childrenHandleInput = true;

    public bool HandleInput
    {
        get => _handleInput;
        set
        {
            if (value == _handleInput) return;
            _handleInput = value;
            Engine.UI.InvalidateInputFocus();
        }
    }

    private bool _handleInput;

    /// <summary>
    /// Whether the mouse is currently inside this window.
    /// </summary>
    [DontSerialize]
    public bool MouseInside { get; protected set; }

    /// <summary>
    /// Find the window under the mouse cursor in this parent.
    /// This could be either a child window or the parent itself.
    /// </summary>
    public virtual UIBaseWindow? FindWindowUnderMouse(Vector2 pos, bool respectInputHandling = true)
    {
        if (!Visible) return null;

        if (ChildrenHandleInput || !respectInputHandling)
        {
            for (int i = Children.Count - 1; i >= 0; i--) // Top to bottom
            {
                UIBaseWindow win = Children[i];
                if (win.Visible && win.CalculatedMetrics.Bounds.Contains(pos))
                {
                    UIBaseWindow? inChild = win.FindWindowUnderMouse(pos, respectInputHandling);
                    if (inChild != null)
                        return inChild;
                }
            }
        }

        if ((!respectInputHandling || HandleInput) && CalculatedMetrics.Bounds.Contains(pos))
            return this;

        return null;
    }

    protected virtual bool InternalOnKey(Key key, KeyState status, Vector2 mousePos)
    {
        return true;
    }

    #endregion

    #region Helpers

    public bool VisibleAlongTree()
    {
        if (Controller == null) return false;

        UIBaseWindow? parent = Parent;
        while (parent != null)
        {
            if (!parent.Visible) return false;
            parent = parent.Parent;
        }

        return Visible;
    }

    public T? GetParentOfKind<T>() where T : UIBaseWindow
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is T parentAsT)
                return parentAsT;

            parent = parent.Parent;
        }
        return null;
    }

    #endregion

    #region Layout

    protected virtual Vector2 InternalGetWindowMinSize()
    {
        return Vector2.Zero;
    }

    protected Vector2 MeasureWindow()
    {
        if (Layout.ScaleWithResolution)
            CalculatedMetrics.Scale = Layout.Scale * (Parent?.CalculatedMetrics.Scale ?? Vector2.One);
        else
            Layout.Scale = Vector2.One;

        Vector2 childrenSize = Vector2.Zero;
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;
                    childrenSize = Vector2.Max(childrenSize, windowMinSize);
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();

                Vector2 pen = new Vector2();
                foreach (UIBaseWindow child in Children)
                {
                    Vector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;

                    pen[listMask] += windowMinSize[listMask];
                    pen[inverseListMask] = MathF.Max(pen[inverseListMask], windowMinSize[inverseListMask]);
                }

                float totalSpacing = Layout.LayoutMethod.ListSpacing[listMask] * CalculatedMetrics.Scale[listMask] * (Children.Count - 1);

                //bool fitAlongList = listMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                childrenSize[listMask] += pen[listMask] + totalSpacing;
                childrenSize[inverseListMask] += pen[inverseListMask];

                //bool fitAcrossList = inverseListMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                //if (fitAcrossList)
                //    mySize[inverseListMask] += pen[inverseListMask];

                break;
        }

        Vector2 mySize = Vector2.Zero;

        if (Layout.SizingX.Mode == UISizing.UISizingMode.Fixed)
            mySize.X = Layout.SizingX.Size * CalculatedMetrics.Scale.X;
        if (Layout.SizingY.Mode == UISizing.UISizingMode.Fixed)
            mySize.Y = Layout.SizingY.Size * CalculatedMetrics.Scale.Y;

        mySize += Layout.Padding.TopLeft * CalculatedMetrics.Scale;
        mySize += Layout.Padding.BottomRight * CalculatedMetrics.Scale;
        mySize += Layout.Margins.TopLeft * CalculatedMetrics.Scale;
        mySize += Layout.Margins.BottomRight * CalculatedMetrics.Scale;

        Vector2 contentSize = Vector2.Max(childrenSize, InternalGetWindowMinSize());
        mySize += contentSize;

        return Vector2.Clamp(mySize, Layout.MinSize * CalculatedMetrics.Scale, Layout.MaxSize * CalculatedMetrics.Scale);
    }

    protected void GrowWindow()
    {
        // Early out
        if (Children.Count == 0)
            return;

        Vector2 myMeasuredSize = CalculatedMetrics.Size;
        myMeasuredSize -= Layout.Padding.TopLeft * CalculatedMetrics.Scale;
        myMeasuredSize -= Layout.Padding.BottomRight * CalculatedMetrics.Scale;

        myMeasuredSize -= Layout.Margins.TopLeft * CalculatedMetrics.Scale;
        myMeasuredSize -= Layout.Margins.BottomRight * CalculatedMetrics.Scale;

        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.X = MathF.Max(child.CalculatedMetrics.Size.X, myMeasuredSize.X);

                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.Y = MathF.Max(child.CalculatedMetrics.Size.Y, myMeasuredSize.Y);

                    child.GrowWindow();
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();

                float listRemainingSize = myMeasuredSize[listMask];
                foreach (UIBaseWindow child in Children)
                {
                    float listSize = child.CalculatedMetrics.Size[listMask];
                    listRemainingSize -= listSize;
                }
                listRemainingSize -= Layout.LayoutMethod.ListSpacing[listMask] * CalculatedMetrics.Scale[listMask] * (Children.Count - 1);

                // Grow across list
                foreach (UIBaseWindow child in Children)
                {
                    bool growAcrossList = Layout.LayoutMethod.GrowingAcrossList(child.Layout);
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
                        bool growAlongList = Layout.LayoutMethod.GrowingAlongList(child.Layout);
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
                        bool growAlongList = Layout.LayoutMethod.GrowingAlongList(child.Layout);
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
        // Round size
        CalculatedMetrics.Size = CalculatedMetrics.Size.Ceiling();

        Vector2 offsetScaled = (Layout.Offset * CalculatedMetrics.Scale).Round();

        Vector2 myPos = offsetScaled + pos;
        myPos += Layout.Margins.TopLeft * CalculatedMetrics.Scale;
        CalculatedMetrics.Position = myPos.Floor();

        Vector2 pen = myPos;
        pen += Layout.Padding.TopLeft * CalculatedMetrics.Scale;

        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                foreach (UIBaseWindow child in Children)
                {
                    child.LayoutWindow(pen);
                    pen[listMask] += child.CalculatedMetrics.Size[listMask] + Layout.LayoutMethod.ListSpacing[listMask] * CalculatedMetrics.Scale[listMask];
                }
                break;
        }

        _needsLayout = false;
    }

    #endregion
}