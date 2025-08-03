using Emotion.Core.Systems.Input;

namespace Emotion.Editor.EditorUI.GridEditor;

public class GridEditorTool
{
    public string Name { get; protected set; }

    public bool IsPlacingTool { get; protected set; }

    public bool IsPrecisePaint { get; protected set; }

    public Key HotKey;
}
