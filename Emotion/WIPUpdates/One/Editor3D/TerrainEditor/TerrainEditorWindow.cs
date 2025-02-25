#nullable enable

using Emotion.Common.Serialization;
using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.Scenography;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D;
using Emotion.WIPUpdates.One.Editor2D.TileEditor;
using Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.TileMap;
using Emotion.WIPUpdates.ThreeDee;
using System;
using System.Numerics;
using System.Text;

namespace Emotion.WIPUpdates.One.Editor3D.TerrainEditor;

[DontSerialize]
public sealed class TerrainEditorWindow : UIBaseWindow
{
    public static TileEditorTool[] Tools =
    {
        new TileEditorBrushTool(),
    };

    public TileEditorTool CurrentTool = Tools[0];

    public Vector2? CursorTilePos { get; private set; }

    private HashSet<Vector2> _preciseDrawDedupe = new();

    public TileMapLayer? CurrentLayer { get; private set; }

    public TileMapTileset? CurrentTileset { get; private set; }

    private TileEditorTool _lastUsedPlacingTool = Tools[0];

    private UIBaseWindow? _bottomBarToolButtons;
    private UIRichText? _bottomText;

    private bool _mouseDown;
    private float _brushSize = 100;

    public TerrainEditorWindow()
    {
        HandleInput = true;
        OrderInParent = -1;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        // temp
        controller.SetInputFocus(this);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);

