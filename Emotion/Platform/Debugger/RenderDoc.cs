#region Using

using System.Runtime.InteropServices;
using WinApi.Kernel32;

#endregion

namespace Emotion.Platform.Debugger
{
    // https://renderdoc.org/
    public static unsafe class RenderDoc
    {
        /// <summary>
        /// Whether the API is loaded and ready to be used.
        /// </summary>
        public static bool Loaded
        {
            get => _api.GetAPIVersion != IntPtr.Zero;
        }

        private static RenderDocAPI _api;

        private delegate int RenderDocGetApi(int version, void* api);

        public static void TryLoad()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            // Check for RenderDoc
            IntPtr renderDocModule = Kernel32Methods.GetModuleHandle("renderdoc.dll");
            if (renderDocModule == IntPtr.Zero) return;
            // Get a handle to the RenderDoc API
            IntPtr api = Kernel32Methods.GetProcAddress(renderDocModule, "RENDERDOC_GetAPI");
            if (api == IntPtr.Zero) return;
            var getApiFunc = Marshal.GetDelegateForFunctionPointer<RenderDocGetApi>(api);
            void* apiPointers;
            int ret = getApiFunc(10102, &apiPointers);
            if (ret != 1) return;
            _api = Marshal.PtrToStructure<RenderDocAPI>((IntPtr) apiPointers);
        }

        public static void StartCapture()
        {
            if (_api.StartFrameCapture == IntPtr.Zero) return;
            Marshal.GetDelegateForFunctionPointer<RenderDocAPI.CaptureFunc>(_api.StartFrameCapture)(IntPtr.Zero, IntPtr.Zero);
        }

        public static void EndCapture()
        {
            if (_api.EndFrameCapture == IntPtr.Zero) return;
            Marshal.GetDelegateForFunctionPointer<RenderDocAPI.CaptureFunc>(_api.EndFrameCapture)(IntPtr.Zero, IntPtr.Zero);
        }
    }
}