using Emotion.Common.Serialization;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Tile;
using Emotion.Platform.Input;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.TileMap;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor;

[DontSerialize]
public sealed class TileEditorWindow : UIBaseWindow
{
    public static TileEditorTool[] Tools =
    {
        new TileEditorBrushTool(),
        new TileEditorEraserTool(),
        new TileEditorBucketTool(),
        new TileEditorPickerTool()
    };

    public TileEditorTool CurrentTool = Tools[0];

    public Vector2? CursorTilePos { get; private set; }

    public TileMapLayerGrid? CurrentLayer { get; private set; }

    public TileMapTileset? CurrentTileset { get; private set; }

    private TileEditorTool _lastUsedPlacingTool = Tools[0];

    private UIBaseWindow? _bottomBarToolButtons;
    private UIRichText? _bottomText;
    private TileEditorTileTextureSelector? _tileTextureSelector;

    private DropdownChoiceEditor<TileMapTileset>? _tilesetChoose;

    private bool _mouseDown;

    public TileEditorWindow()
    {
        HandleInput = true;
    }

    public void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        var back = new EditorButton("Back")
        {
            OnClickedProxy = (_) => bar.SpawnEditorChoiceScreen(),
            Anchor = UIAnchor.BottomLeft,
            ParentAnchor = UIAnchor.TopLeft,
        };
        barContent.AddChild(back);

        var sidePanel = new UIBaseWindow()
        {
            Anchor = UIAnchor.BottomRight,
            ParentAnchor = UIAnchor.TopRight,
            MinSizeX = 400,
            //Paddings = new Primitives.Rectangle(10, 10, 10, 10),
            HandleInput = true,
            LayoutMode = LayoutMode.VerticalList
        };
        barContent.AddChild(sidePanel);

        {
            var sidePanelBackground = new UISolidColor
            {
                WindowColor = MapEditorColorPalette.BarColor * 0.3f,
                BackgroundWindow = true
            };
            sidePanel.AddChild(sidePanelBackground);

            var tilesetChoose = new DropdownChoiceEditor<TileMapTileset>();
            sidePanel.AddChild(tilesetChoose);
            _tilesetChoose = tilesetChoose;

            var tilesetTileSelector = new TileEditorTileTextureSelector(this)
            {
                Id = "TileSelector",

                MaxSizeX = 400, // temp
                MinSizeY = 450,
                MaxSizeY = 450
            };
            _tileTextureSelector = tilesetTileSelector;
            sidePanel.AddChild(tilesetTileSelector);
        }

