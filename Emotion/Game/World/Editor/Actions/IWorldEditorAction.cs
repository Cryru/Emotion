#nullable enable

namespace Emotion.Game.World.Editor.Actions;

public interface IWorldEditorAction
{
    public void Undo();
    public bool IsStillValid(WorldBaseEditor editor);
}