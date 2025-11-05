#nullable enable

#region Using

using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Threading;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI.New;
using Emotion.Game.Systems.UI2;
using System.Diagnostics.CodeAnalysis;
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

    public UIWindowCalculatedMetrics CalculatedMetrics = new UIWindowCalculatedMetrics();

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

    protected virtual void InvalidateLoaded()
    {
        //_needsLoad = true;
        //Controller?.InvalidatePreload();
    }

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

        //Assert(State != UIWindowState.Open || GLThread.IsGLThread(), "UI children can only be added from the main thread.");

        child.EnsureParentLinks();
        child.Parent = this;
        if (Engine.UI.InUpdate && State == UIWindowState.Open)
            _childrenToAdd.Enqueue(child);
        else
            Inner_AddChildToList(child);

        // Start loading early, don't wait for next update
        child.UpdateLoading();

        if (State == UIWindowState.Open)
            child.SetStateOpened();
        InvalidateLayout();
    }

    private readonly Queue<UIBaseWindow> _childrenToAdd = new Queue<UIBaseWindow>(0);

    private void Inner_AddChildToList(UIBaseWindow child)
    {
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

        UIBaseWindow parent = Parent;
        while (parent != null) // Note: since this will reach all the way to the UISystem we need to check nullability of parent.
        {
            if (parent._needsLayout) break;
            if (parent.State != UIWindowState.Open) break;

            //if (Parent.Layout.LayoutMethod.Mode == UIMethodName.Free &&
            //    Parent.Layout.SizingX.Mode != UISizing.UISizingMode.Fit &&
            //    Parent.Layout.SizingY.Mode != UISizing.UISizingMode.Fit)
            //{
            //    break;
            //}

            parent._needsLayout = true;
            parent = parent.Parent;
        }




        // todo: Layout functions should be rewritten in a way so they can be resumed at any point in the tree,
        // to prevent all invalidations going all the way up.
        // For instance this case doesn't make sense to propagate:
        //  Parent.Layout.LayoutMethod.Mode == UIMethodName.Free &&
        //  Parent.Layout.SizingX.Mode != UISizing.UISizingMode.Fit &&
        //  Parent.Layout.SizingY.Mode != UISizing.UISizingMode.Fit

        //Parent?.InvalidateLayout(); 
    }

    #endregion

    #region Loading

    private Coroutine _loadingRoutine = Coroutine.CompletedRoutine;

    public bool IsLoading()
    {
        return _needsLoading || !_loadingRoutine.Finished;
    }

    protected Coroutine? UpdateLoading()
    {
        if (!_loadingRoutine.Finished)
            return _loadingRoutine;

        Coroutine? firstLoading = null;

        // Update loading only if not currently loading
        if (_needsLoading && _loadingRoutine.Finished)
        {
            // Try to get the loading routine.
            Coroutine? newLoading = InternalLoad();
            if (newLoading != null)
            {
                _loadingRoutine = newLoading;
                firstLoading = newLoading;
            }

            _needsLoading = false;
        }

        lock(this)
        {
            foreach (UIBaseWindow child in Children)
            {
                Coroutine? childLoadRoutine = child.UpdateLoading();
                firstLoading ??= childLoadRoutine;
            }
        }

        return firstLoading;
    }

    protected virtual Coroutine? InternalLoad()
    {
        return null;
    }

    protected static void ProxyInvalidateLayout(object _, object? owner)
    {
        if (owner is UIBaseWindow win)
            win.InvalidateLayout();
    }

    public IEnumerator WaitLoadingRoutine()
    {
        Coroutine? routine = null;
        do
        {
            routine = UpdateLoading();
            if (routine != null)
                yield return routine;
        }
        while (routine != null);
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

        InternalBeforeRenderChildren(r);
        RenderChildren(r);
        InternalAfterRenderChildren(r);
    }

    protected virtual void InternalRender(Renderer r)
    {
    }

    protected virtual void InternalBeforeRenderChildren(Renderer r)
    {
    }

    protected virtual void InternalAfterRenderChildren(Renderer r)
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

    public void Update()
    {
        Assert(State == UIWindowState.Open);

        // If loading we don't want to update or draw the UI
        if (this is not UISystem && IsLoading()) return;

        UpdateInternal();
        if (State == UIWindowState.Closed) return; // Closed self in update.

        // This is in reverse since children could close themselves in their UpdateInternal
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            UIBaseWindow child = Children[i];
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
        foreach (UIBaseWindow child in Children)
        {
            if (child.Name == id)
                return child;
        }

        foreach (UIBaseWindow child in Children)
        {
            UIBaseWindow? win = child.GetWindowById(id);
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

    public bool GetWindowById<TWindow>(string id, [NotNullWhen(true)] out TWindow? window) where TWindow : UIBaseWindow
    {
        window = GetWindowById<TWindow>(id);
        return window != null;
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

    protected void DefaultLayout()
    {
        PreLayout_Step1_AddQueuedChildren();
        PreLayout_Step2_PreCalculateMetrics();
        Layout_Step1_Measure();
        Layout_Step2_Grow();
        Layout_Step3_Position(CalculatedMetrics.Position);
    }

    private void PreLayout_Step1_AddQueuedChildren()
    {
        while (_childrenToAdd.TryDequeue(out UIBaseWindow? newChild))
        {
            Inner_AddChildToList(newChild);
        }

        foreach (UIBaseWindow child in Children)
        {
            child.PreLayout_Step1_AddQueuedChildren();
        }
    }

    private void PreLayout_Step2_PreCalculateMetrics()
    {
        _needsLayout = false;

        if (Layout.ScaleWithResolution)
            CalculatedMetrics.Scale = Layout.Scale * (Parent?.CalculatedMetrics.Scale ?? Vector2.One);
        else
            Layout.Scale = Vector2.One;

        // Pre-calculate metrics.
        IntVector2 sizeMargins = IntVector2.Zero;
        CalculatedMetrics.MarginLeftTop = Layout.Margins.LeftTop.FloorMultiply(CalculatedMetrics.Scale);
        CalculatedMetrics.MarginRightBottom = Layout.Margins.RightBottom.FloorMultiply(CalculatedMetrics.Scale);

        CalculatedMetrics.PaddingLeftTop = Layout.Padding.LeftTop.FloorMultiply(CalculatedMetrics.Scale);
        CalculatedMetrics.PaddingRightBottom = Layout.Padding.RightBottom.FloorMultiply(CalculatedMetrics.Scale);

        IntVector2 offsets = IntVector2.Zero;
        offsets += Layout.Offset;
        offsets = offsets.FloorMultiply(CalculatedMetrics.Scale);
        CalculatedMetrics.Offsets = offsets;

        foreach (UIBaseWindow child in Children)
        {
            child.PreLayout_Step2_PreCalculateMetrics();
        }
    }

    private void Layout_Step1_Measure()
    {
        IntVector2 childrenSize = IntVector2.Zero;
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;
                    if (child.IsLoading()) continue;

                    child.Layout_Step1_Measure();
                    IntVector2 childSize = child.CalculatedMetrics.Size;
                    childrenSize = IntVector2.Max(childrenSize, childSize + child.CalculatedMetrics.MarginTotalSize);
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

                    IntVector2 childSize;
                    if (child.IsLoading())
                    {
                        childSize = IntVector2.Zero;
                    }
                    else
                    {
                        child.Layout_Step1_Measure();
                        childSize = child.CalculatedMetrics.Size;
                    }
                    pen[listMask] += childSize[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                    pen[inverseListMask] = Math.Max(pen[inverseListMask], childSize[inverseListMask] + child.CalculatedMetrics.MarginTotalSize[inverseListMask]);
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

        // Fixed size
        IntVector2 fixedSize = IntVector2.Zero;
        if (Layout.SizingX.Mode == UISizing.UISizingMode.Fixed)
            fixedSize.X = (int)MathF.Ceiling(Layout.SizingX.Size * CalculatedMetrics.Scale.X);
        if (Layout.SizingY.Mode == UISizing.UISizingMode.Fixed)
            fixedSize.Y = (int)MathF.Ceiling(Layout.SizingY.Size * CalculatedMetrics.Scale.Y);

        // Paddings are added to the children size.
        childrenSize += CalculatedMetrics.PaddingTotalSize;

        // Window size is whichever is the largest between the children, internal measurements, and fixed sizing.
        IntVector2 mySize = IntVector2.Max(IntVector2.Max(childrenSize, fixedSize), InternalGetWindowMinSize());

        // Clamp to limits
        mySize = IntVector2.Clamp(
            mySize,
            Layout.MinSize.CeilMultiply(CalculatedMetrics.Scale),
            Layout.MaxSize.CeilMultiply(CalculatedMetrics.Scale)
        );

        CalculatedMetrics.Size = mySize;
        CalculatedMetrics.ChildrenSize = childrenSize;
    }

    private void Layout_Step2_Grow()
    {
        // Early out
        if (Children.Count == 0)
            return;

        IntVector2 myMeasuredSize = CalculatedMetrics.GetContentSize();
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    if (child.Layout.SizingX.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.X = Math.Max(child.CalculatedMetrics.Size.X, myMeasuredSize.X - child.CalculatedMetrics.MarginTotalSize.X);

                    if (child.Layout.SizingY.Mode == UISizing.UISizingMode.Grow)
                        child.CalculatedMetrics.Size.Y = Math.Max(child.CalculatedMetrics.Size.Y, myMeasuredSize.Y - child.CalculatedMetrics.MarginTotalSize.Y);

                    child.Layout_Step2_Grow();
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();

                // Grow across list
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    bool growingAcross = Layout.LayoutMethod.GrowingAcrossList(child.Layout);
                    if (!growingAcross) continue;

                    child.CalculatedMetrics.Size[inverseListMask] = myMeasuredSize[inverseListMask] - child.CalculatedMetrics.MarginTotalSize[inverseListMask];
                }

                // Grow along list
                int listChildrenCount = 0;
                float listRemainingSize = myMeasuredSize[listMask];
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    float childSize = child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask];
                    listRemainingSize -= childSize;
                    listChildrenCount++;
                }
                listRemainingSize -= GetListSpacing(listMask) * (listChildrenCount - 1);

                int infinitePrevention = 0;
                while (listRemainingSize > Children.Count - 1) // Until list remaining size cannot be split
                {
                    infinitePrevention++;
                    if (infinitePrevention > 50)
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
                    child.Layout_Step2_Grow();
                }

                break;
        }
    }

    private void Layout_Step3_Position(IntVector2 pos)
    {
        CalculatedMetrics.Position = pos;

        IntRectangle contentRect = CalculatedMetrics.GetContentRect();
        switch (Layout.LayoutMethod.Mode)
        {
            case UIMethodName.Free:
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    if (child.Layout.Anchor == UIAnchor.TopLeft && child.Layout.ParentAnchor == UIAnchor.TopLeft) // Shortcut for most common
                    {
                        child.CalculatedMetrics.InsideParent = true;
                        child.Layout_Step3_Position(contentRect.Position + child.CalculatedMetrics.MarginLeftTop + child.CalculatedMetrics.Offsets);
                    }
                    else
                    {
                        child.CalculatedMetrics.InsideParent = AnchorsInsideParent(child.Layout.ParentAnchor, child.Layout.Anchor);

                        // This will prevent left margins affecting us when the anchor is right
                        IntRectangle contentRectForThisChild = contentRect;
                        contentRectForThisChild.Position += child.CalculatedMetrics.MarginLeftTop;
                        contentRectForThisChild.Size -= child.CalculatedMetrics.MarginTotalSize;

                        IntVector2 anchorPos = GetAnchorPosition(
                            child.Layout.ParentAnchor, contentRectForThisChild,
                            child.Layout.Anchor, child.CalculatedMetrics.Size
                        );
                        child.Layout_Step3_Position(anchorPos + child.CalculatedMetrics.Offsets);
                    }
                }
                break;

            case UIMethodName.HorizontalList:
            case UIMethodName.VerticalList:
                int listMask = Layout.LayoutMethod.GetListMask();
                int inverseListMask = Layout.LayoutMethod.GetListInverseMask();
                int listSpacing = GetListSpacing(listMask);
                IntVector2 pen = contentRect.Position;
                foreach (UIBaseWindow child in Children)
                {
                    if (SkipWindowLayout(child)) continue;

                    child.CalculatedMetrics.InsideParent = true;

                    IntVector2 childPosition = pen;
                    ListLayoutItemsAlign alignAcrossList = GetItemsAlignAcrossFromList(Layout.LayoutMethod.Mode, child.Layout.Anchor);
                    switch (alignAcrossList)
                    {
                        case ListLayoutItemsAlign.Center:
                            childPosition[inverseListMask] += contentRect.Size[inverseListMask] / 2 - child.CalculatedMetrics.Size[inverseListMask] / 2;
                            break;
                        case ListLayoutItemsAlign.End:
                            childPosition[inverseListMask] += contentRect.Size[inverseListMask] - child.CalculatedMetrics.Size[inverseListMask];
                            break;
                    }
                    child.AddWarning(child.Layout.Anchor != UIAnchor.TopLeft && alignAcrossList == ListLayoutItemsAlign.Beginning, UILayoutWarning.AnchorInListDoesntDoAnything);

                    // Add margin (todo: this needs to be the right margin when items are aligned to end, none when centered (for the two outside ones) etc)
                    IntVector2 childTopLeftMargin = child.CalculatedMetrics.MarginLeftTop;
                    childPosition[listMask] += childTopLeftMargin[listMask];

                    child.Layout_Step3_Position(childPosition + child.CalculatedMetrics.Offsets);
                    pen[listMask] += child.CalculatedMetrics.Size[listMask] + child.CalculatedMetrics.MarginTotalSize[listMask] + listSpacing;
                }
                break;
        }

        // Custom layout is last so it can react to other window's layouts (UIAttachedWindow)
        foreach (UIBaseWindow child in Children)
        {
            if (child._useCustomLayout)
                child.InternalCustomLayout();
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

    protected void CustomLayout()
    {
        PreLayout_Step1_AddQueuedChildren();
        PreLayout_Step2_PreCalculateMetrics();
        InternalCustomLayout();
    }

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
    private void AddWarning(bool condition, UILayoutWarning warning)
    {
        if (!condition) return;
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