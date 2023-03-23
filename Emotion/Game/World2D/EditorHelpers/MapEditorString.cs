#region Using

using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorString : UIBaseWindow, IMapEditorGeneric
	{
		public XMLFieldHandler Field { get; set; }

		public string Text
		{
			get => (string) GetValue();
			set => SetValue(value);
		}

		private string _value;
		private Action<object>? _callback;
		private UITextInput? _textInput;

		public MapEditorString()
		{
			InputTransparent = false;
			StretchX = true;
			StretchY = true;
			MinSize = new Vector2(70, 0);
		}

		public void SetValue(object value)
		{
			_value = (string) value;
			if (_textInput != null) _textInput.Text = _value;
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
			textEditor.Text = _value;
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
				_callback?.Invoke(_value);
			};
			textEditor.SubmitOnEnter = true;
			textEditor.SubmitOnFocusLoss = true;
			inputBg.AddChild(textEditor);
			_textInput = textEditor;

			AddChild(inputBg);
		}

		public static UIBaseWindow CreateStringEditorWithLabel(string label, out MapEditorString stringEditor)
		{
			var container = new UIBaseWindow();
			container.LayoutMode = LayoutMode.HorizontalList;
			container.InputTransparent = false;
			container.ListSpacing = new Vector2(5, 0);
			container.InputTransparent = false;
			container.StretchX = true;
			container.StretchY = true;

			var labelWnd = new MapEditorLabel(label);
			container.AddChild(labelWnd);

			var stringEditorWnd = new MapEditorString();
			container.AddChild(stringEditorWnd);
			stringEditor = stringEditorWnd;

			return container;
		}
	}
}