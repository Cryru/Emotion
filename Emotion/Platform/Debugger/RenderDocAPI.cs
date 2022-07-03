#region Using

using System;

#endregion

namespace Emotion.Platform.Debugger
{
#pragma warning disable 649 // Uninitialized field
#pragma warning disable 0169 // Unused field
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Unused field

    /// <summary>
    /// RenderDoc Integration API
    /// Holds references to the loaded API functions.
    /// </summary>
    public struct RenderDocAPI
    {
        public IntPtr GetAPIVersion;

        public IntPtr SetCaptureOptionU32;
        public IntPtr SetCaptureOptionF32;

        public IntPtr GetCaptureOptionU32;
        public IntPtr GetCaptureOptionF32;

        public IntPtr SetFocusToggleKeys;
        public IntPtr SetCaptureKeys;

        public IntPtr GetOverlayBits;
        public IntPtr MaskOverlayBits;

        public IntPtr Shutdown;
        public IntPtr UnloadCrashHandler;

        public IntPtr SetCaptureFilePathTemplate;
        public IntPtr GetCaptureFilePathTemplate;

        public IntPtr GetNumCaptures;
        public IntPtr GetCapture;

        public IntPtr TriggerCapture;

        public IntPtr IsTargetControlConnected;
        public IntPtr LaunchReplayUI;

        public IntPtr SetActiveWindow;

        public IntPtr StartFrameCapture;
        public IntPtr IsFrameCapturing;

        public IntPtr EndFrameCapture;

        public IntPtr TriggerMultiFrameCapture;

        public delegate void CaptureFunc(IntPtr device, IntPtr window);
    }
#pragma warning restore 649
#pragma warning restore 0169
#pragma warning restore IDE0044
#pragma warning restore IDE0051
}