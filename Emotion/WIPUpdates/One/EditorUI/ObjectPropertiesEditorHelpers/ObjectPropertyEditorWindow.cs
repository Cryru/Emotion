#nullable enable

using Emotion.Standard.Reflector;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyEditorWindow : EditorWindow
{
    private ObjectPropertyWindow _editorWindow;

    private object _obj;

    public ObjectPropertyEditorWindow(object obj, Vector2 initialPosition) : base($"Object Editor - {ReflectorEngine.GetTypeName(obj.GetType())}")
    {
        _obj = obj;

        _editorWindow = new ObjectPropertyWindow();
        _editorWindow.SetEditor(_obj);

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
        contentParent.AddChild(_editorWindow);
    }
}
