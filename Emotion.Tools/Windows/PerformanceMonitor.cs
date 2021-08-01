#region Using

using System;
using System.Diagnostics;
using Emotion.Audio;
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

        private float[] _eventUpdateTracker;
        private int _eventUpdateIdx;
        private Stopwatch _eventUpdateTimer = new Stopwatch();

        private float[] _eventDrawTracker;
        private int _eventDrawIdx;
        private Stopwatch _eventDrawTimer = new Stopwatch();

        private float[] _eventDeltaTimeTracker;
        private int _eventDeltaTimeIdx;
        private Stopwatch _eventDeltaTimeTimer = new Stopwatch();

        public PerformanceMonitor(int resolution = 200) : base("Performance Monitor")
        {
            _dtTracker = new float[resolution];
            _updateTracker = new float[resolution];

#if DEBUG

            _eventUpdateTracker = new float[resolution];
            _eventDrawTracker = new float[resolution];
            _eventDeltaTimeTracker = new float[resolution];

            Engine.DebugOnUpdateStart += (s, e) => { _eventUpdateTimer.Restart(); };
            Engine.DebugOnUpdateEnd += (s, e) =>
            {
                _eventUpdateTimer.Stop();
                _eventUpdateTracker[_eventUpdateIdx] = _eventUpdateTimer.ElapsedMilliseconds;
                _eventUpdateIdx++;
                if (_eventUpdateIdx >= resolution) _eventUpdateIdx = 0;
            };

            Engine.DebugOnFrameStart += (s, e) =>
            {
                _eventDeltaTimeTimer.Stop();
                _eventDeltaTimeTracker[_eventDeltaTimeIdx] = _eventDeltaTimeTimer.ElapsedMilliseconds;
                _eventDeltaTimeIdx++;
                if (_eventDeltaTimeIdx >= resolution) _eventDeltaTimeIdx = 0;
                _eventDeltaTimeTimer.Restart();

                _eventDrawTimer.Restart();
            };
            Engine.DebugOnFrameEnd += (s, e) =>
            {
                _eventDrawTimer.Stop();
                _eventDrawTracker[_eventDrawIdx] = _eventDrawTimer.ElapsedMilliseconds;
                _eventDrawIdx++;
                if (_eventDrawIdx >= resolution) _eventDrawIdx = 0;

                if (PerfProfiler.LagSpikeMonitor && _eventDrawTimer.ElapsedMilliseconds > 5)
                {
                    Engine.Log.Warning($"Lag spike detected! Draw took {_eventDrawTimer.ElapsedMilliseconds}ms", "Profiler");
                    PerfProfiler.LagSpikeProfileFrame();
                }
            };
#endif
        }

        protected override void RenderContent(RenderComposer composer)
        {
            _updateTracker[_updateIdx] = _updatesPerFrame;
            _updateIdx++;
            if (_updateIdx > _updateTracker.Length - 1) _updateIdx = 0;
            _updatesPerFrame = 0;

            ImGui.Text(Engine.Host.ToString());

            ImGui.PlotLines("Actual DeltaTime", ref _dtTracker[0], _dtTracker.Length, 0, "", 0, 30);
            ImGui.PlotLines("Update Count", ref _updateTracker[0], _updateTracker.Length, 0, "", 0, 5);
            ImGui.Text($"FPS {_fps}");
            ImGui.Text($"Reported DeltaTime {Engine.DeltaTime}");

#if DEBUG
            ImGui.PlotLines("Precise Update (Ms)", ref _eventUpdateTracker[0], _eventUpdateTracker.Length, 0, "", 0, 3000);
            ImGui.PlotLines("Precise Render (Ms)", ref _eventDrawTracker[0], _eventDrawTracker.Length, 0, "", 0, 30);
            ImGui.PlotLines("Precise DeltaTime (Ms)", ref _eventDeltaTimeTracker[0], _eventDeltaTimeTracker.Length, 0, "", 0, 30);

            ImGui.Text("This option requires Emotion to have been compiled with the `PROFILER` flag.");
            ImGui.Checkbox("Profiler LagSpike Monitor", ref PerfProfiler.LagSpikeMonitor);

            ImGui.Text(" ");
            ImGui.Text($"Audio Performance - {Engine.Host.Audio}");

            ImGui.Text($"AudioLayer Samples Stored {AudioLayer.SamplesStored}");
            ImGui.Text($"   Least Stored {AudioLayer.LeastSamplesStored}");
            ImGui.SameLine();
            if (ImGui.Button("Clear Least")) AudioLayer.LeastSamplesStored = AudioLayer.SamplesStored;
            ImGui.Text($"Resorted to Layer {AudioLayer.ResortedToLayer}");
            ImGui.SameLine();
            if (ImGui.Button("Clear")) AudioLayer.ResortedToLayer = false;
            ImGui.Text($"Time Taken To Fill {AudioLayer.TimeTaken}");
            ImGui.SameLine();
            if (ImGui.Button("Reset")) AudioLayer.TimeTaken = 0;
#else
            ImGui.Text("Compile in debug mode for more detailed info.");
#endif

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