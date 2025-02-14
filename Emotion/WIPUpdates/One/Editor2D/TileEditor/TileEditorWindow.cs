﻿#nullable enable

using Emotion.Common.Serialization;
using Emotion.Game.World.Editor;
using Emotion.Platform.Input;
using Emotion.Scenography;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.Editor2D.TileEditor.Tools;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.WIPUpdates.One.TileMap;

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

    private HashSet<Vector2> _preciseDrawDedupe = new();

    public TileMapLayer? CurrentLayer { get; private set; }

    public TileMapTileset? CurrentTileset { get; private set; }

    private TileEditorTool _lastUsedPlacingTool = Tools[0];

    private UIBaseWindow? _bottomBarToolButtons;
    private UIRichText? _bottomText;
    public TileEditorTileTextureSelector TileTextureSelector = null!;

    private DropdownChoiceEditor<TileMapTileset>? _tilesetChoose;

    private bool _mouseDown;

    public TileEditorWindow()
    {
        HandleInput = true;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        // temp
        controller.SetInputFocus(this);
    }

    public void SpawnBottomBarContent(Editor2DBottomBar bar, UIBaseWindow barContent)
    {
        var sidePanel = new UIBaseWindow()
        {
            Id = "TileEditorSidePanel",
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
            WindowColor = MapEditorColorPalette.BarColor,
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
            ComplexTypeHandlerMember? layerHandler = tileDataTypeHandler?.GetMemberByName(nameof(tileData.Layers));
            if (tileData != null && layerHandler != null)
            {
                var listEditor = new ListEditor<TileMapLayer>();
                listEditor.OnItemSelected = SelectTileLayer;

                EditorWithLabel layerEditor = new EditorWithLabel(listEditor, tileData, layerHandler);
                layerEditor.MinSizeY = 350;
                layerEditor.MaxSizeY = 350;
                layerEditor.SetVertical();

                sidePanel.AddChild(layerEditor);
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
                Id = "TileSelector",

                //MaxSizeX = 400, // temp
                MinSizeY = 450,
                MaxSizeY = 450
            };
            TileTextureSelector = tilesetTileSelector;
            sidePanel.AddChild(tilesetTileSelector);
        }

        // Bottom text
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
        }

        // Tool buttons
        {
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
        c.SetUseViewMatrix(true);

        // Render current layer bounds
        if (CurrentLayer != null)
        {
            foreach (Rectangle chunkBound in CurrentLayer.ForEachLoadedChunkBound())
            {
                c.RenderOutline(chunkBound, Color.Black * 0.5f, 4 * GetScale());
            }
        }

        // Render cursor
        if (MouseInside)
        {
            if (CursorTilePos != null && CurrentLayer != null)
                CurrentTool.RenderCursor(c, this, CurrentLayer, CursorTilePos.Value);
        }

        c.SetUseViewMatrix(false);

        return base.RenderInternal(c);
    }


    #region Using Tools

    private void UpdateCurrentTool(Vector2 previousPos, Vector2 newPos)
    {
        UpdateCursor();

        if (!MouseInside) return;
        if (!_mouseDown) return;
        if (CurrentLayer == null) return;
        if (CursorTilePos == null) return;

        if (CurrentTool.RequireTileSelection)
        {
            AssertNotNull(TileTextureSelector);
            if (TileTextureSelector == null) return;
            (TileTextureId, Vector2)[] currentlySelectedTile = TileTextureSelector.GetSelectedTileTextures(out _);
            if (currentlySelectedTile[0].Item1 == TileMapTile.Empty) return;
        }

        previousPos = Engine.Renderer.Camera.ScreenToWorld(previousPos).ToVec2();
        newPos = Engine.Renderer.Camera.ScreenToWorld(newPos).ToVec2();

        LineSegment moveSegment = new LineSegment(previousPos, newPos);
        float moveSegmentLength = moveSegment.Length();
        if (!CurrentTool.IsPrecisePaint || moveSegmentLength == 0)
            CurrentTool.ApplyTool(this, CurrentLayer, CursorTilePos.Value);
        else
            PrecisePaintApplyTool(moveSegment);
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

            CurrentTool.ApplyTool(this, CurrentLayer, tile);
        }
    }

    public void UpdateCursor()
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

        bool inMap = CurrentLayer.IsValidPosition(tilePos);
        string inMapText = "";
        if (!inMap) inMapText = " (Outside Map)";

        if (_bottomText != null)
            _bottomText.Text = $"Rollover Tile - {tilePos}{inMapText}";
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
            EngineEditor.SetGridSize(0);
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

        EngineEditor.SetGridSize(tileLayer.TileSize.X);
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
