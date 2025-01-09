namespace Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;

public abstract class TileEditorTool
{
    public string Name { get; protected set; }

    public bool IsPlacingTool { get; protected set; }

    public abstract void RenderCursor(RenderComposer c, TileEditorWindow editor);
}