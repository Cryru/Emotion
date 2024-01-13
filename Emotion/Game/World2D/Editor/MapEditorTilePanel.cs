#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World.Editor;
using Emotion.Game.World2D.Tile;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public enum TileEditorTool
{
    Brush,
    Eraser,
    Bucket,

    // Pointer = todo: allows copy paste of sections
    // ColorPicker = todo: tile picker from the selected layer :)
}

public class MapEditorTilePanel : EditorPanel
{
    public TileEditorTool CurrentTool = TileEditorTool.Brush;

    private Map2D _map;
    private EditorListOfItemsWithSelection<Map2DTileMapLayer> _layerList = null!;
    private EditorButtonDropDown _tileSetList = null!;
    private Map2DTileMapLayer? _currentLayer;
    private Map2DTileset? _currentTileset;
    private TilesetTileSelector? _tileSelector;

    public MapEditorTilePanel(Map2D map) : base("Tile Editor")
    {
        _map = map;
        UseNewLayoutSystem = false;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        MapEditorPanelTopBar? topBar = GetWindowById("TopBar") as MapEditorPanelTopBar;
        if (topBar != null) topBar.CanMove = false;

        // todo: with new ui this can go between top bar and bottom bar snuggly.
        _centered = false;
        _container.Anchor = UIAnchor.TopRight;
        _container.ParentAnchor = UIAnchor.TopRight;
        _container.StretchY = true;
        _container.StretchX = true;
        _container.MaxSizeY = Engine.Renderer.CurrentTarget.Size.Y;
        _container.Margins = new Rectangle(0, 15, 0, 10);

        var innerContainer = new UIBaseWindow();
        innerContainer.ListSpacing = new Vector2(0, 1);
        innerContainer.LayoutMode = LayoutMode.VerticalList;
        innerContainer.UseNewLayoutSystem = true;
        innerContainer.MinSizeX = 190f;
        innerContainer.MaxSizeX = 190f;
        _contentParent.StretchY = false;
        _contentParent.AddChild(innerContainer);

        var editTileData = new EditorButton("Edit TileMap Properties");
        editTileData.StretchY = true;
        editTileData.OnClickedProxy = _ =>
        {
            var propPanel = new GenericPropertiesEditorPanel(_map.Tiles);
            propPanel.OnPropertyEdited = (propName, propValue) =>
            {
                _map.Tiles.InitRuntimeState(_map).Wait();

                // Refresh lists by re-setting items.
                int currentLayer = _currentLayer != null ? _map.Tiles.Layers.IndexOf(_currentLayer) : 0;
                _layerList.SetItems(_map.Tiles.Layers, new List<int> { currentLayer });

                int currentTileset = _currentTileset != null ? _map.Tiles.Tilesets.IndexOf(_currentTileset) : 0;
                _tileSetList.SetItems(_map.Tiles.Tilesets, currentTileset);
            };
            Controller!.AddChild(propPanel);
        };
        innerContainer.AddChild(editTileData);

        var layerLabel = new MapEditorLabel("Layers:");
        innerContainer.AddChild(layerLabel);

        var layerListContainer = new UIBaseWindow();
        layerListContainer.FillXInList = true;
        layerListContainer.StretchX = true;
        layerListContainer.StretchY = true;
        layerListContainer.ListSpacing = new Vector2(0, 1);
        innerContainer.AddChild(layerListContainer);

        var background = new UISolidColor();
        background.WindowColor = Color.Black * 0.5f;
        layerListContainer.AddChild(background);

        // todo: reverse order.
        var layerList = new EditorListOfItemsWithSelection<Map2DTileMapLayer>();
        layerList.OnSelectionChanged = (i, item, selected) =>
        {
            if (selected) _currentLayer = item;
        };
        layerList.SetItems(_map.Tiles.Layers);
        layerList.AllowMultiSelect = true;
        layerList.MinSizeY = 99;
        layerList.MaxSizeY = 99;
        _layerList = layerList;
        layerListContainer.AddChild(layerList);

        var tileSetList = new EditorButtonDropDown();
        tileSetList.Text = "Tileset:";
        tileSetList.OnSelectionChanged = (i, uiItem) =>
        {
            var tileset = uiItem.UserData as Map2DTileset;
            _currentTileset = tileset;

            var tileSelector = GetWindowById("TileSelector") as TilesetTileSelector;
            tileSelector?.SetTileset(_currentTileset);
        };
        tileSetList.SetItems(_map.Tiles.Tilesets);
        _tileSetList = tileSetList;
        innerContainer.AddChild(tileSetList);

        var tileSetControls = new UIBaseWindow();
        tileSetControls.LayoutMode = LayoutMode.HorizontalList;
        tileSetControls.ListSpacing = new Vector2(2, 0);
        innerContainer.AddChild(tileSetControls);

        var tilesetPropertiesButton = new EditorButton("Tileset Metadata");
        tileSetControls.AddChild(tilesetPropertiesButton);

        var tilesetScaleLabel = new MapEditorLabel("");
        tilesetScaleLabel.Margins = new Rectangle(1, 0, 0, 0);
        tileSetControls.AddChild(tilesetScaleLabel);

        void UpdateTilesetScaleLabel()
        {
            if (_tileSelector == null) return;
            tilesetScaleLabel.Text = $"Scale: {_tileSelector.Scale * 100f}%";
        }

        var scalePlus = new EditorButton("+");
        scalePlus.OnClickedProxy = (_) =>
        {
            if (_tileSelector != null) _tileSelector.Scale += 0.5f;
            UpdateTilesetScaleLabel();
        };
        tileSetControls.AddChild(scalePlus);

        var scaleMinus = new EditorButton("-");
        scaleMinus.OnClickedProxy = (_) =>
        {
            if (_tileSelector != null && _tileSelector.Scale > 0.5f) _tileSelector.Scale -= 0.5f;
            UpdateTilesetScaleLabel();
        };
        tileSetControls.AddChild(scaleMinus);

        var tileSelector = new TilesetTileSelector(_map.Tiles);
        tileSelector.Id = "TileSelector";
        tileSelector.SetTileset(_currentTileset);
        tileSelector.FillXInList = true;
        tileSelector.MinSizeY = 170; // temp;
        tileSelector.MaxSizeY = 170; // temp;
        _tileSelector = tileSelector;
        innerContainer.AddChild(tileSelector);

        GenerateToolWindow();
        UpdateTilesetScaleLabel();
    }

