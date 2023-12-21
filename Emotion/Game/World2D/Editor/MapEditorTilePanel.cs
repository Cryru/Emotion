#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World2D.Tile;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public class MapEditorTilePanel : EditorPanel
{
    private Map2D _map;
    private ItemListWithActions<Map2DTileMapLayer> _layerList = null!;
    private ItemListWithActions<Map2DTileset> _tileSetList = null!;
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
        _container.Margins = new Rectangle(0, 14, 0, 11);

        var innerContainer = new UIBaseWindow();
        innerContainer.StretchX = true;
        innerContainer.StretchY = true;
        innerContainer.LayoutMode = LayoutMode.VerticalList;
        _contentParent.AddChild(innerContainer);

        var editTileData = new EditorButton("Edit Meta");
        editTileData.StretchY = true;
        editTileData.OnClickedProxy = _ =>
        {
            var propPanel = new GenericPropertiesEditorPanel(_map.Tiles);
            propPanel.OnPropertyEdited = (propName, propValue) =>
            {
                _map.Tiles.InitRuntimeState(_map).Wait();
                _layerList.RefreshListUI();
                _tileSetList.RefreshListUI();
            };
            Controller!.AddChild(propPanel);
        };
        innerContainer.AddChild(editTileData);

        var topPartContainer = new UIBaseWindow();
        topPartContainer.StretchY = true;
        topPartContainer.StretchX = true;
        topPartContainer.LayoutMode = LayoutMode.HorizontalList;
        topPartContainer.Id = "TopPartContainer";
        topPartContainer.MaxSizeY = 150;
        innerContainer.AddChild(topPartContainer);

        var layerListContainer = new UIBaseWindow();
        layerListContainer.LayoutMode = LayoutMode.VerticalList;
        layerListContainer.StretchX = true;
        layerListContainer.StretchY = true;
        layerListContainer.ListSpacing = new Vector2(0, 1);
        topPartContainer.AddChild(layerListContainer);

        var layerLabel = new MapEditorLabel("Layers:");
        layerListContainer.AddChild(layerLabel);

        // todo: reverse order.
        var layerList = new ItemListWithActions<Map2DTileMapLayer>();
        layerList.OnSelectionChanged = LayerSelectionChanged;
        layerList.SetItems(_map.Tiles.Layers);
        _layerList = layerList;
        layerListContainer.AddChild(layerList);

        var bottomPartContainer = new UIBaseWindow();
        bottomPartContainer.StretchY = true;
        bottomPartContainer.StretchX = true;
        bottomPartContainer.LayoutMode = LayoutMode.HorizontalList;
        bottomPartContainer.Id = "BottomPartContainer";
        innerContainer.AddChild(bottomPartContainer);

        var tileSetListContainer = new UIBaseWindow();
        tileSetListContainer.LayoutMode = LayoutMode.VerticalList;
        tileSetListContainer.StretchX = true;
        tileSetListContainer.StretchY = true;
        tileSetListContainer.ListSpacing = new Vector2(0, 1);
        bottomPartContainer.AddChild(tileSetListContainer);

        var tileSetLabel = new MapEditorLabel("Tilesets:");
        tileSetListContainer.AddChild(tileSetLabel);

        var tileSetList = new ItemListWithActions<Map2DTileset>(true);
        tileSetList.OnSelectionChanged = TilesetSelectionChanged;
        tileSetList.NewItemCreated = item =>
        {
            _map.Tiles.InitRuntimeState(_map).Wait();
        };
        tileSetList.SetItems(_map.Tiles.Tilesets);
        _tileSetList = tileSetList;
        tileSetListContainer.AddChild(tileSetList);

        var tempNewLayoutHolder = new UIBaseWindow();
        // tempNewLayoutHolder.UseNewLayoutSystem <- to find during cleanup
        tempNewLayoutHolder.MaxSizeX = 190f;
        tileSetListContainer.AddChild(tempNewLayoutHolder);

        var tileSelector = new TilesetTileSelector(_map.Tiles);
        tileSelector.Id = "TileSelector";
        tileSelector.SetTileset(_currentTileset);
        _tileSelector = tileSelector;
        tempNewLayoutHolder.AddChild(tileSelector);
    }

    private void LayerSelectionChanged(Map2DTileMapLayer selected)
    {
        _currentLayer = selected;
    }

    private void TilesetSelectionChanged(Map2DTileset tileset)
    {
        _currentTileset = tileset;

        var tileSelector = GetWindowById("TileSelector") as TilesetTileSelector;
        tileSelector?.SetTileset(_currentTileset);
    }

    public Map2DTileMapLayer? GetLayer()
    {
        return _currentLayer;
    }

    public uint GetTidToPlace()
    {
        if (_tileSelector == null || _tileSelector.SelectedTiles.Count == 0) return 0;
        return _tileSelector.SelectedTiles[0];
    }
}

// todo: replace with dropdown and selectable(multi) editors.
public class ItemListWithActions<T> : UIBaseWindow
{
    public Action<T>? OnSelectionChanged;
    public Action<T>? NewItemCreated;

    public Func<T>? FactoryCreateNew;

    private List<T>? _items;
    private int _selectedItemIdx;

    private bool _dropDownMode;

