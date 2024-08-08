#region Using

using System;
using System.Diagnostics;
using Emotion.Common;
using Emotion.Tools.DevUI;
using ImGuiNET;

#endregion

namespace Emotion.Tools.Editors
{
    public class PerformanceMonitor : ImGuiBaseWindow
    {
        private float[] _dtTracker;
        private int _dtIdx;
        private Stopwatch _dtTimer = new Stopwatch();

        private float[] _updateTracker;
        private int _updateIdx;

        private int _framesPerUpdate;

        private DateTime _fpsSecond = DateTime.Now;
        private int _fpsTracker;
        private int _fps;

        public PerformanceMonitor(int resolution = 200) : base("Performance Monitor")
        {
            _dtTracker = new float[resolution];
            _updateTracker = new float[resolution];
        }

        private float Average(float[] arr)
        {
            float sum = 0;
            for (var i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }

            return sum / arr.Length;
        }

        protected override bool UpdateInternal()
        {
            _updateTracker[_updateIdx] = _framesPerUpdate;
            _updateIdx++;
            if (_updateIdx > _updateTracker.Length - 1) _updateIdx = 0;
            _framesPerUpdate = 0;

            return true;
        }

        protected override void RenderImGui()
        {
            _dtTracker[_dtIdx] = _dtTimer.ElapsedMilliseconds;
            _dtTimer.Restart();
            _dtIdx++;
            if (_dtIdx > _dtTracker.Length - 1) _dtIdx = 0;

            _framesPerUpdate++;

            if (_fpsSecond.Second != DateTime.Now.Second)
            {
                _fps = _fpsTracker;
                _fpsTracker = 0;
                _fpsSecond = DateTime.Now;
            }

            ImGui.Text(Engine.Host.ToString());
            ImGui.Text(Engine.Host.Audio.ToString());

            ImGui.PlotLines("Actual DeltaTime", ref _dtTracker[0], _dtTracker.Length, 0, "", 0, 30);
            ImGui.Text($"Avg: {Average(_dtTracker)}");
            ImGui.PlotLines("Frames Per Update", ref _updateTracker[0], _updateTracker.Length, 0, "", 0, 5);
            ImGui.Text($"Avg: {Average(_updateTracker)}");
            ImGui.Text($"FPS {_fps}");
            ImGui.Text($"Reported DeltaTime {Engine.DeltaTime}");

            _fpsTracker++;
        }
    }
}