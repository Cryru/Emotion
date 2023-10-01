#nullable enable

#region Using

using Emotion.Common.Serialization;
using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

public class EditorDropdown : UIDropDown
{
	public bool CloseOnClick;

	public UICallbackListNavigator List;

	[DontSerialize] public Action? OnCloseProxy;

	public EditorDropdown(bool closeOnClick = false)
	{
		CloseOnClick = closeOnClick;
		WindowColor = MapEditorColorPalette.ActiveButtonColor;
#if !NEW_UI
		StretchX = true;
		StretchY = true;
#endif
		Offset = new Vector2(-2, 1);

		var innerBg = new UISolidColor
		{
			IgnoreParentColor = true,
			WindowColor = MapEditorColorPalette.BarColor.SetAlpha(255),
#if !NEW_UI
			StretchX = true,
			StretchY = true,
#endif
			Paddings = new Rectangle(3, 3, 3, 3),
		};

		AddChild(innerBg);

		var list = new UICallbackListNavigator
		{
			IgnoreParentColor = true,
			LayoutMode = LayoutMode.VerticalList,
#if !NEW_UI
			StretchX = true,
			ChildrenAllSameWidth = true,
			Margins = new Rectangle(0, 0, 8, 0),
#else

#endif
			ListSpacing = new Vector2(0, 2),
			
			MaxSizeY = 100,
			HideScrollBarWhenNothingToScroll = true,
		};

		var scrollBar = new EditorScrollBar();
		scrollBar.MaxSizeY = 90;
		list.SetScrollbar(scrollBar);
		innerBg.AddChild(scrollBar);

		innerBg.AddChild(list);
		List = list;
	}

	protected override void AfterRenderChildren(RenderComposer c)
	{
		base.AfterRenderChildren(c);
		c.RenderOutline(Position, Size, WindowColor);
	}

	public void SetItems(EditorDropDownButtonDescription[]? items, Action<EditorDropDownButtonDescription>? selectedCallback = null)
	{
		List.ClearChildren();

		if (items == null) return;

		List<PropEditorBool>? checkBoxes = null;
		for (var i = 0; i < items.Length; i++)
		{
			EditorDropDownButtonDescription item = items[i];
			if (item is EditorCheckboxListItem itemCheckBox)
			{
				checkBoxes ??= new List<PropEditorBool>();

				var checkMark = new PropEditorBool();
				checkMark.SetValue(itemCheckBox.Checked());
				checkMark.SetCallbackValueChanged(newVal => {
					if (item.Click == null) return;
					if (item.Enabled != null)
					{
						bool enabled = item.Enabled();
						if (!enabled) return;
					}

					selectedCallback?.Invoke(itemCheckBox);
					item.Click(item, null);

					AssertNotNull(checkBoxes);
					for (int c = 0; c < checkBoxes.Count; c++)
					{
						var checkBox = checkBoxes[c];
						var thatCheckBoxItem = items[c];
						var thatCheckBoxItemAsCheckBoxItem = thatCheckBoxItem as EditorCheckboxListItem;
						checkBox.SetValue(thatCheckBoxItemAsCheckBoxItem?.Checked() ?? false);
					}
				});
				checkMark.ZOffset = -1;

				checkBoxes.Add(checkMark);

				var editor = new FieldEditorWithLabel(itemCheckBox.Name, checkMark);
				editor.Margins = Rectangle.Empty;
				List.AddChild(editor);
			}
			else
			{
				var ddButton = new EditorButton
				{
#if !NEW_UI
					StretchX = true,
					StretchY = true,

					MinSize = new Vector2(50, 0),
#else
				FillXInList = true,
#endif
					Text = item.Name,
				};
				ddButton.OnClickedProxy = _ =>
				{
					if (item.Click == null) return;
					if (item.Enabled != null)
					{
						bool enabled = item.Enabled();
						if (!enabled) return;
					}

					selectedCallback?.Invoke(item);
					item.Click(item, ddButton);

					if (CloseOnClick) Controller?.RemoveChild(this);
				};
				ddButton.Enabled = item.Enabled?.Invoke() ?? true;

				List.AddChild(ddButton);
			}
		}
	}

	public void SetChildren(UIBaseWindow[] items)
	{
		List.ClearChildren();

		for (var i = 0; i < items.Length; i++)
		{
			UIBaseWindow item = items[i];
#if !NEW_UI
			item.StretchX = true;
			item.StretchY = true;
			item.MinSize = new Vector2(50, 0);
#endif
			List.AddChild(item);
		}
	}

	public override void DetachedFromController(UIController controller)
	{
		base.DetachedFromController(controller);
		OnCloseProxy?.Invoke();
	}
}