#region Using

using System.Runtime.InteropServices;
using User32 = Emotion.Core.Platform.Implementation.Win32.Native.User32.User32;
using Gdi32 = Emotion.Core.Platform.Implementation.Win32.Native.Gdi32.Gdi32;
using DwmApi = Emotion.Core.Platform.Implementation.Win32.Native.DwmApi.DwmApi;
using Kernel32 = Emotion.Core.Platform.Implementation.Win32.Native.Kernel32.Kernel32;
using Emotion.Core.Platform.Implementation.Win32;
using Emotion.Core.Platform;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Platform.Implementation.Win32.Native.Core;
using Emotion.Core.Platform.Implementation.Win32.Native.Gdi32;





#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Wgl
{
    public sealed unsafe class GalliumGraphicsContext : GraphicsContext
    {
        private nint _openGlLibrary;

        private WglFunctions.WglDeleteContext _deleteContext;
        private WglFunctions.WglGetProcAddress _getProcAddress;
        private WglFunctions.WglMakeCurrent _makeCurrent;
        private WglFunctions.SwapInternalExt _swapIntervalExt;

        private nint _contextHandle;
        private nint _dc;
        private Win32Platform _platform;

        [DllImport("msvcrt")]
        public static extern int _putenv_s(string e, string v);

        public void Init(nint windowHandle, Win32Platform platform)
        {
            _platform = platform;

            // This is how you request a context version from MESA - lol
            // This needs to be pInvoked as Environment.SetEnvironmentVariable doesn't affect native getenv calls.
            _putenv_s("MESA_GL_VERSION_OVERRIDE", "3.3FC");

            // Unload old OpenGL, if any.
            nint loadedOpenGl = Kernel32.GetModuleHandle("opengl32.dll");
            var counter = 0;
            if (loadedOpenGl != nint.Zero) Engine.Log.Info("OpenGL32.dll is already loaded. Attempting to unload...", MessageSource.WGallium);
            while (loadedOpenGl != nint.Zero)
            {
                Kernel32.FreeLibrary(loadedOpenGl);
                loadedOpenGl = Kernel32.GetModuleHandle("opengl32.dll");
                counter++;
                if (counter < 100) continue;
                Engine.Log.Error("Couldn't unload the loaded OpenGL32.dll. Gallium context couldn't be created.", MessageSource.WGallium);
                return;
            }

            if (counter > 0) Engine.Log.Info($"OpenGL32.dll was unloaded after {counter} attempts.", MessageSource.WGallium);

            // Load library.
            _openGlLibrary = _platform.LoadLibrary("mesa");
            if (_openGlLibrary == nint.Zero)
            {
                Engine.Log.Error("Couldn't load mesa.", MessageSource.WGallium);
                return;
            }

            var createContext = _platform.GetFunctionByName<WglFunctions.WglCreateContext>(_openGlLibrary, "wglCreateContext");
            _deleteContext = _platform.GetFunctionByName<WglFunctions.WglDeleteContext>(_openGlLibrary, "wglDeleteContext");
            _getProcAddress = _platform.GetFunctionByName<WglFunctions.WglGetProcAddress>(_openGlLibrary, "wglGetProcAddress");
            _makeCurrent = _platform.GetFunctionByName<WglFunctions.WglMakeCurrent>(_openGlLibrary, "wglMakeCurrent");
            _swapIntervalExt = NativeHelpers.GetFunctionByPtr<WglFunctions.SwapInternalExt>(_getProcAddress("wglSwapIntervalEXT"));

            // Setup context.
            _dc = User32.GetDC(windowHandle);
            if (_dc == nint.Zero) Win32Platform.CheckError("WGL: Could not get window dc.", true);

            int pixelFormatIdx = SupportedPixelFormat(_dc);
            if (pixelFormatIdx == 0) return;

            var pfd = new PixelFormatDescriptor();
            pfd.NSize = (ushort) Marshal.SizeOf(pfd);
            if (Gdi32.DescribePixelFormat(_dc, pixelFormatIdx, (uint) sizeof(PixelFormatDescriptor), ref pfd) == 0)
            {
                Win32Platform.CheckError("WGL: Failed to retrieve PFD for the selected pixel format.", true);
                return;
            }

            if (!Gdi32.SetPixelFormat(_dc, pixelFormatIdx, ref pfd)) Win32Platform.CheckError("WGL: Could not set pixel format.", true);

            const string contextName = "Gallium-OpenGL";
            _contextHandle = createContext(_dc);
            if (_contextHandle == nint.Zero) Win32Platform.CheckError(contextName, true);

            Win32Platform.CheckError("Checking if context creation passed.");
            Engine.Log.Trace($"Requested {contextName} using pixel format id {pixelFormatIdx}", MessageSource.Win32);

            Valid = true;
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <param name="dc">The window's handle.</param>
        /// <returns>The index of the pixel format to use.</returns>
        private static int SupportedPixelFormat(nint dc)
        {
            var usableConfigs = new List<FramebufferConfig>();

            // Get the number of formats.
            int nativeCount = Gdi32.DescribePixelFormat(dc, 1, (uint) sizeof(PixelFormatDescriptor), nint.Zero);

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

            FramebufferConfig closestConfig = ChoosePixelFormat(usableConfigs);
            if (closestConfig != null) return closestConfig.Handle;

            Engine.Log.Error("Couldn't find suitable pixel format.", MessageSource.WGallium);
            return 0;
        }

        #region Graphic Context API

        protected override void SetSwapIntervalPlatform(int interval)
        {
            _swapIntervalExt(interval);
        }

        public override void SwapBuffers()
        {
            if (_platform.DisplayMode != DisplayMode.Fullscreen)
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
                    for (var i = 0; i < SwapInterval; i++)
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

        public override nint GetProcAddress(string func)
        {
            nint proc = _getProcAddress(func);
            return proc == nint.Zero ? _platform.GetLibrarySymbolPtr(_openGlLibrary, func) : proc;
        }

        public override void Dispose()
        {
            if (_contextHandle == nint.Zero) return;
            _deleteContext(_contextHandle);
        }

        #endregion
    }
}