#region Using

using System;
using System.Runtime.InteropServices;
using Emotion.Common;
using WinApi.Kernel32;

#endregion

namespace Emotion.Platform.RenderDoc
{
    public abstract unsafe class RenderDocGraphicsContext : GraphicsContext
    {
        // https://renderdoc.org/

        private delegate int RenderDocGetApi(int version, void* api);

        protected RenderDocGraphicsContext()
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
            RenderDoc = Marshal.PtrToStructure<RenderDocAPI>((IntPtr)apiPointers);
        }

        public static void RenderDocCaptureStart()
        {
            ((RenderDocGraphicsContext)Engine.Host.Context).RenderDoc.StartCapture();
        }

        public static void RenderDocCaptureEnd()
        {
            ((RenderDocGraphicsContext)Engine.Host.Context).RenderDoc.EndCapture();
        }
    }
}