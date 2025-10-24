#nullable enable

using Emotion.Editor.EditorUI.Components;

namespace Emotion.Editor.Tools;

public class JobSystemVisualizer : EditorWindow
{
    public const int MANY_JOBS = 20;

    private UIProgressBar[]? _threadBars;

    public JobSystemVisualizer() : base("Job System")
    {
    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow contentParent = GetContentParent();
        contentParent.Layout.LayoutMethod = UILayoutMethod.VerticalList(5);
        contentParent.Layout.SizingY = UISizing.Fit();
        contentParent.Layout.SizingX = UISizing.Fit();

        int threadCount = Engine.Jobs.ThreadCount;
        _threadBars = new UIProgressBar[threadCount + 1];
        for (int i = 0; i < threadCount + 1; i++)
        {
            UIBaseWindow threadContainer = new()
            {
                Layout =
                {
                    LayoutMethod = UILayoutMethod.HorizontalList(10)
                }
            };
            contentParent.AddChild(threadContainer);

            threadContainer.AddChild(new EditorLabel(i == 0 ? "Queued" : $"JobThread {i}")
            {
                Layout =
                {
                    SizingX = UISizing.Grow()
                }
            });

            UIProgressBar threadBar = new()
            {
                Layout =
                {
                    SizingX = UISizing.Fixed(200)
                },
                Visuals =
                {
                    Border = 1,
                    BorderColor = Color.White * 0.5f
                }
            };
            threadContainer.AddChild(threadBar);
            _threadBars[i] = threadBar;
        }
    }

    protected override bool UpdateInternal()
    {
        AssertNotNull(_threadBars);
        for (int i = 0; i < _threadBars.Length; i++)
        {
            int threadJobs;
            if (i == 0)
                threadJobs = Engine.Jobs.DebugOnly_GetQueuedJobCount();
            else
                threadJobs = Engine.Jobs.DebugOnly_GetThreadJobAmount(i - 1);

            float percentBusy = (float)threadJobs / MANY_JOBS;
            if (percentBusy > 1.0f) percentBusy = 1.0f;
            UIProgressBar bar = _threadBars[i];

            bar.Progress = percentBusy;
            if (percentBusy >= 1.0f)
                bar.ProgressColor = Color.PrettyPurple;
            else if (percentBusy > 0.8f)
                bar.ProgressColor = Color.PrettyRed;
            else if (percentBusy > 0.4f)
                bar.ProgressColor = Color.PrettyYellow;
            else
                bar.ProgressColor = Color.PrettyGreen;
        }

        return base.UpdateInternal();
    }
}
