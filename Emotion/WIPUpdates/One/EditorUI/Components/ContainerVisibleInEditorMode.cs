using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.WIPUpdates.One.EditorUI.Components;

public class ContainerVisibleInEditorMode : UIBaseWindow
{
    public MapEditorMode VisibleIn = MapEditorMode.Off | MapEditorMode.TwoDee | MapEditorMode.ThreeDee;

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
        Visible = VisibleIn.EnumHasFlag(currentMode);
    }
}
