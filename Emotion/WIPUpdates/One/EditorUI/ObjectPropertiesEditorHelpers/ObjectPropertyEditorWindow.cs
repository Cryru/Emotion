#nullable enable

using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyEditorWindow : EditorWindow
{
    private object _obj;

    public ObjectPropertyEditorWindow(object obj) : base($"Object Editor")
    {
        _obj = obj;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        var propertyWindow = new ObjectPropertyWindow();
        propertyWindow.SetEditor(_obj);
        contentParent.AddChild(propertyWindow);
    }
}
