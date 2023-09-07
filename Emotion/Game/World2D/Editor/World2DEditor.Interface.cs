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
		Map2D? map = CurrentMap;

		base.EditorAttachTopBarButtons(parentList);

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

		EditorButton editorMenu = EditorDropDownButton("Editor", new[]
		{
			// Shows actions done in the editor, can be undone
			new EditorDropDownButtonDescription
			{
				Name = "Undo History",
				Click = (_, __) =>
				{
					var panel = new EditorListOfItemsPanel<IWorldEditorAction>("Actions", _actions, obj => { });
					_editUI!.AddChild(panel);
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Model Viewer (WIP)",
				Click = (_, __) =>
				{
					var panel = new ModelViewer();
					_editUI!.AddChild(panel);
				},
			},
		});

		EditorButton otherTools = EditorDropDownButton("Other", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Open Folder",
				Click = (_, __) => { Process.Start("explorer.exe", "."); },
				Enabled = () => Engine.Host is Win32Platform
			},

			new EditorDropDownButtonDescription
			{
				Name = "Performance Monitor",
				Click = (_, __) =>
				{
					var panel = new PerformanceMonitor();
					_editorUIAlways!.AddChild(panel);
				},
			},
		});

		// todo: GPU texture viewer
		// todo: animation tool (convert from imgui)
		// todo: asset preview tool
		// todo: ema integration

		parentList.AddChild(tilesMenu);
		parentList.AddChild(editorMenu);
		parentList.AddChild(otherTools);
	}
}