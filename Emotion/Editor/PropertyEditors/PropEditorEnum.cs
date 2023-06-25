#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
	public class PropEditorEnum : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler? Field { get; set; }

		private Type _enumType;
		private object? _value;

		private string[] _enumValueNames;

		private Action<object>? _callback;
		private MapEditorTopBarButton? _button;

		public PropEditorEnum(Type enumType)
		{
			InputTransparent = false;
			StretchX = true;
			StretchY = true;

			_enumType = enumType;
			_enumValueNames = Enum.GetNames(enumType);
		}

		public void SetValue(object value)
		{
			_value = value;
			if (_button != null) _button.Text = _value?.ToString() ?? "null";
		}

		public object GetValue()
		{
			return _value!;
		}

		public void SetCallbackValueChanged(Action<object> callback)
		{
			_callback = callback;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var textEditor = new MapEditorTopBarButton();
			textEditor.Text = (_value?.ToString() ?? "null");
			textEditor.MinSize = new Vector2(20, 0);
			textEditor.IgnoreParentColor = true;
			textEditor.Id = "textEditor";
			textEditor.OnClickedProxy = (click) =>
			{
				var dropDown = new MapEditorDropdown();
				dropDown.Offset = textEditor.RenderBounds.BottomLeft / textEditor.GetScale();

				var dropDownItems = new EditorDropDownButtonDescription[_enumValueNames.Length];

				for (int i = 0; i < _enumValueNames.Length; i++)
				{
					string enumValName = _enumValueNames[i];
					object enumVal = Enum.Parse(_enumType, enumValName);

					dropDownItems[i] = new EditorDropDownButtonDescription
					{
						Name = enumValName,
						Click = _ =>
						{
							SetValue(enumVal);
							_callback?.Invoke(enumVal);
						},
						Enabled = () => enumVal != _value
					};
				}

				dropDown.SetItems(dropDownItems);
				Controller!.AddChild(dropDown);
			};
			textEditor.StretchY = true;
			textEditor.LayoutMode = LayoutMode.HorizontalList;
			_button = textEditor;

			var arrowImage = new UITexture();
			arrowImage.TextureFile = "Editor/LittleArrow.png";
			arrowImage.ImageScale = new Vector2(0.2f);
			arrowImage.Anchor = UIAnchor.CenterRight;
			arrowImage.ParentAnchor = UIAnchor.CenterRight;
			arrowImage.Margins = new Rectangle(3, 0, 0, 0);
			textEditor.AddChild(arrowImage);

			AddChild(textEditor);
		}
	}
}