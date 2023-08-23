#nullable enable

#region Using

using Emotion.Editor.PropertyEditors;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers;

public interface IEditorCheckboxListItem
{
	public bool Checked { get; set; }
	public string Name { get; set; }
}

public class EditorCheckboxList : UIBaseWindow
{
	public string Text
	{
		get => _label;
		set
		{
			_label = value;
			if (_button != null) _button.Text = value;
		}
	}

	private string _label;
	private EditorButton? _button;
	private IEditorCheckboxListItem[]? _items;

	public EditorCheckboxList(string label)
	{
		_label = label;
		StretchX = true;
		StretchY = true;
	}

	public void SetItems(IEditorCheckboxListItem[]? items)
	{
		_items = items;
		if(_button != null) _button.Enabled = _items != null;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var button = new EditorButton();
		button.Text = _label;
		button.MinSize = new Vector2(20, 0);
		button.IgnoreParentColor = true;
		button.Enabled = _items != null;
		button.OnClickedProxy = click =>
		{
			if (_items == null) return;

			var dropDown = new EditorDropdown();
			dropDown.Offset = button.RenderBounds.BottomLeft / button.GetScale();

			var editors = new UIBaseWindow[_items.Length];
			for (var i = 0; i < _items.Length; i++)
			{
				IEditorCheckboxListItem item = _items[i];

				var checkMark = new PropEditorBool();
				checkMark.SetValue(item.Checked);
				checkMark.SetCallbackValueChanged((newVal) =>
				{
					item.Checked = (bool) newVal;
				});
				checkMark.ZOffset = -1;
				var editor = new FieldEditorWithLabel(item.Name, checkMark);
				editor.Margins = Rectangle.Empty;
				editors[i] = editor;
			}

			dropDown.SetChildren(editors);
			Controller!.AddChild(dropDown);
		};
		button.StretchY = true;
		button.LayoutMode = LayoutMode.HorizontalList;
		_button = button;

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