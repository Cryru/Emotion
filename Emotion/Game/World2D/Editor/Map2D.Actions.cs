#region Using

using System.Collections.Generic;
using System.Numerics;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
    public partial class Map2D
    {
        private abstract class EditorAction
        {
            public GameObject2D ObjTarget;
            public abstract void Undo();

            protected EditorAction(GameObject2D obj)
            {
                ObjTarget = obj;
            }
        }

        private class EditorActionMove : EditorAction
        {
            public Vector2 StartPos;
            public Vector2 NewPos;

            public EditorActionMove(GameObject2D obj) : base(obj)
            {
            }

            public override void Undo()
            {
                ObjTarget.Position2 = StartPos;
            }
        }

        private List<EditorAction>? _actions;

        private void EditorRegisterMoveAction(GameObject2D obj, Vector2 from, Vector2 to)
        {
            if (from == to) return;

            _actions ??= new();

            // If the last action is a move from the same position, then overwrite it.
            if (_actions.Count > 0)
            {
                EditorAction lastAction = _actions[^1];
                if (lastAction is EditorActionMove moveAction)
                    if (moveAction.ObjTarget == obj && moveAction.StartPos == from)
                    {
                        moveAction.NewPos = to;
                        return;
                    }
            }

            var newMove = new EditorActionMove(obj);
            newMove.StartPos = from;
            newMove.NewPos = to;
            _actions.Add(newMove);
        }

        private void EditorUndoLastAction()
        {
            if (_actions == null || _actions.Count <= 0) return;
            EditorAction lastAction = _actions[^1];
            lastAction.Undo();
            _actions.RemoveAt(_actions.Count - 1);
        }
    }
}