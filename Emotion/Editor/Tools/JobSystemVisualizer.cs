#nullable enable

using Emotion.Core.Systems.JobSystem;
using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;

namespace Emotion.Editor.Tools;

public class JobSystemVisualizer : EditorWindow
{
    private UISolidColor[]? _threadBars;

    public JobSystemVisualizer() : base("Job System")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var contentParent = GetContentParent();
        contentParent.LayoutMode = LayoutMode.VerticalList;

        int threadCount = Engine.Jobs.ThreadCount;
        _threadBars = new UISolidColor[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            UIBaseWindow threadContainer = new UIBaseWindow();
            threadContainer.GrowY = false;
            threadContainer.GrowX = false;
            threadContainer.MaxSizeX = 300;
            threadContainer.LayoutMode = LayoutMode.HorizontalList;
            //threadContainer.ListSpacing = new Vector2(10, 0); // nani the hell? why do I need this as a margin instead
            contentParent.AddChild(threadContainer);

            threadContainer.AddChild(new EditorLabel($"JobThread {i}"));

            UISolidColor threadBar = new UISolidColor
            {
                MinSizeX = 200,
                MaxSizeY = 20,
                Margins = new Primitives.Rectangle(10, 0, 0, 0)
            };
            _threadBars[i] = threadBar;
            threadContainer.AddChild(threadBar);
        }
    }

    protected override bool UpdateInternal()
    {
        AssertNotNull(_threadBars);
        for (int i = 0; i < _threadBars.Length; i++)
        {
            int threadJobs = Engine.Jobs.DebugOnly_GetThreadJobAmount(i);
            float percentBusy = (float)threadJobs / AsyncJobManager.MANY_JOBS;

            var bar = _threadBars[i];

            bar.MinSizeX = 200 * percentBusy;
            if (percentBusy > 1.0f)
                bar.WindowColor = Color.PrettyPurple;
            else if (percentBusy > 0.8f)
                bar.WindowColor = Color.PrettyRed;
            else if (percentBusy > 0.4f)
                bar.WindowColor = Color.PrettyYellow;
            else
                bar.WindowColor = Color.PrettyGreen;
        }
        InvalidateLayout();

        return base.UpdateInternal();
    }
}
