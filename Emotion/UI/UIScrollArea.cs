using Emotion.Editor.EditorHelpers;
using Emotion.Platform.Input;

#nullable enable

namespace Emotion.UI;

public class UIScrollArea : UIBaseWindow
{
    public bool AutoHideScrollX = true;
    public bool AutoHideScrollY = false;

    protected UIScrollContentArea _content;
    protected UIScrollbar _verticalScroll;
    protected UIScrollbar _horizontalScroll;

    public UIScrollArea()
    {
        HandleInput = true;

        // todo: not sure why this is needed, but
        // the margins on the scroll content make the whole area bigger otherwise
        // so this is a workaround
        var scrollParent = new UIBaseWindow
        {
            OrderInParent = -1,
        };
        AddChild(scrollParent);

        var areaInside = new UIScrollContentArea
        {
            Id = "ScrollAreaContent",
        };
        scrollParent.AddChild(areaInside);
        _content = areaInside;

        var scrollVert = new EditorScrollBar
        {
            OnScroll = ScrollBarCallbackVertical,
            DontTakeSpaceWhenHidden = true
        };
        AddChild(scrollVert);
        _verticalScroll = scrollVert;

        var scrollHorz = new EditorScrollBarHorizontal
        {
            OnScroll = ScrollBarCallbackHorizontal,
            Margins = new Primitives.Rectangle(0, 0, 20, 0),
            DontTakeSpaceWhenHidden = true
        };
        AddChild(scrollHorz);
        _horizontalScroll = scrollHorz;
    }

    public void AddChildInside(UIBaseWindow win)
    {
        _content.AddChild(win);
    }

    public void ClearChildrenInside()
    {
        _content.ClearChildren();
    }

    protected void SyncScrollbar()
    {
        _horizontalScroll.Current = _content.CurrentScroll.X;
        _horizontalScroll.TotalArea = _content.MaxScroll.X;
        _horizontalScroll.PageArea = MathF.Min(_horizontalScroll.TotalArea, _content.Width);
        _horizontalScroll.UpdateScrollbar();

        _verticalScroll.Current = _content.CurrentScroll.Y;
        _verticalScroll.TotalArea = _content.MaxScroll.Y;
        _verticalScroll.PageArea = MathF.Min(_verticalScroll.TotalArea, _content.Height);
        _verticalScroll.UpdateScrollbar();

        bool horizontalVisible = true;
        if (AutoHideScrollX)
            horizontalVisible = _horizontalScroll.TotalArea > _content.Width;
        _horizontalScroll.SetVisible(horizontalVisible);

        bool verticalVisible = true;
        if (AutoHideScrollY)
            verticalVisible = _verticalScroll.TotalArea > _content.Height;
        _verticalScroll.SetVisible(verticalVisible);

        Rectangle paddings = new Primitives.Rectangle(0, 0, verticalVisible ? 20 : 0, horizontalVisible ? 20 : 0);
        _content.Margins = paddings;
    }

    protected void ScrollBarCallbackVertical(float amount)
    {
        _content.ScrollToPos(new Vector2(_content.CurrentScroll.X, amount));
        SyncScrollbar();
    }

    protected void ScrollBarCallbackHorizontal(float amount)
    {
        _content.ScrollToPos(new Vector2(amount, _content.CurrentScroll.Y));
        SyncScrollbar();
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (Children == null) return true;

        if (key == Key.MouseWheel && _content != null)
        {
            var currentScroll = _content.CurrentScroll;
            bool up = status == KeyState.MouseWheelScrollUp;
            if (up)
                _content.ScrollToPos(currentScroll - new Vector2(0, 1) * Engine.DeltaTime);
            else
                _content.ScrollToPos(currentScroll + new Vector2(0, 1) * Engine.DeltaTime);
            SyncScrollbar();

            return false;
        }

        return base.OnKey(key, status, mousePos);
    }

    protected override void AfterLayout()
    {
        base.AfterLayout();
        SyncScrollbar();
    }

    protected class UIScrollContentArea : UIBaseWindow
    {
        public Matrix4x4 ScrollTranslationMatrix = Matrix4x4.Identity;

        public Vector2 CurrentScroll;
        public Vector2 MaxScroll;

        public UIScrollContentArea()
        {
            ChildrenCanExpandParent = false;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, Color.White * 0.05f);
            return base.RenderInternal(c);
        }

        protected override Rectangle GetChildrenLayoutSpace(Vector2 pos, Vector2 space)
        {
            return new Rectangle(pos, UIBaseWindow.DefaultMaxSize);
        }

        protected override void AfterLayout()
        {
            base.AfterLayout();
            MaxScroll = Vector2.Max(Size, _measureChildrenUsedSpace);
            CurrentScroll = Vector2.Clamp(CurrentScroll, Vector2.Zero, MaxScroll - Size);
            ScrollTranslationMatrix = Matrix4x4.CreateTranslation(-CurrentScroll.X, -CurrentScroll.Y, 0);
        }

        public bool ScrollToPos(Vector2 posToScrollTo)
        {
            posToScrollTo = Vector2.Clamp(posToScrollTo, Vector2.Zero, MaxScroll - Size);
            ScrollTranslationMatrix = Matrix4x4.CreateTranslation(-posToScrollTo.X, -posToScrollTo.Y, 0);
            CurrentScroll = posToScrollTo;

            _renderBoundsCalculatedFrom = Rectangle.Empty;
            _renderBounds = Rectangle.Empty;

            return true;
        }

        protected override void RenderChildren(RenderComposer c)
        {
            List<UIBaseWindow> children = GetWindowChildren();

            c.PushModelMatrix(ScrollTranslationMatrix);
            Rectangle? clip = c.CurrentState.ClipRect;
            c.SetClipRect(Bounds);

            // c.RenderOutline(Bounds, Color.Red);
            for (var i = 0; i < children.Count; i++)
            {
                UIBaseWindow child = children[i];
                child.EnsureRenderBoundsCached(c);

                if (!child.Visible) continue;

                child.Render(c);
            }

            c.SetClipRect(clip);
            c.PopModelMatrix();
        }
    }
}
