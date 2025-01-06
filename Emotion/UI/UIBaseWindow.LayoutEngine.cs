using Emotion.WIPUpdates.One.Tools;

namespace Emotion.UI;

#nullable enable

public partial class UIBaseWindow
{
    public enum UIPass
    {
        Measure,
        Layout,
        LayoutExtra
    }

    private UILayoutEngine _layoutEngine = new UILayoutEngine();

    public class UILayoutEngine
    {
        private Rectangle _bound;
        private Rectangle _boundWithoutPadding;
        private Rectangle _padding;

        private struct ChildData
        {
            public UIBaseWindow Child;
            public Rectangle Bound;

            public bool InsideParent;
            public bool OutsideCurrentLayout;
            public bool EndOfList;
            public bool ReversedInList;
            public bool IsListSizeZero;

            public override string ToString()
            {
                return $"{Child} - {Bound}";
            }
        }

        private List<ChildData> _children = new(); // todo: optimize
        private List<ChildData> _childrenPrePass = new(); // todo: optimize
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
            _childrenPrePass.Clear();
            _listSpacing = Vector2.Zero;
            _layoutMode = LayoutMode.Free;
            _listMask = 0;
            _wrappingListOtherAxisLimit = -1;
        }

        public void SetLayoutMode(UIPass pass, LayoutMode mode, Vector2 listSpacing)
        {
            _pass = pass;
            _layoutMode = mode;
            _listSpacing = listSpacing;
            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                case LayoutMode.HorizontalEditorPanel:
                    _listMask = 0;
                    break;
                case LayoutMode.VerticalList:
                    _listMask = 1;
                    break;
            }
        }

        public void SetLayoutDimensions(Rectangle dimensions, Rectangle margins, Vector2 limit, Rectangle paddings)
        {
            _bound = dimensions;

            // The parent's margins and paddings is space children cannot take, so we take them out.
            // For more explanation on the order of operations check out ApplyLayout.

            _bound = DeflateRect(_bound, margins);

            _bound.Size = Vector2.Min(_bound.Size, limit);

            _boundWithoutPadding = _bound;
            _bound = DeflateRect(_bound, paddings);
            _padding = paddings;
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

            ChildData childData = new ChildData();
            childData.Child = child;
            childData.InsideParent = AnchorsInsideParent(child.ParentAnchor, child.Anchor);
            childData.OutsideCurrentLayout = !childData.InsideParent || child.BackgroundWindow;

            switch (_layoutMode)
            {
                case LayoutMode _ when childData.OutsideCurrentLayout:
                    OutsideLayoutAppend(child, size, ref childData);
                    break;
                case LayoutMode.HorizontalEditorPanel when _pass == UIPass.LayoutExtra:
                case LayoutMode.HorizontalList:
                case LayoutMode.VerticalList:
                    ListAppend(child, size, ref childData);
                    break;
                case LayoutMode.HorizontalListWrap:
                case LayoutMode.VerticalListWrap:
                    ListWrapAppend(child, size, ref childData);
                    break;
                case LayoutMode.HorizontalEditorPanel:
                case LayoutMode.Free:
                    FreeAppend(child, size, ref childData);
                    break;
            }

            _children.Add(childData);
        }

        public Vector2 ApplyMeasure()
        {
            Rectangle spaceUsedByChildren = Rectangle.Empty;
            for (int i = 0; i < _children.Count; i++)
            {
                ChildData childData = _children[i];
                childData.EndOfList = i == _children.Count - 1;

                Rectangle childBound = childData.Bound;

                UIBaseWindow childWin = childData.Child;
                bool childInsideParent = childData.InsideParent;
                if (childInsideParent && childWin._expandParent)
                    spaceUsedByChildren = spaceUsedByChildren == Rectangle.Empty ? childBound : Rectangle.Union(spaceUsedByChildren, childBound);
            }

            // Technically padding is also space used by the children.
            Vector2 paddingSize = new Vector2(_padding.X + _padding.Width, _padding.Y + _padding.Height);

            return spaceUsedByChildren.Size + paddingSize;
        }

        public void ApplyLayout()
        {
            // Special panel mode
            bool requireExtraPass = false;
            if (_layoutMode == LayoutMode.HorizontalEditorPanel)
            {
                requireExtraPass = true;
                ApplyPanelModePrePass();
            }

            // Some UI layouts require an extra pass, so re-append children now that we
            // have done extra calculations with the knowledge of what all of our children are.
            if (requireExtraPass)
            {
                _childrenPrePass.AddRange(_children);
                _children.Clear();

                _pass = UIPass.LayoutExtra;
                for (int i = 0; i < _childrenPrePass.Count; i++)
                {
                    ChildData childDataPre = _childrenPrePass[i];
                    AppendChild(childDataPre.Child, childDataPre.Bound.Size, Rectangle.Empty); // Margins should have been added to size by the first layout pass.
                }
            }

            for (int i = 0; i < _children.Count; i++)
            {
                ChildData childData = _children[i];
                childData.EndOfList = i == _children.Count - 1;

                Rectangle childBound = childData.Bound;

                UIBaseWindow childWin = childData.Child;
                bool childInsideParent = childData.InsideParent;

                // 1. Anchor must be before margin in order for w and h margins to work.
                // 2. Limit must be after margin as not to fold the margin size into the window size.
                // 3. Fill has been decided to be after anchors in order for anchors to matter to filling children.

                UIController.DebugShouldBreakpointLayout(childWin);

                childBound = ApplyAnchors(ref childData, childBound);
                if (childInsideParent) childBound = ApplyFill(ref childData, childBound);
                childBound = DeflateRect(childBound, childWin.Margins * childWin.GetScale()); // Subtract the margins from the child size, since the child should be layouted inside.
                childBound = ApplyLimits(ref childData, childBound);

                childData.Child.Layout(childBound.Position, childBound.Size);
            }
        }

        #region Outside Layout

        private void OutsideLayoutAppend(UIBaseWindow child, Vector2 size, ref ChildData data)
        {
            data.Bound = new Rectangle(_boundWithoutPadding.Position, size);
        }

        private Rectangle Outside_FillX(ref ChildData childData, Rectangle childBound)
        {
            childBound.Width = _boundWithoutPadding.X + _boundWithoutPadding.Width - childBound.X;
            return childBound;
        }

        private Rectangle Outside_FillY(ref ChildData childData, Rectangle childBound)
        {
            childBound.Height = _boundWithoutPadding.Y + _boundWithoutPadding.Height - childBound.Y;
            return childBound;
        }

        #endregion

        #region Free

        private void FreeAppend(UIBaseWindow child, Vector2 size, ref ChildData data)
        {
            data.Bound = new Rectangle(_bound.Position, size);
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
                if (child.OutsideCurrentLayout) continue;

                if (filterAxis != -1)
                {
                    var filterLimit = child.Bound.Position[filterAxis];
                    if (filterLimit < limitInAxis) continue;
                }

                if (child.ReversedInList == reverseSide) return true;
            }
            return false;
        }

        private void ListAppend(UIBaseWindow child, Vector2 size, ref ChildData data)
        {
            int listMask = _listMask;

            bool isReverse = IsListItemInReversedOrder(listMask, child);
            if (_pass == UIPass.Measure) isReverse = false; // In the measure pass we cant layout the reverse side.

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

            data.Bound = new Rectangle(rowOrColumnPos, size);
            data.ReversedInList = isReverse;
            data.IsListSizeZero = isListSizeZero;
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
                if (winData.OutsideCurrentLayout) continue;

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
            if (childData.EndOfList || _listMask == 1)
                childBound.Width = _bound.X + _bound.Width - childBound.X;
            return childBound;
        }

        private Rectangle List_FillY(ref ChildData childData, Rectangle childBound)
        {
            if (childData.EndOfList || _listMask == 0)
                childBound.Height = _bound.Y + _bound.Height - childBound.Y;
            return childBound;
        }

        #endregion

        #region List Wrap

        private void ListWrapAppend(UIBaseWindow child, Vector2 size, ref ChildData data)
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

            data.Bound = new Rectangle(rowOrColumnPos, size);
            data.ReversedInList = false;
            data.IsListSizeZero = isListSizeZero;
        }

        #endregion

        #region Panel

        private void ApplyPanelModePrePass()
        {
            // temp
            float sizeAvailable = _bound.Width;
            HorizontalPanelSeparator? separator = null;
            float separation = 0.5f;
            for (int i = 0; i < _children.Count; i++)
            {
                ChildData childData = _children[i];
                UIBaseWindow childWin = childData.Child;
                if (childWin is HorizontalPanelSeparator sep)
                {
                    separation = sep.SeparationPercent;
                    separator = sep;

                    sizeAvailable -= childData.Bound.Width;
                }
            }

            float rightSidePercent = 1f - separation;

            ChildData leftChild = _children[0];
            leftChild.Bound.Width = sizeAvailable * separation;
            _children[0] = leftChild;

            ChildData rightChild = _children[2];
            rightChild.Bound.Width = sizeAvailable * separation;
            _children[2] = rightChild;
        }
        
        #endregion

        private Rectangle ApplyFill(ref ChildData childData, Rectangle childBound)
        {
            UIBaseWindow childWin = childData.Child;

            switch (_layoutMode)
            {
                case LayoutMode _ when childData.OutsideCurrentLayout:
                    {
                        if (childWin.FillX)
                            childBound = Outside_FillX(ref childData, childBound);
                        if (childWin.FillY)
                            childBound = Outside_FillY(ref childData, childBound);
                        break;
                    }

                case LayoutMode.HorizontalEditorPanel:
                case LayoutMode.HorizontalList:
                case LayoutMode.VerticalList:
                    {
                        if (childWin.FillX)
                            childBound = List_FillX(ref childData, childBound);
                        if (childWin.FillY)
                            childBound = List_FillY(ref childData, childBound);
                        break;
                    }

                case LayoutMode.Free:
                    {
                        if (childWin.FillX)
                            childBound = Free_FillX(ref childData, childBound);
                        if (childWin.FillY)
                            childBound = Free_FillY(ref childData, childBound);
                        break;
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

        private Rectangle ApplyAnchors(ref ChildData childData, Rectangle childBound)
        {
            Rectangle myItemSpace = _bound;
            if (childData.OutsideCurrentLayout) myItemSpace = _boundWithoutPadding;

            switch (_layoutMode)
            {
                case LayoutMode.HorizontalList:
                    myItemSpace.Width = childBound.Width;
                    break;
                case LayoutMode.VerticalList:
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
            Vector2 diff = positionOffset - myItemSpace.Position;

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
