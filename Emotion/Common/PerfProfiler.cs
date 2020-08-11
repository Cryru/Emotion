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

        private static BlockingCollection<string> _onGoingEventCaptures;

        private static Stopwatch _timer = Stopwatch.StartNew();
        private static Stopwatch _frameTimer;

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
            if (_onGoingEventCaptures == null) _onGoingEventCaptures = new BlockingCollection<string>();
            _onGoingEventCaptures.Add(GetEventJSON(eventName, eventGroup));
        }

        [Conditional("PROFILER")]
        public static void ProfilerEventEnd(string eventName, string eventGroup)
        {
            _onGoingEventCaptures.Add(GetEventJSON(eventName, eventGroup, false));
        }

        [Conditional("PROFILER")]
        public static void FrameStart()
        {
            if (!_profileNextFrame) return;

            if (_frameTimer == null) _frameTimer = Stopwatch.StartNew();
            else
                _frameTimer.Restart();

            _captureSoFar.Append(GetEventJSON("FrameStart", $"Frame Capture {_capturedFrames}", true, 0));
            _captureSoFar.Append(",");
            _profileFrame = true;
            _profileNextFrame = false;
        }

        [Conditional("PROFILER")]
        public static void FrameEnd()
        {
            if (!_profileFrame) return;

            _captureSoFar.Append(GetEventJSON("FrameStart", $"Frame Capture {_capturedFrames}", false, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
            _profileFrame = false;
            _frameTimer.Stop();
            _capturedFrames++;

            string name = $"ProfilerResults{DateTime.Now.ToBinary()}.json";

            string traceEvents = _captureSoFar + string.Join(",", _onGoingEventCaptures) + "]";
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

            if (_captureSoFar == null)
                _captureSoFar = new StringBuilder("[", 50 * 100);
        }

        [Conditional("PROFILER")]
        public static void FrameEventStart(string name)
        {
            if (!_profileFrame) return;
            _captureSoFar.Append(GetEventJSON(name, $"Frame Capture {_capturedFrames}", true, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
        }

        [Conditional("PROFILER")]
        public static void FrameEventEnd(string name)
        {
            if (!_profileFrame) return;
            _captureSoFar.Append(GetEventJSON(name, $"Frame Capture {_capturedFrames}", false, _frameTimer.GetElapsedMicroseconds()));
            _captureSoFar.Append(",");
        }
    }
}