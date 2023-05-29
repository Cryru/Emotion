#region Using

using Emotion.Common.Serialization;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorDropdown : UIDropDown
	{
		public UICallbackListNavigator List;

		[DontSerialize]
		public Action OnCloseProxy;

		public MapEditorDropdown()
		{
			InputTransparent = false;
			WindowColor = MapEditorColorPalette.ActiveButtonColor;
			StretchX = true;
			StretchY = true;
			Offset = new Vector2(-5, 1);

			var innerBg = new UISolidColor();
			innerBg.IgnoreParentColor = true;
			innerBg.InputTransparent = false;
			innerBg.WindowColor = MapEditorColorPalette.BarColor.SetAlpha(255);
			innerBg.StretchX = true;
			innerBg.StretchY = true;
			innerBg.Paddings = new Rectangle(3, 3, 3, 3);

			AddChild(innerBg);

			var list = new UICallbackListNavigator();
			list.IgnoreParentColor = true;
			list.LayoutMode = LayoutMode.VerticalList;
			list.InputTransparent = false;
			list.StretchX = true;
			list.StretchY = true;
			list.ChildrenAllSameWidth = true;
			list.ListSpacing = new Vector2(0, 2);

			innerBg.AddChild(list);
			List = list;
		}

		public void SetItems(EditorDropDownButtonDescription[] items)
		{
			List.ClearChildren();

			for (var i = 0; i < items.Length; i++)
			{
				EditorDropDownButtonDescription buttonMeta = items[i];

				var ddButton = new MapEditorTopBarButton();
				ddButton.StretchX = true;
				ddButton.StretchY = true;
				ddButton.InputTransparent = false;
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

					buttonMeta.Click(ddButton);
				};
				ddButton.Enabled = buttonMeta.Enabled?.Invoke() ?? true;

				List.AddChild(ddButton);
			}
		}

		public override void DetachedFromController(UIController controller)
		{
			base.DetachedFromController(controller);
			OnCloseProxy?.Invoke();
		}
	}
}