        GameMap? map = EngineEditor.GetCurrentMap();
        TerrainMeshGrid? terrain = map?.TerrainGrid;
        terrain?.SetEditorBrush(false, 0);
    }

    private Vector2 _previousMousePos = new Vector2(-1);

    public override void OnMouseMove(Vector2 mousePos)
    {
        UpdateCurrentTool(_previousMousePos, mousePos);
        _previousMousePos = mousePos;
    }

    protected override bool UpdateInternal()
    {
        return base.UpdateInternal();
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        return base.RenderInternal(c);
    }

    public void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        {
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
                Text = "Terrain Editor",
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
        }
    }

    #region Using Tools

    private void UpdateCurrentTool(Vector2 previousPos, Vector2 newPos)
    {
        UpdateCursor();

        if (!MouseInside) return;
        if (!_mouseDown) return;
        if (CursorTilePos == null) return;

        //if (CurrentTool.RequireTileSelection)
        //{
        //    AssertNotNull(TileTextureSelector);
        //    if (TileTextureSelector == null) return;
        //    (TileTextureId, Vector2)[] currentlySelectedTile = TileTextureSelector.GetSelectedTileTextures(out _);
        //    if (currentlySelectedTile[0].Item1 == TileMapTile.Empty) return;
        //}

        previousPos = Engine.Renderer.Camera.ScreenToWorld(previousPos).ToVec2();
        newPos = Engine.Renderer.Camera.ScreenToWorld(newPos).ToVec2();

        GameMap? map = EngineEditor.GetCurrentMap();
        TerrainMeshGrid? terrain = map?.TerrainGrid;
        if (terrain == null) return;

        Vector2 cursorPos = terrain.GetEditorBrushWorldPosition();
        Vector2 cursorPosInGrid = terrain.GetTilePosOfWorldPos(cursorPos);

        for (int i = 0; i < _brushGrid.Length; i++)
        {
            BrushGrid tileInfo = _brushGrid[i];
            float influence = tileInfo.Influence;
            if (influence == 0) continue;

            Vector2 tileCoord = tileInfo.TileCoord;

            float brushStrength = 5 * influence;

            float val = terrain.GetAt(tileCoord);
            terrain.ExpandingSetAt(tileCoord, val + brushStrength);
        }

        // Smooth
        //float averageVal = 0;
        //int values = 0;
        //for (int i = 0; i < _brushGrid.Length; i++)
        //{
        //    BrushGrid tileInfo = _brushGrid[i];
        //    float influence = tileInfo.Influence;
        //    if (influence == 0) continue;

        //    Vector2 tileCoord = tileInfo.TileCoord;

        //    float val = terrain.GetAt(tileCoord);
        //    averageVal += val;
        //    values++;
        //}

        //averageVal = averageVal / values;
        //for (int i = 0; i < _brushGrid.Length; i++)
        //{
        //    BrushGrid tileInfo = _brushGrid[i];
        //    float influence = tileInfo.Influence;
        //    if (influence == 0) continue;

        //    Vector2 tileCoord = tileInfo.TileCoord;
        //    float val = terrain.GetAt(tileCoord);

        //    float brushStrength = 5 * influence;

        //    float diff = averageVal - val;
        //    float diffAbs = MathF.Abs(diff);
        //    val += MathF.Min(diffAbs, brushStrength) * MathF.Sign(diff);
        //    terrain.ExpandingSetAt(tileCoord, val);
        //}

        //LineSegment moveSegment = new LineSegment(previousPos, newPos);
        //float moveSegmentLength = moveSegment.Length();
        //if (!CurrentTool.IsPrecisePaint || moveSegmentLength == 0)
        //    CurrentTool.ApplyTool(this, CurrentLayer, CursorTilePos.Value);
        //else
        //    PrecisePaintApplyTool(moveSegment);
    }

    private void PrecisePaintApplyTool(LineSegment moveSegment)
    {
        AssertNotNull(CurrentLayer);

        // Draw as a line
        float moveSegmentLength = moveSegment.Length();
        _preciseDrawDedupe.Clear();
        for (float i = 0; i < moveSegmentLength; i += 0.5f)
        {
            Vector2 pointAtLineSegment = moveSegment.PointOnLineAtDistance(i);
            Vector2 tile = CurrentLayer.GetTilePosOfWorldPos(pointAtLineSegment);
            if (!_preciseDrawDedupe.Add(tile)) continue;

            //CurrentTool.ApplyTool(this, CurrentLayer, tile);
        }
    }

    public struct BrushGrid
    {
        public Vector2 TileCoord;
        public float Influence;
    }

    private BrushGrid[] _brushGrid = Array.Empty<BrushGrid>();

    public void UpdateCursor()
    {
        GameMap? map = EngineEditor.GetCurrentMap();
        TerrainMeshGrid? terrain = map?.TerrainGrid;

        if (terrain == null || !MouseInside)
        {
            if (_bottomText != null)
                _bottomText.Text = "";
            return;
        }

        terrain.SetEditorBrush(true, _brushSize);

        // Construct brush grid
        Vector2 tileSize = terrain.TileSize;
        Vector2 brushPosWorld = terrain.GetEditorBrushWorldPosition();

        Rectangle brushRect = new Rectangle(0, 0, new Vector2(_brushSize * 2));
        brushRect.Center = brushPosWorld - tileSize / 2f;

        Rectangle brushRectSnapped = brushRect;
        brushRectSnapped.SnapToGrid(tileSize);
        brushRectSnapped.GetMinMaxPoints(out Vector2 min, out Vector2 max);

        min /= tileSize;
        max /= tileSize;

        min = min.Round();
        max = max.Round();

        int brushRectSizeY = (int)(max.Y - min.Y);
        int brushRectSizeX = (int)(max.X - min.X);

        int brushInfluenceSize = brushRectSizeX * brushRectSizeY;
        if (brushInfluenceSize > _brushGrid.Length)
            Array.Resize(ref _brushGrid, brushInfluenceSize);

        int tile = 0;
        for (float y = min.Y; y < max.Y; y++)
        {
            for (float x = min.X; x < max.X; x++)
            {
                Vector2 tileCoord = new Vector2(x, y);
                Vector2 tileWorldPos = terrain.GetWorldPosOfTile(tileCoord);

                float distToTile = Vector2.Distance(tileWorldPos, brushPosWorld);
                float falloff = MathF.Exp(-MathF.Pow(distToTile, 2) / (2 * MathF.Pow(_brushSize * 0.5f, 2)));
                falloff = MathF.Max(falloff, 0f);

                _brushGrid[tile] = new BrushGrid()
                {
                    Influence = falloff,
                    TileCoord = tileCoord
                };
                tile++;
            }
        }

        Vector2 tilePos = terrain.GetTilePosOfWorldPos(brushPosWorld);

        // Correct this for .ToString() reasons
        if (tilePos.X == -0) tilePos.X = 0;
        if (tilePos.Y == -0) tilePos.Y = 0;

        CursorTilePos = tilePos;

        if (_bottomText != null)
            _bottomText.Text = $"Rollover Tile - {tilePos}";
    }

    public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
    {
        if (key == Key.MouseKeyLeft)
        {
            _mouseDown = status == KeyState.Down;

            // Instantly responsive on the mouse click event, don't wait for update
            if (_mouseDown) UpdateCurrentTool(mousePos, mousePos);

            // Clear last action to group undos by mouse clicks.
            //if (!_mouseDown) _lastAction = null;
        }

        if (status == KeyState.Down)
        {
            for (int i = 0; i < Tools.Length; i++)
            {
                TileEditorTool tool = Tools[i];
                if (key == tool.HotKey)
                {
                    SetCurrentTool(tool);
                    break;
                }
            }
        }

        return base.OnKey(key, status, mousePos);
    }

    #endregion

    public void SetCurrentTool(TileEditorTool currentTool)
    {
        CurrentTool = currentTool;
        if (currentTool.IsPlacingTool) _lastUsedPlacingTool = CurrentTool;

        if (_bottomBarToolButtons == null) return;
        foreach (UIBaseWindow child in _bottomBarToolButtons)
        {
            if (child is TileEditorToolButton toolButton)
            {
                toolButton.UpdateStyle();
            }
        }
    }

    public void SetCurrentToolAsLastPlacingTool()
    {
        SetCurrentTool(_lastUsedPlacingTool);
    }

    public GameMapTileData? GetCurrentMapTileData()
    {
        if (Engine.SceneManager.Current is SceneWithMap sceneWithMap && sceneWithMap.Map != null && sceneWithMap.Map.TileMapData != null)
            return sceneWithMap.Map.TileMapData;
        return null;
    }
}
