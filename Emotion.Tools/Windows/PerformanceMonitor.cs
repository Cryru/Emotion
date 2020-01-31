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
        private float[] _dtTracker;
        private int _dtIdx;
        private Stopwatch _dtTimer = new Stopwatch();

        private float[] _updateTracker;
        private int _updateIdx;

        private int _updatesPerFrame;

        private DateTime _fpsSecond = DateTime.Now;
        private int _fpsTracker;
        private int _fps;

        public PerformanceMonitor(int resolution = 100) : base("Performance Monitor")
        {
            _dtTracker = new float[resolution];
            _updateTracker = new float[resolution];
        }

        protected override void RenderContent(RenderComposer composer)
        {
            _updateTracker[_updateIdx] = _updatesPerFrame;
            _updateIdx++;
            if (_updateIdx > _updateTracker.Length - 1) _updateIdx = 0;
            _updatesPerFrame = 0;
            ImGui.PlotLines("Actual DeltaTime", ref _dtTracker[0], _dtTracker.Length, 0, "", 0, 30);
            ImGui.PlotLines("Update Count", ref _updateTracker[0], _updateTracker.Length, 0, "", 0, 5);
            ImGui.Text($"FPS {_fps}");
            ImGui.Text($"Reported DeltaTime {Engine.DeltaTime}");

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