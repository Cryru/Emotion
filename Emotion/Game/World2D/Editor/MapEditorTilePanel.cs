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
	private UICallbackListNavigator _layerList = null!;
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

		var list = new UICallbackListNavigator();
		list.LayoutMode = LayoutMode.VerticalList;
		list.StretchX = true;
		list.ChildrenAllSameWidth = true;
		_layerList = list;
		_contentParent.AddChild(list);

		PopulateLayerList();
	}

	public void PopulateLayerList()
	{
		_layerList.ClearChildren();

		Map2DTileMapData? tileData = _map.TileData;
		if (tileData == null) return;

		for (var i = 0; i < tileData.Layers.Count; i++)
		{
			Map2DTileMapLayer layer = tileData.Layers[i];
			_currentLayer ??= layer;

			var button = new MapEditorTopBarButton
			{
				Text = layer.Name,
				StretchY = true,
				Enabled = layer != _currentLayer
			};

			_layerList.AddChild(button);
		}

		// Add common functionality for a list with:
		// Add arrows up and down
		// Add delete
		// Add add
	}
}

public class ItemListWithActionsItem<T>
{
	public T Object;
	public List<ItemListWithActionsItem<T>> Children;
}

public class ItemListWithActions<T> : UICallbackListNavigator
{
}