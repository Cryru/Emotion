#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
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
            new EditorDropDownItem
            {
                Name = "Open Tile Editor",
                Click = (_, __) =>
                {
                    AssertNotNull(map);
                    _tileEditor = new MapEditorTilePanel(map);
                    _editUI!.AddChild(_tileEditor);
                },
                Enabled = () => map != null
            },
        });
        tilesMenu.ZOffset = 4;

        // todo: GPU texture viewer
        // todo: animation tool (convert from imgui)
        // todo: asset preview tool
        // todo: ema integration

        parentList.AddChild(tilesMenu);
    }
}