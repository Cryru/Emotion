#region Using

using Emotion.Common.Threading;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.UI;
using OpenGL;
using System.Threading.Tasks;
using static Emotion.Graphics.RenderComposer;
using static Emotion.Platform.PlatformBase;

#endregion

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Helpers;

public class EditorWindowContent : UIBaseWindow
{
    public Vector2 SizeConstraint = new Vector2(100, 100);

    protected override Vector2 NEW_InternalMeasure(Vector2 space)
    {
        var scale = GetScale();
        return SizeConstraint * scale;
    }
}

public class EditorWindow : UIBaseWindow
{
    public string Header
    {
        get => _header;
        set
        {
            _header = value;
            ApplySettings();
        }
    }

    private string _header = "Untitled";

    public PanelMode PanelMode
    {
        get => _panelMode;
        set
        {
            if ((value == PanelMode.Embedded || _panelMode == PanelMode.Embedded) && Controller != null)
            {
                Assert(false, "Embedded mode can only be set/unset prior to it attaching to a controller.");
                return;
            }

            if ((value == PanelMode.Modal || _panelMode == PanelMode.Modal) && Controller != null)
            {
                Assert(false, "Modal mode can only be set/unset prior to it attaching to a controller.");
                return;
            }

            _panelMode = value;
            HandleInput = value == PanelMode.Modal;
        }
    }

    private PanelMode _panelMode = PanelMode.Default;

    protected UIBaseWindow _contentParent = null!;
    private EditorWindowContent _panelItself = null!;
    private UIBaseWindow _panelInner = null!;
    private bool _centered;

    public EditorWindow()
    {
    }

    public EditorWindow(string header)
    {
        Header = header;
        Priority = 10;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var panelItself = new EditorWindowContent()
        {
            HandleInput = true,
            LayoutMode = LayoutMode.VerticalList,
            FillX = false,
            FillY = false,
            AnchorAndParentAnchor = UIAnchor.CenterCenter,
            Id = "PanelItself",
        };
        AddChild(panelItself);
        _centered = true;

        var panelInner = new UIBaseWindow()
        {
            HandleInput = true,
            Id = "PanelInner"
        };
        panelItself.AddChild(panelInner);

        var panelContent = new UIBaseWindow
        {
            Id = "Content",
            Margins = new Rectangle(5, 5, 5, 5)
        };
        panelInner.AddChild(panelContent);

        _panelItself = panelItself;
        _panelInner = panelInner;
        _contentParent = panelContent;

        if (_panelMode == PanelMode.Modal)
        {
            AllowDragMove = false;
            AllowSubWindow = false;
        }
        else if (_panelMode == PanelMode.Default)
        {
            AttachResizeButton(panelInner);
        }
        AttachTopBar(panelItself);

        controller.SetInputFocus(panelContent);
        ApplySettings();
    }

    protected void ApplySettings()
    {
        if (GetWindowById("PanelLabel") is EditorLabel panelLabel)
            panelLabel.Text = Header;
    }

    public override void InputFocusChanged(bool haveFocus)
    {
        if (haveFocus)
            Priority++;
        else
            Priority--;

        base.InputFocusChanged(haveFocus);
    }

