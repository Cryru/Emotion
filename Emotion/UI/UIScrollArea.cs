using Emotion.Editor.EditorHelpers;
using Emotion.Platform.Input;

namespace Emotion.UI
{
    public class UIScrollArea : UIBaseWindow
    {
        protected class UIScrollAreaScrollableArea : UIBaseWindow
        {
            public Matrix4x4 Displacement = Matrix4x4.Identity;

            public Vector2 CurrentScroll;
            public Vector2 MaxScroll;

            public UIScrollAreaScrollableArea()
            {
                CodeGenerated = true;
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
                Displacement = Matrix4x4.CreateTranslation(-posToScrollTo.X, -posToScrollTo.Y, 0);
                CurrentScroll = posToScrollTo;

                _renderBoundsCalculatedFrom = Rectangle.Empty;
                _renderBounds = Rectangle.Empty;

                return true;
            }

            protected override void RenderChildren(RenderComposer c)
            {
                c.PushModelMatrix(Displacement);
                Rectangle? clip = c.CurrentState.ClipRect;
                c.SetClipRect(Bounds);

                // c.RenderOutline(Bounds, Color.Red);
                for (var i = 0; i < Children!.Count; i++)
                {
                    UIBaseWindow child = Children[i];
                    child.EnsureRenderBoundsCached(c);

                    if (!child.Visible) continue;

                    child.Render(c);
                }

                c.SetClipRect(clip);
                c.PopModelMatrix();
            }
        }

        protected UIScrollAreaScrollableArea _content;
        protected UIScrollbar _verticalScroll;
        protected UIScrollbar _horizontalScroll;

        public UIScrollArea()
        {
            UseNewLayoutSystem = true;
            HandleInput = true;

            var areaInside = new UIScrollAreaScrollableArea();
            areaInside.OrderInParent = 5;
            _content = areaInside;
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

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            AddChild(_content);

            //var scrollVert = new EditorScrollBar
            //{
            //    Dock = UIDockDirection.Right,
            //    OnScroll = ScrollBarCallbackVertical,
            //    Margins = new Rectangle(0, 0, 0, 5)
            //};
            //AddChild(scrollVert);
            //_verticalScroll = scrollVert;

            //var scrollHorz = new EditorScrollBarHorizontal
            //{
            //    Dock = UIDockDirection.Bottom,
            //    OnScroll = ScrollBarCallbackHorizontal
            //};
            //AddChild(scrollHorz);
            //_horizontalScroll = scrollHorz;
        }
    }
}
