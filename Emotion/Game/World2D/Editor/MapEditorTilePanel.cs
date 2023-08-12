#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

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

		var layerLabel = new MapEditorLabel("Layers:");
		_contentParent.AddChild(layerLabel);

		var list = new ItemListWithActions<Map2DTileMapLayer>();
		list.LayoutMode = LayoutMode.VerticalList;
		list.StretchX = true;
		list.ChildrenAllSameWidth = true;
		list.OnSelectionChanged = ListSelectionChanged;
		_layerList = list;
		_contentParent.AddChild(list);

		PopulateLayerList();
	}

	public void PopulateLayerList()
	{
		_layerList.ClearChildren();

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
	}
}

public class ItemListWithActionsItem<T>
{
	public T Object;
	public List<ItemListWithActionsItem<T>> Children;
}

public class ItemListWithActions<T> : UICallbackListNavigator
{
	public Action<T> OnSelectionChanged;

	private IEnumerable<ItemListWithActionsItem<T>>? _items;
	private T? _selectedItem;

	// can add (callback)
	// can create children/can move in
	// can delete (callback)

	public void SetItems(IEnumerable<ItemListWithActionsItem<T>>? items)
	{
		ClearChildren();
		GenerateChildrenFromItems();
	}

	public void SetSelectedItem(T selectedItem)
	{
		_selectedItem = selectedItem;
		OnSelectionChanged?.Invoke(selectedItem);
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

				OnClickedProxy = _ => { SetSelectedItem(item.Object); }
			};
			AddChild(button);
		}
	}
}