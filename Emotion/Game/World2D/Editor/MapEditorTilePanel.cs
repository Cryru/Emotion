#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World2D.Editor;

public class MapEditorTilePanel : EditorPanel
{
	private Map2D _map;
	private ItemListWithActions<Map2DTileMapLayer> _layerList = null!;
	private Map2DTileMapLayer? _currentLayer;

	public MapEditorTilePanel(Map2D map) : base("Tile Editor")
	{
		_map = map;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		_centered = false;
		_container.Anchor = UIAnchor.TopRight;
		_container.ParentAnchor = UIAnchor.TopRight;
		_container.StretchY = false;
		_container.StretchX = true;
		_container.Offset = new Vector2(0, 10); // Top bar size, todo: const from MapEditorPanelTopBar

		var topPartContainer = new UIBaseWindow();
		topPartContainer.StretchY = true;
		topPartContainer.StretchX = true;
		topPartContainer.LayoutMode = LayoutMode.HorizontalList;
		topPartContainer.Id = "TopPartContainer";
		_contentParent.AddChild(topPartContainer);

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

		PopulateLayerList();
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

	protected UICallbackListNavigator _list = null!;

	public ItemListWithActions()
	{
		StretchX = true;
		StretchY = true;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		LayoutMode = LayoutMode.VerticalList;

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
				newItem = (T?) Activator.CreateInstance(typeof(T), true);
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

		_list = new UICallbackListNavigator();
		_list.StretchX = true;
		_list.ChildrenAllSameWidth = true;
		_list.LayoutMode = LayoutMode.VerticalList;
		AddChild(_list);
	}

	// can add (callback)
	// can create children/can move in
	// can delete (callback)

	public void SetItems(List<ItemListWithActionsItem<T>> items)
	{
		_list.ClearChildren();
		_items = items;
		GenerateChildrenFromItems();
	}

	public void SetSelectedItem(T selectedItem)
	{
		_selectedItem = selectedItem;
		OnSelectionChanged?.Invoke(selectedItem);
	}

	public void Modified(T obj) // todo: UI data bindings?
	{
		if (_list == null || _list.Children == null) return;
		if (obj == null) return;

		for (int i = 0; i < _list.Children.Count; i++)
		{
			EditorButton? button = _list.Children[i] as EditorButton;
			if (button != null && Helpers.AreObjectsEqual(obj, button.UserData)) button.Text = obj.ToString() ?? "<null>";
		}
	}

	private void GenerateChildrenFromItems()
	{
		if (_items == null) return;

		foreach (ItemListWithActionsItem<T> item in _items)
		{
			var button = new EditorButton
			{
				Text = item.Object?.ToString() ?? "<null>",
				StretchY = true,
				UserData = item,

				OnClickedProxy = _ => { SetSelectedItem(item.Object); }
			};
			_list.AddChild(button);
		}
	}
}