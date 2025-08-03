#nullable enable

namespace Emotion.Editor.EditorUI.Components;

public class EditorUndoableOperation
{
    public string Name;

    public virtual void Undo()
    {

    }

    public virtual void Redo()
    {

    }
}
