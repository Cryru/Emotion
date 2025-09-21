#nullable enable

using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using Emotion.Standard.Reflector;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

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

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow contentParent = GetContentParent();
        contentParent.AddChild(_editorWindow);
    }
}