    public ItemListWithActions(bool dropDownMode = false)
    {
        StretchX = true;
        StretchY = true;

        _dropDownMode = dropDownMode;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        LayoutMode = LayoutMode.VerticalList;
        ListSpacing = new Vector2(0, 2);

        UIBaseWindow buttonsContainer = new UIBaseWindow();
        buttonsContainer.StretchX = true;
        buttonsContainer.StretchY = true;
        buttonsContainer.LayoutMode = LayoutMode.HorizontalList;
        buttonsContainer.ListSpacing = new Vector2(1, 0);
        AddChild(buttonsContainer);

        //EditorButton addItem = new EditorButton();
        //addItem.Text = "Add";
        //addItem.StretchY = true;
        //addItem.OnClickedProxy = _ =>
        //{
        //    if (_items == null) return;

        //    T? newItem;
        //    if (FactoryCreateNew == null)
        //        newItem = (T?)Activator.CreateInstance(typeof(T), true);
        //    else
        //        newItem = FactoryCreateNew();

        //    if (newItem == null) return;

        //    _items.Add(newItem);
        //    NewItemCreated?.Invoke(newItem);
        //    SetSelectedItem(_items.Count - 1);
        //    SetItems(_items);
        //};
        //buttonsContainer.AddChild(addItem);

        //      EditorButton deleteSelectedItem = new EditorButton();
        //deleteSelectedItem.Text = "Remove";
        //      deleteSelectedItem.StretchY = true;
        //      buttonsContainer.AddChild(deleteSelectedItem);

        //      EditorButton moveUp = new EditorButton();
        //moveUp.Text = "^";
        //moveUp.StretchY = true;
        //      buttonsContainer.AddChild(moveUp);

        //      EditorButton moveDown = new EditorButton();
        //moveDown.Text = "V";
        //moveDown.StretchY = true;
        //buttonsContainer.AddChild(moveDown);

        //var editSelected = new EditorButton("Edit");
        //editSelected.StretchY = true;
        //editSelected.OnClickedProxy = (_) =>
        //{
        //    if (GetSelectedItem(out T selectedItem))
        //    {
        //        var propPanel = new GenericPropertiesEditorPanel(selectedItem);
        //        propPanel.OnPropertyEdited = (propName, propValue) =>
        //        {
        //            RefreshListUI();
        //        };
        //        Controller!.AddChild(propPanel);
        //    }
        //};
        //buttonsContainer.AddChild(editSelected);

        if (_dropDownMode)
        {
            var dropDown = new EditorButtonDropDown();
            dropDown.Id = "DropDown";
            AddChild(dropDown);
        }
        else
        {
            var list = new UICallbackListNavigator();
            list.Id = "List";
            list.StretchX = true;
            list.ChildrenAllSameWidth = true;
            list.LayoutMode = LayoutMode.VerticalList;
            AddChild(list);
        }

        if (_items != null) SetItems(_items);
    }

    // can add (callback)
    // can create children/can move in
    // can delete (callback)

    private EditorDropDownItem[] _dropDownNoItems =
    {
        new()
        {
            Name = "Empty"
        }
    };

    public void SetItems(List<T> items)
    {
        _items = items;
        if (Controller == null) return;

        if (_dropDownMode)
        {
            var dropDownButton = GetWindowById("DropDown") as EditorButtonDropDown;
            AssertNotNull(dropDownButton);

            EditorDropDownItem[] dropDownItems = _items == null ? _dropDownNoItems : new EditorDropDownItem[_items.Count];
            if (items != null)
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var itemDescr = new EditorDropDownItem
                    {
                        Name = item?.ToString() ?? "<null>",
                        UserData = i,
                        Click = (dropDownItem, __) => { SetSelectedItem((int)dropDownItem.UserData); }
                    };
                    dropDownItems[i] = itemDescr;
                }

            dropDownButton.SetItems(dropDownItems, 0);
        }
        else
        {
            var list = GetWindowById("List") as UICallbackListNavigator;
            AssertNotNull(list);

            list.ClearChildren();
            if (_items != null)
                for (int i = 0; i < _items.Count; i++)
                {
                    T item = _items[i];
                    var button = new EditorButton
                    {
                        Text = item?.ToString() ?? "<null>",
                        StretchY = true,
                        UserData = i,

                        OnClickedProxy = button =>
                        {
                            EditorButton? asEditorButton = button as EditorButton;
                            if (asEditorButton != null) SetSelectedItem((int)asEditorButton.UserData);
                        }
                    };
                    list.AddChild(button);
                }
        }

        SetSelectedItem(0);
    }

    public void SetSelectedItem(int selectedItemIdx)
    {
        if (_items == null || _items.Count == 0)
        {
            _selectedItemIdx = -1;
            return;
        }

        _selectedItemIdx = selectedItemIdx;
        var selectedItem = _items[_selectedItemIdx];
        OnSelectionChanged?.Invoke(selectedItem);
    }

    public bool GetSelectedItem(out T selectedItem)
    {
        T? t = default;
        selectedItem = t;
        if (_items == null || _items.Count == 0) return false;
        Assert(_selectedItemIdx < _items.Count);
        selectedItem = _items[_selectedItemIdx];

        return true;
    }

    public void RefreshListUI() // todo: UI data bindings?
    {
        if (_items == null) return;

        var sel = _selectedItemIdx; // Set items will reset this
        SetItems(_items);
        SetSelectedItem(sel);
    }
}