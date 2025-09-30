#nullable enable

namespace Emotion.Game.Systems.UI.New;

// todo: scenes - make current scene current even while loading, add loaded bool
// todo: make scene wait for UI loading - they will usually just queue assets so maybe one update is all thats needed.
//      (which will also perform layout in the loading screen)

[DontSerialize]
public class UISystem : UIBaseWindow
{
    public Vector2 TargetResolution = new Vector2(1920, 1080);
    public Vector2 TargetDPI = new Vector2(96);

    public UISystem()
    {
        InitInput();

        State = UIWindowState.Open;
        Engine.Host.OnResize += HostResized;
        HostResized(Engine.Renderer.ScreenBuffer.Size);
    }

    protected override bool UpdateInternal()
    {
        UpdateInput();
        UpdateLoading();
        UpdateLayout();

        return base.UpdateInternal();
    }

    private void UpdateLayout()
    {
        if (!_needsLayout)
            return;

        if (_useCustomLayout)
            CustomLayout();
        else
            DefaultLayout();
    }

    #region Scaling

    private void HostResized(Vector2 size)
    {
        Vector2 scaledTarget = TargetResolution * TargetDPI;
        Vector2 scaledCurrent = size * Engine.Host.GetDPI();

        Layout.Scale = scaledCurrent / scaledTarget;
        Engine.Log.Info($"UI Scale is {Layout.Scale}", MessageSource.UI);
        InvalidateLayout();
    }

    protected override IntVector2 InternalGetWindowMinSize()
    {
        return IntVector2.FromVec2Ceiling(Engine.Renderer.ScreenBuffer.Size);
    }

    #endregion

    #region Input

    /// <summary>
    /// The window that will receive keyboard key events.
    /// </summary>
    public UIBaseWindow? InputFocus { get; protected set; }

    private UIBaseWindow? _inputFocusManual;

    /// <summary>
    /// The window that will receive mouse events.
    /// </summary>
    public UIBaseWindow? MouseFocus { get; protected set; }

    private Func<Key, KeyState, bool> _mouseFocusOnKeyDelegateCache = null!;
    private Func<Key, KeyState, bool> _keyboardFocusOnKeyDelegateCache = null!;
    private bool _needsFocusUpdate = true;
    private bool _mouseMovedThisTick = false;

    private bool[] _mouseFocusKeysHeld = new bool[Key.MouseKeyEnd - Key.MouseKeyStart];

    private void InitInput()
    {
        HandleInput = true;
        _mouseFocusOnKeyDelegateCache = MouseFocusOnKey;
        _keyboardFocusOnKeyDelegateCache = KeyboardFocusOnKey;
        Engine.Host.OnMouseMove += Host_MouseMove;
    }

    private void Host_MouseMove(Vector2 old, Vector2 nu)
    {
        UpdateMouseFocus();
        _mouseMovedThisTick = true;
    }

    public void InvalidateInputFocus()
    {
        _needsFocusUpdate = true;
    }

    private void UpdateInput()
    {
        if (_needsFocusUpdate) UpdateFocus();
        if (!_mouseMovedThisTick) UpdateMouseFocus();
        _mouseMovedThisTick = false;
    }

    private void UpdateMouseFocus()
    {
        if (Engine.Host.HostPaused)
        {
            SetMouseFocus(null);
            return;
        }

        UIBaseWindow? hasPriority = HasButtonHeldMouseFocus();
        if (hasPriority != null)
        {
            SetMouseFocus(hasPriority);
            return;
        }

        Vector2 mousePos = Engine.Input.MousePosition;
        UIBaseWindow? focus = FindWindowUnderMouse(mousePos);
        SetMouseFocus(focus);
    }

    private void SetMouseFocus(UIBaseWindow? window)
    {
        Vector2 mousePos = Engine.Host.MousePosition;
        if (MouseFocus != window)
        {
            if (MouseFocus != null)
            {
                MouseFocus.OnMouseLeft(mousePos);
                Engine.Host.OnKey.RemoveListener(_mouseFocusOnKeyDelegateCache); // This will handle "down" keys receiving "ups"
            }
            MouseFocus = window;
            if (MouseFocus != null)
            {
                Engine.Host.OnKey.AddListener(_mouseFocusOnKeyDelegateCache, KeyListenerType.UI);
                MouseFocus.OnMouseEnter(mousePos);
            }

            //RemoveCurrentRollover();

            //if (newMouseFocus != null && newMouseFocus is not UIController)
            //{
            //    UIRollover? newRollover = newMouseFocus.GetRollover();
            //    newMouseFocus.Controller!.AddChild(newRollover);
            //    CurrentRollover = newRollover;
            //}
        }
        else
        {
            MouseFocus?.OnMouseMove(mousePos);
        }
    }

