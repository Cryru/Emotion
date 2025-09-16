#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.Input;


#endregion

namespace Emotion.Game.Systems.UI;

public partial class UIController : UIBaseWindow
{
    /// <summary>
    /// The window that will receive keyboard key events.
    /// </summary>
    public UIBaseWindow? InputFocus { get; protected set; }

    /// <summary>
    /// Override for InputFocus due to internal logic such as focused text inputs.
    /// </summary>
    protected UIBaseWindow? _inputFocusManual;

    /// <summary>
    /// The currently open dropdown.
    /// </summary>
    public UIDropDown? DropDown { get; set; }

    protected bool[] _mouseFocusKeysHeld = new bool[Key.MouseKeyEnd - Key.MouseKeyStart];

    protected bool _updatePreload = true;
    protected bool _updateLayout = true;
    protected bool _updateInputFocus = true;
    protected bool _mouseUpdatedThisTick = false;

    private Func<Key, KeyState, bool> _mouseFocusOnKeyDelegateCache;
    private Func<Key, KeyState, bool> _keyboardFocusOnKeyDelegateCache;

    public UIController()
    {
        _mouseFocusOnKeyDelegateCache = MouseFocusOnKey;
        _keyboardFocusOnKeyDelegateCache = KeyboardFocusOnKey;

        HandleInput = true;
        Engine.Host.OnResize += Host_OnResize;
        Engine.Host.OnMouseMove += Host_MouseMove;
        KeepTemplatePreloaded(this);
    }

    public virtual void Dispose()
    {
        StopPreloadTemplate(this);
        Engine.Host.OnResize -= Host_OnResize;
        Engine.Host.OnMouseMove -= Host_MouseMove;
        if (InputFocus != null) Engine.Host.OnKey.RemoveListener(_keyboardFocusOnKeyDelegateCache);
        if (_myMouseFocus != null) Engine.Host.OnKey.RemoveListener(_mouseFocusOnKeyDelegateCache);
        ClearChildren();
    }

    private void Host_OnResize(Vector2 obj)
    {
        InvalidateLayout();
        InvalidatePreload();
    }

    private void Host_MouseMove(Vector2 old, Vector2 nu)
    {
        UpdateMouseFocus();
        _mouseUpdatedThisTick = true;
    }

    public override void InvalidateLayout()
    {
        _updateLayout = true;
        _updateInputFocus = true;
    }

    public void InvalidatePreload()
    {
        _updatePreload = true;
    }

    protected override void AfterRenderChildren(Renderer c)
    {
#if false
        {
            if(_myMouseFocus != null) c.RenderOutline(_myMouseFocus.RenderBounds, Color.Red);
            c.RenderSprite(new Rectangle(Engine.Host.MousePosition.X, Engine.Host.MousePosition.Y, 1, 1), Color.Pink);
        }
#endif

        base.AfterRenderChildren(c);
    }

    protected override bool UpdateInternal()
    {
        if (!_loadingThread.IsCompleted) return false;

        if (_updateInputFocus) UpdateInputFocus();
        if (!_mouseUpdatedThisTick) UpdateMouseFocus();
        _mouseUpdatedThisTick = false;

        if (_updatePreload) UpdateLoading();
        if (_updateLayout) UpdateLayout();

        return true;
    }

#if NEW_UI
		protected override Vector2 InternalMeasure(Vector2 space)
		{
			return Engine.Renderer.DrawBuffer.Size;
		}
#else
    protected override Vector2 InternalMeasure(Vector2 space)
    {
        return Engine.Renderer.DrawBuffer.Size;
    }

    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        return Engine.Renderer.DrawBuffer.Size;
    }