        var textList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(10, 0),
            AnchorAndParentAnchor = UIAnchor.CenterLeft,
            FillY = false,
        };
        barContent.AddChild(textList);

        var label = new EditorLabel
        {
            Text = "Tile Editor",
            WindowColor = Color.White * 0.5f
        };
        textList.AddChild(label);

        var labelDynamic = new EditorLabel
        {
            Text = "",
            AllowRenderBatch = false
        };
        textList.AddChild(labelDynamic);
        _bottomText = labelDynamic;

        var buttonList = new UIBaseWindow()
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0),
            AnchorAndParentAnchor = UIAnchor.CenterRight,
            Margins = new Rectangle(5, 5, 5, 5),
        };
        barContent.AddChild(buttonList);
        _bottomBarToolButtons = buttonList;

        for (int i = 0; i < Tools.Length; i++)
        {
            TileEditorTool tool = Tools[i];
            buttonList.AddChild(new TileEditorToolButton(this, tool));
        }

        TilesetsChanged();

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

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);


    }

    protected override bool UpdateInternal()
    {
        UpdateCursor();
        if (_mouseDown && CursorTilePos != null)
            PaintCurrentTile();

        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.SetUseViewMatrix(true);

        // Render current layer bounds
        if (CurrentLayer != null)
        {
            Vector2 tileSize = CurrentLayer.TileSize;

            Vector2 renderOffset = (CurrentLayer.RenderOffsetInTiles * tileSize) - tileSize / 2f;
            Vector2 sizeWorldSpace = CurrentLayer.SizeInTiles * tileSize;
            c.RenderOutline(renderOffset.ToVec3(), sizeWorldSpace, Color.PrettyRed * 0.5f, 4 * GetScale());
        }

        // Render cursor
        if (CursorTilePos != null && CurrentLayer != null)
        {
            Vector2 cursorTile = CursorTilePos.Value;
            Vector2 tileInWorld = CurrentLayer.GetWorldPosOfTile(cursorTile);
            Vector2 tileSize = CurrentLayer.TileSize;

            CurrentTool.RenderCursor(c, this);
        }

        c.SetUseViewMatrix(false);

        return base.RenderInternal(c);
    }

    private void UpdateCursor()
    {
        CursorTilePos = null;

        if (CurrentLayer == null || !MouseInside)
        {
            if (_bottomText != null)
                _bottomText.Text = "";
            return;
        }

        Vector3 worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
        Vector2 tilePos = CurrentLayer.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());

        // Correct this for .ToString() reasons
        if (tilePos.X == -0) tilePos.X = 0;
        if (tilePos.Y == -0) tilePos.Y = 0;

        CursorTilePos = tilePos;

        bool inMap = CurrentLayer.IsPositionInMap(tilePos);
        string inMapText = "";
        if (!inMap) inMapText = " (Outside Map)";

        if (_bottomText != null)
            _bottomText.Text = $"Rollover Tile - {tilePos}{inMapText}";
    }

    public void SetCurrentTool(TileEditorTool currentTool)
    {
        CurrentTool = currentTool;

        if (_bottomBarToolButtons == null) return;
        foreach (UIBaseWindow child in _bottomBarToolButtons)
        {
            if (child is TileEditorToolButton toolButton)
            {
                toolButton.UpdateStyle();
            }
        }
    }

    private GameMap? GetCurrentMap()
    {
        GameMap? currentMap = null;
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap)
            currentMap = sceneWithMap.Map;
        return currentMap;
    }

    private GameMapTileData? GetCurrentMapTileData()
    {
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap && sceneWithMap.Map != null && sceneWithMap.Map.TileMapData != null)
            return sceneWithMap.Map.TileMapData;
        return null;
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            _mouseDown = status == KeyState.Down;

            // Clear last action to group undos by mouse clicks.
            //if (!_mouseDown) _lastAction = null;
        }

        return base.OnKey(key, status, mousePos);
    }

    #region TileLayer

    public IEnumerable<TileMapLayerGrid> GetTileLayers()
    {
        GameMapTileData? tileData = GetCurrentMapTileData();
        if (tileData == null) return Array.Empty<TileMapLayerGrid>();

        return tileData.Layers;
    }

    public void SelectTileLayer(TileMapLayerGrid? tileLayer)
    {
        if (tileLayer == null)
        {
            CurrentLayer = null;
            EngineEditor.SetGridSize(0);
            return;
        }

        bool found = false;
        foreach (var mapLayer in GetTileLayers())
        {
            if (mapLayer == tileLayer)
            {
                found = true;
                break;
            }
        }
        Assert(found, "Currently selected tile layer doesn't exist in the current map.");

        EngineEditor.SetGridSize(tileLayer.TileSize.X);
        CurrentLayer = tileLayer;
    }

    #endregion


    #region Tilesets

    private void TilesetsChanged()
    {
        _tilesetChoose.SetEditor(GetTilesets(), CurrentTileset);
    }

    public IEnumerable<TileMapTileset> GetTilesets()
    {
        GameMapTileData? tileData = GetCurrentMapTileData();
        if (tileData == null) return Array.Empty<TileMapTileset>();

        return tileData.Tilesets;
    }

    public void SelectTileset(TileMapTileset? tileset)
    {
        if (_tileTextureSelector == null)
        {
            CurrentTileset = null;
            _tileTextureSelector?.SetTileset(null);
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

        _tileTextureSelector?.SetTileset(tileset);
        CurrentTileset = tileset;
    }

    #endregion

    #region Paint

    public void PaintCurrentTile()
    {
        AssertNotNull(_tileTextureSelector);
        if (_tileTextureSelector == null) return;

        GameMapTileData? tileData = GetCurrentMapTileData();
        if (tileData == null) return;

        if (CurrentLayer == null) return;

        if (CursorTilePos == null) return;
        Vector2 cursorPos = CursorTilePos.Value;

        //_mouseDown = false;

        (TileTextureId, Vector2)[] placementPattern = _tileTextureSelector.GetSelectedTileTextures(out Vector2 center);

        Vector2 tileSize = CurrentLayer.TileSize;
        Vector2 centerInWorldSpace = (center / tileSize).Floor() * tileSize;

        for (int i = 0; i < placementPattern.Length; i++)
        {
            (TileTextureId, Vector2) data = placementPattern[i];
            TileTextureId tileToPlace = data.Item1;
            Vector2 tileToPlaceOffset = data.Item2 - center;

            Vector2 thisTilePos = cursorPos + (tileToPlaceOffset / tileSize);

            // todo: current tool
            bool success = CurrentLayer.EditorSetTileAt(thisTilePos, new TileMapTile(tileToPlace, 0), out bool layerBoundsChanged);
            if (success)
            {
                if (layerBoundsChanged)
                {
                    tileData.EditorUpdateRenderCacheForLayer(CurrentLayer);
                    UpdateCursor();
                }
                else
                {
                    tileData.EditorUpdateRenderCacheForTile(CurrentLayer, thisTilePos);
                }
            }
        }
    }

    #endregion
}
