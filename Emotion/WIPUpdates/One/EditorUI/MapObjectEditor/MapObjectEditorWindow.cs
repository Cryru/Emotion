using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.MapObjectEditor;

public class MapObjectEditorWindow : UIBaseWindow
{
    private EditorLabel _bottomText = null!;

    public MapObjectEditorWindow()
    {

    }

    public void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        // Bottom text
        {
            var textList = new UIBaseWindow()
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(10, 0),
                AnchorAndParentAnchor = UIAnchor.CenterLeft,
                FillY = false,
            };
            barContent.AddChild(textList);

            var label = new EditorLabel
            {
                Text = "Objects",
                WindowColor = Color.White * 0.5f
            };
            textList.AddChild(label);

            var labelDynamic = new EditorLabel
            {
                Text = "",
                AllowRenderBatch = false
            };
            textList.AddChild(labelDynamic);
            _bottomText = labelDynamic;
        }
    }
}
