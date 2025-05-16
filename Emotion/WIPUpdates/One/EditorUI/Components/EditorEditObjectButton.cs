using Emotion.Game.World.Editor;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorEditObjectButton : SquareEditorButtonWithTexture
{
    private object? _obj;
    private object? _parentObj;
    private IGenericReflectorTypeHandler? _typeHandler;

    private Func<object?>? _getObjectLazy;
    private Func<(object?, object?)>? _getObjectWithParentLazy;

    public EditorEditObjectButton(object? obj) : base("Editor/Edit.png")
    {
        SetObjectToEdit(obj, null);
    }

    public EditorEditObjectButton(Func<object?> getObjectLazy) : base("Editor/Edit.png")
    {
        _getObjectLazy = getObjectLazy;
    }

    public EditorEditObjectButton(Func<(object?, object?)> getObjectWithParentLazy) : base("Editor/Edit.png")
    {
        _getObjectWithParentLazy = getObjectWithParentLazy;
    }

    private void SetObjectToEdit(object? obj, object? parentObject)
    {
        _obj = obj;
        _parentObj = parentObject;
        _typeHandler = obj != null ? ReflectorEngine.GetTypeHandler(obj.GetType()) : null;
        Enabled = _typeHandler != null;
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        if (_getObjectLazy != null)
        {
            object? obj = _getObjectLazy.Invoke();
            if (obj == null) return;
            SetObjectToEdit(obj, null);
        }

        if (_getObjectWithParentLazy != null)
        {
            (object? obj, object? parent) = _getObjectWithParentLazy.Invoke();
            if (obj == null) return;
            SetObjectToEdit(obj, parent);
        }

        AssertNotNull(_obj);
        AssertNotNull(_typeHandler);

        var objEditWindow = new ObjectPropertyEditorWindow(_obj, _parentObj);
        
        EngineEditor.EditorRoot.AddChild(objEditWindow);
    }
}