    protected override void AfterLayout()
    {
        base.AfterLayout();

        if (_centered && PanelMode != PanelMode.Embedded)
        {
            _panelItself.AnchorAndParentAnchor = UIAnchor.TopLeft;
            _panelItself.Offset = _panelItself.Position2 / GetScale();
            _centered = false;
        }
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        // If rendering to another window, make its context current.
        if (_hostWindow != null && _hostWindow.IsOpen)
        {
            AssertNotNull(_windowFB);
            if (_windowFB.Size != _hostWindow.Size)
            {
                _windowFB.Resize(_hostWindow.Size, true);
                _panelItself.SizeConstraint = _hostWindow.Size / GetScale();
                _panelItself.InvalidateLayout();
            }
            c.RenderToAndClear(_windowFB);
        }

        if (PanelMode == PanelMode.Modal)
            c.RenderSprite(Position, Size, Color.Black * 0.7f);

        c.RenderSprite(_panelInner.Position, _panelInner.Size, MapEditorColorPalette.BarColor * 0.8f);
        c.RenderOutline(_panelInner.Position, _panelInner.Size, MapEditorColorPalette.ActiveButtonColor * 0.9f, 2);

        if (_topBar != null)
        {
            UIBaseWindow? focus = Controller!.InputFocus;
            if (focus != null && focus.IsWithin(this))
                c.RenderSprite(_topBar.Position, _topBar.Size, _topBarMouseDown || _topBar.MouseInside ? MapEditorColorPalette.ActiveButtonColor : MapEditorColorPalette.ButtonColor);
            else
                c.RenderSprite(_topBar.Position, _topBar.Size, Color.Black * 0.5f);

            c.RenderLine(_topBar.Bounds.TopLeft.ToVec3(_topBar.Z), _topBar.Bounds.TopRight.ToVec3(_topBar.Z), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
            c.RenderLine(_topBar.Bounds.BottomLeft.ToVec3(_topBar.Z), _topBar.Bounds.TopLeft.ToVec3(_topBar.Z), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
            c.RenderLine(_topBar.Bounds.TopRight.ToVec3(_topBar.Z), _topBar.Bounds.BottomRight.ToVec3(_topBar.Z), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
        }

        return base.RenderInternal(c);
    }

    protected override void AfterRenderChildren(RenderComposer c)
    {
        base.AfterRenderChildren(c);

        // Render to other window.
        if (_hostWindow != null && _hostWindow.IsOpen)
        {
            c.RenderTo(null);
            FlushToOwnWindow(c);
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
                Parent!.RemoveChild(this);
                return false;
            }
            Priority = _hostWindow.IsFocused ? 50 : 0;
        }

        UpdateResize();
        UpdateDragMove();
        return base.UpdateInternal();
    }

    public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
    {
        bool returnVal = base.OnKey(key, status, mousePos);

        if (key == Key.MouseKeyLeft) return false;
        if (PanelMode == PanelMode.Modal) return false;

        return returnVal;
    }

    #region Resize Drag

    private bool _panelDragResize;
    private UIBaseWindow? _dragButton;

    private void AttachResizeButton(UIBaseWindow parent)
    {
        // todo: move to panel property

        UITexture dragArea = new UITexture
        {
            TextureFile = "Editor/PanelDragArea.png",
            RenderSize = new Vector2(25, 25),
            Smooth = true,
            WindowColor = MapEditorColorPalette.ButtonColor
        };

        var dragButton = new UICallbackButton
        {
            OnMouseEnterProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ActiveButtonColor; },
            OnMouseLeaveProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ButtonColor; },
            OnClickedProxy = _ => { _panelDragResize = true; },
            OnClickedUpProxy = _ => { _panelDragResize = false; },
            Priority = 99,
            Anchor = UIAnchor.BottomRight,
            ParentAnchor = UIAnchor.BottomRight,
            FillX = false,
            FillY = false
        };
        parent.AddChild(dragButton);
        dragButton.AddChild(dragArea);
        _dragButton = dragButton;
    }

    private void UpdateResize()
    {
        if (_panelDragResize)
        {
            Vector2 contentMinSize = _contentParent.MinSize;
            if (_topBar != null) contentMinSize += new Vector2(0, _topBar.MaxSizeY);
            contentMinSize.X += _contentParent.Margins.X + _contentParent.Margins.Width;
            contentMinSize.Y += _contentParent.Margins.Y + _contentParent.Margins.Height;

            Vector2 curMouse = Engine.Host.MousePosition;
            Rectangle r = Rectangle.FromMinMaxPoints(_panelItself.Position2, curMouse);
            r.Size /= GetScale(); // Unscale
            r.Size = Vector2.Max(r.Size, contentMinSize);
            r.Size = Vector2.Max(r.Size, _panelItself.MinSize);
            _panelItself.SizeConstraint = r.Size;
            _panelItself.InvalidateLayout();
        }
    }

    #endregion

    #region TopBar

    public bool AllowDragMove = true;
    public bool AllowSubWindow = true;

    private bool _topBarMouseDown;
    private Vector2 _topBarMouseDownPos;
    private UIBaseWindow? _topBar;

