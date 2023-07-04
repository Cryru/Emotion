#region Using

using Emotion.Common.Serialization;
using Emotion.Editor.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorDropdown : UIDropDown
	{
		public bool CloseOnClick;

		public UICallbackListNavigator List;

		[DontSerialize] public Action OnCloseProxy;

		public MapEditorDropdown(bool closeOnClick = false)
		{
			CloseOnClick = closeOnClick;
			WindowColor = MapEditorColorPalette.ActiveButtonColor;
			StretchX = true;
			StretchY = true;
			Offset = new Vector2(-5, 1);
			Paddings = new Rectangle(1, 1, 1, 1);

			var innerBg = new UISolidColor
			{
				IgnoreParentColor = true,
				WindowColor = MapEditorColorPalette.BarColor.SetAlpha(255),
				StretchX = true,
				StretchY = true,
				Paddings = new Rectangle(3, 3, 3, 3),
			};

			AddChild(innerBg);

			var list = new UICallbackListNavigator
			{
				IgnoreParentColor = true,
				LayoutMode = LayoutMode.VerticalList,
				StretchX = true,
				ChildrenAllSameWidth = true,
				ListSpacing = new Vector2(0, 2),
				Margins = new Rectangle(0, 0, 8, 0),
				MaxSizeY = 100,
				HideScrollBarWhenNothingToScroll = true
			};

			var scrollBar = new EditorScrollBar();
			scrollBar.MaxSizeY = 90;
			list.SetScrollbar(scrollBar);
			innerBg.AddChild(scrollBar);

			innerBg.AddChild(list);
			List = list;
		}

		protected override bool RenderInternal(RenderComposer c)
		{
			c.RenderOutline(Bounds, WindowColor);
			return base.RenderInternal(c);
		}

		public void SetItems(EditorDropDownButtonDescription[] items, Action<EditorDropDownButtonDescription> selectedCallback = null)
		{
			List.ClearChildren();

			for (var i = 0; i < items.Length; i++)
			{
				EditorDropDownButtonDescription buttonMeta = items[i];

				var ddButton = new MapEditorTopBarButton();
				ddButton.StretchX = true;
				ddButton.StretchY = true;
				ddButton.Text = buttonMeta.Name;
				ddButton.MinSize = new Vector2(50, 0);
				ddButton.OnClickedProxy = _ =>
				{
					if (buttonMeta.Click == null) return;
					if (buttonMeta.Enabled != null)
					{
						bool enabled = buttonMeta.Enabled();
						if (!enabled) return;
					}

					selectedCallback?.Invoke(buttonMeta);
					buttonMeta.Click(buttonMeta, ddButton);

					if (CloseOnClick) Controller?.RemoveChild(this);
				};
				ddButton.Enabled = buttonMeta.Enabled?.Invoke() ?? true;

				List.AddChild(ddButton);
			}
		}

		public void SetChildren(UIBaseWindow[] items)
		{
			List.ClearChildren();

			for (var i = 0; i < items.Length; i++)
			{
				UIBaseWindow item = items[i];
				item.StretchX = true;
				item.StretchY = true;
				item.MinSize = new Vector2(50, 0);
				List.AddChild(item);
			}
		}

		public override void DetachedFromController(UIController controller)
		{
			base.DetachedFromController(controller);
			OnCloseProxy?.Invoke();
		}
	}
}