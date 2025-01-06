using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Grid;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.Tile;
using Emotion.Scenography;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;

#nullable enable

namespace Emotion.WIPUpdates.One.Editor2D.TileEditor;

public class TileEditorWindow : UIBaseWindow
{
    public TileEditorTool? CurrentTool { get; private set; } = null;

    public TileEditorTool _previousPlacingTool = TileEditorTool.Brush;

    private Vector2? _cursorTilePos;
    private TileDataLayerGrid? _currentLayer;

    private UIRichText? _bottomText;

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

        var layers = new UIBaseWindow()
        {
            Anchor = UIAnchor.BottomRight,
            ParentAnchor = UIAnchor.BottomRight,
            Id = "Layers"
        };
        barContent.AddChild(layers);

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

        TileEditorTool[] toolsEnumVal = Enum.GetValues<TileEditorTool>();
        for (int i = 0; i < toolsEnumVal.Length; i++)
        {
            TileEditorTool myTool = toolsEnumVal[i];
            buttonList.AddChild(new TileEditorToolButton(this, myTool));
        }

        foreach (var layer in GetTileLayers())
        {
            SelectTileLayer(layer);
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

        if (_cursorTilePos != null && _currentLayer != null)
        {
            Vector2 cursorTile = _cursorTilePos.Value;
            Vector2 tileInWorld = _currentLayer.GetWorldPosOfTile(cursorTile);

            c.RenderSprite(tileInWorld, _currentLayer.TileSize, Color.Black * 0.3f);
            c.RenderOutline(tileInWorld.ToVec3(), _currentLayer.TileSize, Color.Black);
            c.RenderCircle(new Vector3(0, 0, 0), 10, Color.Red, true);
        }

        c.SetUseViewMatrix(false);

        return base.RenderInternal(c);
    }

    private void UpdateCursor()
    {
        _cursorTilePos = null;

        if (!MouseInside) return;
        if (_currentLayer == null) return;

        Vector3 worldSpaceMousePos = Engine.Renderer.Camera.ScreenToWorld(Engine.Host.MousePosition);
        Vector2 tilePos = _currentLayer.GetTilePosOfWorldPos(worldSpaceMousePos.ToVec2());
        _cursorTilePos = tilePos;

        GameMap? map = GetCurrentMap();

        bool inMap = _currentLayer.IsPositionInMap(tilePos);
        string inMapText = "";
        if (!inMap) inMapText = " (Outside Map)";

        if (_bottomText != null)
            _bottomText.Text = $"Rollover Tile - {tilePos}{inMapText}";
    }

    public void SetCurrentTool(TileEditorTool currentTool)
    {

    }

    private GameMap? GetCurrentMap()
    {
        GameMap? currentMap = null;
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap)
            currentMap = sceneWithMap.Map;
        return currentMap;
    }

    public IEnumerable<TileDataLayerGrid> GetTileLayers()
    {
        GameMap? map = GetCurrentMap();
        if (map == null) yield break;

        foreach (var grid in map.Grids)
        {
            if (grid is TileDataLayerGrid tileData) yield return tileData;
        }
    }

    public void SelectTileLayer(TileDataLayerGrid? tileLayer)
    {
        if (tileLayer == null)
        {
            _currentLayer = null;
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
        _currentLayer = tileLayer;
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
