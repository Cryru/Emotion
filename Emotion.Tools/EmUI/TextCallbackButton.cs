#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Tools.EmUI
{
    public class TextCallbackButton : UICallbackButton
    {
        public string Text
        {
            get
            {
                var winTitle = (UIText?) GetWindowById("ButtonText");
                if (winTitle == null) return "";

                return winTitle.Text;
            }
            set
            {
                var winTitle = (UIText?) GetWindowById("ButtonText");
                if (winTitle == null) return;

                winTitle.Text = value;
            }
        }

        public TextCallbackButton(string text)
        {
            var label = new EditorTextWindow();
            label.Id = "ButtonText";
            label.Anchor = UIAnchor.CenterCenter;
            label.ParentAnchor = UIAnchor.CenterCenter;
            AddChild(label);

            WindowColor = IMBaseWindow.MainColorLight;
            StretchX = true;
            StretchY = true;
            InputTransparent = false;
            Paddings = new Primitives.Rectangle(3, 1, 3, 1);

            Text = text;
        }

        public override void OnMouseEnter(Vector2 _)
        {
            WindowColor = IMBaseWindow.MainColorLightMouseIn;
            base.OnMouseEnter(_);
        }

        public override void OnMouseLeft(Vector2 _)
        {
            WindowColor = IMBaseWindow.MainColorLight;
            base.OnMouseEnter(_);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, WindowColor);
            return base.RenderInternal(c);
        }
    }
}