    private void AttachTopBar(UIBaseWindow parent)
    {
        UICallbackButton topBar = new UICallbackButton
        {
            Priority = -1,
            HandleInput = true,
            MaxSizeY = 40,
            Id = "TopBar",
            OnClickedProxy = (_) =>
            {
                _topBarMouseDown = true;
                _topBarMouseDownPos = Engine.Host.MousePosition;
            },
            OnClickedUpProxy = (_) =>
            {
                _topBarMouseDown = false;
            },
            LayoutMode = LayoutMode.HorizontalList
        };
        _topBar = topBar;
        parent.AddChild(topBar);

        var txt = new EditorLabel
        {
            Id = "PanelLabel",
            Margins = new Rectangle(10, 0, 10, 0)
        };
        topBar.AddChild(txt);

        var topBarButtonList = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList,
            Anchor = UIAnchor.CenterRight,
            ParentAnchor = UIAnchor.CenterRight,
        };
        topBar.AddChild(topBarButtonList);

        if (Engine.Host.SupportsSubWindows() && AllowSubWindow)
        {
            var subWindowButton = new EditorButton
            {
                Text = "Sub",
                Id = "SubWindowButton",
                NormalColor = Color.PrettyYellow * 0.75f,
                RolloverColor = Color.PrettyYellow,
                OnClickedProxy = _ => CreateOwnWindow()
            };
            topBarButtonList.AddChild(subWindowButton);
        }

        var closeButton = new EditorButton
        {
            Text = "X",
            Id = "CloseButton",
            NormalColor = Color.PrettyRed * 0.75f,
            RolloverColor = Color.PrettyRed,
            OnClickedProxy = _ => { Close(); }
        };
        topBarButtonList.AddChild(closeButton);
    }

    private void UpdateDragMove()
    {
        if (_topBarMouseDown && AllowDragMove)
        {
            Vector2 mousePosNow = Engine.Host.MousePosition;
            Vector2 posDiff = mousePosNow - _topBarMouseDownPos;
            _topBarMouseDownPos = mousePosNow;

            float containerScale = _panelItself.GetScale();
            var panelBounds = new Rectangle(_panelItself.Offset * containerScale + posDiff, _panelItself.Size);

            Rectangle snapArea = Controller!.Bounds;
            snapArea.Width += panelBounds.Width / 2f;
            snapArea.Height += panelBounds.Height / 2f;

            UIBaseWindow? mapEditorTopBar = Controller.GetWindowById("EditorTopBar");
            if (mapEditorTopBar != null)
            {
                float topBarPos = mapEditorTopBar.Bounds.Bottom;
                snapArea.Y = topBarPos;
                snapArea.Height -= topBarPos;
            }

            _panelItself.Offset = snapArea.SnapRectangleInside(panelBounds) / containerScale;
            _panelItself.InvalidateLayout();
        }
    }

    #endregion

    #region Open as Window

    private PlatformSubWindow? _hostWindow;
    private FrameBuffer? _windowFB;

    public override UIBaseWindow? FindMouseInput(Vector2 pos)
    {
        // If using my own host window,
        // receive mouse input only when the host window is focused!
        if (_hostWindow != null && !_hostWindow.IsFocused) return null;

        return base.FindMouseInput(pos);
    }

    private void CreateOwnWindow()
    {
        Task.Run(() => GLThread.ExecuteGLThreadAsync(() =>
        {
            var contentSize = _contentParent.Size;
            _windowFB = new FrameBuffer(contentSize).WithColor();
            _hostWindow = Engine.Host.CreateSubWindow(Header, contentSize);

            // Transition layout to embedded mode.
            _topBar?.Parent?.RemoveChild(_topBar);
            _topBar = null;

            _dragButton?.Parent?.RemoveChild(_dragButton);
            _dragButton = null;

            _contentParent.Margins = Rectangle.Empty;

            _panelItself.Offset = Vector2.Zero;
            _panelItself.SizeConstraint = _hostWindow.Size / GetScale();

            InvalidateLayout();
        }));
    }

    private void FlushToOwnWindow(RenderComposer c)
    {
        AssertNotNull(_hostWindow);
        AssertNotNull(_windowFB);

        // debug - draw where it actually is on the main window
        // c.RenderSprite(Vector3.Zero, _windowFB.Size, _windowFB.ColorAttachment);

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
}