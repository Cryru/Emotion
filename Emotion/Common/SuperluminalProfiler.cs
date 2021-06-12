#region Using

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emotion.Utility;

#endregion

namespace Emotion.Common
{
    /// <summary>
    /// https://superluminal.eu/
    /// </summary>
    public static class SuperluminalProfiler
    {
        private static SuperluminalAPI _superluminal;
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

            private delegate void BeginEventFunc(IntPtr name, ushort len, IntPtr data, ushort lenData, uint c);

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
                IntPtr ptrName = NativeHelpers.StringToPtr(name);
                IntPtr ptrData = NativeHelpers.StringToPtr(data);
                Marshal.GetDelegateForFunctionPointer<BeginEventFunc>(_beginEventN)(ptrName, (ushort) name.Length, ptrData, (ushort) data.Length, c);
            }

            public void EndEvent()
            {
                if (_endEvent == IntPtr.Zero) return;
                Marshal.GetDelegateForFunctionPointer<EndEventFunc>(_endEvent)();
            }
        }

        [DllImport("PerformanceAPI.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "PerformanceAPI_GetAPI")]
        private static extern void SuperluminalGetAPI(int version, ref SuperluminalAPI api);

        private static int _frameId;

        [Conditional("Superluminal")]
        public static void FrameStart()
        {
            if (!_superluminalLoaded)
            {
                const int majorVer = 2;
                const int minorVer = 0;
                SuperluminalGetAPI((majorVer << 16) | minorVer, ref _superluminal);
                _superluminalLoaded = true;
            }

            _superluminal.BeginEvent("FrameStart", _frameId.ToString());
            _frameId++;
        }

        [Conditional("Superluminal")]
        public static void FrameEnd()
        {
            _superluminal.EndEvent();
        }
    }
}