    private void UpdateFocus()
    {


        _needsFocusUpdate = false;
    }

    private bool MouseFocusOnKey(Key key, KeyState status)
    {
        bool isMouse = key > Key.MouseKeyStart && key < Key.MouseKeyEnd;
        if (!isMouse) return true;

        if (MouseFocus == null) return true;

        // It is possible to receive input with dirty focus.
        if (status == KeyState.Down)
            UpdateInput();

        bool isScroll = key == Key.MouseWheel;
        if (!isScroll)
            _mouseFocusKeysHeld[key - Key.MouseKeyStart] = status == KeyState.Down;

        bool isLeftClick = key == Key.MouseKeyLeft && status == KeyState.Down;
        if (isLeftClick)
        {
            // Clicked outside dropdown - close it
            // note: should right click do this too?
            if (Dropdown != null && !MouseFocus.IsWithin(Dropdown))
            {
                CloseDropdown();
                SetInputFocus(null);
                return false;
            }

            SetInputFocus(MouseFocus);
        }

        // Propagate input up from the window
        Vector2 mousePos = Engine.Host.MousePosition;
        UIBaseWindow current = MouseFocus;
        while (current != null)
        {
            bool propagate = current.OnKey(key, status, mousePos);
            if (!propagate) return false;
            current = current.Parent;
        }

        return true;
    }

    private bool KeyboardFocusOnKey(Key key, KeyState status)
    {
        bool isMouse = key > Key.MouseKeyStart && key < Key.MouseKeyEnd;
        if (isMouse) return true;

        if (InputFocus == null) return true;

        // It is possible to receive input with dirty focus.
        if (status == KeyState.Down)
            UpdateInput();


        return true;
    }

    // API
    // ----

    public void SetInputFocus(UIBaseWindow? window)
    {
        InputFocus = window;
    }

    // Helpers
    // ----

    private UIBaseWindow? HasButtonHeldMouseFocus()
    {
        if (MouseFocus == null || !SupportsInputAlongTree(MouseFocus)) return null;

        for (var i = 0; i < _mouseFocusKeysHeld.Length; i++)
        {
            if (_mouseFocusKeysHeld[i])
                return MouseFocus;
        }

        return null;
    }

    private static bool SupportsInputAlongTree(UIBaseWindow window)
    {
        if (!window.HandleInput || !window.Visible) return false;

        UIBaseWindow? parent = window.Parent;
        while (parent != null)
        {
            if (!parent.ChildrenHandleInput || !parent.Visible) return false;
            parent = parent.Parent;
        }

        return true;
    }

    #endregion

    #region Dropdown Support

    /// <summary>
    /// The currently open dropdown.
    /// </summary>
    public UIDropDown? Dropdown { get; private set; }

    /// <summary>
    /// The window that spawned the currently open dropdown
    /// </summary>
    public UIBaseWindow? DropdownSpawningWindow { get; private set; }

    public void OpenDropdown(UIBaseWindow window, UIDropDown dropdown)
    {
        CloseDropdown();

        UIBaseWindow? closestHost = GetParentOfKind<UIOverlayWindowParent>();
        closestHost ??= this;

        dropdown.AttachedTo = window;
        Dropdown = dropdown;
        closestHost.AddChild(dropdown);

        DropdownSpawningWindow = window;
        window.OnDropdownStateChanged(true);

        SetInputFocus(dropdown);
    }

    public bool HasDropdown(UIBaseWindow window)
    {
        if (Dropdown == null) return false;
        return DropdownSpawningWindow == window;
    }

    public void CloseDropdown()
    {
        if (Dropdown == null) return;

        AssertNotNull(DropdownSpawningWindow);
        UIDropDown dropDown = Dropdown;
        Dropdown = null; // We need to set this as dropdown will call CloseDropdown on its close.
        dropDown.Close();

        DropdownSpawningWindow.OnDropdownStateChanged(false);
        DropdownSpawningWindow = null;
    }

    #endregion

    #region Debugger

    private bool _debugInspectMode = false;
    private List<UIBaseWindow>? _debugWindowsUnderMouse;

    public void EnterInspectMode()
    {
        _debugInspectMode = true;
        _debugWindowsUnderMouse = new List<UIBaseWindow>();
    }

    private void DebugInspectModeUpdate()
    {
        //Debug_GetWindowsUnderMouse(_debugWindowsUnderMouse);
    }

    public List<UIBaseWindow>? GetInspectModeSelectedWindow()
    {
        if (!_debugInspectMode) return null;
        return _debugWindowsUnderMouse;
    }

    #endregion
}

