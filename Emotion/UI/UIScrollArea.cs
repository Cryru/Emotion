using Emotion.Editor.EditorHelpers;
using Emotion.Platform.Input;

#nullable enable

namespace Emotion.UI;

public class UIScrollArea : UIBaseWindow
{
    protected UIScrollAreaScrollableArea _content;
    protected UIScrollbar _verticalScroll;
    protected UIScrollbar _horizontalScroll;

    public UIScrollArea()
    {
        HandleInput = true;

        var areaInside = new UIScrollAreaScrollableArea
        {
            OrderInParent = -1
        };
        AddChild(areaInside);
        _content = areaInside;

        var scrollVert = new EditorScrollBar
        {
            OnScroll = ScrollBarCallbackVertical,
        };
        AddChild(scrollVert);
        _verticalScroll = scrollVert;

        var scrollHorz = new EditorScrollBarHorizontal
        {
            OnScroll = ScrollBarCallbackHorizontal,
            Margins = new Primitives.Rectangle(0, 0, 15, 0)
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

        _horizontalScroll.DontTakeSpaceWhenHidden = true;
        _verticalScroll.DontTakeSpaceWhenHidden = true;
        _horizontalScroll.SetVisible(_horizontalScroll.TotalArea > _content.Width);
        _verticalScroll.SetVisible(true);// _verticalScroll.TotalArea > _content.Height);
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

    protected class UIScrollAreaScrollableArea : UIBaseWindow
    {
        public Matrix4x4 ScrollTranslationMatrix = Matrix4x4.Identity;

        public Vector2 CurrentScroll;
        public Vector2 MaxScroll;

        public UIScrollAreaScrollableArea()
        {
            ChildrenCanExpandParent = false;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, Color.White * 0.05f);
            return base.RenderInternal(c);
        }

        protected override void AfterLayout()
        {
            base.AfterLayout();
            MaxScroll = Vector2.Max(Size, _measureChildrenUsedSpace);
            CurrentScroll = Vector2.Clamp(CurrentScroll, Vector2.Zero, MaxScroll - Size);
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
