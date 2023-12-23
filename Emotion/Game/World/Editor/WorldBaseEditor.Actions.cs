#region Using

using Emotion.Game.World.Editor.Actions;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Game.World.Editor;

#nullable enable

public abstract partial class WorldBaseEditor
{
    protected static List<IWorldEditorAction> _actions = new();

    public static void EditorInvalidateUndoHistory()
    {
        _actions.Clear();
    }

    public static void EditorRegisterObjectMoveAction(BaseGameObject obj, Vector3 from, Vector3 to)
    {
        if (from == to) return;

        // If the last action is a move from the same position, then overwrite it.
        if (_actions.Count > 0)
        {
            IWorldEditorAction lastAction = _actions[^1];
            if (lastAction is EditorActionMove moveAction)
                if (moveAction.ObjTarget == obj && moveAction.StartPos == from)
                {
                    moveAction.NewPos = to;
                    return;
                }
        }

        var newMove = new EditorActionMove(obj)
        {
            StartPos = from,
            NewPos = to
        };
        _actions.Add(newMove);
    }

    public static void EditorRegisterObjectMutateAction(
        WorldBaseEditor editor,
        BaseGameObject obj,
        XMLFieldHandler fieldHandler,
        object? oldValue
    )
    {
        var newMutate = new WorldEditorActionMutate(editor, obj, fieldHandler, oldValue);
        _actions.Add(newMutate);
    }

    public static void EditorRegisterAction(IWorldEditorAction action)
    {
        _actions.Add(action);
    }

    public static void EditorUndoLastAction(WorldBaseEditor editor)
    {
        if (_actions.Count <= 0) return;

        for (int i = _actions.Count - 1; i >= 0; i--)
        {
            IWorldEditorAction lastAction = _actions[i];
            _actions.RemoveAt(i);

            if (lastAction.IsStillValid(editor))
            {
                lastAction.Undo();
                break;
            }
        }
    }
}