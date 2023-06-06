#region Using

using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class FieldEditorWithLabel : UIBaseWindow
	{
		public FieldEditorWithLabel(string labelText, IMapEditorGeneric editor)
		{
			InputTransparent = false;
			StretchX = true;
			StretchY = true;
			Margins = new Rectangle(3, 0, 0, 0);
			LayoutMode = LayoutMode.HorizontalList;
			ListSpacing = new Vector2(5, 0);

			var label = new MapEditorLabel(labelText);
			AddChild(label);

			if (editor == null) return;
			var editorAsWnd = (UIBaseWindow) editor;
			AddChild(editorAsWnd);
		}
	}
}