#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class EditorListOfItemsPanel<T> : MapEditorPanel where T : class
	{
		public bool CloseOnClick { get; set; }

		public string Text
		{
			set
			{
				_text = value;

				var introText = (UIText) GetWindowById("IntroText");
				if (introText != null)
				{
					introText.Text = value;
					introText.Visible = !string.IsNullOrEmpty(_text);
				}
			}
		}

		private string _text;

		private IEnumerable<T> _items;
		private Action<T> _onClick;
		private Action<T> _onRollover;

		private T _rollover;

		public EditorListOfItemsPanel(string header, IEnumerable<T> items, Action<T> onClick, Action<T> onRollover = null) : base(header)
		{
			_items = items;
			_onClick = onClick;
			_onRollover = onRollover;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			UIBaseWindow contentWin = _contentParent;
			contentWin.InputTransparent = false;

			var innerContainer = new UIBaseWindow();
			innerContainer.StretchX = true;
			innerContainer.StretchY = true;
			innerContainer.InputTransparent = false;
			innerContainer.LayoutMode = LayoutMode.VerticalList;
			innerContainer.ListSpacing = new Vector2(0, 3);
			innerContainer.ChildrenAllSameWidth = true;
			contentWin.AddChild(innerContainer);

			var txt = new UIText();
			txt.ScaleMode = UIScaleMode.FloatScale;
			txt.WindowColor = MapEditorColorPalette.TextColor;
			txt.Id = "IntroText";
			txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
			txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			txt.IgnoreParentColor = true;
			txt.Text = _text;
			txt.DontTakeSpaceWhenHidden = true;
			txt.Visible = !string.IsNullOrEmpty(_text);
			innerContainer.AddChild(txt);

			var listContainer = new UIBaseWindow();
			listContainer.StretchX = true;
			listContainer.StretchY = true;
			listContainer.InputTransparent = false;
			listContainer.LayoutMode = LayoutMode.HorizontalList;
			innerContainer.AddChild(listContainer);

			var listNav = new UICallbackListNavigator();
			listNav.LayoutMode = LayoutMode.VerticalList;
			listNav.StretchX = true;
			listNav.ListSpacing = new Vector2(0, 1);
			listNav.Margins = new Rectangle(0, 0, 5, 0);
			listNav.MinSize = new Vector2(100, 100);
			listNav.InputTransparent = false;
			listNav.ChildrenAllSameWidth = true;
			listContainer.AddChild(listNav);

			var scrollBar = new EditorScrollBar();
			listNav.SetScrollbar(scrollBar);
			listContainer.AddChild(scrollBar);

			foreach (T item in _items)
			{
				var itemButton = new MapEditorTopBarButton();
				itemButton.Text = item.ToString();
				itemButton.Anchor = UIAnchor.TopCenter;
				itemButton.ParentAnchor = UIAnchor.TopCenter;
				itemButton.StretchY = true;
				itemButton.OnClickedProxy = _ =>
				{
					_onClick?.Invoke(item);
					if (CloseOnClick) Close();
				};
				itemButton.OnMouseEnterProxy = _ => { _onRollover?.Invoke(item); };
				itemButton.OnMouseLeaveProxy = _ => { _onRollover?.Invoke(null); };
				listNav.AddChild(itemButton);
			}
		}

		public override void DetachedFromController(UIController controller)
		{
			if (_rollover != null)
			{
				_onRollover?.Invoke(null);
				_rollover = null;
			}

			base.DetachedFromController(controller);
		}
	}
}