#nullable enable

#region Using

#endregion

namespace Emotion.Game.World.Editor.Actions;

public class EditorActionMove : IWorldEditorAction
{
	public BaseGameObject ObjTarget;
	public Vector2 StartPos;
	public Vector2 NewPos;

	public EditorActionMove(BaseGameObject obj)
	{
		ObjTarget = obj;
	}

	public void Undo()
	{
		ObjTarget.Position2 = StartPos;
	}

	public override string ToString()
	{
		return $"Moved {ObjTarget} From {StartPos} to {NewPos}";
	}
}