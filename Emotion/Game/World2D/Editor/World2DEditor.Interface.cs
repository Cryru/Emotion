#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows;
using Emotion.Game.World.Editor.Actions;
using Emotion.Platform.Implementation.Win32;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	protected override void EditorAttachTopBarButtons(UIBaseWindow parentList)
	{
		base.EditorAttachTopBarButtons(parentList);

		Map2D? map = CurrentMap;
		EditorButton tilesMenu = EditorDropDownButton("Tiles", new[]
		{
			// false by default, mouseover shows props, alt switch layers
			//new EditorDropDownButtonDescription
			//{
			//	Name = $"Selection: {(_tileSelect ? "Enabled" : "Disabled")}"
			//},
			// Shows layers, tilesets and other special editors for this mode, disables object selection while open
			new EditorDropDownButtonDescription
			{
				Name = "Open Tile Editor",
				Click = (_, __) =>
				{
					AssertNotNull(map);
					_editUI!.AddChild(new MapEditorTilePanel(map));
				},
				Enabled = () => map != null
			},
		});

		// todo: GPU texture viewer
		// todo: animation tool (convert from imgui)
		// todo: asset preview tool
		// todo: ema integration

		parentList.AddChild(tilesMenu);
	}
}