namespace Emotion.UI;

public partial class UIBaseWindow
{
    private UILayoutEngine _layoutEngine = new UILayoutEngine();

    public class UILayoutEngine
    {
        private Rectangle _bound;
        private Rectangle _padding;

        private struct ChildData
        {
            public UIBaseWindow Child;
            public Rectangle Bound;
            public Rectangle Margins;
            public bool EndOfList;
        }

        private List<ChildData> _children = new(); // todo: optimize
        private Vector2 _listSpacing;
        private LayoutMode _layoutMode;
        private int _listMask;

        public void Reset()
        {
            _bound = Rectangle.Empty;
            _padding = Rectangle.Empty;
            _children.Clear();
            _listSpacing = Vector2.Zero;
            _layoutMode = LayoutMode.Free;
            _listMask = 0;
        }

        public void SetDimensions(Rectangle rect)
        {
            _bound = rect;
        }

        public void SetLayoutMode(LayoutMode mode, Vector2 listSpacing)
        {
            _layoutMode = mode;
            _listSpacing = listSpacing;
            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                    _listMask = 0;
                    break;
                case LayoutMode.VerticalList:
                    _listMask = 1;
                    break;
            }
        }

        public void DeflateDimensions(Rectangle rect)
        {
            _bound = DeflateRect(_bound, rect);
        }

        public void AddPadding(Rectangle rect)
        {
            DeflateDimensions(rect);
            _padding = rect;
        }

        private Rectangle DeflateRect(Rectangle bound, Rectangle amount)
        {
            bound.X += amount.X;
            bound.Y += amount.Y;
            bound.Width -= amount.X + amount.Width;
            bound.Height -= amount.Y + amount.Width;
            return bound;
        }

        public Vector2 GetFreeSpace()
        {
            return _bound.Size;
        }

        public void AppendChild(UIBaseWindow child, Vector2 size, Rectangle margins)
        {
            size += new Vector2(margins.X + margins.Width, margins.Y + margins.Height);

            ChildData appendData = new ChildData();
            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                case LayoutMode.VerticalList:
                    appendData = ListAppend(child, size);
                    break;
                case LayoutMode.Free:
                    appendData = FreeAppend(child, size);
                    break;
            }

