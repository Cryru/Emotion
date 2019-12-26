#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Platform.Config;
using Emotion.Platform.Helpers;
using Emotion.Standard.Logging;
using WinApi.Core;
using WinApi.Gdi32;
using User32 = WinApi.User32.User32Methods;
using Gdi32 = WinApi.Gdi32.Gdi32Methods;
using DwmApi = WinApi.DwmApi.DwmApiMethods;
using Kernel32 = WinApi.Kernel32.Kernel32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32.Wgl
{
    public sealed unsafe class GalliumGraphicsContext : GraphicsContext
    {
        private IntPtr _openGlLibrary;

        private WglFunctions.WglCreateContext _createContext;
        private WglFunctions.WglDeleteContext _deleteContext;
        private WglFunctions.WglGetProcAddress _getProcAddress;
        private WglFunctions.WglMakeCurrent _makeCurrent;
        private WglFunctions.SwapInternalExt _swapIntervalExt;

        private IntPtr _contextHandle;
        private IntPtr _dc;
        private Win32Platform _platform;

        public GalliumGraphicsContext(IntPtr windowHandle, Win32Platform platform)
        {
            PrepareLlvmPipe();

            _platform = platform;
            PlatformConfig config = _platform.Config;

            _dc = User32.GetDC(windowHandle);
            if (_dc == IntPtr.Zero) Win32Platform.CheckError("WGL: Could not get window dc.", true);

            int pixelFormatIdx = SupportedPixelFormat(_dc, config);
            if (pixelFormatIdx == 0) return;

            var pfd = new PixelFormatDescriptor();
            pfd.NSize = (ushort) Marshal.SizeOf(pfd);
            if (Gdi32.DescribePixelFormat(_dc, pixelFormatIdx, (uint) sizeof(PixelFormatDescriptor), ref pfd) == 0)
            {
                Win32Platform.CheckError("WGL: Failed to retrieve PFD for the selected pixel format.", true);
                return;
            }

            if (!Gdi32.SetPixelFormat(_dc, pixelFormatIdx, ref pfd)) Win32Platform.CheckError("WGL: Could not set pixel format.", true);

            var contextName = "Gallium-OpenGL";
            _contextHandle = _createContext(_dc);
            if (_contextHandle == IntPtr.Zero) Win32Platform.CheckError(contextName, true);

            Win32Platform.CheckError("Checking if context creation passed.");
            Engine.Log.Trace($"Requested {contextName} using pixel format id {pixelFormatIdx}", MessageSource.Win32);

            Valid = true;
        }

        #region Init

        private void PrepareLlvmPipe()
        {
            // Unload old OpenGL, if any.
            IntPtr loadedOpenGl = Kernel32.GetModuleHandle("opengl32.dll");
            var counter = 0;
            if (loadedOpenGl != IntPtr.Zero) Engine.Log.Info("OpenGL32.dll is already loaded. Attempting to unload...", MessageSource.Win32);
            while (loadedOpenGl != IntPtr.Zero)
            {
                Kernel32.FreeLibrary(loadedOpenGl);
                loadedOpenGl = Kernel32.GetModuleHandle("opengl32.dll");
                counter++;
                if (counter >= 100)
                    throw new Exception("Couldn't unload the loaded OpenGL32.dll. Gallium context couldn't be created.");
            }

            if (counter > 0) Engine.Log.Info($"OpenGL32.dll was unloaded after {counter} attempts.", MessageSource.Win32);

            // Load library.
            _openGlLibrary = NativeHelpers.LoadLibrary("mesa\\opengl32.dll");
            if (_openGlLibrary == IntPtr.Zero) throw new Exception("mesa\\opengl32.dll not found.");

            _createContext = NativeHelpers.GetFunctionByName<WglFunctions.WglCreateContext>(_openGlLibrary, "wglCreateContext");
            _deleteContext = NativeHelpers.GetFunctionByName<WglFunctions.WglDeleteContext>(_openGlLibrary, "wglDeleteContext");
            _getProcAddress = NativeHelpers.GetFunctionByName<WglFunctions.WglGetProcAddress>(_openGlLibrary, "wglGetProcAddress");
            _makeCurrent = NativeHelpers.GetFunctionByName<WglFunctions.WglMakeCurrent>(_openGlLibrary, "wglMakeCurrent");
            _makeCurrent = NativeHelpers.GetFunctionByName<WglFunctions.WglMakeCurrent>(_openGlLibrary, "wglMakeCurrent");
            _swapIntervalExt = NativeHelpers.GetFunctionByPtr<WglFunctions.SwapInternalExt>(_getProcAddress("wglSwapIntervalEXT"));
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <param name="dc">The window's handle.</param>
        /// <param name="config">The config which contains the requested pixel format.</param>
        /// <returns>The index of the pixel format to use.</returns>
        private int SupportedPixelFormat(IntPtr dc, PlatformConfig config)
        {
            var usableConfigs = new List<FramebufferConfig>();

            // Get the number of formats.
            int nativeCount = Gdi32.DescribePixelFormat(dc, 1, (uint) sizeof(PixelFormatDescriptor), IntPtr.Zero);

            for (var i = 0; i < nativeCount; i++)
            {
                var fbConfig = new FramebufferConfig();

                var pfd = new PixelFormatDescriptor();
                pfd.NSize = (ushort) Marshal.SizeOf(pfd);
                if (Gdi32.DescribePixelFormat(dc, i + 1, (uint) sizeof(PixelFormatDescriptor), ref pfd) == 0)
                {
                    Win32Platform.CheckError("Gallium: Could not describe pixel format.", true);
                    continue;
                }

                // We need both of these to be supported.
                if ((pfd.DwFlags & PixelFormatFlags.DrawToWindow) == 0 ||
                    (pfd.DwFlags & PixelFormatFlags.SupportOpenGl) == 0)
                    continue;

                // Should be GenericAccelerated, and mustn't be generic.
                if ((pfd.DwFlags & PixelFormatFlags.GenericAccelerated) == 0 &&
                    (pfd.DwFlags & PixelFormatFlags.Generic) != 0)
                    continue;

                if (pfd.PixelType != (byte) PixelFormatFlags.RGBA)
                    continue;

                fbConfig.RedBits = pfd.CRedBits;
                fbConfig.GreenBits = pfd.CGreenBits;
                fbConfig.BlueBits = pfd.CBlueBits;
                fbConfig.AlphaBits = pfd.CAlphaBits;

                fbConfig.DepthBits = pfd.CDepthBits;
                fbConfig.StencilBits = pfd.CStencilBits;

                fbConfig.AccumRedBits = pfd.CAccumRedBits;
                fbConfig.AccumGreenBits = pfd.CAccumGreenBits;
                fbConfig.AccumBlueBits = pfd.CAccumBlueBits;
                fbConfig.AccumAlphaBits = pfd.CAccumAlphaBits;

                if ((pfd.DwFlags & PixelFormatFlags.Stereo) != 0) fbConfig.Stereo = true;

                if ((pfd.DwFlags & PixelFormatFlags.DoubleBuffer) != 0) fbConfig.Doublebuffer = true;

                fbConfig.Handle = i + 1;
                usableConfigs.Add(fbConfig);
            }

            if (usableConfigs.Count == 0)
            {
                Engine.Log.Error("The driver doesn't seem to support OpenGL.", MessageSource.WGallium);
                return 0;
            }

            FramebufferConfig closestConfig = ChoosePixelFormat(config, usableConfigs);
            if (closestConfig != null) return closestConfig.Handle;

            Engine.Log.Error("Couldn't find suitable pixel format.", MessageSource.WGallium);
            return 0;
        }

        #endregion

        #region Graphic Context API

        protected override void SetSwapIntervalPlatform(int interval)
        {
            _swapIntervalExt(interval);
        }

        public override void SwapBuffers()
        {
            if (_platform.Window.DisplayMode != DisplayMode.Fullscreen)
            {
                bool dwmComposition;

                // DWM Composition is always enabled on Win8+
                if (Win32Platform.IsWindows8OrGreater)
                {
                    dwmComposition = true;
                }
                else
                {
                    HResult result = DwmApi.DwmIsCompositionEnabled(out dwmComposition);
                    if (result != HResult.S_OK) dwmComposition = false;
                }

                if (dwmComposition)
                    for (var i = 0; i < SwapInternal; i++)
                    {
                        DwmApi.DwmFlush();
                    }
            }

            Gdi32.SwapBuffers(_dc);
        }

        public override void MakeCurrent()
        {
            if (!_makeCurrent(_dc, _contextHandle)) Win32Platform.CheckError("Gallium: Couldn't make context current.", true);
        }

        public override IntPtr GetProcAddress(string func)
        {
            IntPtr proc = _getProcAddress(func);
            return proc == IntPtr.Zero ? NativeLibrary.GetExport(_openGlLibrary, func) : proc;
        }

        public override void Dispose()
        {
            if (_contextHandle == IntPtr.Zero) return;
            _deleteContext(_contextHandle);
        }

        #endregion
    }
}