    public Map2DTileMapLayer? GetLayer()
    {
        return _currentLayer;
    }

    public bool AreMultipleTilesSelected()
    {
        if (_tileSelector == null) return false;
        return _tileSelector.SelectedTiles.Count > 1;
    }

    public uint GetTidToPlace()
    {
        if (_tileSelector == null || _tileSelector.SelectedTiles.Count == 0) return 0;
        return _tileSelector.SelectedTiles[0];
    }

    public (uint, Vector2)[]? GetTidToPlaceMultiPattern(out Vector2 center)
    {
        center = Vector2.Zero;
        if (_tileSelector == null || _tileSelector.SelectedTiles.Count == 0) return null;

        Map2DTileMapData tileData = _map.Tiles;
        var tsId = tileData.GetTsIdFromTilesetRef(_currentTileset);
        if (tsId == -1) return null;

        (uint, Vector2)[] pattern = new (uint, Vector2)[_tileSelector.SelectedTiles.Count];
        Vector2 originPos = Vector2.Zero;
        for (int i = 0; i < _tileSelector.SelectedTiles.Count; i++)
        {
            var tId = _tileSelector.SelectedTiles[i];
            var tileCoord = tileData.GetUVFromTileImageIdAndTileset(tId, tsId);

            if (i == 0)
            {
                originPos = tileCoord.Position;
                pattern[i] = (tId, Vector2.Zero);
                continue;
            }
            Vector2 offsetFromOriginInTiles = tileCoord.Position - originPos;
            center += offsetFromOriginInTiles;
            pattern[i] = (tId, offsetFromOriginInTiles);
        }
        center /= _tileSelector.SelectedTiles.Count;

        return pattern;
    }

    private void GenerateToolWindow()
    {
        var toolWindow = new UIBaseWindow();
        toolWindow.UseNewLayoutSystem = true;
        toolWindow.ParentAnchor = UIAnchor.TopLeft;
        toolWindow.Anchor = UIAnchor.TopRight;
        toolWindow.FillX = false;
        toolWindow.FillY = false;
        _container.AddChild(toolWindow);

        var toolWindowBG = new UISolidColor();
        toolWindowBG.WindowColor = MapEditorColorPalette.BarColor;
        toolWindow.AddChild(toolWindowBG);

        var toolWindowList = new UIBaseWindow();
        toolWindowList.LayoutMode = LayoutMode.VerticalList;
        toolWindowList.ListSpacing = new Vector2(0, 1);
        toolWindow.AddChild(toolWindowList);

        var selectedColor = new Color(210, 210, 210);
        var normalColor = new Color(140, 140, 140);
        var rolloverColor = new Color(170, 170, 170);

        UISolidColor? previousSelectedTool = null;

        var tools = Enum.GetNames(typeof(TileEditorTool));
        var toolsEnumVal = Enum.GetValues<TileEditorTool>();
        for (int i = 0; i < tools.Length; i++)
        {
            var myIdx = i;
            var myTool = toolsEnumVal[i];

            var window = new UICallbackButton();

            bool isSelected = CurrentTool == myTool;
            var windowBackground = new UISolidColor();
            windowBackground.WindowColor = isSelected ? selectedColor : normalColor;
            window.AddChild(windowBackground);
            if (isSelected) previousSelectedTool = windowBackground;

            window.OnClickedProxy = (_) =>
            {
                CurrentTool = myTool;

                if (previousSelectedTool != null) previousSelectedTool.WindowColor = normalColor;
                previousSelectedTool = windowBackground;

                windowBackground.WindowColor = selectedColor;
            };
            window.OnMouseEnterProxy = (_) =>
            {
                bool isSelected = CurrentTool == myTool;
                windowBackground.WindowColor = isSelected ? selectedColor : rolloverColor;
            };
            window.OnMouseLeaveProxy = (_) =>
            {
                bool isSelected = CurrentTool == myTool;
                windowBackground.WindowColor = isSelected ? selectedColor : normalColor;
            };
            window.Paddings = new Rectangle(1, 1, 1, 1);

            var iconUI = new UITexture();
            iconUI.TextureFile = $"Editor/{tools[i]}.png";
            iconUI.ImageScale = new Vector2(0.5f);
            window.AddChild(iconUI);

            toolWindowList.AddChild(window);
        }
    }
}
