#region Using

using Emotion.Standard.XML;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public partial class Map2D
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
			public GameObject2D OldData;
			public Map2D ObjectMap;

			public EditorActionObjectMutate(GameObject2D oldData)
			{
				OldData = oldData;
				ObjectMap = oldData.Map;
			}

			public override void Undo()
			{
				// Restore old data in place of new data.
				Map2D map = ObjectMap;

				for (var i = 0; i < map._objects.Count; i++)
				{
					GameObject2D obj = map._objects[i];
					if (obj.UniqueId == OldData.UniqueId) // Found the new representation of the object.
					{
						map.RemoveObject(obj, true);
						break;
					}
				}

				// Keep parity with ApplyObjectChange
				string oldDataAsXML = XMLFormat.To(OldData);
				var oldDataRecreated = XMLFormat.From<GameObject2D>(oldDataAsXML);

				map.AddObject(oldDataRecreated);
			}

			public override string ToString()
			{
				return $"Changed Property on {OldData.UniqueId}";
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

			MapEditorObjectPropertiesPanel? propPanelOpen = EditorGetAlreadyOpenPropertiesPanelForObject(obj.UniqueId);
			propPanelOpen?.InvalidateObjectReference();
		}

		private void EditorRegisterObjectPropertyChange(GameObject2D oldObjData)
		{
			_actions ??= new();

			var newMutate = new EditorActionObjectMutate(oldObjData);
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
}