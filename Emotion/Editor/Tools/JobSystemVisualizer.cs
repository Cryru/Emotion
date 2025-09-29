#nullable enable

using Emotion.Core.Systems.JobSystem;
using Emotion.Editor.EditorUI.Components;

namespace Emotion.Editor.Tools;

public class JobSystemVisualizer : EditorWindow
{
    private UIBaseWindow[]? _threadBars;

    public JobSystemVisualizer() : base("Job System")
    {
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow contentParent = GetContentParent();
        contentParent.Layout.LayoutMethod = UILayoutMethod.VerticalList(0);

        int threadCount = Engine.Jobs.ThreadCount;
        _threadBars = new UIBaseWindow[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            UIBaseWindow threadContainer = new()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(10),
                    SizingX = UISizing.Fixed(300)
                }
            };
            contentParent.AddChild(threadContainer);

            threadContainer.AddChild(new EditorLabel($"JobThread {i}"));

            UIBaseWindow threadBar = new()
            {
                Layout =
                {
                    MinSizeX = 200,
                    MaxSizeY = 20
                }
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

            UIBaseWindow bar = _threadBars[i];

            bar.Layout.SizingX = UISizing.Fixed((int) MathF.Round(200 * percentBusy));
            if (percentBusy > 1.0f)
                bar.Visuals.BackgroundColor = Color.PrettyPurple;
            else if (percentBusy > 0.8f)
                bar.Visuals.BackgroundColor = Color.PrettyRed;
            else if (percentBusy > 0.4f)
                bar.Visuals.BackgroundColor = Color.PrettyYellow;
            else
                bar.Visuals.BackgroundColor = Color.PrettyGreen;
        }
        InvalidateLayout();

        return base.UpdateInternal();
    }
}
