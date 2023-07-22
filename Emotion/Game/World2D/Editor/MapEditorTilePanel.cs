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
	private UICallbackListNavigator _layerList;

	public MapEditorTilePanel(Map2D map) : base("Tile Editor")
	{
		_map = map;
	}

	public override void AttachedToController(UIController controller)
	{
		base.AttachedToController(controller);

		var list = new UICallbackListNavigator();
		list.LayoutMode = LayoutMode.VerticalList;
		_layerList = list;
		_contentParent.AddChild(list);

		PopulateLayerList();
	}

	public void PopulateLayerList()
	{
		_layerList.ClearChildren();

		Map2DTileMapData? tileData = _map.TileData;
		if (tileData == null)
		{
			return;
		}

		for (var i = 0; i < tileData.Layers.Count; i++)
		{
			Map2DTileMapLayer layer = tileData.Layers[i];

			var button = new MapEditorTopBarButton
			{
				Text = layer.Name,
				StretchY = true
			};

			_layerList.AddChild(button);
		}
	}
}