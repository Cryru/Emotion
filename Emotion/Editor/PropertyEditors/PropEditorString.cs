#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors
{
    public class PropEditorString : UIBaseWindow, IPropEditorGeneric
	{
		public XMLFieldHandler Field { get; set; } = null!;

		public string Text
		{
			get => (string) GetValue();
			set => SetValue(value);
		}

		private string _value = "";
		private Action<object>? _callback;
		private UITextInput? _textInput;

		private bool _updateOnTextChange;
		private string _updateOnTextChangeLastText;

		public PropEditorString(bool updateOnTextChange = false)
		{
			StretchX = true;
			StretchY = true;
			MinSize = new Vector2(70, 0);

			_updateOnTextChange = updateOnTextChange;
		}

		public void SetValue(object value)
		{
			_value = (string) value;
			if (_textInput != null) _textInput.Text = _value;
			_updateOnTextChangeLastText = _value;
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
			textEditor.FontFile = "Editor/UbuntuMono-Regular.ttf";
			textEditor.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			textEditor.SizeOfText = true;
			textEditor.MinSize = new Vector2(20, 0);
			textEditor.MinSize = new Vector2(70, 0);
			textEditor.IgnoreParentColor = true;
			textEditor.Id = "textEditor";
			textEditor.OnSubmit = val =>
			{
				if (val == _value) return;
				_value = val;
				_updateOnTextChangeLastText = val;
				_callback?.Invoke(_value);
			};
			textEditor.SubmitOnEnter = true;
			textEditor.SubmitOnFocusLoss = true;
			inputBg.AddChild(textEditor);
			_textInput = textEditor;
			SetValue(_value); // To force setting it in the UI

			AddChild(inputBg);
		}

		protected override bool UpdateInternal()
		{
			if (_updateOnTextChange && _textInput != null && _textInput.Text != _updateOnTextChangeLastText) _textInput.OnSubmit?.Invoke(_textInput.Text);

			return base.UpdateInternal();
		}

		public static UIBaseWindow CreateStringEditorWithLabel(string label, bool updateOnTextChange, out PropEditorString stringEditor)
		{
			var container = new UIBaseWindow();
			container.LayoutMode = LayoutMode.HorizontalList;
			container.ListSpacing = new Vector2(5, 0);
			container.StretchX = true;
			container.StretchY = true;

			var labelWnd = new MapEditorLabel(label);
			container.AddChild(labelWnd);

			var stringEditorWnd = new PropEditorString(updateOnTextChange);
			container.AddChild(stringEditorWnd);
			stringEditor = stringEditorWnd;

			return container;
		}
	}
}