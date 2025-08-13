#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.Kernel32;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Core.Utility.Profiling.RenderDoc;

// https://renderdoc.org/
public static unsafe class RenderDoc
{
    /// <summary>
    /// Whether the API is loaded and ready to be used.
    /// </summary>
    public static bool Loaded
    {
        get => _api.GetAPIVersion != nint.Zero;
    }

    private static RenderDocAPI _api;

    private delegate int RenderDocGetApi(int version, void* api);

    public static void TryLoad()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

        // Check for RenderDoc
        nint renderDocModule = Kernel32.GetModuleHandle("renderdoc.dll");
        if (renderDocModule == nint.Zero) return;
        // Get a handle to the RenderDoc API
        nint api = Kernel32.GetProcAddress(renderDocModule, "RENDERDOC_GetAPI");
        if (api == nint.Zero) return;
        var getApiFunc = Marshal.GetDelegateForFunctionPointer<RenderDocGetApi>(api);
        void* apiPointers;
        int ret = getApiFunc(10102, &apiPointers);
        if (ret != 1) return;
        _api = Marshal.PtrToStructure<RenderDocAPI>((nint) apiPointers);
    }

    public static void StartCapture()
    {
        if (_api.StartFrameCapture == nint.Zero) return;
        Marshal.GetDelegateForFunctionPointer<RenderDocAPI.CaptureFunc>(_api.StartFrameCapture)(nint.Zero, nint.Zero);
    }

    public static void EndCapture()
    {
        if (_api.EndFrameCapture == nint.Zero) return;
        Marshal.GetDelegateForFunctionPointer<RenderDocAPI.CaptureFunc>(_api.EndFrameCapture)(nint.Zero, nint.Zero);
    }
}