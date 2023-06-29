#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.EditorHelpers;

public class EditorButtonDropDown : UIBaseWindow
{
	public string? Text
	{
		get => _text;
		set
		{
			_text = value;
			var label = (MapEditorLabel?) GetWindowById("Label");
			if (label != null) label.Text = _text;
		}
	}

	private string? _text;

	private EditorDropDownButtonDescription? _currentOption;
	private EditorDropDownButtonDescription[]? _items;

	public EditorButtonDropDown()
	{
		InputTransparent = false;
		StretchX = true;
		StretchY = true;
		LayoutMode = LayoutMode.HorizontalList;
	}

	public void SetItems(EditorDropDownButtonDescription[] items, int current)
	{
		_items = items;
		_currentOption = items[current];
		UpdateCurrentOptionText();
	}

	private void UpdateCurrentOptionText()
	{
		var button = (MapEditorTopBarButton?) GetWindowById("Button");
		if (button == null || _currentOption == null) return;
		button.Text = _currentOption.Name;
		button.Enabled = _items != null && _items.Length > 1;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var label = new MapEditorLabel(_text);
		label.Id = "Label";
		AddChild(label);

		var button = new MapEditorTopBarButton();
		button.Text = _currentOption?.Name ?? "null";
		button.MinSize = new Vector2(20, 0);
		button.IgnoreParentColor = true;
		button.Id = "Button";
		button.Enabled = _items != null && _items.Length > 1;
		button.OnClickedProxy = click =>
		{
			var dropDown = new MapEditorDropdown(true);
			dropDown.Offset = button.RenderBounds.BottomLeft / button.GetScale();

			dropDown.SetItems(_items, selected =>
			{
				_currentOption = selected;
				UpdateCurrentOptionText();
			});
			Controller!.AddChild(dropDown);
		};
		button.StretchY = true;
		button.LayoutMode = LayoutMode.HorizontalList;

		var arrowImage = new UITexture();
		arrowImage.TextureFile = "Editor/LittleArrow.png";
		arrowImage.ImageScale = new Vector2(0.2f);
		arrowImage.Anchor = UIAnchor.CenterRight;
		arrowImage.ParentAnchor = UIAnchor.CenterRight;
		arrowImage.Margins = new Rectangle(3, 0, 0, 0);
		button.AddChild(arrowImage);

		AddChild(button);
	}
}