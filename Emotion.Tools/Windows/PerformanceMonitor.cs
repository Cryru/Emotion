#region Using

using System;
using Emotion.Common;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class PerformanceMonitor : ImGuiWindow
    {
        private float[] dtTracker = new float[100];
        private int dtIdx;

        private float[] updateTracker = new float[100];
        private int updateIdx;

        private int _updatesPerFrame;

        private DateTime _fpsSecond = DateTime.Now;
        private int _fpsTracker;
        private int _fps;

        public PerformanceMonitor() : base("Performance Monitor")
        {
        }

        protected override void RenderContent()
        {
            updateTracker[updateIdx] = _updatesPerFrame;
            updateIdx++;
            if (updateIdx > updateTracker.Length - 1) updateIdx = 0;
            _updatesPerFrame = 0;
            ImGui.PlotLines("DeltaTime", ref dtTracker[0], 100, 0, "", 0, 50);
            ImGui.PlotLines("Updates", ref updateTracker[0], 100, 0, "", 0, 5);
            ImGui.Text($"FPS {_fps}");

            _fpsTracker++;
        }

        public override void Update()
        {
            dtTracker[dtIdx] = Engine.DeltaTime;
            dtIdx++;
            if (dtIdx > dtTracker.Length - 1) dtIdx = 0;

            _updatesPerFrame++;

            if (_fpsSecond.Second != DateTime.Now.Second)
            {
                _fps = _fpsTracker;
                _fpsTracker = 0;
                _fpsSecond = DateTime.Now;
            }
        }
    }
}