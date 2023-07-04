#region Using

using Emotion.Editor.PropertyEditors;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.EditorHelpers
{
	public class PropEditorNumber<T> : UIBaseWindow, IPropEditorGeneric where T : INumber<T>
	{
		public XMLFieldHandler? Field { get; set; }

		private T _value = T.Zero;
		private Action<object>? _callback;
		private UITextInput? _textInput;

		public PropEditorNumber()
		{
			StretchX = true;
			StretchY = true;
		}

		public void SetValue(object value)
		{
			_value = (T) (value ?? T.Zero);
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
			textEditor.MinSize = new Vector2(25, 0);
			textEditor.IgnoreParentColor = true;
			textEditor.Id = "textEditor";
			textEditor.OnSubmit = val =>
			{
				if (!T.TryParse(val, null, out T? intVal)) return;

				if (intVal == _value) return;
				_value = intVal;
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