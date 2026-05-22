#nullable enable

#region Using

using Emotion.Core.Utility.Threading;
using Emotion.Game.Systems.UI.New;
using OpenGL;
using static Emotion.Core.Platform.PlatformBase;

#endregion

namespace Emotion.Editor.EditorUI.Components;

public enum PanelMode
{
    Default,
    Modal,
    Embedded,
    SubWindow
}

public class EditorWindow : UIBaseWindow
{
    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            ApplyHeaderText();
        }
    }

    private string _header = "Untitled";

    public PanelMode PanelMode
    {
        get => _panelMode;
        set
        {
            if ((value == PanelMode.Embedded || _panelMode == PanelMode.Embedded) && State != UIWindowState.Open)
            {
                Assert(false, "Embedded mode can only be set/unset prior the window opening.");
                return;
            }

            if ((value == PanelMode.Modal || _panelMode == PanelMode.Modal) && State != UIWindowState.Open)
            {
                Assert(false, "Modal mode can only be set/unset prior the window opening.");
                return;
            }

            _panelMode = value;
            HandleInput = value == PanelMode.Modal;
        }
    }

    private PanelMode _panelMode = PanelMode.SubWindow;

    private UIBaseWindow _contentParent = null!;
    private UIBaseWindow _panelItself = null!;
    private UIBaseWindow _panelInner = null!;
    private bool _centered;

    protected Vector2 _initialSize = new Vector2(960, 540);

    public EditorWindow()
    {
        OrderInParent = 10;
        _panelMode = PanelMode.Default;
#if AUTOBUILD
        _panelMode = PanelMode.Default;
#endif

        var panelItself = new UIBaseWindow()
        {
            Name = "PanelItself",

            HandleInput = true,
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(0),
                AnchorAndParentAnchor = UIAnchor.CenterCenter,
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit(),

                MaxSize = new IntVector2(1800, 970),
            },
            Visuals =
            {
                BackgroundColor = EditorColorPalette.BarColor * 0.9f,
                BorderColor = EditorColorPalette.ActiveButtonColor * 0.9f,
                Border = 2
            }
        };
        AddChild(panelItself);

        var panelInner = new UIBaseWindow()
        {
            HandleInput = true,
            Name = "PanelInner"
        };
        panelItself.AddChild(panelInner);

        // Make sure the window's rollovers and dropdowns are rendered as children,
        // to make them work inside the subwindows.
        var panelContent = new UIOverlayWindowParent
        {
            Name = "Content",
            Layout =
            {
                Margins = new UISpacing(5, 5, 5, 5),
                OverflowX = UIOverflow.Scroll,
                OverflowY = UIOverflow.Scroll
            }
        };
        panelInner.AddChild(panelContent);

        _panelItself = panelItself;
        _panelInner = panelInner;
        _contentParent = panelContent;
    }

    public EditorWindow(string header) : this()
    {
        Header = header;
    }

    #region Wrangling the UI System

    protected override void InternalOnLayoutComplete()
    {
        if (!_centered)
        {
            _panelItself.Layout.Offset = _panelItself.CalculatedMetrics.Position.RoundMultiply(_panelItself.CalculatedMetrics.ScaleInv);
            _panelItself.Layout.AnchorAndParentAnchor = UIAnchor.TopLeft;

            _centered = true;
        }
        base.InternalOnLayoutComplete();
    }

    #endregion

    #region Helpers

    protected void ApplyHeaderText()
    {
        if (GetWindowById<EditorLabel>("PanelLabel", out EditorLabel? panelLabel))
            panelLabel.Text = Header;
    }

    protected virtual UIBaseWindow GetContentParent()
    {
        return _contentParent;
    }

    #endregion

    protected override void OnOpen()
    {
        base.OnOpen();

        if (_panelMode == PanelMode.SubWindow && !Engine.Host.SupportsSubWindows())
            _panelMode = PanelMode.Default;

        switch (_panelMode)
        {
            case PanelMode.Default:
                AllowDragMove = true;
                AllowSubWindow = true;
                Visuals.BackgroundColor = Color.White.CloneWithAlpha(0);
                AttachResizeButton(_panelInner);
                AttachTopBar(_panelItself);
                break;
            case PanelMode.Embedded:
                AllowDragMove = false;
                AllowSubWindow = false;
                Visuals.BackgroundColor = Color.White.CloneWithAlpha(0);
                break;
            case PanelMode.Modal:
                AllowDragMove = false;
                AllowSubWindow = false;
                Visuals.BackgroundColor = Color.Black * 0.7f;
                AttachTopBar(_panelItself);
                break;
        }

        Engine.UI.SetInputFocus(_panelInner);
        ApplyHeaderText();

        //IEnumerator OpenSubWindowDelayedRoutine()
        //        {
        //            yield return null;
        //            CreateSubWindow();
        //        }
        //        Engine.CoroutineManager.StartCoroutine(OpenSubWindowDelayedRoutine());
    }

    public override void InputFocusChanged(bool haveFocus)
    {
        if (haveFocus)
            OrderInParent++;
        else
            OrderInParent--;

        UpdateTopBarColor();

        base.InputFocusChanged(haveFocus);
    }

    protected override void InternalRender(Renderer r)
    {
        // If rendering to another window, we render to the FB which we will later flush to its context
        if (_hostWindow != null && _hostWindow.IsOpen)
        {
            AssertNotNull(_windowFB);
            if (_windowFB.Size != _hostWindow.Size)
            {
                _windowFB.Resize(_hostWindow.Size, true);

                IntVector2 size = IntVector2.FromVec2Ceiling(_hostWindow.Size * CalculatedMetrics.ScaleInv);
                _panelItself.Layout.SizingX = UISizing.Fixed(size.X);
                _panelItself.Layout.SizingY = UISizing.Fixed(size.Y);
            }
            r.RenderToAndClear(_windowFB);
            r.RenderSprite(CalculatedMetrics.Position.ToVec2(), CalculatedMetrics.Size.ToVec2(), Color.CornflowerBlue);
        }

        base.InternalRender(r);
    }

    protected override void InternalAfterRenderChildren(Renderer r)
    {
        base.InternalAfterRenderChildren(r);

        // Render to other window.
        if (_hostWindow != null && _hostWindow.IsOpen)
        {
            r.RenderTo(null);
            FlushToOwnWindow(r);
        }
    }

    protected override bool UpdateInternal()
    {
        // Check if window has been closed.
        if (_hostWindow != null)
        {
            if (!_hostWindow.IsOpen)
            {
                _hostWindow = null;
                Close();
                return false;
            }
        }

        UpdateResize();
        UpdateDragMove();
        return base.UpdateInternal();
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        bool returnVal = base.OnKey(key, status, mousePos);

        if (key == Key.MouseKeyLeft) return false;

        if (key == Key.Z && Engine.Host.IsCtrlModifierHeld())
        {

        }

        if (PanelMode == PanelMode.Modal) return false;

        return returnVal;
    }

    #region Resize Drag

    private bool _panelDragResize;
    private UIBaseWindow? _dragButton;

    private void AttachResizeButton(UIBaseWindow parent)
    {
        // todo: move to panel property

        var dragAreaColor = new Color(180, 180, 180, 125);
        var dragAreaActive = new Color(240, 240, 240);

        UIPicture dragArea = new()
        {
            Layout =
            {
                SizingX = UISizing.Fixed(20),
                SizingY = UISizing.Fixed(20),
            },
            Texture = "Editor/PanelDragArea.png",
            Smooth = true,
            ImageColor = dragAreaColor
        };

        var dragButton = new UICallbackButton
        {
            OnMouseEnterProxy = _ => { dragArea.ImageColor = dragAreaActive; },
            OnMouseLeaveProxy = _ => { dragArea.ImageColor = dragAreaColor; },
            OnClickedProxy = _ => { _panelDragResize = true; },
            OnClickedUpProxy = _ => { _panelDragResize = false; },
            OrderInParent = 99,
            Layout =
            {
                SizingX = UISizing.Fit(),
                SizingY = UISizing.Fit(),
                AnchorAndParentAnchor = UIAnchor.BottomRight
            },
        };
        parent.AddChild(dragButton);
        dragButton.AddChild(dragArea);
        _dragButton = dragButton;
    }

    private void UpdateResize()
    {
        if (_panelDragResize)
        {
            Vector2 curMouse = Engine.Host.MousePosition;
            curMouse = Vector2.Clamp(curMouse, Vector2.Zero, CalculatedMetrics.Size.ToVec2());
            Rectangle r = Rectangle.FromMinMaxPoints(_panelItself.CalculatedMetrics.Position.ToVec2(), curMouse);

            IntVector2 newSize = IntVector2.FromVec2Floor(r.Size * _panelItself.CalculatedMetrics.ScaleInv); // Unscale
            _panelItself.Layout.SizingX = UISizing.Fixed(newSize.X);
            _panelItself.Layout.SizingY = UISizing.Fixed(newSize.Y);
        }
    }

    #endregion

    #region TopBar

    public bool AllowDragMove = true;
    public bool AllowSubWindow = true;

    private Vector2? _topBarMouseDown;
    private UIBaseWindow? _topBar;

    private void AttachTopBar(UIBaseWindow parent)
    {
        UICallbackButton topBar = new UICallbackButton
        {
            Name = "TopBar",
            OrderInParent = -1,
            HandleInput = true,

            Visuals =
            {
                Border = 1,
                BorderColor = Color.White * 0.5f
            },
            Layout =
            {
                SizingY = UISizing.Fit(),
                LayoutMethod = UILayoutMethod.HorizontalList(0)
            },
            OnClickedProxy = (self) =>
            {
                _topBarMouseDown = self.CalculatedMetrics.Position.ToVec2() - Engine.Host.MousePosition;
            },
            OnClickedUpProxy = (_) =>
            {
                _topBarMouseDown = null;
            },
            OnMouseEnterProxy = (_) =>
            {
                UpdateTopBarColor();
            },
            OnMouseLeaveProxy = (_) =>
            {
                UpdateTopBarColor();
            }
        };
        _topBar = topBar;
        parent.AddChild(topBar);

        var txt = new EditorLabel
        {
            Name = "PanelLabel",
            Layout =
            {
                Margins = new UISpacing(10, 3, 10, 3),
                SizingX = UISizing.Grow()
            }
        };
        topBar.AddChild(txt);

        var topBarButtonList = new UIBaseWindow
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(0),
                SizingX = UISizing.Fit()
            }
        };
        topBar.AddChild(topBarButtonList);

        if (Engine.Host.SupportsSubWindows() && AllowSubWindow)
        {
            var subWindowButton = new SquareEditorButtonWithTexture("Editor/SubWindow.png", 23)
            {
                IconColor = new Color(70, 70, 70),
                Name = "SubWindowButton",
                NormalColor = Color.PrettyYellow * 0.75f,
                RolloverColor = Color.PrettyYellow,
                OnClickedProxy = _ => CreateSubWindow()
            };
            topBarButtonList.AddChild(subWindowButton);
        }

        var closeButton = new SquareEditorButtonWithTexture("Editor/Close.png", 23)
        {
            IconColor = new Color(70, 70, 70),
            Name = "CloseButton",
            NormalColor = Color.PrettyRed * 0.75f,
            RolloverColor = Color.PrettyRed,
            OnClickedProxy = _ => Close(),
            Texture =
            {
                ImageScale = new Vector2(0.25f)
            }
        };
        topBarButtonList.AddChild(closeButton);

        UpdateTopBarColor();
    }

    private void UpdateTopBarColor()
    {
        if (_topBar == null) return;

        bool focused = Engine.UI.InputFocus?.IsWithin(this) ?? false;
        if (focused)
            _topBar.Visuals.BackgroundColor = _topBar.MouseInside ? EditorColorPalette.ActiveButtonColor : EditorColorPalette.ButtonColor;
        else
            _topBar.Visuals.BackgroundColor = _topBar.MouseInside ? Color.White * 0.2f : Color.Black * 0.5f;
    }

    private void UpdateDragMove()
    {
        if (_topBarMouseDown != null && AllowDragMove)
        {
            Vector2 pressOffset = _topBarMouseDown.Value;
            Vector2 mouseOffset = Engine.Host.MousePosition + pressOffset;

            // Estimate new bounds to snap to screen
            Rectangle panelBounds = new(
                mouseOffset,
                _panelItself.CalculatedMetrics.Size.ToVec2()
            );

            IntRectangle snapArea = IntRectangle.FromRectFloor(Engine.UI.CalculatedMetrics.Bounds.ToRect());
            UIBaseWindow? mapEditorTopBar = Engine.UI.GetWindowById("EditorTopBar");
            if (mapEditorTopBar != null)
            {
                int topBarPos = (int)MathF.Floor(mapEditorTopBar.CalculatedMetrics.Bounds.Bottom);
                snapArea.Y = topBarPos;
                snapArea.Height -= topBarPos;
            }

            Vector2 newPosScreenSpace = snapArea.ToRect().SnapRectangleInside(panelBounds);
            Vector2 invScale = _panelItself.CalculatedMetrics.ScaleInv;
            IntVector2 newPosUISpace = IntVector2.FromVec2Floor(newPosScreenSpace * invScale);
            _panelItself.Layout.Offset = newPosUISpace;
        }
    }

    #endregion

    #region Sub-Window Mode

    private PlatformSubWindow? _hostWindow;
    private FrameBuffer? _windowFB;

    public override MouseFocusPair FindWindowUnderMouse(Vector2 pos, bool respectInputHandling = true)
    {
        // If using my own host window,
        // receive mouse input only when the host window is focused!
        if (_hostWindow != null && !_hostWindow.IsFocused) return MouseFocusPair.None;
        return base.FindWindowUnderMouse(pos, respectInputHandling);
    }

    protected void CreateSubWindow()
    {
        // Transition layout to embedded mode.
        _topBar?.Close();
        _topBar = null;

        _dragButton?.Close();
        _dragButton = null;

        _contentParent.Layout.Margins = new UISpacing();
        _panelMode = PanelMode.SubWindow;

        GLThread.ExecuteOnGLThreadAsync(() =>
        {
            Vector2 contentSize = _panelItself.CalculatedMetrics.Size.ToVec2();
            _windowFB = new FrameBuffer(contentSize).WithColor();
            _hostWindow = Engine.Host.CreateSubWindow(Header, contentSize);

            _panelItself.Layout.Offset = IntVector2.Zero;
            _panelItself.Layout.AnchorAndParentAnchor = UIAnchor.TopLeft;

            IntVector2 size = IntVector2.FromVec2Ceiling(_hostWindow.Size * CalculatedMetrics.ScaleInv);
            _panelItself.Layout.SizingX = UISizing.Fixed(size.X);
            _panelItself.Layout.SizingY = UISizing.Fixed(size.Y);

            _centered = true;
        });
    }

    private void FlushToOwnWindow(Renderer c)
    {
        AssertNotNull(_hostWindow);
        AssertNotNull(_windowFB);

        // debug - draw where it actually is on the main window
        //c.RenderSprite(Vector3.Zero, _windowFB.Size, Color.White * 0.5f, _windowFB.ColorAttachment);

        c.FlushRenderStream();
        _hostWindow.MakeCurrent();

        c.ClearFrameBuffer();

        // Manually blit.
        int w = (int)_windowFB.Viewport.Width;
        int h = (int)_windowFB.Viewport.Height;
        Gl.BlitNamedFramebuffer(
            _windowFB.Pointer, 0,
            0, 0, w, h,
            0, 0, w, h,
            ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Linear
        );

        _hostWindow.SwapBuffers();

        // Restore main window context.
        Engine.Host.Context.MakeCurrent();
        Engine.Renderer.CurrentTarget.Bind();
    }

    #endregion

    #region UndoSystem

    private List<EditorUndoableOperation> _undoList = new List<EditorUndoableOperation>();
    private int _currentOperationIndex = 0;

    public static void SubmitUndoOperation(UIBaseWindow originator, EditorUndoableOperation operation)
    {
        UIBaseWindow? parent = originator.Parent;
        while (true)
        {
            if (parent == null) break;
            if (parent is EditorWindow editorWindow && editorWindow.PanelMode != PanelMode.Embedded)
            {
                // submit
                return;
            }

            parent = parent.Parent;
        }
    }

    public void SubmitUndoOperation(EditorUndoableOperation operation)
    {

    }

    protected void Undo()
    {

    }

    protected void Redo()
    {

    }

    #endregion
}