#nullable enable

#pragma warning disable 0612
// ONE

using Emotion.Editor.EditorHelpers;

namespace Emotion.UI;

public class UIScrollArea : UIBaseWindow
{
    public bool ExpandX { get; set; }
    public bool ExpandY { get; set; }

    public bool AutoHideScrollX { get; set; } = true;
    public bool AutoHideScrollY { get; set; } = false;

    protected UIScrollContentArea _content;
    protected UIScrollbar _verticalScroll;
    protected UIScrollbar _horizontalScroll;

    private bool _noChildrenAddLock = false;

    public UIScrollArea()
    {
        HandleInput = true;

        // todo: not sure why this is needed, but
        // the margins on the scroll content make the whole area bigger otherwise
        // so this is a workaround
        //var scrollParent = new UIBaseWindow
        //{
        //    OrderInParent = -1,
        //};
        //AddChild(scrollParent);

        var areaInside = new UIScrollContentArea(this)
        {
            OrderInParent = -1,
            Id = "ScrollAreaContent",
        };
        AddChild(areaInside);
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
            Margins = new Rectangle(0, 0, 20, 0),
            DontTakeSpaceWhenHidden = true
        };
        AddChild(scrollHorz);
        _horizontalScroll = scrollHorz;

        _noChildrenAddLock = true;
    }

    [Obsolete("Don't call, this, call AddChildInside!")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override void AddChild(UIBaseWindow? child)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    {
        Assert(!_noChildrenAddLock);
        base.AddChild(child);
    }

    [Obsolete("Don't call, this, call ClearChildrenInside!")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override void ClearChildren()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    {
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

        Rectangle paddings = new Rectangle(0, 0, verticalVisible ? 20 : 0, horizontalVisible ? 20 : 0);
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

        if (key == Key.MouseWheel)
        {
            bool anyScroll = false;

            Vector2 currentScroll = _content.CurrentScroll;
            bool up = status == KeyState.Up;
            if (up)
                anyScroll = _content.ScrollToPos(currentScroll - new Vector2(0, 1) * Engine.DeltaTime);
            else
                anyScroll = _content.ScrollToPos(currentScroll + new Vector2(0, 1) * Engine.DeltaTime);

            // If didn't scroll - don't consume the scroll input if within another scroll area.
            if (!anyScroll)
            {
                UIScrollArea? inAnotherScroll = FindParentOfType<UIScrollArea>();
                if (inAnotherScroll != null) return true;
            }

            SyncScrollbar();

            return false;
        }

        return base.OnKey(key, status, mousePos);
    }

    public void ScrollTo(Vector2 pos)
    {
        _content.ScrollToPos(pos);
        SyncScrollbar();
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

        private UIScrollArea _parent;

        public UIScrollContentArea(UIScrollArea parent)
        {
            _parent = parent;
        }

        protected override Vector2 Measure_ExpandByChildren(Vector2 myMeasure, Vector2 childrenUsed)
        {
            // The scroll area is only expanded by its children if set to expand
            Vector2 size = myMeasure;
            if (_parent.ExpandX) size.X = MathF.Max(childrenUsed.X, size.X);
            if (_parent.ExpandY) size.Y = MathF.Max(childrenUsed.Y, size.Y);
            return size;
        }

        protected override void Measure_SetLayoutEngineDimensions(UILayoutEngine layoutEngine, Vector2 space, float scale)
        {
            layoutEngine.SetLayoutDimensions(new Rectangle(Vector2.Zero, DefaultMaxSize), Margins * scale, DefaultMaxSize, Paddings * scale);
        }

        protected override void Layout_SetLayoutEngineDimensions(UILayoutEngine layoutEngine, Vector2 pos, Vector2 size, float scale)
        {
            // Layout in the content space - this allows fills to work properly.
            size = Vector2.Max(size, _measureChildrenUsedSpace);
            base.Layout_SetLayoutEngineDimensions(layoutEngine, pos, size, scale);
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
            if (CurrentScroll == posToScrollTo) return false;

            ScrollTranslationMatrix = Matrix4x4.CreateTranslation(-posToScrollTo.X, -posToScrollTo.Y, 0);
            CurrentScroll = posToScrollTo;

            _renderBoundsCalculatedFrom = Rectangle.Empty;
            _renderBounds = Rectangle.Empty;

            return true;
        }

        protected override void RenderChildren(RenderComposer c)
        {
            List<UIBaseWindow> children = GetWindowChildren();

            Rectangle clipRect = Bounds.Offset(c.ModelMatrix.Translation.ToVec2());
            Rectangle? prevClip = c.CurrentState.ClipRect;
            if (prevClip != null)
                clipRect = Rectangle.Clip(prevClip.Value, clipRect);
            c.SetClipRect(clipRect);

            c.PushModelMatrix(ScrollTranslationMatrix);
            // c.RenderOutline(Bounds, Color.Red);
            for (var i = 0; i < children.Count; i++)
            {
                UIBaseWindow child = children[i];
                child.EnsureRenderBoundsCached(c);

                if (!child.Visible) continue;

                child.Render(c);
            }
            c.PopModelMatrix();

            c.SetClipRect(prevClip);
        }
    }
}
