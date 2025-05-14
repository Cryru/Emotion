﻿using Emotion.Game.World.Editor;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class EditorEditObjectButton : SquareEditorButtonWithTexture
{
    private object? _obj;
    private IGenericReflectorTypeHandler? _typeHandler;

    private Func<object?>? _getObjectLazy;

    public EditorEditObjectButton(object? obj) : base("Editor/Edit.png")
    {
        SetObjectToEdit(obj);
    }

    public EditorEditObjectButton(Func<object?> getObjectLazy) : base("Editor/Edit.png")
    {
        _getObjectLazy = getObjectLazy;
    }

    private void SetObjectToEdit(object? obj)
    {
        _obj = obj;
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
            SetObjectToEdit(obj);
        }

        AssertNotNull(_obj);
        AssertNotNull(_typeHandler);

        var objEditWindow = new ObjectPropertyEditorWindow(_obj, Position2);
        EngineEditor.EditorRoot.AddChild(objEditWindow);
    }
}
