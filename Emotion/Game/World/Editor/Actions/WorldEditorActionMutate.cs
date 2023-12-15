#nullable enable

#region Using

using Emotion.Standard.XML;

#endregion

namespace Emotion.Game.World.Editor.Actions;

public class WorldEditorActionMutate : IWorldEditorAction
{
    public WorldBaseEditor Editor;
    public BaseGameObject ObjTarget;
    public XMLFieldHandler Field;
    public object? OldValue;

    public WorldEditorActionMutate(WorldBaseEditor editor, BaseGameObject objTarget, XMLFieldHandler fieldHandler, object? oldValue)
    {
        Editor = editor;
        ObjTarget = objTarget;
        Field = fieldHandler;
        OldValue = oldValue;
    }

    public void Undo()
    {
        Editor.ChangeObjectProperty(ObjTarget, Field, OldValue, false);
    }

    public override string ToString()
    {
        return $"Changed {ObjTarget} property {Field.Name}";
    }
}