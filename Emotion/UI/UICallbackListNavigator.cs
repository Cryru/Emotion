#region Using

using System.Diagnostics;
using Emotion.Common.Serialization;
using Emotion.Graphics;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI
{
	/// <summary>
	/// Provides list navigation to UICallbackButton children.
	/// </summary>
	[DontSerializeMembers("Paddings")]
	public class UICallbackListNavigator : UIBaseWindow
	{
		/// <summary>
		/// If enabled all children windows outside the bounds of the list will not be rendered.
		/// On by default, and needed to support scrolling.
		/// </summary>
		public bool HideOutsideChildren = true;

		/// <summary>
		/// Scrolls the children in a way that ensures that at least one item before and after the selected item is visible.
		/// </summary>
		public bool ScrollOneAhead = true;

		/// <summary>
		/// Whether to hide the scrollbar window when all children fit within the list and there is no scrolling.
		/// </summary>
		public bool HideScrollBarWhenNothingToScroll = false;

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
		private Vector2 _gridStart;
		private Vector2 _gridSize;
		private int _lastRowColumn;

		private Rectangle _scrollArea; // The total area of all children that is being scrolled through.
		private Vector2 _scrollPos = Vector2.Zero; // The grid-like-pos of the current child in view.
		private Vector2 _lastScrollChildPos; // The grid-like-pos of the last child that can be scrolled to.
		private Matrix4x4 _scrollDisplacement = Matrix4x4.Identity; // The current scroll translation.
		private Vector2 _fullSize; // The full size the scroll area was given.

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
			Vector2 measuredSize = _measuredSize;

			// Make area as big as the children shown. Might look weird if all children are not the same size.
			// This will also tell us how big the page size is.
			float spaceTaken = 0;
			if (Children != null)
			{
				Vector2 scaledListSpacing = (ListSpacing * GetScale()).RoundClosest();
				for (var i = 0; i < Children.Count; i++)
				{
					UIBaseWindow child = Children[i];
					if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

					float childSize = child.Height;
					if (spaceTaken != 0) childSize += scaledListSpacing.Y;
					if (spaceTaken + childSize > measuredSize.Y) break;
					spaceTaken += childSize;
				}
			}

			spaceTaken = MathF.Max(spaceTaken, MathF.Ceiling(MinSize.Y * GetScale()));
			_fullSize = _measuredSize;
			_measuredSize.Y = spaceTaken;

			_scrollArea.Size = usedSpace.Round();
			base.AfterMeasureChildren(usedSpace);
		}

		protected override void AfterLayout()
		{
			_gridPosToChild.Clear();
			_gridStart = Vector2.Zero;
			_gridSize = Vector2.Zero;

			var pen = new Vector2();
			for (var i = 0; i < Children?.Count; i++)
			{
				UIBaseWindow child = Children[i];
				if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

				_gridPosToChild.Add(pen, child);

				_gridStart.X = MathF.Min(pen.X, _gridStart.X);
				_gridStart.Y = MathF.Min(pen.Y, _gridStart.Y);

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

			// Add position to scroll rect.
			_scrollArea.X += X;
			_scrollArea.Y += Y;

			// Calculate last child that can be scrolled to.
			// This is the highest/leftmost child that allows the last child to be visible.
			float diff = 0, visibleAtOnce = 0;
			_lastScrollChildPos = Vector2.Zero;
			var isHorizontal = false;
			if (Children != null)
				for (int i = Children.Count - 1; i >= 0; i--)
				{
					UIBaseWindow child = Children[i];
					if (!child.Visible && child.DontTakeSpaceWhenHidden) continue;

					switch (LayoutMode)
					{
						case LayoutMode.HorizontalListWrap:
						case LayoutMode.HorizontalList:
							visibleAtOnce = Width;
							diff = _scrollArea.X + _scrollArea.Width - child.X;
							isHorizontal = true;
							break;
						case LayoutMode.VerticalListWrap:
						case LayoutMode.VerticalList:
							visibleAtOnce = Height;
							diff = _scrollArea.Y + _scrollArea.Height - child.Y;
							break;
					}

					if (diff > visibleAtOnce && i != Children.Count - 1)
					{
						UIBaseWindow lastVisChild = Children[i + 1];
						_lastScrollChildPos = GetGridLikePosFromChild(lastVisChild);

						// Sometimes when children are different sizes there will be a small margin at
						// the end of the scrollbar due to having to scroll an entire child to display<1 child.
						// We want this value to be a part of the _scrollArea size in order to guarantee its accuracy.
						if (isHorizontal)
						{
							float chX = lastVisChild.X;
							float lastChildScrolledOffset = -(_scrollArea.X - chX) + Width;
							_scrollArea.Width = lastChildScrolledOffset;
						}
						else
						{
							float chY = lastVisChild.Y;
							float lastChildScrolledOffset = -(_scrollArea.Y - chY) + Height;
							_scrollArea.Height = lastChildScrolledOffset;
						}

						break;
					}
				}

			// Restore or reset scroll.
			if (!ScrollToPos(_scrollPos) && !ScrollToPos(Vector2.Zero))
			{
				// No children
				_scrollDisplacement = Matrix4x4.Identity;
				SyncScrollbar();
			}

			base.AfterLayout();
		}

		protected override void RenderChildren(RenderComposer c)
		{
			Rectangle renderRect = _renderBounds;
			// c.RenderOutline(renderRect, Color.Red);

			c.PushModelMatrix(_scrollDisplacement);
			// c.RenderOutline(_scrollArea, Color.Red);
			for (var i = 0; i < Children!.Count; i++)
			{
				UIBaseWindow child = Children[i];
				child.EnsureRenderBoundsCached(c);

				if (!child.Visible) continue;
				if (!child.IsInsideOrIntersectRect(renderRect, out bool inside) && HideOutsideChildren) continue;

				if (inside)
				{
					child.Render(c);
				}
				else
				{
					Rectangle? clip = c.CurrentState.ClipRect;
					c.SetClipRect(renderRect);
					child.Render(c);
					c.SetClipRect(clip);
				}
			}

			c.PopModelMatrix();
		}

		public bool ScrollByAbsolutePos(float pos)
		{
			if (Children == null) return false;

			// todo: horizontal list
			pos += _scrollArea.Y;
			for (var i = 0; i < Children.Count; i++)
			{
				UIBaseWindow child = Children[i];
				if (child.Y < pos && child.Y + child.Height > pos) return ScrollToPos(GetGridLikePosFromChild(child));
			}

			return false;
		}

		public bool ScrollToPos(Vector2 gridLikePos)
		{
			gridLikePos = Vector2.Min(gridLikePos, _lastScrollChildPos);
			gridLikePos = Vector2.Max(gridLikePos, Vector2.Zero);
			UIBaseWindow? child = GetChildByGridLikePos(gridLikePos, out int _, true);
			if (child == null) return false;
			_scrollPos = gridLikePos;

			_scrollDisplacement = Matrix4x4.CreateTranslation(
				_scrollArea.X - child.X + child.Margins.X * child.GetScale(),
				_scrollArea.Y - child.Y + child.Margins.Y * child.GetScale(), 0);
			SyncScrollbar();

			// Code to keep selection in view (by moving the selection) idk it's kinda jank
			//if (SelectedWnd != null)
			//{
			//    UIBaseWindow? selChild = GetChildByGridLikePos(_selectedPos, out int _, false);
			//    if (selChild == null || !selChild.IsInsideRect(_renderBounds))
			//    {
			//        SetSelection(child);
			//    }
			//}

			return true;
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
				while ((newItem == null || !newItem.Visible || newItem.InputTransparent) && childIdx != Children.Count - 1) // Go next until visible.
				{
					_selectedPos += axis;
					_selectedPos = Vector2.Clamp(_selectedPos, Vector2.Zero, _gridSize);
					if (_selectedPos.Y == _gridSize.Y && _selectedPos.X > _lastRowColumn && _lastRowColumn != -1) _selectedPos.X = _lastRowColumn;
					newItem = GetChildByGridLikePos(_selectedPos, out childIdx, true);
				}

				if (newItem == null || !newItem.Visible) return true; // Reached end.
				Debug.Assert(childIdx != -1);

				Vector2 offset = Vector2.Zero;
				if (ScrollOneAhead) offset = new Vector2(1);

				// Scroll new selection into view.
				Vector2 diff = axis;
				offset *= diff;
				UIBaseWindow? child = GetChildByGridLikePos(_selectedPos + offset, out int _, true);
				if (child != null && !child.IsInsideRect(_renderBounds))
				{
					Vector2 newPos = _scrollPos + diff;
					ScrollToPos(newPos);
				}

				SetSelection(newItem);
				//UIBaseWindow? oldSel = SelectedWnd;
				//SelectedWnd = newItem;
				//SelectedChildIdx = childIdx;
				//if (newItem != oldSel) OnItemSelected?.Invoke(newItem, oldSel);
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
			if (Children == null) return;

			if (up)
				ScrollToPos(_scrollPos - new Vector2(0, 1));
			else
				ScrollToPos(_scrollPos + new Vector2(0, 1));

			// If scrolling invalidate the mouse cache as something else will scroll under.
			_lastMousePos = Vector2.Zero;
			_renderBoundsCalculatedFrom = Rectangle.Empty;
			_renderBounds = Rectangle.Empty;

			// Debug code to check if all windows are the same distance from each other.
			//UIBaseWindow? lastChild = null;
			//foreach (KeyValuePair<Vector2, UIBaseWindow> child in _gridPosToChild)
			//{
			//    if (lastChild != null)
			//    {
			//        float diff = lastChild.Y - child.Value.Y;
			//        Console.WriteLine(child.Key + " " + diff);
			//    }
			//    lastChild = child.Value;
			//}
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
				UIBaseWindow child = Children[i];
				bool enabled = child.Visible && !child.InputTransparent;
				if (enabled)
				{
					SetSelection(child, true);
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
				b.OnClickedProxy = ProxyButtonClicked;
				b.OnMouseEnterProxy = ProxyButtonSelected;
			}
		}

		private void ProxyButtonClicked(UICallbackButton b)
		{
			if (!b.Visible || b.InputTransparent) return;
			SetSelection(b);
			Debug.Assert(SelectedWnd != null);
			OnChoiceConfirmed?.Invoke(SelectedWnd, SelectedChildIdx);
		}

		private void ProxyButtonSelected(UICallbackButton b)
		{
			if (!b.Visible || b.InputTransparent) return;
			SetSelection(b);
		}

		public void SetScrollbar(UIScrollbar scrollBar)
		{
			_scrollBar = scrollBar;
			SyncScrollbar();
			_scrollBar.ScrollParent = this;
		}

		public void SyncScrollbar()
		{
			// Todo: Horizontal list implementation
			if (_scrollBar == null) return;
			_scrollBar.TotalArea = _scrollArea.Height;
			_scrollBar.PageArea = Height;
			UIBaseWindow? child = GetChildByGridLikePos(_scrollPos, out int _, false);
			_scrollBar.Current = child?.Y - _scrollArea.Y ?? 0;

			if (HideScrollBarWhenNothingToScroll)
				_scrollBar.Visible = Visible && _scrollArea.Height != Height;
			else
				_scrollBar.Visible = Visible;

			_scrollBar.UpdateScrollbar();
		}

		// Ensure that scrolling via shortcuts doesn't trigger mouse change focus.
		// It is very annoying when the cursor is on top of the list.
		private Vector2 _lastMousePos;
		private UIBaseWindow? _lastResult;

		public override UIBaseWindow? FindMouseInput(Vector2 pos)
		{
			if (pos == _lastMousePos) return _lastResult;

			UIBaseWindow? focus = this;
			if (Children != null)
			{
				Rectangle renderRect = _renderBounds;
				for (var i = 0; i < Children.Count; i++)
				{
					UIBaseWindow win = Children[i];
					if (win.InputTransparent || !win.Visible) continue;
					if (!win.IsInsideOrIntersectRect(renderRect, out _)) continue;
					if (!win.IsPointInside(pos)) continue;
					focus = win.FindMouseInput(pos);
				}
			}

			// Cache mouse target only if calculation is done.
			// It is done so that moving the selection via buttons isn't overriden by the mouse.
			if (_renderBoundsCalculatedFrom != Rectangle.Empty)
			{
				_lastMousePos = pos;
				_lastResult = focus;
			}

			if (focus == this && _scrollBar != null && _scrollBar.IsPointInside(pos)) return _scrollBar;
			return focus;
		}
	}
}