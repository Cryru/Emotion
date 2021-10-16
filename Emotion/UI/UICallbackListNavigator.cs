#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.UI
{
    /// <summary>
    /// Provides list navigation to UICallbackButton children.
    /// </summary>
    public class UICallbackListNavigator : UIBaseWindow
    {
        /// <summary>
        /// If enabled all children windows outside the bounds of the list will not be rendered.
        /// On by default, and needed to support scrolling.
        /// </summary>
        public bool HideOutsideChildren = true;

        // These three are the same thing.
        [DontSerialize]
        public UIBaseWindow? SelectedWnd { get; protected set; }

        [DontSerialize]
        public int SelectedChildIdx { get; protected set; } = -1;

        [DontSerialize]
        public Vector2 SelectedChildPos
        {
            get => _selectedPos;
        }

        /// <summary>
        /// Callback on new item selected. First param is the new item, second is the old one.
        /// </summary>
        [DontSerialize] public Action<UIBaseWindow?, UIBaseWindow?>? OnItemSelected;

        /// <summary>
        /// Callback on ConfirmChoice pressed. Passes in the selected window and its index.
        /// </summary>
        [DontSerialize] public Action<UIBaseWindow, int>? OnChoiceConfirmed;

        [DontSerialize] protected Vector2 _selectedPos;

        public Key NavigationKey;
        public Key ConfirmChoice;

        private Dictionary<Vector2, UIBaseWindow> _gridPosToChild = new Dictionary<Vector2, UIBaseWindow>();
        private Vector2 _gridSize;
        private int _lastRowColumn;

        private Rectangle _scrollArea; // The total area of all children that is being scrolled through.
        private Vector2 _scrollPos = Vector2.Zero; // The grid-like-pos of the current child in view.
        private int _firstVisibleChild = -1; // Render-wise first visible child index.
        private int _lastVisibleChild = -1; // ^ same but last.
        private Vector2 _lastScrollChildPos; // The grid-like-pos of the last child that can be scrolled to.
        private Matrix4x4 _scrollDisplacement = Matrix4x4.Identity; // The current scroll translation.

        private UIScrollbar? _scrollBar;

        public UICallbackListNavigator()
        {
            InputTransparent = false;
        }

        protected override Vector2 GetChildrenLayoutSize(Vector2 space, Vector2 measuredSize, Vector2 paddingSize)
        {
            Vector2 baseChildSize = base.GetChildrenLayoutSize(space, measuredSize, paddingSize);
            Vector2 scrollRange = baseChildSize;
            switch (LayoutMode)
            {
                case LayoutMode.VerticalListWrap:
                case LayoutMode.HorizontalList:
                    scrollRange.X = MaxSize.X;
                    break;
                case LayoutMode.HorizontalListWrap:
                case LayoutMode.VerticalList:
                    scrollRange.Y = MaxSize.Y;
                    break;
            }

            Rectangle parentPadding = Paddings * GetScale();
            _scrollArea = new Rectangle(parentPadding.X, parentPadding.Y, -1, -1);
            return scrollRange;
        }

        protected override void AfterMeasureChildren(Vector2 usedSpace)
        {
            _scrollArea.Size = usedSpace;
            base.AfterMeasureChildren(usedSpace);
        }

        protected override void AfterLayout()
        {
            if (Children == null) return;

            _gridPosToChild.Clear();
            _gridSize = Vector2.Zero;

            var pen = new Vector2();
            for (var i = 0; i < Children.Count; i++)
            {
                UIBaseWindow child = Children[i];
                if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                _gridPosToChild.Add(pen, child);
                _gridSize.X = MathF.Max(pen.X, _gridSize.X);
                _gridSize.Y = MathF.Max(pen.Y, _gridSize.Y);

                switch (LayoutMode)
                {
                    case LayoutMode.Free:
                        pen.X++;
                        pen.Y++;
                        break;
                    case LayoutMode.HorizontalListWrap:
                        pen.X++;
                        if (i != Children.Count - 1 && Children[i + 1].Y > child.Y)
                        {
                            pen.X = 0;
                            pen.Y++;
                        }

                        break;
                    case LayoutMode.HorizontalList:
                        pen.X++;
                        break;
                    case LayoutMode.VerticalListWrap:
                        pen.Y++;
                        if (i != Children.Count - 1 && Children[i + 1].X > child.X)
                        {
                            pen.Y = 0;
                            pen.X++;
                        }

                        break;
                    case LayoutMode.VerticalList:
                        pen.Y++;
                        break;
                }
            }

            _lastRowColumn = (int) pen.X - 1;

            // Reset visible.
            _firstVisibleChild = -1;
            _lastVisibleChild = -1;

            // Add position to scroll rect.
            _scrollArea.X += X;
            _scrollArea.Y += Y;

            // Calculate last child that can be scrolled to.
            float min = 0, maxArea = 0, max = 0;
            _lastScrollChildPos = Vector2.Zero;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIBaseWindow child = Children[i];
                if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

                switch (LayoutMode)
                {
                    case LayoutMode.HorizontalListWrap:
                    case LayoutMode.HorizontalList:
                        max = Width;
                        maxArea = _scrollArea.Width + _scrollArea.Width;
                        min = child.X;
                        break;
                    case LayoutMode.VerticalListWrap:
                    case LayoutMode.VerticalList:
                        max = Height;
                        maxArea = _scrollArea.Y + _scrollArea.Height;
                        min = child.Y;
                        break;
                }

                if (min < maxArea - max && i != Children.Count - 1)
                {
                    _lastScrollChildPos = GetGridLikePosFromChild(Children[i + 1]);
                    break;
                }
            }

            if (_scrollBar != null)
            {
                // Todo: horizontal list
                // Make scroll bar as big as the children shown. Might look weird if all children are not the same size.
                float childrenHeight = Children[0].Height;
                float scaledListSpacing = ListSpacing.Y * GetScale();
                float visibleChildrenAtATime = MathF.Floor((Height + scaledListSpacing) / (childrenHeight + scaledListSpacing));
                float scrollRange = (childrenHeight * visibleChildrenAtATime) + scaledListSpacing * (visibleChildrenAtATime - 1);
                scrollRange /= GetScale();
                if (scrollRange != _scrollBar.MaxSize.Y)
                {
                    _scrollBar.MaxSize = new Vector2(_scrollBar.MaxSize.X, scrollRange);
                    _scrollBar.InvalidateLayout();
                }
               
                SyncScrollbar();
            }

            base.AfterLayout();
        }

        protected override void RenderChildren(RenderComposer c)
        {
            Rectangle renderRect = _renderBounds;
            // c.RenderOutline(renderRect, Color.Red);

            c.PushModelMatrix(_scrollDisplacement);
            var lastVis = 0;
            for (var i = 0; i < Children!.Count; i++)
            {
                UIBaseWindow child = Children[i];
                child.EnsureRenderBoundsCached(c);

                if (!child.IsInsideRect(renderRect) && HideOutsideChildren) continue;
                if (child.Visible) child.Render(c);
                if (_firstVisibleChild == -1) _firstVisibleChild = i;
                lastVis = i;
            }

            c.PopModelMatrix();
            if (_lastVisibleChild == -1) _lastVisibleChild = lastVis;
        }

        private void ScrollToPos(Vector2 gridLikePos)
        {
            UIBaseWindow? child = GetChildByGridLikePos(gridLikePos, out int _, true);
            if (child == null) return;
            _scrollPos = Vector2.Zero;
            _scrollPos = gridLikePos;
            _scrollDisplacement = Matrix4x4.CreateTranslation(
                _scrollArea.X - child.X + child.Margins.X * child.GetScale(),
                _scrollArea.Y - child.Y + child.Margins.Y * child.GetScale(), 0);
            _firstVisibleChild = -1;
            _lastVisibleChild = -1;
            SyncScrollbar();
        }

        public UIBaseWindow? GetChildByGridLikePos(Vector2 gridLikePos, out int index, bool includeInvisible)
        {
            index = -1;
            if (Children == null) return null;
            if (!_gridPosToChild.TryGetValue(gridLikePos, out UIBaseWindow? child)) return null;
            if (!child.Visible && !includeInvisible) return null;
            index = Children.IndexOf(child);
            return child;
        }

        public Vector2 GetGridLikePosFromChild(UIBaseWindow win)
        {
            foreach (KeyValuePair<Vector2, UIBaseWindow> child in _gridPosToChild)
            {
                if (child.Value == win) return child.Key;
            }

            return Vector2.Zero;
        }

        public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
        {
            if (Children == null) return true;

            Vector2 axis = Engine.Host.IsKeyPartOfAxis(key, NavigationKey);
            if (axis != Vector2.Zero)
            {
                UIBaseWindow? newItem = null;
                var childIdx = 0;
                while ((newItem == null || !newItem.Visible) && childIdx != Children.Count - 1) // Go next until visible.
                {
                    _selectedPos += axis;
                    _selectedPos = Vector2.Clamp(_selectedPos, Vector2.Zero, _gridSize);
                    if (_selectedPos.Y == _gridSize.Y && _selectedPos.X > _lastRowColumn && _lastRowColumn != -1) _selectedPos.X = _lastRowColumn;
                    newItem = GetChildByGridLikePos(_selectedPos, out childIdx, true);
                }
                if (newItem == null || !newItem.Visible) return true; // Reached end.
                Debug.Assert(childIdx != -1);

                // Check if the new item is on screen.
                if (childIdx > _lastVisibleChild || childIdx < _firstVisibleChild)
                {
                    Vector2 diff = Vector2.Normalize(_selectedPos - _scrollPos);
                    ScrollToPos(_scrollPos + diff);
                }

                UIBaseWindow? oldSel = SelectedWnd;
                SelectedWnd = newItem;
                SelectedChildIdx = childIdx;
                if (newItem != oldSel) OnItemSelected?.Invoke(newItem, oldSel);
            }

            if (key == ConfirmChoice && status == KeyStatus.Down && SelectedWnd != null)
            {
                OnChoiceConfirmed?.Invoke(SelectedWnd, SelectedChildIdx);
                return false;
            }

            return base.OnKey(key, status, mousePos);
        }

        public override void OnMouseScroll(float scroll)
        {
            // Todo: Horizontal list implementation
            bool up = scroll > 0;
            if (_firstVisibleChild == -1 || _lastVisibleChild == -1 || Children == null) return;

            if (up)
            {
                int firstVisibleChild = _firstVisibleChild;
                firstVisibleChild--;
                if (firstVisibleChild < 0) return;
                Vector2 gridPos = GetGridLikePosFromChild(Children[firstVisibleChild]);

                Vector2 diff = Vector2.Normalize(gridPos - _scrollPos);
                ScrollToPos(_scrollPos + diff);
            }
            else
            {
                int lastChildIdx = _lastVisibleChild;
                lastChildIdx++;
                if (lastChildIdx == Children.Count) return;
                Vector2 gridPos = GetGridLikePosFromChild(Children[lastChildIdx]);

                Vector2 diff = Vector2.Normalize(gridPos - _scrollPos);
                ScrollToPos(_scrollPos + diff);
            }
        }

        public void ResetSelection(bool nullSelection = false)
        {
            if (nullSelection)
            {
                SelectedWnd = null;
                SelectedChildIdx = -1;
                _selectedPos = Vector2.Zero;
                return;
            }

            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i].Visible)
                {
                    SetSelection(Children[i], true);
                    return;
                }
            }
        }

        public void SetSelection(UIBaseWindow wnd, bool force = false)
        {
            if (Children == null) return;

            UIBaseWindow? oldSel = SelectedWnd;
            if (wnd == oldSel)
            {
                // Prevent event from having old == new
                if (force)
                    oldSel = null;
                else
                    return;
            }

            Debug.Assert(Children.IndexOf(wnd) != -1);
            SelectedChildIdx = Children.IndexOf(wnd);
            SelectedWnd = wnd;
            _selectedPos = GetGridLikePosFromChild(wnd);

            OnItemSelected?.Invoke(wnd, oldSel);
        }

        public void SetupMouseSelection()
        {
            if (Children == null) return;
            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not UICallbackButton b) continue;
                Debug.Assert(!Children[i].InputTransparent);
                b.OnClickedProxy = ProxyButtonClicked;
                b.OnMouseEnterProxy = ProxyButtonSelected;
            }
        }

        private void ProxyButtonClicked(UIBaseWindow b)
        {
            if (!b.Visible) return;
            SetSelection(b);
            Debug.Assert(SelectedWnd != null);
            OnChoiceConfirmed?.Invoke(SelectedWnd, SelectedChildIdx);
        }

        private void ProxyButtonSelected(UIBaseWindow b)
        {
            if (!b.Visible) return;
            SetSelection(b);
        }

        public void SetScrollbar(UIScrollbar scrollBar)
        {
            _scrollBar = scrollBar;
            SyncScrollbar();
            _scrollBar.OnValueChanged += ScrollbarScrolled;
        }

        private void ScrollbarScrolled(int newValue)
        {
            // Todo: Horizontal list
            if (_scrollPos.Y == newValue) return;
            _scrollPos.Y = newValue;
            ScrollToPos(_scrollPos);
        }

        public void SyncScrollbar()
        {
            // Todo: Horizontal list implementation
            if (_scrollBar == null) return;
            _scrollBar.MinValue = 0;
            _scrollBar.MaxValue = (int) _lastScrollChildPos.Y;
            _scrollBar.Value = (int) _scrollPos.Y;
        }

        public override UIBaseWindow? FindMouseInput(Vector2 pos)
        {
            UIBaseWindow? focus = this;
            if (Children != null && _firstVisibleChild != -1 && _lastVisibleChild != -1)
                for (int i = _firstVisibleChild; i <= _lastVisibleChild; i++)
                {
                    UIBaseWindow win = Children[i];
                    if (!win.InputTransparent && win.Visible && win.IsPointInside(pos))
                    {
                        focus = win.FindMouseInput(pos);
                    }
                }

            if (focus == this && _scrollBar != null && _scrollBar.IsPointInside(pos)) return _scrollBar;
            return focus;
        }
    }
}