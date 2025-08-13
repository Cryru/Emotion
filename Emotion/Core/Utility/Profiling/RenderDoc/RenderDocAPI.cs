#nullable enable

namespace Emotion.Core.Utility.Profiling.RenderDoc;

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
    public nint GetAPIVersion;

    public nint SetCaptureOptionU32;
    public nint SetCaptureOptionF32;

    public nint GetCaptureOptionU32;
    public nint GetCaptureOptionF32;

    public nint SetFocusToggleKeys;
    public nint SetCaptureKeys;

    public nint GetOverlayBits;
    public nint MaskOverlayBits;

    public nint Shutdown;
    public nint UnloadCrashHandler;

    public nint SetCaptureFilePathTemplate;
    public nint GetCaptureFilePathTemplate;

    public nint GetNumCaptures;
    public nint GetCapture;

    public nint TriggerCapture;

    public nint IsTargetControlConnected;
    public nint LaunchReplayUI;

    public nint SetActiveWindow;

    public nint StartFrameCapture;
    public nint IsFrameCapturing;

    public nint EndFrameCapture;

    public nint TriggerMultiFrameCapture;

    public delegate void CaptureFunc(nint device, nint window);
}
#pragma warning restore 649
#pragma warning restore 0169
#pragma warning restore IDE0044
#pragma warning restore IDE0051
