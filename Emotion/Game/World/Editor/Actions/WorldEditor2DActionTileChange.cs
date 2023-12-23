using Emotion.Game.World2D;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.Tile;

#nullable enable

namespace Emotion.Game.World.Editor.Actions;

public class WorldEditor2DActionTileChange : IWorldEditorAction
{
    protected World2DEditor _editor;
    protected Map2D? _map;
    protected Map2DTileMapLayer _layer;

    protected List<(int coordinate, uint previousTid)> _editsDone = new();

    public WorldEditor2DActionTileChange(World2DEditor editor, Map2DTileMapLayer layer)
    {
        _editor = editor;
        _map = editor.CurrentMap;
        _layer = layer;
    }

    public bool IsStillValid(WorldBaseEditor editor)
    {
        if (_map == null) return false;

        var map = editor.CurrentMap;
        if (map != _map) return false;

        var tileData = _map.Tiles;
        if (!tileData.Layers.Contains(_layer)) return false;

        return true;
    }

    public void AddToEditHistory(int coordinate, uint tId)
    {
        for (int i = 0; i < _editsDone.Count; i++)
        {
            var edit = _editsDone[i];
            if (edit.coordinate == coordinate) return;
        }

        _editsDone.Add((coordinate, tId));
    }

    public void Undo()
    {
        var tileData = _map?.Tiles;
        if (tileData == null) return;

        for (int i = _editsDone.Count - 1; i >= 0; i--)
        {
            var edit = _editsDone[i];
            tileData.SetTileData(_layer, edit.coordinate, edit.previousTid);
        }
    }

    public override string ToString()
    {
        var tileData = _map?.Tiles;
        if (tileData == null) return "Error";

        if (_editsDone.Count == 0) return "Error";

        var firstEdit = _editsDone[0];
        var tile2d = tileData.GetTile2DFromTile1D(firstEdit.coordinate);
        return $"Changed tiles in layer {_layer.Name} around {tile2d}";
    }
}