            appendData.Margins = margins;
            _children.Add(appendData);
        }

        public Vector2 ApplyLayout(bool dryRun = false)
        {
            Rectangle rectUnion = Rectangle.Empty;
            for (int i = 0; i < _children.Count; i++)
            {
                var childData = _children[i];
                childData.EndOfList = i == _children.Count - 1;

                var childBound = childData.Bound;

                // Subtract the margins from the child size,
                // since the child should be layouted inside.
                childBound = DeflateRect(childBound, childData.Margins);
                rectUnion = i == 0 ? childBound : Rectangle.Union(rectUnion, childBound);

                if (!dryRun)
                {
                    childBound = ApplyFill(ref childData, childBound);
                    childBound = ApplyLimits(ref childData, childBound);
                    childBound = ApplyAnchors(ref childData, childBound);
                    childData.Child.Layout(childBound.Position, childBound.Size);
                }
            }

            // Technically padding is also space used by the children.
            Vector2 paddingSize = new Vector2(_padding.X + _padding.Width, _padding.Y + _padding.Height);

            return rectUnion.Size + paddingSize;
        }

        private Rectangle ApplyFill(ref ChildData childData, Rectangle childBound)
        {
            var childWin = childData.Child;

            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                case LayoutMode.VerticalList:
                    {
                        if (childWin.FillX)
                            childBound = List_FillX(ref childData, childBound);
                        if (childWin.FillY)
                            childBound = List_FillY(ref childData, childBound);
                        return childBound;
                    }
                case LayoutMode.Free:
                    {
                        if (childWin.FillX)
                            childBound = Free_FillX(ref childData, childBound);
                        if (childWin.FillY)
                            childBound = Free_FillY(ref childData, childBound);
                        return childBound;
                    }
            }

            return childBound;
        }

        private Rectangle ApplyLimits(ref ChildData childData, Rectangle childBound)
        {
            var childWin = childData.Child;
            var scale = childWin.GetScale();

            Vector2 size = childBound.Size;
            size = Vector2.Clamp(size, childWin.MinSize * scale, childWin.MaxSize * scale);
            size = size.Ceiling();

            childBound.Size = size;
            return childBound;
        }

        #region Free

        private ChildData FreeAppend(UIBaseWindow child, Vector2 size)
        {
            return new ChildData()
            {
                Child = child,
                Bound = new Rectangle(_bound.Position, size)
            };
        }

        private Rectangle Free_FillX(ref ChildData childData, Rectangle childBound)
        {
            childBound.Width = _bound.X + _bound.Width - childBound.X;
            return childBound;
        }

        private Rectangle Free_FillY(ref ChildData childData, Rectangle childBound)
        {
            childBound.Height = _bound.Y + _bound.Height - childBound.Y;
            return childBound;
        }

        #endregion

        #region List

        private ChildData ListAppend(UIBaseWindow child, Vector2 size)
        {
            int listMask = _listMask;

            float wall = ListGetLowerWallInMaskDirection(listMask);

            var boundPos = _bound.Position;
            var boundSize = _bound.Size;

            var rowOrColumnPos = boundPos;
            var rowOrColumnSize = boundSize;
            rowOrColumnPos[listMask] = wall;
            rowOrColumnSize[listMask] = rowOrColumnSize[listMask] - (boundPos[listMask] - rowOrColumnPos[listMask]);

            Rectangle rowOrColumn = new Rectangle(rowOrColumnPos, rowOrColumnSize);

            if (_children.Count > 0 && size[listMask] != 0)
            {
                rowOrColumnPos[listMask] += _listSpacing[listMask];
            }

            return new ChildData()
            {
                Child = child,
                Bound = new Rectangle(rowOrColumnPos, size)
            };
        }

        private float ListGetLowerWallInMaskDirection(int mask)
        {
            float wall = _bound.Position[mask];
            for (int i = 0; i < _children.Count; i++)
            {
                var win = _children[i];
                var winBound = win.Bound;
                float boundWall = winBound.Position[mask] + winBound.Size[mask];
                if (boundWall > wall)
                {
                    wall = boundWall;
                }
            }

            return wall;
        }

        private Rectangle List_FillX(ref ChildData childData, Rectangle childBound)
        {
            if (childData.EndOfList || _layoutMode == LayoutMode.VerticalList)
                childBound.Width = _bound.X + _bound.Width - childBound.X;
            return childBound;
        }

        private Rectangle List_FillY(ref ChildData childData, Rectangle childBound)
        {
            if (childData.EndOfList || _layoutMode == LayoutMode.HorizontalList)
                childBound.Height = _bound.Y + _bound.Height - childBound.Y;
            return childBound;
        }

        #endregion

        private Rectangle ApplyAnchors(ref ChildData childData, Rectangle childBound)
        {
            Vector2 positionOffset = GetAnchorOffset(childData.Child, childBound.Size, _bound);

            return new Rectangle(childBound.Position + positionOffset, childBound.Size);
        }

        private Vector2 GetAnchorOffset(UIBaseWindow win, Vector2 contentSize, Rectangle parentContentRect)
        {
            Vector2 offset = Vector2.Zero;
            UIAnchor anchor = win.Anchor;
            UIAnchor parentAnchor = win.ParentAnchor;

            switch (parentAnchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.CenterLeft:
                case UIAnchor.BottomLeft:
                    offset.X += parentContentRect.X;
                    break;
                case UIAnchor.TopCenter:
                case UIAnchor.CenterCenter:
                case UIAnchor.BottomCenter:
                    offset.X += parentContentRect.Center.X;
                    break;
                case UIAnchor.TopRight:
                case UIAnchor.CenterRight:
                case UIAnchor.BottomRight:
                    offset.X += parentContentRect.Right;
                    break;
            }

            switch (parentAnchor)
            {
                case UIAnchor.TopLeft:
                case UIAnchor.TopCenter:
                case UIAnchor.TopRight:
                    offset.Y += parentContentRect.Y;
                    break;
                case UIAnchor.CenterLeft:
                case UIAnchor.CenterCenter:
                case UIAnchor.CenterRight:
                    offset.Y += parentContentRect.Center.Y;
                    break;
                case UIAnchor.BottomLeft:
                case UIAnchor.BottomCenter:
                case UIAnchor.BottomRight:
                    offset.Y += parentContentRect.Bottom;
                    break;
            }

            switch (anchor)
            {
                case UIAnchor.TopCenter:
                case UIAnchor.CenterCenter:
                case UIAnchor.BottomCenter:
                    offset.X -= contentSize.X / 2;
                    break;
                case UIAnchor.TopRight:
                case UIAnchor.CenterRight:
                case UIAnchor.BottomRight:
                    offset.X -= contentSize.X;
                    break;
            }

            switch (anchor)
            {
                case UIAnchor.CenterLeft:
                case UIAnchor.CenterCenter:
                case UIAnchor.CenterRight:
                    offset.Y -= contentSize.Y / 2;
                    break;
                case UIAnchor.BottomLeft:
                case UIAnchor.BottomCenter:
                case UIAnchor.BottomRight:
                    offset.Y -= contentSize.Y;
                    break;
            }

            return offset;
        }

        public void RenderDebug(UIBaseWindow p, RenderComposer c)
        {
            Color[] colorList = new Color[]
            {
                Color.Red,
                Color.Green,
                Color.Blue,
            };

            c.RenderSprite(p.Bounds, new Color(100, 100, 100));
            for (int i = 0; i < _children.Count; i++)
            {
                c.RenderSprite(_children[i].Child.Bounds, Color.White * 0.2f);
            }

            for (int i = 0; i < _children.Count; i++)
            {
                c.RenderOutline(_children[i].Child.Bounds, Color.Red * 0.2f, 2f);
            }
        }
    }
}
