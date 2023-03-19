#region Using

using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorLabel : UIText
	{
		public MapEditorLabel(string label)
		{
			ScaleMode = UIScaleMode.FloatScale;
			WindowColor = MapEditorColorPalette.TextColor;
			FontFile = "Editor/UbuntuMono-Regular.ttf";
			FontSize = MapEditorColorPalette.EditorButtonTextSize;
			IgnoreParentColor = true;
			Text = label;
			Anchor = UIAnchor.CenterLeft;
			ParentAnchor = UIAnchor.CenterLeft;
		}
	}
}