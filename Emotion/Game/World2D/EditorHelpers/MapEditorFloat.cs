#region Using

using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorFloat : UIBaseWindow, IMapEditorGeneric
	{
		private float _value;
		private Action<object>? _callback;
		private UITextInput? _textInput;

		public MapEditorFloat()
		{
			LayoutMode = LayoutMode.HorizontalList;
			ListSpacing = new Vector2(2, 0);
			InputTransparent = false;
			StretchX = true;
			StretchY = true;
		}

		public void SetValue(object value)
		{
			_value = (float) value;
			if (_textInput != null) _textInput.Text = _value.ToString();
		}

		public object GetValue()
		{
			return _value;
		}

		public void SetCallbackValueChanged(Action<object> callback)
		{
			_callback = callback;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var inputBg = new UISolidColor();
			inputBg.InputTransparent = false;
			inputBg.StretchX = true;
			inputBg.StretchY = true;
			inputBg.WindowColor = MapEditorColorPalette.ButtonColor;
			inputBg.Paddings = new Rectangle(2, 1, 2, 1);
			inputBg.Anchor = UIAnchor.CenterLeft;
			inputBg.ParentAnchor = UIAnchor.CenterLeft;

			var textEditor = new UITextInput();
			textEditor.Text = _value.ToString();
			textEditor.FontFile = "Editor/UbuntuMono-Regular.ttf";
			textEditor.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			textEditor.SizeOfText = true;
			textEditor.MinSize = new Vector2(20, 0);
			textEditor.IgnoreParentColor = true;
			textEditor.Id = "textEditor";
			textEditor.OnSubmit = val =>
			{
				if (!float.TryParse(val, out float floatVal)) return;

				if (floatVal == _value) return;
				_value = floatVal;
				_callback?.Invoke(_value);
			};
			textEditor.SubmitOnEnter = true;
			textEditor.SubmitOnFocusLoss = true;
			inputBg.AddChild(textEditor);
			_textInput = textEditor;

			AddChild(inputBg);
		}
	}
}