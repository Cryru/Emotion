#nullable enable

#region Using

#endregion

namespace Emotion.Game.World.Editor.Actions;

public class EditorActionMove : IWorldEditorAction
{
    public BaseGameObject ObjTarget;
    public Vector3 StartPos;
    public Vector3 NewPos;

    public EditorActionMove(BaseGameObject obj)
    {
        ObjTarget = obj;
    }

    public void Undo()
    {
        ObjTarget.Position = StartPos;
    }

    public override string ToString()
    {
        return $"Moved {ObjTarget} From {StartPos} to {NewPos}";
    }
}