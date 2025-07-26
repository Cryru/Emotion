using Emotion.Common.Serialization;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.Components;

[DontSerialize]
public class ContainerVisibleInEditorMode : UIBaseWindow
{
    public MapEditorMode VisibleIn = MapEditorMode.Off | MapEditorMode.TwoDee | MapEditorMode.ThreeDee;
    public Action<MapEditorMode>? OnModeChanged;

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        EngineEditor.OnMapEditorModeChanged += UpdateVisibility;
        UpdateVisibility(EngineEditor.MapEditorMode);
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);

        EngineEditor.OnMapEditorModeChanged -= UpdateVisibility;
    }

    private void UpdateVisibility(MapEditorMode currentMode)
    {
        Visible = VisibleIn == currentMode || VisibleIn.EnumHasFlag(currentMode);
        OnModeChanged?.Invoke(currentMode);
    }
}
