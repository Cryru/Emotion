#nullable enable

using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyEditorWindow : EditorWindow
{
    private object _obj;

    public ObjectPropertyEditorWindow(object obj, Vector2 initialPosition) : base($"Object Editor - {obj.GetType().Name}")
    {
        _obj = obj;

        //_initialPosition = initialPosition;
        _initialSize = new Vector2(500, 300);
    }

    public ObjectPropertyEditorWindow(object obj) : this(obj, new Vector2(-1))
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        var propertyWindow = new ObjectPropertyWindow();
        propertyWindow.MinSize = new Vector2(110);
        propertyWindow.SetEditor(_obj);
        contentParent.AddChild(propertyWindow);
    }
}
