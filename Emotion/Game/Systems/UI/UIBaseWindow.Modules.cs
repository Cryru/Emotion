#nullable enable

#region Using

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Editor.EditorUI;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI.New;
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

public partial class UIBaseWindow : IEnumerable<UIBaseWindow>
{
    /// <summary>
    /// Unique identifier for this window within its parent, to be used with GetWindowById.
    /// If two windows share a name in the hierarchy the one closer to the parent GetWindowById is called from will be returned.
    /// </summary>
    public string Name = string.Empty;

    [DontSerialize]
    public UIWindowState State = UIWindowState.Uninitialized;

    [SerializeNonPublicGetSet]
    public UIWindowLayoutConfig Layout = new UIWindowLayoutConfig();

    [SerializeNonPublicGetSet]
    public UIWindowVisualConfig Visuals = new UIWindowVisualConfig();

    public O_UIWindowCalculatedMetrics CalculatedMetrics = new O_UIWindowCalculatedMetrics();

    #region Main Properties

    /// <summary>
    /// Whether the window is visible.
    /// If not, the RenderInternal function will not be called and
    /// children will not be drawn either.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (value == _visible) return;
            _visible = value;

            Engine.UI.InvalidateInputFocus();
            if (DontTakeSpaceWhenHidden)
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

    /// <summary>
    /// The Z axis is combined with that of the parent, whose is combined with that of their parent, and so forth.
    /// This is the Z offset for this window, added to this window and its children.
    /// </summary>
    public float OrderInParent
    {
        get => _priority;
        set
        {
            if (_priority == value) return;
            _priority = value;
            InvalidateLayout();
        }
    }

    protected float _priority;

    #endregion

    public UIBaseWindow()
    {
        Layout.SetWindowOwner(this);
    }

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

        if (Engine.UI.DropdownSpawningWindow == this)
            Engine.UI.CloseDropdown();

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

    public const string SPECIAL_WIN_ID_MOUSE_FOCUS = "MouseFocus";

    /// <summary>
    /// The scale factor applied on the UI.
    /// </summary>
    /// <returns></returns>
    public float GetScale()
    {
        return CalculatedMetrics.ScaleF;
    }

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

    /// <summary>
    /// The point in the parent to anchor the window to.
    /// </summary>
    public UIAnchor ParentAnchor
    {
        get => _parentAnchor;
        set
        {
            if (value == _parentAnchor) return;
            _parentAnchor = value;
            InvalidateLayout();
        }
    }

    private UIAnchor _parentAnchor { get; set; } = UIAnchor.TopLeft;

    /// <summary>
    /// Where the window should anchor to relative to the alignment in its parent.
    /// </summary>
    public UIAnchor Anchor
    {
        get => _anchor;
        set
        {
            if (value == _anchor) return;
            _anchor = value;
            InvalidateLayout();
        }
    }

    private UIAnchor _anchor { get; set; } = UIAnchor.TopLeft;

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
    public List<UIBaseWindow> Children
    {
        get => _children;
        set
        {
            Assert(State != UIWindowState.Open, "You cant substitute children of 'Open' windows.");
            if (State == UIWindowState.Open) return;
            _children = value;
            EnsureParentLinks();
        }
    }
    private List<UIBaseWindow> _children = new List<UIBaseWindow>(0);

    public virtual void AddChild(UIBaseWindow? child)
    {
        Assert(child != null || child == this);
        if (child == this || child == null) return;

        Assert(child.State != UIWindowState.Open, "Adding a child that is already attached to the UI system.");
        if (child.State == UIWindowState.Open) return;

        Assert(State != UIWindowState.Open || GLThread.IsGLThread(), "UI children can only be added from the main thread.");

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

        // Start loading early, dont wait for next update
        child.AttemptLoad();

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
        Assert(State != UIWindowState.Open || GLThread.IsGLThread(), "UI children can only be removed from the main thread.");

        Children.Remove(child);
        child.SetStateClosed();

        InvalidateLayout();
    }

