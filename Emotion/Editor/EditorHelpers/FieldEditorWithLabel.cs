#region Using

using Emotion.Editor.PropertyEditors;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers
{
    public class FieldEditorWithLabel : UIBaseWindow
    {
        public FieldEditorWithLabel(string labelText, IPropEditorGeneric editor, LayoutMode layout = LayoutMode.HorizontalList)
        {
            StretchX = true;
            StretchY = true;
            Margins = new Rectangle(3, 0, 0, 0);
            LayoutMode = layout;
            ListSpacing = new Vector2(5, 0);

            var label = new MapEditorLabel(labelText);
            AddChild(label);

            if (editor == null) return;
            var editorAsWnd = (UIBaseWindow)editor;
            AddChild(editorAsWnd);
        }
    }
}