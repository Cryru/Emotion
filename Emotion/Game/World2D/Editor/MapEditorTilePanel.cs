#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;
using Emotion.Utility;
using System.Reflection.Emit;


#endregion

namespace Emotion.Game.World2D.Editor;

public class MapEditorTilePanel : EditorPanel
{
	private Map2D _map;
	private ItemListWithActions<Map2DTileMapLayer> _layerList = null!;
	private ItemListWithActions<Map2DTileset> _tileSetList = null!;
	private Map2DTileMapLayer? _currentLayer;
	private Map2DTileset? _currentTileset;

	public MapEditorTilePanel(Map2D map) : base("Tile Editor")
	{
		_map = map;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var topBar = GetWindowById("TopBar") as MapEditorPanelTopBar;
		if (topBar != null) topBar.CanMove = false;

		// todo: with new ui this can go between top bar and bottom bar snuggly.
		_centered = false;
		_container.Anchor = UIAnchor.TopRight;
		_container.ParentAnchor = UIAnchor.TopRight;
		_container.StretchY = true;
		_container.StretchX = true;
		_container.MaxSizeY = Engine.Renderer.CurrentTarget.Size.Y;

		var innerContainer = new UIBaseWindow();
		innerContainer.StretchX = true;
		innerContainer.StretchY = true;
		innerContainer.LayoutMode = LayoutMode.VerticalList;
		_contentParent.AddChild(innerContainer);

		var topPartContainer = new UIBaseWindow();
		topPartContainer.StretchY = true;
		topPartContainer.StretchX = true;
		topPartContainer.LayoutMode = LayoutMode.HorizontalList;
		topPartContainer.Id = "TopPartContainer";
		topPartContainer.MaxSizeY = 200;
		innerContainer.AddChild(topPartContainer);

		var layerListContainer = new UIBaseWindow();
		layerListContainer.LayoutMode = LayoutMode.VerticalList;
		layerListContainer.StretchX = true;
		layerListContainer.StretchY = true;
		layerListContainer.ListSpacing = new Vector2(0, 1);
		topPartContainer.AddChild(layerListContainer);

		var layerLabel = new MapEditorLabel("Layers:");
		layerListContainer.AddChild(layerLabel);

		var list = new ItemListWithActions<Map2DTileMapLayer>();
		list.OnSelectionChanged = ListSelectionChanged;
		list.NewItemCreated = item =>
		{
			Map2DTileMapData? tileData = _map.TileData;
			if (tileData == null) return;
			tileData.Layers.Add(item);
		};
		_layerList = list;
		layerListContainer.AddChild(list);

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
			//Map2DTileMapData? tileData = _map.TileData;
			//if (tileData == null) return;
			//tileData.Layers.Add(item);
		};
		_tileSetList = tileSetList;
		tileSetListContainer.AddChild(tileSetList);

		PopulateLayerList();
		PopulateTileSets();
	}

	public void PopulateLayerList()
	{
		Map2DTileMapData? tileData = _map.TileData;
		if (tileData == null) return;

		List<ItemListWithActionsItem<Map2DTileMapLayer>> items = new();
		for (var i = 0; i < tileData.Layers.Count; i++)
		{
			Map2DTileMapLayer layer = tileData.Layers[i];
			_currentLayer ??= layer;
			items.Add(new ItemListWithActionsItem<Map2DTileMapLayer>
			{
				Object = layer,
			});
		}

		_layerList.SetItems(items);
		_layerList.SetSelectedItem(_currentLayer);

		// Add common functionality for a list with:
		// Add arrows up and down
		// Add delete
		// Add add
	}

	private void ListSelectionChanged(Map2DTileMapLayer selected)
	{
		_currentLayer = selected;
		if (selected != null)
		{
			var contentParent = GetWindowById("TopPartContainer");
			AssertNotNull(contentParent);

			var oldProperties = contentParent.GetWindowById("Properties");
			if (oldProperties != null) contentParent.RemoveChild(oldProperties);

			var propertyEditor = new GenericPropertiesEditorPanel(selected);
			propertyEditor.Id = "Properties";
			propertyEditor.PanelMode = PanelMode.Embedded;
			propertyEditor.StretchX = true;
			propertyEditor.StretchY = true;
			propertyEditor.OnPropertyEdited = (key, newVal) => { _layerList.Modified(selected); };

			contentParent.AddChild(propertyEditor);
		}
	}

	public void PopulateTileSets()
	{
		var tileData = _map.TileData;
		if (tileData == null) return;

		List<ItemListWithActionsItem<Map2DTileset>> items = new();
		var tileSets = tileData.Tilesets;
		for (int i = 0; i < tileSets.Length; i++)
		{
			var tileSet = tileSets[i];
			if (tileSet == null) continue;

			_currentTileset ??= tileSet;
			items.Add(new ItemListWithActionsItem<Map2DTileset>
			{
				Object = tileSet,
			});
		}
		_tileSetList.SetItems(items);
		_tileSetList.SetSelectedItem(_currentTileset);
	}

	private void TilesetSelectionChanged(Map2DTileset tileset)
	{

	}
}

