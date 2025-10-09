#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Core.Systems.Input;


#endregion

namespace Emotion.Game.Systems.UI;

[DontSerialize]
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
    }

    public virtual void Dispose()
    {
        Engine.Host.OnResize -= Host_OnResize;
        //Engine.Host.OnMouseMove -= Host_MouseMove;
        if (InputFocus != null) Engine.Host.OnKey.RemoveListener(_keyboardFocusOnKeyDelegateCache);
        if (_myMouseFocus != null) Engine.Host.OnKey.RemoveListener(_mouseFocusOnKeyDelegateCache);
        ClearChildren();
    }

    private void Host_OnResize(Vector2 obj)
    {
        InvalidateLayout();
    }

//    protected override void AfterRenderChildren(Renderer c)
//    {
//#if false
//        {
//            if(_myMouseFocus != null) c.RenderOutline(_myMouseFocus.RenderBounds, Color.Red);
//            c.RenderSprite(new Rectangle(Engine.Host.MousePosition.X, Engine.Host.MousePosition.Y, 1, 1), Color.Pink);
//        }
//#endif

//        base.AfterRenderChildren(c);
//    }

    protected override bool UpdateInternal()
    {
        if (_updateInputFocus) UpdateInputFocus();

        //if (_updatePreload) UpdateLoading();
        //if (_updateLayout) UpdateLayout();

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

    //protected override Vector2 NEW_InternalMeasure(Vector2 space)
    //{
    //    return Engine.Renderer.DrawBuffer.Size;
    //}
#endif

    //protected void UpdateLayout()
    //{
    //    _updateLayout = false;

    //    BuildRelativeToMapping();

    //    // 1. Measure the minimum size each window needs, which in turn
    //    // determines the minimum size of the parent.
    //    // Layout rules and extra metrics (paddings, margins) are measured too.
    //    // Children are measured in insertion order (and by the OrderInParent property).
    //    Size = Engine.Renderer.DrawBuffer.Size; // Used by "amInsideParent" during measurement
    //    Measure(Size);

    //    // 2. Layout windows within their parents, starting with the controller taking up the full screen.
    //    // Sizes returned during measuring can be used, but larger sizes can be set. Positions are
    //    // absolute and not relative.
    //    OLDLayout(Vector2.Zero, Size);
    //}

    //public override void AddChild(UIBaseWindow? child)
    //{
    //    if (child == null) return;
    //    base.AddChild(child);
    //    child.AttachedToController(this);
    //}

    //public override void RemoveChild(UIBaseWindow? child)
    //{
    //    if (child == null) return;
    //    base.RemoveChild(child);
    //    child.DetachedFromController(this);
    //    InvalidateInputFocus();
    //}

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

    #region Input

    private bool KeyboardFocusOnKey(Key key, KeyState status)
    {
        // It is possible to receive an input even while a recalculating is pending.
        if (_updateInputFocus && status == KeyState.Down)
        {
            UpdateInputFocus();
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
        //if (_updateInputFocus && status == KeyState.Down)
        //{
        //    UpdateInputFocus();
        //}

        //if (!Visible) return true;

        //if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd && _myMouseFocus != null)
        //{
        //    bool isScroll = key == Key.MouseWheel;
        //    if (!isScroll)
        //    {
        //        _mouseFocusKeysHeld[key - Key.MouseKeyStart] = status == KeyState.Down;
        //    }

        //    if (key == Key.MouseKeyLeft && status == KeyState.Down)
        //    {
        //        // We dont want dropdowns closing if trying to debug them
        //        bool newFocusIsDebugWindow = _debugTool != null && _myMouseFocus.IsWithin(_debugTool);
        //        if (DropDown != null && !newFocusIsDebugWindow && !_myMouseFocus.IsWithin(DropDown))
        //        {
        //            SetInputFocus(null);
        //            DropDown.Close();
        //            DropDown = null;
        //            return false;
        //        }

        //        // todo: there also must be a way to consume clicks inside yourself that cause you to focus
        //        // as it is possible for another key handler to then change the focus due to propagation.
        //        // careful - since we dont want buttons to have to be double clicked xd
        //        if (_myMouseFocus.HandleInput)
        //            SetInputFocus(_myMouseFocus);
        //    }

        //    // Testing if this is fixed.
        //    // Theoretically if two clicks occur within one tick the first one removing the window, this can happen.
        //    // We don't want to call event handlers of destroyed windows, so lets return out.
        //    // todo: do we want to build something in the input system that prevents the
        //    // same button from being pressed twice in one tick? At least for mouse buttons?
        //    // todo: wouldnt the removal of the window (which was in the focus order?) cause an update of focus
        //    // (unless of course it was removed by a focus event sent from within this function)
        //    if (status == KeyState.Down && _myMouseFocus is not UIController && _myMouseFocus.Controller == null)
        //        return true;

        //    Vector2 mousePos = Engine.Host.MousePosition;
        //    var current = _myMouseFocus;
        //    while (current != null)
        //    {
        //        bool propagate = current.OnKey(key, status, mousePos);
        //        if (!propagate) return false;
        //        current = current.Parent;
        //    }
        //}

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
        else if (_inputFocusManual != null && _inputFocusManual.VisibleAlongTree() && _inputFocusManual.HandleInput && _inputFocusManual.State == UIWindowState.Open)
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

        UIBaseWindow? p = startFrom.Parent;
        while (p != null)
        {
            if (p == stopAt) break;
            p.InputFocusChanged(focus);
            p = p.Parent;
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