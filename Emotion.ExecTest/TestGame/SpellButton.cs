using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.ExecTest.TestGame
{
    public class SpellButton : UIBaseWindow
    {
        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);

            UITexture icon = new UITexture()
            {
                TextureFile = "thrash.png",
                Paddings = new Rectangle(3, -3, 3, 3),
                Smooth = true
            };
            AddChild(icon);

            UIRichText hotkey = new UIRichText
            {
                Text = "4",
                WindowColor = Color.White,
                OutlineColor = Color.Black,
                OutlineSize = 2,
                FontSize = 20,
                AnchorAndParentAnchor = UIAnchor.TopRight,
                TextHeightMode = Game.Text.GlyphHeightMeasurement.NoDescent
            };
            icon.AddChild(hotkey);
        }
    }
}
