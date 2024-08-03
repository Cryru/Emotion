using Emotion.Editor.EditorHelpers;
using Emotion.Game.Time.Routines;
using Emotion.IO;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;
using System.Collections;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public class SpriteEntityTool : EditorWindow
{
    private UIList _list = null!;

    public SpriteEntityTool() : base("Sprite Entity Tool")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        {
            EditorButton button = new EditorButton("Import Texture");
            button.OnClickedProxy = (_) =>
            {
                var explorer = new FilePicker<TextureAsset>((file) =>
                {

                });
                Parent!.AddChild(explorer);
            };
            _contentParent.AddChild(button);
        }
    }

    protected override bool UpdateInternal()
    {

        return base.UpdateInternal();
    }
}
