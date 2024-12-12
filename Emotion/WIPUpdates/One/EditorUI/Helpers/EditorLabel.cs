using Emotion.Game.World.Editor;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.EditorUI.Helpers;

public class EditorLabel : UIRichText
{
    public EditorLabel()
    {
        WindowColor = MapEditorColorPalette.TextColor;
        FontSize = MapEditorColorPalette.EditorButtonTextSize;
        IgnoreParentColor = true;
        Anchor = UIAnchor.CenterLeft;
        ParentAnchor = UIAnchor.CenterLeft;
    }
}
