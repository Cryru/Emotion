#nullable enable

using Emotion.Core.Systems.Input;
using Emotion.Game.World.TileMap;

namespace Emotion.Editor.Editor2D.TileEditor.Tools;

public class TileEditorEraserTool : TileEditorTool
{
    public TileEditorEraserTool()
    {
        Name = "Eraser";
        IsPrecisePaint = true;
        HotKey = Key.R;
    }

    public override void ApplyTool(TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        AssertNotNull(editor.TileTextureSelector);
        if (editor.TileTextureSelector == null) return;

        GameMapTileData? tileData = editor.GetCurrentMapTileData();
        if (tileData == null) return;

        currentLayer.ExpandingSetAt(cursorPos, TileMapTile.Empty);
    }

    public override void RenderCursor(Renderer c, TileEditorWindow editor, TileMapLayer currentLayer, Vector2 cursorPos)
    {
        Vector2 tileInWorld = currentLayer.GetWorldPosOfTile(cursorPos);
        Vector2 tileSize = currentLayer.TileSize;

        c.RenderSprite(tileInWorld.ToVec3(), tileSize, Color.PrettyPink * 0.2f);
        c.RenderRectOutline(tileInWorld.ToVec3(), tileSize, Color.PrettyPink, 3f * editor.GetScale());
    }
}

