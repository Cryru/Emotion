#region Using

using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.UI;
using System.Numerics;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
    public class MapEditorTopBarButton : UICallbackButton
    {
        public string Text
        {
            get
            {
                var text = (UIText) GetWindowById("buttonText");
                return text!.Text;
            }
            set
            {
                var text = (UIText) GetWindowById("buttonText");
                text!.Text = value;
            }
        }

        public MapEditorTopBarButton()
        {
            WindowColor = MapEditorColorPalette.ButtonColor;
            ScaleMode = UIScaleMode.FloatScale;

            var txt = new UIText();
            txt.ParentAnchor = UIAnchor.CenterLeft;
            txt.Anchor = UIAnchor.CenterLeft;
            txt.ScaleMode = UIScaleMode.FloatScale;
            txt.WindowColor = MapEditorColorPalette.TextColor;
            txt.Id = "buttonText";
            txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
            txt.FontSize = 6;
            //txt.TextHeightMode = Game.Text.GlyphHeightMeasurement.NoMinY;
            txt.IgnoreParentColor = true;
            AddChild(txt);

            StretchX = true;
            Paddings = new Rectangle(2, 1, 2, 1);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Bounds, _calculatedColor);
            return base.RenderInternal(c);
        }

        public override void OnMouseEnter(Vector2 _)
        {
            WindowColor = MapEditorColorPalette.ActiveButtonColor;
            base.OnMouseEnter(_);
        }

        public override void OnMouseLeft(Vector2 _)
        {
            WindowColor = MapEditorColorPalette.ButtonColor;
            base.OnMouseLeft(_);
        }
    }
}