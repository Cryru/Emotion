using Emotion.Graphics;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorFloat2 : UIBaseWindow
	{
		private Vector2 _value;
		private string _xEdit;
		private string _yEdit;

		private Action<Vector2> _callback;

		public MapEditorFloat2(Vector2 startingValue, Action<Vector2> valueChange)
		{
			_value = startingValue;
			_callback = valueChange;

			LayoutMode = LayoutMode.HorizontalList;
			ListSpacing = new Vector2(2, 0);
			InputTransparent = false;
			StretchX = true;
			StretchY = true;
			//Paddings = new Rectangle(2, 1, 2, 1);
		}

		public void FloatEditor(string label)
		{
			var txt = new UIText();
			txt.ScaleMode = UIScaleMode.FloatScale;
			txt.WindowColor = MapEditorColorPalette.TextColor;
			txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
			txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			txt.IgnoreParentColor = true;
			txt.Text = label;
			txt.Anchor = UIAnchor.CenterLeft;
			txt.ParentAnchor = UIAnchor.CenterLeft;
			if(Children != null && Children.Count > 0) txt.Margins = new Rectangle(2, 0, 0, 0);
			AddChild(txt);

			var inputBg = new UISolidColor();
			inputBg.InputTransparent = false;
			inputBg.StretchX = true;
			inputBg.StretchY = true;
			inputBg.WindowColor = MapEditorColorPalette.ButtonColor;
			inputBg.Paddings = new Rectangle(2, 1, 2, 1);
			inputBg.Anchor = UIAnchor.CenterLeft;
			inputBg.ParentAnchor = UIAnchor.CenterLeft;

			var xEditor = new UITextInput();
			xEditor.Text = _value.X.ToString();
			xEditor.FontFile = "Editor/UbuntuMono-Regular.ttf";
			xEditor.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			xEditor.SizeOfText = true;
			xEditor.MinSize = new Vector2(20, 0);
			xEditor.IgnoreParentColor = true;
			inputBg.AddChild(xEditor);

			AddChild(inputBg);
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			FloatEditor("X:");
			FloatEditor("Y:");
		}

		protected override bool RenderInternal(RenderComposer c)
		{
			return base.RenderInternal(c);
		}
	}
}