    public bool IsWithin(UIBaseWindow? within)
    {
        if (within == null) return false;
        if (within == this) return true;

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

    public int CompareTo(UIBaseWindow? other)
    {
        if (other == null) return 1;
        return MathF.Sign(OrderInParent - other.OrderInParent);
    }

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
        if (_needsLayout) return;
        if (State != UIWindowState.Open) return;

        // Assert(GLThread.IsGLThread(), "Layout can only be invalidated from the main thread to prevent invalidates mid-update.");

        _needsLayout = true;
        if (Parent != null)
        {
            if (
                Parent.Layout.LayoutMethod.Mode == UIMethodName.Free &&
                Parent.Layout.SizingX.Mode != UISizing.UISizingMode.Fit &&
                Parent.Layout.SizingY.Mode != UISizing.UISizingMode.Fit
            )
                return;

            // Note: since this will reach all the way to the UISystem we need to check nullability of parent.
            Parent?.InvalidateLayout();
        }
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

    protected static void ProxyInvalidateLayout(object? owner)
    {
        if (owner is UIBaseWindow win)
            win.InvalidateLayout();
    }

    #endregion

    #region Per-Frame Methods

    public void Render(Renderer r)
    {
        Assert(State == UIWindowState.Open);

        // Draw background
        if (Visuals.BackgroundColor.A != 0)
            r.RenderSprite(CalculatedMetrics.Position.ToVec2(), CalculatedMetrics.Size.ToVec2(), Visuals.BackgroundColor);

        if (Visuals.Border != 0)
            r.RenderRectOutline(
                CalculatedMetrics.Position.ToVec3(),
                CalculatedMetrics.Size.ToVec2(),
                Visuals.BorderColor,
                Maths.RoundAwayFromZero(Visuals.Border * CalculatedMetrics.ScaleF)
            );

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
            child.Render(r);
        }
    }

    private float _lastUpdateTime;

    public void UpdateLayout()
    {
        if (!_needsLayout)
        {
            foreach (UIBaseWindow child in Children)
            {
                child.UpdateLayout();
            }
            return;
        }

        // A layout can be started at any part of the tree,
        // and all UI below it will be handled.
        if (_useCustomLayout)
        {
            InternalCustomLayout();
            _needsLayout = false;
        }
        else
        {
            PreLayout();
            CalculatedMetrics.Size = MeasureWindow();
            if (Parent != null)
            {
                Assert(Parent.Layout.LayoutMethod.Mode == UIMethodName.Free,
                    "UI layout can only be resumed/initiated from a child of a parent with a layout method set to 'Free'"
                );
                Parent.GrowWindow();
            }
            else
            {
                GrowWindow();
            }
            LayoutWindow(CalculatedMetrics.Position);
        }
    }

    public void Update()
    {
        Assert(State == UIWindowState.Open);

        if (Engine.TotalTime - _lastUpdateTime < 1000) return;
        _lastUpdateTime = Engine.TotalTime;

        // Check if needs to load...If loading we don't want to update or draw the UI
        if (_needsLoading) AttemptLoad();
        if (IsLoading()) return;

        UpdateInternal();
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
            Engine.UI?.InvalidateInputFocus();
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
            Engine.UI?.InvalidateInputFocus(); // Needs null check since the system sets this for itself
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

    public virtual void OnMouseEnter(Vector2 mousePos)
    {
        MouseInside = true;
    }

    public virtual void OnMouseLeft(Vector2 mousePos)
    {
        MouseInside = false;
    }

    public virtual void OnMouseMove(Vector2 mousePos)
    {
    }

    public virtual bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        return true;
    }

    #endregion

    #region Other

    public virtual void OnDropdownStateChanged(bool opened)
    {

    }

    #endregion

    #region Helpers

