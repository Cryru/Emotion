#region Using

using System;
using System.Diagnostics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Tools.DevUI;
using Emotion.UI;
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

        private float[] _eventDeltaTimeTracker;
        private int _eventDeltaTimeIdx;

        public PerformanceMonitor(int resolution = 200) : base("Performance Monitor")
        {
            _dtTracker = new float[resolution];
            _updateTracker = new float[resolution];
#if DEBUG
            _eventDeltaTimeTracker = new float[resolution];

            AudioLayer.AudioRequestDone += AudioLayer_AudioRequestDone;
        }

        private void AudioLayer_AudioRequestDone(float time)
        {
            _eventDeltaTimeTracker[_eventDeltaTimeIdx] = time;
            _eventDeltaTimeIdx++;
            if (_eventDeltaTimeIdx >= _eventDeltaTimeTracker.Length) _eventDeltaTimeIdx = 0;
        }

        public override void DetachedFromController(UIController controller)
        {
            base.DetachedFromController(controller);
            AudioLayer.AudioRequestDone -= AudioLayer_AudioRequestDone;
        }
#else
        }
#endif

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

            ImGui.PlotLines("Actual DeltaTime", ref _dtTracker[0], _dtTracker.Length, 0, "", 0, 30);
            ImGui.PlotLines("Frames Per Update", ref _updateTracker[0], _updateTracker.Length, 0, "", 0, 5);
            ImGui.Text($"FPS {_fps}");
            ImGui.Text($"Reported DeltaTime {Engine.DeltaTime}");

#if DEBUG
            ImGui.Text(Engine.Host.Audio.ToString());
            ImGui.PlotLines("Audio Request Response (MS)", ref _eventDeltaTimeTracker[0], _eventDeltaTimeTracker.Length, 0, "", 0, 1);
#else
            ImGui.Text("Compile in debug mode for more detailed info.");
#endif

            _fpsTracker++;
        }
    }
}