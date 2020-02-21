#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

#endregion

namespace Emotion.Common
{
    public static class PerfProfiler
    {
        public class ProfileEvent
        {
            public string Name;
            public long StartTime;
            public long EndTime;

            public ProfileEvent(string name, long startTime)
            {
                Name = name;
                StartTime = startTime;
            }

            public void Done(long timestamp)
            {
                EndTime = timestamp;
            }

            /// <summary>
            /// Chrome's about:tracing format.
            /// </summary>
            /// <returns>The profiler event as a JSON string.</returns>
            public override string ToString()
            {
                int pid = Process.GetCurrentProcess().Id;

                string begin = $"{{\"name\": \"{Name}\", \"cat\": \"PERF\", \"ph\": \"B\", \"pid\": {pid}, \"tid\": 0, \"ts\": {StartTime}}}";
                if (EndTime == 0) return begin;
                string end = $"{{\"name\": \"{Name}\", \"cat\": \"PERF\", \"ph\": \"E\", \"pid\": {pid}, \"tid\": 0, \"ts\": {EndTime}}}";

                return begin + "," + end;
            }
        }

        private static bool _profileFrame;
        private static bool _profileNextFrame;

        private static long _frameStartTime;

        private static Stack<ProfileEvent> _profilerEvents;
        private static Queue<ProfileEvent> _doneEvents;

        [Conditional("PROFILER")]
        public static void FrameStart()
        {
            if (!_profileNextFrame) return;

            _profileFrame = true;
            if (_profilerEvents == null)
                _profilerEvents = new Stack<ProfileEvent>();
            else
                _profilerEvents.Clear();
            if (_doneEvents == null)
                _doneEvents = new Queue<ProfileEvent>();
            else
                _doneEvents.Clear();

            _doneEvents.Enqueue(new ProfileEvent("FrameStart", 0));
            _frameStartTime = Stopwatch.GetTimestamp();
            _profileNextFrame = false;
        }

        [Conditional("PROFILER")]
        public static void FrameEnd()
        {
            if (!_profileFrame) return;

            _profileFrame = false;
            _doneEvents.Peek().Done(Stopwatch.GetTimestamp() - _frameStartTime);
            _frameStartTime = 0;

            // Get events which didn't finish.
            foreach (ProfileEvent notFinishedEvents in _profilerEvents)
            {
                _doneEvents.Enqueue(notFinishedEvents);
            }

            _profilerEvents.Clear();

            // Transform to the JSON format.
            var output = "[";
            while (_doneEvents.Count > 0)
            {
                ProfileEvent ev = _doneEvents.Dequeue();
                output += ev.ToString();
                if (_doneEvents.Count > 0) output += ",";
            }

            output += "]";

            Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(output), $"profiledFrame-{Stopwatch.GetTimestamp()}.json");
        }

        [Conditional("PROFILER")]
        public static void ProfileNextFrame()
        {
            _profileNextFrame = true;
        }

        [Conditional("PROFILER")]
        public static void Start(string name)
        {
            if (!_profileFrame) return;
            _profilerEvents.Push(new ProfileEvent(name, Stopwatch.GetTimestamp() - _frameStartTime));
        }

        [Conditional("PROFILER")]
        public static void Stop()
        {
            if (!_profileFrame) return;
            if (!_profilerEvents.TryPop(out ProfileEvent ev)) return;
            ev.Done(Stopwatch.GetTimestamp() - _frameStartTime);
            _doneEvents.Enqueue(ev);
        }
    }
}