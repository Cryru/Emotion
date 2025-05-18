#nullable enable

using Emotion.Standard.Reflector;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class ObjectPropertyEditorWindow : EditorWindow
{
    private ObjectPropertyWindow _editorWindow;

    public ObjectPropertyEditorWindow(object obj) : base($"Object Editor - {ReflectorEngine.GetTypeName(obj.GetType())}")
    {
        _editorWindow = new ObjectPropertyWindow();
        _editorWindow.SetEditor(obj);

        //_initialPosition = initialPosition;
        _initialSize = new Vector2(500, 300);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();
        contentParent.AddChild(_editorWindow);
    }
}
