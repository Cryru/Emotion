﻿#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Platform.Implementation.Win32.Wgl
{
#pragma warning disable 649 // Uninitialized field
#pragma warning disable 0169 // Unused field
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Unused field
    public struct RenderDocAPI
    {
        private IntPtr _getAPIVersion;

        private IntPtr _setCaptureOptionU32;
        private IntPtr _setCaptureOptionF32;

        private IntPtr _getCaptureOptionU32;
        private IntPtr _getCaptureOptionF32;

        private IntPtr _setFocusToggleKeys;
        private IntPtr _setCaptureKeys;

        private IntPtr _getOverlayBits;
        private IntPtr _maskOverlayBits;

        private IntPtr _shutdown;
        private IntPtr _unloadCrashHandler;

        private IntPtr _setCaptureFilePathTemplate;
        private IntPtr _getCaptureFilePathTemplate;

        private IntPtr _getNumCaptures;
        private IntPtr _getCapture;

        private IntPtr _triggerCapture;

        private IntPtr _isTargetControlConnected;
        private IntPtr _launchReplayUI;

        private IntPtr _setActiveWindow;

        private IntPtr _startFrameCapture;
        private IntPtr _isFrameCapturing;

        private IntPtr _endFrameCapture;

        private IntPtr _triggerMultiFrameCapture;

        public void StartCapture()
        {
            if (_startFrameCapture == IntPtr.Zero) return;
            Marshal.GetDelegateForFunctionPointer<CaptureFunc>(_startFrameCapture)(IntPtr.Zero, IntPtr.Zero);
        }

        public void EndCapture()
        {
            if (_endFrameCapture == IntPtr.Zero) return;
            Marshal.GetDelegateForFunctionPointer<CaptureFunc>(_endFrameCapture)(IntPtr.Zero, IntPtr.Zero);
        }

        public delegate void CaptureFunc(IntPtr device, IntPtr window);
    }
#pragma warning restore 649
#pragma warning restore 0169
#pragma warning restore IDE0044
#pragma warning restore IDE0051
}