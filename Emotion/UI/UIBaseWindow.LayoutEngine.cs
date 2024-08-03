using System.ComponentModel;

namespace Emotion.UI;

public partial class UIBaseWindow
{
    public enum UIPass
    {
        Measure,
        Layout
    }

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
            public bool ReversedInList;
            public bool IsListSizeZero;

            public override string ToString()
            {
                return $"{Child} - {Bound}";
            }
        }

        private List<ChildData> _children = new(); // todo: optimize
        private Vector2 _listSpacing;
        private UIPass _pass;
        private LayoutMode _layoutMode;
        private int _listMask;
        private float _wrappingListOtherAxisLimit;

        public void Reset()
        {
            _bound = Rectangle.Empty;
            _padding = Rectangle.Empty;
            _children.Clear();
            _listSpacing = Vector2.Zero;
            _layoutMode = LayoutMode.Free;
            _listMask = 0;
            _wrappingListOtherAxisLimit = -1;
        }

        public void SetDimensions(Rectangle rect)
        {
            _bound = rect;
        }

        public void SetLayoutMode(UIPass pass, LayoutMode mode, Vector2 listSpacing)
        {
            _pass = pass;
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

        public void SetBoundLimit(Vector2 limit)
        {
            _bound.Size = Vector2.Min(_bound.Size, limit);
        }

        private Rectangle DeflateRect(Rectangle bound, Rectangle amount)
        {
            bound.X += amount.X;
            bound.Y += amount.Y;
            bound.Width -= amount.X + amount.Width;
            bound.Height -= amount.Y + amount.Height;
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
                case LayoutMode.HorizontalListWrap:
                case LayoutMode.VerticalListWrap:
                    appendData = ListWrapAppend(child, size);
                    break;
                case LayoutMode.Free:
                    appendData = FreeAppend(child, size);
                    break;
            }

            appendData.Margins = margins;
            _children.Add(appendData);
        }

        public Vector2 ApplyLayout()
        {
            Rectangle spaceUsedByChildren = Rectangle.Empty;
            for (int i = 0; i < _children.Count; i++)
            {
                var childData = _children[i];
                childData.EndOfList = i == _children.Count - 1;

                var childBound = childData.Bound;

                UIBaseWindow childWin = childData.Child;
                var childInsideParent = AnchorsInsideParent(childWin.ParentAnchor, childWin.Anchor);
                if (childInsideParent && childWin._expandParent)
                    spaceUsedByChildren = spaceUsedByChildren == Rectangle.Empty ? childBound : Rectangle.Union(spaceUsedByChildren, childBound);

                if (_pass == UIPass.Layout)
                {
                    // 1. Anchor must be before margin in order for w and h margins to work.
                    // 2. Limit must be after margin as not to fold the margin size into the window size.
                    // 3. Fill has been decided to be after anchors in order for anchors to matter to filling children.

                    childBound = ApplyAnchors(ref childData, childBound);
                    childBound = ApplyFill(ref childData, childBound);
                    childBound = DeflateRect(childBound, childData.Margins); // Subtract the margins from the child size, since the child should be layouted inside.
                    childBound = ApplyLimits(ref childData, childBound);

                    childData.Child.Layout(childBound.Position, childBound.Size);
                }
            }

            // Technically padding is also space used by the children.
            Vector2 paddingSize = new Vector2(_padding.X + _padding.Width, _padding.Y + _padding.Height);

            return spaceUsedByChildren.Size + paddingSize;
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

        public bool IsList()
        {
            return _layoutMode is LayoutMode.HorizontalList or LayoutMode.VerticalList;
        }

        private bool IsListItemInReversedOrder(int listMask, UIBaseWindow child)
        {
            bool isReverse = false;
            if (listMask == 0)
                isReverse = child.ParentAnchor is UIAnchor.TopRight or UIAnchor.CenterRight or UIAnchor.BottomRight;
            else if (listMask == 1)
                isReverse = child.ParentAnchor is UIAnchor.BottomLeft or UIAnchor.BottomCenter or UIAnchor.BottomRight;
            return isReverse;
        }

        private bool ListHasAnyItems(bool reverseSide, int filterAxis = -1, float limitInAxis = -1)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                ChildData child = _children[i];
                if (child.IsListSizeZero) continue;

                if (filterAxis != -1)
                {
                    var filterLimit = child.Bound.Position[filterAxis];
                    if (filterLimit < limitInAxis) continue;
                }

                if (child.ReversedInList == reverseSide) return true;
            }
            return false;
        }

        private ChildData ListAppend(UIBaseWindow child, Vector2 size)
        {
            bool measurePass = _pass == UIPass.Measure;
            int listMask = _listMask;

            bool isReverse = IsListItemInReversedOrder(listMask, child);
            if (measurePass) isReverse = false; // In the measure pass we cant layout the reverse side.

            float wall = ListGetWallInDirection(listMask, isReverse);
            if (isReverse) wall -= size[listMask];

            Vector2 boundPos = _bound.Position;
            Vector2 boundSize = _bound.Size;

            Vector2 rowOrColumnPos = boundPos;
            rowOrColumnPos[listMask] = wall;

            Vector2 rowOrColumnSize = boundSize;
            rowOrColumnSize[listMask] = rowOrColumnSize[listMask] - (boundPos[listMask] - rowOrColumnPos[listMask]);

            bool isListSizeZero = size[listMask] == 0;
            if (!isListSizeZero && ListHasAnyItems(isReverse)) // Add spacing if not first item and not size 0.
            {
                if (isReverse)
                    rowOrColumnPos[listMask] -= _listSpacing[listMask];
                else
                    rowOrColumnPos[listMask] += _listSpacing[listMask];
            }

            return new ChildData()
            {
                Child = child,
                Bound = new Rectangle(rowOrColumnPos, size),
                ReversedInList = isReverse,
                IsListSizeZero = isListSizeZero
            };
        }

        private float ListGetWallInDirection(int mask, bool isReverse, float limitInOtherAxis = -1)
        {
            float wall;
            if (isReverse)
                wall = _bound.Position[mask] + _bound.Size[mask];
            else
                wall = _bound.Position[mask];

            for (int i = 0; i < _children.Count; i++)
            {
                ChildData winData = _children[i];
                Rectangle winBound = winData.Bound;

                if (limitInOtherAxis != -1)
                {
                    bool skip = winBound.Position[mask == 0 ? 1 : 0] < limitInOtherAxis;
                    if (skip) continue;
                }

                bool isWindowReversed = winData.ReversedInList;
                if (isReverse)
                {
                    if (!isWindowReversed) continue;

                    float boundWall = winBound.Position[mask];
                    if (boundWall < wall)
                        wall = boundWall;
                }
                else
                {
                    float boundWall = winBound.Position[mask] + winBound.Size[mask];
                    if (boundWall > wall)
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

        #region List Wrap

        private ChildData ListWrapAppend(UIBaseWindow child, Vector2 size)
        {
            bool measurePass = _pass == UIPass.Measure;
            int listMask = _listMask;
            int reverseListMask = _listMask == 0 ? 1 : 0;

            //if (measurePass)
            //{
            //    return new ChildData()
            //    {
            //        Child = child,
            //        Bound = new Rectangle(_bound.Position, size),
            //        ReversedInList = false,
            //        IsListSizeZero = false
            //    };
            //}

            float wall = ListGetWallInDirection(listMask, false, _wrappingListOtherAxisLimit);

            Vector2 boundPos = _bound.Position;
            Vector2 boundSize = _bound.Size;

            bool wrap = wall + size[listMask] > boundPos[listMask] + boundSize[listMask];
            if (wrap)
            {
                float limit = ListGetWallInDirection(reverseListMask, false);
                _wrappingListOtherAxisLimit = limit + _listSpacing[reverseListMask];
                wall = ListGetWallInDirection(listMask, false, _wrappingListOtherAxisLimit);
            }

            Vector2 rowOrColumnPos = boundPos;
            rowOrColumnPos[listMask] = wall;
            if (_wrappingListOtherAxisLimit != -1)
                rowOrColumnPos[reverseListMask] = _wrappingListOtherAxisLimit;

            Vector2 rowOrColumnSize = boundSize;
            rowOrColumnSize[listMask] = rowOrColumnSize[listMask] - (boundPos[listMask] - rowOrColumnPos[listMask]);

            // Add spacing if not first item and not size 0.
            bool isListSizeZero = size[listMask] == 0;
            if (!isListSizeZero && ListHasAnyItems(false, reverseListMask, _wrappingListOtherAxisLimit))
                rowOrColumnPos[listMask] += _listSpacing[listMask];

            return new ChildData()
            {
                Child = child,
                Bound = new Rectangle(rowOrColumnPos, size),
                ReversedInList = false,
                IsListSizeZero = isListSizeZero
            };
        }

        #endregion

        private Rectangle ApplyAnchors(ref ChildData childData, Rectangle childBound)
        {
            Rectangle myItemSpace = _bound;
            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                    myItemSpace = _bound;
                    myItemSpace.Width = childBound.Width;
                    break;
                case LayoutMode.VerticalList:
                    myItemSpace = _bound;
                    myItemSpace.Height = childBound.Height;
                    break;
                case LayoutMode.HorizontalListWrap:
                case LayoutMode.VerticalListWrap:
                    myItemSpace.Width = childBound.Width;
                    myItemSpace.Height = childBound.Height;
                    break;
            }

            UIBaseWindow childWin = childData.Child;
            Vector2 positionFromLayout = childBound.Position;
            Vector2 positionOffset = GetAnchorOffset(childWin.Anchor, childWin.ParentAnchor, childBound.Size, myItemSpace);
            Vector2 diff = positionOffset - _bound.Position;

            return new Rectangle(positionFromLayout + diff, childBound.Size);
        }

        private Vector2 GetAnchorOffset(UIAnchor anchor, UIAnchor parentAnchor, Vector2 contentSize, Rectangle parentContentRect)
        {
            Vector2 offset = Vector2.Zero;

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
