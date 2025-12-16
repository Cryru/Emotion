#nullable enable

namespace Emotion.Game.Systems.UI.New;

// todo: scenes - make current scene current even while loading, add loaded bool
// todo: make scene wait for UI loading - they will usually just queue assets so maybe one update is all thats needed.
//      (which will also perform layout in the loading screen)

[DontSerialize]
public class UISystem : UIBaseWindow
{
    public bool InUpdate { get; private set; }

    public Vector2 TargetResolution = new Vector2(1920, 1080);
    public Vector2 TargetDPI = new Vector2(96);

    public UISystem()
    {
        InitInput();

        if (Engine.Configuration.DebugMode)
            SetupDebugger();

        State = UIWindowState.Open;
        Engine.Host.OnResize += HostResized;
        HostResized(Engine.Renderer.ScreenBuffer.Size);
    }

    public void UpdateSystem()
    {
        InUpdate = true;
        Update();
        InUpdate = false;
    }

    public void RenderSystem(Renderer r)
    {
        InUpdate = true;
        Render(r);
        InUpdate = false;
    }

    protected override bool UpdateInternal()
    {
        if (_debugInspectMode)
            InspectModeUpdate();

        UpdateInput();
        UpdateLoading();
        UpdateLayout();

        return base.UpdateInternal();
    }

    protected override void InternalAfterRenderChildren(Renderer r)
    {
        base.InternalAfterRenderChildren(r);
        InspectModeRender(r);
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
        Vector2 scaledTarget = TargetResolution;// / TargetDPI;
        Vector2 scaledCurrent = size;// / Engine.Host.GetDPI();

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
        Engine.Input.OnMouseMove += Host_MouseMove;
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
        Vector2 mousePos = Engine.Input.MousePosition;
        if (MouseFocus != window)
        {
            if (MouseFocus != null)
            {
                MouseFocus.OnMouseLeft(mousePos);
                Engine.Input.OnKey.RemoveListener(_mouseFocusOnKeyDelegateCache); // This will handle "down" keys receiving "ups"
            }
            MouseFocus = window;
            if (MouseFocus != null)
            {
                Engine.Input.OnKey.AddListener(_mouseFocusOnKeyDelegateCache, KeyListenerType.UI);
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
        Vector2 mousePos = Engine.Input.MousePosition;
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

        UIBaseWindow? closestHost = window.GetParentOfKind<UIOverlayWindowParent>();
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

    private void SetupDebugger()
    {
        Engine.Input.OnKey.AddListener(DebuggerOnKey, KeyListenerType.System);
    }

    private bool DebuggerOnKey(Key key, KeyState state)
    {
        if (_debugInspectMode && key == Key.MouseKeyLeft && state == KeyState.Down)
        {
            InspectModeClick();
            return false;
        }

        if (key == Key.X && Engine.Host.IsAltModifierHeld() && state == KeyState.Down)
        {
            if (!_debugInspectMode)
                EnterInspectMode();
            else
                LeaveInspectMode();

            return false;
        }
        return true;
    }

    private bool _debugInspectMode = false;
    private float _debugInspectModeFadeText = 0;
    private List<UIBaseWindow>? _debugWindowsUnderMouse;

    public void EnterInspectMode()
    {
        _debugInspectMode = true;
        _debugInspectModeFadeText = 0;
        _debugWindowsUnderMouse = new List<UIBaseWindow>();
    }

    private void LeaveInspectMode()
    {
        _debugInspectMode = false;
    }

    private static void Debug_GetWindowsUnderMouseInner(UIBaseWindow win, Vector2 mousePos, List<(UIBaseWindow, int)> output, int depth)
    {
        List<UIBaseWindow> children = win.Children;
        output.Add((win, depth));

        for (int i = children.Count - 1; i >= 0; i--)
        {
            UIBaseWindow child = children[i];
            if (child.Visible && child.CalculatedMetrics.Bounds.Contains(mousePos))
            {
                Debug_GetWindowsUnderMouseInner(child, mousePos, output, depth + 1);
            }
        }
    }

    private void InspectModeUpdate()
    {
        AssertNotNull(_debugWindowsUnderMouse);

        _debugWindowsUnderMouse.Clear();
        Vector2 mousePos = Engine.Input.MousePosition;

        List<(UIBaseWindow, int)> outputWithDepth = new List<(UIBaseWindow, int)>();
        Debug_GetWindowsUnderMouseInner(this, mousePos, outputWithDepth, 0);
        outputWithDepth.Sort((x, y) => MathF.Sign(x.Item2 - y.Item2));
        for (int i = 0; i < outputWithDepth.Count; i++)
        {
            UIBaseWindow window = outputWithDepth[i].Item1;
            _debugWindowsUnderMouse.Add(window);
        }
    }

    private void InspectModeRender(Renderer r)
    {
        if (_debugInspectMode)
        {
            _debugInspectModeFadeText += Engine.DeltaTime;
            if (_debugInspectModeFadeText < 1000)
            {
                float fade = (_debugInspectModeFadeText - 800) / 200f;
                fade = 1.0f - fade;
                if (_debugInspectModeFadeText < 800) fade = 1f;

                r.RenderSprite(Vector3.Zero, new Vector2(400, 20), Color.Black * fade);
                r.RenderString(Vector3.Zero, Color.Red * fade, $"UI INSPECT MODE ACTIVE!", 20);
            }

            foreach (UIBaseWindow win in _debugWindowsUnderMouse!)
            {
                Rectangle bounds = win.CalculatedMetrics.Bounds.ToRect();
                r.RenderRectOutline(bounds, Color.Red, 2);
                r.RenderSprite(bounds.BottomLeft.ToVec3() + new Vector3(0, 100, 0), new Vector2(450, 20), Color.Black);
                r.RenderString(bounds.BottomLeft.ToVec3() + new Vector3(0, 100, 0), Color.Red, $"{win} {win.Layout.LayoutMethod.Mode}", 15);
            }
        }
    }

    private void InspectModeClick()
    {
        Console.WriteLine($"Selected windows:");
        foreach (UIBaseWindow win in _debugWindowsUnderMouse!)
        {
            Console.WriteLine($"    {win} - {win.CalculatedMetrics.Bounds}");

        }
    }

    public List<UIBaseWindow>? GetInspectModeSelectedWindow()
    {
        if (!_debugInspectMode) return null;
        return _debugWindowsUnderMouse;
    }

    #endregion
}

