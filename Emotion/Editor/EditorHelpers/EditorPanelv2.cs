#region Using

using Emotion.Common.Threading;
using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.UI;
using OpenGL;
using System.Threading.Tasks;
using static Emotion.Graphics.RenderComposer;
using static Emotion.Platform.PlatformBase;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class EditorPanelv2 : UIBaseWindow
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
                Assert(false, "Embedded mode can only be set prior to it attaching to a controller.");
                return;
            }

            _panelMode = value;
            HandleInput = value == PanelMode.Modal;
        }
    }

    private PanelMode _panelMode = PanelMode.Default;

    protected UIBaseWindow _contentParent = null!;
    protected Vector2 _initialPanelSize = new Vector2(100, 100);

    private UIBaseWindow _panelItself = null!;
    private UIBaseWindow _panelInner = null!;
    private bool _centered;

    public EditorPanelv2()
    {
    }

    public EditorPanelv2(string header)
    {
        Header = header;
        UseNewLayoutSystem = true;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var panelItself = new UIBaseWindow()
        {
            HandleInput = true,
            LayoutMode = LayoutMode.VerticalList,

            FillX = false,
            FillY = false,

            MinSize = _initialPanelSize,
            Anchor = UIAnchor.CenterCenter,
            ParentAnchor = UIAnchor.CenterCenter,

            Id = "PanelItself"
        };
        AddChild(panelItself);
        _centered = true;

        AttachTopBar(panelItself);

        var panelInner = new UIBaseWindow()
        {
            HandleInput = true,
            Id = "PanelInner"
        };
        panelItself.AddChild(panelInner);
        AttachResizeButton(panelInner);

        var panelContent = new UIBaseWindow();
        panelContent.Id = "Content";
        panelContent.Margins = new Rectangle(5, 5, 5, 5);
        panelInner.AddChild(panelContent);

        _panelItself = panelItself;
        _panelInner = panelInner;
        _contentParent = panelContent;

        controller.SetInputFocus(panelContent);
        ApplySettings();
    }

    protected void ApplySettings()
    {
        if (GetWindowById("PanelLabel") is UIText panelLabel)
            panelLabel.Text = Header;
    }

    public override void InputFocusChanged(bool haveFocus)
    {
        if (haveFocus)
            ZOffset++;
        else
            ZOffset--;

        base.InputFocusChanged(haveFocus);
    }

    protected override void AfterLayout()
    {
        base.AfterLayout();

        if (_centered && PanelMode != PanelMode.Embedded)
        {
            _panelItself.AnchorAndParentAnchor = UIAnchor.TopLeft;
            _panelItself.Offset = _panelItself.Position2 / GetScale();
            _panelItself.MinSize = new Vector2(100, 100);
            _panelItself.MaxSize = _panelItself.Size / GetScale();
            _panelItself.FillX = true;
            _panelItself.FillY = true;
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
                _panelItself.MaxSize = _hostWindow.Size / GetScale();
                _panelItself.InvalidateLayout();
            }
            c.RenderToAndClear(_windowFB);
        }

        if (PanelMode == PanelMode.Modal) c.RenderSprite(Bounds, Color.Black * 0.7f);

        c.RenderSprite(_panelInner.Bounds, MapEditorColorPalette.BarColor * 0.8f);
        c.RenderOutline(_panelInner.Bounds, MapEditorColorPalette.ActiveButtonColor * 0.9f, 2);

        if (_topBar != null)
        {
            UIBaseWindow? focus = Controller!.InputFocus;
            if (focus != null && focus.IsWithin(this))
                c.RenderSprite(_topBar.Bounds, _topBarMouseDown || _topBar.MouseInside ? MapEditorColorPalette.ActiveButtonColor : MapEditorColorPalette.ButtonColor);
            else
                c.RenderSprite(_topBar.Bounds, Color.Black * 0.5f);

            c.RenderLine(_topBar.Bounds.TopLeft.ToVec3(), _topBar.Bounds.TopRight.ToVec3(), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
            c.RenderLine(_topBar.Bounds.BottomLeft.ToVec3(), _topBar.Bounds.TopLeft.ToVec3(), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
            c.RenderLine(_topBar.Bounds.TopRight.ToVec3(), _topBar.Bounds.BottomRight.ToVec3(), Color.White * 0.5f, 1, true, RenderLineMode.Inward);
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
            ZOffset = _hostWindow.IsFocused ? 999 : 0;
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

    #region Dragger

    private bool _panelDragResize;
    private UIBaseWindow? _dragButton;

    private void AttachResizeButton(UIBaseWindow parent)
    {
        // todo: move to panel property

        UITexture dragArea = new UITexture();
        var dragButton = new UICallbackButton
        {
            OnMouseEnterProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ActiveButtonColor; },
            OnMouseLeaveProxy = _ => { dragArea.WindowColor = MapEditorColorPalette.ButtonColor; },
            OnClickedProxy = _ => { _panelDragResize = true; },
            OnClickedUpProxy = _ => { _panelDragResize = false; },
            ZOffset = 99
        };
        dragButton.Anchor = UIAnchor.BottomRight;
        dragButton.ParentAnchor = UIAnchor.BottomRight;
        dragButton.FillX = false;
        dragButton.FillY = false;
        parent.AddChild(dragButton);

        dragArea.TextureFile = "Editor/PanelDragArea.png";
        dragArea.RenderSize = new Vector2(8, 8);
        dragArea.Smooth = true;
        dragArea.WindowColor = MapEditorColorPalette.ButtonColor;
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
            r.Size /= GetScale();
            r.Size = Vector2.Max(r.Size, contentMinSize);
            r.Size = Vector2.Max(r.Size, _panelItself.MinSize);
            _panelItself.MaxSize = r.Size;
            _panelItself.InvalidateLayout();
        }
    }

    #endregion

    #region TopBar

    public bool AllowDrag = true;

    private bool _topBarMouseDown;
    private Vector2 _topBarMouseDownPos;
    private UIBaseWindow? _topBar;

    private void AttachTopBar(UIBaseWindow parent)
    {
        UICallbackButton topBar = new UICallbackButton();
        topBar.HandleInput = true;
        topBar.MaxSizeY = 10;
        topBar.Id = "TopBar";
        topBar.OnClickedProxy = (_) =>
        {
            _topBarMouseDown = true;
            _topBarMouseDownPos = Engine.Host.MousePosition;
        };
        topBar.OnClickedUpProxy = (_) =>
        {
            _topBarMouseDown = false;
        };
        _topBar = topBar;
        parent.AddChild(topBar);

        var txt = new UIText();
        txt.ScaleMode = UIScaleMode.FloatScale;
        txt.WindowColor = MapEditorColorPalette.TextColor;
        txt.Id = "PanelLabel";
        txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
        txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
        txt.IgnoreParentColor = true;
        txt.Anchor = UIAnchor.CenterLeft;
        txt.ParentAnchor = UIAnchor.CenterLeft;
        txt.Margins = new Rectangle(3, 0, 5, 0);
        topBar.AddChild(txt);

        var topBarButtonList = new UIBaseWindow();
        topBarButtonList.LayoutMode = LayoutMode.HorizontalList;
        topBarButtonList.Anchor = UIAnchor.TopRight;
        topBarButtonList.ParentAnchor = UIAnchor.TopRight;
        topBar.AddChild(topBarButtonList);

        if (Engine.Host.SupportsSubWindows())
        {
            var subWindowButton = new EditorButton();
            subWindowButton.ScaleMode = UIScaleMode.NoScale;
            subWindowButton.Text = "Sub";
            subWindowButton.Id = "SubWindowButton";
            subWindowButton.NormalColor = Color.PrettyYellow * 0.75f;
            subWindowButton.RolloverColor = Color.PrettyYellow;
            subWindowButton.OnClickedProxy = _ => CreateOwnWindow();
            topBarButtonList.AddChild(subWindowButton);
        }

        var closeButton = new EditorButton();
        closeButton.ScaleMode = UIScaleMode.NoScale;
        closeButton.Text = "X";
        closeButton.Id = "CloseButton";
        closeButton.NormalColor = Color.PrettyRed * 0.75f;
        closeButton.RolloverColor = Color.PrettyRed;
        closeButton.OnClickedProxy = _ => { Controller?.RemoveChild(this); };
        topBarButtonList.AddChild(closeButton);
    }

    private void UpdateDragMove()
    {
        if (_topBarMouseDown && AllowDrag)
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

    #region Own Host Window

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

            // Transition to embedded mode.
            _topBar?.Parent?.RemoveChild(_topBar);
            _topBar = null;

            _dragButton?.Parent?.RemoveChild(_dragButton);
            _dragButton = null;

            _contentParent.Margins = Rectangle.Empty;

            _panelItself.Offset = Vector2.Zero;
            _panelItself.MaxSize = _hostWindow.Size / GetScale();

            InvalidateLayout();
        }));
    }

    private void FlushToOwnWindow(RenderComposer c)
    {
        AssertNotNull(_hostWindow);
        AssertNotNull(_windowFB);

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