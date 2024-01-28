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
    protected Vector2? _cursorPos = null;
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
        _cursorPos = null;
        if (!IsTileEditorOpen()) return;

        bool mouseInUI = UIController.MouseFocus != null && UIController.MouseFocus != _editUI;
        if (mouseInUI) return;

        var worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);

        var tileData = GetMapTileData();
        if (tileData == null) return;

        Vector2 tilePos = tileData.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());
        _cursorPos = tilePos;

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

        if (_cursorPos != null)
        {
            var mapTileData = GetMapTileData();
            if (mapTileData == null) return;

            var currentTool = editor.CurrentTool;
            var tileSize = mapTileData.TileSize;

            Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();

            var pos = _cursorPos.Value;
            if (currentTool == TileEditorTool.Brush)
            {
                (uint, Vector2)[]? placementPattern;
                Vector2 center = Vector2.Zero;
                if (editor.AreMultipleTilesSelected())
                {
                    placementPattern = editor.GetTidToPlaceMultiPattern(out center);
                }
                else
                {
                    uint tileToPlace = editor.GetTidToPlace();
                    placementPattern = new (uint, Vector2)[]
                    {
                        (tileToPlace, Vector2.Zero)
                    };
                }

                if (placementPattern == null) return;

                center = (center / tileSize).Floor() * tileSize;
                Vector2 cursorPosAbsolute = pos * tileSize;
                for (int i = 0; i < placementPattern.Length; i++)
                {
                    var data = placementPattern[i];
                    var tileToPlace = data.Item1;
                    var tileToPlaceOffset = data.Item2 - center;

                    Rectangle tileUv = mapTileData.GetUvFromTileImageId(tileToPlace, out int tsId);
                    Texture? tileSetTexture = mapTileData.GetTilesetTexture(tsId);
                    c.ClearDepth();
                    if (tileToPlace != 0)
                        c.RenderSprite((cursorPosAbsolute + tileToPlaceOffset).ToVec3(), tileSize, Color.White, tileSetTexture, tileUv);
                    c.RenderSprite((cursorPosAbsolute + tileToPlaceOffset).ToVec3(), tileSize, Color.Blue * 0.2f);
                    c.RenderOutline((cursorPosAbsolute + tileToPlaceOffset).ToVec3(), tileSize, Color.PrettyBlue, 1f);
                }
            }
            else if (currentTool == TileEditorTool.Eraser)
            {
                c.ClearDepth();
                c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.PrettyPink * 0.2f);
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.PrettyPink, 1f);
            }
            else if (currentTool == TileEditorTool.Bucket)
            {
                uint tileToPlace = editor.GetTidToPlace();
                Rectangle tileUv = mapTileData.GetUvFromTileImageId(tileToPlace, out int tsId);
                Texture? tileSetTexture = mapTileData.GetTilesetTexture(tsId);
                c.ClearDepth();
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.Green, 1f);
                uint previousTileID = mapTileData.GetTileData(layerToPlaceIn, mapTileData.GetTile1DFromTile2D(_cursorPos.Value));
                if (tileToPlace == 0 || tileToPlace == previousTileID) return;
                if (tileToPlace != 0 && layerToPlaceIn != null)
                {
                    _bucketTilesToSet.Clear();
                    SpanFill(pos, previousTileID, layerToPlaceIn);

                    var cam = c.Camera;
                    var camBound = cam.GetCameraFrustum();
                    int tilesShown = 0;
                    foreach (var tile in _bucketTilesToSet)
                    {
                        Rectangle drawRect = new Rectangle(tile * tileSize, tileSize);
                        if (!camBound.Intersects(drawRect)) continue; // Clip visible

                        c.RenderSprite((tile * tileSize).ToVec3(), tileSize, Color.White, tileSetTexture, tileUv);

                        if (tilesShown < 1500)
                            c.RenderSprite((tile * tileSize).ToVec3(), tileSize, Color.Blue * 0.2f);
                        tilesShown++;
                    }
                }
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.Green, 1f);
            }
            else if (currentTool == TileEditorTool.TilePicker)
            {
                c.ClearDepth();
                c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.PrettyPurple * 0.2f);
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.White, 1.5f);
                c.RenderOutline((pos * tileSize).ToVec3(), tileSize, Color.PrettyPurple, 1f);
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

    #region Bucket Tool

    private Stack<(Vector2, uint)> _spanFillStack = new Stack<(Vector2, uint)>();
    protected HashSet<Vector2> _bucketTilesToSet = new();

    private HashSet<Vector2> SpanFill(Vector2 cursorPosition, uint previousTileData, Map2DTileMapLayer layerToPlaceIn)
    {
        var mapTileData = GetMapTileData();
        if (mapTileData == null) return _bucketTilesToSet;

        _bucketTilesToSet.Clear();
        _spanFillStack.Clear();
        _spanFillStack.Push((cursorPosition, previousTileData));

        while (_spanFillStack.Count > 0)
        {
            var (cursorpos, currentTile) = _spanFillStack.Pop();
            int lx = (int)cursorpos.X;
            while (SpanFill_IsValid(new Vector2(lx - 1, (int)cursorpos.Y), currentTile, layerToPlaceIn, mapTileData))
            {
                _bucketTilesToSet.Add(new Vector2(lx - 1, (int)cursorpos.Y));
                lx--;
            }

            int rx = (int)cursorpos.X;
            while (SpanFill_IsValid(new Vector2(rx, (int)cursorpos.Y), currentTile, layerToPlaceIn, mapTileData))
            {
                _bucketTilesToSet.Add(new Vector2(rx, (int)cursorpos.Y));
                rx++;
            }

            SpanFill_Scan(lx, rx - 1, (int)cursorpos.Y + 1, currentTile, mapTileData, layerToPlaceIn);
            SpanFill_Scan(lx, rx - 1, (int)cursorpos.Y - 1, currentTile, mapTileData, layerToPlaceIn);
        }

        return _bucketTilesToSet;
    }

    private bool SpanFill_IsValid(Vector2 tilePosToCheck, uint currentTile, Map2DTileMapLayer layerToPlaceIn, Map2DTileMapData mapTileData)
    {
        var tile = mapTileData.GetTile1DFromTile2D(tilePosToCheck);
        uint TileID = mapTileData.GetTileData(layerToPlaceIn, tile);

        if (_bucketTilesToSet.Contains(new Vector2(tilePosToCheck.X, tilePosToCheck.Y))) return false;
        else return tilePosToCheck.X >= 0 && tilePosToCheck.X < mapTileData.SizeInTiles.X && tilePosToCheck.Y >= 0 && tilePosToCheck.Y < mapTileData.SizeInTiles.Y && currentTile == TileID;
    }

    private void SpanFill_Scan(int lx, int rx, int y, uint currentTile, Map2DTileMapData mapTileData, Map2DTileMapLayer layerToPlaceIn)
    {
        bool spanAdded = false;
        for (int i = lx; i <= rx; i++)
        {
            if (!SpanFill_IsValid(new Vector2(i, y), currentTile, layerToPlaceIn, mapTileData)) spanAdded = false;
            else if (!spanAdded)
            {
                _spanFillStack.Push((new Vector2(i, y), currentTile));
                spanAdded = true;
            }
        }
    }

    #endregion

    private void PaintCurrentTile()
    {
        MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
        AssertNotNull(editor);

        Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();
        if (layerToPlaceIn == null || _cursorPos == null) return;

        var mapTileData = GetMapTileData();
        if (mapTileData == null) return;

        WorldEditor2DActionTileChange? undoAction = _lastAction;
        if (undoAction == null)
            undoAction = new WorldEditor2DActionTileChange(this, layerToPlaceIn);

        switch (editor.CurrentTool)
        {
            case TileEditorTool.Eraser:
                {
                    var cursorPos1D = mapTileData.GetTile1DFromTile2D(_cursorPos.Value);
                    uint previousTileID = mapTileData.GetTileData(layerToPlaceIn, cursorPos1D);

                    mapTileData.SetTileData(layerToPlaceIn, cursorPos1D, 0);
                    undoAction.AddToEditHistory(cursorPos1D, previousTileID);
                    break;
                }
            case TileEditorTool.Brush:
                {
                    (uint, Vector2)[]? placementPattern;
                    Vector2 center = Vector2.Zero;
                    if (editor.AreMultipleTilesSelected())
                    {
                        placementPattern = editor.GetTidToPlaceMultiPattern(out center);
                    }
                    else
                    {
                        uint tileToPlace = editor.GetTidToPlace();
                        if (tileToPlace == 0) return;

                        placementPattern = new (uint, Vector2)[]
                        {
                            (tileToPlace, Vector2.Zero)
                        };
                    }
                    if (placementPattern == null) return;

                    var tileSize = mapTileData.TileSize;
                    center = (center / tileSize).Floor() * tileSize;

                    for (int i = 0; i < placementPattern.Length; i++)
                    {
                        var data = placementPattern[i];
                        var tileToPlace = data.Item1;
                        var tileToPlaceOffset = data.Item2 - center;

                        Vector2 thisTilePos = _cursorPos.Value + (tileToPlaceOffset / tileSize);
                        int thisTilePos1D = mapTileData.GetTile1DFromTile2D(thisTilePos);
                        uint previousTileID = mapTileData.GetTileData(layerToPlaceIn, thisTilePos1D);
                        mapTileData.SetTileData(layerToPlaceIn, thisTilePos1D, tileToPlace);


                        undoAction.AddToEditHistory(thisTilePos1D, previousTileID);
                    }
                    break;
                }
            case TileEditorTool.Bucket:
                {
                    var cursorPos1D = mapTileData.GetTile1DFromTile2D(_cursorPos.Value);
                    uint previousTileID = mapTileData.GetTileData(layerToPlaceIn, cursorPos1D);

                    uint tileToFill = editor.GetTidToPlace();
                    if (tileToFill == 0 || tileToFill == previousTileID) return;
                    if (_lastAction != null) return;
                    _bucketTilesToSet.Clear();
                    SpanFill(_cursorPos.Value, previousTileID, layerToPlaceIn);
                    foreach (var tile in _bucketTilesToSet)
                    {
                        mapTileData.SetTileData(layerToPlaceIn, mapTileData.GetTile1DFromTile2D(tile), tileToFill);
                        undoAction.AddToEditHistory(mapTileData.GetTile1DFromTile2D(tile), previousTileID);
                    }
                    break;
                }
            case TileEditorTool.TilePicker:
                {
                    uint tileToPick = mapTileData.GetTileData(layerToPlaceIn, mapTileData.GetTile1DFromTile2D(_cursorPos.Value));

                    uint tileToFill = editor.GetTidToPlace();
                    if (tileToPick == 0 || tileToPick == tileToFill) return;

                    editor.SetTidToPlace(tileToPick);
                    GlobalEditorMsg($"Picked tId {tileToPick}");
                    // return tool to brush 
                    // or to the previous tool ???
                    return;
                }
        }

        if (_lastAction == null)
        {
            _lastAction = undoAction;
            EditorRegisterAction(_lastAction);
        }
    }
}