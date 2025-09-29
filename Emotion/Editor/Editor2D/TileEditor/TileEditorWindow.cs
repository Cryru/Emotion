#nullable enable

using Emotion.Core.Systems.Scenography;
using Emotion.Editor.Editor2D.TileEditor.Tools;
using Emotion.Editor.EditorUI.Components;
using Emotion.Editor.EditorUI.GridEditor;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Game.Systems.UI;
using Emotion.Game.World.TileMap;
using Emotion.Primitives.Grids;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.Editor2D.TileEditor;

[DontSerialize]
public sealed class TileEditorWindow : GridEditorWindow
{
    public TileMapLayer? CurrentLayer { get; private set; }

    public TileMapTileset? CurrentTileset { get; private set; }

    public TileEditorTileTextureSelector TileTextureSelector { get; private set; } = null!;

    private DropdownChoiceEditor<TileMapTileset>? _tilesetChoose;

    public TileEditorWindow() : base()
    {
    }

    protected override GridEditorTool[] GetTools()
    {
        return [
            new TileEditorBrushTool(),
            new TileEditorEraserTool(),
            new TileEditorBucketTool(),
            new TileEditorPickerTool()
        ];
    }

    protected override string GetGridName()
    {
        return "TileMap";
    }

    protected override IGridWorldSpaceTiles? GetCurrentGrid()
    {
        return CurrentLayer;
    }

    protected override bool CanEdit()
    {
        if (CurrentTool is TileEditorTool tileEditorTool && tileEditorTool.RequireTileSelection)
        {
            AssertNotNull(TileTextureSelector);
            if (TileTextureSelector == null) return false;
            (TileTextureId, Vector2)[] currentlySelectedTile = TileTextureSelector.GetSelectedTileTextures(out _);
            if (currentlySelectedTile[0].Item1 == TileMapTile.Empty) return false;
        }

        return true;
    }

