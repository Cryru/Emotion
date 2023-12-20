#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.Tile;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
    // Tile selection
    private Vector2? _tileBrush = null;

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
        _grid.Tint = Color.Black.Clone().SetAlpha(125);
        _grid.Size3D = (tileData.TileSize * tileData.SizeInTiles).ToVec3();
        _grid.Position = Vector3.Zero + _grid.Size3D / 2f;
        _grid.ApplyTopLeftOriginCorrection = true;
    }

    protected virtual bool TileEditorInputHandler(Key key, KeyStatus status)
    {
        if (!IsTileEditorOpen()) return true;

        var mapTileData = GetMapTileData();
        if (mapTileData == null) return true;

        if (key == Key.MouseKeyLeft && status == KeyStatus.Up)
        {
            MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
            AssertNotNull(editor);

            Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();
            uint tileToPlace = editor.GetTidToPlace();

            if (layerToPlaceIn != null && _tileBrush != null)
            {
                var tileBrush1D = mapTileData.GetTile1DFromTile2D(_tileBrush.Value);
                layerToPlaceIn.SetTileData(tileBrush1D, tileToPlace);

                // Recache render data
                mapTileData.LoadTilesetTextures().Wait();
            }
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

            var tileSize = mapTileData.TileSize;

            Map2DTileMapLayer? layerToPlaceIn = editor.GetLayer();
            uint tileToPlace = editor.GetTidToPlace();
            Rectangle tileUv = mapTileData.GetUvFromTileImageId(tileToPlace, out int tsId);

            TextureAsset? tileSetTexture = mapTileData.TilesetsLoaded != null ? mapTileData.TilesetsLoaded[tsId] : null;

            var pos = _tileBrush.Value;
            c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.White, tileSetTexture?.Texture, tileUv);
            c.RenderSprite((pos * tileSize).ToVec3(), tileSize, Color.Blue * 0.2f);
        }
    }

    protected Map2DTileMapData? GetMapTileData()
    {
        var map = CurrentMap;
        if (map == null) return null;
        return map.TileData;
    }

    protected bool IsTileEditorOpen()
    {
        return _tileEditor != null && _tileEditor.Controller != null;
    }
}