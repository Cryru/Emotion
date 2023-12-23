#region Using

using Emotion.Game.World.Editor;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorHelpers
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

    public class MapEditorLabelWithBackground : MapEditorLabel
    {
        public Color BackgroundColor;

        public MapEditorLabelWithBackground(string label) : base(label)
        {
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Bounds, BackgroundColor * _calculatedColor.A);
            return base.RenderInternal(c);
            ;
        }
    }
}