#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D
{
	public class MapEditorOpenMapPanel : MapEditorPanel
	{
		private World2DEditor _editor;
		private Type _mapType;

		public MapEditorOpenMapPanel(World2DEditor editor, Type mapType) : base("Open Map")
		{
			_editor = editor;
			_mapType = mapType;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var listNav = new UICallbackListNavigator();
			listNav.LayoutMode = LayoutMode.VerticalList;
			listNav.StretchX = true;
			listNav.ListSpacing = new Vector2(0, 1);
			listNav.Margins = new Rectangle(0, 0, 10, 0);
			listNav.InputTransparent = false;
			listNav.ChildrenAllSameWidth = true;
			_contentParent.AddChild(listNav);

			var scrollBar = new UIScrollbar();
			scrollBar.DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
			scrollBar.SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
			scrollBar.WindowColor = Color.Black * 0.5f;
			scrollBar.Anchor = UIAnchor.TopRight;
			scrollBar.ParentAnchor = UIAnchor.TopRight;
			scrollBar.MinSize = new Vector2(5, 0);
			scrollBar.MaxSize = new Vector2(5, 9999);
			listNav.SetScrollbar(scrollBar);
			_contentParent.AddChild(scrollBar);

			var mapAssets = new List<string>();
			string mapType = _mapType.FullName;
			var xmlTag = $"<Map2D type=\"{mapType}\"";

			string[] allAssets = Engine.AssetLoader.AllAssets;
			for (var i = 0; i < allAssets.Length; i++)
			{
				string asset = allAssets[i];
				if (!asset.Contains(".xml")) continue;

				var assetLoaded = Engine.AssetLoader.Get<TextAsset>(asset, false);
				if (assetLoaded?.Content != null && assetLoaded.Content.Contains(xmlTag)) mapAssets.Add(asset);
			}

			Task openingTask = null;
			for (var i = 0; i < mapAssets.Count; i++)
			{
				string mapAsset = mapAssets[i];

				var mapButton = new MapEditorTopBarButton();
				mapButton.Text = mapAsset;
				mapButton.StretchY = true;
				mapButton.OnClickedProxy = _ =>
				{
					if (openingTask != null && !openingTask.IsCompleted) return;

					openingTask = Task.Run(() =>
					{
						var newMapAsset = Engine.AssetLoader.Get<XMLAsset<Map2D>>(mapAsset, false);
						Map2D? newMap = newMapAsset?.Content;
						if (newMap == null) return;

						EditorUtility.ChangeCurrentMapInCurrentScene(newMap);
						Close();
					});
				};

				listNav.AddChild(mapButton);
			}
		}
	}
}