#endif

    protected void UpdateLayout()
    {
        _updateLayout = false;

        BuildRelativeToMapping();

        // 1. Measure the minimum size each window needs, which in turn
        // determines the minimum size of the parent.
        // Layout rules and extra metrics (paddings, margins) are measured too.
        // Children are measured in insertion order (and by the OrderInParent property).
        Size = Engine.Renderer.DrawBuffer.Size; // Used by "amInsideParent" during measurement
        Measure(Size);

        // 2. Layout windows within their parents, starting with the controller taking up the full screen.
        // Sizes returned during measuring can be used, but larger sizes can be set. Positions are
        // absolute and not relative.
        Layout(Vector2.Zero, Size);
    }

    public override void AddChild(UIBaseWindow? child)
    {
        if (child == null) return;
        base.AddChild(child);
        child.AttachedToController(this);
    }

    public override void RemoveChild(UIBaseWindow? child, bool evict = true)
    {
        if (child == null) return;
        base.RemoveChild(child, evict);
        child.DetachedFromController(this);
        InvalidateInputFocus();
    }

    protected override void RenderChildren(Renderer c)
    {
        // Reset overlay children render flag.
        for (int i = 0; i < _overlayWindows.Count; i++)
        {
            (UIBaseWindow win, bool rendered) pair = _overlayWindows[i];
            pair.rendered = false;
            _overlayWindows[i] = pair;
        }

        base.RenderChildren(c);
        RenderOverlayChildren(this, c);
    }

    public void RenderOverlayChildren(UIBaseWindow within, Renderer c)
    {
        // todo: apply parent displacements (combine together all the way up?)

        for (int i = 0; i < _overlayWindows.Count; i++)
        {
            (UIBaseWindow win, bool rendered) pair = _overlayWindows[i];
            if (pair.rendered)
                continue;

            if (within == this || pair.win.IsWithin(within)) // can be cached, in theory
            {
                pair.win.Render(c);
                pair.rendered = true;
                _overlayWindows[i] = pair; // tuple is value type
            }
        }
    }

    #region Dedupe Hierarchy Checker

    // This is to avoid infinite loops in the recursive measure, layout, update, and render functions.

    private HashSet<UIBaseWindow> _doubleAddChecker = new HashSet<UIBaseWindow>();

    public bool IsWindowPresentInHierarchy(UIBaseWindow win)
    {
        bool present = false;
        lock (_doubleAddChecker)
        {
            present = _doubleAddChecker.Contains(win);
        }

        return present;
    }

    public void RegisterWindowInController(UIBaseWindow win)
    {
        lock (_doubleAddChecker)
        {
            _doubleAddChecker.Add(win);
        }
    }

    public void RemoveWindowFromController(UIBaseWindow win)
    {
        lock (_doubleAddChecker)
        {
            _doubleAddChecker.Remove(win);
        }
    }

    #endregion

    #region RelativeTo Layout

    private Dictionary<UIBaseWindow, List<UIBaseWindow>> _parentToChildren = new(16);
    private List<(UIBaseWindow win, bool rendered)> _overlayWindows = new(2);

    public List<UIBaseWindow> GetChildrenMapping(UIBaseWindow win)
    {
        if (_parentToChildren == null) return EMPTY_CHILDREN_LIST;
        if (_parentToChildren.TryGetValue(win, out List<UIBaseWindow>? list)) return list;
        return EMPTY_CHILDREN_LIST;
    }

    protected UIBaseWindow? GetParentRelativeRespecting(UIBaseWindow win)
    {
        if (win.RelativeTo != null)
        {
            UIBaseWindow? relativeToWin = win.GetWindowById(win.RelativeTo) ?? GetWindowById(win.RelativeTo);
            if (relativeToWin != null)
                return relativeToWin;
        }

        return win.Parent;
    }

    protected void BuildRelativeToMapping()
    {
        _overlayWindows.Clear();

        Dictionary<UIBaseWindow, List<UIBaseWindow>> parentToChildren = _parentToChildren;
        parentToChildren.Clear();

        foreach (UIBaseWindow child in ForEachChildrenDeep())
        {
            UIBaseWindow thisWin = child;
            List<UIBaseWindow> children = thisWin.Children ?? EMPTY_CHILDREN_LIST;

            // Create mapping or add my children if it exists.
            // This window would have a mapping already (encountered) only if a relative window is attached to it.
            bool hasMapping = parentToChildren.TryGetValue(thisWin, out List<UIBaseWindow>? myMappingList);

            // Check if any of my children are supposed to be relative to another window,
            // in which case my mapping can't reuse my children list.
            bool anyRelative = false;
            for (int i = 0; i < children.Count; i++)
            {
                UIBaseWindow ch = children[i];
                if (ch.RelativeTo != null)
                {
                    anyRelative = true;
                    break;
                }
            }

            bool iterateAddToMapping = false;
            if (!hasMapping)
            {
                if (anyRelative)
                {
                    myMappingList = new List<UIBaseWindow>();
                    parentToChildren.Add(thisWin, myMappingList);
                    iterateAddToMapping = true;
                }
                else
                {
                    parentToChildren.Add(thisWin, children);
                }
            }
            else
            {
                iterateAddToMapping = true;
            }

            // Add children which are not relative to another window.
            if (iterateAddToMapping)
            {
                AssertNotNull(myMappingList);

                for (int i = 0; i < children.Count; i++)
                {
                    UIBaseWindow ch = children[i];
                    if (ch.RelativeTo == null)
                        myMappingList.Add(ch);
                }
            }

            if (thisWin.OverlayWindow)
                _overlayWindows.Add((thisWin, false));

            // Check if my window is supposed to be relative to another.
            if (thisWin.RelativeTo != null)
            {
                UIBaseWindow? relativeToParent = thisWin.GetWindowById(thisWin.RelativeTo) ?? GetWindowById(thisWin.RelativeTo);
                if (relativeToParent != null)
                {
                    bool relativeToWindowHasMapping = parentToChildren.TryGetValue(relativeToParent, out List<UIBaseWindow>? parentMapping);
                    if (!relativeToWindowHasMapping)
                    {
                        parentMapping = new List<UIBaseWindow>();
                        parentToChildren.Add(relativeToParent, parentMapping);
                    }
                    // Check if remapping from children copy
                    else if (relativeToWindowHasMapping && parentMapping == relativeToParent.Children)
                    {
                        AssertNotNull(parentMapping);

                        var newParentMapping = new List<UIBaseWindow>();
                        newParentMapping.AddRange(parentMapping);
                        parentMapping = newParentMapping;

                        parentToChildren[relativeToParent] = newParentMapping;
                    }

                    AssertNotNull(parentMapping);
                    if (parentMapping.IndexOf(thisWin) == -1)
                    {
                        parentMapping.Add(thisWin);
                    }
                    else
                    {
                        // Window that is RelativeTo a window that is a sibling.
                    }
                }
            }
        }

        // Stable sort children based on priority.
        foreach (KeyValuePair<UIBaseWindow, List<UIBaseWindow>> pair in parentToChildren)
        {
            UIBaseWindow.SortChildren(pair.Value);
        }
    }

    public override List<UIBaseWindow> GetWindowChildren()
    {
        return GetChildrenMapping(this);
    }

    #endregion

    #region Loading

    public Task PreloadUI()
    {
        if (!_loadingThread.IsCompleted) return _loadingThread;
        UpdateLoading();
        return _loadingThread;
    }

    private Task _loadingThread = Task.CompletedTask;
    private UILoadingContext _loadingContext = new UILoadingContext();

    protected void UpdateLoading()
    {
        if (!_loadingThread.IsCompleted) return;
        CheckLoadContent(_loadingContext);
        _loadingThread = Task.Run(_loadingContext.LoadWindows);
        _updatePreload = false;

        // If one controller is loading, check global.
        GlobalLoadUI();
    }

    /// <summary>
    /// You can keep windows globally loaded.
    /// </summary>
    private class PreloadWindowStorage : UIBaseWindow
    {
        public override void AddChild(UIBaseWindow? child)
        {
            if (child == null) return;
            Children ??= new List<UIBaseWindow>();
            Children.Add(child);
        }

        public override void RemoveChild(UIBaseWindow child, bool evict = true)
        {
            if (Children == null) return;
            if (evict) Children.Remove(child);
        }
    }

    private static object _globalLoadingLock = new object();
    private static Task _globalLoadingThread = Task.CompletedTask;
    private static PreloadWindowStorage _keepWindowsLoaded = new();
    private static UILoadingContext _globalLoadingContext = new UILoadingContext();

    public static Task GlobalLoadUI()
    {
        lock (_globalLoadingLock)
        {
            if (!_globalLoadingThread.IsCompleted) return _globalLoadingThread;
            _keepWindowsLoaded.CheckLoadContent(_globalLoadingContext);
            _globalLoadingThread = Task.Run(_globalLoadingContext.LoadWindows);
        }

        return _globalLoadingThread;
    }

    public static void KeepTemplatePreloaded(UIBaseWindow window)
    {
        _keepWindowsLoaded.AddChild(window);
    }

    public static void StopPreloadTemplate(UIBaseWindow window)
    {
        _keepWindowsLoaded.RemoveChild(window);
    }

    #endregion

    #region Input

    private bool KeyboardFocusOnKey(Key key, KeyState status)
    {
        // It is possible to receive an input even while a recalculating is pending.
        if (_updateInputFocus && status == KeyState.Down)
        {
            UpdateInputFocus();
            UpdateMouseFocus();
        }

        if (!Visible) return true;
        if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd) return true;
        if (InputFocus != null && InputFocus.VisibleAlongTree())
        {
            Vector2 mousePos = Engine.Host.MousePosition;
            var current = InputFocus;
            while (current != null)
            {
                bool propagate = current.OnKey(key, status, mousePos);
                if (!propagate) return false;
                current = current.Parent;
            }
        }

        return true;
    }

    protected virtual bool MouseFocusOnKey(Key key, KeyState status)
    {
        if (_updateInputFocus && status == KeyState.Down)
        {
            UpdateInputFocus();
            UpdateMouseFocus();
        }

        if (!Visible) return true;

        if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && _myMouseFocus != null)
        {
            bool isScroll = key == Key.MouseWheel;
            if (!isScroll)
            {
                _mouseFocusKeysHeld[key - Key.MouseKeyStart] = status == KeyState.Down;
            }

            if (key == Key.MouseKeyLeft && status == KeyState.Down)
            {
                // We dont want dropdowns closing if trying to debug them
                bool newFocusIsDebugWindow = _debugTool != null && _myMouseFocus.IsWithin(_debugTool);
                if (DropDown != null && !newFocusIsDebugWindow && !_myMouseFocus.IsWithin(DropDown))
                {
                    SetInputFocus(null);
                    DropDown.Close();
                    DropDown = null;
                    return false;
                }

                // todo: there also must be a way to consume clicks inside yourself that cause you to focus
                // as it is possible for another key handler to then change the focus due to propagation.
                // careful - since we dont want buttons to have to be double clicked xd
                if (_myMouseFocus.HandleInput)
                    SetInputFocus(_myMouseFocus);
            }

            // Testing if this is fixed.
            // Theoretically if two clicks occur within one tick the first one removing the window, this can happen.
            // We don't want to call event handlers of destroyed windows, so lets return out.
            // todo: do we want to build something in the input system that prevents the
            // same button from being pressed twice in one tick? At least for mouse buttons?
            // todo: wouldnt the removal of the window (which was in the focus order?) cause an update of focus
            // (unless of course it was removed by a focus event sent from within this function)
            if (status == KeyState.Down && _myMouseFocus is not UIController && _myMouseFocus.Controller == null)
                return true;

            Vector2 mousePos = Engine.Host.MousePosition;
            var current = _myMouseFocus;
            while (current != null)
            {
                bool propagate = current.OnKey(key, status, mousePos);
                if (!propagate) return false;
                current = current.Parent;
            }
        }

        return true;
    }

    public void InvalidateInputFocus()
    {
        _updateInputFocus = true;
    }

    public void SetInputFocus(UIBaseWindow? win)
    {
        // If focus is being removed (set to null) then we explicitly don't want to
        // focus the same window as before (or their tree). So we temporary remove their focus.
        var removedHandleInput = false;
        UIBaseWindow? oldFocus = InputFocus;
        if (win == null && oldFocus != null && oldFocus.ChildrenHandleInput)
        {
            oldFocus.ChildrenHandleInput = false;
            removedHandleInput = true;
        }

        _inputFocusManual = win;
        UpdateInputFocus();

        if (removedHandleInput) oldFocus!.ChildrenHandleInput = true;
    }

    private void UpdateInputFocus()
    {
        _updateInputFocus = false;

        UIBaseWindow? newFocus;
        if (!ChildrenHandleInput || !Visible)
        {
            newFocus = null;
        }
        else if (_inputFocusManual != null && _inputFocusManual.VisibleAlongTree() && _inputFocusManual.HandleInput && _inputFocusManual.Controller == this)
        {
            newFocus = _inputFocusManual;
        }
        else
        {
            _inputFocusManual = null;
            newFocus = FindInputFocusable(this);
        }

        if (newFocus == this) newFocus = null;

        if (InputFocus != newFocus)
        {
            UIBaseWindow? commonParentWithOldFocus = null;

            // Re-hook event to get KeyUp events on keys that are pressed down.
            if (InputFocus != null)
            {
                Engine.Host.OnKey.RemoveListener(_keyboardFocusOnKeyDelegateCache);

                // Send focus remove events only on the part of the tree that will be unfocused.
                commonParentWithOldFocus = FindCommonParent(InputFocus, newFocus);
                SetFocusUpTree(InputFocus, false, commonParentWithOldFocus);
            }

            InputFocus = newFocus;

            if (InputFocus != null)
            {
                Engine.Host.OnKey.AddListener(_keyboardFocusOnKeyDelegateCache, KeyListenerType.UI);

                // Send focus add events down to the child that will be focused.
                SetFocusUpTree(InputFocus, true, commonParentWithOldFocus);
            }

            // Kinda spammy.
            // Engine.Log.Info($"New input focus {InputFocus}", "UI");
        }
    }

    protected void SetFocusUpTree(UIBaseWindow startFrom, bool focus, UIBaseWindow? stopAt)
    {
        if (stopAt == startFrom) return;
        startFrom.InputFocusChanged(focus);

        // We need to respect relative children here,
        // because IsWithin respects it. Otherwise we get double focus.
        UIBaseWindow? p = GetParentRelativeRespecting(startFrom);
        while (p != null)
        {
            if (p == stopAt) break;
            p.InputFocusChanged(focus);
            p = GetParentRelativeRespecting(p);
        }
    }

    protected static UIBaseWindow? FindCommonParent(UIBaseWindow one, UIBaseWindow? two)
    {
        if (two == null) return null;
        if (two.IsWithin(one)) return one;
        if (one.IsWithin(two)) return two;

        UIBaseWindow? p = one.Parent;
        while (p != null)
        {
            if (two.IsWithin(p)) return p;
            p = p.Parent;
        }

        return null;
    }

    protected static UIBaseWindow? FindInputFocusable(UIBaseWindow wnd)
    {
        if (!wnd.Visible) return null;

        if (wnd.Children != null && wnd.ChildrenHandleInput)
            for (int i = wnd.Children.Count - 1; i >= 0; i--)
            {
                UIBaseWindow win = wnd.Children[i];
                if (win.ChildrenHandleInput && win.Visible)
                {
                    UIBaseWindow? found = FindInputFocusable(win);
                    if (found != null) return found;
                }
            }

        return wnd.HandleInput ? wnd : null;
    }

    #endregion
}