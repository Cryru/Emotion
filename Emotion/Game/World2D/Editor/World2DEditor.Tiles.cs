#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World.Editor.Actions;
using Emotion.Game.World2D.Tile;
using Emotion.Game.World3D.Objects;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
    protected Vector2? _tileBrush = null;
    protected bool _mouseDown = false;
    protected WorldEditor2DActionTileChange? _lastAction;

    protected SquareGrid3D? _grid;
    protected UIBaseWindow? _tileEditor;

    protected void InitializeTileEditor()
    {
        InitializeTileGrid();
    }

    private void InitializeTileGrid()
    {
        if (_grid == null)
        {
            Task.Run(() =>
            {
                _grid = new SquareGrid3D();
                _grid.LoadAssetsAsync().Wait();
                InitializeTileGrid();
            });
            return;
        }

        var tileData = GetMapTileData();
        if (tileData == null) return;

        _grid.TileSize = tileData.TileSize.X;
        _grid.GridOffset = Vector2.Zero;
        _grid.Tint = Color.Black.Clone().SetAlpha(90);
        _grid.Size3D = (tileData.TileSize * tileData.SizeInTiles).ToVec3();
        _grid.Position = Vector3.Zero + _grid.Size3D / 2f;
        _grid.ApplyTopLeftOriginCorrection = true;
    }

    protected virtual bool TileEditorInputHandler(Key key, KeyStatus status)
    {
        if (!IsTileEditorOpen()) return true;

        var mapTileData = GetMapTileData();
        if (mapTileData == null) return true;

        if (key == Key.MouseKeyLeft)
        {
            _mouseDown = status == KeyStatus.Down;

            // Clear last action to group undos by mouse clicks.
            if (!_mouseDown) _lastAction = null;
        }

        return true;
    }

    protected void UpdateTileEditor()
    {
        _tileBrush = null;
        if (!IsTileEditorOpen()) return;

        bool mouseInUI = UIController.MouseFocus != null && UIController.MouseFocus != _editUI;
        if (mouseInUI) return;

        var worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);

        var tileData = GetMapTileData();
        if (tileData == null) return;

        Vector2 tilePos = tileData.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());
        _tileBrush = tilePos;

        MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
        AssertNotNull(editor);
        Map2DTileMapLayer? selectedLayer = editor.GetLayer();
        var tileBrush1D = tileData.GetTile1DFromTile2D(tilePos);
        uint tId = tileData.GetTileData(selectedLayer, tileBrush1D);

        if (_bottomBarText != null)
            _bottomBarText.Text = $"Rollover ({tilePos}) TId - {(tId == 0 ? "0 (Empty)" : tId.ToString())}";

        if (_mouseDown)
            PaintCurrentTile();
    }

    protected void RenderTileEditor(RenderComposer c)
    {
        if (!IsTileEditorOpen()) return;
        _grid?.Render(c);

        MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
        AssertNotNull(editor);

        if (_tileBrush != null)
        {
            var mapTileData = GetMapTileData();
            if (mapTileData == null) return;

            var currentTool = editor.CurrentTool;
            var tileSize = mapTileData.TileSize;

            Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();
            uint tileToPlace = editor.GetTidToPlace();

            var pos = _tileBrush.Value;
            if (currentTool == TileEditorTool.Brush)
            {
                Rectangle tileUv = mapTileData.GetUvFromTileImageId(tileToPlace, out int tsId);
                Texture? tileSetTexture = mapTileData.GetTilesetTexture(tsId);
                c.ClearDepth();
                if (tileToPlace != 0)
                    c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.White, tileSetTexture, tileUv);
                c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.Blue * 0.2f);
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.PrettyBlue, 1f);
            }
            else if (currentTool == TileEditorTool.Eraser)
            {
                c.ClearDepth();
                c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.PrettyPink * 0.2f);
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.PrettyPink, 1f);
            }
        }
    }

    protected Map2DTileMapData? GetMapTileData()
    {
        var map = CurrentMap;
        if (map == null) return null;
        return map.Tiles;
    }

    protected bool IsTileEditorOpen()
    {
        return _tileEditor != null && _tileEditor.Controller != null;
    }

    private void PaintCurrentTile()
    {
        MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
        AssertNotNull(editor);

        Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();
        if (layerToPlaceIn == null || _tileBrush == null) return;

        var mapTileData = GetMapTileData();
        if (mapTileData == null) return;

        var tileBrush1D = mapTileData.GetTile1DFromTile2D(_tileBrush.Value);
        uint previousTileID = mapTileData.GetTileData(layerToPlaceIn, tileBrush1D);

        WorldEditor2DActionTileChange? undoAction = _lastAction;
        if (undoAction == null)
        {
            undoAction = new WorldEditor2DActionTileChange(this, layerToPlaceIn);
        }

        switch (editor.CurrentTool)
        {
            case TileEditorTool.Eraser:
                mapTileData.SetTileData(layerToPlaceIn, tileBrush1D, 0);
                break;
            case TileEditorTool.Brush:
                uint tileToPlace = editor.GetTidToPlace();
                if (tileToPlace == 0) return;
                mapTileData.SetTileData(layerToPlaceIn, tileBrush1D, tileToPlace);
                break;
        }

        undoAction.AddToEditHistory(tileBrush1D, previousTileID);
        if (_lastAction == null)
        {
            _lastAction = undoAction;
            EditorRegisterAction(_lastAction);
        }
    }
}