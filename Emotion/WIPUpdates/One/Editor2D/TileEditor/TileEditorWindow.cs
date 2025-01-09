using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Tile;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;
using Emotion.WIPUpdates.One.EditorUI.Helpers;
using Emotion.WIPUpdates.One.TileMap;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor;

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
    private TileEditorTilesetSelector? _tilesetSelector;

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

            var dropDown = new UISolidColor()
            {
                WindowColor = Color.PrettyGreen,
                FillY = false,
            };
            sidePanel.AddChild(dropDown);

            var tilesetTileSelector = new TileEditorTilesetSelector(this)
            {
                Id = "TileSelector",
                
                MaxSizeX = 400, // temp
                MinSizeY = 450,
                MaxSizeY = 450
            };
            _tilesetSelector = tilesetTileSelector;
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




        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        c.SetUseViewMatrix(true);

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
        CursorTilePos = tilePos;

        GameMap? map = GetCurrentMap();

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

    public IEnumerable<TileMapLayerGrid> GetTileLayers()
    {
        GameMap? map = GetCurrentMap();
        if (map == null) return Array.Empty<TileMapLayerGrid>();
        if (map.TileMapData == null) return Array.Empty<TileMapLayerGrid>();

        return map.TileMapData.Layers;
    }

    public IEnumerable<TileMapTileset> GetTilesets()
    {
        GameMap? map = GetCurrentMap();
        if (map == null) return Array.Empty<TileMapTileset>();
        if (map.TileMapData == null) return Array.Empty<TileMapTileset>();

        return map.TileMapData.Tilesets;
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

    public void SelectTileset(TileMapTileset? tileset)
    {
        if (_tilesetSelector == null)
        {
            CurrentTileset = null;
            _tilesetSelector?.SetTileset(null);
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

        _tilesetSelector?.SetTileset(tileset);
        CurrentTileset = tileset;
    }

    //protected void UpdateTileEditor()
    //{
    //    _cursorPos = null;
    //    if (!IsTileEditorOpen()) return;

    //    bool mouseInUI = UIController.MouseFocus != null && UIController.MouseFocus != _editUI;
    //    if (mouseInUI) return;

    //    var worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);

    //    var tileData = GetMapTileData();
    //    if (tileData == null) return;

    //    Vector2 tilePos = tileData.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());
    //    _cursorPos = tilePos;

    //    MapEditorTilePanel? editor = _tileEditor as MapEditorTilePanel;
    //    AssertNotNull(editor);
    //    Map2DTileMapLayer? selectedLayer = editor.GetLayer();
    //    var tileBrush1D = tileData.GetTile1DFromTile2D(tilePos);
    //    uint tId = tileData.GetTileData(selectedLayer, tileBrush1D);

    //    if (_bottomBarText != null)
    //        _bottomBarText.Text = $"Rollover ({tilePos}) TId - {(tId == 0 ? "0 (Empty)" : tId.ToString())}";

    //    if (_mouseDown)
    //        PaintCurrentTile();
    //}
}
