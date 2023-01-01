#region Using

using System.Diagnostics;
using Emotion.Game.ThreeDee;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public sealed class MapEditorObjectList : MapEditorPanel
	{
		public Map2D ObjectMap;

		public MapEditorObjectList(Map2D objectMap) : base("All Map Objects")
		{
			ObjectMap = objectMap;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			UIBaseWindow contentWin = _contentParent;
			contentWin.InputTransparent = false;

			var innerContainer = new UIBaseWindow();
			innerContainer.StretchX = true;
			innerContainer.StretchY = true;
			innerContainer.InputTransparent = false;
			innerContainer.LayoutMode = LayoutMode.VerticalList;
			innerContainer.ListSpacing = new Vector2(0, 3);
			innerContainer.ChildrenAllSameWidth = true;
			contentWin.AddChild(innerContainer);

			var listContainer = new UIBaseWindow();
			listContainer.StretchX = true;
			listContainer.StretchY = true;
			listContainer.InputTransparent = false;
			innerContainer.AddChild(listContainer);

			var listNav = new UICallbackListNavigator();
			listNav.LayoutMode = LayoutMode.VerticalList;
			listNav.StretchX = true;
			listNav.ListSpacing = new Vector2(0, 1);
			listNav.Margins = new Rectangle(0, 0, 10, 0);
			listNav.InputTransparent = false;
			listNav.ChildrenAllSameWidth = true;
			listContainer.AddChild(listNav);

			var scrollBar = new UIScrollbar();
			scrollBar.DefaultSelectorColor = MapEditorColorPalette.ButtonColor;
			scrollBar.SelectorMouseInColor = MapEditorColorPalette.ActiveButtonColor;
			scrollBar.WindowColor = Color.Black * 0.5f;
			scrollBar.Anchor = UIAnchor.TopRight;
			scrollBar.ParentAnchor = UIAnchor.TopRight;
			scrollBar.MinSize = new Vector2(5, 0);
			scrollBar.MaxSize = new Vector2(5, 9999);
			listNav.SetScrollbar(scrollBar);
			listContainer.AddChild(scrollBar);

			foreach (GameObject2D obj in ObjectMap.GetObjects())
			{
				var objectLabel = new UIText();
				objectLabel.ScaleMode = UIScaleMode.FloatScale;
				objectLabel.WindowColor = MapEditorColorPalette.TextColor;
				objectLabel.FontFile = "Editor/UbuntuMono-Regular.ttf";
				objectLabel.FontSize = MapEditorColorPalette.EditorButtonTextSize;
				objectLabel.Text = $"[{obj.UniqueId}] {obj.ObjectName ?? $"Object {obj.GetType()}"} {(obj.ObjectState != ObjectState.Alive ? obj.ObjectState : "")}";
				listNav.AddChild(objectLabel);
			}
		}
	}
}