#nullable enable

#region Using

using Emotion.Standard.XML;

#endregion

#region Using

#endregion

namespace Emotion.Game.World2D;

public partial class World2DEditor
{
	private abstract class EditorAction
	{
		public abstract void Undo();
	}

	private class EditorActionMove : EditorAction
	{
		public GameObject2D ObjTarget;
		public Vector2 StartPos;
		public Vector2 NewPos;

		public EditorActionMove(GameObject2D obj)
		{
			ObjTarget = obj;
		}

		public override void Undo()
		{
			ObjTarget.Position2 = StartPos;
		}

		public override string ToString()
		{
			return $"Move {ObjTarget.ToString()} From {StartPos} to {NewPos}";
		}
	}

	private class EditorActionObjectMutate : EditorAction
	{
		public World2DEditor Editor;
		public GameObject2D Obj;
		public XMLFieldHandler Field;
		public object OldValue;

		public EditorActionObjectMutate(World2DEditor editor, GameObject2D obj, XMLFieldHandler fieldHandler, object oldValue)
		{
			Editor = editor;
			Obj = obj;
			Field = fieldHandler;
			OldValue = oldValue;
		}

		public override void Undo()
		{
			Editor.ChangeObjectProperty(Obj, Field, OldValue, false);
		}

		public override string ToString()
		{
			return $"Changed {Obj} property {Field.Name}";
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

	private void EditorRegisterObjectPropertyChange(GameObject2D obj, XMLFieldHandler fieldHandler, object oldValue)
	{
		_actions ??= new();

		var newMutate = new EditorActionObjectMutate(this, obj, fieldHandler, oldValue);
		_actions.Add(newMutate);
	}

	private void EditorUndoLastAction()
	{
		if (_actions == null || _actions.Count <= 0) return;
		EditorAction lastAction = _actions[^1];
		lastAction.Undo();
		_actions.RemoveAt(_actions.Count - 1);
	}
}