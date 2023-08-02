#nullable enable

#region Using

using Emotion.Game.World.Editor.Actions;
using Emotion.Standard.XML;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	protected List<IWorldEditorAction> _actions = new();

	protected void EditorRegisterMoveAction(GameObject2D obj, Vector2 from, Vector2 to)
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

	protected void EditorRegisterObjectMutateAction(GameObject2D obj, XMLFieldHandler fieldHandler, object? oldValue)
	{
		var newMutate = new WorldEditorActionMutate(this, obj, fieldHandler, oldValue);
		_actions.Add(newMutate);
	}

	protected void EditorUndoLastAction()
	{
		if (_actions.Count <= 0) return;

		IWorldEditorAction lastAction = _actions[^1];
		lastAction.Undo();
		_actions.RemoveAt(_actions.Count - 1);
	}
}