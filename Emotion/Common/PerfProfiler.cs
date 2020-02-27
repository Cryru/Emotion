#region Using

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace Emotion.Common
{
    public static class PerfProfiler
    {
        public struct ProfileEvent
        {
            public string Name;
            public long StartTime;
            public long EndTime;

            public void Init(string name, long startTime)
            {
                Name = name;
                StartTime = startTime;
                EndTime = 0;
            }

            public void Done(long timestamp)
            {
                EndTime = timestamp;
            }

            /// <summary>
            /// Chrome's about:tracing format.
            /// </summary>
            /// <returns>The profiler event as a JSON string.</returns>
            public void ToString(StringBuilder builder, int pid, int frame)
            {
                builder.Append($"{{\"name\": \"{Name}\", \"cat\": \"PERF\", \"ph\": \"B\", \"pid\": {pid}, \"tid\": {frame}, \"ts\": {StartTime}}}");
                if (EndTime == 0) return;
                builder.Append($",{{\"name\": \"{Name}\", \"cat\": \"PERF\", \"ph\": \"E\", \"pid\": {pid}, \"tid\": {frame}, \"ts\": {EndTime}}}");
            }
        }

        private static bool _profileFrame;
        private static bool _profileNextFrame;

        private static long _frameStartTime;

        private static Stack<int> _profilerEvents;

        private static ProfileEvent[] _profileEventPool;
        private static int _poolIdx;

        private static StringBuilder _captureSoFar;
        private static int _capturedFrames;

        [Conditional("PROFILER")]
        public static void FrameStart()
        {
            if (!_profileNextFrame) return;

            _profileFrame = true;

            ref ProfileEvent ev = ref GetEventFromPool();
            ev.Init("FrameStart", 0);
            _frameStartTime = Stopwatch.GetTimestamp();
            _profileNextFrame = false;
        }

        [Conditional("PROFILER")]
        public static void FrameEnd()
        {
            if (!_profileFrame) return;

            _profileEventPool[0].Done(Stopwatch.GetTimestamp() - _frameStartTime);
            _profileFrame = false;
            _frameStartTime = 0;

            int pid = Process.GetCurrentProcess().Id;

            // Append to the JSON capture so far.
            if (_capturedFrames > 0) _captureSoFar.Append(",");
            for (var i = 0; i < _poolIdx; i++)
            {
                ref ProfileEvent ev = ref _profileEventPool[i];
                ev.ToString(_captureSoFar, pid, _capturedFrames);
                if (i != _poolIdx - 1) _captureSoFar.Append(",");
            }

            _capturedFrames++;
            Engine.AssetLoader.Save(Encoding.UTF8.GetBytes(_captureSoFar.ToString()), $"ProfiledFrame-{pid}.json", false);
        }

        [Conditional("PROFILER")]
        public static void ProfileNextFrame()
        {
            _profileNextFrame = true;

            if (_profilerEvents == null)
                _profilerEvents = new Stack<int>();
            else
                _profilerEvents.Clear();
            if (_captureSoFar == null)
                _captureSoFar = new StringBuilder("[", 50 * 100);

            _poolIdx = 0;
            if (_profileEventPool == null) _profileEventPool = new ProfileEvent[100];
        }

        [Conditional("PROFILER")]
        public static void Start(string name)
        {
            if (!_profileFrame) return;

            ref ProfileEvent ev = ref GetEventFromPool();
            ev.Init(name, Stopwatch.GetTimestamp() - _frameStartTime);
            _profilerEvents.Push(_poolIdx - 1);
        }

        [Conditional("PROFILER")]
        public static void Stop()
        {
            if (!_profileFrame) return;
            if (!_profilerEvents.TryPop(out int evIdx)) return;
            _profileEventPool[evIdx].Done(Stopwatch.GetTimestamp() - _frameStartTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref ProfileEvent GetEventFromPool()
        {
            ref ProfileEvent ev = ref _profileEventPool[_poolIdx];
            _poolIdx++;
            return ref ev;
        }
    }
}