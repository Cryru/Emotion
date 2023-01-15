#region Using

using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorTextFieldEditor : UIBaseWindow
	{
		public string Text
		{
			get
			{
				var input = (UITextInput) GetWindowById("Input");
				return input.Text;
			}
			set
			{
				var input = (UITextInput) GetWindowById("Input");
				input.Text = value;
			}
		}

		private Action<string> _onSubmit;

		public MapEditorTextFieldEditor(string label, Action<string> onSubmit)
		{
			_onSubmit = onSubmit;

			LayoutMode = LayoutMode.VerticalList;
			ListSpacing = new Vector2(0, 5);
			InputTransparent = false;
			StretchX = true;
			StretchY = true;

			var txt = new UIText();
			txt.ScaleMode = UIScaleMode.FloatScale;
			txt.WindowColor = MapEditorColorPalette.TextColor;
			txt.Id = "buttonText";
			txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
			txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			txt.IgnoreParentColor = true;
			txt.Text = label;
			AddChild(txt);

			var inputBg = new UISolidColor();
			inputBg.InputTransparent = false;
			inputBg.StretchX = true;
			inputBg.StretchY = true;
			inputBg.WindowColor = Color.Black;

			{
				var input = new UITextInput();
				input.MinSize = new Vector2(100, 0);
				input.FontFile = "Editor/UbuntuMono-Regular.ttf";
				input.FontSize = MapEditorColorPalette.EditorButtonTextSize;
				input.IgnoreParentColor = true;
				input.SizeOfText = true;
				input.Id = "Input";
				inputBg.AddChild(input);
			}

			AddChild(inputBg);
		}

		public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
		{
			if (key == Key.Enter && status == KeyStatus.Down) _onSubmit?.Invoke(Text);

			return base.OnKey(key, status, mousePos);
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);
		}
	}
}