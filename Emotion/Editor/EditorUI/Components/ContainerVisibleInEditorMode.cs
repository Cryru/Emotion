#nullable enable

using Emotion.Game.Systems.UI;

namespace Emotion.Editor.EditorUI.Components;

[DontSerialize]
public class ContainerVisibleInEditorMode : UIBaseWindow
{
    public MapEditorMode VisibleIn = MapEditorMode.Off | MapEditorMode.TwoDee | MapEditorMode.ThreeDee;
    public Action<MapEditorMode>? OnModeChanged;

    protected override void OnOpen()
    {
        base.OnOpen();

        EngineEditor.OnMapEditorModeChanged += UpdateVisibility;
        UpdateVisibility(EngineEditor.MapEditorMode);
    }

    protected override void OnClose()
    {
        base.OnClose();

        EngineEditor.OnMapEditorModeChanged -= UpdateVisibility;
    }

    private void UpdateVisibility(MapEditorMode currentMode)
    {
        Visible = VisibleIn == currentMode || VisibleIn.EnumHasFlag(currentMode);
        OnModeChanged?.Invoke(currentMode);
    }
}
