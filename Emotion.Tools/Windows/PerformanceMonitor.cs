#region Using

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.Plugins.ImGuiNet.Windowing;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Windows
{
    public class PerformanceMonitor : ImGuiWindow
    {
        private float[] _dtTracker = new float[100];
        private int _dtIdx;
        private Stopwatch _dtTimer = new Stopwatch();

        private float[] _updateTracker = new float[100];
        private int _updateIdx;

        private int _updatesPerFrame;

        private DateTime _fpsSecond = DateTime.Now;
        private int _fpsTracker;
        private int _fps;

        public PerformanceMonitor() : base("Performance Monitor")
        {
        }

        protected override void RenderContent(RenderComposer composer)
        {
            _updateTracker[_updateIdx] = _updatesPerFrame;
            _updateIdx++;
            if (_updateIdx > _updateTracker.Length - 1) _updateIdx = 0;
            _updatesPerFrame = 0;
            ImGui.PlotLines("DeltaTime", ref _dtTracker[0], 100, 0, "", 0, 30);
            ImGui.PlotLines("Updates", ref _updateTracker[0], 100, 0, "", 0, 5);
            ImGui.Text($"FPS {_fps}");

            _fpsTracker++;
        }

        public override void Update()
        {
            _dtTracker[_dtIdx] = _dtTimer.ElapsedMilliseconds;
            _dtTimer.Restart();
            _dtIdx++;
            if (_dtIdx > _dtTracker.Length - 1) _dtIdx = 0;

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