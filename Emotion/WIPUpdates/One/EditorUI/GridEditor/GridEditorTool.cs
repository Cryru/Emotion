using Emotion.Common.Input;

namespace Emotion.WIPUpdates.One.EditorUI.GridEditor;

public class GridEditorTool
{
    public string Name { get; protected set; }

    public bool IsPlacingTool { get; protected set; }

    public bool IsPrecisePaint { get; protected set; }

    public Key HotKey;
}