public class ItemListWithActionsItem<T>
{
	public T Object;
	public List<ItemListWithActionsItem<T>> Children;
}

public class ItemListWithActions<T> : UIBaseWindow
{
	public Action<T>? OnSelectionChanged;
	public Action<T>? NewItemCreated;

	public Func<T>? FactoryCreateNew;

	private List<ItemListWithActionsItem<T>>? _items;
	private T? _selectedItem;

	private bool _dropDownMode = false;

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

		EditorButton addItem = new EditorButton();
		addItem.Text = "Add";
		addItem.StretchY = true;
		addItem.OnClickedProxy = _ =>
		{
			if (_items == null) return;

			T? newItem;
			if (FactoryCreateNew == null)
				newItem = (T?)Activator.CreateInstance(typeof(T), true);
			else
				newItem = FactoryCreateNew();

			if (newItem == null) return;

			_items.Add(new ItemListWithActionsItem<T>
			{
				Object = newItem
			});
			NewItemCreated?.Invoke(newItem);
			SetSelectedItem(newItem);
		};
		buttonsContainer.AddChild(addItem);

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

	public void SetItems(List<ItemListWithActionsItem<T>> items)
	{
		_items = items;

		if (_dropDownMode)
		{
			var dropDownButton = GetWindowById("DropDown") as EditorButtonDropDown;
			AssertNotNull(dropDownButton);

			int selectedItem = 0;
			EditorDropDownItem[] dropDownItems = _items == null ? _dropDownNoItems : new EditorDropDownItem[_items.Count];
			if (items != null)
			{
				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];

					EditorDropDownItem desc = new EditorDropDownItem()
					{
						Name = item.Object?.ToString() ?? "<null>",
						UserData = item,
						Click = (_, __) =>
						{
							SetSelectedItem(item.Object);
						}
					};
					dropDownItems[i] = desc;

					if (ReferenceEquals(_selectedItem, item.Object)) selectedItem = i;
				}
			}

			dropDownButton.SetItems(dropDownItems, selectedItem);
		}
		else
		{
			var list = GetWindowById("List") as UICallbackListNavigator;
			AssertNotNull(list);

			list.ClearChildren();
			if (_items == null) return;
			for (int i = 0; i < _items.Count; i++)
			{
				ItemListWithActionsItem<T> item = _items[i];
				var button = new EditorButton
				{
					Text = item.Object?.ToString() ?? "<null>",
					StretchY = true,
					UserData = item,

					OnClickedProxy = _ => { SetSelectedItem(item.Object); }
				};
				list.AddChild(button);
			}
		}
	}

	public void SetSelectedItem(T selectedItem)
	{
		_selectedItem = selectedItem;
		OnSelectionChanged?.Invoke(selectedItem);
	}

	public void Modified(T obj) // todo: UI data bindings?
	{
		if (obj == null) return;

		if (!_dropDownMode)
		{
			var list = GetWindowById("List") as UICallbackListNavigator;
			AssertNotNull(list);

			if (list == null || list.Children == null) return;

			for (int i = 0; i < list.Children.Count; i++)
			{
				EditorButton? button = list.Children[i] as EditorButton;
				if (button != null && Helpers.AreObjectsEqual(obj, button.UserData)) button.Text = obj.ToString() ?? "<null>";
			}
		}
	}

	private void GenerateChildrenFromItems()
	{
		if (_items == null) return;


	}
}