    /// <summary>
    /// Get a window with the specified id which is either a child of this window,
    /// or below it on the tree.
    /// </summary>
    public virtual UIBaseWindow? GetWindowById(string id)
    {
        if (id == Name) return this;
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].Name == id)
                return Children[i];
        }

        for (var i = 0; i < Children.Count; i++)
        {
            UIBaseWindow? win = Children[i].GetWindowById(id);
            if (win != null) return win;
        }

        return null;
    }

    public TWindow? GetWindowById<TWindow>(string id) where TWindow : UIBaseWindow
    {
        UIBaseWindow? win = GetWindowById(id);
        TWindow? asType = win as TWindow;
        if (asType == null && win != null)
            Engine.Log.Warning($"Window with id {id} found of the {win.GetType().Name} type rather than the {typeof(TWindow).Name} type!", "UI", true);

        return asType;
    }

    private static UIBaseWindow _invalidWindow = new UIBaseWindow() { Name = "Invalid Window" };

    public UIBaseWindow GetWindowByIdSafe(string id)
    {
        UIBaseWindow? win = GetWindowById(id);
        if (win == null) return _invalidWindow;
        return win;
    }

    public bool VisibleAlongTree()
    {
        if (State != UIWindowState.Open)
            return false;

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

    public override string ToString()
    {
        return $"{(string.IsNullOrEmpty(Name) ? "Window" : Name)}: {GetType().ToString().Replace("Emotion.UI.", "")}";
    }

    #endregion

    #region Layout

    protected virtual IntVector2 InternalGetWindowMinSize()
    {
        return IntVector2.Zero;
    }

    protected virtual void InternalOnLayoutComplete()
    {

    }

    protected void PreLayout()
    {
        _needsLayout = false;

        if (Layout.ScaleWithResolution)
            CalculatedMetrics.Scale = Layout.Scale * (Parent?.CalculatedMetrics.Scale ?? Vector2.One);
        else
            Layout.Scale = Vector2.One;

        foreach (UIBaseWindow child in Children)
        {
            child.PreLayout();
        }
    }

    protected IntVector2 MeasureWindow()
    {
        IntVector2 childrenSize = IntVector2.Zero;
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;
                    if (child.IsLoading()) continue;

                    IntVector2 windowMinSize = child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;
                    childrenSize = IntVector2.Max(childrenSize, windowMinSize);
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();

                int listChildrenCount = 0;
                IntVector2 pen = IntVector2.Zero;
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    IntVector2 windowMinSize = child.IsLoading() ? IntVector2.Zero : child.MeasureWindow();
                    child.CalculatedMetrics.Size = windowMinSize;

                    pen[listMask] += windowMinSize[listMask];
                    pen[inverseListMask] = Math.Max(pen[inverseListMask], windowMinSize[inverseListMask]);
                    listChildrenCount++;
                }

                int totalSpacing = GetListSpacing(listMask) * (listChildrenCount - 1);

                //bool fitAlongList = listMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                childrenSize[listMask] += pen[listMask] + totalSpacing;
                childrenSize[inverseListMask] += pen[inverseListMask];

                //bool fitAcrossList = inverseListMask == 0 ? Layout.SizingX.Mode == UISizing.UISizingMode.Fit : Layout.SizingY.Mode == UISizing.UISizingMode.Fit;
                //if (fitAcrossList)
                //    mySize[inverseListMask] += pen[inverseListMask];

                break;
        }

        IntVector2 mySize = IntVector2.Zero;

        // Add margin and padding offsets
        IntVector2 sizePaddings = IntVector2.Zero;
        sizePaddings += Layout.Padding.TopLeft;
        sizePaddings += Layout.Padding.BottomRight;
        sizePaddings = sizePaddings.FloorMultiply(CalculatedMetrics.Scale);
        CalculatedMetrics.PaddingsSize = sizePaddings;

        IntVector2 sizeMargins = IntVector2.Zero;
        sizeMargins += Layout.Margins.TopLeft;
        sizeMargins += Layout.Margins.BottomRight;
        sizeMargins = sizeMargins.FloorMultiply(CalculatedMetrics.Scale);
        CalculatedMetrics.MarginsSize = sizeMargins;

        // We add both paddings (which is expected) AND margins to the size
        // so that the parents don't have to handle margins. (although this is done in LayoutWindow for lists)
        // We might change this in the future as it does require subtracting them later
        mySize += sizePaddings + sizeMargins;

        // Determine content size
        IntVector2 contentSize = IntVector2.Zero;

        // Fixed size
        if (Layout.SizingX.Mode == UISizing.UISizingMode.Fixed)
            contentSize.X = (int)MathF.Ceiling(Layout.SizingX.Size * CalculatedMetrics.Scale.X);
        if (Layout.SizingY.Mode == UISizing.UISizingMode.Fixed)
            contentSize.Y = (int)MathF.Ceiling(Layout.SizingY.Size * CalculatedMetrics.Scale.Y);

        // Determine content size.
        mySize += IntVector2.Max(IntVector2.Max(childrenSize, contentSize), InternalGetWindowMinSize());

        if (this is MapEditorViewMode)
        {
            bool a = true;
        }

        return IntVector2.Clamp(
            mySize,
            Layout.MinSize.CeilMultiply(CalculatedMetrics.Scale),
            Layout.MaxSize.CeilMultiply(CalculatedMetrics.Scale)
        );
    }

    protected void GrowWindow()
    {
        // Early out
        if (Children.Count == 0)
            return;

        IntVector2 myMeasuredSize = CalculatedMetrics.Size - CalculatedMetrics.MarginsSize;
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.X = Math.Max(child.CalculatedMetrics.Size.X, myMeasuredSize.X);

                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.Y = Math.Max(child.CalculatedMetrics.Size.Y, myMeasuredSize.Y);

                    child.GrowWindow();
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();

                int listChildrenCount = 0;
                float listRemainingSize = myMeasuredSize[listMask];
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    float listSize = child.CalculatedMetrics.Size[listMask];
                    listRemainingSize -= listSize;
                    listChildrenCount++;
                }
                listRemainingSize -= GetListSpacing(listMask) * (listChildrenCount - 1);

                // Grow across list
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    bool growAcrossList = Layout.LayoutMethod.GrowingAcrossList(child.Layout);
                    if (growAcrossList)
                        child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask];
                }

                // Grow along list
                int infDetect = 0;
                while (listRemainingSize > 1)
                {
                    infDetect++;
                    if (infDetect > 50)
                    {
                        Assert(false, "Infinite loop in GrowWindow() :(");
                        break;
                    }

                    int smallest = int.MaxValue;
                    int secondSmallest = int.MaxValue;
                    int growingCount = 0;
                    foreach (UIBaseWindow child in Children)
                    {
                        if (SkipWindowLayout(child)) continue;

                        bool growAlongList = Layout.LayoutMethod.GrowingAlongList(child.Layout);
                        if (!growAlongList) continue;

                        growingCount++;

                        int sizeListDirection = child.CalculatedMetrics.Size[listMask];
                        // Initialize smallest
                        if (smallest == int.MaxValue)
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

                    int widthToAdd = Math.Min(secondSmallest - smallest, (int)Math.Round(listRemainingSize / growingCount));
                    foreach (UIBaseWindow child in Children)
                    {
                        if (SkipWindowLayout(child)) continue;

                        bool growAlongList = Layout.LayoutMethod.GrowingAlongList(child.Layout);
                        if (!growAlongList) continue;

                        int sizeListDirection = child.CalculatedMetrics.Size[listMask];
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
                    if (SkipWindowLayout(child)) continue;

                    child.GrowWindow();
                }

                break;
        }
    }

    protected void LayoutWindow(IntVector2 pos)
    {
        // Corrections
        // ----
        CalculatedMetrics.Size = CalculatedMetrics.Size - CalculatedMetrics.MarginsSize;
        // ----

        // Todo: these offsets should be handled by the parent in order for windows to be able to relayout with CalculatedMetrics.Position
        IntVector2 offsets = IntVector2.Zero;
        offsets += Layout.Offset;
        offsets += Layout.Margins.TopLeft;
        offsets = offsets.FloorMultiply(CalculatedMetrics.Scale);

        pos += offsets;
        CalculatedMetrics.Position = pos;

        IntVector2 pen = pos;
        pen += Layout.Padding.TopLeft.FloorMultiply(CalculatedMetrics.Scale);

        IntRectangle parentContentRect = new IntRectangle(
           pen,
           CalculatedMetrics.Size - CalculatedMetrics.PaddingsSize
        );

        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (child._useCustomLayout)
                    {
                        child.InternalCustomLayout();
                    }
                    else
                    {
                        if (child.Layout.Anchor == UIAnchor.TopLeft && child.Layout.ParentAnchor == UIAnchor.TopLeft) // Shortcut for most common
                        {
                            child.CalculatedMetrics.InsideParent = true;
                            child.LayoutWindow(pen);
                        }
                        else
                        {
                            child.CalculatedMetrics.InsideParent = AnchorsInsideParent(child.Layout.ParentAnchor, child.Layout.Anchor);

                            // This will prevent left margins affecting us when the anchor is right
                            //var contentRectForThisChild = parentContentRect;
                            //contentRectForThisChild.Position += child.Layout.Margins.TopLeft.FloorMultiply(child.CalculatedMetrics.Scale);
                            //contentRectForThisChild.Size -= child.CalculatedMetrics.MarginsSize;

                            IntVector2 anchorPos = GetAnchorPosition(
                                child.Layout.ParentAnchor, parentContentRect,
                                child.Layout.Anchor, child.CalculatedMetrics.Size
                            );
                            child.LayoutWindow(anchorPos);
                        }
                    }
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();
                int listSpacing = GetListSpacing(listMask);
                foreach (UIBaseWindow child in Children)
                {
                    if (child._useCustomLayout)
                    {
                        child.InternalCustomLayout();
                    }
                    else
                    {
                        child.CalculatedMetrics.InsideParent = true;

                        IntVector2 childPosition = pen;
                        ListLayoutItemsAlign alignAcrossList = GetItemsAlignAcrossFromList(Layout.LayoutMethod.Mode, child.Layout.Anchor);
                        switch (alignAcrossList)
                        {
                            case ListLayoutItemsAlign.Center:
                                childPosition[inverseListMask] += parentContentRect.Size[inverseListMask] / 2 - child.CalculatedMetrics.Size[inverseListMask] / 2;
                                break;
                            case ListLayoutItemsAlign.End:
                                childPosition[inverseListMask] += parentContentRect.Size[inverseListMask] - child.CalculatedMetrics.Size[inverseListMask];
                                break;
                        }

                        if (child.Layout.Anchor != UIAnchor.TopLeft && alignAcrossList == ListLayoutItemsAlign.Beginning)
                            child.AddWarning(UILayoutWarning.AnchorInListDoesntDoAnything);

                        child.LayoutWindow(childPosition);
                        pen[listMask] += child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginsSize[listMask] + listSpacing;
                    }
                }
                break;
        }

        InternalOnLayoutComplete();
        foreach (UIBaseWindow child in Children)
        {
            child.InternalOnLayoutComplete();
        }
    }

    private int GetListSpacing(int listMask)
    {
        return (int)Maths.RoundAwayFromZero(Layout.LayoutMethod.ListSpacing[listMask] * CalculatedMetrics.Scale[listMask]);
    }

    private static ListLayoutItemsAlign GetItemsAlignAcrossFromList(UIMethodName listType, UIAnchor anchor)
    {
        if (anchor == UIAnchor.TopLeft)
            return ListLayoutItemsAlign.Beginning;

        if (listType == UIMethodName.HorizontalList)
        {
            if (anchor == UIAnchor.CenterLeft || anchor == UIAnchor.CenterRight || anchor == UIAnchor.CenterCenter)
                return ListLayoutItemsAlign.Center;
            if (anchor == UIAnchor.BottomLeft || anchor == UIAnchor.BottomRight || anchor == UIAnchor.BottomCenter)
                return ListLayoutItemsAlign.End;
        }
        else if (listType == UIMethodName.VerticalList)
        {
            if (anchor == UIAnchor.TopCenter || anchor == UIAnchor.BottomCenter || anchor == UIAnchor.CenterCenter)
                return ListLayoutItemsAlign.Center;
            if (anchor == UIAnchor.TopRight || anchor == UIAnchor.BottomRight || anchor == UIAnchor.CenterRight)
                return ListLayoutItemsAlign.End;
        }

        return ListLayoutItemsAlign.Beginning;
    }

    private static bool SkipWindowLayout(UIBaseWindow window)
    {
        if (!window.Visuals.Visible && window.Visuals.DontTakeSpaceWhenHidden) return true;
        if (window._useCustomLayout) return true;
        return false;
    }

    #region Custom

    protected bool _useCustomLayout;

    protected virtual void InternalCustomLayout()
    {

    }

    #endregion

    #region Anchor

    protected static IntVector2 GetAnchorPosition(UIAnchor parentAnchor, IntRectangle parentContentRect, UIAnchor anchor, IntVector2 contentSize)
    {
        IntVector2 offset = IntVector2.Zero;

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
                offset.X += parentContentRect.X + parentContentRect.Width / 2;
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
                offset.Y += parentContentRect.Y + parentContentRect.Height / 2;
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

    /// <summary>
    /// A very simple check for whether the anchors will land the window inside or outside the parent.
    /// </summary>
    protected static bool AnchorsInsideParent(UIAnchor parentAnchor, UIAnchor anchor)
    {
        bool parentIsTop = parentAnchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight;
        bool parentIsVCenter = parentAnchor is UIAnchor.CenterLeft or UIAnchor.CenterCenter or UIAnchor.CenterRight;
        bool parentIsBottom = parentAnchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight;

        bool parentIsLeft = parentAnchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft;
        bool parentIsHCenter = parentAnchor is UIAnchor.TopCenter or UIAnchor.CenterCenter or UIAnchor.BottomCenter;
        bool parentIsRight = parentAnchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight;

        if (parentIsTop)
        {
            if (parentIsLeft && anchor == UIAnchor.TopLeft) return true;

            if (parentIsHCenter && anchor is UIAnchor.TopLeft or UIAnchor.TopCenter or UIAnchor.TopRight) return true;

            if (parentIsRight && anchor == UIAnchor.TopRight) return true;
        }
        else if (parentIsVCenter)
        {
            if (parentIsLeft && anchor is UIAnchor.TopLeft or UIAnchor.CenterLeft or UIAnchor.BottomLeft) return true;
            if (parentIsHCenter) return true;
            if (parentIsRight && anchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight) return true;
        }
        else if (parentIsBottom)
        {
            if (parentIsLeft && anchor == UIAnchor.BottomLeft) return true;
            if (parentIsHCenter && anchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight) return true;
            if (parentIsRight && anchor == UIAnchor.BottomRight) return true;
        }

        return false;
    }

    #endregion

    #endregion

    #region Debug

    private enum UILayoutWarning
    {
        None,
        AnchorInListDoesntDoAnything
    }

    private HashSet<UILayoutWarning>? _warnings;

    [Conditional("DEBUG")]
    private void AddWarning(UILayoutWarning warning)
    {
        _warnings ??= new HashSet<UILayoutWarning>();
        _warnings.Add(warning);
    }

    #endregion

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}