    protected override Vector2 UpdateCursor()
    {
        Vector3 worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
        return CurrentLayer!.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());
    }

    protected override void UseCurrentToolAtPosition(Vector2 tilePos)
    {
        if (CurrentTool is TileEditorTool tileTool)
            tileTool.ApplyTool(this, CurrentLayer, tilePos);
    }

    public override void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        base.SpawnBottomBarContent(bar, barContent);

        var sidePanel = new UIBaseWindow()
        {
            Name = "TileEditorSidePanel",
            Anchor = UIAnchor.BottomRight,
            ParentAnchor = UIAnchor.TopRight,
            MinSizeX = 400,
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            HandleInput = true,
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5)
        };
        barContent.AddChild(sidePanel);

        var sidePanelBg = new UISolidColor
        {
            WindowColor = EditorColorPalette.BarColor,
            BackgroundWindow = true
        };
        sidePanel.AddChild(sidePanelBg);

        IGenericReflectorComplexTypeHandler? tileDataTypeHandler = ReflectorEngine.GetComplexTypeHandler<GameMapTileData>();

        // Layers
        {
            //GameMapTileData? tileData = GetCurrentMapTileData();
            //List<TileMapLayerGrid>? list = tileData?.Layers;
            //_layerChoose?.SetEditorExtended(list, CurrentLayer, SelectTileLayer);
            GameMapTileData? tileData = GetCurrentMapTileData();
            ComplexTypeHandlerMemberBase? layerHandler = tileDataTypeHandler?.GetMemberByName(nameof(tileData.Layers));
            if (tileData != null && layerHandler != null)
            {
                //var listEditor = new ListEditor<TileMapLayer>(typeof(TileMapLayer));
                //listEditor.OnItemSelected = SelectTileLayer;

                //ObjectPropertyEditor layerEditor = new ObjectPropertyEditor(listEditor, tileData, layerHandler);
                //layerEditor.MinSizeY = 350;
                //layerEditor.MaxSizeY = 350;
                //layerEditor.SetVertical();

                //sidePanel.AddChild(layerEditor);
            }

            //var layers = new EditorSelectableListWithButtons<TileMapLayerGrid>()
            //{
            //    CanEdit = true,

            //    LabelText = "Layers",

            //    MaxSizeX = 400, // temp
            //    MinSizeY = 350,
            //    MaxSizeY = 350
            //};
            //sidePanel.AddChild(layers);
            //_layerChoose = layers;
        }

        // Tile selector
        {
            sidePanel.AddChild(new EditorLabel("Tilesets")
            {
                Margins = new Primitives.Rectangle(5, 0, 0, 0)
            });

            var tilesetChoose = new DropdownChoiceEditor<TileMapTileset>();
            sidePanel.AddChild(tilesetChoose);
            _tilesetChoose = tilesetChoose;

            var tilesetTileSelector = new TileEditorTileTextureSelector(this)
            {
                Name = "TileSelector",
                Layout =
                {
                    SizingY = UISizing.Fixed(450)
                },
                //MaxSizeX = 400, // temp
            };
            TileTextureSelector = tilesetTileSelector;
            sidePanel.AddChild(tilesetTileSelector);
        }

        // Select first tileset and layer.
        foreach (var layer in GetTileLayers())
        {
            SelectTileLayer(layer);
            break;
        }

        foreach (var tileset in GetTilesets())
        {
            SelectTileset(tileset);
            break;
        }
    }

    protected override bool RenderInternal(Renderer c)
    {
        c.SetUseViewMatrix(true);

        // Render current layer bounds
        if (CurrentLayer != null)
        {
            foreach (Rectangle chunkBound in CurrentLayer.ForEachLoadedChunkBound())
            {
                c.RenderRectOutline(chunkBound, Color.Black * 0.5f, 4 * GetScale());
            }
        }

        // Render cursor
        if (MouseInside)
        {
            if (CursorTilePos != null && CurrentLayer != null && CurrentTool is TileEditorTool tileTool)
                tileTool.RenderCursor(c, this, CurrentLayer, CursorTilePos.Value);
        }

        c.SetUseViewMatrix(false);

        return base.RenderInternal(c);
    }

    public GameMapTileData? GetCurrentMapTileData()
    {
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap && sceneWithMap.Map != null && sceneWithMap.Map.TileMapData != null)
            return sceneWithMap.Map.TileMapData;
        return null;
    }

    #region TileLayer

    public void TileLayersChanged()
    {
        //GameMapTileData? tileData = GetCurrentMapTileData();
        //List<TileMapLayerGrid>? list = tileData?.Layers;
        //_layerChoose?.SetEditorExtended(list, CurrentLayer, SelectTileLayer);
    }

    public IEnumerable<TileMapLayer> GetTileLayers()
    {
        GameMapTileData? tileData = GetCurrentMapTileData();
        if (tileData == null) return Array.Empty<TileMapLayer>();

        return tileData.Layers;
    }

    public void SelectTileLayer(TileMapLayer? tileLayer)
    {
        if (tileLayer == null)
        {
            CurrentLayer = null;
            EngineEditor.SetGridSize(Vector2.Zero);
            return;
        }

        bool found = false;
        foreach (TileMapLayer mapLayer in GetTileLayers())
        {
            if (mapLayer == tileLayer)
            {
                found = true;
                break;
            }
        }
        Assert(found, "Currently selected tile layer doesn't exist in the current map.");

        EngineEditor.SetGridSize(tileLayer.TileSize);
        CurrentLayer = tileLayer;

        TileLayersChanged();
    }

    #endregion

    #region Tilesets

    private void TilesetsChanged()
    {
        _tilesetChoose?.SetEditor(GetTilesets(), CurrentTileset, SelectTileset);
    }

    public IEnumerable<TileMapTileset> GetTilesets()
    {
        GameMapTileData? tileData = GetCurrentMapTileData();
        if (tileData == null) return Array.Empty<TileMapTileset>();

        return tileData.Tilesets;
    }

    public void SelectTileset(TileMapTileset? tileset)
    {
        if (TileTextureSelector == null)
        {
            CurrentTileset = null;
            TileTextureSelector?.SetTileset(null);
            return;
        }

        bool found = false;
        foreach (var mapTileset in GetTilesets())
        {
            if (mapTileset == tileset)
            {
                found = true;
                break;
            }
        }
        Assert(found, "Currently selected tileset doesn't exist in the current map.");

        TileTextureSelector?.SetTileset(tileset);
        CurrentTileset = tileset;

        TilesetsChanged();
    }

    public TilesetId GetCurrentTilesetIndex()
    {
        TilesetId i = 0;
        foreach (var mapTileset in GetTilesets())
        {
            if (mapTileset == CurrentTileset)
                return i;
            i++;
        }

        return TilesetId.Invalid;
    }

    #endregion
}
