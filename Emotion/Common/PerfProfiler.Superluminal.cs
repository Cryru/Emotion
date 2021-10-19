#if Superluminal
#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Utility;

#endregion

#pragma warning disable 649 // Uninitialized field
#pragma warning disable 0169 // Unused field
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Unused field

namespace Emotion.Common
{
    /// <summary>
    /// https://superluminal.eu/
    /// </summary>
    public static class PerfProfiler
    {
        private static SuperluminalAPI _superluminal;
        private static IntPtr _superluminalHandle;
        private static bool _superluminalLoaded;

        private struct SuperluminalAPI
        {
            private IntPtr _setCurrentThreadName;
            private IntPtr _setCurrentThreadNameN;

            private IntPtr _beginEvent;
            private IntPtr _beginEventN;
            private IntPtr _beginEventWide;
            private IntPtr _beginEventWideN;

            private IntPtr _endEvent;

            private delegate void BeginEventFunc(string name, ushort len, string data, ushort lenData, uint c);

            private struct TailCallOptimization
            {
                private uint _a;
                private uint _b;
                private uint _c;
            }

            private delegate TailCallOptimization EndEventFunc();

            public void BeginEvent(string name, string data, uint c = 0)
            {
                if (_beginEventN == IntPtr.Zero) return;
                Marshal.GetDelegateForFunctionPointer<BeginEventFunc>(_beginEventN)(name, (ushort) name.Length, data, (ushort) data.Length, c);
            }

            public void EndEvent()
            {
                if (_endEvent == IntPtr.Zero) return;
                Marshal.GetDelegateForFunctionPointer<EndEventFunc>(_endEvent)();
            }
        }

        [DllImport("AssetsNativeLibs/Superluminal/PerformanceAPI.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "PerformanceAPI_GetAPI")]
        private static extern IntPtr SuperluminalGetAPI(int version, ref SuperluminalAPI api);

        private static int _frameId;

        private static void EnsureAPILoaded()
        {
            if (_superluminalLoaded) return;
            const int majorVer = 2;
            const int minorVer = 0;
            _superluminalHandle = SuperluminalGetAPI((majorVer << 16) | minorVer, ref _superluminal);
            _superluminalLoaded = true;
        }

        public static void FrameStart()
        {
            EnsureAPILoaded();
            _superluminal.BeginEvent("Frame", _frameId.ToString());
            _frameId++;
        }

        public static void FrameEnd()
        {
            _superluminal.EndEvent();
        }

        public static void ProfilerEventStart(string eventName, string eventGroup)
        {
            EnsureAPILoaded();
            _superluminal.BeginEvent(eventGroup, eventName);
        }

        public static void ProfilerEventEnd(string eventName, string eventGroup)
        {
            _superluminal.EndEvent();
        }

        public static void FrameEventStart(string name, string desc = "")
        {
            _superluminal.BeginEvent(name, desc);
        }

        public static void FrameEventEnd(string name)
        {
            _superluminal.EndEvent();
        }

        public static bool LagSpikeMonitor = false;

        public static void LagSpikeProfileFrame()
        {
            // nop
        }

        public static void ProfileNextFrame()
        {
            // nop
        }
    }
}
#endif