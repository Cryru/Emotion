#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#endregion

namespace Emotion.Editor.PropertyEditors;

public class PropEditorStringPath : PropEditorString
{
    private AssetFileNameAttribute _attribute;

    public PropEditorStringPath(AssetFileNameAttribute attribute)
    {
        _attribute = attribute;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var b = new EditorButton();
        b.Margins = new Rectangle(1, 0, 0, 0);
        b.StretchY = true;
        b.Text = "...";
        b.OnClickedProxy = _ =>
        {
            EditorPanel fileExplorer = _attribute.CreateFileExplorer(this);
            Controller!.AddChild(fileExplorer);
        };
        AddChild(b);

        LayoutMode = LayoutMode.HorizontalList;
    }
}