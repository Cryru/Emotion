#if !Superluminal

#region Using

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

#endregion

namespace Emotion.Common
{
    /// <summary>
    /// Load files into chrome://tracing
    /// </summary>
    public static class PerfProfiler
    {
        private static bool _profileFrame;
        private static bool _profileNextFrame;

        private static StringBuilder _captureSoFar;
        private static int _capturedFrames;

        private static BlockingCollection<string> _ongoingEventCaptures;

        private static Stopwatch _timer = Stopwatch.StartNew();
        private static Stopwatch _frameTimer;

        public static bool LagSpikeMonitor = false;

        public static void LagSpikeProfileFrame()
        {
            if (!LagSpikeMonitor) return;
            _profileFrame = true;
        }

        private static long GetElapsedMicroseconds(this Stopwatch timer)
        {
            return timer.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000);
        }

        private static string GetEventJSON(string eventName, string group, bool start = true, long? timestamp = null)
        {
            timestamp ??= _timer.GetElapsedMicroseconds();
            return
                $"{{\"name\": \"{eventName}\", \"cat\": \"PERF\", \"ph\": \"{(start ? "B" : "E")}\", \"tid\": {Thread.CurrentThread.ManagedThreadId}, \"pid\": \"{group}\", \"ts\": {timestamp}}}";
        }

        [Conditional("PROFILER")]
        public static void ProfilerEventStart(string eventName, string eventGroup)
        {
            if (_ongoingEventCaptures == null) _ongoingEventCaptures = new BlockingCollection<string>();
            _ongoingEventCaptures.Add(GetEventJSON(eventName, eventGroup));
        }

        [Conditional("PROFILER")]
        public static void ProfilerEventEnd(string eventName, string eventGroup)
        {
            _ongoingEventCaptures.Add(GetEventJSON(eventName, eventGroup, false));
        }

        [Conditional("PROFILER")]
        public static void FrameStart()
        {
            if (_frameTimer == null) _frameTimer = Stopwatch.StartNew();
            else
                _frameTimer.Restart();

            _captureSoFar ??= new StringBuilder("[", 50 * 100);

            if (!_profileNextFrame && !LagSpikeMonitor) return;

            _captureSoFar.Append(GetEventJSON("Frame", $"Frame Capture {_capturedFrames}", true, 0));
            _captureSoFar.Append(",");
            _profileFrame = _profileNextFrame;
            _profileNextFrame = false;

            if (LagSpikeMonitor)
            {
                _captureSoFar.Clear();
                _captureSoFar.Append("[");
            }
        }

        [Conditional("PROFILER")]
        public static void FrameEnd()
        {
            if (!_profileFrame) return;

            _captureSoFar.Append(GetEventJSON("Frame", $"Frame Capture {_capturedFrames}", false, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
            _profileFrame = false;
            _frameTimer.Stop();
            _capturedFrames++;

            var name = $"Player/Profiler/ProfilerResults{DateTime.Now.ToBinary()}.json";
            string traceEvents = _captureSoFar + string.Join(",", _ongoingEventCaptures) + "]";
            string json = "{" +
                          $"\"traceEvents\":{traceEvents}," +
                          "\"displayTimeUnit\":\"ms\"" +
                          "}";

            Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(json), name, false);
            Engine.Log.Info($"Saved profiler data to {name}.", "Profiler");
        }

        [Conditional("PROFILER")]
        public static void ProfileNextFrame()
        {
            _profileNextFrame = true;
        }

        [Conditional("PROFILER")]
        public static void FrameEventStart(string name, string desc = "")
        {
            if (!_profileFrame && !LagSpikeMonitor) return;
            _captureSoFar.Append(GetEventJSON(name, $"Frame Capture {_capturedFrames}", true, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
        }

        [Conditional("PROFILER")]
        public static void FrameEventEnd(string name)
        {
            if (!_profileFrame && !LagSpikeMonitor) return;
            _captureSoFar.Append(GetEventJSON(name, $"Frame Capture {_capturedFrames}", false, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
        }
    }
}
#endif