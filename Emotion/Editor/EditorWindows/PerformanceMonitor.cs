#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;
using Emotion.Utility;

#endregion

#nullable enable

namespace Emotion.Editor.EditorWindows;

public class PerformanceMonitor : EditorPanel
{
    private MapEditorLabel _avgDelta = null!;
    private MapEditorLabel _fps = null!;
    private MapEditorLabel _reportedDelta = null!;

    private EditorPlotLineWindow _deltaTimePlot = null!;
    private EditorPlotLineWindow _renderTimePlot = null!;

    public PerformanceMonitor() : base("Performance Monitor")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        _contentParent.LayoutMode = LayoutMode.VerticalList;

        var hostInfo = new MapEditorLabel($"{Engine.Host.GetType().Name}\n{Engine.Host.Audio.GetType().Name}");
        _contentParent.AddChild(hostInfo);

        _deltaTimePlot = new EditorPlotLineWindow();
        _contentParent.AddChild(_deltaTimePlot);

        _avgDelta = new MapEditorLabel("0");
        _contentParent.AddChild(_avgDelta);

        _reportedDelta = new MapEditorLabel("0");
        _contentParent.AddChild(_reportedDelta);

        _renderTimePlot = new EditorPlotLineWindow();
        _contentParent.AddChild(_renderTimePlot);

        _fps = new MapEditorLabel("0");
        _contentParent.AddChild(_fps);
    }

    protected override bool RenderInternal(RenderComposer c)
    {
        _avgDelta.Text = $"Delta Time Avg: {Helpers.GetArrayAverage(PerformanceMetrics.TickRate)}";
        _reportedDelta.Text = $"Reported Delta: {Engine.DeltaTime}";
        _fps.Text = $"FPS: {PerformanceMetrics.FpsLastSecond} ({PerformanceMetrics.FramesPerUpdateLastSecond} FPT)";

        _deltaTimePlot.SetData(PerformanceMetrics.TickRate, PerformanceMetrics.TickRateRingIdx);
        _renderTimePlot.SetData(PerformanceMetrics.FrameDelta, PerformanceMetrics.FrameRateRingIdx);

        return base.RenderInternal(c);
    }
}